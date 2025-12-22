namespace ShopVRG.Events;

/// <summary>
/// Event topic constants for async communication
/// Maps to Azure Service Bus topics
/// </summary>
public static class EventTopics
{
    // Azure Service Bus topic names (must match topics created in Azure)
    public const string OrderPlaced = "order-placed";
    public const string OrderFailed = "order-placed"; // Same topic, different event type
    public const string PaymentProcessed = "payment-processed";
    public const string PaymentFailed = "payment-processed"; // Same topic, different event type
    public const string OrderShipped = "order-shipped";
    public const string ShippingFailed = "order-shipped"; // Same topic, different event type
}
