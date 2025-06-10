using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Services
    {
    /// <summary>
    /// Interface for API service that handles all HTTP communication with the backend services
    /// This provides a contract for making HTTP requests to our microservices through the API Gateway
    /// </summary>
    public interface IApiService
        {
        // Authentication endpoints
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest);
        Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequestDto registerRequest);
        Task<ApiResponse<bool>> LogoutAsync();
        Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken);

        // Product endpoints
        Task<ApiResponse<PagedResponse<ProductDto>>> GetProductsAsync(PaginationParameters parameters);
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto product);
        Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, CreateProductDto product);
        Task<ApiResponse<bool>> DeleteProductAsync(int id);

        // Category endpoints
        Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync();
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CategoryDto category);

        // Customer endpoints
        Task<ApiResponse<PagedResponse<CustomerDto>>> GetCustomersAsync(PaginationParameters parameters);
        Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int id);
        Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerDto customer);
        Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(int id, CustomerDto customer);

        // Sales endpoints
        Task<ApiResponse<PagedResponse<SaleDto>>> GetSalesAsync(PaginationParameters parameters);
        Task<ApiResponse<SaleDto>> GetSaleByIdAsync(int id);
        Task<ApiResponse<SaleDto>> CreateSaleAsync(CreateSaleDto sale);

        // Dashboard endpoints
        Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync();
        }
    }