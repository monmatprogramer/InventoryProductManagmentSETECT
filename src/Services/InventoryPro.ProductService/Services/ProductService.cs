using Microsoft.EntityFrameworkCore;

namespace InventoryPro.ProductService.Services
    {
    /// <summary>
    /// Implementation of product service
    /// Handles all product-related business logic
    /// </summary>
    public class ProductService : IProductService
        {
        private readonly ProductDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductDbContext context, ILogger<ProductService> logger)
            {
            _context = context;
            _logger = logger;
            }

        /// <summary>
        /// Gets all active products with category information
        /// </summary>
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
            {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
            }

        /// <summary>
        /// Gets product by ID
        /// </summary>
        public async Task<Product?> GetProductByIdAsync(int id)
            {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            }

        /// <summary>
        /// Gets product by SKU
        /// </summary>
        public async Task<Product?> GetProductBySkuAsync(string sku)
            {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.SKU == sku && p.IsActive);
            }

        /// <summary>
        /// Creates new product
        /// </summary>
        public async Task<Product> CreateProductAsync(Product product)
            {
            try
                {
                // Validate SKU uniqueness
                if (await _context.Products.AnyAsync(p => p.SKU == product.SKU))
                    {
                    throw new InvalidOperationException($"Product with SKU {product.SKU} already exists");
                    }

                product.CreatedAt = DateTime.UtcNow;
                _context.Products.Add(product);

                // Save product first to get the ID
                await _context.SaveChangesAsync();

                // Create initial stock movement with the generated product ID
                var stockMovement = new StockMovement
                    {
                    ProductId = product.Id,
                    Quantity = product.StockQuantity,
                    MovementType = "Initial",
                    Reason = "Initial stock",
                    CreatedBy = "System"
                    };
                _context.StockMovements.Add(stockMovement);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Product created: {ProductName} (SKU: {SKU})",
                    product.Name, product.SKU);

                return product;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error creating product");
                throw;
                }
            }

        /// <summary>
        /// Updates existing product
        /// </summary>
        public async Task<Product?> UpdateProductAsync(int id, Product product)
            {
            try
                {
                var existingProduct = await GetProductByIdAsync(id);
                if (existingProduct == null)
                    return null;

                // Check if SKU is being changed and ensure uniqueness
                if (existingProduct.SKU != product.SKU)
                    {
                    if (await _context.Products.AnyAsync(p => p.SKU == product.SKU && p.Id != id))
                        {
                        throw new InvalidOperationException($"Product with SKU {product.SKU} already exists");
                        }
                    }

                // Track stock changes
                if (existingProduct.StockQuantity != product.StockQuantity)
                    {
                    var difference = product.StockQuantity - existingProduct.StockQuantity;
                    var stockMovement = new StockMovement
                        {
                        ProductId = id,
                        Quantity = difference,
                        MovementType = "Adjustment",
                        Reason = "Manual stock adjustment",
                        CreatedBy = "System" // Should be actual user
                        };
                    _context.StockMovements.Add(stockMovement);
                    }

                // Update properties
                existingProduct.Name = product.Name;
                existingProduct.SKU = product.SKU;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.MinimumStock = product.MinimumStock;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Product updated: {ProductName} (ID: {ProductId})",
                    existingProduct.Name, id);

                return existingProduct;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error updating product");
                throw;
                }
            }

        /// <summary>
        /// Soft deletes product (marks as inactive)
        /// </summary>
        public async Task<bool> DeleteProductAsync(int id)
            {
            try
                {
                var product = await GetProductByIdAsync(id);
                if (product == null)
                    return false;

                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Product deleted: {ProductName} (ID: {ProductId})",
                    product.Name, id);

                return true;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error deleting product");
                return false;
                }
            }

        /// <summary>
        /// Updates product stock with movement tracking
        /// </summary>
        public async Task<bool> UpdateStockAsync(int productId, int quantity,
            string movementType, string reason, string userId)
            {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
                {
                var product = await GetProductByIdAsync(productId);
                if (product == null)
                    return false;

                // Update stock quantity
                product.StockQuantity += quantity;

                // Validate stock doesn't go negative
                if (product.StockQuantity < 0)
                    {
                    throw new InvalidOperationException("Insufficient stock");
                    }

                // Create stock movement record
                var stockMovement = new StockMovement
                    {
                    ProductId = productId,
                    Quantity = quantity,
                    MovementType = movementType,
                    Reason = reason,
                    CreatedBy = userId
                    };

                _context.StockMovements.Add(stockMovement);
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Stock updated for product {ProductId}: {Quantity} ({MovementType})",
                    productId, quantity, movementType);

                return true;
                }
            catch (Exception ex)
                {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating stock");
                throw;
                }
            }

        /// <summary>
        /// Gets products with stock below minimum threshold
        /// </summary>
        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
            {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity <= p.MinimumStock)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
            }

        /// <summary>
        /// Gets stock movement history for a product
        /// </summary>
        public async Task<IEnumerable<StockMovement>> GetStockMovementsAsync(int productId)
            {
            return await _context.StockMovements
                .Where(sm => sm.ProductId == productId)
                .OrderByDescending(sm => sm.MovementDate)
                .Take(50) // Last 50 movements
                .ToListAsync();
            }

        /// <summary>
        /// Gets all categories
        /// </summary>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
            {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
            }

        /// <summary>
        /// Gets category by ID
        /// </summary>
        public async Task<Category?> GetCategoryByIdAsync(int id)
            {
            return await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
            }
        }
    }
