using Microsoft.EntityFrameworkCore;
using InventoryPro.SalesService.Models;
using InventoryPro.SalesService.Data;

namespace InventoryPro.SalesService.Services
{
    /// <summary>
    /// Implementation of sales service
    /// Handles all sales-related business logic
    /// </summary>
    public class SalesService : ISalesService
    {
        private readonly SalesDbContext _context;
        private readonly ILogger<SalesService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SalesService(SalesDbContext context, ILogger<SalesService> logger, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Customer Operations

        /// <summary>
        /// Gets all active customers
        /// </summary>
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gets customer by ID
        /// </summary>
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Sales)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        /// <summary>
        /// Gets customer by email
        /// </summary>
        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email && c.IsActive);
        }

        /// <summary>
        /// Creates new customer
        /// </summary>
        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            try
            {
                // Validate email uniqueness
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
                    {
                        throw new InvalidOperationException($"Customer with email {customer.Email} already exists");
                    }
                }

                customer.CreatedAt = DateTime.UtcNow;
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Customer created: {CustomerName} (ID: {CustomerId})",
                    customer.Name, customer.Id);

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                throw;
            }
        }

        /// <summary>
        /// Updates existing customer
        /// </summary>
        public async Task<Customer?> UpdateCustomerAsync(int id, Customer customer)
        {
            try
            {
                var existingCustomer = await GetCustomerByIdAsync(id);
                if (existingCustomer == null)
                    return null;

                // Check if email is being changed and ensure uniqueness
                if (existingCustomer.Email != customer.Email && !string.IsNullOrEmpty(customer.Email))
                {
                    if (await _context.Customers.AnyAsync(c => c.Email == customer.Email && c.Id != id))
                    {
                        throw new InvalidOperationException($"Customer with email {customer.Email} already exists");
                    }
                }

                // Update properties
                existingCustomer.Name = customer.Name;
                existingCustomer.Email = customer.Email;
                existingCustomer.Phone = customer.Phone;
                existingCustomer.Address = customer.Address;
                existingCustomer.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Customer updated: {CustomerName} (ID: {CustomerId})",
                    existingCustomer.Name, id);

                return existingCustomer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer");
                throw;
            }
        }

        /// <summary>
        /// Soft deletes customer (marks as inactive)
        /// </summary>
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await GetCustomerByIdAsync(id);
                if (customer == null)
                    return false;

                customer.IsActive = false;
                customer.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Customer deleted: {CustomerName} (ID: {CustomerId})",
                    customer.Name, id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer");
                return false;
            }
        }

        /// <summary>
        /// Searches customers by name, email, or phone
        /// </summary>
        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCustomersAsync();

            searchTerm = searchTerm.ToLower();

            return await _context.Customers
                .Where(c => c.IsActive && (
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm) ||
                    c.Phone.Contains(searchTerm)))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        #endregion

        #region Sales Operations

        /// <summary>
        /// Gets all sales
        /// </summary>
        public async Task<IEnumerable<Sale>> GetAllSalesAsync()
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets sale by ID
        /// </summary>
        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Gets sales by customer
        /// </summary>
        public async Task<IEnumerable<Sale>> GetSalesByCustomerAsync(int customerId)
        {
            return await _context.Sales
                .Include(s => s.SaleItems)
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets sales by date range
        /// </summary>
        public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Creates new sale
        /// </summary>
        public async Task<Sale> CreateSaleAsync(Sale sale)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate customer exists
                var customer = await GetCustomerByIdAsync(sale.CustomerId);
                if (customer == null)
                    throw new InvalidOperationException("Customer not found");

                // Get authorization header from current HTTP context
                var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

                // Check product availability and update stock
                foreach (var item in sale.SaleItems)
                {
                    var (isAvailable, currentStock, actualProductName) = await CheckProductAvailabilityDetailedAsync(item.ProductId, item.Quantity, authorizationHeader);
                    if (!isAvailable)
                    {
                        // Use the most reliable product name: from API > from client > fallback
                        var productDisplayName = !string.IsNullOrEmpty(actualProductName) && actualProductName != "Unknown Product"
                            ? actualProductName
                            : (!string.IsNullOrEmpty(item.ProductName) && item.ProductName != "Product" 
                                ? item.ProductName 
                                : $"Product ID {item.ProductId}");
                        
                        throw new InvalidOperationException($"Insufficient stock for product '{productDisplayName}': requested {item.Quantity}, available {currentStock}");
                    }
                }

                // Validate totals (tax information is already calculated by frontend)
                // Verify that the passed total matches the calculation for security
                var calculatedSubtotal = sale.SaleItems.Sum(i => i.FinalAmount);
                var calculatedTaxAmount = calculatedSubtotal * sale.TaxRate;
                var calculatedTotal = calculatedSubtotal + calculatedTaxAmount;
                
                // Log discrepancies for debugging
                if (Math.Abs(sale.SubtotalAmount - calculatedSubtotal) > 0.01m)
                {
                    _logger.LogWarning("Subtotal mismatch: passed {PassedSubtotal}, calculated {CalculatedSubtotal}", 
                        sale.SubtotalAmount, calculatedSubtotal);
                }
                if (Math.Abs(sale.TaxAmount - calculatedTaxAmount) > 0.01m)
                {
                    _logger.LogWarning("Tax amount mismatch: passed {PassedTax}, calculated {CalculatedTax}", 
                        sale.TaxAmount, calculatedTaxAmount);
                }
                if (Math.Abs(sale.TotalAmount - calculatedTotal) > 0.01m)
                {
                    _logger.LogWarning("Total amount mismatch: passed {PassedTotal}, calculated {CalculatedTotal}", 
                        sale.TotalAmount, calculatedTotal);
                }
                
                sale.ChangeAmount = sale.PaidAmount - sale.TotalAmount;

                if (sale.PaidAmount < sale.TotalAmount)
                {
                    throw new InvalidOperationException("Paid amount is less than total amount");
                }

                sale.CreatedAt = DateTime.UtcNow;
                sale.Status = "Completed";

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                // Update product stock
                foreach (var item in sale.SaleItems)
                {
                    await UpdateProductStockAsync(item.ProductId, -item.Quantity, authorizationHeader);
                }

                // Create payment record
                var payment = new Payment
                {
                    SaleId = sale.Id,
                    PaymentMethod = sale.PaymentMethod,
                    Amount = sale.PaidAmount,
                    Status = "Completed",
                    PaymentDate = DateTime.UtcNow
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Sale created: {SaleId} for customer {CustomerId} - Total: {TotalAmount}",
                    sale.Id, sale.CustomerId, sale.TotalAmount);

                return sale;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating sale");
                throw;
            }
        }

        /// <summary>
        /// Updates sale status
        /// </summary>
        public async Task<bool> UpdateSaleStatusAsync(int saleId, string status)
        {
            try
            {
                var sale = await GetSaleByIdAsync(saleId);
                if (sale == null)
                    return false;

                sale.Status = status;
                sale.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Sale status updated: {SaleId} to {Status}", saleId, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sale status");
                return false;
            }
        }

        /// <summary>
        /// Cancels a sale
        /// </summary>
        public async Task<bool> CancelSaleAsync(int saleId, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var sale = await GetSaleByIdAsync(saleId);
                if (sale == null || sale.Status == "Cancelled")
                    return false;

                // Get authorization header from current HTTP context
                var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

                // Reverse stock for each item
                foreach (var item in sale.SaleItems)
                {
                    await UpdateProductStockAsync(item.ProductId, item.Quantity, authorizationHeader);
                }

                sale.Status = "Cancelled";
                sale.Notes = $"{sale.Notes}\nCancelled: {reason}";
                sale.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Sale cancelled: {SaleId} - Reason: {Reason}", saleId, reason);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling sale");
                return false;
            }
        }

        #endregion

        #region Sales Statistics

        /// <summary>
        /// Gets total sales amount for a date range
        /// </summary>
        public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Sales.Where(s => s.Status == "Completed");

            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate <= endDate.Value);

            return await query.SumAsync(s => s.TotalAmount);
        }

        /// <summary>
        /// Gets sales count for a date range
        /// </summary>
        public async Task<int> GetSalesCountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Sales.Where(s => s.Status == "Completed");

            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate <= endDate.Value);

            return await query.CountAsync();
        }

        /// <summary>
        /// Gets sales grouped by payment method
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetSalesByPaymentMethodAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Sales.Where(s => s.Status == "Completed");

            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate <= endDate.Value);

            return await query
                .GroupBy(s => s.PaymentMethod)
                .Select(g => new { Method = g.Key, Total = g.Sum(s => s.TotalAmount) })
                .ToDictionaryAsync(x => x.Method, x => x.Total);
        }

        /// <summary>
        /// Gets total customers count
        /// </summary>
        public async Task<int> GetTotalCustomersAsync()
        {
            return await _context.Customers.Where(c => c.IsActive).CountAsync();
        }

        /// <summary>
        /// Gets count of new customers in date range
        /// </summary>
        public async Task<int> GetNewCustomersCountAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Customers
                .Where(c => c.IsActive && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .CountAsync();
        }

        /// <summary>
        /// Gets recent sales for dashboard activities
        /// </summary>
        public async Task<IEnumerable<Sale>> GetRecentSalesAsync(int count = 10)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.Status == "Completed")
                .OrderByDescending(s => s.SaleDate)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Gets top customers by purchase amount
        /// </summary>
        public async Task<IEnumerable<(Customer Customer, decimal TotalPurchases)>> GetTopCustomersAsync(int count = 10)
        {
            var topCustomers = await _context.Sales
                .Where(s => s.Status == "Completed")
                .GroupBy(s => s.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    TotalPurchases = g.Sum(s => s.TotalAmount)
                })
                .OrderByDescending(x => x.TotalPurchases)
                .Take(count)
                .ToListAsync();

            var result = new List<(Customer Customer, decimal TotalPurchases)>();

            foreach (var item in topCustomers)
            {
                var customer = await GetCustomerByIdAsync(item.CustomerId);
                if (customer != null)
                {
                    result.Add((customer, item.TotalPurchases));
                }
            }

            return result;
        }

        #endregion

        #region Inventory Integration

        /// <summary>
        /// Checks product availability by calling Product Service and returns detailed stock info
        /// </summary>
        public async Task<(bool isAvailable, int currentStock, string productName)> CheckProductAvailabilityDetailedAsync(int productId, int quantity, string? authorizationHeader = null)
        {
            try
            {
                _logger.LogInformation("Checking availability for product {ProductId}, quantity {Quantity}",
                    productId, quantity);

                // Create request message with authorization header
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}");
                
                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    request.Headers.Authorization = 
                        System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authorizationHeader);
                }

                // Call Product Service to get current stock
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Product {ProductId} not found or unavailable", productId);
                    return (false, 0, $"Product ID {productId}");
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(jsonContent);
                var productElement = doc.RootElement;

                // Debug logging to see the actual JSON response
                _logger.LogInformation("Product Service response for ProductId {ProductId}: {JsonContent}", 
                    productId, jsonContent);

                var productName = "Unknown Product";
                var currentStock = 0;

                // Try both PascalCase and camelCase for product name
                if (productElement.TryGetProperty("Name", out var nameElement) || 
                    productElement.TryGetProperty("name", out nameElement))
                {
                    productName = nameElement.GetString() ?? $"Product ID {productId}";
                    _logger.LogInformation("Successfully extracted product name: '{ProductName}'", productName);
                }
                else
                {
                    _logger.LogWarning("Could not find 'Name' or 'name' property in product response for ProductId {ProductId}", productId);
                }

                // Try both PascalCase and camelCase for stock
                if (productElement.TryGetProperty("Stock", out var stockElement) || 
                    productElement.TryGetProperty("stock", out stockElement))
                {
                    currentStock = stockElement.GetInt32();
                    var isAvailable = currentStock >= quantity;
                    
                    _logger.LogInformation("Product {ProductId} ({ProductName}) has {CurrentStock} in stock, requested {Quantity}, available: {IsAvailable}",
                        productId, productName, currentStock, quantity, isAvailable);
                        
                    return (isAvailable, currentStock, productName);
                }

                _logger.LogWarning("Could not determine stock for product {ProductId}", productId);
                return (false, 0, productName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product availability for product {ProductId}", productId);
                return (false, 0, $"Product ID {productId}");
            }
        }

        /// <summary>
        /// Checks product availability by calling Product Service
        /// </summary>
        public async Task<bool> CheckProductAvailabilityAsync(int productId, int quantity)
        {
            var (isAvailable, _, _) = await CheckProductAvailabilityDetailedAsync(productId, quantity);
            return isAvailable;
        }

        /// <summary>
        /// Updates product stock by calling Product Service (backward compatibility)
        /// </summary>
        public async Task<bool> UpdateProductStockAsync(int productId, int quantity)
        {
            // Get authorization header from current HTTP context
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            return await UpdateProductStockAsync(productId, quantity, authorizationHeader);
        }

        /// <summary>
        /// Updates product stock by calling Product Service
        /// </summary>
        public async Task<bool> UpdateProductStockAsync(int productId, int quantity, string? authorizationHeader = null)
        {
            try
            {
                _logger.LogInformation("Updating stock for product {ProductId}, quantity change {Quantity}",
                    productId, quantity);

                // Determine movement type and reason based on quantity
                var movementType = quantity < 0 ? "Sale" : "Return";
                var reason = quantity < 0 ? "Product sold" : "Product returned";

                // Create the stock update request
                var updateRequest = new
                {
                    Quantity = quantity,
                    MovementType = movementType,
                    Reason = reason
                };

                var jsonContent = System.Text.Json.JsonSerializer.Serialize(updateRequest);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Create request message with authorization header
                using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/products/{productId}/stock")
                {
                    Content = content
                };
                
                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    request.Headers.Authorization = 
                        System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authorizationHeader);
                }

                // Call Product Service to update stock
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated stock for product {ProductId}, change: {Quantity}",
                        productId, quantity);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to update stock for product {ProductId}. Status: {StatusCode}, Error: {Error}",
                        productId, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product stock for product {ProductId}", productId);
                return false;
            }
        }

        #endregion
    }
}