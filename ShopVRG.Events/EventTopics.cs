namespace ShopVRG.Events;

/// <summary>
/// Event topic constants for async communication
/// </summary>
public static class EventTopics
{
    public const string OrderPlaced = "orders/placed";
    public const string OrderFailed = "orders/failed";
    public const string PaymentProcessed = "payments/processed";
    public const string PaymentFailed = "payments/failed";
    public const string OrderShipped = "shipping/shipped";
    public const string ShippingFailed = "shipping/failed";
}
