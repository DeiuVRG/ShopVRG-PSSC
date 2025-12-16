namespace ShopVRG.Domain.Models.ValueObjects;

using System.Globalization;

/// <summary>
/// Value object for StockQuantity representing the available stock of a product
/// Validation rules:
/// - Must be a non-negative integer
/// - Must be between 0 and 100,000
/// </summary>
public sealed class StockQuantity : IEquatable<StockQuantity>, IComparable<StockQuantity>
{
    private const int MinValue = 0;
    private const int MaxValue = 100_000;

    public int Value { get; }

    private StockQuantity(int value) => Value = value;

    public static bool TryCreate(string? input, out StockQuantity? stockQuantity, out string? error)
    {
        stockQuantity = null;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Stock quantity must not be empty";
            return false;
        }

        if (!int.TryParse(input.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
        {
            error = "Stock quantity must be a valid integer";
            return false;
        }

        return TryCreate(v, out stockQuantity, out error);
    }

    public static bool TryCreate(int input, out StockQuantity? stockQuantity, out string? error)
    {
        stockQuantity = null;
        error = null;

        if (input < MinValue)
        {
            error = $"Stock quantity cannot be negative";
            return false;
        }

        if (input > MaxValue)
        {
            error = $"Stock quantity must not exceed {MaxValue}";
            return false;
        }

        stockQuantity = new StockQuantity(input);
        return true;
    }

    public static StockQuantity FromInt(int value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentException($"Stock quantity must be between {MinValue} and {MaxValue}", nameof(value));
        return new StockQuantity(value);
    }

    public static StockQuantity Zero => new(0);

    public bool IsAvailable => Value > 0;

    public bool HasEnoughStock(Quantity quantity) => Value >= quantity.Value;

    public StockQuantity Decrease(Quantity quantity)
    {
        var newValue = Value - quantity.Value;
        if (newValue < MinValue)
            throw new InvalidOperationException("Not enough stock available");
        return new StockQuantity(newValue);
    }

    public StockQuantity Increase(Quantity quantity)
    {
        var newValue = Value + quantity.Value;
        if (newValue > MaxValue)
            throw new InvalidOperationException($"Stock quantity cannot exceed {MaxValue}");
        return new StockQuantity(newValue);
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    public override bool Equals(object? obj) => obj is StockQuantity other && Value == other.Value;

    public bool Equals(StockQuantity? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(StockQuantity? other) => other is null ? 1 : Value.CompareTo(other.Value);

    public static bool operator ==(StockQuantity? left, StockQuantity? right) =>
        left?.Value == right?.Value;

    public static bool operator !=(StockQuantity? left, StockQuantity? right) =>
        !(left == right);

    public static bool operator <(StockQuantity left, StockQuantity right) =>
        left.Value < right.Value;

    public static bool operator >(StockQuantity left, StockQuantity right) =>
        left.Value > right.Value;
}
