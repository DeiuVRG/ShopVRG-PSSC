namespace ShopVRG.Events.ServiceBus;

using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

/// <summary>
/// Azure Service Bus implementation of IEventSender
/// Uses CloudEvents specification for event formatting
/// </summary>
public class ServiceBusEventSender : IEventSender, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();
    private readonly string _source;

    public ServiceBusEventSender(string connectionString, string source = "shopvrg")
    {
        _client = new ServiceBusClient(connectionString);
        _source = source;
    }

    public async Task SendAsync<T>(string topic, T @event) where T : class
    {
        var sender = _senders.GetOrAdd(topic, t => _client.CreateSender(t));

        var cloudEvent = CreateCloudEvent(topic, @event);
        var formatter = new JsonEventFormatter();
        var bytes = formatter.EncodeStructuredModeMessage(cloudEvent, out _);

        var message = new ServiceBusMessage(bytes)
        {
            ContentType = "application/cloudevents+json",
            Subject = typeof(T).Name
        };

        await sender.SendMessageAsync(message);
    }

    private CloudEvent CreateCloudEvent<T>(string topic, T data) where T : class
    {
        return new CloudEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = typeof(T).Name,
            Source = new Uri($"urn:{_source}"),
            Time = DateTimeOffset.UtcNow,
            DataContentType = "application/json",
            Data = JsonSerializer.Serialize(data),
            Subject = topic
        };
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }
        await _client.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
