namespace ShopVRG.Domain.Operations;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Operation to validate order from unvalidated to validated state
/// Validates all inputs and converts them to value objects
/// </summary>
internal sealed class ValidateOrderOperation : OrderOperation
{
    protected override IOrder OnUnvalidated(UnvalidatedOrder order)
    {
        var errors = new List<string>();

        // 1. Validate CustomerName
        if (!CustomerName.TryCreate(order.CustomerName, out var customerName, out var nameError))
        {
            errors.Add(nameError ?? "Invalid customer name");
        }

        // 2. Validate CustomerEmail
        if (!CustomerEmail.TryCreate(order.CustomerEmail, out var customerEmail, out var emailError))
        {
            errors.Add(emailError ?? "Invalid customer email");
        }

        // 3. Validate ShippingAddress
        if (!ShippingAddress.TryCreate(
            order.ShippingStreet,
            order.ShippingCity,
            order.ShippingPostalCode,
            order.ShippingCountry,
            out var shippingAddress,
            out var addressError))
        {
            errors.Add(addressError ?? "Invalid shipping address");
        }

        // 4. Validate order has at least one line
        if (order.OrderLines.Count == 0)
        {
            errors.Add("Order must have at least one item");
        }

        // 5. Validate each order line
        var validatedLines = new List<ValidatedOrderLine>();
        for (int i = 0; i < order.OrderLines.Count; i++)
        {
            var line = order.OrderLines[i];
            var linePrefix = $"Order line {i + 1}";

            if (!ProductCode.TryCreate(line.ProductCode, out var productCode, out var codeError))
            {
                errors.Add($"{linePrefix}: {codeError ?? "Invalid product code"}");
                continue;
            }

            if (!Quantity.TryCreate(line.Quantity, out var quantity, out var qtyError))
            {
                errors.Add($"{linePrefix}: {qtyError ?? "Invalid quantity"}");
                continue;
            }

            validatedLines.Add(new ValidatedOrderLine(productCode!, quantity!));
        }

        // 6. Check for duplicate products
        var duplicates = validatedLines
            .GroupBy(l => l.ProductCode.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            errors.Add($"Duplicate products in order: {string.Join(", ", duplicates)}");
        }

        // 7. Return validated or invalid state
        if (errors.Count > 0)
        {
            return new InvalidOrder(errors);
        }

        return new ValidatedOrder(
            OrderId.NewOrderId(),
            customerName!,
            customerEmail!,
            shippingAddress!,
            validatedLines,
            DateTime.UtcNow);
    }
}
