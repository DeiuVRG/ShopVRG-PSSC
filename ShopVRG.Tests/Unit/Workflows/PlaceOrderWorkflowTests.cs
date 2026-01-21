using FluentAssertions;
using ShopVRG.Domain.Models.Commands;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.Events;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Workflows;

namespace ShopVRG.Tests.Unit.Workflows;

/// <summary>
/// Unit tests for PlaceOrderWorkflow
/// Tests the complete order placement pipeline using fake dependencies
/// </summary>
public class PlaceOrderWorkflowTests
{
    private readonly Dictionary<string, (ProductName Name, Price Price, StockQuantity Stock)> _products;
    private readonly List<(OrderId Id, StockCheckedOrder Order)> _persistedOrders;

    public PlaceOrderWorkflowTests()
    {
        // Setup fake product catalog
        ProductName.TryCreate("RTX 4090 Gaming GPU", out var gpuName, out _);
        ProductName.TryCreate("Intel Core i9-13900K", out var cpuName, out _);
        ProductName.TryCreate("Corsair DDR5 32GB", out var ramName, out _);

        _products = new Dictionary<string, (ProductName, Price, StockQuantity)>
        {
            ["GPU001"] = (gpuName!, Price.FromDecimal(1599.99m), StockQuantity.FromInt(10)),
            ["CPU002"] = (cpuName!, Price.FromDecimal(599.99m), StockQuantity.FromInt(25)),
            ["RAM003"] = (ramName!, Price.FromDecimal(189.99m), StockQuantity.FromInt(50))
        };

        _persistedOrders = [];
    }

    private bool CheckProductExists(ProductCode code) => _products.ContainsKey(code.Value);

    private (ProductName Name, Price Price, StockQuantity Stock)? GetProductDetails(ProductCode code)
    {
        return _products.TryGetValue(code.Value, out var details) ? details : null;
    }

    private bool ReserveStock(ProductCode code, Quantity quantity)
    {
        if (!_products.TryGetValue(code.Value, out var details))
            return false;

        if (!details.Stock.HasEnoughStock(quantity))
            return false;

        var newStock = details.Stock.Decrease(quantity);
        _products[code.Value] = (details.Name, details.Price, newStock);
        return true;
    }

    private bool PersistOrder(OrderId orderId, StockCheckedOrder order)
    {
        _persistedOrders.Add((orderId, order));
        return true;
    }

    [Fact]
    public void Execute_ValidOrder_ShouldReturnOrderPendingPaymentEvent()
    {
        // Arrange
        var workflow = new PlaceOrderWorkflow();
        var command = new PlaceOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingStreet = "123 Main Street",
            ShippingCity = "Bucharest",
            ShippingPostalCode = "010101",
            ShippingCountry = "Romania",
            OrderLines = [new OrderLineCommand { ProductCode = "GPU001", Quantity = "2" }]
        };

        // Act
        var result = workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPendingPaymentEvent>();
        var pendingEvent = (OrderPendingPaymentEvent)result;
        pendingEvent.CustomerName.Value.Should().Be("John Doe");
        pendingEvent.CustomerEmail.Value.Should().Be("john@example.com");
        pendingEvent.OrderLines.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_ValidOrder_ShouldPersistOrder()
    {
        // Arrange
        var workflow = new PlaceOrderWorkflow();
        var command = new PlaceOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingStreet = "123 Main Street",
            ShippingCity = "Bucharest",
            ShippingPostalCode = "010101",
            ShippingCountry = "Romania",
            OrderLines = [new OrderLineCommand { ProductCode = "GPU001", Quantity = "1" }]
        };

        // Act
        workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        _persistedOrders.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_ValidOrder_ShouldReserveStock()
    {
        // Arrange
        var workflow = new PlaceOrderWorkflow();
        var initialStock = _products["GPU001"].Stock.Value;
        var command = new PlaceOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingStreet = "123 Main Street",
            ShippingCity = "Bucharest",
            ShippingPostalCode = "010101",
            ShippingCountry = "Romania",
            OrderLines = [new OrderLineCommand { ProductCode = "GPU001", Quantity = "3" }]
        };

        // Act
        workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        _products["GPU001"].Stock.Value.Should().Be(initialStock - 3);
    }

    [Fact]
    public void Execute_InvalidEmail_ShouldReturnOrderPlacementFailedEvent()
    {
        // Arrange
        var workflow = new PlaceOrderWorkflow();
        var command = new PlaceOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "invalid-email",
            ShippingStreet = "123 Main Street",
            ShippingCity = "Bucharest",
            ShippingPostalCode = "010101",
            ShippingCountry = "Romania",
            OrderLines = [new OrderLineCommand { ProductCode = "GPU001", Quantity = "1" }]
        };

        // Act
        var result = workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPlacementFailedEvent>();
        var failedEvent = (OrderPlacementFailedEvent)result;
        failedEvent.Reasons.Should().Contain(r => r.ToLower().Contains("email"));
    }

    [Fact]
    public void Execute_ProductNotFound_ShouldReturnOrderPlacementFailedEvent()
    {
        // Arrange
        var workflow = new PlaceOrderWorkflow();
        var command = new PlaceOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingStreet = "123 Main Street",
            ShippingCity = "Bucharest",
            ShippingPostalCode = "010101",
            ShippingCountry = "Romania",
            OrderLines = [new OrderLineCommand { ProductCode = "XXX999", Quantity = "1" }]
        };

        // Act
        var result = workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPlacementFailedEvent>();
        var failedEvent = (OrderPlacementFailedEvent)result;
        failedEvent.Reasons.Should().Contain(r => r.Contains("does not exist"));
    }

    [Fact]
    public void Execute_InsufficientStock_ShouldReturnOrderPlacementFailedEvent()
    {
        // Arrange
        var workflow = new PlaceOrderWorkflow();
        var command = new PlaceOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingStreet = "123 Main Street",
            ShippingCity = "Bucharest",
            ShippingPostalCode = "010101",
            ShippingCountry = "Romania",
            OrderLines = [new OrderLineCommand { ProductCode = "GPU001", Quantity = "100" }]
        };

        // Act
        var result = workflow.Execute(
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

    [Fact]
    public void Execute_EmptyOrderLines_ShouldReturnOrderPlacementFailedEvent()
    {
        // Arrange
        var workflow = new PlaceOrderWorkflow();
        var command = new PlaceOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingStreet = "123 Main Street",
            ShippingCity = "Bucharest",
            ShippingPostalCode = "010101",
            ShippingCountry = "Romania",
            OrderLines = []
        };

        // Act
        var result = workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPlacementFailedEvent>();
        var failedEvent = (OrderPlacementFailedEvent)result;
        failedEvent.Reasons.Should().Contain(r => r.Contains("at least one item"));
    }

    [Fact]
    public void Execute_MultipleProducts_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var workflow = new PlaceOrderWorkflow();
        var command = new PlaceOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingStreet = "123 Main Street",
            ShippingCity = "Bucharest",
            ShippingPostalCode = "010101",
            ShippingCountry = "Romania",
            OrderLines =
            [
                new OrderLineCommand { ProductCode = "GPU001", Quantity = "1" }, // 1599.99
                new OrderLineCommand { ProductCode = "CPU002", Quantity = "1" }  // 599.99
            ]
        };

        // Act
        var result = workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPendingPaymentEvent>();
        var pendingEvent = (OrderPendingPaymentEvent)result;
        pendingEvent.OrderLines.Should().HaveCount(2);
        // Total should be approximately 2199.98
        pendingEvent.TotalPrice.Value.Should().BeGreaterThan(2000m);
    }

    [Fact]
    public void Execute_DuplicateProducts_ShouldReturnOrderPlacementFailedEvent()
    {
        // Arrange
        var workflow = new PlaceOrderWorkflow();
        var command = new PlaceOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingStreet = "123 Main Street",
            ShippingCity = "Bucharest",
            ShippingPostalCode = "010101",
            ShippingCountry = "Romania",
            OrderLines =
            [
                new OrderLineCommand { ProductCode = "GPU001", Quantity = "1" },
                new OrderLineCommand { ProductCode = "GPU001", Quantity = "2" }
            ]
        };

        // Act
        var result = workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPlacementFailedEvent>();
        var failedEvent = (OrderPlacementFailedEvent)result;
        failedEvent.Reasons.Should().Contain(r => r.Contains("Duplicate"));
    }

    [Fact]
    public void Execute_SuccessfulOrder_ShouldHaveOrderIdInEvent()
    {
        // Arrange
        var workflow = new PlaceOrderWorkflow();
        var command = new PlaceOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingStreet = "123 Main Street",
            ShippingCity = "Bucharest",
            ShippingPostalCode = "010101",
            ShippingCountry = "Romania",
            OrderLines = [new OrderLineCommand { ProductCode = "GPU001", Quantity = "1" }]
        };

        // Act
        var result = workflow.Execute(
            command,
            CheckProductExists,
            GetProductDetails,
            ReserveStock,
            PersistOrder);

        // Assert
        result.Should().BeOfType<OrderPendingPaymentEvent>();
        var pendingEvent = (OrderPendingPaymentEvent)result;
        pendingEvent.OrderId.Should().NotBeNull();
        pendingEvent.OrderId.Value.Should().NotBe(Guid.Empty);
    }
}
