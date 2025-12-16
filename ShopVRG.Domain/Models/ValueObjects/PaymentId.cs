namespace ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Value object for PaymentId representing a unique payment identifier
/// Uses GUID for uniqueness
/// </summary>
public sealed class PaymentId : IEquatable<PaymentId>
{
    public Guid Value { get; }

    private PaymentId(Guid value) => Value = value;

    public static PaymentId NewPaymentId() => new(Guid.NewGuid());

    public static bool TryCreate(string? input, out PaymentId? paymentId, out string? error)
    {
        paymentId = null;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Payment ID must not be empty";
            return false;
        }

        if (!Guid.TryParse(input.Trim(), out var guid))
        {
            error = "Invalid Payment ID format";
            return false;
        }

        paymentId = new PaymentId(guid);
        return true;
    }

    public static PaymentId FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Payment ID cannot be empty", nameof(value));
        return new PaymentId(value);
    }

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj) => obj is PaymentId other && Value == other.Value;

    public bool Equals(PaymentId? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(PaymentId? left, PaymentId? right) =>
        left?.Value == right?.Value;

    public static bool operator !=(PaymentId? left, PaymentId? right) =>
        !(left == right);
}
