using System.Text.Json;
using InventoryPro.ReportService.Models;
using InventoryPro.Shared.DTOs;

namespace InventoryPro.ReportService.Services
{
    /// <summary>
    /// Service for fetching real data from microservices
    /// </summary>
    public class RealDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RealDataService(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        #region Sales Data

        /// <summary>
        /// Fetches real sales data from Sales Service
        /// </summary>
        public async Task<List<SaleDto>> GetRealSalesDataAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var salesUrl = "http://localhost:5282/api/sales";
                
                // Add date filtering if provided
                if (startDate.HasValue && endDate.HasValue)
                {
                    salesUrl += $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                }

                var response = await _httpClient.GetAsync(salesUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var pagedResponse = JsonSerializer.Deserialize<PagedResponse<SaleDto>>(content, _jsonOptions);
                    return pagedResponse?.Items ?? new List<SaleDto>();
                }
                
                _logger.LogWarning("Failed to fetch sales data. Status: {StatusCode}", response.StatusCode);
                return new List<SaleDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching real sales data");
                return new List<SaleDto>();
            }
        }

        /// <summary>
        /// Fetches real customer data from Sales Service
        /// </summary>
        public async Task<List<CustomerDto>> GetRealCustomersDataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:5282/api/customers");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var pagedResponse = JsonSerializer.Deserialize<PagedResponse<CustomerDto>>(content, _jsonOptions);
                    return pagedResponse?.Items ?? new List<CustomerDto>();
                }
                
                _logger.LogWarning("Failed to fetch customers data. Status: {StatusCode}", response.StatusCode);
                return new List<CustomerDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching real customers data");
                return new List<CustomerDto>();
            }
        }

        #endregion

        #region Product Data

        /// <summary>
        /// Fetches real product data from Product Service
        /// </summary>
        public async Task<List<ProductDto>> GetRealProductsDataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:5089/api/products");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var pagedResponse = JsonSerializer.Deserialize<PagedResponse<ProductDto>>(content, _jsonOptions);
                    return pagedResponse?.Items ?? new List<ProductDto>();
                }
                
                _logger.LogWarning("Failed to fetch products data. Status: {StatusCode}", response.StatusCode);
                return new List<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching real products data");
                return new List<ProductDto>();
            }
        }

        /// <summary>
        /// Fetches real category data from Product Service
        /// </summary>
        public async Task<List<CategoryDto>> GetRealCategoriesDataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:5089/api/products/categories");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<CategoryDto>>(content, _jsonOptions);
                    return categories ?? new List<CategoryDto>();
                }
                
                _logger.LogWarning("Failed to fetch categories data. Status: {StatusCode}", response.StatusCode);
                return new List<CategoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching real categories data");
                return new List<CategoryDto>();
            }
        }

        #endregion

        #region Data Processing for Reports

        /// <summary>
        /// Processes real sales data into daily sales format
        /// </summary>
        public List<DailySales> ProcessDailySalesData(List<SaleDto> sales, DateTime startDate, DateTime endDate)
        {
            var dailySales = new List<DailySales>();
            
            // Group sales by date
            var salesByDate = sales
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .GroupBy(s => s.Date.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Create daily sales entries for each day in the range
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var daySales = salesByDate.ContainsKey(date) ? salesByDate[date] : new List<SaleDto>();
                
                dailySales.Add(new DailySales
                {
                    Date = date,
                    TotalAmount = daySales.Sum(s => s.TotalAmount),
                    OrderCount = daySales.Count
                });
            }

            return dailySales.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// Processes real sales data into top products format
        /// </summary>
        public async Task<List<ProductSales>> ProcessTopProductsDataAsync(List<SaleDto> sales)
        {
            var products = await GetRealProductsDataAsync();
            var productSales = new Dictionary<int, ProductSales>();

            foreach (var sale in sales.Where(s => s.Status == "Completed"))
            {
                foreach (var item in sale.Items)
                {
                    if (!productSales.ContainsKey(item.ProductId))
                    {
                        var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                        productSales[item.ProductId] = new ProductSales
                        {
                            ProductId = item.ProductId,
                            ProductName = product?.Name ?? "Unknown Product",
                            SKU = product?.SKU ?? "N/A",
                            QuantitySold = 0,
                            TotalRevenue = 0
                        };
                    }

                    productSales[item.ProductId].QuantitySold += item.Quantity;
                    productSales[item.ProductId].TotalRevenue += (item.UnitPrice - item.DiscountAmount) * item.Quantity;
                }
            }

            return productSales.Values
                .OrderByDescending(p => p.TotalRevenue)
                .ToList();
        }

        /// <summary>
        /// Processes real sales data into top customers format
        /// </summary>
        public List<CustomerSales> ProcessTopCustomersData(List<SaleDto> sales, List<CustomerDto> customers)
        {
            var customerSales = new Dictionary<int, CustomerSales>();

            foreach (var sale in sales.Where(s => s.Status == "Completed"))
            {
                if (!customerSales.ContainsKey(sale.CustomerId))
                {
                    var customer = customers.FirstOrDefault(c => c.Id == sale.CustomerId);
                    customerSales[sale.CustomerId] = new CustomerSales
                    {
                        CustomerId = sale.CustomerId,
                        CustomerName = customer?.Name ?? "Unknown Customer",
                        OrderCount = 0,
                        TotalAmount = 0
                    };
                }

                customerSales[sale.CustomerId].OrderCount++;
                customerSales[sale.CustomerId].TotalAmount += sale.TotalAmount;
            }

            return customerSales.Values
                .OrderByDescending(c => c.TotalAmount)
                .ToList();
        }

        /// <summary>
        /// Processes real sales data by category
        /// </summary>
        public async Task<Dictionary<string, decimal>> ProcessSalesByCategoryAsync(List<SaleDto> sales)
        {
            var products = await GetRealProductsDataAsync();
            var categories = await GetRealCategoriesDataAsync();
            var salesByCategory = new Dictionary<string, decimal>();

            foreach (var sale in sales.Where(s => s.Status == "Completed"))
            {
                foreach (var item in sale.Items)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    var category = categories.FirstOrDefault(c => c.Id == product?.CategoryId);
                    var categoryName = category?.Name ?? "Uncategorized";

                    if (!salesByCategory.ContainsKey(categoryName))
                        salesByCategory[categoryName] = 0;

                    salesByCategory[categoryName] += (item.UnitPrice - item.DiscountAmount) * item.Quantity;
                }
            }

            return salesByCategory;
        }

        /// <summary>
        /// Processes sales by payment method
        /// </summary>
        public Dictionary<string, decimal> ProcessSalesByPaymentMethod(List<SaleDto> sales)
        {
            return sales
                .Where(s => s.Status == "Completed")
                .GroupBy(s => s.PaymentMethod)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.TotalAmount));
        }

        /// <summary>
        /// Processes inventory data by category
        /// </summary>
        public async Task<List<CategoryInventory>> ProcessInventoryByCategoryAsync()
        {
            var products = await GetRealProductsDataAsync();
            var categories = await GetRealCategoriesDataAsync();

            var inventoryByCategory = categories.Select(category => 
            {
                var categoryProducts = products.Where(p => p.CategoryId == category.Id).ToList();
                return new CategoryInventory
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    ProductCount = categoryProducts.Count,
                    TotalStock = categoryProducts.Sum(p => p.Stock),
                    TotalValue = categoryProducts.Sum(p => p.Stock * p.Price)
                };
            }).ToList();

            return inventoryByCategory;
        }

        /// <summary>
        /// Processes low stock products data
        /// </summary>
        public async Task<List<ProductInventory>> ProcessLowStockProductsAsync()
        {
            var products = await GetRealProductsDataAsync();
            var categories = await GetRealCategoriesDataAsync();

            var lowStockProducts = products
                .Where(p => p.Stock <= p.MinStock)
                .Select(p =>
                {
                    var category = categories.FirstOrDefault(c => c.Id == p.CategoryId);
                    return new ProductInventory
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        SKU = p.SKU,
                        Category = category?.Name ?? "Uncategorized",
                        CurrentStock = p.Stock,
                        MinimumStock = p.MinStock,
                        UnitPrice = p.Price,
                        StockValue = p.Stock * p.Price,
                        StockStatus = p.Stock == 0 ? "Out of Stock" : 
                                    p.Stock <= p.MinStock ? "Low" : "Normal"
                    };
                })
                .OrderBy(p => p.CurrentStock)
                .ToList();

            return lowStockProducts;
        }

        /// <summary>
        /// Generates monthly revenue data
        /// </summary>
        public List<MonthlyRevenue> ProcessMonthlyRevenueData(List<SaleDto> sales, int year)
        {
            var monthlyRevenue = new List<MonthlyRevenue>();
            decimal previousRevenue = 0;

            for (int month = 1; month <= 12; month++)
            {
                var monthSales = sales
                    .Where(s => s.Date.Year == year && s.Date.Month == month && s.Status == "Completed")
                    .ToList();

                var revenue = monthSales.Sum(s => s.TotalAmount);
                var growth = previousRevenue > 0 ? ((revenue - previousRevenue) / previousRevenue) * 100 : 0;

                monthlyRevenue.Add(new MonthlyRevenue
                {
                    Year = year,
                    Month = month,
                    Revenue = revenue,
                    TransactionCount = monthSales.Count,
                    Growth = Math.Round(growth, 2)
                });

                previousRevenue = revenue;
            }

            return monthlyRevenue;
        }

        #endregion
    }
}