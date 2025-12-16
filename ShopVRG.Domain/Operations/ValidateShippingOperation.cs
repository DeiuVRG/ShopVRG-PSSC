namespace ShopVRG.Domain.Operations;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Operation to validate shipping details
/// </summary>
internal sealed class ValidateShippingOperation : ShippingOperation
{
    private static readonly HashSet<string> ValidCarriers =
    [
        "DHL",
        "FEDEX",
        "UPS",
        "DPD",
        "GLS",
        "CARGUS",
        "FAN_COURIER",
        "SAMEDAY"
    ];

    private readonly Func<OrderId, bool> _checkOrderExists;
    private readonly Func<OrderId, bool> _checkOrderPaid;
    private readonly Func<OrderId, ShippingAddress?> _getOrderShippingAddress;

    public ValidateShippingOperation(
        Func<OrderId, bool> checkOrderExists,
        Func<OrderId, bool> checkOrderPaid,
        Func<OrderId, ShippingAddress?> getOrderShippingAddress)
    {
        _checkOrderExists = checkOrderExists;
        _checkOrderPaid = checkOrderPaid;
        _getOrderShippingAddress = getOrderShippingAddress;
    }

    protected override IShipping OnUnvalidated(UnvalidatedShipping shipping)
    {
        var errors = new List<string>();

        // 1. Validate OrderId
        if (!OrderId.TryCreate(shipping.OrderId, out var orderId, out var orderIdError))
        {
            errors.Add(orderIdError ?? "Invalid order ID");
            return new InvalidShipping(shipping.OrderId, errors);
        }

        // 2. Check if order exists
        if (!_checkOrderExists(orderId!))
        {
            errors.Add($"Order '{orderId}' does not exist");
            return new InvalidShipping(shipping.OrderId, errors);
        }

        // 3. Check if order is paid
        if (!_checkOrderPaid(orderId!))
        {
            errors.Add($"Order '{orderId}' has not been paid yet");
            return new InvalidShipping(shipping.OrderId, errors);
        }

        // 4. Get shipping address
        var shippingAddress = _getOrderShippingAddress(orderId!);
        if (shippingAddress == null)
        {
            errors.Add("Could not retrieve shipping address for order");
            return new InvalidShipping(shipping.OrderId, errors);
        }

        // 5. Validate Carrier
        var carrier = shipping.Carrier?.Trim().ToUpperInvariant().Replace(" ", "_") ?? "";
        if (!ValidCarriers.Contains(carrier))
        {
            errors.Add($"Invalid carrier '{shipping.Carrier}'. Valid carriers: {string.Join(", ", ValidCarriers)}");
        }

        if (errors.Count > 0)
        {
            return new InvalidShipping(shipping.OrderId, errors);
        }

        // Generate tracking number
        var trackingNumber = GenerateTrackingNumber(carrier);

        return new ValidatedShipping(
            orderId!,
            trackingNumber,
            carrier,
            shippingAddress,
            DateTime.UtcNow);
    }

    private static string GenerateTrackingNumber(string carrier)
    {
        var prefix = carrier switch
        {
            "DHL" => "DHL",
            "FEDEX" => "FDX",
            "UPS" => "1Z",
            "DPD" => "DPD",
            "GLS" => "GLS",
            "CARGUS" => "CRG",
            "FAN_COURIER" => "FAN",
            "SAMEDAY" => "SMD",
            _ => "TRK"
        };
        return $"{prefix}{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
    }
}
