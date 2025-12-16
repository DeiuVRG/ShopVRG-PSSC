namespace ShopVRG.Domain.Models.ValueObjects;

using System.Text.RegularExpressions;

/// <summary>
/// Value object for ProductCode representing the unique identifier of a PC component
/// Validation rules:
/// - Format: 2-4 uppercase letters followed by 3-6 digits
/// - Examples: "CPU001", "GPU12345", "RAM123", "MB001234"
/// </summary>
public sealed partial class ProductCode : IEquatable<ProductCode>
{
    public string Value { get; }

    private ProductCode(string value) => Value = value;

    public static bool TryCreate(string? input, out ProductCode? productCode, out string? error)
    {
        productCode = null;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Product code must not be empty";
            return false;
        }

        var s = input.Trim().ToUpperInvariant();

        if (!ProductCodeRegex().IsMatch(s))
        {
            error = "Invalid product code format. Expected 2-4 uppercase letters followed by 3-6 digits (e.g. CPU001, GPU12345)";
            return false;
        }

        productCode = new ProductCode(s);
        return true;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is ProductCode other && Value == other.Value;

    public bool Equals(ProductCode? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ProductCode? left, ProductCode? right) =>
        left?.Value == right?.Value;

    public static bool operator !=(ProductCode? left, ProductCode? right) =>
        !(left == right);

    [GeneratedRegex(@"^[A-Z]{2,4}\d{3,6}$")]
    private static partial Regex ProductCodeRegex();
}
