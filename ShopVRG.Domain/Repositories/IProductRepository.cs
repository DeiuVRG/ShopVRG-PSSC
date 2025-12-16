namespace ShopVRG.Domain.Repositories;

using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Repository interface for Product operations
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByCodeAsync(ProductCode code);
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<IReadOnlyList<Product>> GetByCategoryAsync(string category);
    Task<IReadOnlyList<Product>> GetActiveProductsAsync();
    Task<bool> ExistsAsync(ProductCode code);
    Task<bool> AddAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> ReserveStockAsync(ProductCode code, Quantity quantity);
    Task<bool> RestoreStockAsync(ProductCode code, Quantity quantity);
}
