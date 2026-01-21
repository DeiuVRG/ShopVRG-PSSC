using FluentAssertions;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.ValueObjects;

/// <summary>
/// Unit tests for ProductName value object
/// </summary>
public class ProductNameTests
{
    [Theory]
    [InlineData("RTX 4090")]
    [InlineData("Intel Core i9-13900K")]
    [InlineData("Samsung 980 Pro 2TB NVMe SSD")]
    [InlineData("Corsair Vengeance DDR5 32GB")]
    public void TryCreate_WithValidName_ShouldSucceed(string name)
    {
        // Act
        var result = ProductName.TryCreate(name, out var productName, out var error);

        // Assert
        result.Should().BeTrue();
        productName.Should().NotBeNull();
        productName!.Value.Should().Be(name);
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryCreate_WithEmptyOrNull_ShouldFail(string? name)
    {
        // Act
        var result = ProductName.TryCreate(name, out var productName, out var error);

        // Assert
        result.Should().BeFalse();
        productName.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("AB")] // Too short
    [InlineData("X")]
    public void TryCreate_WithTooShortName_ShouldFail(string name)
    {
        // Act
        var result = ProductName.TryCreate(name, out var productName, out var error);

        // Assert
        result.Should().BeFalse();
        productName.Should().BeNull();
        error.Should().Contain("at least 3 characters");
    }

    [Fact]
    public void TryCreate_WithTooLongName_ShouldFail()
    {
        // Arrange
        var name = new string('A', 201);

        // Act
        var result = ProductName.TryCreate(name, out var productName, out var error);

        // Assert
        result.Should().BeFalse();
        productName.Should().BeNull();
        error.Should().Contain("exceed 200 characters");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        ProductName.TryCreate("RTX 4090", out var name1, out _);
        ProductName.TryCreate("RTX 4090", out var name2, out _);

        // Assert
        name1.Should().Be(name2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        // Arrange
        ProductName.TryCreate("RTX 4090", out var name1, out _);
        ProductName.TryCreate("RTX 4080", out var name2, out _);

        // Assert
        name1.Should().NotBe(name2);
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        // Arrange
        ProductName.TryCreate("RTX 4090", out var name, out _);

        // Assert
        name!.ToString().Should().Be("RTX 4090");
    }
}
