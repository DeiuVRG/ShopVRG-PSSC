namespace ShopVRG.Domain.Operations;

using ShopVRG.Domain.Models.Entities;

/// <summary>
/// Base class for Order operations using Transform pattern
/// Each operation uses pattern matching to handle different states
/// </summary>
internal abstract class OrderOperation
{
    internal IOrder Transform(IOrder order)
    {
        return order switch
        {
            UnvalidatedOrder unvalidated => OnUnvalidated(unvalidated),
            ValidatedOrder validated => OnValidated(validated),
            StockCheckedOrder stockChecked => OnStockChecked(stockChecked),
            PendingOrder pending => OnPending(pending),
            PlacedOrder placed => OnPlaced(placed),
            InvalidOrder invalid => OnInvalid(invalid),
            _ => throw new InvalidOperationException($"Unknown order state: {order.GetType().Name}")
        };
    }

    protected virtual IOrder OnUnvalidated(UnvalidatedOrder order) => order;
    protected virtual IOrder OnValidated(ValidatedOrder order) => order;
    protected virtual IOrder OnStockChecked(StockCheckedOrder order) => order;
    protected virtual IOrder OnPending(PendingOrder order) => order;
    protected virtual IOrder OnPlaced(PlacedOrder order) => order;
    protected virtual IOrder OnInvalid(InvalidOrder order) => order;
}
