namespace ShopVRG.Domain.Workflows;

using ShopVRG.Domain.Models.Commands;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.Events;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Operations;

/// <summary>
/// Workflow for processing a payment
/// Composes operations: Validate → Process
/// </summary>
public sealed class ProcessPaymentWorkflow
{
    public IPaymentEvent Execute(
        ProcessPaymentCommand command,
        Func<OrderId, bool> checkOrderExists,
        Func<OrderId, Price?> getOrderTotal,
        Func<ValidatedPayment, string?> processPayment,
        Func<PaymentId, OrderId, Price, string, bool> persistPayment)
    {
        // 1. Create unvalidated state from command
        IPayment payment = new UnvalidatedPayment(
            command.OrderId,
            command.Amount,
            command.CardNumber,
            command.CardHolderName,
            command.ExpiryDate,
            command.Cvv);

        // 2. Pipeline of operations using Transform
        payment = new ValidatePaymentOperation(checkOrderExists, getOrderTotal)
            .Transform(payment);

        payment = new ProcessPaymentOperation(processPayment, persistPayment)
            .Transform(payment);

        // 3. Convert final state to event
        return payment.ToEvent();
    }
}
