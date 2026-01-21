using FluentAssertions;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.ValueObjects;

/// <summary>
/// Unit tests for CustomerEmail value object
/// </summary>
public class CustomerEmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("john.doe123@company.co.uk")]
    [InlineData("simple@test.io")]
    public void TryCreate_WithValidEmail_ShouldSucceed(string email)
    {
        // Act
        var result = CustomerEmail.TryCreate(email, out var customerEmail, out var error);

        // Assert
        result.Should().BeTrue();
        customerEmail.Should().NotBeNull();
        customerEmail!.Value.Should().Be(email.ToLowerInvariant());
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryCreate_WithEmptyOrNull_ShouldFail(string? email)
    {
        // Act
        var result = CustomerEmail.TryCreate(email, out var customerEmail, out var error);

        // Assert
        result.Should().BeFalse();
        customerEmail.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    public void TryCreate_WithInvalidFormat_ShouldFail(string email)
    {
        // Act
        var result = CustomerEmail.TryCreate(email, out var customerEmail, out var error);

        // Assert
        result.Should().BeFalse();
        customerEmail.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        CustomerEmail.TryCreate("test@example.com", out var email1, out _);
        CustomerEmail.TryCreate("test@example.com", out var email2, out _);

        // Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        // Arrange
        CustomerEmail.TryCreate("test1@example.com", out var email1, out _);
        CustomerEmail.TryCreate("test2@example.com", out var email2, out _);

        // Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void ToString_ShouldReturnEmail()
    {
        // Arrange
        CustomerEmail.TryCreate("test@example.com", out var email, out _);

        // Assert
        email!.ToString().Should().Be("test@example.com");
    }

    [Fact]
    public void GetDomain_ShouldReturnDomain()
    {
        // Arrange
        CustomerEmail.TryCreate("test@example.com", out var email, out _);

        // Assert
        email!.GetDomain().Should().Be("example.com");
    }
}
