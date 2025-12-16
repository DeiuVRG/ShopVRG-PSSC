namespace ShopVRG.Api.Models;

/// <summary>
/// Generic API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// Product DTO for API responses
/// </summary>
public class ProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request to place an order
/// </summary>
public class PlaceOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingStreet { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public List<OrderLineRequest> OrderLines { get; set; } = [];
}

/// <summary>
/// Order line in request
/// </summary>
public class OrderLineRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

/// <summary>
/// Order DTO for API responses
/// </summary>
public class OrderDto
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public AddressDto ShippingAddress { get; set; } = new();
    public List<OrderLineDto> OrderLines { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Address DTO
/// </summary>
public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Order line DTO
/// </summary>
public class OrderLineDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Request to process payment
/// </summary>
public class ProcessPaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
}

/// <summary>
/// Payment DTO for API responses
/// </summary>
public class PaymentDto
{
    public string PaymentId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string MaskedCardNumber { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Request to ship an order
/// </summary>
public class ShipOrderRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
}

/// <summary>
/// Shipment DTO for API responses
/// </summary>
public class ShipmentDto
{
    public string OrderId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public AddressDto Destination { get; set; } = new();
    public DateTime ShippedAt { get; set; }
    public DateTime EstimatedDelivery { get; set; }
}
