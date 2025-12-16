namespace ShopVRG.Domain.Repositories;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Repository interface for Order operations
/// </summary>
public interface IOrderRepository
{
    Task<bool> SaveOrderAsync(OrderId orderId, StockCheckedOrder order);
    Task<bool> ExistsAsync(OrderId orderId);
    Task<Price?> GetOrderTotalAsync(OrderId orderId);
    Task<ShippingAddress?> GetShippingAddressAsync(OrderId orderId);
    Task<bool> IsPaidAsync(OrderId orderId);
    Task<bool> MarkAsPaidAsync(OrderId orderId);
    Task<bool> MarkAsShippedAsync(OrderId orderId, string trackingNumber, string carrier);
}
