using FluentAssertions;
using ShopVRG.Tests.Fakes;

namespace ShopVRG.Tests.Integration;

/// <summary>
/// Integration tests for fake repositories
/// Verifies in-memory repository behavior matches expected patterns
/// </summary>
public class RepositoryIntegrationTests
{
    #region FakeProductRepository Tests

    [Fact]
    public void FakeProductRepository_ShouldHaveInitialSeedData()
    {
        // Arrange & Act
        var repository = new FakeProductRepository();
        var products = repository.GetAll();

        // Assert
        products.Should().NotBeEmpty();
        products.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public void FakeProductRepository_GetByCode_ShouldReturnProduct()
    {
        // Arrange
        var repository = new FakeProductRepository();

        // Act
        var product = repository.GetByCode("GPU001");

        // Assert
        product.Should().NotBeNull();
        product!.Code.Should().Be("GPU001");
    }

    [Fact]
    public void FakeProductRepository_GetByCode_NonExistent_ShouldReturnNull()
    {
        // Arrange
        var repository = new FakeProductRepository();

        // Act
        var product = repository.GetByCode("NONEXISTENT999");

        // Assert
        product.Should().BeNull();
    }

    [Fact]
    public void FakeProductRepository_Exists_ShouldReturnCorrectResult()
    {
        // Arrange
        var repository = new FakeProductRepository();

        // Assert
        repository.Exists("GPU001").Should().BeTrue();
        repository.Exists("NONEXISTENT999").Should().BeFalse();
    }

    [Fact]
    public void FakeProductRepository_ReserveStock_ShouldDecreaseStock()
    {
        // Arrange
        var repository = new FakeProductRepository();
        var initialStock = repository.GetByCode("GPU001")!.Stock;

        // Act
        var result = repository.ReserveStock("GPU001", 3);

        // Assert
        result.Should().BeTrue();
        repository.GetByCode("GPU001")!.Stock.Should().Be(initialStock - 3);
    }

    [Fact]
    public void FakeProductRepository_ReserveStock_InsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var repository = new FakeProductRepository();
        var initialStock = repository.GetByCode("GPU001")!.Stock;

        // Act
        var result = repository.ReserveStock("GPU001", initialStock + 100);

        // Assert
        result.Should().BeFalse();
        // Stock should remain unchanged
        repository.GetByCode("GPU001")!.Stock.Should().Be(initialStock);
    }

    [Fact]
    public void FakeProductRepository_ReleaseStock_ShouldIncreaseStock()
    {
        // Arrange
        var repository = new FakeProductRepository();
        var initialStock = repository.GetByCode("GPU001")!.Stock;
        repository.ReserveStock("GPU001", 5);

        // Act
        repository.ReleaseStock("GPU001", 3);

        // Assert
        repository.GetByCode("GPU001")!.Stock.Should().Be(initialStock - 2);
    }

    #endregion

    #region FakeOrderRepository Tests

    [Fact]
    public void FakeOrderRepository_Add_ShouldStoreOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var orderId = Guid.NewGuid();

        // Act
        repository.Add(orderId, new { CreatedAt = DateTime.UtcNow });

        // Assert
        repository.Exists(orderId).Should().BeTrue();
    }

    [Fact]
    public void FakeOrderRepository_GetById_ShouldReturnOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var orderId = Guid.NewGuid();
        repository.Add(orderId, new { CreatedAt = DateTime.UtcNow });

        // Act
        var result = repository.GetById(orderId);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void FakeOrderRepository_GetById_NonExistent_ShouldReturnNull()
    {
        // Arrange
        var repository = new FakeOrderRepository();

        // Act
        var result = repository.GetById(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FakeOrderRepository_GetAll_ShouldReturnAllOrders()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        repository.Add(Guid.NewGuid(), new { CreatedAt = DateTime.UtcNow });
        repository.Add(Guid.NewGuid(), new { CreatedAt = DateTime.UtcNow });
        repository.Add(Guid.NewGuid(), new { CreatedAt = DateTime.UtcNow });

        // Act
        var result = repository.GetAll();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void FakeOrderRepository_Clear_ShouldRemoveAllOrders()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        repository.Add(Guid.NewGuid(), new { CreatedAt = DateTime.UtcNow });
        repository.Add(Guid.NewGuid(), new { CreatedAt = DateTime.UtcNow });

        // Act
        repository.Clear();

        // Assert
        repository.GetAll().Should().BeEmpty();
    }

    #endregion

    #region FakePaymentRepository Tests

    [Fact]
    public void FakePaymentRepository_Add_ShouldStorePayment()
    {
        // Arrange
        var repository = new FakePaymentRepository();
        var paymentId = Guid.NewGuid();

        // Act
        repository.Add(paymentId, Guid.NewGuid(), 100.00m, "TXN-123");

        // Assert
        repository.Exists(paymentId).Should().BeTrue();
    }

    [Fact]
    public void FakePaymentRepository_GetById_ShouldReturnPayment()
    {
        // Arrange
        var repository = new FakePaymentRepository();
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        repository.Add(paymentId, orderId, 150.00m, "TXN-456");

        // Act
        var result = repository.GetById(paymentId);

        // Assert
        result.Should().NotBeNull();
        result!.PaymentId.Should().Be(paymentId.ToString());
        result.OrderId.Should().Be(orderId.ToString());
        result.Amount.Value.Should().Be(150.00m);
        result.TransactionReference.Should().Be("TXN-456");
    }

    [Fact]
    public void FakePaymentRepository_GetByOrderId_ShouldReturnPayment()
    {
        // Arrange
        var repository = new FakePaymentRepository();
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        repository.Add(paymentId, orderId, 200.00m, "TXN-789");

        // Act
        var result = repository.GetByOrderId(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(orderId.ToString());
    }

    #endregion
}
