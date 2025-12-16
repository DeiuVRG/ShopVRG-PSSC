namespace ShopVRG.Events;

/// <summary>
/// Interface for sending domain events asynchronously
/// </summary>
public interface IEventSender
{
    Task SendAsync<T>(string topic, T @event) where T : class;
}
