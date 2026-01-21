using FluentAssertions;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.ValueObjects;

/// <summary>
/// Unit tests for ProductCode value object
/// </summary>
public class ProductCodeTests
{
    [Theory]
    [InlineData("GPU001")]
    [InlineData("CPU123")]
    [InlineData("RAM9999")]
    [InlineData("SSD12345")]
    [InlineData("MB123456")]
    public void TryCreate_WithValidCode_ShouldSucceed(string code)
    {
        // Act
        var result = ProductCode.TryCreate(code, out var productCode, out var error);

        // Assert
        result.Should().BeTrue();
        productCode.Should().NotBeNull();
        productCode!.Value.Should().Be(code);
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryCreate_WithEmptyOrNull_ShouldFail(string? code)
    {
        // Act
        var result = ProductCode.TryCreate(code, out var productCode, out var error);

        // Assert
        result.Should().BeFalse();
        productCode.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("123ABC")] // starts with number
    [InlineData("AB")] // too short
    [InlineData("TOOLONGCODE1234567")] // too long
    [InlineData("G001")] // only 1 letter
    public void TryCreate_WithInvalidFormat_ShouldFail(string code)
    {
        // Act
        var result = ProductCode.TryCreate(code, out var productCode, out var error);

        // Assert
        result.Should().BeFalse();
        productCode.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryCreate_WithLowercase_ShouldNormalizeToUppercase()
    {
        // Act
        var result = ProductCode.TryCreate("gpu001", out var productCode, out var error);

        // Assert
        result.Should().BeTrue();
        productCode!.Value.Should().Be("GPU001");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        ProductCode.TryCreate("GPU001", out var code1, out _);
        ProductCode.TryCreate("GPU001", out var code2, out _);

        // Assert
        code1.Should().Be(code2);
        (code1 == code2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        // Arrange
        ProductCode.TryCreate("GPU001", out var code1, out _);
        ProductCode.TryCreate("GPU002", out var code2, out _);

        // Assert
        code1.Should().NotBe(code2);
        (code1 != code2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        ProductCode.TryCreate("GPU001", out var code, out _);

        // Assert
        code!.ToString().Should().Be("GPU001");
    }
}
