namespace ShopVRG.Domain.Models.Entities;

using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Base interface for Payment entity
/// States: Unvalidated -> Validated -> Processed / Invalid
/// </summary>
public interface IPayment { }

/// <summary>
/// Initial state - raw payment input
/// </summary>
public sealed record UnvalidatedPayment : IPayment
{
    public string OrderId { get; }
    public string Amount { get; }
    public string CardNumber { get; }
    public string CardHolderName { get; }
    public string ExpiryDate { get; }
    public string Cvv { get; }

    public UnvalidatedPayment(
        string orderId,
        string amount,
        string cardNumber,
        string cardHolderName,
        string expiryDate,
        string cvv)
    {
        OrderId = orderId;
        Amount = amount;
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        ExpiryDate = expiryDate;
        Cvv = cvv;
    }
}

/// <summary>
/// Validated state - payment details validated
/// </summary>
public sealed record ValidatedPayment : IPayment
{
    public PaymentId PaymentId { get; }
    public OrderId OrderId { get; }
    public Price Amount { get; }
    public string MaskedCardNumber { get; }
    public string CardHolderName { get; }
    public DateTime ValidatedAt { get; }

    internal ValidatedPayment(
        PaymentId paymentId,
        OrderId orderId,
        Price amount,
        string maskedCardNumber,
        string cardHolderName,
        DateTime validatedAt)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        MaskedCardNumber = maskedCardNumber;
        CardHolderName = cardHolderName;
        ValidatedAt = validatedAt;
    }
}

/// <summary>
/// Final successful state - payment processed
/// </summary>
public sealed record ProcessedPayment : IPayment
{
    public PaymentId PaymentId { get; }
    public OrderId OrderId { get; }
    public Price Amount { get; }
    public string MaskedCardNumber { get; }
    public string CardHolderName { get; }
    public string TransactionReference { get; }
    public DateTime ProcessedAt { get; }

    internal ProcessedPayment(
        PaymentId paymentId,
        OrderId orderId,
        Price amount,
        string maskedCardNumber,
        string cardHolderName,
        string transactionReference,
        DateTime processedAt)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        MaskedCardNumber = maskedCardNumber;
        CardHolderName = cardHolderName;
        TransactionReference = transactionReference;
        ProcessedAt = processedAt;
    }
}

/// <summary>
/// Invalid payment state
/// </summary>
public sealed record InvalidPayment : IPayment
{
    public string? OrderId { get; }
    public IReadOnlyList<string> Reasons { get; }

    internal InvalidPayment(string? orderId, IEnumerable<string> reasons)
    {
        OrderId = orderId;
        Reasons = reasons.ToList().AsReadOnly();
    }
}
