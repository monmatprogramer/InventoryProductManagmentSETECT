using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing.Chart;
using System.Drawing;
using InventoryPro.ReportService.Models;

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
        /// Generates Excel workbook for inventory report
        /// </summary>
        public static byte[] GenerateInventoryReportExcel(InventoryReport report)
        {
            using var package = new ExcelPackage();
            
            // Create worksheets
            var summarySheet = package.Workbook.Worksheets.Add("Inventory Summary");
            var detailsSheet = package.Workbook.Worksheets.Add("Product Details");
            var categoriesSheet = package.Workbook.Worksheets.Add("Categories");

            // Generate Summary Sheet
            GenerateInventorySummarySheet(summarySheet, report);
            
            // Generate Product Details Sheet
            GenerateInventoryDetailsSheet(detailsSheet, report);
            
            // Generate Categories Sheet
            GenerateInventoryCategoriesSheet(categoriesSheet, report);

            return package.GetAsByteArray();
        }

        /// <summary>
        /// Generates Excel workbook for sales report
        /// </summary>
        public static byte[] GenerateSalesReportExcel(SalesReport report)
        {
            using var package = new ExcelPackage();
            
            // Create worksheets
            var summarySheet = package.Workbook.Worksheets.Add("Sales Summary");
            var dailySalesSheet = package.Workbook.Worksheets.Add("Daily Sales");
            var productsSheet = package.Workbook.Worksheets.Add("Top Products");
            var customersSheet = package.Workbook.Worksheets.Add("Top Customers");

            // Generate sheets
            GenerateSalesSummarySheet(summarySheet, report);
            GenerateDailySalesSheet(dailySalesSheet, report);
            GenerateTopProductsSheet(productsSheet, report);
            GenerateTopCustomersSheet(customersSheet, report);

            return package.GetAsByteArray();
        }

        private static void GenerateInventorySummarySheet(ExcelWorksheet sheet, InventoryReport report)
        {
            // Title
            sheet.Cells["A1"].Value = "INVENTORY REPORT SUMMARY";
            sheet.Cells["A1:E1"].Merge = true;
            sheet.Cells["A1"].Style.Font.Size = 16;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

            // Report date
            sheet.Cells["A3"].Value = "Report Date:";
            sheet.Cells["B3"].Value = report.ReportDate.ToString("MMM dd, yyyy");
            sheet.Cells["A3"].Style.Font.Bold = true;

            // Summary data
            var row = 5;
            var summaryData = new[]
            {
                ("Total Products", report.TotalProducts.ToString("N0")),
                ("Active Products", report.ActiveProducts.ToString("N0")),
                ("Inactive Products", report.InactiveProducts.ToString("N0")),
                ("Low Stock Products", report.LowStockProducts.ToString("N0")),
                ("Out of Stock Products", report.OutOfStockProducts.ToString("N0")),
                ("Total Inventory Value", $"${report.TotalInventoryValue:N2}")
            };

            sheet.Cells["A4"].Value = "Metric";
            sheet.Cells["B4"].Value = "Value";
            sheet.Cells["A4:B4"].Style.Font.Bold = true;
            sheet.Cells["A4:B4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A4:B4"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            foreach (var (metric, value) in summaryData)
            {
                sheet.Cells[row, 1].Value = metric;
                sheet.Cells[row, 2].Value = value;
                row++;
            }

            // Auto-fit columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private static void GenerateInventoryDetailsSheet(ExcelWorksheet sheet, InventoryReport report)
        {
            // Title
            sheet.Cells["A1"].Value = "PRODUCT INVENTORY DETAILS";
            sheet.Cells["A1:G1"].Merge = true;
            sheet.Cells["A1"].Style.Font.Size = 14;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

            // Headers
            var headers = new[] { "Product Name", "SKU", "Category", "Current Stock", "Min Stock", "Unit Price", "Stock Value", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[3, i + 1].Value = headers[i];
                sheet.Cells[3, i + 1].Style.Font.Bold = true;
                sheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            // Data
            var row = 4;
            foreach (var product in report.ProductInventoryDetails)
            {
                sheet.Cells[row, 1].Value = product.ProductName;
                sheet.Cells[row, 2].Value = product.SKU;
                sheet.Cells[row, 3].Value = product.Category;
                sheet.Cells[row, 4].Value = product.CurrentStock;
                sheet.Cells[row, 5].Value = product.MinimumStock;
                sheet.Cells[row, 6].Value = product.UnitPrice;
                sheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
                sheet.Cells[row, 7].Value = product.StockValue;
                sheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";
                sheet.Cells[row, 8].Value = product.StockStatus;

                // Color code status
                if (product.StockStatus == "Out of Stock")
                {
                    sheet.Cells[row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                }
                else if (product.StockStatus == "Low")
                {
                    sheet.Cells[row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                }

                row++;
            }

            // Auto-fit columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            // Add borders
            var range = sheet.Cells[3, 1, row - 1, headers.Length];
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        private static void GenerateInventoryCategoriesSheet(ExcelWorksheet sheet, InventoryReport report)
        {
            // Title
            sheet.Cells["A1"].Value = "INVENTORY BY CATEGORY";
            sheet.Cells["A1:D1"].Merge = true;
            sheet.Cells["A1"].Style.Font.Size = 14;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

            // Headers
            sheet.Cells["A3"].Value = "Category";
            sheet.Cells["B3"].Value = "Total Stock";
            sheet.Cells["C3"].Value = "Total Value";
            sheet.Cells["A3:C3"].Style.Font.Bold = true;
            sheet.Cells["A3:C3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A3:C3"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            // Data
            var row = 4;
            foreach (var category in report.InventoryByCategory)
            {
                sheet.Cells[row, 1].Value = category.CategoryName;
                sheet.Cells[row, 2].Value = category.TotalStock;
                sheet.Cells[row, 3].Value = category.TotalValue;
                sheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
                row++;
            }

            // Auto-fit columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            // Create pie chart
            try
            {
                var chart = sheet.Drawings.AddChart("CategoryChart", eChartType.Pie);
                chart.Title.Text = "Inventory Distribution by Category";
                chart.SetPosition(row + 2, 0, 1, 0);
                chart.SetSize(400, 300);

                var series = chart.Series.Add(sheet.Cells[4, 2, row - 1, 2], sheet.Cells[4, 1, row - 1, 1]);
                series.Header = "Stock Distribution";
            }
            catch (Exception)
            {
                // Chart creation failed, continue without chart
            }
        }

        private static void GenerateSalesSummarySheet(ExcelWorksheet sheet, SalesReport report)
        {
            // Title and period
            sheet.Cells["A1"].Value = "SALES REPORT SUMMARY";
            sheet.Cells["A1:E1"].Merge = true;
            sheet.Cells["A1"].Style.Font.Size = 16;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

            sheet.Cells["A3"].Value = $"Period: {report.StartDate:MMM dd, yyyy} - {report.EndDate:MMM dd, yyyy}";
            sheet.Cells["A3"].Style.Font.Bold = true;

            // Summary metrics
            var summaryData = new[]
            {
                ("Total Sales", $"${report.TotalSales:N2}"),
                ("Total Orders", report.TotalOrders.ToString("N0")),
                ("Average Order Value", $"${report.AverageOrderValue:N2}")
            };

            sheet.Cells["A5"].Value = "Metric";
            sheet.Cells["B5"].Value = "Value";
            sheet.Cells["A5:B5"].Style.Font.Bold = true;
            sheet.Cells["A5:B5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A5:B5"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            var row = 6;
            foreach (var (metric, value) in summaryData)
            {
                sheet.Cells[row, 1].Value = metric;
                sheet.Cells[row, 2].Value = value;
                row++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private static void GenerateDailySalesSheet(ExcelWorksheet sheet, SalesReport report)
        {
            // Title
            sheet.Cells["A1"].Value = "DAILY SALES DATA";
            sheet.Cells["A1:C1"].Merge = true;
            sheet.Cells["A1"].Style.Font.Size = 14;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

            // Headers
            sheet.Cells["A3"].Value = "Date";
            sheet.Cells["B3"].Value = "Total Amount";
            sheet.Cells["C3"].Value = "Order Count";
            sheet.Cells["A3:C3"].Style.Font.Bold = true;
            sheet.Cells["A3:C3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A3:C3"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            // Data
            var row = 4;
            foreach (var daily in report.DailySales.OrderBy(d => d.Date))
            {
                sheet.Cells[row, 1].Value = daily.Date.ToString("MMM dd, yyyy");
                sheet.Cells[row, 2].Value = daily.TotalAmount;
                sheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                sheet.Cells[row, 3].Value = daily.OrderCount;
                row++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private static void GenerateTopProductsSheet(ExcelWorksheet sheet, SalesReport report)
        {
            // Title
            sheet.Cells["A1"].Value = "TOP SELLING PRODUCTS";
            sheet.Cells["A1:D1"].Merge = true;
            sheet.Cells["A1"].Style.Font.Size = 14;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

            // Headers
            var headers = new[] { "Product Name", "SKU", "Quantity Sold", "Total Revenue" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[3, i + 1].Value = headers[i];
                sheet.Cells[3, i + 1].Style.Font.Bold = true;
                sheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            // Data
            var row = 4;
            foreach (var product in report.TopProducts)
            {
                sheet.Cells[row, 1].Value = product.ProductName;
                sheet.Cells[row, 2].Value = product.SKU;
                sheet.Cells[row, 3].Value = product.QuantitySold;
                sheet.Cells[row, 4].Value = product.TotalRevenue;
                sheet.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";
                row++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private static void GenerateTopCustomersSheet(ExcelWorksheet sheet, SalesReport report)
        {
            // Title
            sheet.Cells["A1"].Value = "TOP CUSTOMERS";
            sheet.Cells["A1:C1"].Merge = true;
            sheet.Cells["A1"].Style.Font.Size = 14;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

            // Headers
            sheet.Cells["A3"].Value = "Customer Name";
            sheet.Cells["B3"].Value = "Order Count";
            sheet.Cells["C3"].Value = "Total Amount";
            sheet.Cells["A3:C3"].Style.Font.Bold = true;
            sheet.Cells["A3:C3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells["A3:C3"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            // Data
            var row = 4;
            foreach (var customer in report.TopCustomers)
            {
                sheet.Cells[row, 1].Value = customer.CustomerName;
                sheet.Cells[row, 2].Value = customer.OrderCount;
                sheet.Cells[row, 3].Value = customer.TotalAmount;
                sheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
                row++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }
    }
}