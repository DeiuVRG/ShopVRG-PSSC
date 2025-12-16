namespace ShopVRG.Events.ServiceBus;

using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// In-memory implementation of IEventSender for local development
/// Events are stored in memory and logged
/// </summary>
public class InMemoryEventSender : IEventSender
{
    private readonly ILogger<InMemoryEventSender> _logger;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _events = new();

    public InMemoryEventSender(ILogger<InMemoryEventSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync<T>(string topic, T @event) where T : class
    {
        var queue = _events.GetOrAdd(topic, _ => new ConcurrentQueue<string>());
        var json = JsonSerializer.Serialize(@event, new JsonSerializerOptions { WriteIndented = true });

        queue.Enqueue(json);

        _logger.LogInformation(
            "Event published to topic '{Topic}':\nType: {EventType}\nPayload: {Payload}",
            topic,
            typeof(T).Name,
            json);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets all events for a specific topic (for testing purposes)
    /// </summary>
    public IReadOnlyList<string> GetEvents(string topic)
    {
        return _events.TryGetValue(topic, out var queue)
            ? queue.ToList().AsReadOnly()
            : Array.Empty<string>().AsReadOnly();
    }

    /// <summary>
    /// Clears all events (for testing purposes)
    /// </summary>
    public void Clear()
    {
        _events.Clear();
    }
}
