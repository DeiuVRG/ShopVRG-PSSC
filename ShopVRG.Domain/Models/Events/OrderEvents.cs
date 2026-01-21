namespace ShopVRG.Domain.Models.Events;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Base interface for Order events
/// </summary>
public interface IOrderEvent { }

/// <summary>
/// Success event - Order has been placed successfully
/// </summary>
public sealed record OrderPlacedEvent : IOrderEvent
{
    public OrderId OrderId { get; }
    public CustomerName CustomerName { get; }
    public CustomerEmail CustomerEmail { get; }
    public ShippingAddress ShippingAddress { get; }
    public IReadOnlyList<OrderLineDto> OrderLines { get; }
    public Price TotalPrice { get; }
    public DateTime PlacedAt { get; }

    internal OrderPlacedEvent(
        OrderId orderId,
        CustomerName customerName,
        CustomerEmail customerEmail,
        ShippingAddress shippingAddress,
        IEnumerable<OrderLineDto> orderLines,
        Price totalPrice,
        DateTime placedAt)
    {
        OrderId = orderId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
        OrderLines = orderLines.ToList().AsReadOnly();
        TotalPrice = totalPrice;
        PlacedAt = placedAt;
    }
}

/// <summary>
/// Pending event - Order created but awaiting payment confirmation
/// This event is published when the order is first created and stock is reserved
/// The OrderPlacedEvent will be published after payment is confirmed
/// </summary>
public sealed record OrderPendingPaymentEvent : IOrderEvent
{
    public OrderId OrderId { get; }
    public CustomerName CustomerName { get; }
    public CustomerEmail CustomerEmail { get; }
    public ShippingAddress ShippingAddress { get; }
    public IReadOnlyList<OrderLineDto> OrderLines { get; }
    public Price TotalPrice { get; }
    public DateTime CreatedAt { get; }

    internal OrderPendingPaymentEvent(
        OrderId orderId,
        CustomerName customerName,
        CustomerEmail customerEmail,
        ShippingAddress shippingAddress,
        IEnumerable<OrderLineDto> orderLines,
        Price totalPrice,
        DateTime createdAt)
    {
        OrderId = orderId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
        OrderLines = orderLines.ToList().AsReadOnly();
        TotalPrice = totalPrice;
        CreatedAt = createdAt;
    }
}

/// <summary>
/// Order line DTO for events
/// </summary>
public sealed record OrderLineDto(
    string ProductCode,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

/// <summary>
/// Failure event - Order placement failed
/// </summary>
public sealed record OrderPlacementFailedEvent : IOrderEvent
{
    public IReadOnlyList<string> Reasons { get; }

    internal OrderPlacementFailedEvent(IEnumerable<string> reasons)
    {
        Reasons = reasons.ToList().AsReadOnly();
    }
}

/// <summary>
/// Extension method to convert IOrder to event
/// </summary>
public static class OrderEventExtensions
{
    public static IOrderEvent ToEvent(this IOrder order)
    {
        return order switch
        {
            PlacedOrder placed => new OrderPlacedEvent(
                placed.OrderId,
                placed.CustomerName,
                placed.CustomerEmail,
                placed.ShippingAddress,
                placed.OrderLines.Select(l => new OrderLineDto(
                    l.ProductCode.Value,
                    l.ProductName.Value,
                    l.Quantity.Value,
                    l.UnitPrice.Value,
                    l.LineTotal.Value)),
                placed.TotalPrice,
                placed.PlacedAt),

            PendingOrder pending => new OrderPendingPaymentEvent(
                pending.OrderId,
                pending.CustomerName,
                pending.CustomerEmail,
                pending.ShippingAddress,
                pending.OrderLines.Select(l => new OrderLineDto(
                    l.ProductCode.Value,
                    l.ProductName.Value,
                    l.Quantity.Value,
                    l.UnitPrice.Value,
                    l.LineTotal.Value)),
                pending.TotalPrice,
                pending.CreatedAt),

            InvalidOrder invalid => new OrderPlacementFailedEvent(invalid.Reasons),

            UnvalidatedOrder => new OrderPlacementFailedEvent(
                ["Order was not completed - remained in unvalidated state"]),

            ValidatedOrder => new OrderPlacementFailedEvent(
                ["Order was not completed - remained in validated state"]),

            StockCheckedOrder => new OrderPlacementFailedEvent(
                ["Order was not completed - remained in stock checked state"]),

            _ => new OrderPlacementFailedEvent(
                [$"Unknown order state: {order.GetType().Name}"])
        };
    }
}
