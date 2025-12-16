namespace ShopVRG.Domain.Models.Commands;

/// <summary>
/// Command for shipping an order
/// Contains raw string inputs that will be validated by the workflow
/// </summary>
public sealed class ShipOrderCommand
{
    public string OrderId { get; init; } = string.Empty;
    public string Carrier { get; init; } = string.Empty;
}
