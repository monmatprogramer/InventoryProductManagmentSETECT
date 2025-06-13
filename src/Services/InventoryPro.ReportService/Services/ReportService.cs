using InventoryPro.ReportService.Models;
using System.Text;
using System.Net.Http.Json;

namespace InventoryPro.ReportService.Services
    {
    /// <summary>
    /// Implementation of report generation service
    /// Aggregates data from multiple microservices to generate reports
    /// </summary>
    public class ReportService : IReportService
        {
        private readonly ILogger<ReportService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ReportService(ILogger<ReportService> logger, HttpClient httpClient, IConfiguration configuration)
            {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
            }

        #region Sales Reports

        /// <summary>
        /// Generates comprehensive sales report
        /// </summary>
        public async Task<SalesReport> GenerateSalesReportAsync(ReportParameters parameters)
            {
            try
                {
                var startDate = parameters.StartDate ?? DateTime.UtcNow.AddMonths(-1);
                var endDate = parameters.EndDate ?? DateTime.UtcNow;

                var report = new SalesReport
                    {
                    StartDate = startDate,
                    EndDate = endDate
                    };

                // Fetch real data from Sales Service
                try
                    {
                    var salesStatsResponse = await _httpClient.GetAsync($"http://localhost:5282/api/sales/stats?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
                    if (salesStatsResponse.IsSuccessStatusCode)
                        {
                        var salesStats = await salesStatsResponse.Content.ReadFromJsonAsync<dynamic>();
                        report.TotalSales = salesStats?.GetProperty("totalSales").GetDecimal() ?? 0;
                        report.TotalOrders = salesStats?.GetProperty("salesCount").GetInt32() ?? 0;
                        report.AverageOrderValue = report.TotalOrders > 0 ? report.TotalSales / report.TotalOrders : 0;
                        }
                    }
                catch (Exception ex)
                    {
                    _logger.LogWarning(ex, "Could not fetch sales stats, using fallback data");
                    }

                // Get detailed data
                report.DailySales = await GetDailySalesAsync(startDate, endDate);
                report.TopProducts = await GetTopSellingProductsAsync(startDate, endDate);
                report.TopCustomers = await GetTopCustomersAsync(startDate, endDate);

                // Try to get real category data from Product Service
                try
                    {
                    var categoriesResponse = await _httpClient.GetAsync("http://localhost:5089/api/products/categories");
                    if (categoriesResponse.IsSuccessStatusCode)
                        {
                        var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<dynamic>>();
                        report.SalesByCategory = new Dictionary<string, decimal>();
                        foreach (var category in categories ?? new List<dynamic>())
                            {
                            var categoryName = category.GetProperty("name").GetString() ?? "Unknown";
                            // This would ideally come from sales data grouped by category
                            report.SalesByCategory[categoryName] = (decimal)(new Random().Next(1000, 20000));
                            }
                        }
                    else
                        {
                        // Fallback category data
                        report.SalesByCategory = new Dictionary<string, decimal>
                            {
                            { "Electronics", 45230.50m },
                            { "Clothing", 23450.25m },
                            { "Food & Beverages", 35670.00m },
                            { "Home & Garden", 21100.00m },
                            { "Sports & Outdoors", 15000.00m }
                            };
                        }
                    }
                catch
                    {
                    report.SalesByCategory = new Dictionary<string, decimal>
                        {
                        { "Electronics", 45230.50m },
                        { "Clothing", 23450.25m },
                        { "Food & Beverages", 35670.00m },
                        { "Home & Garden", 21100.00m },
                        { "Sports & Outdoors", 15000.00m }
                        };
                    }

                report.SalesByPaymentMethod = new Dictionary<string, decimal>
                    {
                    { "Cash", report.TotalSales * 0.4m },
                    { "Credit Card", report.TotalSales * 0.35m },
                    { "Debit Card", report.TotalSales * 0.25m }
                    };

                _logger.LogInformation("Sales report generated for period {StartDate} to {EndDate}",
                    startDate, endDate);

                return report;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating sales report");
                throw;
                }
            }

        /// <summary>
        /// Gets daily sales data
        /// </summary>
        public async Task<List<DailySales>> GetDailySalesAsync(DateTime startDate, DateTime endDate)
            {
            var dailySales = new List<DailySales>();
            var random = new Random();

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                dailySales.Add(new DailySales
                    {
                    Date = date,
                    TotalAmount = (decimal)(random.Next(2000, 8000) + random.NextDouble()),
                    OrderCount = random.Next(5, 25)
                    });
                }

            return await Task.FromResult(dailySales);
            }

        /// <summary>
        /// Gets top selling products
        /// </summary>
        public async Task<List<ProductSales>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int topCount = 10)
            {
            // Mock data - in real implementation, this would aggregate from Sales Service
            var products = new List<ProductSales>
            {
                new() { ProductId = 1, ProductName = "Laptop Pro 15", SKU = "LAP-001", QuantitySold = 45, TotalRevenue = 58499.55m },
                new() { ProductId = 2, ProductName = "Wireless Mouse", SKU = "MOU-001", QuantitySold = 234, TotalRevenue = 7019.66m },
                new() { ProductId = 3, ProductName = "USB-C Cable", SKU = "CAB-001", QuantitySold = 456, TotalRevenue = 4560.00m },
                new() { ProductId = 4, ProductName = "Bluetooth Headphones", SKU = "HEA-001", QuantitySold = 89, TotalRevenue = 8900.00m },
                new() { ProductId = 5, ProductName = "Webcam HD", SKU = "WEB-001", QuantitySold = 67, TotalRevenue = 6700.00m }
            };

            return await Task.FromResult(products.OrderByDescending(p => p.TotalRevenue).Take(topCount).ToList());
            }

        /// <summary>
        /// Gets top customers by purchase amount
        /// </summary>
        public async Task<List<CustomerSales>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, int topCount = 10)
            {
            // Mock data - in real implementation, this would aggregate from Sales Service
            var customers = new List<CustomerSales>
            {
                new() { CustomerId = 2, CustomerName = "John Doe", OrderCount = 15, TotalAmount = 12340.50m },
                new() { CustomerId = 3, CustomerName = "Jane Smith", OrderCount = 8, TotalAmount = 8950.25m },
                new() { CustomerId = 4, CustomerName = "Bob Johnson", OrderCount = 12, TotalAmount = 7230.00m },
                new() { CustomerId = 5, CustomerName = "Alice Brown", OrderCount = 6, TotalAmount = 5670.75m },
                new() { CustomerId = 6, CustomerName = "Charlie Wilson", OrderCount = 9, TotalAmount = 4560.00m }
            };

            return await Task.FromResult(customers.OrderByDescending(c => c.TotalAmount).Take(topCount).ToList());
            }

        #endregion

        #region Inventory Reports

        /// <summary>
        /// Generates comprehensive inventory report
        /// </summary>
        public async Task<InventoryReport> GenerateInventoryReportAsync(ReportParameters parameters)
            {
            try
                {
                var report = new InventoryReport
                    {
                    ReportDate = DateTime.UtcNow
                    };

                // Fetch data from Product Service
                // In a real implementation, this would make HTTP calls to the Product Service

                // Mock data for demonstration
                report.TotalProducts = 150;
                report.ActiveProducts = 145;
                report.InactiveProducts = 5;
                report.LowStockProducts = 12;
                report.OutOfStockProducts = 3;
                report.TotalInventoryValue = 234567.89m;

                report.InventoryByCategory = new List<CategoryInventory>
                {
                    new() { CategoryId = 1, CategoryName = "Electronics", ProductCount = 45, TotalStock = 1234, TotalValue = 123450.00m },
                    new() { CategoryId = 2, CategoryName = "Clothing", ProductCount = 30, TotalStock = 2345, TotalValue = 45670.00m },
                    new() { CategoryId = 3, CategoryName = "Food & Beverages", ProductCount = 40, TotalStock = 3456, TotalValue = 34567.89m },
                    new() { CategoryId = 4, CategoryName = "Home & Garden", ProductCount = 35, TotalStock = 890, TotalValue = 30880.00m }
                };

                report.ProductInventoryDetails = await GetLowStockProductsAsync();

                _logger.LogInformation("Inventory report generated");

                return report;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating inventory report");
                throw;
                }
            }

        /// <summary>
        /// Gets products with low stock
        /// </summary>
        public async Task<List<ProductInventory>> GetLowStockProductsAsync()
            {
            // Mock data - in real implementation, this would fetch from Product Service
            var products = new List<ProductInventory>
            {
                new() { ProductId = 1, ProductName = "USB Cable", SKU = "USB-001", Category = "Electronics",
                        CurrentStock = 5, MinimumStock = 10, UnitPrice = 10.00m, StockValue = 50.00m, StockStatus = "Low" },
                new() { ProductId = 2, ProductName = "Mouse Pad", SKU = "PAD-001", Category = "Electronics",
                        CurrentStock = 0, MinimumStock = 20, UnitPrice = 15.00m, StockValue = 0.00m, StockStatus = "Out of Stock" },
                new() { ProductId = 3, ProductName = "T-Shirt", SKU = "TSH-001", Category = "Clothing",
                        CurrentStock = 8, MinimumStock = 15, UnitPrice = 25.00m, StockValue = 200.00m, StockStatus = "Low" }
            };

            return await Task.FromResult(products);
            }

        /// <summary>
        /// Gets stock movements for a date range
        /// </summary>
        public async Task<List<StockMovementReport>> GetStockMovementsAsync(DateTime startDate, DateTime endDate)
            {
            // Mock data - in real implementation, this would fetch from Product Service
            var movements = new List<StockMovementReport>
            {
                new() { Date = DateTime.Now.AddDays(-1), ProductName = "Laptop Pro 15", SKU = "LAP-001",
                        MovementType = "Sale", Quantity = -2, Reason = "Customer order", CreatedBy = "admin" },
                new() { Date = DateTime.Now.AddDays(-2), ProductName = "Wireless Mouse", SKU = "MOU-001",
                        MovementType = "Purchase", Quantity = 50, Reason = "Stock replenishment", CreatedBy = "admin" },
                new() { Date = DateTime.Now.AddDays(-3), ProductName = "USB Cable", SKU = "USB-001",
                        MovementType = "Adjustment", Quantity = -5, Reason = "Damaged items", CreatedBy = "admin" }
            };

            return await Task.FromResult(movements.Where(m => m.Date >= startDate && m.Date <= endDate).ToList());
            }

        /// <summary>
        /// Gets stock levels grouped by category
        /// </summary>
        public async Task<Dictionary<string, int>> GetStockLevelsByCategory()
            {
            return await Task.FromResult(new Dictionary<string, int>
            {
                { "Electronics", 1234 },
                { "Clothing", 2345 },
                { "Food & Beverages", 3456 },
                { "Home & Garden", 890 }
            });
            }

        #endregion

        #region Financial Reports

        /// <summary>
        /// Generates financial report
        /// </summary>
        public async Task<FinancialReport> GenerateFinancialReportAsync(ReportParameters parameters)
            {
            try
                {
                var startDate = parameters.StartDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
                var endDate = parameters.EndDate ?? DateTime.UtcNow;

                var report = new FinancialReport
                    {
                    StartDate = startDate,
                    EndDate = endDate,
                    GrossRevenue = 567890.50m,
                    TotalDiscounts = 12340.50m,
                    NetRevenue = 555550.00m,
                    TotalTransactions = 1234,
                    AverageTransactionValue = 450.28m
                    };

                report.MonthlyRevenue = await GetMonthlyRevenueAsync(DateTime.UtcNow.Year);
                report.RevenueByCategory = await GetRevenueByCategory(startDate, endDate);

                _logger.LogInformation("Financial report generated for period {StartDate} to {EndDate}",
                    startDate, endDate);

                return report;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating financial report");
                throw;
                }
            }

        /// <summary>
        /// Gets monthly revenue for a year
        /// </summary>
        public async Task<List<MonthlyRevenue>> GetMonthlyRevenueAsync(int year)
            {
            var monthlyRevenue = new List<MonthlyRevenue>();
            var random = new Random();
            decimal previousRevenue = 40000m;

            for (int month = 1; month <= 12; month++)
                {
                var revenue = previousRevenue + (decimal)(random.Next(-5000, 10000));
                var growth = previousRevenue > 0 ? ((revenue - previousRevenue) / previousRevenue) * 100 : 0;

                monthlyRevenue.Add(new MonthlyRevenue
                    {
                    Year = year,
                    Month = month,
                    Revenue = revenue,
                    TransactionCount = random.Next(80, 150),
                    Growth = Math.Round(growth, 2)
                    });

                previousRevenue = revenue;
                }

            return await Task.FromResult(monthlyRevenue);
            }

        /// <summary>
        /// Gets revenue by category
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetRevenueByCategory(DateTime startDate, DateTime endDate)
            {
            return await Task.FromResult(new Dictionary<string, decimal>
            {
                { "Electronics", 234567.89m },
                { "Clothing", 123456.78m },
                { "Food & Beverages", 98765.43m },
                { "Home & Garden", 76543.21m },
                { "Sports & Outdoors", 34567.89m }
            });
            }

        #endregion

        #region Export Functions

        /// <summary>
        /// Exports report to PDF format
        /// </summary>
        public async Task<byte[]> ExportReportToPdfAsync(object report, string reportType)
            {
            try
                {
                // In a real implementation, this would use a PDF library like iTextSharp or similar
                // For now, return mock PDF data
                var pdfContent = $"PDF Report - {reportType}\n" +
                                $"Generated at: {DateTime.Now}\n" +
                                $"Report Data: {System.Text.Json.JsonSerializer.Serialize(report)}";

                return await Task.FromResult(Encoding.UTF8.GetBytes(pdfContent));
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error exporting report to PDF");
                throw;
                }
            }

        /// <summary>
        /// Exports report to Excel format
        /// </summary>
        public async Task<byte[]> ExportReportToExcelAsync(object report, string reportType)
            {
            try
                {
                // In a real implementation, this would use an Excel library like EPPlus or similar
                // For now, return mock Excel data
                var excelContent = $"Excel Report - {reportType}\n" +
                                  $"Generated at: {DateTime.Now}\n" +
                                  $"Report Data: {System.Text.Json.JsonSerializer.Serialize(report)}";

                return await Task.FromResult(Encoding.UTF8.GetBytes(excelContent));
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error exporting report to Excel");
                throw;
                }
            }

        /// <summary>
        /// Exports report to CSV format
        /// </summary>
        public async Task<byte[]> ExportReportToCsvAsync(object report, string reportType)
            {
            try
                {
                // In a real implementation, this would properly format CSV data
                // For now, return mock CSV data
                var csvContent = $"Report Type,Generated At,Data\n" +
                                $"{reportType},{DateTime.Now},{System.Text.Json.JsonSerializer.Serialize(report)}";

                return await Task.FromResult(Encoding.UTF8.GetBytes(csvContent));
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error exporting report to CSV");
                throw;
                }
            }

        #endregion

        #region Dashboard Statistics

        /// <summary>
        /// Gets dashboard statistics
        /// </summary>
        public async Task<Dictionary<string, object>> GetDashboardStatisticsAsync()
            {
            var stats = new Dictionary<string, object>
            {
                { "TotalProducts", 150 },
                { "LowStockProducts", 12 },
                { "TotalCustomers", 1250 },
                { "TodaySales", 4567.89m },
                { "MonthSales", 123456.78m },
                { "YearSales", 1234567.89m },
                { "PendingOrders", 5 },
                { "CompletedOrders", 342 }
            };

            return await Task.FromResult(stats);
            }

        /// <summary>
        /// Gets recent activities
        /// </summary>
        public async Task<List<object>> GetRecentActivitiesAsync(int count = 10)
            {
            var activities = new List<object>
            {
                new { Type = "Sale", Description = "New sale #1234 - $456.78", Time = DateTime.Now.AddMinutes(-5) },
                new { Type = "Product", Description = "Product 'Laptop Pro 15' stock updated", Time = DateTime.Now.AddMinutes(-15) },
                new { Type = "Customer", Description = "New customer 'John Doe' registered", Time = DateTime.Now.AddMinutes(-30) },
                new { Type = "Alert", Description = "Low stock alert for 'USB Cable'", Time = DateTime.Now.AddHours(-1) },
                new { Type = "Sale", Description = "Sale #1233 completed", Time = DateTime.Now.AddHours(-2) }
            };

            return await Task.FromResult(activities.Take(count).ToList());
            }

        #endregion
        }
    }