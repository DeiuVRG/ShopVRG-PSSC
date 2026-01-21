using ShopVRG.Domain.Models.Entities;
using ShopVRG.Domain.Models.ValueObjects;
using ShopVRG.Domain.Repositories;

namespace ShopVRG.Tests.Fakes;

/// <summary>
/// Fake implementation of IProductRepository for testing
/// Uses in-memory storage instead of database
/// </summary>
public class FakeProductRepository : IProductRepository
{
    private readonly Dictionary<string, Product> _products = new();

    public FakeProductRepository()
    {
        // Seed with test products
        SeedTestProducts();
    }

    private void SeedTestProducts()
    {
        var products = new[]
        {
            CreateProduct("GPU001", "NVIDIA RTX 4090", "High-end graphics card", "GPU", 1599.99m, 10),
            CreateProduct("GPU002", "AMD RX 7900 XTX", "AMD flagship GPU", "GPU", 999.99m, 15),
            CreateProduct("CPU001", "Intel Core i9-14900K", "24-core processor", "CPU", 589.99m, 20),
            CreateProduct("CPU002", "AMD Ryzen 9 7950X", "16-core processor", "CPU", 549.99m, 25),
            CreateProduct("RAM001", "Corsair 32GB DDR5", "High-speed memory", "RAM", 149.99m, 50),
            CreateProduct("SSD001", "Samsung 990 Pro 2TB", "NVMe SSD", "SSD", 179.99m, 30),
            CreateProduct("MB001", "ASUS ROG Maximus", "Z790 motherboard", "Motherboard", 629.99m, 8),
            CreateProduct("PSU001", "Corsair RM1000x", "1000W power supply", "PSU", 189.99m, 12),
            CreateProduct("CASE001", "Lian Li O11 Dynamic", "Mid-tower case", "Case", 149.99m, 18),
            CreateProduct("COOL001", "Noctua NH-D15", "CPU air cooler", "Cooling", 99.99m, 40),
        };

        foreach (var product in products)
        {
            if (product != null)
            {
                _products[product.Code.Value] = product;
            }
        }
    }

    private static Product? CreateProduct(string code, string name, string desc, string category, decimal price, int stock)
    {
        if (!ProductCode.TryCreate(code, out var productCode, out _) || productCode == null)
            return null;
        if (!ProductName.TryCreate(name, out var productName, out _) || productName == null)
            return null;

        var priceValue = Price.FromDecimal(price);
        var stockValue = StockQuantity.FromInt(stock);

        return Product.Create(
            productCode,
            productName,
            desc,
            category,
            priceValue,
            stockValue
        );
    }

    public Task<Product?> GetByCodeAsync(ProductCode code)
    {
        _products.TryGetValue(code.Value, out var product);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyList<Product>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<Product>>(_products.Values.ToList());
    }

    public Task<IReadOnlyList<Product>> GetByCategoryAsync(string category)
    {
        var products = _products.Values.Where(p => p.Category == category).ToList();
        return Task.FromResult<IReadOnlyList<Product>>(products);
    }

    public Task<IReadOnlyList<Product>> GetActiveProductsAsync()
    {
        var products = _products.Values.Where(p => p.IsActive).ToList();
        return Task.FromResult<IReadOnlyList<Product>>(products);
    }

    public Task<bool> ExistsAsync(ProductCode code)
    {
        return Task.FromResult(_products.ContainsKey(code.Value));
    }

    public Task<bool> AddAsync(Product product)
    {
        if (_products.ContainsKey(product.Code.Value))
            return Task.FromResult(false);

        _products[product.Code.Value] = product;
        return Task.FromResult(true);
    }

    public Task<bool> UpdateAsync(Product product)
    {
        if (!_products.ContainsKey(product.Code.Value))
            return Task.FromResult(false);

        _products[product.Code.Value] = product;
        return Task.FromResult(true);
    }

    public Task<bool> ReserveStockAsync(ProductCode code, Quantity quantity)
    {
        if (!_products.TryGetValue(code.Value, out var product))
            return Task.FromResult(false);

        return Task.FromResult(product.TryReserveStock(quantity));
    }

    public Task<bool> RestoreStockAsync(ProductCode code, Quantity quantity)
    {
        if (!_products.TryGetValue(code.Value, out var product))
            return Task.FromResult(false);

        product.RestoreStock(quantity);
        return Task.FromResult(true);
    }

    // Helper methods for testing
    public void Clear() => _products.Clear();
    public int Count => _products.Count;
    public Product? GetProduct(string code) => _products.GetValueOrDefault(code);

    public void SetStock(string code, int stock)
    {
        if (_products.TryGetValue(code, out var product))
        {
            product.UpdateStock(StockQuantity.FromInt(stock));
        }
    }

    // Sync helper methods for easier test usage
    public bool Exists(string code) => _products.ContainsKey(code);

    public FakeProductDto? GetByCode(string code)
    {
        if (_products.TryGetValue(code, out var product))
        {
            return new FakeProductDto
            {
                Code = product.Code.Value,
                Name = product.Name.Value,
                Price = product.Price.Value,
                Stock = product.Stock.Value
            };
        }
        return null;
    }

    public bool ReserveStock(string code, int quantity)
    {
        if (!_products.TryGetValue(code, out var product))
            return false;

        Quantity.TryCreate(quantity, out var qty, out _);
        if (qty == null) return false;

        return product.TryReserveStock(qty);
    }

    public void ReleaseStock(string code, int quantity)
    {
        if (_products.TryGetValue(code, out var product))
        {
            Quantity.TryCreate(quantity, out var qty, out _);
            if (qty != null)
            {
                product.RestoreStock(qty);
            }
        }
    }

    public IEnumerable<FakeProductDto> GetAll()
    {
        return _products.Values.Select(p => new FakeProductDto
        {
            Code = p.Code.Value,
            Name = p.Name.Value,
            Price = p.Price.Value,
            Stock = p.Stock.Value
        });
    }

    public class FakeProductDto
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
