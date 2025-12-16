namespace ShopVRG.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using ShopVRG.Data.Models;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Repositories;

/// <summary>
/// Repository implementation for Order operations
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly ShopDbContext _context;

    public OrderRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SaveOrderAsync(OrderId orderId, StockCheckedOrder order)
    {
        try
        {
            var entity = new OrderEntity
            {
                OrderId = orderId.Value,
                CustomerName = order.CustomerName.Value,
                CustomerEmail = order.CustomerEmail.Value,
                ShippingStreet = order.ShippingAddress.Street,
                ShippingCity = order.ShippingAddress.City,
                ShippingPostalCode = order.ShippingAddress.PostalCode,
                ShippingCountry = order.ShippingAddress.Country,
                TotalPrice = order.TotalPrice.Value,
                Status = OrderStatus.Placed,
                CreatedAt = order.CreatedAt,
                OrderLines = order.OrderLines.Select(l => new OrderLineEntity
                {
                    OrderId = orderId.Value,
                    ProductCode = l.ProductCode.Value,
                    ProductName = l.ProductName.Value,
                    Quantity = l.Quantity.Value,
                    UnitPrice = l.UnitPrice.Value,
                    LineTotal = l.LineTotal.Value
                }).ToList()
            };

            _context.Orders.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ExistsAsync(OrderId orderId)
    {
        return await _context.Orders.AnyAsync(o => o.OrderId == orderId.Value);
    }

    public async Task<Price?> GetOrderTotalAsync(OrderId orderId)
    {
        var entity = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == orderId.Value);

        if (entity == null) return null;

        Price.TryCreate(entity.TotalPrice, out var price, out _);
        return price;
    }

    public async Task<ShippingAddress?> GetShippingAddressAsync(OrderId orderId)
    {
        var entity = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == orderId.Value);

        if (entity == null) return null;

        ShippingAddress.TryCreate(
            entity.ShippingStreet,
            entity.ShippingCity,
            entity.ShippingPostalCode,
            entity.ShippingCountry,
            out var address,
            out _);

        return address;
    }

    public async Task<bool> IsPaidAsync(OrderId orderId)
    {
        var entity = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == orderId.Value);

        return entity?.Status == OrderStatus.Paid || entity?.Status == OrderStatus.Shipped;
    }

    public async Task<bool> MarkAsPaidAsync(OrderId orderId)
    {
        try
        {
            var entity = await _context.Orders.FindAsync(orderId.Value);
            if (entity == null) return false;

            entity.Status = OrderStatus.Paid;
            entity.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> MarkAsShippedAsync(OrderId orderId, string trackingNumber, string carrier)
    {
        try
        {
            var entity = await _context.Orders.FindAsync(orderId.Value);
            if (entity == null) return false;

            entity.Status = OrderStatus.Shipped;
            entity.ShippedAt = DateTime.UtcNow;

            _context.Shipments.Add(new ShipmentEntity
            {
                OrderId = orderId.Value,
                TrackingNumber = trackingNumber,
                Carrier = carrier,
                ShippedAt = DateTime.UtcNow,
                EstimatedDelivery = DateTime.UtcNow.AddDays(GetEstimatedDays(carrier))
            });

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static int GetEstimatedDays(string carrier) => carrier switch
    {
        "DHL" => 3,
        "FEDEX" => 2,
        "UPS" => 3,
        "DPD" => 4,
        "GLS" => 4,
        "CARGUS" => 2,
        "FAN_COURIER" => 1,
        "SAMEDAY" => 1,
        _ => 5
    };
}
