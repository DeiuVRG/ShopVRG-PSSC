namespace ShopVRG.Domain.Operations;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Operation to ship the order
/// </summary>
internal sealed class ShipOrderOperation : ShippingOperation
{
    private readonly Func<OrderId, string, string, bool> _persistShipment;
    private readonly Func<string, int> _getEstimatedDeliveryDays;

    public ShipOrderOperation(
        Func<OrderId, string, string, bool> persistShipment,
        Func<string, int> getEstimatedDeliveryDays)
    {
        _persistShipment = persistShipment;
        _getEstimatedDeliveryDays = getEstimatedDeliveryDays;
    }

    protected override IShipping OnValidated(ValidatedShipping shipping)
    {
        // 1. Persist shipment
        if (!_persistShipment(shipping.OrderId, shipping.TrackingNumber, shipping.Carrier))
        {
            return new InvalidShipping(
                shipping.OrderId.ToString(),
                ["Failed to persist shipment record"]);
        }

        // 2. Calculate estimated delivery
        var deliveryDays = _getEstimatedDeliveryDays(shipping.Carrier);
        var estimatedDelivery = DateTime.UtcNow.AddDays(deliveryDays);

        return new ShippedOrder(
            shipping.OrderId,
            shipping.TrackingNumber,
            shipping.Carrier,
            shipping.Destination,
            DateTime.UtcNow,
            estimatedDelivery);
    }
}
