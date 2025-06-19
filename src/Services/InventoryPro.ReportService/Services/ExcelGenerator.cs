using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing.Chart;
using InventoryPro.ReportService.Models;
using System.Drawing;

namespace InventoryPro.ReportService.Services
{
    /// <summary>
    /// Excel generation service using EPPlus
    /// </summary>
    public class ExcelGenerator
    {
        static ExcelGenerator()
        {
            // Set the license context for EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Generates an Excel workbook for sales report
        /// </summary>
        public static byte[] GenerateSalesReportExcel(SalesReport report)
        {
            using var package = new ExcelPackage();
            
            // Create worksheets
            var summarySheet = package.Workbook.Worksheets.Add("Summary");
            var dailySalesSheet = package.Workbook.Worksheets.Add("Daily Sales");
            var topProductsSheet = package.Workbook.Worksheets.Add("Top Products");
            var topCustomersSheet = package.Workbook.Worksheets.Add("Top Customers");
            var categorySheet = package.Workbook.Worksheets.Add("Sales by Category");

            // Generate Summary Sheet
            GenerateSalesSummarySheet(summarySheet, report);
            
            // Generate Daily Sales Sheet
            GenerateDailySalesSheet(dailySalesSheet, report);
            
            // Generate Top Products Sheet
            GenerateTopProductsSheet(topProductsSheet, report);
            
            // Generate Top Customers Sheet
            GenerateTopCustomersSheet(topCustomersSheet, report);
            
            // Generate Category Sheet
            GenerateCategorySheet(categorySheet, report);

            return package.GetAsByteArray();
        }

        /// <summary>
        /// Generates an Excel workbook for inventory report
        /// </summary>
        public static byte[] GenerateInventoryReportExcel(InventoryReport report)
        {
            using var package = new ExcelPackage();
            
            // Create worksheets
            var summarySheet = package.Workbook.Worksheets.Add("Summary");
            var detailsSheet = package.Workbook.Worksheets.Add("Product Details");
            var categorySheet = package.Workbook.Worksheets.Add("By Category");
            var lowStockSheet = package.Workbook.Worksheets.Add("Low Stock Items");

            // Generate Summary Sheet
            GenerateInventorySummarySheet(summarySheet, report);
            
            // Generate Details Sheet
            GenerateInventoryDetailsSheet(detailsSheet, report);
            
            // Generate Category Sheet
            GenerateInventoryCategorySheet(categorySheet, report);
            
            // Generate Low Stock Sheet
            GenerateLowStockSheet(lowStockSheet, report);

            return package.GetAsByteArray();
        }

        /// <summary>
        /// Generates an Excel workbook for financial report
        /// </summary>
        public static byte[] GenerateFinancialReportExcel(FinancialReport report)
        {
            using var package = new ExcelPackage();
            
            // Create worksheets
            var summarySheet = package.Workbook.Worksheets.Add("Financial Summary");
            var monthlySheet = package.Workbook.Worksheets.Add("Monthly Revenue");
            var categorySheet = package.Workbook.Worksheets.Add("Revenue by Category");

            // Generate Summary Sheet
            GenerateFinancialSummarySheet(summarySheet, report);
            
            // Generate Monthly Revenue Sheet
            GenerateMonthlyRevenueSheet(monthlySheet, report);
            
            // Generate Revenue by Category Sheet
            GenerateRevenueByCategorySheet(categorySheet, report);

            return package.GetAsByteArray();
        }

        /// <summary>
        /// Generates an Excel workbook for custom report
        /// </summary>
        public static byte[] GenerateCustomReportExcel(CustomReport report)
        {
            using var package = new ExcelPackage();
            
            // Create worksheets
            var summarySheet = package.Workbook.Worksheets.Add("Executive Summary");
            
            // Generate Executive Summary
            GenerateCustomSummarySheet(summarySheet, report);

            // Add additional sheets based on included data
            if (report.FilteredDailySales.Any())
            {
                var dailySalesSheet = package.Workbook.Worksheets.Add("Daily Sales");
                GenerateCustomDailySalesSheet(dailySalesSheet, report);
            }

            if (report.FilteredTopProducts.Any())
            {
                var productsSheet = package.Workbook.Worksheets.Add("Top Products");
                GenerateCustomTopProductsSheet(productsSheet, report);
            }

            if (report.FilteredTopCustomers.Any())
            {
                var customersSheet = package.Workbook.Worksheets.Add("Top Customers");
                GenerateCustomTopCustomersSheet(customersSheet, report);
            }

            if (report.FilteredSalesByCategory.Any())
            {
                var categorySheet = package.Workbook.Worksheets.Add("Sales by Category");
                GenerateCustomCategorySheet(categorySheet, report);
            }

            return package.GetAsByteArray();
        }

        #region Sales Report Sheets

        private static void GenerateSalesSummarySheet(ExcelWorksheet worksheet, SalesReport report)
        {
            // Title
            worksheet.Cells["A1"].Value = "SALES REPORT SUMMARY";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1:E1"].Merge = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Period
            worksheet.Cells["A2"].Value = $"Period: {report.StartDate:MMM dd, yyyy} - {report.EndDate:MMM dd, yyyy}";
            worksheet.Cells["A2:E2"].Merge = true;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Summary data
            worksheet.Cells["A4"].Value = "Metric";
            worksheet.Cells["B4"].Value = "Value";
            ApplyHeaderStyle(worksheet.Cells["A4:B4"]);

            worksheet.Cells["A5"].Value = "Total Sales";
            worksheet.Cells["B5"].Value = report.TotalSales;
            worksheet.Cells["B5"].Style.Numberformat.Format = "$#,##0.00";

            worksheet.Cells["A6"].Value = "Total Orders";
            worksheet.Cells["B6"].Value = report.TotalOrders;
            worksheet.Cells["B6"].Style.Numberformat.Format = "#,##0";

            worksheet.Cells["A7"].Value = "Average Order Value";
            worksheet.Cells["B7"].Value = report.AverageOrderValue;
            worksheet.Cells["B7"].Style.Numberformat.Format = "$#,##0.00";

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateDailySalesSheet(ExcelWorksheet worksheet, SalesReport report)
        {
            worksheet.Cells["A1"].Value = "DAILY SALES DATA";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Date";
            worksheet.Cells["B3"].Value = "Sales Amount";
            worksheet.Cells["C3"].Value = "Order Count";
            ApplyHeaderStyle(worksheet.Cells["A3:C3"]);

            // Data
            int row = 4;
            foreach (var dailySale in report.DailySales)
            {
                worksheet.Cells[row, 1].Value = dailySale.Date;
                worksheet.Cells[row, 1].Style.Numberformat.Format = "mm/dd/yyyy";
                worksheet.Cells[row, 2].Value = dailySale.TotalAmount;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 3].Value = dailySale.OrderCount;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                row++;
            }

            // Create chart
            var chart = worksheet.Drawings.AddChart("DailySalesChart", eChartType.Line);
            chart.Title.Text = "Daily Sales Trend";
            chart.SetPosition(1, 0, 4, 0);
            chart.SetSize(600, 400);
            
            var series = chart.Series.Add(worksheet.Cells[4, 2, row - 1, 2], worksheet.Cells[4, 1, row - 1, 1]);
            series.Header = "Daily Sales";

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateTopProductsSheet(ExcelWorksheet worksheet, SalesReport report)
        {
            worksheet.Cells["A1"].Value = "TOP SELLING PRODUCTS";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Product Name";
            worksheet.Cells["B3"].Value = "SKU";
            worksheet.Cells["C3"].Value = "Quantity Sold";
            worksheet.Cells["D3"].Value = "Total Revenue";
            ApplyHeaderStyle(worksheet.Cells["A3:D3"]);

            // Data
            int row = 4;
            foreach (var product in report.TopProducts)
            {
                worksheet.Cells[row, 1].Value = product.ProductName;
                worksheet.Cells[row, 2].Value = product.SKU;
                worksheet.Cells[row, 3].Value = product.QuantitySold;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 4].Value = product.TotalRevenue;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateTopCustomersSheet(ExcelWorksheet worksheet, SalesReport report)
        {
            worksheet.Cells["A1"].Value = "TOP CUSTOMERS";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Customer Name";
            worksheet.Cells["B3"].Value = "Order Count";
            worksheet.Cells["C3"].Value = "Total Amount";
            ApplyHeaderStyle(worksheet.Cells["A3:C3"]);

            // Data
            int row = 4;
            foreach (var customer in report.TopCustomers)
            {
                worksheet.Cells[row, 1].Value = customer.CustomerName;
                worksheet.Cells[row, 2].Value = customer.OrderCount;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 3].Value = customer.TotalAmount;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateCategorySheet(ExcelWorksheet worksheet, SalesReport report)
        {
            worksheet.Cells["A1"].Value = "SALES BY CATEGORY";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Category";
            worksheet.Cells["B3"].Value = "Sales Amount";
            ApplyHeaderStyle(worksheet.Cells["A3:B3"]);

            // Data
            int row = 4;
            foreach (var category in report.SalesByCategory)
            {
                worksheet.Cells[row, 1].Value = category.Key;
                worksheet.Cells[row, 2].Value = category.Value;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        #endregion

        #region Inventory Report Sheets

        private static void GenerateInventorySummarySheet(ExcelWorksheet worksheet, InventoryReport report)
        {
            worksheet.Cells["A1"].Value = "INVENTORY REPORT SUMMARY";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            worksheet.Cells["A2"].Value = $"Report Date: {report.ReportDate:MMM dd, yyyy}";
            worksheet.Cells["A2:E2"].Merge = true;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Summary data
            worksheet.Cells["A4"].Value = "Metric";
            worksheet.Cells["B4"].Value = "Value";
            ApplyHeaderStyle(worksheet.Cells["A4:B4"]);

            worksheet.Cells["A5"].Value = "Total Products";
            worksheet.Cells["B5"].Value = report.TotalProducts;

            worksheet.Cells["A6"].Value = "Active Products";
            worksheet.Cells["B6"].Value = report.ActiveProducts;

            worksheet.Cells["A7"].Value = "Low Stock Products";
            worksheet.Cells["B7"].Value = report.LowStockProducts;

            worksheet.Cells["A8"].Value = "Out of Stock Products";
            worksheet.Cells["B8"].Value = report.OutOfStockProducts;

            worksheet.Cells["A9"].Value = "Total Inventory Value";
            worksheet.Cells["B9"].Value = report.TotalInventoryValue;
            worksheet.Cells["B9"].Style.Numberformat.Format = "$#,##0.00";

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateInventoryDetailsSheet(ExcelWorksheet worksheet, InventoryReport report)
        {
            worksheet.Cells["A1"].Value = "PRODUCT INVENTORY DETAILS";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Product Name";
            worksheet.Cells["B3"].Value = "SKU";
            worksheet.Cells["C3"].Value = "Current Stock";
            worksheet.Cells["D3"].Value = "Minimum Stock";
            worksheet.Cells["E3"].Value = "Unit Price";
            worksheet.Cells["F3"].Value = "Stock Value";
            worksheet.Cells["G3"].Value = "Status";
            ApplyHeaderStyle(worksheet.Cells["A3:G3"]);

            // Data
            int row = 4;
            foreach (var product in report.ProductInventoryDetails)
            {
                worksheet.Cells[row, 1].Value = product.ProductName;
                worksheet.Cells[row, 2].Value = product.SKU;
                worksheet.Cells[row, 3].Value = product.CurrentStock;
                worksheet.Cells[row, 4].Value = product.MinimumStock;
                worksheet.Cells[row, 5].Value = product.UnitPrice;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 6].Value = product.StockValue;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 7].Value = product.StockStatus;

                // Color coding for status
                if (product.StockStatus == "Out of Stock")
                    {
                    worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }
                else if (product.StockStatus == "Low")
                    {
                    worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateInventoryCategorySheet(ExcelWorksheet worksheet, InventoryReport report)
        {
            worksheet.Cells["A1"].Value = "INVENTORY BY CATEGORY";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Category";
            worksheet.Cells["B3"].Value = "Product Count";
            ApplyHeaderStyle(worksheet.Cells["A3:B3"]);

            // Data
            int row = 4;
            foreach (var category in report.InventoryByCategory)
            {
                worksheet.Cells[row, 1].Value = category.CategoryName;
                worksheet.Cells[row, 2].Value = category.ProductCount;
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateLowStockSheet(ExcelWorksheet worksheet, InventoryReport report)
        {
            worksheet.Cells["A1"].Value = "LOW STOCK ITEMS";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            var lowStockItems = report.ProductInventoryDetails.Where(p => p.StockStatus == "Low" || p.StockStatus == "Out of Stock");

            // Headers
            worksheet.Cells["A3"].Value = "Product Name";
            worksheet.Cells["B3"].Value = "SKU";
            worksheet.Cells["C3"].Value = "Current Stock";
            worksheet.Cells["D3"].Value = "Minimum Stock";
            worksheet.Cells["E3"].Value = "Status";
            ApplyHeaderStyle(worksheet.Cells["A3:E3"]);

            // Data
            int row = 4;
            foreach (var product in lowStockItems)
            {
                worksheet.Cells[row, 1].Value = product.ProductName;
                worksheet.Cells[row, 2].Value = product.SKU;
                worksheet.Cells[row, 3].Value = product.CurrentStock;
                worksheet.Cells[row, 4].Value = product.MinimumStock;
                worksheet.Cells[row, 5].Value = product.StockStatus;

                if (product.StockStatus == "Out of Stock")
                    {
                    worksheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }
                else if (product.StockStatus == "Low")
                    {
                    worksheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        #endregion

        #region Financial Report Sheets

        private static void GenerateFinancialSummarySheet(ExcelWorksheet worksheet, FinancialReport report)
        {
            worksheet.Cells["A1"].Value = "FINANCIAL REPORT SUMMARY";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            worksheet.Cells["A2"].Value = $"Period: {report.StartDate:MMM dd, yyyy} - {report.EndDate:MMM dd, yyyy}";
            worksheet.Cells["A2:E2"].Merge = true;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Summary data
            worksheet.Cells["A4"].Value = "Financial Metric";
            worksheet.Cells["B4"].Value = "Amount";
            ApplyHeaderStyle(worksheet.Cells["A4:B4"]);

            worksheet.Cells["A5"].Value = "Gross Revenue";
            worksheet.Cells["B5"].Value = report.GrossRevenue;
            worksheet.Cells["B5"].Style.Numberformat.Format = "$#,##0.00";

            worksheet.Cells["A6"].Value = "Total Discounts";
            worksheet.Cells["B6"].Value = report.TotalDiscounts;
            worksheet.Cells["B6"].Style.Numberformat.Format = "$#,##0.00";

            worksheet.Cells["A7"].Value = "Net Revenue";
            worksheet.Cells["B7"].Value = report.NetRevenue;
            worksheet.Cells["B7"].Style.Numberformat.Format = "$#,##0.00";

            worksheet.Cells["A8"].Value = "Total Transactions";
            worksheet.Cells["B8"].Value = report.TotalTransactions;
            worksheet.Cells["B8"].Style.Numberformat.Format = "#,##0";

            worksheet.Cells["A9"].Value = "Average Transaction Value";
            worksheet.Cells["B9"].Value = report.AverageTransactionValue;
            worksheet.Cells["B9"].Style.Numberformat.Format = "$#,##0.00";

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateMonthlyRevenueSheet(ExcelWorksheet worksheet, FinancialReport report)
        {
            worksheet.Cells["A1"].Value = "MONTHLY REVENUE BREAKDOWN";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Month";
            worksheet.Cells["B3"].Value = "Revenue";
            worksheet.Cells["C3"].Value = "Transaction Count";
            worksheet.Cells["D3"].Value = "Growth %";
            ApplyHeaderStyle(worksheet.Cells["A3:D3"]);

            // Data
            int row = 4;
            foreach (var monthlyRevenue in report.MonthlyRevenue)
            {
                worksheet.Cells[row, 1].Value = $"{monthlyRevenue.Year}-{monthlyRevenue.Month:00}";
                worksheet.Cells[row, 2].Value = monthlyRevenue.Revenue;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 3].Value = monthlyRevenue.TransactionCount;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 4].Value = monthlyRevenue.Growth / 100;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "0.00%";
                row++;
            }

            // Create chart
            var chart = worksheet.Drawings.AddChart("MonthlyRevenueChart", eChartType.Line);
            chart.Title.Text = "Monthly Revenue Trend";
            chart.SetPosition(1, 0, 5, 0);
            chart.SetSize(600, 400);
            
            var series = chart.Series.Add(worksheet.Cells[4, 2, row - 1, 2], worksheet.Cells[4, 1, row - 1, 1]);
            series.Header = "Monthly Revenue";

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateRevenueByCategorySheet(ExcelWorksheet worksheet, FinancialReport report)
        {
            worksheet.Cells["A1"].Value = "REVENUE BY CATEGORY";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Category";
            worksheet.Cells["B3"].Value = "Revenue";
            ApplyHeaderStyle(worksheet.Cells["A3:B3"]);

            // Data
            int row = 4;
            foreach (var category in report.RevenueByCategory)
            {
                worksheet.Cells[row, 1].Value = category.Key;
                worksheet.Cells[row, 2].Value = category.Value;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        #endregion

        #region Custom Report Sheets

        private static void GenerateCustomSummarySheet(ExcelWorksheet worksheet, CustomReport report)
        {
            worksheet.Cells["A1"].Value = report.Title.ToUpper();
            ApplyTitleStyle(worksheet.Cells["A1"]);

            worksheet.Cells["A2"].Value = $"Period: {report.StartDate:MMM dd, yyyy} - {report.EndDate:MMM dd, yyyy}";
            worksheet.Cells["A2:E2"].Merge = true;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A3"].Value = $"Generated: {report.GeneratedAt:MMM dd, yyyy HH:mm}";
            worksheet.Cells["A3:E3"].Merge = true;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Executive Summary
            worksheet.Cells["A5"].Value = "Executive Summary";
            worksheet.Cells["B5"].Value = "Value";
            ApplyHeaderStyle(worksheet.Cells["A5:B5"]);

            worksheet.Cells["A6"].Value = "Total Revenue";
            worksheet.Cells["B6"].Value = report.TotalRevenue;
            worksheet.Cells["B6"].Style.Numberformat.Format = "$#,##0.00";

            worksheet.Cells["A7"].Value = "Total Transactions";
            worksheet.Cells["B7"].Value = report.TotalTransactions;
            worksheet.Cells["B7"].Style.Numberformat.Format = "#,##0";

            worksheet.Cells["A8"].Value = "Average Transaction Value";
            worksheet.Cells["B8"].Value = report.AverageTransactionValue;
            worksheet.Cells["B8"].Style.Numberformat.Format = "$#,##0.00";

            worksheet.Cells["A9"].Value = "Unique Customers";
            worksheet.Cells["B9"].Value = report.UniqueCustomers;
            worksheet.Cells["B9"].Style.Numberformat.Format = "#,##0";

            worksheet.Cells["A10"].Value = "Products Sold";
            worksheet.Cells["B10"].Value = report.ProductsSold;
            worksheet.Cells["B10"].Style.Numberformat.Format = "#,##0";

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateCustomDailySalesSheet(ExcelWorksheet worksheet, CustomReport report)
        {
            worksheet.Cells["A1"].Value = "DAILY SALES ANALYSIS";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Date";
            worksheet.Cells["B3"].Value = "Sales Amount";
            worksheet.Cells["C3"].Value = "Order Count";
            ApplyHeaderStyle(worksheet.Cells["A3:C3"]);

            // Data
            int row = 4;
            foreach (var dailySale in report.FilteredDailySales)
            {
                worksheet.Cells[row, 1].Value = dailySale.Date;
                worksheet.Cells[row, 1].Style.Numberformat.Format = "mm/dd/yyyy";
                worksheet.Cells[row, 2].Value = dailySale.TotalAmount;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 3].Value = dailySale.OrderCount;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateCustomTopProductsSheet(ExcelWorksheet worksheet, CustomReport report)
        {
            worksheet.Cells["A1"].Value = "TOP PERFORMING PRODUCTS";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Product Name";
            worksheet.Cells["B3"].Value = "SKU";
            worksheet.Cells["C3"].Value = "Quantity Sold";
            worksheet.Cells["D3"].Value = "Total Revenue";
            ApplyHeaderStyle(worksheet.Cells["A3:D3"]);

            // Data
            int row = 4;
            foreach (var product in report.FilteredTopProducts)
            {
                worksheet.Cells[row, 1].Value = product.ProductName;
                worksheet.Cells[row, 2].Value = product.SKU;
                worksheet.Cells[row, 3].Value = product.QuantitySold;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 4].Value = product.TotalRevenue;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateCustomTopCustomersSheet(ExcelWorksheet worksheet, CustomReport report)
        {
            worksheet.Cells["A1"].Value = "TOP CUSTOMERS ANALYSIS";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Customer Name";
            worksheet.Cells["B3"].Value = "Order Count";
            worksheet.Cells["C3"].Value = "Total Amount";
            ApplyHeaderStyle(worksheet.Cells["A3:C3"]);

            // Data
            int row = 4;
            foreach (var customer in report.FilteredTopCustomers)
            {
                worksheet.Cells[row, 1].Value = customer.CustomerName;
                worksheet.Cells[row, 2].Value = customer.OrderCount;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 3].Value = customer.TotalAmount;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        private static void GenerateCustomCategorySheet(ExcelWorksheet worksheet, CustomReport report)
        {
            worksheet.Cells["A1"].Value = "SALES BY CATEGORY ANALYSIS";
            ApplyTitleStyle(worksheet.Cells["A1"]);

            // Headers
            worksheet.Cells["A3"].Value = "Category";
            worksheet.Cells["B3"].Value = "Sales Amount";
            ApplyHeaderStyle(worksheet.Cells["A3:B3"]);

            // Data
            int row = 4;
            foreach (var category in report.FilteredSalesByCategory)
            {
                worksheet.Cells[row, 1].Value = category.Key;
                worksheet.Cells[row, 2].Value = category.Value;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                row++;
            }

            worksheet.Cells.AutoFitColumns();
        }

        #endregion

        #region Helper Methods

        private static void ApplyTitleStyle(ExcelRange range)
        {
            range.Style.Font.Size = 16;
            range.Style.Font.Bold = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
        }

        private static void ApplyHeaderStyle(ExcelRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        #endregion
    }
}