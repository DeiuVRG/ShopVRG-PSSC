namespace ShopVRG.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using ShopVRG.Data.Models;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Repositories;

/// <summary>
/// Repository implementation for Payment operations
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly ShopDbContext _context;

    public PaymentRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SavePaymentAsync(
        PaymentId paymentId,
        OrderId orderId,
        Price amount,
        string transactionReference)
    {
        try
        {
            var entity = new PaymentEntity
            {
                PaymentId = paymentId.Value,
                OrderId = orderId.Value,
                Amount = amount.Value,
                MaskedCardNumber = "****-****-****-****",
                CardHolderName = "Card Holder",
                TransactionReference = transactionReference,
                ProcessedAt = DateTime.UtcNow
            };

            _context.Payments.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ExistsForOrderAsync(OrderId orderId)
    {
        return await _context.Payments.AnyAsync(p => p.OrderId == orderId.Value);
    }

    public async Task<PaymentId?> GetPaymentIdByOrderAsync(OrderId orderId)
    {
        var entity = await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == orderId.Value);

        if (entity == null) return null;

        PaymentId.TryCreate(entity.PaymentId.ToString(), out var paymentId, out _);
        return paymentId;
    }
}
