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

        public SalesService(SalesDbContext context, ILogger<SalesService> logger, HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
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

                // Check product availability and update stock
                foreach (var item in sale.SaleItems)
                {
                    var isAvailable = await CheckProductAvailabilityAsync(item.ProductId, item.Quantity);
                    if (!isAvailable)
                    {
                        throw new InvalidOperationException($"Insufficient stock for product: {item.ProductName}");
                    }
                }

                // Calculate totals
                sale.TotalAmount = sale.SaleItems.Sum(i => i.FinalAmount);
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
                    await UpdateProductStockAsync(item.ProductId, -item.Quantity);
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

                // Reverse stock for each item
                foreach (var item in sale.SaleItems)
                {
                    await UpdateProductStockAsync(item.ProductId, item.Quantity);
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
        /// Checks product availability by calling Product Service
        /// </summary>
        public async Task<bool> CheckProductAvailabilityAsync(int productId, int quantity)
        {
            try
            {
                // Simulate an asynchronous operation
                await Task.Delay(10); // Placeholder for actual async API call

                _logger.LogInformation("Checking availability for product {ProductId}, quantity {Quantity}",
                    productId, quantity);

                return true; // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product availability");
                return false;
            }
        }

        /// <summary>
        /// Updates product stock by calling Product Service
        /// </summary>
        public async Task<bool> UpdateProductStockAsync(int productId, int quantity)
        {
            try
            {
                // Simulate an asynchronous operation
                await Task.Delay(10); // Placeholder for actual async API call

                _logger.LogInformation("Updating stock for product {ProductId}, quantity change {Quantity}",
                    productId, quantity);

                // Simulate success response from an external service
                return await Task.FromResult(true); // Ensures the method is truly asynchronous
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product stock");
                return false;
            }
        }

        #endregion
    }
}