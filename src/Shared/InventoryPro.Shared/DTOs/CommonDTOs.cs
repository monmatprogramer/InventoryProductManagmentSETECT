namespace InventoryPro.Shared.DTOs
    {
    /// <summary>
    /// Generic response wrapper for API calls
    /// Provides consistent structure for all API responses
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
        {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }

    /// <summary>
    /// Pagination wrapper for list responses
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    public class PagedResponse<T>
        {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        }

    /// <summary>
    /// Parameters for pagination requests
    /// </summary>
    public class PaginationParameters
        {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
            {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
            }

        public string SearchTerm { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc"; // asc or desc
        }

    /// <summary>
    /// Dashboard statistics DTO
    /// Contains key metrics for the main dashboard
    /// </summary>
    public class DashboardStatsDto
        {
        // Product statistics
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }

        // Sales statistics
        public decimal TodaySales { get; set; }
        public decimal MonthSales { get; set; }
        public decimal YearSales { get; set; }
        public int TodayOrders { get; set; }
        public int MonthOrders { get; set; }

        // Customer statistics
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }

        // Recent activities
        public List<string> RecentActivities { get; set; } = new();

        // Top selling products
        public List<ProductDto> TopSellingProducts { get; set; } = new();

        // Low stock alerts
        public List<ProductDto> LowStockAlerts { get; set; } = new();
        }
    }