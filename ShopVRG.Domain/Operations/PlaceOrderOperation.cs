namespace ShopVRG.Domain.Operations;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Operation to place the order - final step in the workflow
/// Converts StockCheckedOrder to PlacedOrder
/// </summary>
internal sealed class PlaceOrderOperation : OrderOperation
{
    private readonly Func<OrderId, StockCheckedOrder, bool> _persistOrder;

    public PlaceOrderOperation(Func<OrderId, StockCheckedOrder, bool> persistOrder)
    {
        _persistOrder = persistOrder;
    }

    protected override IOrder OnStockChecked(StockCheckedOrder order)
    {
        // 1. Persist the order
        if (!_persistOrder(order.OrderId, order))
        {
            return new InvalidOrder(["Failed to persist order to database"]);
        }

        // 2. Convert to pending order (awaiting payment confirmation)
        var pendingLines = order.OrderLines
            .Select(l => new PendingOrderLine(
                l.ProductCode,
                l.ProductName,
                l.Quantity,
                l.UnitPrice,
                l.LineTotal))
            .ToList();

        return new PendingOrder(
            order.OrderId,
            order.CustomerName,
            order.CustomerEmail,
            order.ShippingAddress,
            pendingLines,
            order.TotalPrice,
            order.CreatedAt);
    }
}
