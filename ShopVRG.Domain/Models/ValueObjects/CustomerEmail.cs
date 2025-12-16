namespace ShopVRG.Domain.Models.ValueObjects;

using System.Text.RegularExpressions;

/// <summary>
/// Value object for CustomerEmail representing a valid email address
/// Validation rules:
/// - Must be a valid email format
/// - Must not be empty
/// - Maximum length 254 characters (RFC 5321)
/// </summary>
public sealed partial class CustomerEmail : IEquatable<CustomerEmail>
{
    private const int MaxLength = 254;

    public string Value { get; }

    private CustomerEmail(string value) => Value = value;

    public static bool TryCreate(string? input, out CustomerEmail? email, out string? error)
    {
        email = null;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Email must not be empty";
            return false;
        }

        var s = input.Trim().ToLowerInvariant();

        if (s.Length > MaxLength)
        {
            error = $"Email must not exceed {MaxLength} characters";
            return false;
        }

        if (!EmailRegex().IsMatch(s))
        {
            error = "Invalid email format";
            return false;
        }

        email = new CustomerEmail(s);
        return true;
    }

    public string GetDomain() => Value.Split('@')[1];

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is CustomerEmail other && Value == other.Value;

    public bool Equals(CustomerEmail? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(CustomerEmail? left, CustomerEmail? right) =>
        left?.Value == right?.Value;

    public static bool operator !=(CustomerEmail? left, CustomerEmail? right) =>
        !(left == right);

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}
