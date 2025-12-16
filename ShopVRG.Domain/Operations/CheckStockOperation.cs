namespace ShopVRG.Domain.Operations;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Operation to check stock availability and calculate prices
/// Dependencies injected via constructor:
/// - checkProductExists: Verifies product exists in catalog
/// - getProductDetails: Gets product name, price, and stock
/// - reserveStock: Reserves stock for the order
/// </summary>
internal sealed class CheckStockOperation : OrderOperation
{
    private readonly Func<ProductCode, bool> _checkProductExists;
    private readonly Func<ProductCode, (ProductName Name, Price Price, StockQuantity Stock)?> _getProductDetails;
    private readonly Func<ProductCode, Quantity, bool> _reserveStock;

    public CheckStockOperation(
        Func<ProductCode, bool> checkProductExists,
        Func<ProductCode, (ProductName Name, Price Price, StockQuantity Stock)?> getProductDetails,
        Func<ProductCode, Quantity, bool> reserveStock)
    {
        _checkProductExists = checkProductExists;
        _getProductDetails = getProductDetails;
        _reserveStock = reserveStock;
    }

    protected override IOrder OnValidated(ValidatedOrder order)
    {
        var errors = new List<string>();
        var stockCheckedLines = new List<StockCheckedOrderLine>();

        foreach (var line in order.OrderLines)
        {
            // 1. Check if product exists
            if (!_checkProductExists(line.ProductCode))
            {
                errors.Add($"Product '{line.ProductCode}' does not exist");
                continue;
            }

            // 2. Get product details
            var productDetails = _getProductDetails(line.ProductCode);
            if (productDetails == null)
            {
                errors.Add($"Could not retrieve details for product '{line.ProductCode}'");
                continue;
            }

            var (productName, unitPrice, stock) = productDetails.Value;

            // 3. Check stock availability
            if (!stock.HasEnoughStock(line.Quantity))
            {
                errors.Add($"Insufficient stock for product '{line.ProductCode}': requested {line.Quantity}, available {stock}");
                continue;
            }

            // 4. Reserve stock
            if (!_reserveStock(line.ProductCode, line.Quantity))
            {
                errors.Add($"Failed to reserve stock for product '{line.ProductCode}'");
                continue;
            }

            // 5. Calculate line total
            var lineTotal = unitPrice.Multiply(line.Quantity.Value);

            stockCheckedLines.Add(new StockCheckedOrderLine(
                line.ProductCode,
                productName,
                line.Quantity,
                unitPrice,
                lineTotal));
        }

        if (errors.Count > 0)
        {
            return new InvalidOrder(errors);
        }

        return new StockCheckedOrder(
            order.OrderId,
            order.CustomerName,
            order.CustomerEmail,
            order.ShippingAddress,
            stockCheckedLines,
            order.CreatedAt);
    }
}
