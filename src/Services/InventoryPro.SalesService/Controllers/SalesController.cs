using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryPro.Shared.DTOs;
using InventoryPro.SalesService.Models;
using InventoryPro.SalesService.Services;

namespace InventoryPro.SalesService.Controllers
    {
    /// <summary>
    /// Sales API controller
    /// Handles all sales-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesController : ControllerBase
        {
        private readonly ISalesService _salesService;
        private readonly ILogger<SalesController> _logger;

        public SalesController(ISalesService salesService, ILogger<SalesController> logger)
            {
            _salesService = salesService;
            _logger = logger;
            }

        /// <summary>
        /// Get all sales with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSales([FromQuery] PaginationParameters parameters)
            {
            try
                {
                var sales = await _salesService.GetAllSalesAsync();

                // Apply search filter
                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                    {
                    sales = sales.Where(s =>
                        s.Customer?.Name.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                        s.Id.ToString().Contains(parameters.SearchTerm));
                    }

                // Apply pagination
                var totalCount = sales.Count();
                var items = sales
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(s => MapToSaleDto(s))
                    .ToList();

                var response = new PagedResponse<SaleDto>
                    {
                    Items = items,
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize,
                    TotalCount = totalCount
                    };

                return Ok(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting sales");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get sale by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSale(int id)
            {
            try
                {
                var sale = await _salesService.GetSaleByIdAsync(id);
                if (sale == null)
                    return NotFound();

                return Ok(MapToSaleDto(sale));
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting sale {SaleId}", id);
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Create new sale
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto dto)
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
                {
                // Get user info from JWT token
                var userId = User.FindFirst("sub")?.Value ?? "0";
                var userName = User.Identity?.Name ?? "Unknown";

                var sale = new Sale
                    {
                    CustomerId = dto.CustomerId,
                    PaymentMethod = dto.PaymentMethod,
                    PaidAmount = dto.PaidAmount,
                    Notes = dto.Notes,
                    UserId = int.Parse(userId),
                    UserName = userName,
                    SaleDate = DateTime.UtcNow,
                    Status = "Pending"
                    };

                // Add sale items
                foreach (var itemDto in dto.Items)
                    {
                    var productName = itemDto.ProductName;
                    var productSKU = itemDto.ProductSKU;

                    // Debug logging to see what we're receiving from client
                    _logger.LogInformation("Received sale item: ProductId={ProductId}, ProductName='{ProductName}', ProductSKU='{ProductSKU}'", 
                        itemDto.ProductId, productName, productSKU);

                    // Fallback: If product name/SKU not provided, use ProductId as identifier
                    if (string.IsNullOrEmpty(productName))
                        {
                        productName = $"Product ID {itemDto.ProductId}";
                        _logger.LogWarning("Product name not provided for ProductId {ProductId}, using fallback", itemDto.ProductId);
                        }
                    if (string.IsNullOrEmpty(productSKU))
                        {
                        productSKU = $"SKU-{itemDto.ProductId}";
                        _logger.LogWarning("Product SKU not provided for ProductId {ProductId}, using fallback", itemDto.ProductId);
                        }

                    sale.SaleItems.Add(new SaleItem
                        {
                        ProductId = itemDto.ProductId,
                        ProductName = productName,
                        ProductSKU = productSKU,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        DiscountAmount = itemDto.DiscountAmount
                        });
                    }

                var created = await _salesService.CreateSaleAsync(sale);

                return CreatedAtAction(nameof(GetSale), new { id = created.Id }, MapToSaleDto(created));
                }
            catch (InvalidOperationException ex)
                {
                return BadRequest(ex.Message);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error creating sale");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Cancel a sale
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CancelSale(int id, [FromBody] string reason)
            {
            try
                {
                var result = await _salesService.CancelSaleAsync(id, reason);
                if (!result)
                    return NotFound();

                return Ok(new { success = true, message = "Sale cancelled successfully" });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error cancelling sale {SaleId}", id);
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get sales statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetSalesStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
            {
            try
                {
                var totalSales = await _salesService.GetTotalSalesAsync(startDate, endDate);
                var salesCount = await _salesService.GetSalesCountAsync(startDate, endDate);
                var salesByPayment = await _salesService.GetSalesByPaymentMethodAsync(startDate, endDate);

                var stats = new
                    {
                    TotalSales = totalSales,
                    SalesCount = salesCount,
                    AverageOrderValue = salesCount > 0 ? totalSales / salesCount : 0,
                    SalesByPaymentMethod = salesByPayment
                    };

                return Ok(stats);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting sales statistics");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get dashboard statistics for sales
        /// </summary>
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
            {
            try
                {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var startOfYear = new DateTime(today.Year, 1, 1);

                var todaySales = await _salesService.GetTotalSalesAsync(today, today.AddDays(1));
                var monthSales = await _salesService.GetTotalSalesAsync(startOfMonth, today.AddDays(1));
                var yearSales = await _salesService.GetTotalSalesAsync(startOfYear, today.AddDays(1));
                
                var todayOrders = await _salesService.GetSalesCountAsync(today, today.AddDays(1));
                var monthOrders = await _salesService.GetSalesCountAsync(startOfMonth, today.AddDays(1));

                var totalCustomers = await _salesService.GetTotalCustomersAsync();
                var newCustomersThisMonth = await _salesService.GetNewCustomersCountAsync(startOfMonth, today.AddDays(1));

                var recentSales = await _salesService.GetRecentSalesAsync(10);
                var recentActivities = recentSales.Select(s => 
                    $"Sale #{s.Id} - {s.Customer?.Name ?? "Walk-in"} - ${s.TotalAmount:N2} ({s.SaleDate:HH:mm})")
                    .ToList();

                var stats = new
                    {
                    TodaySales = todaySales,
                    MonthSales = monthSales,
                    YearSales = yearSales,
                    TodayOrders = todayOrders,
                    MonthOrders = monthOrders,
                    TotalCustomers = totalCustomers,
                    NewCustomersThisMonth = newCustomersThisMonth,
                    RecentActivities = recentActivities
                    };

                return Ok(stats);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting dashboard statistics");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get top customers
        /// </summary>
        [HttpGet("top-customers")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int count = 10)
            {
            try
                {
                var topCustomers = await _salesService.GetTopCustomersAsync(count);
                var result = topCustomers.Select(tc => new
                    {
                    Customer = MapToCustomerDto(tc.Customer),
                    TotalPurchases = tc.TotalPurchases
                    });

                return Ok(result);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting top customers");
                return StatusCode(500, "Internal server error");
                }
            }

        #region Helper Methods

        private static SaleDto MapToSaleDto(Sale sale)
            {
            return new SaleDto
                {
                Id = sale.Id,
                CustomerId = sale.CustomerId,
                CustomerName = sale.Customer?.Name ?? "Unknown",
                Date = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                Status = sale.Status,
                PaymentMethod = sale.PaymentMethod,
                PaidAmount = sale.PaidAmount,
                ChangeAmount = sale.ChangeAmount,
                Notes = sale.Notes,
                UserId = sale.UserId,
                UserName = sale.UserName,
                Items = sale.SaleItems.Select(si => new SaleItemDto
                    {
                    Id = si.Id,
                    SaleId = si.SaleId,
                    ProductId = si.ProductId,
                    ProductName = si.ProductName,
                    ProductSKU = si.ProductSKU,
                    Quantity = si.Quantity,
                    UnitPrice = si.UnitPrice,
                    DiscountAmount = si.DiscountAmount
                    }).ToList()
                };
            }

        private static CustomerDto MapToCustomerDto(Customer customer)
            {
            return new CustomerDto
                {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt,
                IsActive = customer.IsActive
                };
            }

        #endregion
        }
    }