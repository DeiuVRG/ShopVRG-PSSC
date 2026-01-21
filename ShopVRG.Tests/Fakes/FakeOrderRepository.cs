using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Repositories;

namespace ShopVRG.Tests.Fakes;

/// <summary>
/// Fake implementation of IOrderRepository for testing
/// Uses in-memory storage instead of database
/// </summary>
public class FakeOrderRepository : IOrderRepository
{
    private readonly Dictionary<string, FakeOrderData> _orders = new();

    public Task<bool> SaveOrderAsync(OrderId orderId, StockCheckedOrder order)
    {
        _orders[orderId.Value.ToString()] = new FakeOrderData
        {
            OrderId = orderId.Value.ToString(),
            CustomerName = order.CustomerName.Value,
            CustomerEmail = order.CustomerEmail.Value,
            ShippingAddress = order.ShippingAddress,
            TotalPrice = order.TotalPrice,
            IsPaid = false,
            IsShipped = false,
            OrderLines = order.OrderLines.Select(l => (l.ProductCode.Value, l.Quantity.Value)).ToList()
        };
        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(OrderId orderId)
    {
        return Task.FromResult(_orders.ContainsKey(orderId.Value.ToString()));
    }

    public Task<Price?> GetOrderTotalAsync(OrderId orderId)
    {
        if (_orders.TryGetValue(orderId.Value.ToString(), out var order))
        {
            return Task.FromResult<Price?>(order.TotalPrice);
        }
        return Task.FromResult<Price?>(null);
    }

    public Task<ShippingAddress?> GetShippingAddressAsync(OrderId orderId)
    {
        if (_orders.TryGetValue(orderId.Value.ToString(), out var order))
        {
            return Task.FromResult<ShippingAddress?>(order.ShippingAddress);
        }
        return Task.FromResult<ShippingAddress?>(null);
    }

    public Task<bool> IsPaidAsync(OrderId orderId)
    {
        if (_orders.TryGetValue(orderId.Value.ToString(), out var order))
        {
            return Task.FromResult(order.IsPaid);
        }
        return Task.FromResult(false);
    }

    public Task<bool> MarkAsPaidAsync(OrderId orderId)
    {
        if (_orders.TryGetValue(orderId.Value.ToString(), out var order))
        {
            order.IsPaid = true;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> MarkAsShippedAsync(OrderId orderId, string trackingNumber, string carrier)
    {
        if (_orders.TryGetValue(orderId.Value.ToString(), out var order))
        {
            order.IsShipped = true;
            order.TrackingNumber = trackingNumber;
            order.Carrier = carrier;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<IReadOnlyList<(string ProductCode, int Quantity)>> GetOrderLinesAsync(OrderId orderId)
    {
        if (_orders.TryGetValue(orderId.Value.ToString(), out var order))
        {
            return Task.FromResult<IReadOnlyList<(string, int)>>(order.OrderLines);
        }
        return Task.FromResult<IReadOnlyList<(string, int)>>(new List<(string, int)>());
    }

    // Helper methods for testing
    public void Clear() => _orders.Clear();
    public int Count => _orders.Count;
    public FakeOrderData? GetOrder(string orderId) => _orders.GetValueOrDefault(orderId);

    // Sync helper methods for easier test usage
    public void Add(Guid orderId, object order)
    {
        _orders[orderId.ToString()] = new FakeOrderData
        {
            OrderId = orderId.ToString(),
            CustomerName = "Test Customer",
            CustomerEmail = "test@test.com",
            IsPaid = false,
            IsShipped = false,
            OrderLines = []
        };
    }

    public bool Exists(Guid orderId) => _orders.ContainsKey(orderId.ToString());

    public object? GetById(Guid orderId)
    {
        return _orders.GetValueOrDefault(orderId.ToString());
    }

    public IEnumerable<FakeOrderData> GetAll() => _orders.Values;

    public class FakeOrderData
    {
        public string OrderId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public ShippingAddress ShippingAddress { get; set; } = null!;
        public Price TotalPrice { get; set; } = null!;
        public bool IsPaid { get; set; }
        public bool IsShipped { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public List<(string ProductCode, int Quantity)> OrderLines { get; set; } = new();
    }
}
