using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryPro.ReportService.Models;
using InventoryPro.ReportService.Services;

namespace InventoryPro.ReportService.Controllers
    {
    /// <summary>
    /// Report API controller
    /// Handles all report generation and export operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
        {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
            {
            _reportService = reportService;
            _logger = logger;
            }

        /// <summary>
        /// Generate sales report
        /// </summary>
        [HttpPost("sales")]
        public async Task<IActionResult> GenerateSalesReport([FromBody] ReportParameters parameters)
            {
            try
                {
                var report = await _reportService.GenerateSalesReportAsync(parameters);

                if (parameters.Format.ToLower() == "pdf")
                    {
                    var pdfData = await _reportService.ExportReportToPdfAsync(report, "Sales Report");
                    return File(pdfData, "application/pdf", $"SalesReport_{DateTime.Now:yyyyMMdd}.pdf");
                    }
                else if (parameters.Format.ToLower() == "excel")
                    {
                    var excelData = await _reportService.ExportReportToExcelAsync(report, "Sales Report");
                    return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"SalesReport_{DateTime.Now:yyyyMMdd}.xlsx");
                    }
                else if (parameters.Format.ToLower() == "csv")
                    {
                    var csvData = await _reportService.ExportReportToCsvAsync(report, "Sales Report");
                    return File(csvData, "text/csv", $"SalesReport_{DateTime.Now:yyyyMMdd}.csv");
                    }

                return Ok(report);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating sales report");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Generate inventory report
        /// </summary>
        [HttpPost("inventory")]
        public async Task<IActionResult> GenerateInventoryReport([FromBody] ReportParameters parameters)
            {
            try
                {
                var report = await _reportService.GenerateInventoryReportAsync(parameters);

                if (parameters.Format.ToLower() == "pdf")
                    {
                    var pdfData = await _reportService.ExportReportToPdfAsync(report, "Inventory Report");
                    return File(pdfData, "application/pdf", $"InventoryReport_{DateTime.Now:yyyyMMdd}.pdf");
                    }
                else if (parameters.Format.ToLower() == "excel")
                    {
                    var excelData = await _reportService.ExportReportToExcelAsync(report, "Inventory Report");
                    return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"InventoryReport_{DateTime.Now:yyyyMMdd}.xlsx");
                    }

                return Ok(report);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating inventory report");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Generate financial report
        /// </summary>
        [HttpPost("financial")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GenerateFinancialReport([FromBody] ReportParameters parameters)
            {
            try
                {
                var report = await _reportService.GenerateFinancialReportAsync(parameters);

                if (parameters.Format.ToLower() == "pdf")
                    {
                    var pdfData = await _reportService.ExportReportToPdfAsync(report, "Financial Report");
                    return File(pdfData, "application/pdf", $"FinancialReport_{DateTime.Now:yyyyMMdd}.pdf");
                    }
                else if (parameters.Format.ToLower() == "excel")
                    {
                    var excelData = await _reportService.ExportReportToExcelAsync(report, "Financial Report");
                    return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"FinancialReport_{DateTime.Now:yyyyMMdd}.xlsx");
                    }

                return Ok(report);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating financial report");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get daily sales data
        /// </summary>
        [HttpGet("sales/daily")]
        public async Task<IActionResult> GetDailySales([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            {
            try
                {
                var dailySales = await _reportService.GetDailySalesAsync(startDate, endDate);
                return Ok(dailySales);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting daily sales");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get top selling products
        /// </summary>
        [HttpGet("products/top-selling")]
        public async Task<IActionResult> GetTopSellingProducts(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int topCount = 10)
            {
            try
                {
                var products = await _reportService.GetTopSellingProductsAsync(startDate, endDate, topCount);
                return Ok(products);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting top selling products");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get top customers
        /// </summary>
        [HttpGet("customers/top")]
        public async Task<IActionResult> GetTopCustomers(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int topCount = 10)
            {
            try
                {
                var customers = await _reportService.GetTopCustomersAsync(startDate, endDate, topCount);
                return Ok(customers);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting top customers");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get low stock products
        /// </summary>
        [HttpGet("inventory/low-stock")]
        public async Task<IActionResult> GetLowStockProducts()
            {
            try
                {
                var products = await _reportService.GetLowStockProductsAsync();
                return Ok(products);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting low stock products");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get stock movements
        /// </summary>
        [HttpGet("inventory/movements")]
        public async Task<IActionResult> GetStockMovements([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            {
            try
                {
                var movements = await _reportService.GetStockMovementsAsync(startDate, endDate);
                return Ok(movements);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting stock movements");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get monthly revenue
        /// </summary>
        [HttpGet("financial/monthly-revenue")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetMonthlyRevenue([FromQuery] int year)
            {
            try
                {
                var revenue = await _reportService.GetMonthlyRevenueAsync(year);
                return Ok(revenue);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting monthly revenue");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get revenue by category
        /// </summary>
        [HttpGet("financial/revenue-by-category")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetRevenueByCategory([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            {
            try
                {
                var revenue = await _reportService.GetRevenueByCategory(startDate, endDate);
                return Ok(revenue);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting revenue by category");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
            {
            try
                {
                var stats = await _reportService.GetDashboardStatisticsAsync();
                return Ok(stats);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting dashboard statistics");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get recent activities
        /// </summary>
        [HttpGet("dashboard/activities")]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int count = 10)
            {
            try
                {
                var activities = await _reportService.GetRecentActivitiesAsync(count);
                return Ok(activities);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting recent activities");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Generate custom report with user-defined parameters
        /// </summary>
        [HttpPost("custom")]
        public async Task<IActionResult> GenerateCustomReport([FromBody] CustomReportParameters parameters)
            {
            try
                {
                var report = await _reportService.GenerateCustomReportAsync(parameters);

                if (parameters.Format.ToLower() == "pdf")
                    {
                    var pdfData = await _reportService.ExportReportToPdfAsync(report, "Custom Report");
                    return File(pdfData, "application/pdf", $"CustomReport_{DateTime.Now:yyyyMMdd}.pdf");
                    }
                else if (parameters.Format.ToLower() == "excel")
                    {
                    var excelData = await _reportService.ExportReportToExcelAsync(report, "Custom Report");
                    return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"CustomReport_{DateTime.Now:yyyyMMdd}.xlsx");
                    }

                return Ok(report);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating custom report");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get sales report data for viewing (used by WinForms client)
        /// </summary>
        [HttpGet("sales/data")]
        public async Task<IActionResult> GetSalesReportData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            {
            try
                {
                var reportData = await _reportService.GetSalesReportDataAsync(startDate, endDate);
                return Ok(reportData);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting sales report data");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get inventory report data for viewing (used by WinForms client)
        /// </summary>
        [HttpGet("inventory/data")]
        public async Task<IActionResult> GetInventoryReportData()
            {
            try
                {
                var reportData = await _reportService.GetInventoryReportDataAsync();
                return Ok(reportData);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting inventory report data");
                return StatusCode(500, "Internal server error");
                }
            }

        /// <summary>
        /// Get financial report data for viewing (used by WinForms client)
        /// </summary>
        [HttpGet("financial/data")]
        public async Task<IActionResult> GetFinancialReportData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            {
            try
                {
                var reportData = await _reportService.GetFinancialReportDataAsync(startDate, endDate);
                return Ok(reportData);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error getting financial report data");
                return StatusCode(500, "Internal server error");
                }
            }
        }
    }