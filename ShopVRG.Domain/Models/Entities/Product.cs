namespace ShopVRG.Domain.Models.Entities;

using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Product entity representing a PC component in the catalog
/// </summary>
public sealed class Product
{
    public ProductCode Code { get; }
    public ProductName Name { get; }
    public string Description { get; }
    public string Category { get; }
    public Price Price { get; private set; }
    public StockQuantity Stock { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    private Product(
        ProductCode code,
        ProductName name,
        string description,
        string category,
        Price price,
        StockQuantity stock,
        bool isActive,
        DateTime createdAt)
    {
        Code = code;
        Name = name;
        Description = description;
        Category = category;
        Price = price;
        Stock = stock;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public static Product Create(
        ProductCode code,
        ProductName name,
        string description,
        string category,
        Price price,
        StockQuantity stock)
    {
        return new Product(
            code,
            name,
            description,
            category,
            price,
            stock,
            isActive: true,
            createdAt: DateTime.UtcNow);
    }

    public void UpdatePrice(Price newPrice)
    {
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStock(StockQuantity newStock)
    {
        Stock = newStock;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool TryReserveStock(Quantity quantity)
    {
        if (!Stock.HasEnoughStock(quantity))
            return false;

        Stock = Stock.Decrease(quantity);
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void RestoreStock(Quantity quantity)
    {
        Stock = Stock.Increase(quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
