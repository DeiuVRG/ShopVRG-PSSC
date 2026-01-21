using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Repositories;

namespace ShopVRG.Tests.Fakes;

/// <summary>
/// Fake implementation of IPaymentRepository for testing
/// Uses in-memory storage instead of database
/// </summary>
public class FakePaymentRepository : IPaymentRepository
{
    private readonly Dictionary<string, FakePaymentData> _payments = new();
    private readonly Dictionary<string, string> _orderToPayment = new();

    public Task<bool> SavePaymentAsync(PaymentId paymentId, OrderId orderId, Price amount, string transactionReference)
    {
        var payment = new FakePaymentData
        {
            PaymentId = paymentId.Value.ToString(),
            OrderId = orderId.Value.ToString(),
            Amount = amount,
            TransactionReference = transactionReference,
            ProcessedAt = DateTime.UtcNow
        };

        _payments[paymentId.Value.ToString()] = payment;
        _orderToPayment[orderId.Value.ToString()] = paymentId.Value.ToString();

        return Task.FromResult(true);
    }

    public Task<bool> ExistsForOrderAsync(OrderId orderId)
    {
        return Task.FromResult(_orderToPayment.ContainsKey(orderId.Value.ToString()));
    }

    public Task<PaymentId?> GetPaymentIdByOrderAsync(OrderId orderId)
    {
        if (_orderToPayment.TryGetValue(orderId.Value.ToString(), out var paymentId))
        {
            PaymentId.TryCreate(paymentId, out var id, out _);
            return Task.FromResult(id);
        }
        return Task.FromResult<PaymentId?>(null);
    }

    // Helper methods for testing
    public void Clear()
    {
        _payments.Clear();
        _orderToPayment.Clear();
    }

    public int Count => _payments.Count;
    public FakePaymentData? GetPayment(string paymentId) => _payments.GetValueOrDefault(paymentId);
    public FakePaymentData? GetPaymentByOrder(string orderId)
    {
        if (_orderToPayment.TryGetValue(orderId, out var paymentId))
            return _payments.GetValueOrDefault(paymentId);
        return null;
    }

    // Sync helper methods for easier test usage
    public void Add(Guid paymentId, Guid orderId, decimal amount, string transactionRef)
    {
        var payment = new FakePaymentData
        {
            PaymentId = paymentId.ToString(),
            OrderId = orderId.ToString(),
            Amount = Price.FromDecimal(amount),
            TransactionReference = transactionRef,
            ProcessedAt = DateTime.UtcNow
        };

        _payments[paymentId.ToString()] = payment;
        _orderToPayment[orderId.ToString()] = paymentId.ToString();
    }

    public bool Exists(Guid paymentId) => _payments.ContainsKey(paymentId.ToString());

    public FakePaymentData? GetById(Guid paymentId) => _payments.GetValueOrDefault(paymentId.ToString());

    public FakePaymentData? GetByOrderId(Guid orderId)
    {
        if (_orderToPayment.TryGetValue(orderId.ToString(), out var paymentId))
            return _payments.GetValueOrDefault(paymentId);
        return null;
    }

    public class FakePaymentData
    {
        public string PaymentId { get; set; } = "";
        public string OrderId { get; set; } = "";
        public Price Amount { get; set; } = null!;
        public string TransactionReference { get; set; } = "";
        public DateTime ProcessedAt { get; set; }
    }
}
