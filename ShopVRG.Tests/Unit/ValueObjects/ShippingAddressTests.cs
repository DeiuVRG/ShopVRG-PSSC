using FluentAssertions;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.ValueObjects;

/// <summary>
/// Unit tests for ShippingAddress value object
/// </summary>
public class ShippingAddressTests
{
    [Fact]
    public void TryCreate_WithValidAddress_ShouldSucceed()
    {
        // Act
        var result = ShippingAddress.TryCreate(
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            out var address,
            out var error);

        // Assert
        result.Should().BeTrue();
        address.Should().NotBeNull();
        address!.Street.Should().Be("123 Main Street");
        address.City.Should().Be("Bucharest");
        address.PostalCode.Should().Be("010101");
        address.Country.Should().Be("Romania");
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("", "City", "12345", "Country", "Street must not be empty")]
    [InlineData("   ", "City", "12345", "Country", "Street must not be empty")]
    [InlineData(null, "City", "12345", "Country", "Street must not be empty")]
    public void TryCreate_WithEmptyStreet_ShouldFail(string? street, string city, string postal, string country, string expectedError)
    {
        // Act
        var result = ShippingAddress.TryCreate(street, city, postal, country, out var address, out var error);

        // Assert
        result.Should().BeFalse();
        address.Should().BeNull();
        error.Should().Contain(expectedError);
    }

    [Fact]
    public void TryCreate_WithTooShortStreet_ShouldFail()
    {
        // Act
        var result = ShippingAddress.TryCreate("1234", "City", "12345", "Country", out var address, out var error);

        // Assert
        result.Should().BeFalse();
        address.Should().BeNull();
        error.Should().Contain("Street must be between 5 and 200 characters");
    }

    [Theory]
    [InlineData("12345 Valid Street", "", "12345", "Country", "City must not be empty")]
    [InlineData("12345 Valid Street", "   ", "12345", "Country", "City must not be empty")]
    [InlineData("12345 Valid Street", null, "12345", "Country", "City must not be empty")]
    public void TryCreate_WithEmptyCity_ShouldFail(string street, string? city, string postal, string country, string expectedError)
    {
        // Act
        var result = ShippingAddress.TryCreate(street, city, postal, country, out var address, out var error);

        // Assert
        result.Should().BeFalse();
        address.Should().BeNull();
        error.Should().Contain(expectedError);
    }

    [Fact]
    public void TryCreate_WithTooShortCity_ShouldFail()
    {
        // Act
        var result = ShippingAddress.TryCreate("12345 Valid Street", "A", "12345", "Country", out var address, out var error);

        // Assert
        result.Should().BeFalse();
        address.Should().BeNull();
        error.Should().Contain("City must be between 2 and 100 characters");
    }

    [Theory]
    [InlineData("12345 Valid Street", "City", "", "Country", "Postal code must not be empty")]
    [InlineData("12345 Valid Street", "City", "   ", "Country", "Postal code must not be empty")]
    [InlineData("12345 Valid Street", "City", null, "Country", "Postal code must not be empty")]
    public void TryCreate_WithEmptyPostalCode_ShouldFail(string street, string city, string? postal, string country, string expectedError)
    {
        // Act
        var result = ShippingAddress.TryCreate(street, city, postal, country, out var address, out var error);

        // Assert
        result.Should().BeFalse();
        address.Should().BeNull();
        error.Should().Contain(expectedError);
    }

    [Fact]
    public void TryCreate_WithTooShortPostalCode_ShouldFail()
    {
        // Act
        var result = ShippingAddress.TryCreate("12345 Valid Street", "City", "123", "Country", out var address, out var error);

        // Assert
        result.Should().BeFalse();
        address.Should().BeNull();
        error.Should().Contain("Postal code must be between 4 and 10 characters");
    }

    [Theory]
    [InlineData("12345 Valid Street", "City", "12345", "", "Country must not be empty")]
    [InlineData("12345 Valid Street", "City", "12345", "   ", "Country must not be empty")]
    [InlineData("12345 Valid Street", "City", "12345", null, "Country must not be empty")]
    public void TryCreate_WithEmptyCountry_ShouldFail(string street, string city, string postal, string? country, string expectedError)
    {
        // Act
        var result = ShippingAddress.TryCreate(street, city, postal, country, out var address, out var error);

        // Assert
        result.Should().BeFalse();
        address.Should().BeNull();
        error.Should().Contain(expectedError);
    }

    [Fact]
    public void TryCreate_WithTooShortCountry_ShouldFail()
    {
        // Act
        var result = ShippingAddress.TryCreate("12345 Valid Street", "City", "12345", "X", out var address, out var error);

        // Assert
        result.Should().BeFalse();
        address.Should().BeNull();
        error.Should().Contain("Country must be between 2 and 60 characters");
    }

    [Fact]
    public void ToFullAddress_ShouldReturnFormattedAddress()
    {
        // Arrange
        ShippingAddress.TryCreate("123 Main Street", "Bucharest", "010101", "Romania", out var address, out _);

        // Act & Assert
        address!.ToFullAddress().Should().Be("123 Main Street, Bucharest, 010101, Romania");
    }

    [Fact]
    public void ToString_ShouldReturnFullAddress()
    {
        // Arrange
        ShippingAddress.TryCreate("123 Main Street", "Bucharest", "010101", "Romania", out var address, out _);

        // Assert
        address!.ToString().Should().Be("123 Main Street, Bucharest, 010101, Romania");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeTrue()
    {
        // Arrange
        ShippingAddress.TryCreate("123 Main Street", "Bucharest", "010101", "Romania", out var address1, out _);
        ShippingAddress.TryCreate("123 Main Street", "Bucharest", "010101", "Romania", out var address2, out _);

        // Assert
        address1.Should().Be(address2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldBeFalse()
    {
        // Arrange
        ShippingAddress.TryCreate("123 Main Street", "Bucharest", "010101", "Romania", out var address1, out _);
        ShippingAddress.TryCreate("456 Other Street", "Cluj", "400001", "Romania", out var address2, out _);

        // Assert
        address1.Should().NotBe(address2);
    }
}
