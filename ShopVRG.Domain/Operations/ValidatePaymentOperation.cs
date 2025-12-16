namespace ShopVRG.Domain.Operations;

using System.Text.RegularExpressions;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Operation to validate payment details
/// </summary>
internal sealed partial class ValidatePaymentOperation : PaymentOperation
{
    private readonly Func<OrderId, bool> _checkOrderExists;
    private readonly Func<OrderId, Price?> _getOrderTotal;

    public ValidatePaymentOperation(
        Func<OrderId, bool> checkOrderExists,
        Func<OrderId, Price?> getOrderTotal)
    {
        _checkOrderExists = checkOrderExists;
        _getOrderTotal = getOrderTotal;
    }

    protected override IPayment OnUnvalidated(UnvalidatedPayment payment)
    {
        var errors = new List<string>();

        // 1. Validate OrderId
        if (!OrderId.TryCreate(payment.OrderId, out var orderId, out var orderIdError))
        {
            errors.Add(orderIdError ?? "Invalid order ID");
            return new InvalidPayment(payment.OrderId, errors);
        }

        // 2. Check if order exists
        if (!_checkOrderExists(orderId!))
        {
            errors.Add($"Order '{orderId}' does not exist");
            return new InvalidPayment(payment.OrderId, errors);
        }

        // 3. Get order total
        var orderTotal = _getOrderTotal(orderId!);
        if (orderTotal == null)
        {
            errors.Add("Could not retrieve order total");
            return new InvalidPayment(payment.OrderId, errors);
        }

        // 4. Validate Amount
        if (!Price.TryCreate(payment.Amount, out var amount, out var amountError))
        {
            errors.Add(amountError ?? "Invalid payment amount");
        }
        else if (amount!.Value != orderTotal.Value)
        {
            errors.Add($"Payment amount ({amount}) does not match order total ({orderTotal})");
        }

        // 5. Validate Card Number (basic Luhn check simulation)
        var cardNumber = payment.CardNumber?.Trim().Replace(" ", "").Replace("-", "") ?? "";
        if (!CardNumberRegex().IsMatch(cardNumber))
        {
            errors.Add("Invalid card number format. Expected 13-19 digits");
        }

        // 6. Validate Card Holder Name
        if (string.IsNullOrWhiteSpace(payment.CardHolderName) || payment.CardHolderName.Length < 2)
        {
            errors.Add("Card holder name is required");
        }

        // 7. Validate Expiry Date (MM/YY format)
        if (!ExpiryDateRegex().IsMatch(payment.ExpiryDate?.Trim() ?? ""))
        {
            errors.Add("Invalid expiry date format. Expected MM/YY");
        }
        else
        {
            var parts = payment.ExpiryDate!.Split('/');
            var month = int.Parse(parts[0]);
            var year = int.Parse(parts[1]) + 2000;
            var expiry = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            if (expiry < DateTime.UtcNow)
            {
                errors.Add("Card has expired");
            }
        }

        // 8. Validate CVV
        if (!CvvRegex().IsMatch(payment.Cvv?.Trim() ?? ""))
        {
            errors.Add("Invalid CVV format. Expected 3-4 digits");
        }

        if (errors.Count > 0)
        {
            return new InvalidPayment(payment.OrderId, errors);
        }

        // Mask card number for storage
        var maskedCard = $"****-****-****-{cardNumber[^4..]}";

        return new ValidatedPayment(
            PaymentId.NewPaymentId(),
            orderId!,
            amount!,
            maskedCard,
            payment.CardHolderName.Trim(),
            DateTime.UtcNow);
    }

    [GeneratedRegex(@"^\d{13,19}$")]
    private static partial Regex CardNumberRegex();

    [GeneratedRegex(@"^(0[1-9]|1[0-2])\/\d{2}$")]
    private static partial Regex ExpiryDateRegex();

    [GeneratedRegex(@"^\d{3,4}$")]
    private static partial Regex CvvRegex();
}
