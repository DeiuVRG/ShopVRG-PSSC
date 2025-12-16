namespace ShopVRG.Domain.Models.Entities;

using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Base interface for Shipping entity
/// States: Unvalidated -> Validated -> Shipped / Invalid
/// </summary>
public interface IShipping { }

/// <summary>
/// Initial state - raw shipping request
/// </summary>
public sealed record UnvalidatedShipping : IShipping
{
    public string OrderId { get; }
    public string TrackingNumber { get; }
    public string Carrier { get; }

    public UnvalidatedShipping(
        string orderId,
        string trackingNumber,
        string carrier)
    {
        OrderId = orderId;
        TrackingNumber = trackingNumber;
        Carrier = carrier;
    }
}

/// <summary>
/// Validated shipping state
/// </summary>
public sealed record ValidatedShipping : IShipping
{
    public OrderId OrderId { get; }
    public string TrackingNumber { get; }
    public string Carrier { get; }
    public ShippingAddress Destination { get; }
    public DateTime ValidatedAt { get; }

    internal ValidatedShipping(
        OrderId orderId,
        string trackingNumber,
        string carrier,
        ShippingAddress destination,
        DateTime validatedAt)
    {
        OrderId = orderId;
        TrackingNumber = trackingNumber;
        Carrier = carrier;
        Destination = destination;
        ValidatedAt = validatedAt;
    }
}

/// <summary>
/// Final successful state - order shipped
/// </summary>
public sealed record ShippedOrder : IShipping
{
    public OrderId OrderId { get; }
    public string TrackingNumber { get; }
    public string Carrier { get; }
    public ShippingAddress Destination { get; }
    public DateTime ShippedAt { get; }
    public DateTime EstimatedDelivery { get; }

    internal ShippedOrder(
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
/// Invalid shipping state
/// </summary>
public sealed record InvalidShipping : IShipping
{
    public string? OrderId { get; }
    public IReadOnlyList<string> Reasons { get; }

    internal InvalidShipping(string? orderId, IEnumerable<string> reasons)
    {
        OrderId = orderId;
        Reasons = reasons.ToList().AsReadOnly();
    }
}
