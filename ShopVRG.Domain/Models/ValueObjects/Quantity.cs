namespace ShopVRG.Domain.Models.ValueObjects;

using System.Globalization;

/// <summary>
/// Value object for Quantity representing the number of items
/// Validation rules:
/// - Must be a positive integer
/// - Must be between 1 and 1000
/// </summary>
public sealed class Quantity : IEquatable<Quantity>, IComparable<Quantity>
{
    private const int MinValue = 1;
    private const int MaxValue = 1000;

    public int Value { get; }

    private Quantity(int value) => Value = value;

    public static bool TryCreate(string? input, out Quantity? quantity, out string? error)
    {
        quantity = null;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Quantity must not be empty";
            return false;
        }

        if (!int.TryParse(input.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
        {
            error = "Quantity must be a valid integer";
            return false;
        }

        return TryCreate(v, out quantity, out error);
    }

    public static bool TryCreate(int input, out Quantity? quantity, out string? error)
    {
        quantity = null;
        error = null;

        if (input < MinValue)
        {
            error = $"Quantity must be at least {MinValue}";
            return false;
        }

        if (input > MaxValue)
        {
            error = $"Quantity must not exceed {MaxValue}";
            return false;
        }

        quantity = new Quantity(input);
        return true;
    }

    public static Quantity FromInt(int value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentException($"Quantity must be between {MinValue} and {MaxValue}", nameof(value));
        return new Quantity(value);
    }

    public Quantity Add(Quantity other)
    {
        var newValue = Value + other.Value;
        if (newValue > MaxValue)
            throw new InvalidOperationException($"Quantity cannot exceed {MaxValue}");
        return new Quantity(newValue);
    }

    public Quantity Subtract(Quantity other)
    {
        var newValue = Value - other.Value;
        if (newValue < MinValue)
            throw new InvalidOperationException($"Quantity cannot be less than {MinValue}");
        return new Quantity(newValue);
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    public override bool Equals(object? obj) => obj is Quantity other && Value == other.Value;

    public bool Equals(Quantity? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(Quantity? other) => other is null ? 1 : Value.CompareTo(other.Value);

    public static bool operator ==(Quantity? left, Quantity? right) =>
        left?.Value == right?.Value;

    public static bool operator !=(Quantity? left, Quantity? right) =>
        !(left == right);

    public static bool operator <(Quantity left, Quantity right) =>
        left.Value < right.Value;

    public static bool operator >(Quantity left, Quantity right) =>
        left.Value > right.Value;

    public static bool operator <=(Quantity left, Quantity right) =>
        left.Value <= right.Value;

    public static bool operator >=(Quantity left, Quantity right) =>
        left.Value >= right.Value;
}
