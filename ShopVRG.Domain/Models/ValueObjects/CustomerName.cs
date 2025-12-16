namespace ShopVRG.Domain.Models.ValueObjects;

using System.Text.RegularExpressions;

/// <summary>
/// Value object for CustomerName representing the full name of a customer
/// Validation rules:
/// - Must be between 2 and 100 characters
/// - Can contain letters, spaces, hyphens, and apostrophes
/// </summary>
public sealed partial class CustomerName : IEquatable<CustomerName>
{
    private const int MinLength = 2;
    private const int MaxLength = 100;

    public string Value { get; }

    private CustomerName(string value) => Value = value;

    public static bool TryCreate(string? input, out CustomerName? customerName, out string? error)
    {
        customerName = null;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Customer name must not be empty";
            return false;
        }

        var s = input.Trim();

        if (s.Length < MinLength)
        {
            error = $"Customer name must be at least {MinLength} characters";
            return false;
        }

        if (s.Length > MaxLength)
        {
            error = $"Customer name must not exceed {MaxLength} characters";
            return false;
        }

        if (!NameRegex().IsMatch(s))
        {
            error = "Customer name can only contain letters, spaces, hyphens, and apostrophes";
            return false;
        }

        customerName = new CustomerName(s);
        return true;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is CustomerName other && Value == other.Value;

    public bool Equals(CustomerName? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(CustomerName? left, CustomerName? right) =>
        left?.Value == right?.Value;

    public static bool operator !=(CustomerName? left, CustomerName? right) =>
        !(left == right);

    [GeneratedRegex(@"^[\p{L}\s\-']+$")]
    private static partial Regex NameRegex();
}
