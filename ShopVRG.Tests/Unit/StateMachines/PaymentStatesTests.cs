using FluentAssertions;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

namespace ShopVRG.Tests.Unit.StateMachines;

/// <summary>
/// Unit tests for Payment state machine states
/// Tests state transitions: Unvalidated -> Validated -> Processed / Invalid
/// </summary>
public class PaymentStatesTests
{
    #region UnvalidatedPayment Tests

    [Fact]
    public void UnvalidatedPayment_ShouldStoreRawPaymentData()
    {
        // Arrange & Act
        var payment = new UnvalidatedPayment(
            Guid.NewGuid().ToString(),
            "1500.00",
            "4111111111111111",
            "John Doe",
            "12/25",
            "123");

        // Assert
        payment.Should().BeAssignableTo<IPayment>();
        payment.Amount.Should().Be("1500.00");
        payment.CardNumber.Should().Be("4111111111111111");
        payment.CardHolderName.Should().Be("John Doe");
        payment.ExpiryDate.Should().Be("12/25");
        payment.Cvv.Should().Be("123");
    }

    [Fact]
    public void UnvalidatedPayment_ShouldStoreOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid().ToString();

        // Act
        var payment = new UnvalidatedPayment(orderId, "100", "4111111111111111", "Test", "12/25", "123");

        // Assert
        payment.OrderId.Should().Be(orderId);
    }

    #endregion

    #region State Machine Interface Tests

    [Fact]
    public void AllPaymentStates_ShouldImplementIPayment()
    {
        // Assert
        typeof(UnvalidatedPayment).Should().Implement<IPayment>();
        typeof(ValidatedPayment).Should().Implement<IPayment>();
        typeof(ProcessedPayment).Should().Implement<IPayment>();
        typeof(InvalidPayment).Should().Implement<IPayment>();
    }

    [Fact]
    public void PaymentStates_ShouldBeSealed()
    {
        // Assert - sealed records/classes ensure state machine integrity
        typeof(UnvalidatedPayment).Should().BeSealed();
        typeof(ValidatedPayment).Should().BeSealed();
        typeof(ProcessedPayment).Should().BeSealed();
        typeof(InvalidPayment).Should().BeSealed();
    }

    #endregion

    #region ValidatedPayment Tests

    [Fact]
    public void ValidatedPayment_ShouldContainValidatedData()
    {
        // ValidatedPayment constructor is internal, test via interface
        typeof(ValidatedPayment).GetProperty("PaymentId").Should().NotBeNull();
        typeof(ValidatedPayment).GetProperty("OrderId").Should().NotBeNull();
        typeof(ValidatedPayment).GetProperty("Amount").Should().NotBeNull();
        typeof(ValidatedPayment).GetProperty("MaskedCardNumber").Should().NotBeNull();
        typeof(ValidatedPayment).GetProperty("CardHolderName").Should().NotBeNull();
        typeof(ValidatedPayment).GetProperty("ValidatedAt").Should().NotBeNull();
    }

    #endregion

    #region ProcessedPayment Tests

    [Fact]
    public void ProcessedPayment_ShouldContainTransactionReference()
    {
        // ProcessedPayment constructor is internal, test via reflection
        typeof(ProcessedPayment).GetProperty("TransactionReference").Should().NotBeNull();
        typeof(ProcessedPayment).GetProperty("ProcessedAt").Should().NotBeNull();
    }

    #endregion

    #region InvalidPayment Tests

    [Fact]
    public void InvalidPayment_ShouldContainReasons()
    {
        // InvalidPayment constructor is internal, test via reflection
        typeof(InvalidPayment).GetProperty("OrderId").Should().NotBeNull();
        typeof(InvalidPayment).GetProperty("Reasons").Should().NotBeNull();
    }

    #endregion
}
