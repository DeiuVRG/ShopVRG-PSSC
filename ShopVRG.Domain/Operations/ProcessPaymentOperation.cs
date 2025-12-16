namespace ShopVRG.Domain.Operations;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Operation to process the payment with payment gateway
/// </summary>
internal sealed class ProcessPaymentOperation : PaymentOperation
{
    private readonly Func<ValidatedPayment, string?> _processPayment;
    private readonly Func<PaymentId, OrderId, Price, string, bool> _persistPayment;

    public ProcessPaymentOperation(
        Func<ValidatedPayment, string?> processPayment,
        Func<PaymentId, OrderId, Price, string, bool> persistPayment)
    {
        _processPayment = processPayment;
        _persistPayment = persistPayment;
    }

    protected override IPayment OnValidated(ValidatedPayment payment)
    {
        // 1. Process payment with gateway
        var transactionRef = _processPayment(payment);
        if (transactionRef == null)
        {
            return new InvalidPayment(
                payment.OrderId.ToString(),
                ["Payment was declined by the payment gateway"]);
        }

        // 2. Persist payment
        if (!_persistPayment(payment.PaymentId, payment.OrderId, payment.Amount, transactionRef))
        {
            return new InvalidPayment(
                payment.OrderId.ToString(),
                ["Failed to persist payment record"]);
        }

        return new ProcessedPayment(
            payment.PaymentId,
            payment.OrderId,
            payment.Amount,
            payment.MaskedCardNumber,
            payment.CardHolderName,
            transactionRef,
            DateTime.UtcNow);
    }
}
