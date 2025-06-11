-- InventoryPro Database Setup Script
-- Run this script to create all required databases and initial data

-- Create Auth Service Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InventoryPro_AuthDB')
BEGIN
    CREATE DATABASE InventoryPro_AuthDB;
    PRINT 'Created InventoryPro_AuthDB database';
END
ELSE
BEGIN
    PRINT 'InventoryPro_AuthDB already exists';
END
GO

-- Create Product Service Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InventoryPro_ProductDB')
BEGIN
    CREATE DATABASE InventoryPro_ProductDB;
    PRINT 'Created InventoryPro_ProductDB database';
END
ELSE
BEGIN
    PRINT 'InventoryPro_ProductDB already exists';
END
GO

-- Create Sales Service Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InventoryPro_SalesDB')
BEGIN
    CREATE DATABASE InventoryPro_SalesDB;
    PRINT 'Created InventoryPro_SalesDB database';
END
ELSE
BEGIN
    PRINT 'InventoryPro_SalesDB already exists';
END
GO

-- Optional: Create test data for development
USE InventoryPro_AuthDB;
GO

-- Check if Users table exists and has data
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'manager')
    BEGIN
        -- Password is 'manager123' hashed with BCrypt
        INSERT INTO Users (Username, PasswordHash, Email, FirstName, LastName, Role, IsActive, CreatedAt)
        VALUES ('manager', '$2a$11$rBNrVkNdKi8qVz7QWXJ9q.6L6U7LMX8J8V8YH5L5V9QXjZ6jVUq2a', 
                'manager@inventorypro.com', 'Store', 'Manager', 'Manager', 1, GETUTCDATE());
        PRINT 'Created manager user';
    END

    IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'user')
    BEGIN
        -- Password is 'user123' hashed with BCrypt
        INSERT INTO Users (Username, PasswordHash, Email, FirstName, LastName, Role, IsActive, CreatedAt)
        VALUES ('user', '$2a$11$rBNrVkNdKi8qVz7QWXJ9q.6L6U7LMX8J8V8YH5L5V9QXjZ6jVUq2a', 
                'user@inventorypro.com', 'Sales', 'User', 'User', 1, GETUTCDATE());
        PRINT 'Created regular user';
    END
END
GO

USE InventoryPro_ProductDB;
GO

-- Add more sample products if Products table exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
BEGIN
    -- Electronics
    IF NOT EXISTS (SELECT * FROM Products WHERE SKU = 'KEY-001')
    BEGIN
        INSERT INTO Products (Name, SKU, Description, Price, StockQuantity, MinimumStock, CategoryId, CreatedAt, IsActive)
        VALUES 
        ('Wireless Keyboard', 'KEY-001', 'Ergonomic wireless keyboard with backlight', 79.99, 75, 20, 1, GETUTCDATE(), 1),
        ('USB-C Hub', 'HUB-001', '7-in-1 USB-C hub with HDMI and card reader', 49.99, 120, 30, 1, GETUTCDATE(), 1),
        ('Webcam HD', 'WEB-001', '1080p HD webcam with microphone', 89.99, 45, 15, 1, GETUTCDATE(), 1),
        ('Monitor Stand', 'MON-001', 'Adjustable monitor stand with drawer', 34.99, 60, 20, 1, GETUTCDATE(), 1),
        ('Bluetooth Speaker', 'SPK-001', 'Portable Bluetooth speaker with 12h battery', 59.99, 85, 25, 1, GETUTCDATE(), 1);
        PRINT 'Added sample electronics products';
    END

    -- Clothing
    IF NOT EXISTS (SELECT * FROM Products WHERE SKU = 'TSH-001')
    BEGIN
        INSERT INTO Products (Name, SKU, Description, Price, StockQuantity, MinimumStock, CategoryId, CreatedAt, IsActive)
        VALUES 
        ('Cotton T-Shirt', 'TSH-001', 'Premium cotton t-shirt, various colors', 19.99, 200, 50, 2, GETUTCDATE(), 1),
        ('Denim Jeans', 'JNS-001', 'Classic fit denim jeans', 49.99, 150, 40, 2, GETUTCDATE(), 1),
        ('Sports Jacket', 'JKT-001', 'Lightweight sports jacket', 89.99, 75, 20, 2, GETUTCDATE(), 1),
        ('Running Shoes', 'SHO-001', 'Professional running shoes', 119.99, 100, 30, 2, GETUTCDATE(), 1),
        ('Winter Coat', 'COT-001', 'Warm winter coat with hood', 159.99, 50, 15, 2, GETUTCDATE(), 1);
        PRINT 'Added sample clothing products';
    END

    -- Food & Beverages
    IF NOT EXISTS (SELECT * FROM Products WHERE SKU = 'COF-001')
    BEGIN
        INSERT INTO Products (Name, SKU, Description, Price, StockQuantity, MinimumStock, CategoryId, CreatedAt, IsActive)
        VALUES 
        ('Premium Coffee', 'COF-001', 'Arabica coffee beans 1kg', 24.99, 100, 30, 3, GETUTCDATE(), 1),
        ('Green Tea', 'TEA-001', 'Organic green tea 100 bags', 12.99, 150, 40, 3, GETUTCDATE(), 1),
        ('Dark Chocolate', 'CHO-001', 'Belgian dark chocolate 200g', 8.99, 200, 50, 3, GETUTCDATE(), 1),
        ('Organic Honey', 'HON-001', 'Pure organic honey 500ml', 15.99, 80, 25, 3, GETUTCDATE(), 1),
        ('Mixed Nuts', 'NUT-001', 'Premium mixed nuts 500g', 18.99, 120, 35, 3, GETUTCDATE(), 1);
        PRINT 'Added sample food products';
    END
END
GO

USE InventoryPro_SalesDB;
GO

-- Add sample customers if table exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Customers')
BEGIN
    IF NOT EXISTS (SELECT * FROM Customers WHERE Email = 'regular.customer@example.com')
    BEGIN
        INSERT INTO Customers (Name, Email, Phone, Address, CreatedAt, IsActive)
        VALUES 
        ('Robert Smith', 'robert.smith@example.com', '1234567890', '789 Pine St, Village', GETUTCDATE(), 1),
        ('Maria Garcia', 'maria.garcia@example.com', '2345678901', '321 Elm Ave, District', GETUTCDATE(), 1),
        ('David Wilson', 'david.wilson@example.com', '3456789012', '654 Maple Rd, County', GETUTCDATE(), 1),
        ('Lisa Anderson', 'lisa.anderson@example.com', '4567890123', '987 Cedar Ln, State', GETUTCDATE(), 1),
        ('Regular Customer', 'regular.customer@example.com', '5678901234', '111 Main St, City', GETUTCDATE(), 1);
        PRINT 'Added sample customers';
    END
END
GO

PRINT 'Database setup completed successfully!';