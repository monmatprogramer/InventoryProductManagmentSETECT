using InventoryPro.ReportService.Models;
using InventoryPro.ReportService.Data;
using System.Text.Json;
using InventoryPro.Shared.DTOs;
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
        private readonly RealDataService _realDataService;
        private readonly ReportDbContext _reportDbContext;

        public ReportService(ILogger<ReportService> logger, HttpClient httpClient, IConfiguration configuration, ReportDbContext reportDbContext)
            {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
            _reportDbContext = reportDbContext;
            _realDataService = new RealDataService(httpClient,
                logger);
            }

        #region Sales Reports

        /// <summary>
        /// Generates comprehensive sales report with real data
        /// </summary>
        public async Task<SalesReport> GenerateSalesReportAsync(ReportParameters parameters)
            {
            try
                {
                var startDate = parameters.StartDate ?? DateTime.UtcNow.AddMonths(-1);
                var endDate = parameters.EndDate ?? DateTime.UtcNow;

                _logger.LogInformation("Generating sales report for period {StartDate} to {EndDate}", startDate, endDate);

                // Fetch real data from microservices
                var sales = await _realDataService.GetRealSalesDataAsync(startDate, endDate);
                var customers = await _realDataService.GetRealCustomersDataAsync();

                // Calculate totals from real data
                var completedSales = sales.Where(s => s.Status == "Completed").ToList();
                var totalSales = completedSales.Sum(s => s.TotalAmount);
                var totalOrders = completedSales.Count;
                var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

                var report = new SalesReport
                    {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalSales = totalSales,
                    TotalOrders = totalOrders,
                    AverageOrderValue = averageOrderValue,
                    DailySales = _realDataService.ProcessDailySalesData(sales, startDate, endDate),
                    TopProducts = await _realDataService.ProcessTopProductsDataAsync(sales),
                    TopCustomers = _realDataService.ProcessTopCustomersData(sales, customers),
                    SalesByCategory = await _realDataService.ProcessSalesByCategoryAsync(sales),
                    SalesByPaymentMethod = _realDataService.ProcessSalesByPaymentMethod(sales)
                    };

                _logger.LogInformation("Sales report generated successfully with {SalesCount} sales records", sales.Count);
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
        /// Generates comprehensive inventory report with real data
        /// </summary>
        public async Task<InventoryReport> GenerateInventoryReportAsync(ReportParameters parameters)
            {
            try
                {
                _logger.LogInformation("Generating inventory report");

                // Fetch real data from Product Service
                var products = await _realDataService.GetRealProductsDataAsync();
                var inventoryByCategory = await _realDataService.ProcessInventoryByCategoryAsync();
                var lowStockProducts = await _realDataService.ProcessLowStockProductsAsync();

                // Calculate real statistics
                var activeProducts = products.Where(p => p.IsActive).ToList();
                var inactiveProducts = products.Where(p => !p.IsActive).ToList();
                var lowStock = products.Where(p => p.Stock <= p.MinStock && p.Stock > 0).ToList();
                var outOfStock = products.Where(p => p.Stock == 0).ToList();
                var totalValue = products.Sum(p => p.Stock * p.Price);

                var report = new InventoryReport
                    {
                    ReportDate = DateTime.UtcNow,
                    TotalProducts = products.Count,
                    ActiveProducts = activeProducts.Count,
                    InactiveProducts = inactiveProducts.Count,
                    LowStockProducts = lowStock.Count,
                    OutOfStockProducts = outOfStock.Count,
                    TotalInventoryValue = totalValue,
                    InventoryByCategory = inventoryByCategory,
                    ProductInventoryDetails = lowStockProducts
                    };

                _logger.LogInformation("Inventory report generated successfully with {ProductCount} products", products.Count);
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
        /// Generates financial report with real data
        /// </summary>
        public async Task<FinancialReport> GenerateFinancialReportAsync(ReportParameters parameters)
            {
            try
                {
                var startDate = parameters.StartDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
                var endDate = parameters.EndDate ?? DateTime.UtcNow;

                _logger.LogInformation("Generating financial report for period {StartDate} to {EndDate}", startDate, endDate);

                // Fetch real sales data
                var sales = await _realDataService.GetRealSalesDataAsync(startDate, endDate);
                var completedSales = sales.Where(s => s.Status == "Completed").ToList();

                // Calculate real financial metrics
                var grossRevenue = completedSales.Sum(s => s.TotalAmount);
                var totalDiscounts = completedSales.Sum(s => s.Items.Sum(i => i.DiscountAmount * i.Quantity));
                var netRevenue = grossRevenue - totalDiscounts;
                var averageTransactionValue = completedSales.Any() ? grossRevenue / completedSales.Count : 0;

                var report = new FinancialReport
                    {
                    StartDate = startDate,
                    EndDate = endDate,
                    GrossRevenue = grossRevenue,
                    TotalDiscounts = totalDiscounts,
                    NetRevenue = netRevenue,
                    TotalTransactions = completedSales.Count,
                    AverageTransactionValue = averageTransactionValue,
                    MonthlyRevenue = _realDataService.ProcessMonthlyRevenueData(sales, endDate.Year),
                    RevenueByCategory = await _realDataService.ProcessSalesByCategoryAsync(sales)
                    };

                _logger.LogInformation("Financial report generated successfully with {TransactionCount} transactions",
                    completedSales.Count);
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
                return await Task.Run(() =>
                {
                    return reportType.ToLower() switch
                        {
                            "sales report" when report is SalesReport salesReport =>
                                PdfGenerator.GenerateSalesReportPdf(salesReport),
                            "inventory report" when report is InventoryReport inventoryReport =>
                                PdfGenerator.GenerateInventoryReportPdf(inventoryReport),
                            "financial report" when report is FinancialReport financialReport =>
                                PdfGenerator.GenerateFinancialReportPdf(financialReport),
                            "custom report" when report is CustomReport customReport =>
                                PdfGenerator.GenerateCustomReportPdf(customReport),
                            _ => GenerateFallbackPdf(report, reportType)
                            };
                });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error exporting report to PDF");
                throw;
                }
            }

        private byte[] GenerateFallbackPdf(object report, string reportType)
            {
            try
            {
                // Create a simple text-based report
                var pdfContent = $"InventoryPro Report\n" +
                                $"====================\n\n" +
                                $"Report Type: {reportType}\n" +
                                $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                                $"Data Summary:\n" +
                                $"{ReportHelpers.GetSimpleReportSummary(report, reportType, _logger)}\n\n" +
                                $"Note: This is a simplified text-based report generated due to PDF library limitations.\n" +
                                $"For full featured reports, please ensure all required libraries are properly installed.";
                
                return Encoding.UTF8.GetBytes(pdfContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating fallback PDF");
                var errorContent = $"Report Generation Error\n" +
                                  $"Report Type: {reportType}\n" +
                                  $"Generated: {DateTime.Now}\n" +
                                  $"Error: {ex.Message}";
                return Encoding.UTF8.GetBytes(errorContent);
            }
            }

        /// <summary>
        /// Exports report to Excel format
        /// </summary>
        public async Task<byte[]> ExportReportToExcelAsync(object report, string reportType)
            {
            try
                {
                return await Task.Run(() =>
                {
                    return reportType.ToLower() switch
                        {
                            "sales report" when report is SalesReport salesReport =>
                                ExcelGenerator.GenerateSalesReportExcel(salesReport),
                            "inventory report" when report is InventoryReport inventoryReport =>
                                ExcelGenerator.GenerateInventoryReportExcel(inventoryReport),
                            "financial report" when report is FinancialReport financialReport =>
                                ExcelGenerator.GenerateFinancialReportExcel(financialReport),
                            "custom report" when report is CustomReport customReport =>
                                ExcelGenerator.GenerateCustomReportExcel(customReport),
                            _ => GenerateFallbackExcel(report, reportType)
                            };
                });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error exporting report to Excel");
                throw;
                }
            }

        private byte[] GenerateFallbackExcel(object report, string reportType)
            {
            try
            {
                // Create a simple CSV-style report that can be opened in Excel
                var csvContent = $"InventoryPro Report - {reportType}\n" +
                               $"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                               $"{ReportHelpers.GetCsvReportData(report, reportType, _logger)}\n\n" +
                               $"Note: This is a simplified CSV report due to Excel library limitations.";
                
                return Encoding.UTF8.GetBytes(csvContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating fallback Excel");
                var errorContent = $"Report Generation Error\n" +
                                  $"Report Type: {reportType}\n" +
                                  $"Generated: {DateTime.Now}\n" +
                                  $"Error: {ex.Message}";
                return Encoding.UTF8.GetBytes(errorContent);
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

        #region Custom Reports

        /// <summary>
        /// Generates custom report based on user-defined parameters
        /// </summary>
        public async Task<CustomReport> GenerateCustomReportAsync(CustomReportParameters parameters)
            {
            try
                {
                _logger.LogInformation("Generating custom report '{Title}' for period {StartDate} to {EndDate}",
                    parameters.ReportTitle, parameters.StartDate, parameters.EndDate);

                var customReport = new CustomReport
                    {
                    Title = parameters.ReportTitle,
                    StartDate = parameters.StartDate,
                    EndDate = parameters.EndDate,
                    GeneratedAt = DateTime.UtcNow
                    };

                // Fetch base data
                var sales = await _realDataService.GetRealSalesDataAsync(parameters.StartDate, parameters.EndDate);
                var customers = await _realDataService.GetRealCustomersDataAsync();

                // Apply filters
                var filteredSales = ApplyCustomFilters(sales, parameters);
                var completedSales = filteredSales.Where(s => s.Status == parameters.SalesStatus).ToList();

                // Calculate summary metrics
                customReport.TotalRevenue = completedSales.Sum(s => s.TotalAmount);
                customReport.TotalTransactions = completedSales.Count;
                customReport.AverageTransactionValue = completedSales.Any() ?
                    customReport.TotalRevenue / completedSales.Count : 0;
                customReport.UniqueCustomers = completedSales.Select(s => s.CustomerId).Distinct().Count();
                customReport.ProductsSold = completedSales.SelectMany(s => s.Items).Sum(i => i.Quantity);

                // Generate filtered data based on selections
                if (parameters.IncludeDailySales)
                    {
                    customReport.FilteredDailySales = _realDataService.ProcessDailySalesData(
                        filteredSales, parameters.StartDate, parameters.EndDate);
                    }

                if (parameters.IncludeTopProducts)
                    {
                    customReport.FilteredTopProducts = (await _realDataService.ProcessTopProductsDataAsync(filteredSales))
                        .Take(parameters.TopProductsCount).ToList();
                    }

                if (parameters.IncludeTopCustomers)
                    {
                    customReport.FilteredTopCustomers = _realDataService.ProcessTopCustomersData(filteredSales, customers)
                        .Take(parameters.TopCustomersCount).ToList();
                    }

                if (parameters.IncludeSalesByCategory)
                    {
                    customReport.FilteredSalesByCategory = await _realDataService.ProcessSalesByCategoryAsync(filteredSales);
                    }

                // Include full reports if requested
                if (parameters.IncludeSalesOverview)
                    {
                    var salesParams = new ReportParameters
                        {
                        StartDate = parameters.StartDate,
                        EndDate = parameters.EndDate
                        };
                    customReport.SalesData = await GenerateSalesReportAsync(salesParams);
                    }

                if (parameters.IncludeInventoryStatus)
                    {
                    var inventoryParams = new ReportParameters();
                    customReport.InventoryData = await GenerateInventoryReportAsync(inventoryParams);
                    }

                if (parameters.IncludeFinancialSummary)
                    {
                    var financialParams = new ReportParameters
                        {
                        StartDate = parameters.StartDate,
                        EndDate = parameters.EndDate
                        };
                    customReport.FinancialData = await GenerateFinancialReportAsync(financialParams);
                    }

                _logger.LogInformation("Custom report '{Title}' generated successfully with {TransactionCount} transactions",
                    parameters.ReportTitle, customReport.TotalTransactions);

                return customReport;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating custom report");
                throw;
                }
            }

        private List<SaleDto> ApplyCustomFilters(List<SaleDto> sales, CustomReportParameters parameters)
            {
            var filtered = sales.AsQueryable();

            // Filter by selected customers
            if (parameters.SelectedCustomers.Any())
                {
                filtered = filtered.Where(s => parameters.SelectedCustomers.Contains(s.CustomerId));
                }

            // Filter by sales amount range
            if (parameters.MinSalesAmount.HasValue)
                {
                filtered = filtered.Where(s => s.TotalAmount >= parameters.MinSalesAmount.Value);
                }

            if (parameters.MaxSalesAmount.HasValue)
                {
                filtered = filtered.Where(s => s.TotalAmount <= parameters.MaxSalesAmount.Value);
                }

            // Filter by payment method
            if (!string.IsNullOrEmpty(parameters.PaymentMethod))
                {
                filtered = filtered.Where(s => s.PaymentMethod.Equals(parameters.PaymentMethod, StringComparison.OrdinalIgnoreCase));
                }

            // Filter by products (if sale contains any of the selected products)
            if (parameters.SelectedProducts.Any())
                {
                filtered = filtered.Where(s => s.Items.Any(i => parameters.SelectedProducts.Contains(i.ProductId)));
                }

            return filtered.ToList();
            }

        #endregion

        #region Report Data for WinForms Client

        /// <summary>
        /// Gets sales report data for viewing in WinForms client and saves to database
        /// </summary>
        public async Task<object> GetSalesReportDataAsync(DateTime startDate, DateTime endDate)
            {
            try
                {
                _logger.LogInformation("Getting sales report data for period {StartDate} to {EndDate}", startDate, endDate);

                // Generate the full sales report with real data
                var reportParameters = new ReportParameters
                    {
                    StartDate = startDate,
                    EndDate = endDate,
                    Format = "View"
                    };

                var report = await GenerateSalesReportAsync(reportParameters);

                // Create data for WinForms client
                var viewData = new
                    {
                    TotalSales = report.TotalSales,
                    TotalOrders = report.TotalOrders,
                    AverageOrderValue = report.AverageOrderValue,
                    DailySales = report.DailySales,
                    TopProducts = report.TopProducts,
                    TopCustomers = report.TopCustomers,
                    SalesByCategory = report.SalesByCategory,
                    SalesByPaymentMethod = report.SalesByPaymentMethod,
                    ReportDate = DateTime.UtcNow
                    };

                // Save to database
                await SaveReportToDatabase("Sales", "View", "Sales Report", startDate, endDate, reportParameters, viewData);

                return viewData;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting sales report data");
                throw;
                }
            }

        /// <summary>
        /// Gets inventory report data for viewing in WinForms client and saves to database
        /// </summary>
        public async Task<object> GetInventoryReportDataAsync()
            {
            try
                {
                _logger.LogInformation("Getting inventory report data");

                // Generate the full inventory report with real data
                var reportParameters = new ReportParameters
                    {
                    Format = "View"
                    };

                var report = await GenerateInventoryReportAsync(reportParameters);

                // Create data for WinForms client
                var viewData = new
                    {
                    TotalProducts = report.TotalProducts,
                    ActiveProducts = report.ActiveProducts,
                    InactiveProducts = report.InactiveProducts,
                    LowStockProducts = report.LowStockProducts,
                    OutOfStockProducts = report.OutOfStockProducts,
                    TotalInventoryValue = report.TotalInventoryValue,
                    InventoryByCategory = report.InventoryByCategory,
                    ProductInventoryDetails = report.ProductInventoryDetails,
                    ReportDate = report.ReportDate
                    };

                // Save to database
                await SaveReportToDatabase("Inventory", "View", "Inventory Report", null, null, reportParameters, viewData);

                return viewData;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting inventory report data");
                throw;
                }
            }

        /// <summary>
        /// Gets financial report data for viewing in WinForms client and saves to database
        /// </summary>
        public async Task<object> GetFinancialReportDataAsync(DateTime startDate, DateTime endDate)
            {
            try
                {
                _logger.LogInformation("Getting financial report data for period {StartDate} to {EndDate}", startDate, endDate);

                // Generate the full financial report with real data
                var reportParameters = new ReportParameters
                    {
                    StartDate = startDate,
                    EndDate = endDate,
                    Format = "View"
                    };

                var report = await GenerateFinancialReportAsync(reportParameters);

                // Create data for WinForms client
                var viewData = new
                    {
                    MonthlyRevenue = report.MonthlyRevenue,
                    GrossRevenue = report.GrossRevenue,
                    NetRevenue = report.NetRevenue,
                    TotalDiscounts = report.TotalDiscounts,
                    TotalTransactions = report.TotalTransactions,
                    AverageTransactionValue = report.AverageTransactionValue,
                    RevenueByCategory = report.RevenueByCategory,
                    StartDate = report.StartDate,
                    EndDate = report.EndDate
                    };

                // Save to database
                await SaveReportToDatabase("Financial", "View", "Financial Report", startDate, endDate, reportParameters, viewData);

                return viewData;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting financial report data");
                throw;
                }
            }

        /// <summary>
        /// Saves report metadata and view data to the database
        /// </summary>
        private async Task SaveReportToDatabase(string reportType, string format, string title, DateTime? startDate, DateTime? endDate, object parameters, object viewData)
            {
            try
                {
                var reportRecord = new ReportRecord
                    {
                    ReportType = reportType,
                    Format = format,
                    Title = title,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System", // In real app, get from user context
                    StartDate = startDate,
                    EndDate = endDate,
                    Parameters = JsonSerializer.Serialize(parameters),
                    ViewData = JsonSerializer.Serialize(viewData),
                    Status = "Generated",
                    ExpiresAt = DateTime.UtcNow.AddDays(30) // Reports expire after 30 days
                    };

                // Calculate record count and total amount based on report type
                if (reportType == "Sales" && viewData is object salesData)
                    {
                    var totalSales = GetPropertyValue<decimal>(salesData, "TotalSales", 0m);
                    var totalOrders = GetPropertyValue<int>(salesData, "TotalOrders", 0);
                    reportRecord.RecordCount = totalOrders;
                    reportRecord.TotalAmount = totalSales;
                    }

                _reportDbContext.ReportRecords.Add(reportRecord);
                await _reportDbContext.SaveChangesAsync();

                _logger.LogInformation("Report saved to database with ID {ReportId}", reportRecord.Id);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error saving report to database");
                // Don't throw - report generation should succeed even if database save fails
                }
            }

        /// <summary>
        /// Helper method to safely extract property values from objects
        /// </summary>
        private T GetPropertyValue<T>(object obj, string propertyName, T defaultValue)
            {
            try
                {
                if (obj == null) return defaultValue;

                var property = obj.GetType().GetProperty(propertyName);
                if (property != null)
                    {
                    var value = property.GetValue(obj);
                    if (value != null && value is T tValue)
                        return tValue;
                    if (value != null)
                        return (T)Convert.ChangeType(value, typeof(T));
                    }

                return defaultValue;
                }
            catch
                {
                return defaultValue;
                }
            }

        #endregion
        }
    }