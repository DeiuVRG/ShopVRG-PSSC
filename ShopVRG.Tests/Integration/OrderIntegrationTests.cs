using FluentAssertions;
using ShopVRG.Domain.Models.Commands;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.Events;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Workflows;
using ShopVRG.Tests.Fakes;
using ShopVRG.Tests.Mocks;

namespace ShopVRG.Tests.Integration;

/// <summary>
/// Integration tests for Order workflow using fakes and mocks
/// Tests the full pipeline without touching external services
/// </summary>
public class OrderIntegrationTests
{
    private readonly FakeProductRepository _productRepository;
    private readonly FakeOrderRepository _orderRepository;
    private readonly MockEventSender _eventSender;
    private readonly PlaceOrderWorkflow _workflow;

    public OrderIntegrationTests()
    {
        _productRepository = new FakeProductRepository();
        _orderRepository = new FakeOrderRepository();
        _eventSender = new MockEventSender();
        _workflow = new PlaceOrderWorkflow();
    }

    private bool CheckProductExists(ProductCode code) =>
        _productRepository.Exists(code.Value);

    private (ProductName Name, Price Price, StockQuantity Stock)? GetProductDetails(ProductCode code)
    {
        var product = _productRepository.GetByCode(code.Value);
        if (product == null) return null;

        ProductName.TryCreate(product.Name, out var name, out _);
        return (name!, Price.FromDecimal(product.Price), StockQuantity.FromInt(product.Stock));
    }

    private bool ReserveStock(ProductCode code, Quantity quantity) =>
        _productRepository.ReserveStock(code.Value, quantity.Value);

    private bool PersistOrder(OrderId orderId, StockCheckedOrder order)
    {
        _orderRepository.Add(orderId.Value, order);
        return true;
    }

    [Fact]
    public async Task PlaceOrder_WithValidData_ShouldSucceedAndPublishEvent()
    {
        // Arrange
        var command = new PlaceOrderCommand
        {
            CustomerName = "Integration Test User",
            CustomerEmail = "test@integration.com",
            ShippingStreet = "123 Test Street",
            ShippingCity = "Test City",
            ShippingPostalCode = "12345",
            ShippingCountry = "Test Country",
            OrderLines = [new OrderLineCommand { ProductCode = "GPU001", Quantity = "2" }]
        };

        // Act
        var result = _workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Simulate publishing event
        if (result is OrderPendingPaymentEvent pendingEvent)
        {
            await _eventSender.SendAsync("orders/pending", pendingEvent);
        }

        // Assert
        result.Should().BeOfType<OrderPendingPaymentEvent>();
        _eventSender.WasEventSent<OrderPendingPaymentEvent>().Should().BeTrue();
        _eventSender.GetSentEventCount().Should().Be(1);
    }

    [Fact]
    public void PlaceOrder_WithValidData_ShouldPersistToFakeRepository()
    {
        // Arrange
        var command = new PlaceOrderCommand
        {
            CustomerName = "Integration Test User",
            CustomerEmail = "test@integration.com",
            ShippingStreet = "123 Test Street",
            ShippingCity = "Test City",
            ShippingPostalCode = "12345",
            ShippingCountry = "Test Country",
            OrderLines = [new OrderLineCommand { ProductCode = "CPU001", Quantity = "1" }]
        };

        // Act
        var result = _workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPendingPaymentEvent>();
        var pendingEvent = (OrderPendingPaymentEvent)result;

        // Verify order was persisted
        var savedOrder = _orderRepository.GetById(pendingEvent.OrderId.Value);
        savedOrder.Should().NotBeNull();
    }

    [Fact]
    public void PlaceOrder_WithValidData_ShouldUpdateProductStock()
    {
        // Arrange
        var initialStock = _productRepository.GetByCode("GPU001")!.Stock;
        var command = new PlaceOrderCommand
        {
            CustomerName = "Integration Test User",
            CustomerEmail = "test@integration.com",
            ShippingStreet = "123 Test Street",
            ShippingCity = "Test City",
            ShippingPostalCode = "12345",
            ShippingCountry = "Test Country",
            OrderLines = [new OrderLineCommand { ProductCode = "GPU001", Quantity = "3" }]
        };

        // Act
        _workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        var updatedStock = _productRepository.GetByCode("GPU001")!.Stock;
        updatedStock.Should().Be(initialStock - 3);
    }

    [Fact]
    public void PlaceOrder_WithNonexistentProduct_ShouldFailWithoutSideEffects()
    {
        // Arrange
        var command = new PlaceOrderCommand
        {
            CustomerName = "Integration Test User",
            CustomerEmail = "test@integration.com",
            ShippingStreet = "123 Test Street",
            ShippingCity = "Test City",
            ShippingPostalCode = "12345",
            ShippingCountry = "Test Country",
            OrderLines = [new OrderLineCommand { ProductCode = "NONEXISTENT001", Quantity = "1" }]
        };

        // Act
        var result = _workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPlacementFailedEvent>();
        _orderRepository.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void PlaceOrder_MultipleItems_ShouldReserveStockForAll()
    {
        // Arrange
        var gpuInitialStock = _productRepository.GetByCode("GPU001")!.Stock;
        var cpuInitialStock = _productRepository.GetByCode("CPU001")!.Stock;

        var command = new PlaceOrderCommand
        {
            CustomerName = "Integration Test User",
            CustomerEmail = "test@integration.com",
            ShippingStreet = "123 Test Street",
            ShippingCity = "Test City",
            ShippingPostalCode = "12345",
            ShippingCountry = "Test Country",
            OrderLines =
            [
                new OrderLineCommand { ProductCode = "GPU001", Quantity = "1" },
                new OrderLineCommand { ProductCode = "CPU001", Quantity = "2" }
            ]
        };

        // Act
        var result = _workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPendingPaymentEvent>();
        _productRepository.GetByCode("GPU001")!.Stock.Should().Be(gpuInitialStock - 1);
        _productRepository.GetByCode("CPU001")!.Stock.Should().Be(cpuInitialStock - 2);
    }

    [Fact]
    public async Task PlaceOrder_ValidationFails_ShouldNotPublishEvent()
    {
        // Arrange
        var command = new PlaceOrderCommand
        {
            CustomerName = "Integration Test User",
            CustomerEmail = "invalid-email",
            ShippingStreet = "123 Test Street",
            ShippingCity = "Test City",
            ShippingPostalCode = "12345",
            ShippingCountry = "Test Country",
            OrderLines = [new OrderLineCommand { ProductCode = "GPU001", Quantity = "1" }]
        };

        // Act
        var result = _workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Simulate publishing only on success
        if (result is OrderPendingPaymentEvent pendingEvent)
        {
            await _eventSender.SendAsync("orders/pending", pendingEvent);
        }

        // Assert
        result.Should().BeOfType<OrderPlacementFailedEvent>();
        _eventSender.GetSentEventCount().Should().Be(0);
    }

    [Fact]
    public void PlaceOrder_InsufficientStock_ShouldReturnError()
    {
        // Arrange - request more than available
        var command = new PlaceOrderCommand
        {
            CustomerName = "Integration Test User",
            CustomerEmail = "test@integration.com",
            ShippingStreet = "123 Test Street",
            ShippingCity = "Test City",
            ShippingPostalCode = "12345",
            ShippingCountry = "Test Country",
            OrderLines = [new OrderLineCommand { ProductCode = "GPU001", Quantity = "1000" }]
        };

        // Act
        var result = _workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPlacementFailedEvent>();
        var failedEvent = (OrderPlacementFailedEvent)result;
        failedEvent.Reasons.Should().Contain(r => r.Contains("Insufficient stock"));
    }
}
