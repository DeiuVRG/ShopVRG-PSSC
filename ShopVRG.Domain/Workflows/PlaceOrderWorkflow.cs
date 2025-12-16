namespace ShopVRG.Domain.Workflows;

using ShopVRG.Domain.Models.Commands;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.Events;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Operations;

/// <summary>
/// Workflow for placing an order
/// Composes operations: Validate → CheckStock → Place
/// </summary>
public sealed class PlaceOrderWorkflow
{
    public IOrderEvent Execute(
        PlaceOrderCommand command,
        Func<ProductCode, bool> checkProductExists,
        Func<ProductCode, (ProductName Name, Price Price, StockQuantity Stock)?> getProductDetails,
        Func<ProductCode, Quantity, bool> reserveStock,
        Func<OrderId, StockCheckedOrder, bool> persistOrder)
    {
        // 1. Create unvalidated state from command
        IOrder order = new UnvalidatedOrder(
            command.CustomerName,
            command.CustomerEmail,
            command.ShippingStreet,
            command.ShippingCity,
            command.ShippingPostalCode,
            command.ShippingCountry,
            command.OrderLines.Select(l => new UnvalidatedOrderLine(l.ProductCode, l.Quantity)));

        // 2. Pipeline of operations using Transform
        order = new ValidateOrderOperation()
            .Transform(order);

        order = new CheckStockOperation(checkProductExists, getProductDetails, reserveStock)
            .Transform(order);

        order = new PlaceOrderOperation(persistOrder)
            .Transform(order);

        // 3. Convert final state to event
        return order.ToEvent();
    }
}
