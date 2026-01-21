using FluentAssertions;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.ValueObjects;

/// <summary>
/// Unit tests for Quantity value object
/// </summary>
public class QuantityTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(1000)]
    public void TryCreate_WithValidQuantity_ShouldSucceed(int value)
    {
        // Act
        var result = Quantity.TryCreate(value, out var quantity, out var error);

        // Assert
        result.Should().BeTrue();
        quantity.Should().NotBeNull();
        quantity!.Value.Should().Be(value);
        error.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void TryCreate_WithZeroOrNegative_ShouldFail(int value)
    {
        // Act
        var result = Quantity.TryCreate(value, out var quantity, out var error);

        // Assert
        result.Should().BeFalse();
        quantity.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryCreate_ExceedingMaximum_ShouldFail()
    {
        // Act
        var result = Quantity.TryCreate(1001, out var quantity, out var error);

        // Assert
        result.Should().BeFalse();
        quantity.Should().BeNull();
        error.Should().Contain("exceed");
    }

    [Fact]
    public void FromInt_WithValidValue_ShouldReturnQuantity()
    {
        // Act
        var quantity = Quantity.FromInt(5);

        // Assert
        quantity.Should().NotBeNull();
        quantity.Value.Should().Be(5);
    }

    [Fact]
    public void FromInt_WithInvalidValue_ShouldThrow()
    {
        // Act
        var act = () => Quantity.FromInt(0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryCreate_WithValidString_ShouldSucceed()
    {
        // Act
        var result = Quantity.TryCreate("5", out var quantity, out var error);

        // Assert
        result.Should().BeTrue();
        quantity.Should().NotBeNull();
        quantity!.Value.Should().Be(5);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("   ")]
    public void TryCreate_WithInvalidString_ShouldFail(string value)
    {
        // Act
        var result = Quantity.TryCreate(value, out var quantity, out var error);

        // Assert
        result.Should().BeFalse();
        quantity.Should().BeNull();
    }

    [Theory]
    [InlineData("-5")]
    [InlineData("0")]
    public void TryCreate_WithZeroOrNegativeString_ShouldFail(string value)
    {
        // Act
        var result = Quantity.TryCreate(value, out var quantity, out var error);

        // Assert
        result.Should().BeFalse();
        quantity.Should().BeNull();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        var qty1 = Quantity.FromInt(10);
        var qty2 = Quantity.FromInt(10);

        // Assert
        qty1.Should().Be(qty2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        // Arrange
        var qty1 = Quantity.FromInt(10);
        var qty2 = Quantity.FromInt(20);

        // Assert
        qty1.Should().NotBe(qty2);
    }

    [Fact]
    public void Add_TwoQuantities_ShouldReturnSum()
    {
        // Arrange
        var qty1 = Quantity.FromInt(5);
        var qty2 = Quantity.FromInt(3);

        // Act
        var result = qty1.Add(qty2);

        // Assert
        result.Value.Should().Be(8);
    }

    [Fact]
    public void Subtract_TwoQuantities_ShouldReturnDifference()
    {
        // Arrange
        var qty1 = Quantity.FromInt(10);
        var qty2 = Quantity.FromInt(3);

        // Act
        var result = qty1.Subtract(qty2);

        // Assert
        result.Value.Should().Be(7);
    }
}
