-- ShopVRG Database Schema
-- Tables for Products, Orders, Payments, and Shipments
-- Generated for Azure SQL Database with Entra passwordless authentication

-- Create Products Table
CREATE TABLE [dbo].[Products] (
    [Code] NVARCHAR(50) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [Category] NVARCHAR(100) NOT NULL,
    [Price] DECIMAL(18, 2) NOT NULL,
    [Stock] INT NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    CONSTRAINT [CK_Products_Price] CHECK ([Price] >= 0),
    CONSTRAINT [CK_Products_Stock] CHECK ([Stock] >= 0)
);

-- Create Orders Table
CREATE TABLE [dbo].[Orders] (
    [OrderId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [CustomerName] NVARCHAR(255) NOT NULL,
    [CustomerEmail] NVARCHAR(255) NOT NULL,
    [ShippingStreet] NVARCHAR(255) NOT NULL,
    [ShippingCity] NVARCHAR(100) NOT NULL,
    [ShippingPostalCode] NVARCHAR(20) NOT NULL,
    [ShippingCountry] NVARCHAR(100) NOT NULL,
    [TotalPrice] DECIMAL(18, 2) NOT NULL,
    [Status] NVARCHAR(20) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [PaidAt] DATETIME2 NULL,
    [ShippedAt] DATETIME2 NULL,
    CONSTRAINT [CK_Orders_TotalPrice] CHECK ([TotalPrice] >= 0)
);

-- Create OrderLines Table
CREATE TABLE [dbo].[OrderLines] (
    [Id] INT NOT NULL IDENTITY(1, 1) PRIMARY KEY,
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [ProductCode] NVARCHAR(50) NOT NULL,
    [ProductName] NVARCHAR(255) NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(18, 2) NOT NULL,
    [LineTotal] DECIMAL(18, 2) NOT NULL,
    CONSTRAINT [FK_OrderLines_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([OrderId]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderLines_Products] FOREIGN KEY ([ProductCode]) REFERENCES [dbo].[Products]([Code]),
    CONSTRAINT [CK_OrderLines_Quantity] CHECK ([Quantity] > 0),
    CONSTRAINT [CK_OrderLines_UnitPrice] CHECK ([UnitPrice] >= 0),
    CONSTRAINT [CK_OrderLines_LineTotal] CHECK ([LineTotal] >= 0)
);

-- Create Payments Table
CREATE TABLE [dbo].[Payments] (
    [PaymentId] NVARCHAR(50) NOT NULL PRIMARY KEY,
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [Amount] DECIMAL(18, 2) NOT NULL,
    [MaskedCardNumber] NVARCHAR(20) NOT NULL,
    [CardHolderName] NVARCHAR(255) NOT NULL,
    [TransactionReference] NVARCHAR(255) NOT NULL,
    [ProcessedAt] DATETIME2 NOT NULL,
    CONSTRAINT [FK_Payments_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([OrderId]),
    CONSTRAINT [CK_Payments_Amount] CHECK ([Amount] > 0)
);

-- Create Shipments Table
CREATE TABLE [dbo].[Shipments] (
    [Id] INT NOT NULL IDENTITY(1, 1) PRIMARY KEY,
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [TrackingNumber] NVARCHAR(100) NOT NULL,
    [Carrier] NVARCHAR(100) NOT NULL,
    [ShippedAt] DATETIME2 NOT NULL,
    [EstimatedDelivery] DATETIME2 NOT NULL,
    CONSTRAINT [FK_Shipments_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([OrderId]),
    CONSTRAINT [UQ_Shipments_TrackingNumber] UNIQUE ([TrackingNumber])
);

-- Create Indexes for better query performance
CREATE NONCLUSTERED INDEX [IX_Orders_Status] ON [dbo].[Orders]([Status]);
CREATE NONCLUSTERED INDEX [IX_Orders_CustomerEmail] ON [dbo].[Orders]([CustomerEmail]);
CREATE NONCLUSTERED INDEX [IX_Orders_CreatedAt] ON [dbo].[Orders]([CreatedAt]);
CREATE NONCLUSTERED INDEX [IX_OrderLines_OrderId] ON [dbo].[OrderLines]([OrderId]);
CREATE NONCLUSTERED INDEX [IX_OrderLines_ProductCode] ON [dbo].[OrderLines]([ProductCode]);
CREATE NONCLUSTERED INDEX [IX_Payments_OrderId] ON [dbo].[Payments]([OrderId]);
CREATE NONCLUSTERED INDEX [IX_Shipments_OrderId] ON [dbo].[Shipments]([OrderId]);
CREATE NONCLUSTERED INDEX [IX_Shipments_TrackingNumber] ON [dbo].[Shipments]([TrackingNumber]);

-- Create seed data for testing
INSERT INTO [dbo].[Products] ([Code], [Name], [Description], [Category], [Price], [Stock], [IsActive])
VALUES 
    ('CPU001', 'Intel Core i9-14900K', '24-core (8 P-cores + 16 E-cores) processor with up to 6.0 GHz boost clock', 'CPU', 589.99, 50, 1),
    ('CPU002', 'AMD Ryzen 9 7950X', '16-core 32-thread processor with up to 5.7 GHz boost clock', 'CPU', 549.99, 45, 1),
    ('GPU001', 'NVIDIA GeForce RTX 4090', '24GB GDDR6X, 16384 CUDA cores, ray tracing enabled', 'GPU', 1599.99, 25, 1),
    ('GPU002', 'AMD Radeon RX 7900 XTX', '24GB GDDR6, 6144 stream processors, RDNA 3 architecture', 'GPU', 999.99, 30, 1),
    ('RAM001', 'Corsair Vengeance DDR5-6000 32GB', '32GB (2x16GB) DDR5-6000 CL36 memory kit', 'RAM', 159.99, 100, 1),
    ('RAM002', 'G.Skill Trident Z5 RGB DDR5-6400 64GB', '64GB (2x32GB) DDR5-6400 CL32 RGB memory kit', 'RAM', 299.99, 60, 1),
    ('MBD001', 'ASUS ROG Maximus Z790 Hero', 'Intel Z790 chipset, LGA1700, DDR5 support, WiFi 6E', 'Motherboard', 629.99, 35, 1),
    ('MBD002', 'MSI MEG X670E ACE', 'AMD X670E chipset, AM5, DDR5 support, WiFi 6E', 'Motherboard', 699.99, 28, 1),
    ('SSD001', 'Samsung 990 Pro 2TB NVMe', '2TB NVMe M.2 SSD, up to 7450 MB/s read speed', 'Storage', 199.99, 80, 1),
    ('SSD002', 'WD Black SN850X 4TB NVMe', '4TB NVMe M.2 SSD, up to 7300 MB/s read speed', 'Storage', 399.99, 40, 1),
    ('PSU001', 'Corsair RM1000x 1000W 80+ Gold', '1000W fully modular power supply, 80+ Gold certified', 'PSU', 189.99, 55, 1),
    ('CASE001', 'Lian Li O11 Dynamic EVO', 'Mid-tower case with tempered glass, supports E-ATX', 'Case', 169.99, 45, 1);

PRINT 'Tables created successfully!';
PRINT 'Database schema includes: Products, Orders, OrderLines, Payments, and Shipments tables';
