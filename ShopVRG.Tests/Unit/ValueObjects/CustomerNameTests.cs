using FluentAssertions;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.ValueObjects;

/// <summary>
/// Unit tests for CustomerName value object
/// </summary>
public class CustomerNameTests
{
    [Theory]
    [InlineData("John Doe")]
    [InlineData("Ana Maria")]
    [InlineData("O'Brien")]
    [InlineData("Jean-Pierre")]
    [InlineData("Müller")]
    public void TryCreate_WithValidName_ShouldSucceed(string name)
    {
        // Act
        var result = CustomerName.TryCreate(name, out var customerName, out var error);

        // Assert
        result.Should().BeTrue();
        customerName.Should().NotBeNull();
        customerName!.Value.Should().Be(name);
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryCreate_WithEmptyOrNull_ShouldFail(string? name)
    {
        // Act
        var result = CustomerName.TryCreate(name, out var customerName, out var error);

        // Assert
        result.Should().BeFalse();
        customerName.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("A")] // Too short
    public void TryCreate_WithTooShortName_ShouldFail(string name)
    {
        // Act
        var result = CustomerName.TryCreate(name, out var customerName, out var error);

        // Assert
        result.Should().BeFalse();
        customerName.Should().BeNull();
        error.Should().Contain("at least 2 characters");
    }

    [Fact]
    public void TryCreate_WithTooLongName_ShouldFail()
    {
        // Arrange
        var name = new string('A', 101);

        // Act
        var result = CustomerName.TryCreate(name, out var customerName, out var error);

        // Assert
        result.Should().BeFalse();
        customerName.Should().BeNull();
        error.Should().Contain("exceed 100 characters");
    }

    [Theory]
    [InlineData("John123")]
    [InlineData("John@Doe")]
    [InlineData("John#Doe")]
    public void TryCreate_WithInvalidCharacters_ShouldFail(string name)
    {
        // Act
        var result = CustomerName.TryCreate(name, out var customerName, out var error);

        // Assert
        result.Should().BeFalse();
        customerName.Should().BeNull();
        error.Should().Contain("letters, spaces, hyphens, and apostrophes");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        CustomerName.TryCreate("John Doe", out var name1, out _);
        CustomerName.TryCreate("John Doe", out var name2, out _);

        // Assert
        name1.Should().Be(name2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        // Arrange
        CustomerName.TryCreate("John Doe", out var name1, out _);
        CustomerName.TryCreate("Jane Doe", out var name2, out _);

        // Assert
        name1.Should().NotBe(name2);
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        // Arrange
        CustomerName.TryCreate("John Doe", out var name, out _);

        // Assert
        name!.ToString().Should().Be("John Doe");
    }
}
