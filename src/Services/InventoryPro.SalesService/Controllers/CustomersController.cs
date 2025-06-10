using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryPro.Shared.DTOs;
using InventoryPro.SalesService.Models;
using InventoryPro.SalesService.Services;

namespace InventoryPro.SalesService.Controllers
    {
    /// <summary>
    /// Customer API controller
    /// Handles all customer-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
        {
        private readonly ISalesService _salesService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ISalesService salesService, ILogger<CustomersController> logger)
            {
            _salesService = salesService;
            _logger = logger;
            }

        /// <summary>
        /// Get all customers with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomers([FromQuery] PaginationParameters parameters)
            {
            try
                {
                IEnumerable<Customer> customers;

                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                    {
                    customers = await _salesService.SearchCustomersAsync(parameters.SearchTerm);
                    }
                else
                    {
                    customers = await _salesService.GetAllCustomersAsync();
                    }

                // Calculate statistics for each customer
                var customerDtos = new List<CustomerDto>();
                foreach (var customer in customers)
                    {
                    var sales = await _salesService.GetSalesByCustomerAsync(customer.Id);
                    var completedSales = sales.Where(s => s.Status == "Completed").ToList();

                    var dto = new CustomerDto
                        {
                        Id = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.Phone,
                        Address = customer.Address,
                        CreatedAt = customer.CreatedAt,
                        IsActive = customer.IsActive,
                        TotalPurchases = completedSales.Sum(s => s.TotalAmount),
                        OrderCount = completedSales.Count,
                        LastOrderDate = completedSales.MaxBy(s => s.SaleDate)?.SaleDate
                        };
                    customerDtos.Add(dto);
                    }

                // Apply sorting
                if (!string.IsNullOrEmpty(parameters.SortBy))
                    {
                    customerDtos = parameters.SortBy.ToLower() switch
                        {
                            "name" => parameters.SortDirection == "desc"
                                ? customerDtos.OrderByDescending(c => c.Name).ToList()
                                : customerDtos.OrderBy(c => c.Name).ToList(),
                            "totalpurchases" => parameters.SortDirection == "desc"
                                ? customerDtos.OrderByDescending(c => c.TotalPurchases).ToList()
                                : customerDtos.OrderBy(c => c.TotalPurchases).ToList(),
                            "ordercount" => parameters.SortDirection == "desc"
                                ? customerDtos.OrderByDescending(c => c.OrderCount).ToList()
                                : customerDtos.OrderBy(c => c.OrderCount).ToList(),
                            _ => customerDtos
                            };
                    }

                // Apply pagination
                var totalCount = customerDtos.Count;
                var items = customerDtos
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToList();

                var response = new PagedResponse<CustomerDto>
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
                _logger.LogError(ex, "Error getting customers");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
            {
            try
                {
                var customer = await _salesService.GetCustomerByIdAsync(id);
                if (customer == null)
                    return NotFound();

                var sales = await _salesService.GetSalesByCustomerAsync(customer.Id);
                var completedSales = sales.Where(s => s.Status == "Completed").ToList();

                var dto = new CustomerDto
                    {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    CreatedAt = customer.CreatedAt,
                    IsActive = customer.IsActive,
                    TotalPurchases = completedSales.Sum(s => s.TotalAmount),
                    OrderCount = completedSales.Count,
                    LastOrderDate = completedSales.MaxBy(s => s.SaleDate)?.SaleDate
                    };

                return Ok(dto);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting customer {CustomerId}", id);
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Create new customer
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto dto)
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
                {
                var customer = new Customer
                    {
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Address = dto.Address
                    };

                var created = await _salesService.CreateCustomerAsync(customer);

                var resultDto = new CustomerDto
                    {
                    Id = created.Id,
                    Name = created.Name,
                    Email = created.Email,
                    Phone = created.Phone,
                    Address = created.Address,
                    CreatedAt = created.CreatedAt,
                    IsActive = created.IsActive,
                    TotalPurchases = 0,
                    OrderCount = 0,
                    LastOrderDate = null
                    };

                return CreatedAtAction(nameof(GetCustomer), new { id = created.Id }, resultDto);
                }
            catch (InvalidOperationException ex)
                {
                return BadRequest(ex.Message);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Update customer
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerDto dto)
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
                {
                var customer = new Customer
                    {
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Address = dto.Address
                    };

                var updated = await _salesService.UpdateCustomerAsync(id, customer);
                if (updated == null)
                    return NotFound();

                var sales = await _salesService.GetSalesByCustomerAsync(updated.Id);
                var completedSales = sales.Where(s => s.Status == "Completed").ToList();

                var resultDto = new CustomerDto
                    {
                    Id = updated.Id,
                    Name = updated.Name,
                    Email = updated.Email,
                    Phone = updated.Phone,
                    Address = updated.Address,
                    CreatedAt = updated.CreatedAt,
                    IsActive = updated.IsActive,
                    TotalPurchases = completedSales.Sum(s => s.TotalAmount),
                    OrderCount = completedSales.Count,
                    LastOrderDate = completedSales.MaxBy(s => s.SaleDate)?.SaleDate
                    };

                return Ok(resultDto);
                }
            catch (InvalidOperationException ex)
                {
                return BadRequest(ex.Message);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error updating customer {CustomerId}", id);
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Delete customer
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(int id)
            {
            try
                {
                var result = await _salesService.DeleteCustomerAsync(id);
                if (!result)
                    return NotFound();

                return Ok(new { success = true });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get customer purchase history
        /// </summary>
        [HttpGet("{id}/sales")]
        public async Task<IActionResult> GetCustomerSales(int id, [FromQuery] PaginationParameters parameters)
            {
            try
                {
                var customer = await _salesService.GetCustomerByIdAsync(id);
                if (customer == null)
                    return NotFound();

                var sales = await _salesService.GetSalesByCustomerAsync(id);

                // Apply search filter
                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                    {
                    sales = sales.Where(s =>
                        s.Id.ToString().Contains(parameters.SearchTerm) ||
                        s.Status.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase));
                    }

                // Apply pagination
                var totalCount = sales.Count();
                var items = sales
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(s => new SaleDto
                        {
                        Id = s.Id,
                        CustomerId = s.CustomerId,
                        CustomerName = customer.Name,
                        Date = s.SaleDate,
                        TotalAmount = s.TotalAmount,
                        Status = s.Status,
                        PaymentMethod = s.PaymentMethod,
                        PaidAmount = s.PaidAmount,
                        ChangeAmount = s.ChangeAmount,
                        Notes = s.Notes,
                        UserId = s.UserId,
                        UserName = s.UserName,
                        Items = s.SaleItems.Select(si => new SaleItemDto
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
                        })
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
                _logger.LogError(ex, "Error getting customer sales for {CustomerId}", id);
                return StatusCode(500, "Internal server error");
                }
            }
        }
    }