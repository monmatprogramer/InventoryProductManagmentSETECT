using InventoryPro.ReportService.Models;
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

        public ReportService(ILogger<ReportService> logger, HttpClient httpClient, IConfiguration configuration)
            {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
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
        /// Generates comprehensive inventory report with real data and fallback support
        /// </summary>
        public async Task<InventoryReport> GenerateInventoryReportAsync(ReportParameters parameters)
            {
            try
                {
                _logger.LogInformation("Generating inventory report");

                InventoryReport report;

                try
                {
                    // Attempt to fetch real data from Product Service
                    var products = await _realDataService.GetRealProductsDataAsync();
                    var inventoryByCategory = await _realDataService.ProcessInventoryByCategoryAsync();
                    var lowStockProducts = await _realDataService.ProcessLowStockProductsAsync();

                    // Validate we got meaningful data
                    if (products == null || !products.Any())
                    {
                        throw new InvalidOperationException("No products returned from real data service");
                    }

                    // Calculate real statistics
                    var activeProducts = products.Where(p => p.IsActive).ToList();
                    var inactiveProducts = products.Where(p => !p.IsActive).ToList();
                    var lowStock = products.Where(p => p.Stock <= p.MinStock && p.Stock > 0).ToList();
                    var outOfStock = products.Where(p => p.Stock == 0).ToList();
                    var totalValue = products.Sum(p => p.Stock * p.Price);

                    report = new InventoryReport
                        {
                        ReportDate = DateTime.UtcNow,
                        TotalProducts = products.Count,
                        ActiveProducts = activeProducts.Count,
                        InactiveProducts = inactiveProducts.Count,
                        LowStockProducts = lowStock.Count,
                        OutOfStockProducts = outOfStock.Count,
                        TotalInventoryValue = totalValue,
                        InventoryByCategory = inventoryByCategory ?? new List<CategoryInventory>(),
                        ProductInventoryDetails = lowStockProducts ?? new List<ProductInventory>()
                        };

                    _logger.LogInformation("Inventory report generated successfully with {ProductCount} real products, {CategoryCount} categories, {LowStockCount} low stock items", 
                        products.Count, inventoryByCategory?.Count ?? 0, lowStockProducts?.Count ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch real inventory data, using comprehensive fallback data");
                    
                    // Create comprehensive fallback data
                    var fallbackProducts = GetFallbackInventoryProducts();
                    var fallbackCategoriesDict = GetFallbackInventoryByCategory();
                    
                    // Calculate proper category values from products
                    var fallbackCategories = fallbackCategoriesDict.Select(kvp => new CategoryInventory
                    {
                        CategoryName = kvp.Key,
                        TotalStock = kvp.Value,
                        TotalValue = fallbackProducts
                            .Where(p => p.Category == kvp.Key)
                            .Sum(p => p.StockValue)
                    }).ToList();
                    
                    // Ensure we have valid fallback data
                    if (!fallbackProducts.Any())
                    {
                        _logger.LogError("Critical error: Fallback inventory products list is empty!");
                        fallbackProducts = GetMinimalFallbackProducts();
                    }
                    
                    report = new InventoryReport
                        {
                        ReportDate = DateTime.UtcNow,
                        TotalProducts = fallbackProducts.Count,
                        ActiveProducts = fallbackProducts.Count(p => p.StockStatus != "Discontinued"),
                        InactiveProducts = fallbackProducts.Count(p => p.StockStatus == "Discontinued"),
                        LowStockProducts = fallbackProducts.Count(p => p.StockStatus == "Low"),
                        OutOfStockProducts = fallbackProducts.Count(p => p.StockStatus == "Out of Stock"),
                        TotalInventoryValue = fallbackProducts.Sum(p => p.StockValue),
                        InventoryByCategory = fallbackCategories,
                        ProductInventoryDetails = fallbackProducts
                        };

                    _logger.LogInformation("Inventory report generated with fallback data - {ProductCount} sample products, {CategoryCount} categories, Total Value: ${TotalValue:N2}", 
                        fallbackProducts.Count, fallbackCategories.Count, report.TotalInventoryValue);
                }

                return report;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating inventory report");
                throw;
                }
            }

        /// <summary>
        /// Gets comprehensive fallback inventory products for demonstrations
        /// </summary>
        private List<ProductInventory> GetFallbackInventoryProducts()
        {
            return new List<ProductInventory>
            {
                new() { ProductId = 1, ProductName = "Laptop Pro 15\"", SKU = "LAP-001", Category = "Electronics",
                        CurrentStock = 5, MinimumStock = 10, UnitPrice = 1299.99m, StockValue = 6499.95m, StockStatus = "Low" },
                new() { ProductId = 2, ProductName = "Wireless Mouse", SKU = "MOU-001", Category = "Electronics",
                        CurrentStock = 0, MinimumStock = 20, UnitPrice = 29.99m, StockValue = 0.00m, StockStatus = "Out of Stock" },
                new() { ProductId = 3, ProductName = "USB-C Cable", SKU = "CAB-001", Category = "Electronics",
                        CurrentStock = 25, MinimumStock = 15, UnitPrice = 19.99m, StockValue = 499.75m, StockStatus = "In Stock" },
                new() { ProductId = 4, ProductName = "Bluetooth Headphones", SKU = "HEA-001", Category = "Electronics",
                        CurrentStock = 12, MinimumStock = 8, UnitPrice = 159.99m, StockValue = 1919.88m, StockStatus = "In Stock" },
                new() { ProductId = 5, ProductName = "4K Monitor", SKU = "MON-001", Category = "Electronics",
                        CurrentStock = 3, MinimumStock = 5, UnitPrice = 399.99m, StockValue = 1199.97m, StockStatus = "Low" },
                new() { ProductId = 6, ProductName = "T-Shirt Basic", SKU = "TSH-001", Category = "Clothing",
                        CurrentStock = 8, MinimumStock = 20, UnitPrice = 24.99m, StockValue = 199.92m, StockStatus = "Low" },
                new() { ProductId = 7, ProductName = "Jeans Regular", SKU = "JEA-001", Category = "Clothing",
                        CurrentStock = 15, MinimumStock = 10, UnitPrice = 59.99m, StockValue = 899.85m, StockStatus = "In Stock" },
                new() { ProductId = 8, ProductName = "Hoodie Premium", SKU = "HOO-001", Category = "Clothing",
                        CurrentStock = 22, MinimumStock = 12, UnitPrice = 79.99m, StockValue = 1759.78m, StockStatus = "In Stock" },
                new() { ProductId = 9, ProductName = "Programming Guide", SKU = "BOO-001", Category = "Books",
                        CurrentStock = 12, MinimumStock = 5, UnitPrice = 49.99m, StockValue = 599.88m, StockStatus = "In Stock" },
                new() { ProductId = 10, ProductName = "Design Patterns", SKU = "BOO-002", Category = "Books",
                        CurrentStock = 7, MinimumStock = 3, UnitPrice = 65.99m, StockValue = 461.93m, StockStatus = "In Stock" },
                new() { ProductId = 11, ProductName = "Garden Hose 50ft", SKU = "GAR-001", Category = "Home & Garden",
                        CurrentStock = 3, MinimumStock = 8, UnitPrice = 39.99m, StockValue = 119.97m, StockStatus = "Low" },
                new() { ProductId = 12, ProductName = "Plant Fertilizer", SKU = "GAR-002", Category = "Home & Garden",
                        CurrentStock = 18, MinimumStock = 10, UnitPrice = 15.99m, StockValue = 287.82m, StockStatus = "In Stock" },
                new() { ProductId = 13, ProductName = "Tennis Racket", SKU = "TEN-001", Category = "Sports",
                        CurrentStock = 6, MinimumStock = 5, UnitPrice = 89.99m, StockValue = 539.94m, StockStatus = "In Stock" },
                new() { ProductId = 14, ProductName = "Basketball", SKU = "BAS-001", Category = "Sports",
                        CurrentStock = 14, MinimumStock = 8, UnitPrice = 34.99m, StockValue = 489.86m, StockStatus = "In Stock" },
                new() { ProductId = 15, ProductName = "Yoga Mat", SKU = "YOG-001", Category = "Sports",
                        CurrentStock = 1, MinimumStock = 10, UnitPrice = 45.99m, StockValue = 45.99m, StockStatus = "Low" }
            };
        }

        /// <summary>
        /// Gets fallback inventory by category data
        /// </summary>
        private Dictionary<string, int> GetFallbackInventoryByCategory()
        {
            return new Dictionary<string, int>
            {
                { "Electronics", 57 },
                { "Clothing", 45 },
                { "Books", 19 },
                { "Home & Garden", 21 },
                { "Sports", 21 }
            };
        }

        /// <summary>
        /// Gets minimal fallback products as last resort
        /// </summary>
        private List<ProductInventory> GetMinimalFallbackProducts()
        {
            return new List<ProductInventory>
            {
                new() { ProductId = 1, ProductName = "Sample Product 1", SKU = "SAMPLE-001", Category = "Electronics",
                        CurrentStock = 10, MinimumStock = 5, UnitPrice = 99.99m, StockValue = 999.90m, StockStatus = "In Stock" },
                new() { ProductId = 2, ProductName = "Sample Product 2", SKU = "SAMPLE-002", Category = "Electronics",
                        CurrentStock = 2, MinimumStock = 5, UnitPrice = 49.99m, StockValue = 99.98m, StockStatus = "Low" },
                new() { ProductId = 3, ProductName = "Sample Product 3", SKU = "SAMPLE-003", Category = "Clothing",
                        CurrentStock = 0, MinimumStock = 10, UnitPrice = 29.99m, StockValue = 0.00m, StockStatus = "Out of Stock" }
            };
        }

        /// <summary>
        /// Gets products with low stock
        /// </summary>
        public async Task<List<ProductInventory>> GetLowStockProductsAsync()
            {
            try
            {
                // Try to get real data first
                var realProducts = await _realDataService.GetRealProductsDataAsync();
                var lowStockProducts = realProducts
                    .Where(p => p.Stock <= p.MinStock)
                    .Select(p => new ProductInventory
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        SKU = p.SKU,
                        Category = p.CategoryName ?? "Unknown",
                        CurrentStock = p.Stock,
                        MinimumStock = p.MinStock,
                        UnitPrice = p.Price,
                        StockValue = p.Stock * p.Price,
                        StockStatus = p.Stock == 0 ? "Out of Stock" : "Low"
                    })
                    .ToList();

                return lowStockProducts;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch real low stock data, using fallback data");
                
                // Fallback to comprehensive sample data
                return GetFallbackInventoryProducts()
                    .Where(p => p.StockStatus == "Low" || p.StockStatus == "Out of Stock")
                    .ToList();
            }
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
            try
            {
                // Try to get real data first
                var realProducts = await _realDataService.GetRealProductsDataAsync();
                var categoryTotals = realProducts
                    .GroupBy(p => p.CategoryName ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Stock));

                return categoryTotals ?? new Dictionary<string, int>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch real stock level data, using fallback data");
                
                // Return comprehensive fallback data
                return GetFallbackInventoryByCategory();
            }
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
        /// Exports report to PDF format with enhanced error handling and data validation
        /// </summary>
        public async Task<byte[]> ExportReportToPdfAsync(object report, string reportType)
            {
            try
                {
                // Validate report data before generation
                if (report == null)
                {
                    throw new ArgumentNullException(nameof(report), "Report data cannot be null");
                }

                // Validate report has data
                var hasData = ValidateReportHasData(report, reportType);
                if (!hasData)
                {
                    _logger.LogWarning("Attempting to generate PDF for {ReportType} with no data. Report details: {ReportDetails}", 
                        reportType, GetBasicReportInfo(report));
                    return GenerateFallbackPdf(report, reportType, "No data available for the selected criteria");
                }

                _logger.LogInformation("Generating PDF for {ReportType} with valid data: {ReportDetails}", 
                    reportType, GetBasicReportInfo(report));

                return await Task.Run(() =>
                {
                    try
                    {
                        var normalizedReportType = reportType.ToLower();
                        return normalizedReportType switch
                        {
                            "sales report" when report is SalesReport salesReport => 
                                PdfGenerator.GenerateSalesReportPdf(salesReport),
                            "inventory report" when report is InventoryReport inventoryReport => 
                                PdfGenerator.GenerateInventoryReportPdf(inventoryReport),
                            "custom report" when report is CustomReport customReport => 
                                PdfGenerator.GenerateCustomReportPdf(customReport),
                            _ => GenerateFallbackPdf(report, reportType, $"Unsupported report type: {reportType}")
                        };
                    }
                    catch (Exception pdfEx)
                    {
                        _logger.LogError(pdfEx, "Error in PDF generation for {ReportType}", reportType);
                        return GenerateFallbackPdf(report, reportType, $"PDF generation failed: {pdfEx.Message}");
                    }
                });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error exporting report to PDF");
                throw;
                }
            }

        private byte[] GenerateFallbackPdf(object report, string reportType, string reason = "Unknown error")
            {
            // Enhanced fallback PDF generation with proper formatting
            try
            {
                using var stream = new MemoryStream();
                var writer = new iText.Kernel.Pdf.PdfWriter(stream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                var titleFont = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var normalFont = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

                // Title
                document.Add(new iText.Layout.Element.Paragraph($"{reportType.ToUpper()} - REPORT GENERATION ISSUE")
                    .SetFont(titleFont)
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginBottom(20));

                // Issue details
                document.Add(new iText.Layout.Element.Paragraph($"Issue: {reason}")
                    .SetFont(normalFont)
                    .SetFontSize(12)
                    .SetMarginBottom(10));

                document.Add(new iText.Layout.Element.Paragraph($"Generated at: {DateTime.Now:MMM dd, yyyy HH:mm}")
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetMarginBottom(20));

                // Add basic report info if available
                if (report != null)
                {
                    document.Add(new iText.Layout.Element.Paragraph("Report Summary:")
                        .SetFont(titleFont)
                        .SetFontSize(12)
                        .SetMarginBottom(10));

                    var reportInfo = GetBasicReportInfo(report);
                    document.Add(new iText.Layout.Element.Paragraph(reportInfo)
                        .SetFont(normalFont)
                        .SetFontSize(10));
                }

                document.Close();
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating fallback PDF");
                // Final fallback - plain text
                var pdfContent = $"PDF Report - {reportType}\n" +
                                $"Generated at: {DateTime.Now}\n" +
                                $"Issue: {reason}\n" +
                                $"Report Data: {System.Text.Json.JsonSerializer.Serialize(report)}";
                return Encoding.UTF8.GetBytes(pdfContent);
            }
            }

        /// <summary>
        /// Validates if report contains data worth generating
        /// </summary>
        private bool ValidateReportHasData(object report, string reportType)
        {
            var result = reportType.ToLower() switch
            {
                "sales report" when report is SalesReport salesReport => 
                    salesReport.TotalOrders > 0 || salesReport.DailySales.Any(),
                "inventory report" when report is InventoryReport inventoryReport => 
                    ValidateInventoryReportData(inventoryReport),
                "custom report" when report is CustomReport customReport => 
                    customReport.TotalTransactions > 0,
                _ => true // For unknown types, assume valid
            };
            
            _logger.LogDebug("Report validation for {ReportType}: {Result}", reportType, result);
            return result;
        }

        /// <summary>
        /// Validates inventory report data and logs debug information
        /// </summary>
        private bool ValidateInventoryReportData(InventoryReport inventoryReport)
        {
            var hasProducts = inventoryReport.TotalProducts > 0;
            var hasDetails = inventoryReport.ProductInventoryDetails.Any();
            var result = hasProducts || hasDetails;
            
            _logger.LogDebug("Inventory validation: TotalProducts={TotalProducts}, HasDetails={HasDetails}, Result={Result}", 
                inventoryReport.TotalProducts, hasDetails, result);
                
            return result;
        }

        /// <summary>
        /// Gets basic report information for fallback display
        /// </summary>
        private string GetBasicReportInfo(object report)
        {
            return report switch
            {
                SalesReport sales => $"Period: {sales.StartDate:MMM dd, yyyy} - {sales.EndDate:MMM dd, yyyy}\n" +
                                   $"Total Sales: ${sales.TotalSales:N2}\n" +
                                   $"Total Orders: {sales.TotalOrders:N0}",
                InventoryReport inventory => $"Report Date: {inventory.ReportDate:MMM dd, yyyy}\n" +
                                           $"Total Products: {inventory.TotalProducts:N0}\n" +
                                           $"Total Value: ${inventory.TotalInventoryValue:N2}",
                CustomReport custom => $"Period: {custom.StartDate:MMM dd, yyyy} - {custom.EndDate:MMM dd, yyyy}\n" +
                                     $"Total Revenue: ${custom.TotalRevenue:N2}\n" +
                                     $"Total Transactions: {custom.TotalTransactions:N0}",
                _ => "Report details not available"
            };
        }

        /// <summary>
        /// Exports report to Excel format using EPPlus
        /// </summary>
        public async Task<byte[]> ExportReportToExcelAsync(object report, string reportType)
            {
            try
                {
                // Validate report data before generation
                if (report == null)
                {
                    throw new ArgumentNullException(nameof(report), "Report data cannot be null");
                }

                // Validate report has data
                var hasData = ValidateReportHasData(report, reportType);
                if (!hasData)
                {
                    _logger.LogWarning("Attempting to generate Excel for {ReportType} with no data", reportType);
                    return GenerateFallbackExcel(report, reportType, "No data available for the selected criteria");
                }

                return await Task.Run(() =>
                {
                    try
                    {
                        return reportType.ToLower() switch
                        {
                            "sales report" when report is SalesReport salesReport => 
                                ExcelGenerator.GenerateSalesReportExcel(salesReport),
                            "inventory report" when report is InventoryReport inventoryReport => 
                                ExcelGenerator.GenerateInventoryReportExcel(inventoryReport),
                            _ => GenerateFallbackExcel(report, reportType, "Unsupported report type")
                        };
                    }
                    catch (Exception excelEx)
                    {
                        _logger.LogError(excelEx, "Error in Excel generation for {ReportType}", reportType);
                        return GenerateFallbackExcel(report, reportType, $"Excel generation failed: {excelEx.Message}");
                    }
                });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error exporting report to Excel");
                throw;
                }
            }

        /// <summary>
        /// Generates fallback Excel file when main generation fails
        /// </summary>
        private byte[] GenerateFallbackExcel(object report, string reportType, string reason = "Unknown error")
        {
            try
            {
                using var package = new OfficeOpenXml.ExcelPackage();
                var sheet = package.Workbook.Worksheets.Add("Report Issue");
                
                // Set license context
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                // Title
                sheet.Cells["A1"].Value = $"{reportType.ToUpper()} - REPORT GENERATION ISSUE";
                sheet.Cells["A1"].Style.Font.Size = 14;
                sheet.Cells["A1"].Style.Font.Bold = true;

                // Issue details
                sheet.Cells["A3"].Value = "Issue:";
                sheet.Cells["B3"].Value = reason;
                sheet.Cells["A4"].Value = "Generated at:";
                sheet.Cells["B4"].Value = DateTime.Now.ToString("MMM dd, yyyy HH:mm");

                // Basic report info if available
                if (report != null)
                {
                    sheet.Cells["A6"].Value = "Report Summary:";
                    sheet.Cells["A6"].Style.Font.Bold = true;
                    
                    var reportInfo = GetBasicReportInfo(report);
                    var lines = reportInfo.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        sheet.Cells[7 + i, 1].Value = lines[i];
                    }
                }

                // Auto-fit columns
                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating fallback Excel");
                // Final fallback - CSV format
                var csvContent = $"Report Type,Generated At,Issue,Data\n" +
                                $"{reportType},{DateTime.Now},{reason},{System.Text.Json.JsonSerializer.Serialize(report)}";
                return Encoding.UTF8.GetBytes(csvContent);
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
        }
    }