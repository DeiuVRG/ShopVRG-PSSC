namespace ShopVRG.Domain.Models.Events;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Base interface for Payment events
/// </summary>
public interface IPaymentEvent { }

/// <summary>
/// Success event - Payment has been processed successfully
/// </summary>
public sealed record PaymentProcessedEvent : IPaymentEvent
{
    public PaymentId PaymentId { get; }
    public OrderId OrderId { get; }
    public Price Amount { get; }
    public string MaskedCardNumber { get; }
    public string TransactionReference { get; }
    public DateTime ProcessedAt { get; }

    internal PaymentProcessedEvent(
        PaymentId paymentId,
        OrderId orderId,
        Price amount,
        string maskedCardNumber,
        string transactionReference,
        DateTime processedAt)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        MaskedCardNumber = maskedCardNumber;
        TransactionReference = transactionReference;
        ProcessedAt = processedAt;
    }
}

/// <summary>
/// Failure event - Payment processing failed
/// </summary>
public sealed record PaymentFailedEvent : IPaymentEvent
{
    public string? OrderId { get; }
    public IReadOnlyList<string> Reasons { get; }

    internal PaymentFailedEvent(string? orderId, IEnumerable<string> reasons)
    {
        OrderId = orderId;
        Reasons = reasons.ToList().AsReadOnly();
    }
}

/// <summary>
/// Extension method to convert IPayment to event
/// </summary>
public static class PaymentEventExtensions
{
    public static IPaymentEvent ToEvent(this IPayment payment)
    {
        return payment switch
        {
            ProcessedPayment processed => new PaymentProcessedEvent(
                processed.PaymentId,
                processed.OrderId,
                processed.Amount,
                processed.MaskedCardNumber,
                processed.TransactionReference,
                processed.ProcessedAt),

            InvalidPayment invalid => new PaymentFailedEvent(
                invalid.OrderId,
                invalid.Reasons),

            UnvalidatedPayment unvalidated => new PaymentFailedEvent(
                unvalidated.OrderId,
                ["Payment was not completed - remained in unvalidated state"]),

            ValidatedPayment validated => new PaymentFailedEvent(
                validated.OrderId.ToString(),
                ["Payment was not completed - remained in validated state"]),

            _ => new PaymentFailedEvent(
                null,
                [$"Unknown payment state: {payment.GetType().Name}"])
        };
    }
}
