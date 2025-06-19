-- InventoryPro Data Clearing Script
-- WARNING: This script will DELETE ALL DATA from your databases!
-- Use this to start fresh with empty databases
-- Run this script with SQL Server Management Studio or sqlcmd

PRINT 'Starting data clearing process...';
PRINT 'WARNING: This will delete ALL data from your InventoryPro databases!';

-- Clear Auth Database
USE InventoryPro_AuthDB;
GO

PRINT 'Clearing authentication data...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    DELETE FROM Users;
    PRINT 'Cleared all users from AuthDB';
END
GO

-- Clear Product Database  
USE InventoryPro_ProductDB;
GO

PRINT 'Clearing product data...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
BEGIN
    DELETE FROM Products;
    PRINT 'Cleared all products from ProductDB';
END
GO

-- Clear Sales Database
USE InventoryPro_SalesDB;
GO

PRINT 'Clearing sales and customer data...';

-- Clear Sales first (due to foreign key constraints)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Sales')
BEGIN
    DELETE FROM Sales;
    PRINT 'Cleared all sales from SalesDB';
END

-- Clear SaleItems if exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SaleItems')
BEGIN
    DELETE FROM SaleItems;
    PRINT 'Cleared all sale items from SalesDB';
END

-- Clear Customers
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Customers')
BEGIN
    DELETE FROM Customers;
    PRINT 'Cleared all customers from SalesDB';
END
GO

-- Reset identity columns if they exist
USE InventoryPro_AuthDB;
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    DBCC CHECKIDENT ('Users', RESEED, 0);
    PRINT 'Reset Users identity column';
END
GO

USE InventoryPro_ProductDB;
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
BEGIN
    DBCC CHECKIDENT ('Products', RESEED, 0);
    PRINT 'Reset Products identity column';
END
GO

USE InventoryPro_SalesDB;
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Customers')
BEGIN
    DBCC CHECKIDENT ('Customers', RESEED, 0);
    PRINT 'Reset Customers identity column';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Sales')
BEGIN
    DBCC CHECKIDENT ('Sales', RESEED, 0);
    PRINT 'Reset Sales identity column';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SaleItems')
BEGIN
    DBCC CHECKIDENT ('SaleItems', RESEED, 0);
    PRINT 'Reset SaleItems identity column';
END
GO

PRINT '==============================================';
PRINT 'DATA CLEARING COMPLETED SUCCESSFULLY!';
PRINT 'All data has been removed from:';
PRINT '- InventoryPro_AuthDB (Users)';
PRINT '- InventoryPro_ProductDB (Products)';
PRINT '- InventoryPro_SalesDB (Sales, SaleItems, Customers)';
PRINT '==============================================';
PRINT 'Your system is now ready for fresh data input!';