namespace ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Value object for ProductName representing the name of a PC component
/// Validation rules:
/// - Must be between 3 and 200 characters
/// - Must not be empty or whitespace
/// </summary>
public sealed class ProductName : IEquatable<ProductName>
{
    private const int MinLength = 3;
    private const int MaxLength = 200;

    public string Value { get; }

    private ProductName(string value) => Value = value;

    public static bool TryCreate(string? input, out ProductName? productName, out string? error)
    {
        productName = null;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Product name must not be empty";
            return false;
        }

        var s = input.Trim();

        if (s.Length < MinLength)
        {
            error = $"Product name must be at least {MinLength} characters";
            return false;
        }

        if (s.Length > MaxLength)
        {
            error = $"Product name must not exceed {MaxLength} characters";
            return false;
        }

        productName = new ProductName(s);
        return true;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is ProductName other && Value == other.Value;

    public bool Equals(ProductName? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ProductName? left, ProductName? right) =>
        left?.Value == right?.Value;

    public static bool operator !=(ProductName? left, ProductName? right) =>
        !(left == right);
}
