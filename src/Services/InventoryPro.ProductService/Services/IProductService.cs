namespace InventoryPro.ProductService.Services
    {
    /// <summary>
    /// Interface for product service operations
    /// </summary>
    public interface IProductService
        {
        // Product operations
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product?> GetProductBySkuAsync(string sku);
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> UpdateProductAsync(int id, Product product);
        Task<bool> DeleteProductAsync(int id);

        // Stock operations
        Task<bool> UpdateStockAsync(int productId, int quantity, string movementType, string reason, string userId);
        Task<IEnumerable<Product>> GetLowStockProductsAsync();
        Task<IEnumerable<StockMovement>> GetStockMovementsAsync(int productId);

        // Category operations
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        }
    }