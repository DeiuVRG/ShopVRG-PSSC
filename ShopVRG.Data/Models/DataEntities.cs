namespace ShopVRG.Data.Models;

/// <summary>
/// Product entity for database persistence
/// </summary>
public class ProductEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Order entity for database persistence
/// </summary>
public class OrderEntity
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingStreet { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = OrderStatus.Placed;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }

    public ICollection<OrderLineEntity> OrderLines { get; set; } = new List<OrderLineEntity>();
}

/// <summary>
/// Order status constants
/// </summary>
public static class OrderStatus
{
    public const string Placed = "Placed";
    public const string Paid = "Paid";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
}

/// <summary>
/// Order line entity for database persistence
/// </summary>
public class OrderLineEntity
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Payment entity for database persistence
/// </summary>
public class PaymentEntity
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string MaskedCardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Shipment entity for database persistence
/// </summary>
public class ShipmentEntity
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public DateTime ShippedAt { get; set; }
    public DateTime EstimatedDelivery { get; set; }
}
