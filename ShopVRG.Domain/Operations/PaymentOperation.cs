namespace ShopVRG.Domain.Operations;

using ShopVRG.Domain.Models.Entities;

/// <summary>
/// Base class for Payment operations using Transform pattern
/// </summary>
internal abstract class PaymentOperation
{
    internal IPayment Transform(IPayment payment)
    {
        return payment switch
        {
            UnvalidatedPayment unvalidated => OnUnvalidated(unvalidated),
            ValidatedPayment validated => OnValidated(validated),
            ProcessedPayment processed => OnProcessed(processed),
            InvalidPayment invalid => OnInvalid(invalid),
            _ => throw new InvalidOperationException($"Unknown payment state: {payment.GetType().Name}")
        };
    }

    protected virtual IPayment OnUnvalidated(UnvalidatedPayment payment) => payment;
    protected virtual IPayment OnValidated(ValidatedPayment payment) => payment;
    protected virtual IPayment OnProcessed(ProcessedPayment payment) => payment;
    protected virtual IPayment OnInvalid(InvalidPayment payment) => payment;
}
