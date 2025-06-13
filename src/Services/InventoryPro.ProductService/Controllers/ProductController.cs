using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryPro.Shared.DTOs;

namespace InventoryPro.ProductService.Controllers
    {
    /// <summary>
    /// Product API controller
    /// Handles all product-related operations
    /// </summary>
    [ApiController]
    [Route("api/products")]
    [Authorize]
    public class ProductController : ControllerBase
        {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
            {
            _productService = productService;
            _logger = logger;
            }

        /// <summary>
        /// Get all products with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] PaginationParameters parameters)
            {
            try
                {
                var products = await _productService.GetAllProductsAsync();

                // Apply search filter
                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                    {
                    products = products.Where(p =>
                        p.Name.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.SKU.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.Description.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase));
                    }

                // Apply pagination
                var totalCount = products.Count();
                var items = products
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(p => new ProductDto
                        {
                        Id = p.Id,
                        Name = p.Name,
                        SKU = p.SKU,
                        Description = p.Description,
                        Price = p.Price,
                        Stock = p.StockQuantity,
                        MinStock = p.MinimumStock,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category?.Name ?? "",
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt ?? p.CreatedAt
                        })
                    .ToList();

                var response = new PagedResponse<ProductDto>
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
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
            {
            try
                {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound();

                var dto = new ProductDto
                    {
                    Id = product.Id,
                    Name = product.Name,
                    SKU = product.SKU,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.StockQuantity,
                    MinStock = product.MinimumStock,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name ?? "",
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt ?? product.CreatedAt
                    };

                return Ok(dto);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting product {ProductId}", id);
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Create new product
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
                {
                var product = new Product
                    {
                    Name = dto.Name,
                    SKU = dto.SKU,
                    Description = dto.Description,
                    Price = dto.Price,
                    StockQuantity = dto.Stock,
                    MinimumStock = dto.MinStock,
                    CategoryId = dto.CategoryId
                    };

                var created = await _productService.CreateProductAsync(product);

                // Return DTO to avoid circular reference issues
                var productDto = new ProductDto
                {
                    Id = created.Id,
                    Name = created.Name,
                    SKU = created.SKU,
                    Description = created.Description,
                    Price = created.Price,
                    Stock = created.StockQuantity,
                    MinStock = created.MinimumStock,
                    CategoryId = created.CategoryId,
                    CategoryName = created.Category?.Name ?? "",
                    IsActive = created.IsActive,
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.UpdatedAt ?? created.CreatedAt
                };

                return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, productDto);
                }
            catch (InvalidOperationException ex)
                {
                return BadRequest(ex.Message);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Update product
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductDto dto)
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
                {
                var product = new Product
                    {
                    Name = dto.Name,
                    SKU = dto.SKU,
                    Description = dto.Description,
                    Price = dto.Price,
                    StockQuantity = dto.Stock,
                    MinimumStock = dto.MinStock,
                    CategoryId = dto.CategoryId
                    };

                var updated = await _productService.UpdateProductAsync(id, product);
                if (updated == null)
                    return NotFound();

                return Ok(updated);
                }
            catch (InvalidOperationException ex)
                {
                return BadRequest(ex.Message);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Delete product
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
            {
            try
                {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                    return NotFound();

                return Ok(new { success = true });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
            {
            try
                {
                var categories = await _productService.GetAllCategoriesAsync();
                var dtos = categories.Select(c => new CategoryDto
                    {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = true,
                    ProductCount = c.Products.Count
                    }).ToList();

                return Ok(dtos);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("/api/dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
            {
            try
                {
                var products = await _productService.GetAllProductsAsync();
                var lowStockProducts = await _productService.GetLowStockProductsAsync();

                var stats = new DashboardStatsDto
                    {
                    TotalProducts = products.Count(),
                    LowStockProducts = lowStockProducts.Count(),
                    OutOfStockProducts = products.Count(p => p.StockQuantity == 0),
                    TotalInventoryValue = products.Sum(p => p.Price * p.StockQuantity),

                    // Mock data for now - should come from SalesService
                    TodaySales = 1234.56m,
                    MonthSales = 45678.90m,
                    YearSales = 567890.12m,
                    TodayOrders = 15,
                    MonthOrders = 342,
                    TotalCustomers = 1250,
                    NewCustomersThisMonth = 45,

                    RecentActivities = new List<string>
                    {
                        "New product added: Laptop Pro 15",
                        "Stock updated for Wireless Mouse",
                        "Low stock alert: USB Cable",
                        "New customer registered",
                        "Order #1234 completed"
                    },

                    TopSellingProducts = products.Take(5).Select(p => new ProductDto
                        {
                        Id = p.Id,
                        Name = p.Name,
                        SKU = p.SKU,
                        Price = p.Price,
                        Stock = p.StockQuantity
                        }).ToList(),

                    LowStockAlerts = lowStockProducts.Take(5).Select(p => new ProductDto
                        {
                        Id = p.Id,
                        Name = p.Name,
                        SKU = p.SKU,
                        Stock = p.StockQuantity,
                        MinStock = p.MinimumStock
                        }).ToList()
                    };

                return Ok(stats);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, "Internal server error");
                }
            }
        }
    }