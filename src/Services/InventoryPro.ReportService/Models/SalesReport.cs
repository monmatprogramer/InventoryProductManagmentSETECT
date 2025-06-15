using System.ComponentModel.DataAnnotations;

namespace InventoryPro.ReportService.Models
    {
    /// <summary>
    /// Sales report model
    /// </summary>
    public class SalesReport
        {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailySales> DailySales { get; set; } = new();
        public List<ProductSales> TopProducts { get; set; } = new();
        public List<CustomerSales> TopCustomers { get; set; } = new();
        public Dictionary<string, decimal> SalesByCategory { get; set; } = new();
        public Dictionary<string, decimal> SalesByPaymentMethod { get; set; } = new();
        }

    /// <summary>
    /// Daily sales data
    /// </summary>
    public class DailySales
        {
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }
        }

    /// <summary>
    /// Product sales data
    /// </summary>
    public class ProductSales
        {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        }

    /// <summary>
    /// Customer sales data
    /// </summary>
    public class CustomerSales
        {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
        }

    /// <summary>
    /// Inventory report model
    /// </summary>
    public class InventoryReport
        {
        public DateTime ReportDate { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<CategoryInventory> InventoryByCategory { get; set; } = new();
        public List<StockMovementReport> RecentStockMovements { get; set; } = new();
        public List<ProductInventory> ProductInventoryDetails { get; set; } = new();
        }

    /// <summary>
    /// Category inventory data
    /// </summary>
    public class CategoryInventory
        {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalValue { get; set; }
        }

    /// <summary>
    /// Stock movement report data
    /// </summary>
    public class StockMovementReport
        {
        public DateTime Date { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        }

    /// <summary>
    /// Product inventory details
    /// </summary>
    public class ProductInventory
        {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal StockValue { get; set; }
        public string StockStatus { get; set; } = string.Empty; // Normal, Low, Out of Stock
        }

    /// <summary>
    /// Financial report model
    /// </summary>
    public class FinancialReport
        {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal NetRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public List<MonthlyRevenue> MonthlyRevenue { get; set; } = new();
        public Dictionary<string, decimal> RevenueByCategory { get; set; } = new();
        }

    /// <summary>
    /// Monthly revenue data
    /// </summary>
    public class MonthlyRevenue
        {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
        public decimal Growth { get; set; } // Percentage growth from previous month
        }

    /// <summary>
    /// Report parameters
    /// </summary>
    public class ReportParameters
        {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportType { get; set; } = "Sales"; // Sales, Inventory, Financial, Custom
        public string Format { get; set; } = "PDF"; // PDF, Excel, CSV
        public bool IncludeDetails { get; set; } = true;
        public List<string> SelectedCategories { get; set; } = new();
        public List<int> SelectedProducts { get; set; } = new();
        public List<int> SelectedCustomers { get; set; } = new();
        }

    /// <summary>
    /// Custom report parameters with flexible options
    /// </summary>
    public class CustomReportParameters
        {
        public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.UtcNow;
        public string Format { get; set; } = "PDF"; // PDF, Excel, CSV
        public string ReportTitle { get; set; } = "Custom Report";
        
        // Report sections to include
        public bool IncludeSalesOverview { get; set; } = true;
        public bool IncludeDailySales { get; set; } = true;
        public bool IncludeTopProducts { get; set; } = true;
        public bool IncludeTopCustomers { get; set; } = true;
        public bool IncludeSalesByCategory { get; set; } = true;
        public bool IncludeInventoryStatus { get; set; } = false;
        public bool IncludeFinancialSummary { get; set; } = false;
        
        // Filtering options
        public List<int> SelectedCategories { get; set; } = new();
        public List<int> SelectedProducts { get; set; } = new();
        public List<int> SelectedCustomers { get; set; } = new();
        public decimal? MinSalesAmount { get; set; }
        public decimal? MaxSalesAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? SalesStatus { get; set; } = "Completed";
        
        // Chart options
        public bool IncludeCharts { get; set; } = true;
        public int TopProductsCount { get; set; } = 10;
        public int TopCustomersCount { get; set; } = 10;
        }

    /// <summary>
    /// Custom report result
    /// </summary>
    public class CustomReport
        {
        public string Title { get; set; } = "Custom Report";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        
        // Sales data
        public SalesReport? SalesData { get; set; }
        public InventoryReport? InventoryData { get; set; }
        public FinancialReport? FinancialData { get; set; }
        
        // Summary metrics
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public int UniqueCustomers { get; set; }
        public int ProductsSold { get; set; }
        
        // Filtered data based on custom parameters
        public List<DailySales> FilteredDailySales { get; set; } = new();
        public List<ProductSales> FilteredTopProducts { get; set; } = new();
        public List<CustomerSales> FilteredTopCustomers { get; set; } = new();
        public Dictionary<string, decimal> FilteredSalesByCategory { get; set; } = new();
        }
    }