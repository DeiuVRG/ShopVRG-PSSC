namespace ShopVRG.Domain.Operations;

using ShopVRG.Domain.Models.Entities;

/// <summary>
/// Base class for Shipping operations using Transform pattern
/// </summary>
internal abstract class ShippingOperation
{
    internal IShipping Transform(IShipping shipping)
    {
        return shipping switch
        {
            UnvalidatedShipping unvalidated => OnUnvalidated(unvalidated),
            ValidatedShipping validated => OnValidated(validated),
            ShippedOrder shipped => OnShipped(shipped),
            InvalidShipping invalid => OnInvalid(invalid),
            _ => throw new InvalidOperationException($"Unknown shipping state: {shipping.GetType().Name}")
        };
    }

    protected virtual IShipping OnUnvalidated(UnvalidatedShipping shipping) => shipping;
    protected virtual IShipping OnValidated(ValidatedShipping shipping) => shipping;
    protected virtual IShipping OnShipped(ShippedOrder shipping) => shipping;
    protected virtual IShipping OnInvalid(InvalidShipping shipping) => shipping;
}
