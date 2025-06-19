using InventoryPro.ReportService.Models;
using Microsoft.Extensions.Logging;

namespace InventoryPro.ReportService.Services
{
    /// <summary>
    /// Helper methods for report generation fallbacks
    /// </summary>
    public static class ReportHelpers
    {
        public static string GetSimpleReportSummary(object report, string reportType, ILogger logger)
        {
            try
            {
                return reportType.ToLower() switch
                {
                    "sales report" when report is SalesReport salesReport =>
                        $"Total Sales: ${salesReport.TotalSales:N2}\n" +
                        $"Total Orders: {salesReport.TotalOrders:N0}\n" +
                        $"Average Order: ${salesReport.AverageOrderValue:N2}\n" +
                        $"Date Range: {salesReport.StartDate:yyyy-MM-dd} to {salesReport.EndDate:yyyy-MM-dd}",
                    
                    "inventory report" when report is InventoryReport inventoryReport =>
                        $"Total Products: {inventoryReport.TotalProducts:N0}\n" +
                        $"Active Products: {inventoryReport.ActiveProducts:N0}\n" +
                        $"Low Stock Items: {inventoryReport.LowStockProducts:N0}\n" +
                        $"Total Value: ${inventoryReport.TotalInventoryValue:N2}",
                    
                    "financial report" when report is FinancialReport financialReport =>
                        $"Gross Revenue: ${financialReport.GrossRevenue:N2}\n" +
                        $"Net Revenue: ${financialReport.NetRevenue:N2}\n" +
                        $"Total Transactions: {financialReport.TotalTransactions:N0}\n" +
                        $"Average Transaction: ${financialReport.AverageTransactionValue:N2}",
                    
                    "custom report" when report is CustomReport customReport =>
                        $"Total Revenue: ${customReport.TotalRevenue:N2}\n" +
                        $"Total Transactions: {customReport.TotalTransactions:N0}\n" +
                        $"Unique Customers: {customReport.UniqueCustomers:N0}\n" +
                        $"Products Sold: {customReport.ProductsSold:N0}",
                    
                    _ => "Report data is available but cannot be displayed in simplified format."
                };
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error generating simple report summary");
                return "Unable to generate report summary.";
            }
        }

        public static string GetCsvReportData(object report, string reportType, ILogger logger)
        {
            try
            {
                return reportType.ToLower() switch
                {
                    "sales report" when report is SalesReport salesReport =>
                        "Metric,Value\n" +
                        $"Total Sales,${salesReport.TotalSales:N2}\n" +
                        $"Total Orders,{salesReport.TotalOrders:N0}\n" +
                        $"Average Order Value,${salesReport.AverageOrderValue:N2}\n" +
                        $"Start Date,{salesReport.StartDate:yyyy-MM-dd}\n" +
                        $"End Date,{salesReport.EndDate:yyyy-MM-dd}",
                    
                    "inventory report" when report is InventoryReport inventoryReport =>
                        "Metric,Value\n" +
                        $"Total Products,{inventoryReport.TotalProducts:N0}\n" +
                        $"Active Products,{inventoryReport.ActiveProducts:N0}\n" +
                        $"Low Stock Products,{inventoryReport.LowStockProducts:N0}\n" +
                        $"Out of Stock Products,{inventoryReport.OutOfStockProducts:N0}\n" +
                        $"Total Inventory Value,${inventoryReport.TotalInventoryValue:N2}",
                    
                    "financial report" when report is FinancialReport financialReport =>
                        "Metric,Value\n" +
                        $"Gross Revenue,${financialReport.GrossRevenue:N2}\n" +
                        $"Total Discounts,${financialReport.TotalDiscounts:N2}\n" +
                        $"Net Revenue,${financialReport.NetRevenue:N2}\n" +
                        $"Total Transactions,{financialReport.TotalTransactions:N0}\n" +
                        $"Average Transaction Value,${financialReport.AverageTransactionValue:N2}",
                    
                    "custom report" when report is CustomReport customReport =>
                        "Metric,Value\n" +
                        $"Total Revenue,${customReport.TotalRevenue:N2}\n" +
                        $"Total Transactions,{customReport.TotalTransactions:N0}\n" +
                        $"Average Transaction Value,{customReport.AverageTransactionValue:N2}\n" +
                        $"Unique Customers,{customReport.UniqueCustomers:N0}\n" +
                        $"Products Sold,{customReport.ProductsSold:N0}",
                    
                    _ => "Report Type,Value\nData,Not available in CSV format"
                };
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error generating CSV report data");
                return "Error,Unable to generate CSV data";
            }
        }
    }
}