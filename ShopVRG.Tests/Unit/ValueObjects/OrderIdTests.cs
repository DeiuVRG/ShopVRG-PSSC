using FluentAssertions;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.ValueObjects;

/// <summary>
/// Unit tests for OrderId value object
/// </summary>
public class OrderIdTests
{
    [Fact]
    public void NewOrderId_ShouldCreateUniqueId()
    {
        // Act
        var orderId1 = OrderId.NewOrderId();
        var orderId2 = OrderId.NewOrderId();

        // Assert
        orderId1.Should().NotBeNull();
        orderId2.Should().NotBeNull();
        orderId1.Should().NotBe(orderId2);
    }

    [Fact]
    public void TryCreate_WithValidGuidString_ShouldSucceed()
    {
        // Arrange
        var guidString = Guid.NewGuid().ToString();

        // Act
        var result = OrderId.TryCreate(guidString, out var orderId, out var error);

        // Assert
        result.Should().BeTrue();
        orderId.Should().NotBeNull();
        orderId!.Value.ToString().Should().Be(guidString);
        error.Should().BeNull();
    }

    [Fact]
    public void TryCreate_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = OrderId.TryCreate(guid, out var orderId, out var error);

        // Assert
        result.Should().BeTrue();
        orderId.Should().NotBeNull();
        orderId!.Value.Should().Be(guid);
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryCreate_WithEmptyOrNull_ShouldFail(string? input)
    {
        // Act
        var result = OrderId.TryCreate(input, out var orderId, out var error);

        // Assert
        result.Should().BeFalse();
        orderId.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryCreate_WithEmptyGuid_ShouldFail()
    {
        // Act
        var result = OrderId.TryCreate(Guid.Empty, out var orderId, out var error);

        // Assert
        result.Should().BeFalse();
        orderId.Should().BeNull();
        error.Should().Contain("cannot be empty");
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("12345")]
    [InlineData("not-a-guid-at-all")]
    public void TryCreate_WithInvalidFormat_ShouldFail(string input)
    {
        // Act
        var result = OrderId.TryCreate(input, out var orderId, out var error);

        // Assert
        result.Should().BeFalse();
        orderId.Should().BeNull();
        error.Should().Contain("Invalid Order ID format");
    }

    [Fact]
    public void FromGuid_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var orderId = OrderId.FromGuid(guid);

        // Assert
        orderId.Should().NotBeNull();
        orderId.Value.Should().Be(guid);
    }

    [Fact]
    public void FromGuid_WithEmptyGuid_ShouldThrow()
    {
        // Act
        var act = () => OrderId.FromGuid(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var orderId1 = OrderId.FromGuid(guid);
        var orderId2 = OrderId.FromGuid(guid);

        // Assert
        orderId1.Should().Be(orderId2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        // Arrange
        var orderId1 = OrderId.NewOrderId();
        var orderId2 = OrderId.NewOrderId();

        // Assert
        orderId1.Should().NotBe(orderId2);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var orderId = OrderId.FromGuid(guid);

        // Assert
        orderId.ToString().Should().Be(guid.ToString());
    }
}
