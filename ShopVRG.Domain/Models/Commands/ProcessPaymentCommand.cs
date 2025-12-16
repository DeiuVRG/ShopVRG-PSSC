namespace ShopVRG.Domain.Models.Commands;

/// <summary>
/// Command for processing a payment
/// Contains raw string inputs that will be validated by the workflow
/// </summary>
public sealed class ProcessPaymentCommand
{
    public string OrderId { get; init; } = string.Empty;
    public string Amount { get; init; } = string.Empty;
    public string CardNumber { get; init; } = string.Empty;
    public string CardHolderName { get; init; } = string.Empty;
    public string ExpiryDate { get; init; } = string.Empty;
    public string Cvv { get; init; } = string.Empty;
}
