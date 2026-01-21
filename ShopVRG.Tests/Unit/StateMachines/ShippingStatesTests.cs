using FluentAssertions;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.StateMachines;

/// <summary>
/// Unit tests for Shipping state machine states
/// Tests state transitions: Unvalidated -> Validated -> Shipped / Invalid
/// </summary>
public class ShippingStatesTests
{
    #region UnvalidatedShipping Tests

    [Fact]
    public void UnvalidatedShipping_ShouldStoreRawShippingData()
    {
        // Arrange & Act
        var shipping = new UnvalidatedShipping(
            Guid.NewGuid().ToString(),
            "TRACK123456",
            "DHL");

        // Assert
        shipping.Should().BeAssignableTo<IShipping>();
        shipping.TrackingNumber.Should().Be("TRACK123456");
        shipping.Carrier.Should().Be("DHL");
    }

    [Fact]
    public void UnvalidatedShipping_ShouldStoreOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid().ToString();

        // Act
        var shipping = new UnvalidatedShipping(orderId, "TRACK123", "FedEx");

        // Assert
        shipping.OrderId.Should().Be(orderId);
    }

    #endregion

    #region State Machine Interface Tests

    [Fact]
    public void AllShippingStates_ShouldImplementIShipping()
    {
        // Assert
        typeof(UnvalidatedShipping).Should().Implement<IShipping>();
        typeof(ValidatedShipping).Should().Implement<IShipping>();
        typeof(ShippedOrder).Should().Implement<IShipping>();
        typeof(InvalidShipping).Should().Implement<IShipping>();
    }

    [Fact]
    public void ShippingStates_ShouldBeSealed()
    {
        // Assert - sealed records/classes ensure state machine integrity
        typeof(UnvalidatedShipping).Should().BeSealed();
        typeof(ValidatedShipping).Should().BeSealed();
        typeof(ShippedOrder).Should().BeSealed();
        typeof(InvalidShipping).Should().BeSealed();
    }

    #endregion

    #region ValidatedShipping Tests

    [Fact]
    public void ValidatedShipping_ShouldContainValidatedData()
    {
        // ValidatedShipping constructor is internal, test via reflection
        typeof(ValidatedShipping).GetProperty("OrderId").Should().NotBeNull();
        typeof(ValidatedShipping).GetProperty("TrackingNumber").Should().NotBeNull();
        typeof(ValidatedShipping).GetProperty("Carrier").Should().NotBeNull();
        typeof(ValidatedShipping).GetProperty("Destination").Should().NotBeNull();
        typeof(ValidatedShipping).GetProperty("ValidatedAt").Should().NotBeNull();
    }

    #endregion

    #region ShippedOrder Tests

    [Fact]
    public void ShippedOrder_ShouldContainShippingDetails()
    {
        // ShippedOrder constructor is internal, test via reflection
        typeof(ShippedOrder).GetProperty("OrderId").Should().NotBeNull();
        typeof(ShippedOrder).GetProperty("TrackingNumber").Should().NotBeNull();
        typeof(ShippedOrder).GetProperty("Carrier").Should().NotBeNull();
        typeof(ShippedOrder).GetProperty("Destination").Should().NotBeNull();
        typeof(ShippedOrder).GetProperty("ShippedAt").Should().NotBeNull();
        typeof(ShippedOrder).GetProperty("EstimatedDelivery").Should().NotBeNull();
    }

    #endregion

    #region InvalidShipping Tests

    [Fact]
    public void InvalidShipping_ShouldContainReasons()
    {
        // InvalidShipping constructor is internal, test via reflection
        typeof(InvalidShipping).GetProperty("OrderId").Should().NotBeNull();
        typeof(InvalidShipping).GetProperty("Reasons").Should().NotBeNull();
    }

    #endregion
}
