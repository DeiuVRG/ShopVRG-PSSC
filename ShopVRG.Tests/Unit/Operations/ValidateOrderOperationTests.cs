using FluentAssertions;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;
using System.Reflection;

namespace ShopVRG.Tests.Unit.Operations;

/// <summary>
/// Unit tests for ValidateOrderOperation transform
/// Tests the transformation from UnvalidatedOrder to ValidatedOrder or InvalidOrder
/// </summary>
public class ValidateOrderOperationTests
{
    private readonly object _operation;
    private readonly MethodInfo _transformMethod;

    public ValidateOrderOperationTests()
    {
        // Get the internal ValidateOrderOperation class via reflection
        var operationType = typeof(IOrder).Assembly
            .GetType("ShopVRG.Domain.Operations.ValidateOrderOperation")!;
        _operation = Activator.CreateInstance(operationType)!;
        _transformMethod = operationType.GetMethod("Transform", BindingFlags.Instance | BindingFlags.NonPublic)!;
    }

    private IOrder Transform(IOrder order)
    {
        return (IOrder)_transformMethod.Invoke(_operation, [order])!;
    }

    [Fact]
    public void Transform_ValidOrder_ShouldReturnValidatedOrder()
    {
        // Arrange
        var unvalidatedOrder = new UnvalidatedOrder(
            "John Doe",
            "john@example.com",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            [new UnvalidatedOrderLine("GPU001", "2")]);

        // Act
        var result = Transform(unvalidatedOrder);

        // Assert
        result.Should().BeOfType<ValidatedOrder>();
        var validatedOrder = (ValidatedOrder)result;
        validatedOrder.CustomerName.Value.Should().Be("John Doe");
        validatedOrder.CustomerEmail.Value.Should().Be("john@example.com");
        validatedOrder.OrderLines.Should().HaveCount(1);
    }

    [Fact]
    public void Transform_EmptyCustomerName_ShouldReturnInvalidOrder()
    {
        // Arrange
        var unvalidatedOrder = new UnvalidatedOrder(
            "",
            "john@example.com",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            [new UnvalidatedOrderLine("GPU001", "2")]);

        // Act
        var result = Transform(unvalidatedOrder);

        // Assert
        result.Should().BeOfType<InvalidOrder>();
        var invalidOrder = (InvalidOrder)result;
        invalidOrder.Reasons.Should().Contain(r => r.Contains("name"));
    }

    [Fact]
    public void Transform_InvalidEmail_ShouldReturnInvalidOrder()
    {
        // Arrange
        var unvalidatedOrder = new UnvalidatedOrder(
            "John Doe",
            "invalid-email",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            [new UnvalidatedOrderLine("GPU001", "2")]);

        // Act
        var result = Transform(unvalidatedOrder);

        // Assert
        result.Should().BeOfType<InvalidOrder>();
        var invalidOrder = (InvalidOrder)result;
        invalidOrder.Reasons.Should().Contain(r => r.ToLower().Contains("email"));
    }

    [Fact]
    public void Transform_EmptyOrderLines_ShouldReturnInvalidOrder()
    {
        // Arrange
        var unvalidatedOrder = new UnvalidatedOrder(
            "John Doe",
            "john@example.com",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            []);

        // Act
        var result = Transform(unvalidatedOrder);

        // Assert
        result.Should().BeOfType<InvalidOrder>();
        var invalidOrder = (InvalidOrder)result;
        invalidOrder.Reasons.Should().Contain(r => r.Contains("at least one item"));
    }

    [Fact]
    public void Transform_InvalidProductCode_ShouldReturnInvalidOrder()
    {
        // Arrange
        var unvalidatedOrder = new UnvalidatedOrder(
            "John Doe",
            "john@example.com",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            [new UnvalidatedOrderLine("X", "2")]); // Invalid - too short

        // Act
        var result = Transform(unvalidatedOrder);

        // Assert
        result.Should().BeOfType<InvalidOrder>();
    }

    [Fact]
    public void Transform_InvalidQuantity_ShouldReturnInvalidOrder()
    {
        // Arrange
        var unvalidatedOrder = new UnvalidatedOrder(
            "John Doe",
            "john@example.com",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            [new UnvalidatedOrderLine("GPU001", "0")]); // Invalid - zero quantity

        // Act
        var result = Transform(unvalidatedOrder);

        // Assert
        result.Should().BeOfType<InvalidOrder>();
    }

    [Fact]
    public void Transform_DuplicateProducts_ShouldReturnInvalidOrder()
    {
        // Arrange
        var unvalidatedOrder = new UnvalidatedOrder(
            "John Doe",
            "john@example.com",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            [
                new UnvalidatedOrderLine("GPU001", "2"),
                new UnvalidatedOrderLine("GPU001", "3") // Duplicate
            ]);

        // Act
        var result = Transform(unvalidatedOrder);

        // Assert
        result.Should().BeOfType<InvalidOrder>();
        var invalidOrder = (InvalidOrder)result;
        invalidOrder.Reasons.Should().Contain(r => r.Contains("Duplicate"));
    }

    [Fact]
    public void Transform_MultipleOrderLines_ShouldValidateAll()
    {
        // Arrange
        var unvalidatedOrder = new UnvalidatedOrder(
            "John Doe",
            "john@example.com",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            [
                new UnvalidatedOrderLine("GPU001", "2"),
                new UnvalidatedOrderLine("CPU002", "1"),
                new UnvalidatedOrderLine("RAM003", "4")
            ]);

        // Act
        var result = Transform(unvalidatedOrder);

        // Assert
        result.Should().BeOfType<ValidatedOrder>();
        var validatedOrder = (ValidatedOrder)result;
        validatedOrder.OrderLines.Should().HaveCount(3);
    }

    [Fact]
    public void Transform_ValidatedOrder_ShouldHaveOrderId()
    {
        // Arrange
        var unvalidatedOrder = new UnvalidatedOrder(
            "John Doe",
            "john@example.com",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            [new UnvalidatedOrderLine("GPU001", "2")]);

        // Act
        var result = Transform(unvalidatedOrder);

        // Assert
        result.Should().BeOfType<ValidatedOrder>();
        var validatedOrder = (ValidatedOrder)result;
        validatedOrder.OrderId.Should().NotBeNull();
        validatedOrder.OrderId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Transform_ValidatedOrder_ShouldHaveCreatedAtTimestamp()
    {
        // Arrange
        var beforeTest = DateTime.UtcNow;
        var unvalidatedOrder = new UnvalidatedOrder(
            "John Doe",
            "john@example.com",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            [new UnvalidatedOrderLine("GPU001", "2")]);

        // Act
        var result = Transform(unvalidatedOrder);

        // Assert
        result.Should().BeOfType<ValidatedOrder>();
        var validatedOrder = (ValidatedOrder)result;
        validatedOrder.CreatedAt.Should().BeOnOrAfter(beforeTest);
    }
}
