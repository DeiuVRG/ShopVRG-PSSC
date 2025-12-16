namespace ShopVRG.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using ShopVRG.Data.Models;
using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Repositories;

/// <summary>
/// Repository implementation for Product operations
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ShopDbContext _context;

    public ProductRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByCodeAsync(ProductCode code)
    {
        var entity = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == code.Value);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        var entities = await _context.Products
            .AsNoTracking()
            .ToListAsync();

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(string category)
    {
        var entities = await _context.Products
            .AsNoTracking()
            .Where(p => p.Category == category)
            .ToListAsync();

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Product>> GetActiveProductsAsync()
    {
        var entities = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync();

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<bool> ExistsAsync(ProductCode code)
    {
        return await _context.Products.AnyAsync(p => p.Code == code.Value);
    }

    public async Task<bool> AddAsync(Product product)
    {
        try
        {
            var entity = MapToEntity(product);
            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        try
        {
            var entity = await _context.Products.FindAsync(product.Code.Value);
            if (entity == null) return false;

            entity.Name = product.Name.Value;
            entity.Description = product.Description;
            entity.Category = product.Category;
            entity.Price = product.Price.Value;
            entity.Stock = product.Stock.Value;
            entity.IsActive = product.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ReserveStockAsync(ProductCode code, Quantity quantity)
    {
        try
        {
            var entity = await _context.Products.FindAsync(code.Value);
            if (entity == null || entity.Stock < quantity.Value) return false;

            entity.Stock -= quantity.Value;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RestoreStockAsync(ProductCode code, Quantity quantity)
    {
        try
        {
            var entity = await _context.Products.FindAsync(code.Value);
            if (entity == null) return false;

            entity.Stock += quantity.Value;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Product MapToDomain(ProductEntity entity)
    {
        if (!ProductCode.TryCreate(entity.Code, out var code, out _))
            throw new InvalidOperationException($"Invalid product code in database: {entity.Code}");

        if (!ProductName.TryCreate(entity.Name, out var name, out _))
            throw new InvalidOperationException($"Invalid product name in database: {entity.Name}");

        if (!Price.TryCreate(entity.Price, out var price, out _))
            throw new InvalidOperationException($"Invalid price in database: {entity.Price}");

        if (!StockQuantity.TryCreate(entity.Stock, out var stock, out _))
            throw new InvalidOperationException($"Invalid stock in database: {entity.Stock}");

        return Product.Create(
            code!,
            name!,
            entity.Description,
            entity.Category,
            price!,
            stock!);
    }

    private static ProductEntity MapToEntity(Product product)
    {
        return new ProductEntity
        {
            Code = product.Code.Value,
            Name = product.Name.Value,
            Description = product.Description,
            Category = product.Category,
            Price = product.Price.Value,
            Stock = product.Stock.Value,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
