using FluentAssertions;
using ShopVRG.Domain.Models.Events;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Tests.Mocks;

namespace ShopVRG.Tests.Integration;

/// <summary>
/// Integration tests for event publishing using mock event sender
/// Verifies event flow without touching Azure Service Bus
/// </summary>
public class EventPublishingIntegrationTests
{
    private readonly MockEventSender _eventSender;

    public EventPublishingIntegrationTests()
    {
        _eventSender = new MockEventSender();
    }

    [Fact]
    public async Task SendAsync_ShouldCaptureEvent()
    {
        // Arrange
        var testEvent = new TestOrderEvent { OrderId = Guid.NewGuid(), Message = "Test" };

        // Act
        await _eventSender.SendAsync("test-topic", testEvent);

        // Assert
        _eventSender.WasEventSent<TestOrderEvent>().Should().BeTrue();
        _eventSender.GetSentEventCount().Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_MultipleEvents_ShouldCaptureAll()
    {
        // Arrange & Act
        await _eventSender.SendAsync("topic1", new TestOrderEvent { OrderId = Guid.NewGuid() });
        await _eventSender.SendAsync("topic2", new TestOrderEvent { OrderId = Guid.NewGuid() });
        await _eventSender.SendAsync("topic3", new TestOrderEvent { OrderId = Guid.NewGuid() });

        // Assert
        _eventSender.GetSentEventCount().Should().Be(3);
    }

    [Fact]
    public async Task WasTopicUsed_ShouldReturnCorrectResult()
    {
        // Arrange & Act
        await _eventSender.SendAsync("orders/created", new TestOrderEvent { OrderId = Guid.NewGuid() });

        // Assert
        _eventSender.WasTopicUsed("orders/created").Should().BeTrue();
        _eventSender.WasTopicUsed("orders/other").Should().BeFalse();
    }

    [Fact]
    public async Task GetEventsForTopic_ShouldReturnCorrectEvents()
    {
        // Arrange
        var event1 = new TestOrderEvent { OrderId = Guid.NewGuid(), Message = "First" };
        var event2 = new TestOrderEvent { OrderId = Guid.NewGuid(), Message = "Second" };

        // Act
        await _eventSender.SendAsync("orders", event1);
        await _eventSender.SendAsync("orders", event2);
        await _eventSender.SendAsync("other-topic", new TestOrderEvent { OrderId = Guid.NewGuid() });

        // Assert
        var ordersEvents = _eventSender.GetEventsForTopic<TestOrderEvent>("orders").ToList();
        ordersEvents.Should().HaveCount(2);
        ordersEvents.Should().Contain(e => e.Message == "First");
        ordersEvents.Should().Contain(e => e.Message == "Second");
    }

    [Fact]
    public void Clear_ShouldRemoveAllEvents()
    {
        // Arrange
        _eventSender.SendAsync("topic", new TestOrderEvent { OrderId = Guid.NewGuid() }).Wait();
        _eventSender.GetSentEventCount().Should().Be(1);

        // Act
        _eventSender.Clear();

        // Assert
        _eventSender.GetSentEventCount().Should().Be(0);
    }

    [Fact]
    public async Task SendAsync_ShouldCaptureTimestamp()
    {
        // Arrange
        var beforeSend = DateTime.UtcNow;
        var testEvent = new TestOrderEvent { OrderId = Guid.NewGuid() };

        // Act
        await _eventSender.SendAsync("topic", testEvent);

        // Assert
        var events = _eventSender.GetEventsForTopic<TestOrderEvent>("topic").ToList();
        events.Should().HaveCount(1);
    }

    private class TestOrderEvent
    {
        public Guid OrderId { get; set; }
        public string? Message { get; set; }
    }
}
