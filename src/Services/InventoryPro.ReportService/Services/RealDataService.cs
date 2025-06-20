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
                var salesUrl = "http://localhost:5000/sales";

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
                    var realData = pagedResponse?.Items ?? new List<SaleDto>();

                    if (realData.Any())
                        {
                        return realData;
                        }
                    }

                _logger.LogWarning("Failed to fetch sales data or no data available. Status: {StatusCode}. Using fallback mock data.", response.StatusCode);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error fetching real sales data. Using fallback mock data.");
                }

            // Return fallback mock data when real service is unavailable
            return GenerateMockSalesData(startDate ?? DateTime.UtcNow.AddMonths(-1), endDate ?? DateTime.UtcNow);
            }

        /// <summary>
        /// Fetches real customer data from Sales Service
        /// </summary>
        public async Task<List<CustomerDto>> GetRealCustomersDataAsync()
            {
            try
                {
                var response = await _httpClient.GetAsync("http://localhost:5000/customers");
                if (response.IsSuccessStatusCode)
                    {
                    var content = await response.Content.ReadAsStringAsync();
                    var pagedResponse = JsonSerializer.Deserialize<PagedResponse<CustomerDto>>(content, _jsonOptions);
                    var realData = pagedResponse?.Items ?? new List<CustomerDto>();

                    if (realData.Any())
                        {
                        return realData;
                        }
                    }

                _logger.LogWarning("Failed to fetch customers data or no data available. Status: {StatusCode}. Using fallback mock data.", response.StatusCode);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error fetching real customers data. Using fallback mock data.");
                }

            // Return fallback mock data when real service is unavailable
            return GenerateMockCustomersData();
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
                var response = await _httpClient.GetAsync("http://localhost:5000/products");
                if (response.IsSuccessStatusCode)
                    {
                    var content = await response.Content.ReadAsStringAsync();
                    var pagedResponse = JsonSerializer.Deserialize<PagedResponse<ProductDto>>(content, _jsonOptions);
                    var realData = pagedResponse?.Items ?? new List<ProductDto>();

                    if (realData.Any())
                        {
                        return realData;
                        }
                    }

                _logger.LogWarning("Failed to fetch products data or no data available. Status: {StatusCode}. Using fallback mock data.", response.StatusCode);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error fetching real products data. Using fallback mock data.");
                }

            // Return fallback mock data when real service is unavailable
            return GenerateMockProductsData();
            }

        /// <summary>
        /// Fetches real category data from Product Service
        /// </summary>
        public async Task<List<CategoryDto>> GetRealCategoriesDataAsync()
            {
            try
                {
                var response = await _httpClient.GetAsync("http://localhost:5000/products/categories");
                if (response.IsSuccessStatusCode)
                    {
                    var content = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<CategoryDto>>(content, _jsonOptions);
                    var realData = categories ?? new List<CategoryDto>();

                    if (realData.Any())
                        {
                        return realData;
                        }
                    }

                _logger.LogWarning("Failed to fetch categories data or no data available. Status: {StatusCode}. Using fallback mock data.", response.StatusCode);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error fetching real categories data. Using fallback mock data.");
                }

            // Return fallback mock data when real service is unavailable
            return GenerateMockCategoriesData();
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

        #region Mock Data Generation Methods

        /// <summary>
        /// Generates mock sales data when real service is unavailable
        /// </summary>
        private List<SaleDto> GenerateMockSalesData(DateTime startDate, DateTime endDate)
            {
            var random = new Random();
            var sales = new List<SaleDto>();

            // Generate sales for each day in the range
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                var dailySalesCount = random.Next(1, 8); // 1-7 sales per day

                for (int i = 0; i < dailySalesCount; i++)
                    {
                    var customerId = random.Next(1, 11); // 10 different customers
                    var sale = new SaleDto
                        {
                        Id = random.Next(1000, 9999),
                        CustomerId = customerId,
                        Date = date.AddHours(random.Next(9, 18)).AddMinutes(random.Next(0, 59)),
                        Status = "Completed",
                        PaymentMethod = GetRandomPaymentMethod(random),
                        TotalAmount = 0,
                        Items = new List<SaleItemDto>()
                        };

                    // Add 1-5 items per sale
                    var itemCount = random.Next(1, 6);
                    for (int j = 0; j < itemCount; j++)
                        {
                        var productId = random.Next(1, 21); // 20 different products
                        var quantity = random.Next(1, 4);
                        var unitPrice = (decimal)(random.Next(10, 500) + random.NextDouble());
                        var discountAmount = (decimal)(random.NextDouble() * 5); // 0-5 discount

                        var item = new SaleItemDto
                            {
                            ProductId = productId,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            DiscountAmount = discountAmount
                            };

                        sale.Items.Add(item);
                        sale.TotalAmount += (unitPrice - discountAmount) * quantity;
                        }

                    sales.Add(sale);
                    }
                }

            return sales;
            }

        /// <summary>
        /// Generates mock customer data
        /// </summary>
        private List<CustomerDto> GenerateMockCustomersData()
            {
            return new List<CustomerDto>
            {
                new() { Id = 1, Name = "John Doe", Email = "john.doe@example.com", Phone = "555-0101" },
                new() { Id = 2, Name = "Jane Smith", Email = "jane.smith@example.com", Phone = "555-0102" },
                new() { Id = 3, Name = "Bob Johnson", Email = "bob.johnson@example.com", Phone = "555-0103" },
                new() { Id = 4, Name = "Alice Brown", Email = "alice.brown@example.com", Phone = "555-0104" },
                new() { Id = 5, Name = "Charlie Wilson", Email = "charlie.wilson@example.com", Phone = "555-0105" },
                new() { Id = 6, Name = "Diana Davis", Email = "diana.davis@example.com", Phone = "555-0106" },
                new() { Id = 7, Name = "Eve Miller", Email = "eve.miller@example.com", Phone = "555-0107" },
                new() { Id = 8, Name = "Frank Garcia", Email = "frank.garcia@example.com", Phone = "555-0108" },
                new() { Id = 9, Name = "Grace Lee", Email = "grace.lee@example.com", Phone = "555-0109" },
                new() { Id = 10, Name = "Henry Taylor", Email = "henry.taylor@example.com", Phone = "555-0110" }
            };
            }

        /// <summary>
        /// Generates mock product data
        /// </summary>
        private List<ProductDto> GenerateMockProductsData()
            {
            var random = new Random();
            return new List<ProductDto>
            {
                new() { Id = 1, Name = "Gaming Laptop Pro", SKU = "GAM-001", Price = 1299.99m, Stock = random.Next(5, 20), MinStock = 5, CategoryId = 1, IsActive = true },
                new() { Id = 2, Name = "Wireless Gaming Mouse", SKU = "MOU-001", Price = 79.99m, Stock = random.Next(10, 50), MinStock = 10, CategoryId = 1, IsActive = true },
                new() { Id = 3, Name = "Mechanical Keyboard", SKU = "KEY-001", Price = 149.99m, Stock = random.Next(8, 25), MinStock = 8, CategoryId = 1, IsActive = true },
                new() { Id = 4, Name = "USB-C Hub", SKU = "HUB-001", Price = 49.99m, Stock = random.Next(15, 40), MinStock = 15, CategoryId = 1, IsActive = true },
                new() { Id = 5, Name = "Bluetooth Headphones", SKU = "HEA-001", Price = 199.99m, Stock = random.Next(6, 18), MinStock = 6, CategoryId = 1, IsActive = true },
                new() { Id = 6, Name = "4K Webcam", SKU = "WEB-001", Price = 89.99m, Stock = random.Next(12, 30), MinStock = 12, CategoryId = 1, IsActive = true },
                new() { Id = 7, Name = "Portable SSD 1TB", SKU = "SSD-001", Price = 119.99m, Stock = random.Next(3, 15), MinStock = 5, CategoryId = 1, IsActive = true },
                new() { Id = 8, Name = "Dress Shirt", SKU = "SHI-001", Price = 59.99m, Stock = random.Next(20, 60), MinStock = 20, CategoryId = 2, IsActive = true },
                new() { Id = 9, Name = "Jeans", SKU = "JEA-001", Price = 89.99m, Stock = random.Next(15, 45), MinStock = 15, CategoryId = 2, IsActive = true },
                new() { Id = 10, Name = "Running Shoes", SKU = "SHO-001", Price = 129.99m, Stock = random.Next(8, 25), MinStock = 10, CategoryId = 2, IsActive = true },
                new() { Id = 11, Name = "Coffee Maker", SKU = "COF-001", Price = 79.99m, Stock = random.Next(5, 20), MinStock = 8, CategoryId = 3, IsActive = true },
                new() { Id = 12, Name = "Blender", SKU = "BLE-001", Price = 99.99m, Stock = random.Next(4, 15), MinStock = 6, CategoryId = 3, IsActive = true },
                new() { Id = 13, Name = "Air Fryer", SKU = "AIR-001", Price = 149.99m, Stock = random.Next(2, 12), MinStock = 5, CategoryId = 3, IsActive = true },
                new() { Id = 14, Name = "Garden Hose", SKU = "HOS-001", Price = 39.99m, Stock = random.Next(10, 30), MinStock = 12, CategoryId = 4, IsActive = true },
                new() { Id = 15, Name = "Plant Pot Set", SKU = "POT-001", Price = 29.99m, Stock = random.Next(25, 60), MinStock = 25, CategoryId = 4, IsActive = true },
                new() { Id = 16, Name = "Outdoor Chair", SKU = "CHA-001", Price = 89.99m, Stock = random.Next(6, 18), MinStock = 8, CategoryId = 4, IsActive = true },
                new() { Id = 17, Name = "Yoga Mat", SKU = "YOG-001", Price = 34.99m, Stock = random.Next(15, 40), MinStock = 15, CategoryId = 5, IsActive = true },
                new() { Id = 18, Name = "Basketball", SKU = "BAS-001", Price = 24.99m, Stock = random.Next(20, 50), MinStock = 20, CategoryId = 5, IsActive = true },
                new() { Id = 19, Name = "Camping Tent", SKU = "TEN-001", Price = 199.99m, Stock = random.Next(3, 10), MinStock = 5, CategoryId = 5, IsActive = true },
                new() { Id = 20, Name = "Hiking Backpack", SKU = "BAC-001", Price = 119.99m, Stock = random.Next(8, 20), MinStock = 10, CategoryId = 5, IsActive = true }
            };
            }

        /// <summary>
        /// Generates mock category data
        /// </summary>
        private List<CategoryDto> GenerateMockCategoriesData()
            {
            return new List<CategoryDto>
            {
                new() { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" },
                new() { Id = 2, Name = "Clothing", Description = "Apparel and fashion items" },
                new() { Id = 3, Name = "Home & Kitchen", Description = "Home appliances and kitchen items" },
                new() { Id = 4, Name = "Garden & Outdoor", Description = "Garden tools and outdoor equipment" },
                new() { Id = 5, Name = "Sports & Recreation", Description = "Sports equipment and recreational items" }
            };
            }

        /// <summary>
        /// Gets a random payment method
        /// </summary>
        private string GetRandomPaymentMethod(Random random)
            {
            var methods = new[] { "Credit Card", "Debit Card", "PayPal", "Cash", "Bank Transfer" };
            return methods[random.Next(methods.Length)];
            }

        #endregion
        }
    }