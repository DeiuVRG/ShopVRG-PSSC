namespace ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Value object for OrderId representing a unique order identifier
/// Uses GUID for uniqueness
/// </summary>
public sealed class OrderId : IEquatable<OrderId>
{
    public Guid Value { get; }

    private OrderId(Guid value) => Value = value;

    public static OrderId NewOrderId() => new(Guid.NewGuid());

    public static bool TryCreate(string? input, out OrderId? orderId, out string? error)
    {
        orderId = null;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Order ID must not be empty";
            return false;
        }

        if (!Guid.TryParse(input.Trim(), out var guid))
        {
            error = "Invalid Order ID format";
            return false;
        }

        orderId = new OrderId(guid);
        return true;
    }

    public static bool TryCreate(Guid input, out OrderId? orderId, out string? error)
    {
        orderId = null;
        error = null;

        if (input == Guid.Empty)
        {
            error = "Order ID cannot be empty";
            return false;
        }

        orderId = new OrderId(input);
        return true;
    }

    public static OrderId FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Order ID cannot be empty", nameof(value));
        return new OrderId(value);
    }

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj) => obj is OrderId other && Value == other.Value;

    public bool Equals(OrderId? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(OrderId? left, OrderId? right) =>
        left?.Value == right?.Value;

    public static bool operator !=(OrderId? left, OrderId? right) =>
        !(left == right);
}
