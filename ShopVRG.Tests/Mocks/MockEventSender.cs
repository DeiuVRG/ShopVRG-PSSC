using ShopVRG.Events;

namespace ShopVRG.Tests.Mocks;

/// <summary>
/// Mock implementation of IEventSender for testing
/// Captures sent events for verification without using Service Bus
/// </summary>
public class MockEventSender : IEventSender
{
    private readonly List<SentEvent> _sentEvents = new();
    private readonly object _lock = new();

    public Task SendAsync<T>(string topic, T @event) where T : class
    {
        lock (_lock)
        {
            _sentEvents.Add(new SentEvent
            {
                Topic = topic,
                Event = @event,
                EventType = typeof(T).Name,
                SentAt = DateTime.UtcNow
            });
        }
        return Task.CompletedTask;
    }

    // Verification methods for testing
    public IReadOnlyList<SentEvent> GetAllSentEvents()
    {
        lock (_lock)
        {
            return _sentEvents.ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<SentEvent> GetEventsByTopic(string topic)
    {
        lock (_lock)
        {
            return _sentEvents.Where(e => e.Topic == topic).ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<T> GetEvents<T>() where T : class
    {
        lock (_lock)
        {
            return _sentEvents
                .Where(e => e.Event is T)
                .Select(e => (T)e.Event)
                .ToList()
                .AsReadOnly();
        }
    }

    public bool WasEventSent<T>() where T : class
    {
        lock (_lock)
        {
            return _sentEvents.Any(e => e.Event is T);
        }
    }

    public bool WasEventSentToTopic<T>(string topic) where T : class
    {
        lock (_lock)
        {
            return _sentEvents.Any(e => e.Topic == topic && e.Event is T);
        }
    }

    public int EventCount => _sentEvents.Count;

    public void Clear()
    {
        lock (_lock)
        {
            _sentEvents.Clear();
        }
    }

    // Additional helper methods for testing
    public int GetSentEventCount() => EventCount;

    public bool WasTopicUsed(string topic)
    {
        lock (_lock)
        {
            return _sentEvents.Any(e => e.Topic == topic);
        }
    }

    public IEnumerable<T> GetEventsForTopic<T>(string topic) where T : class
    {
        lock (_lock)
        {
            return _sentEvents
                .Where(e => e.Topic == topic && e.Event is T)
                .Select(e => (T)e.Event)
                .ToList();
        }
    }

    public class SentEvent
    {
        public string Topic { get; set; } = "";
        public object Event { get; set; } = null!;
        public string EventType { get; set; } = "";
        public DateTime SentAt { get; set; }
    }
}
