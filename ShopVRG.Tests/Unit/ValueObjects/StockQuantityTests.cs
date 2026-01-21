using FluentAssertions;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.ValueObjects;

/// <summary>
/// Unit tests for StockQuantity value object
/// </summary>
public class StockQuantityTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(100000)]
    public void TryCreate_WithValidQuantity_ShouldSucceed(int value)
    {
        // Act
        var result = StockQuantity.TryCreate(value, out var quantity, out var error);

        // Assert
        result.Should().BeTrue();
        quantity.Should().NotBeNull();
        quantity!.Value.Should().Be(value);
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("0")]
    [InlineData("100")]
    [InlineData("99999")]
    public void TryCreate_WithValidStringQuantity_ShouldSucceed(string value)
    {
        // Act
        var result = StockQuantity.TryCreate(value, out var quantity, out var error);

        // Assert
        result.Should().BeTrue();
        quantity.Should().NotBeNull();
        error.Should().BeNull();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void TryCreate_WithNegativeQuantity_ShouldFail(int value)
    {
        // Act
        var result = StockQuantity.TryCreate(value, out var quantity, out var error);

        // Assert
        result.Should().BeFalse();
        quantity.Should().BeNull();
        error.Should().Contain("cannot be negative");
    }

    [Fact]
    public void TryCreate_WithQuantityExceedingMax_ShouldFail()
    {
        // Act
        var result = StockQuantity.TryCreate(100001, out var quantity, out var error);

        // Assert
        result.Should().BeFalse();
        quantity.Should().BeNull();
        error.Should().Contain("must not exceed 100000");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryCreate_WithEmptyOrNullString_ShouldFail(string? value)
    {
        // Act
        var result = StockQuantity.TryCreate(value, out var quantity, out var error);

        // Assert
        result.Should().BeFalse();
        quantity.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12.5")]
    [InlineData("invalid")]
    public void TryCreate_WithInvalidString_ShouldFail(string value)
    {
        // Act
        var result = StockQuantity.TryCreate(value, out var quantity, out var error);

        // Assert
        result.Should().BeFalse();
        quantity.Should().BeNull();
        error.Should().Contain("valid integer");
    }

    [Fact]
    public void FromInt_WithValidValue_ShouldSucceed()
    {
        // Act
        var quantity = StockQuantity.FromInt(50);

        // Assert
        quantity.Should().NotBeNull();
        quantity.Value.Should().Be(50);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100001)]
    public void FromInt_WithInvalidValue_ShouldThrow(int value)
    {
        // Act
        var act = () => StockQuantity.FromInt(value);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Zero_ShouldReturnZeroQuantity()
    {
        // Act
        var zero = StockQuantity.Zero;

        // Assert
        zero.Value.Should().Be(0);
    }

    [Fact]
    public void IsAvailable_WithPositiveValue_ShouldBeTrue()
    {
        // Arrange
        var quantity = StockQuantity.FromInt(10);

        // Assert
        quantity.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_WithZeroValue_ShouldBeFalse()
    {
        // Assert
        StockQuantity.Zero.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void HasEnoughStock_WithSufficientStock_ShouldBeTrue()
    {
        // Arrange
        var stock = StockQuantity.FromInt(10);
        Quantity.TryCreate(5, out var requested, out _);

        // Assert
        stock.HasEnoughStock(requested!).Should().BeTrue();
    }

    [Fact]
    public void HasEnoughStock_WithInsufficientStock_ShouldBeFalse()
    {
        // Arrange
        var stock = StockQuantity.FromInt(5);
        Quantity.TryCreate(10, out var requested, out _);

        // Assert
        stock.HasEnoughStock(requested!).Should().BeFalse();
    }

    [Fact]
    public void Decrease_WithValidQuantity_ShouldSucceed()
    {
        // Arrange
        var stock = StockQuantity.FromInt(10);
        Quantity.TryCreate(5, out var decrease, out _);

        // Act
        var result = stock.Decrease(decrease!);

        // Assert
        result.Value.Should().Be(5);
    }

    [Fact]
    public void Decrease_WithQuantityExceedingStock_ShouldThrow()
    {
        // Arrange
        var stock = StockQuantity.FromInt(5);
        Quantity.TryCreate(10, out var decrease, out _);

        // Act
        var act = () => stock.Decrease(decrease!);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*Not enough stock*");
    }

    [Fact]
    public void Increase_WithValidQuantity_ShouldSucceed()
    {
        // Arrange
        var stock = StockQuantity.FromInt(10);
        Quantity.TryCreate(5, out var increase, out _);

        // Act
        var result = stock.Increase(increase!);

        // Assert
        result.Value.Should().Be(15);
    }

    [Fact]
    public void Increase_ExceedingMaximum_ShouldThrow()
    {
        // Arrange
        var stock = StockQuantity.FromInt(99999);
        Quantity.TryCreate(10, out var increase, out _);

        // Act
        var act = () => stock.Increase(increase!);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*cannot exceed 100000*");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        var qty1 = StockQuantity.FromInt(10);
        var qty2 = StockQuantity.FromInt(10);

        // Assert
        qty1.Should().Be(qty2);
    }

    [Fact]
    public void CompareTo_ShouldCompareCorrectly()
    {
        // Arrange
        var small = StockQuantity.FromInt(5);
        var large = StockQuantity.FromInt(10);

        // Assert
        (small < large).Should().BeTrue();
        (large > small).Should().BeTrue();
    }
}
