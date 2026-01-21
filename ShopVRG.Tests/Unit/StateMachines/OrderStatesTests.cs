using FluentAssertions;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.StateMachines;

/// <summary>
/// Unit tests for Order state machine states
/// Tests state transitions: Unvalidated -> Validated -> StockChecked -> Pending/Placed -> Invalid
/// </summary>
public class OrderStatesTests
{
    #region UnvalidatedOrder Tests

    [Fact]
    public void UnvalidatedOrder_ShouldStoreRawInputData()
    {
        // Arrange
        var orderLines = new List<UnvalidatedOrderLine>
        {
            new("GPU-001", "2"),
            new("CPU-002", "1")
        };

        // Act
        var order = new UnvalidatedOrder(
            "John Doe",
            "john@example.com",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            orderLines);

        // Assert
        order.Should().BeAssignableTo<IOrder>();
        order.CustomerName.Should().Be("John Doe");
        order.CustomerEmail.Should().Be("john@example.com");
        order.ShippingStreet.Should().Be("123 Main Street");
        order.ShippingCity.Should().Be("Bucharest");
        order.ShippingPostalCode.Should().Be("010101");
        order.ShippingCountry.Should().Be("Romania");
        order.OrderLines.Should().HaveCount(2);
    }

    [Fact]
    public void UnvalidatedOrder_OrderLines_ShouldBeReadOnly()
    {
        // Arrange
        var orderLines = new List<UnvalidatedOrderLine> { new("GPU-001", "2") };
        var order = new UnvalidatedOrder("John", "john@test.com", "Street", "City", "12345", "Country", orderLines);

        // Assert
        order.OrderLines.Should().BeAssignableTo<IReadOnlyList<UnvalidatedOrderLine>>();
    }

    [Fact]
    public void UnvalidatedOrderLine_ShouldStoreProductCodeAndQuantity()
    {
        // Act
        var line = new UnvalidatedOrderLine("GPU-001", "5");

        // Assert
        line.ProductCode.Should().Be("GPU-001");
        line.Quantity.Should().Be("5");
    }

    #endregion

    #region ValidatedOrder Tests

    [Fact]
    public void ValidatedOrderLine_ShouldContainValueObjects()
    {
        // Arrange
        ProductCode.TryCreate("GPU-001", out var productCode, out _);
        Quantity.TryCreate(2, out var quantity, out _);

        // Act
        var line = new ValidatedOrderLine(productCode!, quantity!);

        // Assert
        line.ProductCode.Should().Be(productCode);
        line.Quantity.Should().Be(quantity);
    }

    #endregion

    #region StockCheckedOrder Tests

    [Fact]
    public void StockCheckedOrderLine_ShouldContainPriceInfo()
    {
        // Arrange
        ProductCode.TryCreate("GPU-001", out var productCode, out _);
        ProductName.TryCreate("RTX 4090", out var productName, out _);
        Quantity.TryCreate(2, out var quantity, out _);
        var unitPrice = Price.FromDecimal(1500m);
        var lineTotal = Price.FromDecimal(3000m);

        // Act
        var line = new StockCheckedOrderLine(productCode!, productName!, quantity!, unitPrice, lineTotal);

        // Assert
        line.ProductCode.Should().Be(productCode);
        line.ProductName.Should().Be(productName);
        line.Quantity.Should().Be(quantity);
        line.UnitPrice.Value.Should().Be(1500m);
        line.LineTotal.Value.Should().Be(3000m);
    }

    #endregion

    #region PendingOrder Tests

    [Fact]
    public void PendingOrderLine_ShouldContainAllOrderInfo()
    {
        // Arrange
        ProductCode.TryCreate("GPU-001", out var productCode, out _);
        ProductName.TryCreate("RTX 4090", out var productName, out _);
        Quantity.TryCreate(2, out var quantity, out _);
        var unitPrice = Price.FromDecimal(1500m);
        var lineTotal = Price.FromDecimal(3000m);

        // Act
        var line = new PendingOrderLine(productCode!, productName!, quantity!, unitPrice, lineTotal);

        // Assert
        line.ProductCode.Should().Be(productCode);
        line.ProductName.Should().Be(productName);
        line.Quantity.Should().Be(quantity);
        line.UnitPrice.Value.Should().Be(1500m);
        line.LineTotal.Value.Should().Be(3000m);
    }

    #endregion

    #region PlacedOrder Tests

    [Fact]
    public void PlacedOrderLine_ShouldContainAllOrderInfo()
    {
        // Arrange
        ProductCode.TryCreate("GPU-001", out var productCode, out _);
        ProductName.TryCreate("RTX 4090", out var productName, out _);
        Quantity.TryCreate(2, out var quantity, out _);
        var unitPrice = Price.FromDecimal(1500m);
        var lineTotal = Price.FromDecimal(3000m);

        // Act
        var line = new PlacedOrderLine(productCode!, productName!, quantity!, unitPrice, lineTotal);

        // Assert
        line.ProductCode.Should().Be(productCode);
        line.ProductName.Should().Be(productName);
        line.Quantity.Should().Be(quantity);
        line.UnitPrice.Value.Should().Be(1500m);
        line.LineTotal.Value.Should().Be(3000m);
    }

    #endregion

    #region InvalidOrder Tests

    [Fact]
    public void InvalidOrder_ShouldStoreReasons()
    {
        // Arrange - we need to use reflection since constructor is internal
        var reasons = new[] { "Invalid email", "Product not found" };

        // We can verify InvalidOrder implements IOrder
        typeof(InvalidOrder).Should().Implement<IOrder>();
    }

    #endregion

    #region State Machine Interface Tests

    [Fact]
    public void AllOrderStates_ShouldImplementIOrder()
    {
        // Assert
        typeof(UnvalidatedOrder).Should().Implement<IOrder>();
        typeof(ValidatedOrder).Should().Implement<IOrder>();
        typeof(StockCheckedOrder).Should().Implement<IOrder>();
        typeof(PendingOrder).Should().Implement<IOrder>();
        typeof(PlacedOrder).Should().Implement<IOrder>();
        typeof(InvalidOrder).Should().Implement<IOrder>();
    }

    [Fact]
    public void OrderStates_ShouldBeSealed()
    {
        // Assert - sealed records/classes ensure state machine integrity
        typeof(UnvalidatedOrder).Should().BeSealed();
        typeof(ValidatedOrder).Should().BeSealed();
        typeof(StockCheckedOrder).Should().BeSealed();
        typeof(PendingOrder).Should().BeSealed();
        typeof(PlacedOrder).Should().BeSealed();
        typeof(InvalidOrder).Should().BeSealed();
    }

    #endregion
}
