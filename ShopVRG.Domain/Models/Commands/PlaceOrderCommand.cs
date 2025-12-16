namespace ShopVRG.Domain.Models.Commands;

/// <summary>
/// Command for placing an order
/// Contains raw string inputs that will be validated by the workflow
/// </summary>
public sealed class PlaceOrderCommand
{
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string ShippingStreet { get; init; } = string.Empty;
    public string ShippingCity { get; init; } = string.Empty;
    public string ShippingPostalCode { get; init; } = string.Empty;
    public string ShippingCountry { get; init; } = string.Empty;
    public IReadOnlyList<OrderLineCommand> OrderLines { get; init; } = [];
}

/// <summary>
/// Command for an order line
/// </summary>
public sealed class OrderLineCommand
{
    public string ProductCode { get; init; } = string.Empty;
    public string Quantity { get; init; } = string.Empty;
}
