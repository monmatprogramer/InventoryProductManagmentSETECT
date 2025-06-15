using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using InventoryPro.Shared.DTOs;
using Microsoft.Extensions.Configuration;

namespace InventoryPro.WinForms.Services
    {
    /// <summary>
    /// Implementation of API service for HTTP communication
    /// </summary>
    public class ApiService : IApiService
        {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IAuthService authService, ILogger<ApiService> logger, IConfiguration configuration)
            {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Get base URL from configuration
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

            // Ensure BaseAddress is set
            if (_httpClient.BaseAddress == null)
                {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                }

            _jsonOptions = new JsonSerializerOptions
                {
                PropertyNameCaseInsensitive = true
                };

            _logger.LogInformation("ApiService initialized with BaseUrl: {BaseUrl}", _baseUrl);
            }

        #region Authentication

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
            {
            try
                {
                var uri = _httpClient.BaseAddress != null ? "auth/login" : $"{_baseUrl}/auth/login";
                var response = await _httpClient.PostAsJsonAsync(uri, loginRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    {

                    var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);
                    if (loginResponse != null)
                        {
                        // Store token and user info
                        await _authService.SetTokenAsync(loginResponse.Token);
                        await _authService.SetCurrentUserAsync(loginResponse.User);

                        return new ApiResponse<LoginResponseDto>
                            {
                            Success = true,
                            Data = loginResponse,
                            StatusCode = (int)response.StatusCode
                            };
                        }
                    }

                return new ApiResponse<LoginResponseDto>
                    {
                    Success = false,
                    Message = "Login failed",
                    StatusCode = (int)response.StatusCode
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during login");
                return new ApiResponse<LoginResponseDto>
                    {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.ToString() }
                    };
                }
            }

        public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequestDto registerRequest)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("auth/register", registerRequest);
                return await HandleResponse<UserDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during registration");
                return CreateErrorResponse<UserDto>(ex);
                }
            }

        public async Task<ApiResponse<bool>> LogoutAsync()
            {
            try
                {
                await _authService.ClearTokenAsync();
                return new ApiResponse<bool>
                    {
                    Success = true,
                    Data = true,
                    Message = "Logged out successfully"
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during logout");
                return CreateErrorResponse<bool>(ex);
                }
            }

        public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken)
            {
            try
                {
                var response = await _httpClient.PostAsJsonAsync("auth/refresh", new { refreshToken });
                return await HandleResponse<LoginResponseDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error refreshing token");
                return CreateErrorResponse<LoginResponseDto>(ex);
                }
            }

        #endregion

        #region Products

        public async Task<ApiResponse<PagedResponse<ProductDto>>> GetProductsAsync(PaginationParameters parameters)
            {
            try
                {
                await AddAuthorizationHeader();
                var queryString = BuildQueryString(parameters);
                var response = await _httpClient.GetAsync($"products?{queryString}");
                return await HandleResponse<PagedResponse<ProductDto>>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting products");
                return CreateErrorResponse<PagedResponse<ProductDto>>(ex);
                }
            }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"products/{id}");
                return await HandleResponse<ProductDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting product by ID");
                return CreateErrorResponse<ProductDto>(ex);
                }
            }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto product)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("products", product);
                return await HandleResponse<ProductDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error creating product");
                return CreateErrorResponse<ProductDto>(ex);
                }
            }

        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, CreateProductDto product)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync($"products/{id}", product);
                return await HandleResponse<ProductDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error updating product");
                return CreateErrorResponse<ProductDto>(ex);
                }
            }

        public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"products/{id}");
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Delete product response content: {Content}", content);

                if (response.IsSuccessStatusCode)
                    {
                    return new ApiResponse<bool>
                        {
                        Success = true,
                        Data = true,
                        StatusCode = (int)response.StatusCode
                        };
                    }

                return new ApiResponse<bool>
                    {
                    Success = false,
                    Message = $"Request failed with status {response.StatusCode}: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error deleting product");
                return CreateErrorResponse<bool>(ex);
                }
            }

        #endregion

        #region Categories

        public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync()
            {
            try
                {
                _logger.LogInformation("Making request to products/categories endpoint");
                await AddAuthorizationHeader();
                var response = await _httpClient.GetAsync("products/categories");
                _logger.LogInformation("Categories response: {StatusCode}", response.StatusCode);
                return await HandleResponse<List<CategoryDto>>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting categories");
                return CreateErrorResponse<List<CategoryDto>>(ex);
                }
            }

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CategoryDto category)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("products/categories", category);
                return await HandleResponse<CategoryDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error creating category");
                return CreateErrorResponse<CategoryDto>(ex);
                }
            }

        #endregion

        #region Customers

        public async Task<ApiResponse<PagedResponse<CustomerDto>>> GetCustomersAsync(PaginationParameters parameters)
            {
            try
                {
                await AddAuthorizationHeader();
                var queryString = BuildQueryString(parameters);
                var response = await _httpClient.GetAsync($"customers?{queryString}");
                return await HandleResponse<PagedResponse<CustomerDto>>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting customers");
                return CreateErrorResponse<PagedResponse<CustomerDto>>(ex);
                }
            }

        public async Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int id)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"customers/{id}");
                return await HandleResponse<CustomerDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting customer by ID");
                return CreateErrorResponse<CustomerDto>(ex);
                }
            }

        public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerDto customer)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("customers", customer);
                return await HandleResponse<CustomerDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error creating customer");
                return CreateErrorResponse<CustomerDto>(ex);
                }
            }

        public async Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(int id, CustomerDto customer)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync($"customers/{id}", customer);
                return await HandleResponse<CustomerDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error updating customer");
                return CreateErrorResponse<CustomerDto>(ex);
                }
            }

        public async Task<ApiResponse<bool>> DeleteCustomerAsync(int id)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"customers/{id}");
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Delete customer response content: {Content}", content);

                if (response.IsSuccessStatusCode)
                    {
                    return new ApiResponse<bool>
                        {
                        Success = true,
                        Data = true,
                        StatusCode = (int)response.StatusCode
                        };
                    }

                return new ApiResponse<bool>
                    {
                    Success = false,
                    Message = $"Request failed with status {response.StatusCode}: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error deleting customer");
                return CreateErrorResponse<bool>(ex);
                }
            }

        #endregion

        #region Sales

        public async Task<ApiResponse<PagedResponse<SaleDto>>> GetSalesAsync(PaginationParameters parameters)
            {
            try
                {
                await AddAuthorizationHeader();
                var queryString = BuildQueryString(parameters);
                var response = await _httpClient.GetAsync($"sales?{queryString}");
                return await HandleResponse<PagedResponse<SaleDto>>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting sales");
                return CreateErrorResponse<PagedResponse<SaleDto>>(ex);
                }
            }

        public async Task<ApiResponse<SaleDto>> GetSaleByIdAsync(int id)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"sales/{id}");
                return await HandleResponse<SaleDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting sale by ID");
                return CreateErrorResponse<SaleDto>(ex);
                }
            }

        public async Task<ApiResponse<SaleDto>> CreateSaleAsync(CreateSaleDto sale)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("sales", sale);
                return await HandleResponse<SaleDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error creating sale");
                return CreateErrorResponse<SaleDto>(ex);
                }
            }

        #endregion

        #region Dashboard

        public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync()
            {
            try
                {
                await AddAuthorizationHeader();
                _logger.LogInformation("Making request to dashboard/stats endpoint");
                var response = await _httpClient.GetAsync("dashboard/stats");
                _logger.LogInformation("Dashboard stats response: {StatusCode}", response.StatusCode);
                return await HandleResponse<DashboardStatsDto>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting dashboard stats");
                return CreateErrorResponse<DashboardStatsDto>(ex);
                }
            }

        #endregion

        #region Helper Methods

        private async Task AddAuthorizationHeader()
            {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
                {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
            {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Response content (first 500 chars): {Content}", 
                content.Length > 500 ? content.Substring(0, 500) + "..." : content);

            if (response.IsSuccessStatusCode)
                {
                try
                    {
                    var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    return new ApiResponse<T>
                        {
                        Success = true,
                        Data = data,
                        StatusCode = (int)response.StatusCode
                        };
                    }
                catch (JsonException ex)
                    {
                    _logger.LogError(ex, "Error deserializing response. Content: {Content}", content);
                    return new ApiResponse<T>
                        {
                        Success = false,
                        Message = "Error processing server response",
                        StatusCode = (int)response.StatusCode
                        };
                    }
                }

            // Handle error response - improved logging
            _logger.LogWarning("API request failed. Status: {StatusCode}, Content: {Content}", 
                response.StatusCode, content);

            try
                {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                return errorResponse ?? new ApiResponse<T>
                    {
                    Success = false,
                    Message = $"Request failed with status {response.StatusCode}: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                    };
                }
            catch
                {
                return new ApiResponse<T>
                    {
                    Success = false,
                    Message = $"Request failed with status {response.StatusCode}: {response.ReasonPhrase}. Response: {content}",
                    StatusCode = (int)response.StatusCode
                    };
                }
            }

        private string BuildQueryString(PaginationParameters parameters)
            {
            var queryParams = new List<string>
            {
                $"pageNumber={parameters.PageNumber}",
                $"pageSize={parameters.PageSize}"
            };

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(parameters.SearchTerm)}");

            if (!string.IsNullOrEmpty(parameters.SortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(parameters.SortBy)}");

            if (!string.IsNullOrEmpty(parameters.SortDirection))
                queryParams.Add($"sortDirection={Uri.EscapeDataString(parameters.SortDirection)}");

            return string.Join("&", queryParams);
            }

        #region Report Methods

        /// <summary>
        /// Generate sales report
        /// </summary>
        public async Task<ApiResponse<byte[]>> GenerateSalesReportAsync(DateTime startDate, DateTime endDate, string format)
            {
            try
                {
                await AddAuthorizationHeader();

                var requestData = new
                    {
                    StartDate = startDate,
                    EndDate = endDate,
                    Format = format
                    };

                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("reports/sales", content);
                
                if (response.IsSuccessStatusCode)
                    {
                    var fileContent = await response.Content.ReadAsByteArrayAsync();
                    return new ApiResponse<byte[]>
                        {
                        Success = true,
                        Data = fileContent,
                        StatusCode = (int)response.StatusCode
                        };
                    }

                return new ApiResponse<byte[]>
                    {
                    Success = false,
                    Message = $"Failed to generate sales report: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating sales report");
                return CreateErrorResponse<byte[]>(ex);
                }
            }

        /// <summary>
        /// Generate inventory report
        /// </summary>
        public async Task<ApiResponse<byte[]>> GenerateInventoryReportAsync(string format)
            {
            try
                {
                await AddAuthorizationHeader();

                var requestData = new { Format = format };
                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("reports/inventory", content);
                
                if (response.IsSuccessStatusCode)
                    {
                    var fileContent = await response.Content.ReadAsByteArrayAsync();
                    return new ApiResponse<byte[]>
                        {
                        Success = true,
                        Data = fileContent,
                        StatusCode = (int)response.StatusCode
                        };
                    }

                return new ApiResponse<byte[]>
                    {
                    Success = false,
                    Message = $"Failed to generate inventory report: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating inventory report");
                return CreateErrorResponse<byte[]>(ex);
                }
            }

        /// <summary>
        /// Generate financial report
        /// </summary>
        public async Task<ApiResponse<byte[]>> GenerateFinancialReportAsync(DateTime startDate, DateTime endDate, string format)
            {
            try
                {
                await AddAuthorizationHeader();

                var requestData = new
                    {
                    StartDate = startDate,
                    EndDate = endDate,
                    Format = format
                    };

                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("reports/financial", content);
                
                if (response.IsSuccessStatusCode)
                    {
                    var fileContent = await response.Content.ReadAsByteArrayAsync();
                    return new ApiResponse<byte[]>
                        {
                        Success = true,
                        Data = fileContent,
                        StatusCode = (int)response.StatusCode
                        };
                    }

                return new ApiResponse<byte[]>
                    {
                    Success = false,
                    Message = $"Failed to generate financial report: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating financial report");
                return CreateErrorResponse<byte[]>(ex);
                }
            }

        /// <summary>
        /// Get daily sales data
        /// </summary>
        public async Task<ApiResponse<List<object>>> GetDailySalesAsync(DateTime startDate, DateTime endDate)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"reports/sales/daily?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
                return await HandleResponse<List<object>>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting daily sales");
                return CreateErrorResponse<List<object>>(ex);
                }
            }

        /// <summary>
        /// Get top selling products
        /// </summary>
        public async Task<ApiResponse<List<object>>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int count = 10)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"reports/products/top-selling?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&topCount={count}");
                return await HandleResponse<List<object>>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting top selling products");
                return CreateErrorResponse<List<object>>(ex);
                }
            }

        /// <summary>
        /// Get top customers
        /// </summary>
        public async Task<ApiResponse<List<object>>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, int count = 10)
            {
            try
                {
                await AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"reports/customers/top?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&topCount={count}");
                return await HandleResponse<List<object>>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting top customers");
                return CreateErrorResponse<List<object>>(ex);
                }
            }

        /// <summary>
        /// Generate custom report as file export
        /// </summary>
        public async Task<ApiResponse<byte[]>> GenerateCustomReportAsync(object parameters)
            {
            try
                {
                await AddAuthorizationHeader();

                var json = JsonSerializer.Serialize(parameters, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("reports/custom", content);
                
                if (response.IsSuccessStatusCode)
                    {
                    var fileContent = await response.Content.ReadAsByteArrayAsync();
                    return new ApiResponse<byte[]>
                        {
                        Success = true,
                        Data = fileContent,
                        StatusCode = (int)response.StatusCode
                        };
                    }

                return new ApiResponse<byte[]>
                    {
                    Success = false,
                    Message = $"Failed to generate custom report: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating custom report");
                return CreateErrorResponse<byte[]>(ex);
                }
            }

        /// <summary>
        /// Get sales report data for viewing
        /// </summary>
        public async Task<ApiResponse<object>> GetSalesReportDataAsync(DateTime startDate, DateTime endDate)
            {
            try
                {
                await AddAuthorizationHeader();

                var requestData = new
                    {
                    StartDate = startDate,
                    EndDate = endDate,
                    Format = "View"
                    };

                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("reports/sales", content);
                return await HandleResponse<object>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting sales report data");
                return CreateErrorResponse<object>(ex);
                }
            }

        /// <summary>
        /// Get inventory report data for viewing
        /// </summary>
        public async Task<ApiResponse<object>> GetInventoryReportDataAsync()
            {
            try
                {
                await AddAuthorizationHeader();

                var requestData = new { Format = "View" };
                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("reports/inventory", content);
                return await HandleResponse<object>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting inventory report data");
                return CreateErrorResponse<object>(ex);
                }
            }

        /// <summary>
        /// Get financial report data for viewing
        /// </summary>
        public async Task<ApiResponse<object>> GetFinancialReportDataAsync(DateTime startDate, DateTime endDate)
            {
            try
                {
                await AddAuthorizationHeader();

                var requestData = new
                    {
                    StartDate = startDate,
                    EndDate = endDate,
                    Format = "View"
                    };

                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("reports/financial", content);
                return await HandleResponse<object>(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting financial report data");
                return CreateErrorResponse<object>(ex);
                }
            }

        #endregion

        private ApiResponse<T> CreateErrorResponse<T>(Exception ex)
            {
            return new ApiResponse<T>
                {
                Success = false,
                Message = ex.Message,
                Errors = new List<string> { ex.ToString() }
                };
            }

        #endregion
        }
    }