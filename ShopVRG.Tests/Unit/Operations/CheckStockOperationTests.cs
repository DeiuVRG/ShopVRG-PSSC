using FluentAssertions;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;
using System.Reflection;

namespace ShopVRG.Tests.Unit.Operations;

/// <summary>
/// Unit tests for CheckStockOperation transform
/// Tests the transformation from ValidatedOrder to StockCheckedOrder or InvalidOrder
/// Uses fake product data for testing without touching real database
/// </summary>
public class CheckStockOperationTests
{
    private readonly Dictionary<string, (ProductName Name, Price Price, StockQuantity Stock)> _products;

    public CheckStockOperationTests()
    {
        // Setup fake product catalog
        ProductName.TryCreate("RTX 4090 Gaming GPU", out var gpuName, out _);
        ProductName.TryCreate("Intel Core i9-13900K", out var cpuName, out _);
        ProductName.TryCreate("Corsair DDR5 32GB", out var ramName, out _);
        ProductName.TryCreate("Samsung 980 Pro 2TB", out var ssdName, out _);

        _products = new Dictionary<string, (ProductName, Price, StockQuantity)>
        {
            ["GPU001"] = (gpuName!, Price.FromDecimal(1599.99m), StockQuantity.FromInt(10)),
            ["CPU002"] = (cpuName!, Price.FromDecimal(599.99m), StockQuantity.FromInt(25)),
            ["RAM003"] = (ramName!, Price.FromDecimal(189.99m), StockQuantity.FromInt(50)),
            ["SSD004"] = (ssdName!, Price.FromDecimal(249.99m), StockQuantity.FromInt(0)) // Out of stock
        };
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

        // Simulate stock reservation
        var newStock = details.Stock.Decrease(quantity);
        _products[code.Value] = (details.Name, details.Price, newStock);
        return true;
    }

    private IOrder Transform(IOrder order)
    {
        // Get the internal CheckStockOperation class via reflection
        var operationType = typeof(IOrder).Assembly
            .GetType("ShopVRG.Domain.Operations.CheckStockOperation")!;

        var operation = Activator.CreateInstance(
            operationType,
            new Func<ProductCode, bool>(CheckProductExists),
            new Func<ProductCode, (ProductName, Price, StockQuantity)?>(GetProductDetails),
            new Func<ProductCode, Quantity, bool>(ReserveStock))!;

        var transformMethod = operationType.GetMethod("Transform", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (IOrder)transformMethod.Invoke(operation, [order])!;
    }

    private ValidatedOrder CreateValidatedOrder(params (string code, int qty)[] lines)
    {
        // Create via reflection since constructor is internal
        var orderId = OrderId.NewOrderId();
        CustomerName.TryCreate("John Doe", out var customerName, out _);
        CustomerEmail.TryCreate("john@example.com", out var customerEmail, out _);
        ShippingAddress.TryCreate("123 Main St", "Bucharest", "010101", "Romania", out var address, out _);

        var validatedLines = lines.Select(l =>
        {
            ProductCode.TryCreate(l.code, out var code, out _);
            Quantity.TryCreate(l.qty, out var qty, out _);
            return new ValidatedOrderLine(code!, qty!);
        }).ToList();

        // Use reflection to create ValidatedOrder
        var validatedOrderType = typeof(ValidatedOrder);
        var constructor = validatedOrderType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
        return (ValidatedOrder)constructor.Invoke([
            orderId, customerName, customerEmail, address, validatedLines, DateTime.UtcNow
        ]);
    }

    [Fact]
    public void Transform_ValidOrderWithStock_ShouldReturnStockCheckedOrder()
    {
        // Arrange
        var validatedOrder = CreateValidatedOrder(("GPU001", 2));

        // Act
        var result = Transform(validatedOrder);

        // Assert
        result.Should().BeOfType<StockCheckedOrder>();
        var stockChecked = (StockCheckedOrder)result;
        stockChecked.OrderLines.Should().HaveCount(1);
    }

    [Fact]
    public void Transform_ProductNotFound_ShouldReturnInvalidOrder()
    {
        // Arrange
        var validatedOrder = CreateValidatedOrder(("XXX999", 1));

        // Act
        var result = Transform(validatedOrder);

        // Assert
        result.Should().BeOfType<InvalidOrder>();
        var invalid = (InvalidOrder)result;
        invalid.Reasons.Should().Contain(r => r.Contains("does not exist"));
    }

    [Fact]
    public void Transform_InsufficientStock_ShouldReturnInvalidOrder()
    {
        // Arrange - SSD004 has 0 stock
        var validatedOrder = CreateValidatedOrder(("SSD004", 5));

        // Act
        var result = Transform(validatedOrder);

        // Assert
        result.Should().BeOfType<InvalidOrder>();
        var invalid = (InvalidOrder)result;
        invalid.Reasons.Should().Contain(r => r.Contains("Insufficient stock"));
    }

    [Fact]
    public void Transform_MultipleProducts_ShouldCalculatePricesCorrectly()
    {
        // Arrange
        var validatedOrder = CreateValidatedOrder(
            ("GPU001", 1),  // 1599.99
            ("CPU002", 2)); // 599.99 * 2 = 1199.98

        // Act
        var result = Transform(validatedOrder);

        // Assert
        result.Should().BeOfType<StockCheckedOrder>();
        var stockChecked = (StockCheckedOrder)result;

        var gpuLine = stockChecked.OrderLines.First(l => l.ProductCode.Value == "GPU001");
        gpuLine.UnitPrice.Value.Should().Be(1599.99m);
        gpuLine.LineTotal.Value.Should().Be(1599.99m);

        var cpuLine = stockChecked.OrderLines.First(l => l.ProductCode.Value == "CPU002");
        cpuLine.UnitPrice.Value.Should().Be(599.99m);
        cpuLine.LineTotal.Value.Should().Be(1199.98m);
    }

    [Fact]
    public void Transform_StockCheckedOrder_ShouldHaveProductNames()
    {
        // Arrange
        var validatedOrder = CreateValidatedOrder(("GPU001", 1));

        // Act
        var result = Transform(validatedOrder);

        // Assert
        result.Should().BeOfType<StockCheckedOrder>();
        var stockChecked = (StockCheckedOrder)result;
        var line = stockChecked.OrderLines.First();
        line.ProductName.Value.Should().Be("RTX 4090 Gaming GPU");
    }

    [Fact]
    public void Transform_ShouldPreserveOrderMetadata()
    {
        // Arrange
        var validatedOrder = CreateValidatedOrder(("GPU001", 1));

        // Act
        var result = Transform(validatedOrder);

        // Assert
        result.Should().BeOfType<StockCheckedOrder>();
        var stockChecked = (StockCheckedOrder)result;
        stockChecked.OrderId.Should().Be(validatedOrder.OrderId);
        stockChecked.CustomerName.Should().Be(validatedOrder.CustomerName);
        stockChecked.CustomerEmail.Should().Be(validatedOrder.CustomerEmail);
        stockChecked.ShippingAddress.Should().Be(validatedOrder.ShippingAddress);
        stockChecked.CreatedAt.Should().Be(validatedOrder.CreatedAt);
    }

    [Fact]
    public void Transform_RequestingMoreThanAvailable_ShouldReturnInvalidOrder()
    {
        // Arrange - GPU001 has 10 in stock
        var validatedOrder = CreateValidatedOrder(("GPU001", 15));

        // Act
        var result = Transform(validatedOrder);

        // Assert
        result.Should().BeOfType<InvalidOrder>();
        var invalid = (InvalidOrder)result;
        invalid.Reasons.Should().Contain(r => r.Contains("Insufficient stock"));
    }
}
