using InventoryPro.ReportService.Models;

namespace InventoryPro.ReportService.Services
    {
    /// <summary>
    /// Interface for report generation service
    /// </summary>
    public interface IReportService
        {
        // Sales Reports
        Task<SalesReport> GenerateSalesReportAsync(ReportParameters parameters);
        Task<List<DailySales>> GetDailySalesAsync(DateTime startDate, DateTime endDate);
        Task<List<ProductSales>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int topCount = 10);
        Task<List<CustomerSales>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, int topCount = 10);

        // Inventory Reports
        Task<InventoryReport> GenerateInventoryReportAsync(ReportParameters parameters);
        Task<List<ProductInventory>> GetLowStockProductsAsync();
        Task<List<StockMovementReport>> GetStockMovementsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetStockLevelsByCategory();

        // Financial Reports
        Task<FinancialReport> GenerateFinancialReportAsync(ReportParameters parameters);
        Task<List<MonthlyRevenue>> GetMonthlyRevenueAsync(int year);
        Task<Dictionary<string, decimal>> GetRevenueByCategory(DateTime startDate, DateTime endDate);

        // Export Functions
        Task<byte[]> ExportReportToPdfAsync(object report, string reportType);
        Task<byte[]> ExportReportToExcelAsync(object report, string reportType);
        Task<byte[]> ExportReportToCsvAsync(object report, string reportType);

        // Dashboard Statistics
        Task<Dictionary<string, object>> GetDashboardStatisticsAsync();
        Task<List<object>> GetRecentActivitiesAsync(int count = 10);
        }
    }