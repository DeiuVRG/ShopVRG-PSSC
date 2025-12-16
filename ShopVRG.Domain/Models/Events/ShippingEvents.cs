namespace ShopVRG.Domain.Models.Events;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Base interface for Shipping events
/// </summary>
public interface IShippingEvent { }

/// <summary>
/// Success event - Order has been shipped
/// </summary>
public sealed record OrderShippedEvent : IShippingEvent
{
    public OrderId OrderId { get; }
    public string TrackingNumber { get; }
    public string Carrier { get; }
    public ShippingAddress Destination { get; }
    public DateTime ShippedAt { get; }
    public DateTime EstimatedDelivery { get; }

    internal OrderShippedEvent(
        OrderId orderId,
        string trackingNumber,
        string carrier,
        ShippingAddress destination,
        DateTime shippedAt,
        DateTime estimatedDelivery)
    {
        OrderId = orderId;
        TrackingNumber = trackingNumber;
        Carrier = carrier;
        Destination = destination;
        ShippedAt = shippedAt;
        EstimatedDelivery = estimatedDelivery;
    }
}

/// <summary>
/// Failure event - Shipping failed
/// </summary>
public sealed record ShippingFailedEvent : IShippingEvent
{
    public string? OrderId { get; }
    public IReadOnlyList<string> Reasons { get; }

    internal ShippingFailedEvent(string? orderId, IEnumerable<string> reasons)
    {
        OrderId = orderId;
        Reasons = reasons.ToList().AsReadOnly();
    }
}

/// <summary>
/// Extension method to convert IShipping to event
/// </summary>
public static class ShippingEventExtensions
{
    public static IShippingEvent ToEvent(this IShipping shipping)
    {
        return shipping switch
        {
            ShippedOrder shipped => new OrderShippedEvent(
                shipped.OrderId,
                shipped.TrackingNumber,
                shipped.Carrier,
                shipped.Destination,
                shipped.ShippedAt,
                shipped.EstimatedDelivery),

            InvalidShipping invalid => new ShippingFailedEvent(
                invalid.OrderId,
                invalid.Reasons),

            UnvalidatedShipping unvalidated => new ShippingFailedEvent(
                unvalidated.OrderId,
                ["Shipping was not completed - remained in unvalidated state"]),

            ValidatedShipping validated => new ShippingFailedEvent(
                validated.OrderId.ToString(),
                ["Shipping was not completed - remained in validated state"]),

            _ => new ShippingFailedEvent(
                null,
                [$"Unknown shipping state: {shipping.GetType().Name}"])
        };
    }
}
