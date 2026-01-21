using FluentAssertions;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.ValueObjects;

/// <summary>
/// Unit tests for Price value object
/// </summary>
public class PriceTests
{
    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(99.99)]
    [InlineData(1599.99)]
    [InlineData(10000.00)]
    public void TryCreate_WithValidAmount_ShouldSucceed(decimal amount)
    {
        // Act
        var result = Price.TryCreate(amount, out var price, out var error);

        // Assert
        result.Should().BeTrue();
        price.Should().NotBeNull();
        price!.Value.Should().Be(amount);
        error.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99.99)]
    public void TryCreate_WithZeroOrNegative_ShouldFail(decimal amount)
    {
        // Act
        var result = Price.TryCreate(amount, out var price, out var error);

        // Assert
        result.Should().BeFalse();
        price.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromDecimal_WithValidAmount_ShouldReturnPrice()
    {
        // Act
        var price = Price.FromDecimal(100.00m);

        // Assert
        price.Should().NotBeNull();
        price.Value.Should().Be(100.00m);
    }

    [Fact]
    public void FromDecimal_WithInvalidAmount_ShouldThrow()
    {
        // Act
        var act = () => Price.FromDecimal(0m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_TwoPrices_ShouldReturnSum()
    {
        // Arrange
        var price1 = Price.FromDecimal(100.00m);
        var price2 = Price.FromDecimal(50.50m);

        // Act
        var result = price1.Add(price2);

        // Assert
        result.Value.Should().Be(150.50m);
    }

    [Fact]
    public void Multiply_ByQuantity_ShouldReturnProduct()
    {
        // Arrange
        var price = Price.FromDecimal(25.00m);

        // Act
        var result = price.Multiply(4);

        // Assert
        result.Value.Should().Be(100.00m);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        var price1 = Price.FromDecimal(99.99m);
        var price2 = Price.FromDecimal(99.99m);

        // Assert
        price1.Should().Be(price2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        // Arrange
        var price1 = Price.FromDecimal(99.99m);
        var price2 = Price.FromDecimal(100.00m);

        // Assert
        price1.Should().NotBe(price2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedValue()
    {
        // Arrange
        var price = Price.FromDecimal(1599.99m);

        // Assert
        price.ToString().Should().Be("1599.99");
    }

    [Fact]
    public void CompareTo_ShouldCompareCorrectly()
    {
        // Arrange
        var small = Price.FromDecimal(50.00m);
        var large = Price.FromDecimal(100.00m);

        // Assert
        (small < large).Should().BeTrue();
        (large > small).Should().BeTrue();
    }
}
