# Running SQL Scripts with MSSQL Extension in VS Code

## Prerequisites
1. MSSQL extension installed in VS Code
2. Azure SQL Database setup with Entra authentication configured
3. Your Azure account logged into VS Code

## Steps to Create Tables

### Option 1: Using VS Code MSSQL Extension
1. Open the `CreateTables.sql` file in VS Code
2. Open the Command Palette (Ctrl+Shift+P)
3. Type and run: `MS SQL: Connect`
4. Select your Azure SQL Server: `server-incercare.database.windows.net`
5. Select database: `ShopVRG-db`
6. Select authentication: `Azure Authentication` (for passwordless Entra connection)
7. Once connected, select all the SQL script text
8. Right-click and select `Execute Query` or press Ctrl+Shift+E
9. Check the output panel for success message

### Option 2: Running via Azure Portal
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your SQL Database `ShopVRG-db`
3. Go to "Query editor (preview)"
4. Copy and paste the contents of `CreateTables.sql`
5. Click "Run" to execute

### Option 3: Using Azure Data Studio
1. Download [Azure Data Studio](https://github.com/microsoft/azuredatastudio)
2. Connect to your Azure SQL Server
3. Open `CreateTables.sql`
4. Click "Run" or press Ctrl+Shift+E

## Verify Tables Were Created

Run this query to verify all tables exist:

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;
```

Expected output:
- OrderLines
- Orders
- Payments
- Products
- Shipping

## Database Schema Overview

- **Products**: PC components catalog with pricing and stock
- **Orders**: Main order aggregate with customer and shipping info
- **OrderLines**: Line items for each order (one-to-many with Orders)
- **Payments**: Payment processing with status tracking
- **Shipping**: Shipping and tracking information

All tables include:
- Proper constraints (FK, CK, UQ)
- Audit timestamps (CreatedAt, UpdatedAt, etc.)
- Status columns for state management
- Indexes for query performance
