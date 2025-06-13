using InventoryPro.SalesService.Models;

namespace InventoryPro.SalesService.Services
    {
    /// <summary>
    /// Interface for sales service operations
    /// </summary>
    public interface ISalesService
        {
        // Customer operations
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer?> UpdateCustomerAsync(int id, Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);

        // Sales operations
        Task<IEnumerable<Sale>> GetAllSalesAsync();
        Task<Sale?> GetSaleByIdAsync(int id);
        Task<IEnumerable<Sale>> GetSalesByCustomerAsync(int customerId);
        Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Sale> CreateSaleAsync(Sale sale);
        Task<bool> UpdateSaleStatusAsync(int saleId, string status);
        Task<bool> CancelSaleAsync(int saleId, string reason);

        // Sales statistics
        Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetSalesCountAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, decimal>> GetSalesByPaymentMethodAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalCustomersAsync();
        Task<int> GetNewCustomersCountAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Sale>> GetRecentSalesAsync(int count = 10);
        Task<IEnumerable<(Customer Customer, decimal TotalPurchases)>> GetTopCustomersAsync(int count = 10);

        // Inventory integration (will call Product Service)
        Task<bool> CheckProductAvailabilityAsync(int productId, int quantity);
        Task<bool> UpdateProductStockAsync(int productId, int quantity);
        }
    }