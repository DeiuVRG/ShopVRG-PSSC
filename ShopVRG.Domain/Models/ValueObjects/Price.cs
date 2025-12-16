namespace ShopVRG.Domain.Models.ValueObjects;

using System.Globalization;

/// <summary>
/// Value object for Price representing the monetary value of a product
/// Validation rules:
/// - Must be a positive decimal value
/// - Must be greater than 0
/// - Maximum value is 1,000,000
/// - Precision up to 2 decimal places
/// </summary>
public sealed class Price : IEquatable<Price>, IComparable<Price>
{
    private const decimal MinValue = 0.01m;
    private const decimal MaxValue = 1_000_000m;

    public decimal Value { get; }

    private Price(decimal value) => Value = value;

    public static bool TryCreate(string? input, out Price? price, out string? error)
    {
        price = null;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Price must not be empty";
            return false;
        }

        if (!decimal.TryParse(input.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
        {
            error = "Price must be a valid number";
            return false;
        }

        return TryCreate(v, out price, out error);
    }

    public static bool TryCreate(decimal input, out Price? price, out string? error)
    {
        price = null;
        error = null;

        if (input < MinValue)
        {
            error = $"Price must be at least {MinValue:F2}";
            return false;
        }

        if (input > MaxValue)
        {
            error = $"Price must not exceed {MaxValue:N0}";
            return false;
        }

        price = new Price(Math.Round(input, 2));
        return true;
    }

    public static Price FromDecimal(decimal value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentException($"Price must be between {MinValue} and {MaxValue}", nameof(value));
        return new Price(Math.Round(value, 2));
    }

    public Price Add(Price other) => new(Value + other.Value);

    public Price Multiply(int quantity) => new(Value * quantity);

    public override string ToString() => Value.ToString("F2", CultureInfo.InvariantCulture);

    public override bool Equals(object? obj) => obj is Price other && Value == other.Value;

    public bool Equals(Price? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(Price? other) => other is null ? 1 : Value.CompareTo(other.Value);

    public static bool operator ==(Price? left, Price? right) =>
        left?.Value == right?.Value;

    public static bool operator !=(Price? left, Price? right) =>
        !(left == right);

    public static bool operator <(Price left, Price right) =>
        left.Value < right.Value;

    public static bool operator >(Price left, Price right) =>
        left.Value > right.Value;

    public static bool operator <=(Price left, Price right) =>
        left.Value <= right.Value;

    public static bool operator >=(Price left, Price right) =>
        left.Value >= right.Value;

    public static Price operator +(Price left, Price right) =>
        new(left.Value + right.Value);

    public static Price operator *(Price left, int right) =>
        new(left.Value * right);
}
