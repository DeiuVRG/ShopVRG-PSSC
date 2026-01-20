namespace ShopVRG.Domain.Models.Entities;

using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Base interface for Order entity representing the aggregate root
/// Implements State Machine pattern for order processing
/// States: Unvalidated -> Validated -> StockChecked -> Placed / Invalid
/// </summary>
public interface IOrder { }

/// <summary>
/// Initial state - raw input from the API that needs validation
/// </summary>
public sealed record UnvalidatedOrder : IOrder
{
    public string CustomerName { get; }
    public string CustomerEmail { get; }
    public string ShippingStreet { get; }
    public string ShippingCity { get; }
    public string ShippingPostalCode { get; }
    public string ShippingCountry { get; }
    public IReadOnlyList<UnvalidatedOrderLine> OrderLines { get; }

    public UnvalidatedOrder(
        string customerName,
        string customerEmail,
        string shippingStreet,
        string shippingCity,
        string shippingPostalCode,
        string shippingCountry,
        IEnumerable<UnvalidatedOrderLine> orderLines)
    {
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        ShippingStreet = shippingStreet;
        ShippingCity = shippingCity;
        ShippingPostalCode = shippingPostalCode;
        ShippingCountry = shippingCountry;
        OrderLines = orderLines.ToList().AsReadOnly();
    }
}

/// <summary>
/// Unvalidated order line - raw input
/// </summary>
public sealed record UnvalidatedOrderLine(
    string ProductCode,
    string Quantity);

/// <summary>
/// Validated state - all inputs have been validated and converted to value objects
/// </summary>
public sealed record ValidatedOrder : IOrder
{
    public OrderId OrderId { get; }
    public CustomerName CustomerName { get; }
    public CustomerEmail CustomerEmail { get; }
    public ShippingAddress ShippingAddress { get; }
    public IReadOnlyList<ValidatedOrderLine> OrderLines { get; }
    public DateTime CreatedAt { get; }

    internal ValidatedOrder(
        OrderId orderId,
        CustomerName customerName,
        CustomerEmail customerEmail,
        ShippingAddress shippingAddress,
        IEnumerable<ValidatedOrderLine> orderLines,
        DateTime createdAt)
    {
        OrderId = orderId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
        OrderLines = orderLines.ToList().AsReadOnly();
        CreatedAt = createdAt;
    }
}

/// <summary>
/// Validated order line with value objects
/// </summary>
public sealed record ValidatedOrderLine(
    ProductCode ProductCode,
    Quantity Quantity);

/// <summary>
/// Stock checked state - stock has been verified for all products
/// </summary>
public sealed record StockCheckedOrder : IOrder
{
    public OrderId OrderId { get; }
    public CustomerName CustomerName { get; }
    public CustomerEmail CustomerEmail { get; }
    public ShippingAddress ShippingAddress { get; }
    public IReadOnlyList<StockCheckedOrderLine> OrderLines { get; }
    public DateTime CreatedAt { get; }

    internal StockCheckedOrder(
        OrderId orderId,
        CustomerName customerName,
        CustomerEmail customerEmail,
        ShippingAddress shippingAddress,
        IEnumerable<StockCheckedOrderLine> orderLines,
        DateTime createdAt)
    {
        OrderId = orderId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
        OrderLines = orderLines.ToList().AsReadOnly();
        CreatedAt = createdAt;
    }

    public Price TotalPrice => OrderLines
        .Select(line => line.LineTotal)
        .Aggregate(Price.FromDecimal(0.01m), (acc, price) => Price.FromDecimal(acc.Value + price.Value - 0.01m));
}

/// <summary>
/// Stock checked order line with price information
/// </summary>
public sealed record StockCheckedOrderLine(
    ProductCode ProductCode,
    ProductName ProductName,
    Quantity Quantity,
    Price UnitPrice,
    Price LineTotal);

/// <summary>
/// Final successful state - order has been placed
/// </summary>
public sealed record PlacedOrder : IOrder
{
    public OrderId OrderId { get; }
    public CustomerName CustomerName { get; }
    public CustomerEmail CustomerEmail { get; }
    public ShippingAddress ShippingAddress { get; }
    public IReadOnlyList<PlacedOrderLine> OrderLines { get; }
    public Price TotalPrice { get; }
    public DateTime CreatedAt { get; }
    public DateTime PlacedAt { get; }

    internal PlacedOrder(
        OrderId orderId,
        CustomerName customerName,
        CustomerEmail customerEmail,
        ShippingAddress shippingAddress,
        IEnumerable<PlacedOrderLine> orderLines,
        Price totalPrice,
        DateTime createdAt,
        DateTime placedAt)
    {
        OrderId = orderId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
        OrderLines = orderLines.ToList().AsReadOnly();
        TotalPrice = totalPrice;
        CreatedAt = createdAt;
        PlacedAt = placedAt;
    }
}

/// <summary>
/// Placed order line
/// </summary>
public sealed record PlacedOrderLine(
    ProductCode ProductCode,
    ProductName ProductName,
    Quantity Quantity,
    Price UnitPrice,
    Price LineTotal);

/// <summary>
/// Pending payment state - order created but awaiting payment confirmation
/// </summary>
public sealed record PendingOrder : IOrder
{
    public OrderId OrderId { get; }
    public CustomerName CustomerName { get; }
    public CustomerEmail CustomerEmail { get; }
    public ShippingAddress ShippingAddress { get; }
    public IReadOnlyList<PendingOrderLine> OrderLines { get; }
    public Price TotalPrice { get; }
    public DateTime CreatedAt { get; }

    internal PendingOrder(
        OrderId orderId,
        CustomerName customerName,
        CustomerEmail customerEmail,
        ShippingAddress shippingAddress,
        IEnumerable<PendingOrderLine> orderLines,
        Price totalPrice,
        DateTime createdAt)
    {
        OrderId = orderId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
        OrderLines = orderLines.ToList().AsReadOnly();
        TotalPrice = totalPrice;
        CreatedAt = createdAt;
    }
}

/// <summary>
/// Pending order line
/// </summary>
public sealed record PendingOrderLine(
    ProductCode ProductCode,
    ProductName ProductName,
    Quantity Quantity,
    Price UnitPrice,
    Price LineTotal);

/// <summary>
/// Invalid state - order processing failed
/// </summary>
public sealed record InvalidOrder : IOrder
{
    public IReadOnlyList<string> Reasons { get; }

    internal InvalidOrder(IEnumerable<string> reasons)
    {
        Reasons = reasons.ToList().AsReadOnly();
    }
}
