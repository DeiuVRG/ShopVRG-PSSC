namespace ShopVRG.Domain.Workflows;

using ShopVRG.Domain.Models.Commands;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.Events;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Operations;

/// <summary>
/// Workflow for shipping an order
/// Composes operations: Validate → Ship
/// </summary>
public sealed class ShipOrderWorkflow
{
    public IShippingEvent Execute(
        ShipOrderCommand command,
        Func<OrderId, bool> checkOrderExists,
        Func<OrderId, bool> checkOrderPaid,
        Func<OrderId, ShippingAddress?> getOrderShippingAddress,
        Func<OrderId, string, string, bool> persistShipment,
        Func<string, int> getEstimatedDeliveryDays)
    {
        // 1. Create unvalidated state from command
        IShipping shipping = new UnvalidatedShipping(
            command.OrderId,
            string.Empty, // Tracking number will be generated
            command.Carrier);

        // 2. Pipeline of operations using Transform
        shipping = new ValidateShippingOperation(checkOrderExists, checkOrderPaid, getOrderShippingAddress)
            .Transform(shipping);

        shipping = new ShipOrderOperation(persistShipment, getEstimatedDeliveryDays)
            .Transform(shipping);

        // 3. Convert final state to event
        return shipping.ToEvent();
    }
}
