-- ShopVRG Azure SQL Database Queries
-- Connection: ShopVRG Azure SQL
-- Database: ShopVRG-db @ server-incercare.database.windows.net

-- ============================================
-- View all products
-- ============================================
SELECT 
    Code as 'Product Code',
    Name as 'Product Name',
    Category,
    Price,
    Stock,
    IsActive
FROM Products
ORDER BY Category, Name;

-- ============================================
-- Products summary by category
-- ============================================
SELECT 
    Category,
    COUNT(*) as 'Total Products',
    SUM(Stock) as 'Total Stock',
    AVG(Price) as 'Average Price',
    MIN(Price) as 'Min Price',
    MAX(Price) as 'Max Price'
FROM Products
GROUP BY Category
ORDER BY Category;

-- ============================================
-- Low stock products (Stock < 50)
-- ============================================
SELECT Code, Name, Category, Stock
FROM Products
WHERE Stock < 50
ORDER BY Stock ASC;

-- ============================================
-- Most expensive products
-- ============================================
SELECT TOP 5 Code, Name, Category, Price, Stock
FROM Products
ORDER BY Price DESC;

-- ============================================
-- View all tables in database
-- ============================================
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- ============================================
-- Recent orders (if any)
-- ============================================
SELECT 
    OrderId,
    CustomerName,
    CustomerEmail,
    TotalPrice,
    Status,
    CreatedAt
FROM Orders
ORDER BY CreatedAt DESC;
