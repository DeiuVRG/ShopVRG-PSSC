namespace ShopVRG.Data;

using Microsoft.EntityFrameworkCore;
using ShopVRG.Data.Models;

/// <summary>
/// Entity Framework DbContext for PC Components Shop
/// </summary>
public class ShopDbContext : DbContext
{
    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<OrderLineEntity> OrderLines { get; set; } = null!;
    public DbSet<PaymentEntity> Payments { get; set; } = null!;
    public DbSet<ShipmentEntity> Shipments { get; set; } = null!;

    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product configuration
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(p => p.Code);
            entity.Property(p => p.Code).HasMaxLength(10);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.Category).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.Stock).IsRequired();
            entity.Property(p => p.IsActive).IsRequired();
            entity.HasIndex(p => p.Category);
            entity.HasIndex(p => p.IsActive);
        });

        // Order configuration
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(o => o.OrderId);
            entity.Property(o => o.CustomerName).HasMaxLength(100).IsRequired();
            entity.Property(o => o.CustomerEmail).HasMaxLength(254).IsRequired();
            entity.Property(o => o.ShippingStreet).HasMaxLength(200).IsRequired();
            entity.Property(o => o.ShippingCity).HasMaxLength(100).IsRequired();
            entity.Property(o => o.ShippingPostalCode).HasMaxLength(10).IsRequired();
            entity.Property(o => o.ShippingCountry).HasMaxLength(60).IsRequired();
            entity.Property(o => o.TotalPrice).HasPrecision(18, 2).IsRequired();
            entity.Property(o => o.Status).HasMaxLength(20).IsRequired();
            entity.HasIndex(o => o.CustomerEmail);
            entity.HasIndex(o => o.Status);
            entity.HasMany(o => o.OrderLines)
                  .WithOne()
                  .HasForeignKey(ol => ol.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderLine configuration
        modelBuilder.Entity<OrderLineEntity>(entity =>
        {
            entity.HasKey(ol => ol.Id);
            entity.Property(ol => ol.ProductCode).HasMaxLength(10).IsRequired();
            entity.Property(ol => ol.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(ol => ol.UnitPrice).HasPrecision(18, 2).IsRequired();
            entity.Property(ol => ol.LineTotal).HasPrecision(18, 2).IsRequired();
            entity.HasIndex(ol => ol.OrderId);
        });

        // Payment configuration
        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.HasKey(p => p.PaymentId);
            entity.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.MaskedCardNumber).HasMaxLength(20).IsRequired();
            entity.Property(p => p.CardHolderName).HasMaxLength(100).IsRequired();
            entity.Property(p => p.TransactionReference).HasMaxLength(100).IsRequired();
            entity.HasIndex(p => p.OrderId);
        });

        // Shipment configuration
        modelBuilder.Entity<ShipmentEntity>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.TrackingNumber).HasMaxLength(50).IsRequired();
            entity.Property(s => s.Carrier).HasMaxLength(50).IsRequired();
            entity.HasIndex(s => s.OrderId);
            entity.HasIndex(s => s.TrackingNumber);
        });

        // Seed initial product data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>().HasData(
            new ProductEntity
            {
                Code = "CPU001",
                Name = "Intel Core i9-14900K",
                Description = "24-core (8 P-cores + 16 E-cores) processor with up to 6.0 GHz boost clock",
                Category = "CPU",
                Price = 589.99m,
                Stock = 50,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "CPU002",
                Name = "AMD Ryzen 9 7950X",
                Description = "16-core 32-thread processor with up to 5.7 GHz boost clock",
                Category = "CPU",
                Price = 549.99m,
                Stock = 45,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "GPU001",
                Name = "NVIDIA GeForce RTX 4090",
                Description = "24GB GDDR6X, 16384 CUDA cores, ray tracing enabled",
                Category = "GPU",
                Price = 1599.99m,
                Stock = 25,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "GPU002",
                Name = "AMD Radeon RX 7900 XTX",
                Description = "24GB GDDR6, 6144 stream processors, RDNA 3 architecture",
                Category = "GPU",
                Price = 999.99m,
                Stock = 30,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "RAM001",
                Name = "Corsair Vengeance DDR5-6000 32GB",
                Description = "32GB (2x16GB) DDR5-6000 CL36 memory kit",
                Category = "RAM",
                Price = 159.99m,
                Stock = 100,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "RAM002",
                Name = "G.Skill Trident Z5 RGB DDR5-6400 64GB",
                Description = "64GB (2x32GB) DDR5-6400 CL32 RGB memory kit",
                Category = "RAM",
                Price = 299.99m,
                Stock = 60,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "MBD001",
                Name = "ASUS ROG Maximus Z790 Hero",
                Description = "Intel Z790 chipset, LGA1700, DDR5 support, WiFi 6E",
                Category = "Motherboard",
                Price = 629.99m,
                Stock = 35,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "MBD002",
                Name = "MSI MEG X670E ACE",
                Description = "AMD X670E chipset, AM5, DDR5 support, WiFi 6E",
                Category = "Motherboard",
                Price = 699.99m,
                Stock = 28,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "SSD001",
                Name = "Samsung 990 Pro 2TB NVMe",
                Description = "2TB NVMe M.2 SSD, up to 7450 MB/s read speed",
                Category = "Storage",
                Price = 199.99m,
                Stock = 80,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "SSD002",
                Name = "WD Black SN850X 4TB NVMe",
                Description = "4TB NVMe M.2 SSD, up to 7300 MB/s read speed",
                Category = "Storage",
                Price = 399.99m,
                Stock = 40,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "PSU001",
                Name = "Corsair RM1000x 1000W 80+ Gold",
                Description = "1000W fully modular power supply, 80+ Gold certified",
                Category = "PSU",
                Price = 189.99m,
                Stock = 55,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductEntity
            {
                Code = "CASE001",
                Name = "Lian Li O11 Dynamic EVO",
                Description = "Mid-tower case with tempered glass, supports E-ATX",
                Category = "Case",
                Price = 169.99m,
                Stock = 45,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
