namespace ShopVRG.Domain.Repositories;

using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Repository interface for Payment operations
/// </summary>
public interface IPaymentRepository
{
    Task<bool> SavePaymentAsync(PaymentId paymentId, OrderId orderId, Price amount, string transactionReference);
    Task<bool> ExistsForOrderAsync(OrderId orderId);
    Task<PaymentId?> GetPaymentIdByOrderAsync(OrderId orderId);
}
