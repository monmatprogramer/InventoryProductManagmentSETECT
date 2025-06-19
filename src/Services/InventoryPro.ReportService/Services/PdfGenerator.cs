using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using InventoryPro.ReportService.Models;
using InventoryPro.Shared.DTOs;

namespace InventoryPro.ReportService.Services
    {
    /// <summary>
    /// PDF generation service using iText7
    /// </summary>
    public class PdfGenerator
        {
        /// <summary>
        /// Generates a PDF for sales report
        /// </summary>
        public static byte[] GenerateSalesReportPdf(SalesReport report)
            {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Fonts
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Title
            document.Add(new Paragraph("SALES REPORT")
                .SetFont(titleFont)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Report period
            document.Add(new Paragraph($"Period: {report.StartDate:MMM dd, yyyy} - {report.EndDate:MMM dd, yyyy}")
                .SetFont(normalFont)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Summary section
            document.Add(new Paragraph("SUMMARY")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetMarginBottom(10));

            var summaryTable = new Table(2);
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Summary data
            AddTableRow(summaryTable, "Total Sales:", $"${report.TotalSales:N2}", headerFont, normalFont);
            AddTableRow(summaryTable, "Total Orders:", report.TotalOrders.ToString("N0"), headerFont, normalFont);
            AddTableRow(summaryTable, "Average Order Value:", $"${report.AverageOrderValue:N2}", headerFont, normalFont);

            document.Add(summaryTable);
            document.Add(new Paragraph("\n"));

            // Daily Sales Chart
            if (report.DailySales.Any())
                {
                document.Add(new Paragraph("DAILY SALES TREND")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                try
                    {
                    var chartBytes = ChartGenerator.GenerateDailySalesChart(report.DailySales);
                    var chartImage = ImageDataFactory.Create(chartBytes);
                    var image = new Image(chartImage);
                    image.SetWidth(UnitValue.CreatePercentValue(80));
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                    document.Add(new Paragraph("\n"));
                    }
                catch (Exception ex)
                    {
                    document.Add(new Paragraph($"Chart generation failed: {ex.Message}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.RED));
                    }
                }

            // Top Products section
            if (report.TopProducts.Any())
                {
                document.Add(new Paragraph("TOP SELLING PRODUCTS")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                var productsTable = new Table(4);
                productsTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Product").SetFont(headerFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("SKU").SetFont(headerFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Quantity Sold").SetFont(headerFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Revenue").SetFont(headerFont)));

                // Data rows
                foreach (var product in report.TopProducts.Take(10))
                    {
                    productsTable.AddCell(new Cell().Add(new Paragraph(product.ProductName).SetFont(normalFont)));
                    productsTable.AddCell(new Cell().Add(new Paragraph(product.SKU).SetFont(normalFont)));
                    productsTable.AddCell(new Cell().Add(new Paragraph(product.QuantitySold.ToString("N0")).SetFont(normalFont)));
                    productsTable.AddCell(new Cell().Add(new Paragraph($"${product.TotalRevenue:N2}").SetFont(normalFont)));
                    }

                document.Add(productsTable);

                // Top Products Chart
                try
                    {
                    var chartBytes = ChartGenerator.GenerateTopProductsChart(report.TopProducts);
                    var chartImage = ImageDataFactory.Create(chartBytes);
                    var image = new Image(chartImage);
                    image.SetWidth(UnitValue.CreatePercentValue(80));
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                    }
                catch (Exception ex)
                    {
                    document.Add(new Paragraph($"Product chart generation failed: {ex.Message}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.RED));
                    }

                document.Add(new Paragraph("\n"));
                }

            // Top Customers section
            if (report.TopCustomers.Any())
                {
                document.Add(new Paragraph("TOP CUSTOMERS")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                var customersTable = new Table(3);
                customersTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                customersTable.AddHeaderCell(new Cell().Add(new Paragraph("Customer").SetFont(headerFont)));
                customersTable.AddHeaderCell(new Cell().Add(new Paragraph("Orders").SetFont(headerFont)));
                customersTable.AddHeaderCell(new Cell().Add(new Paragraph("Total Amount").SetFont(headerFont)));

                // Data rows
                foreach (var customer in report.TopCustomers.Take(10))
                    {
                    customersTable.AddCell(new Cell().Add(new Paragraph(customer.CustomerName).SetFont(normalFont)));
                    customersTable.AddCell(new Cell().Add(new Paragraph(customer.OrderCount.ToString("N0")).SetFont(normalFont)));
                    customersTable.AddCell(new Cell().Add(new Paragraph($"${customer.TotalAmount:N2}").SetFont(normalFont)));
                    }

                document.Add(customersTable);
                document.Add(new Paragraph("\n"));
                }

            // Sales by Category section
            if (report.SalesByCategory.Any())
                {
                document.Add(new Paragraph("SALES BY CATEGORY")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                var categoryTable = new Table(2);
                categoryTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                categoryTable.AddHeaderCell(new Cell().Add(new Paragraph("Category").SetFont(headerFont)));
                categoryTable.AddHeaderCell(new Cell().Add(new Paragraph("Sales Amount").SetFont(headerFont)));

                // Data rows
                foreach (var category in report.SalesByCategory)
                    {
                    categoryTable.AddCell(new Cell().Add(new Paragraph(category.Key).SetFont(normalFont)));
                    categoryTable.AddCell(new Cell().Add(new Paragraph($"${category.Value:N2}").SetFont(normalFont)));
                    }

                document.Add(categoryTable);

                // Sales by Category Chart
                try
                    {
                    var chartBytes = ChartGenerator.GenerateSalesByCategoryChart(report.SalesByCategory);
                    var chartImage = ImageDataFactory.Create(chartBytes);
                    var image = new Image(chartImage);
                    image.SetWidth(UnitValue.CreatePercentValue(60));
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                    }
                catch (Exception ex)
                    {
                    document.Add(new Paragraph($"Category chart generation failed: {ex.Message}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.RED));
                    }

                document.Add(new Paragraph("\n"));
                }

            // Sales by Payment Method section
            if (report.SalesByPaymentMethod.Any())
                {
                document.Add(new Paragraph("SALES BY PAYMENT METHOD")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                var paymentTable = new Table(2);
                paymentTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                paymentTable.AddHeaderCell(new Cell().Add(new Paragraph("Payment Method").SetFont(headerFont)));
                paymentTable.AddHeaderCell(new Cell().Add(new Paragraph("Amount").SetFont(headerFont)));

                // Data rows
                foreach (var payment in report.SalesByPaymentMethod)
                    {
                    paymentTable.AddCell(new Cell().Add(new Paragraph(payment.Key).SetFont(normalFont)));
                    paymentTable.AddCell(new Cell().Add(new Paragraph($"${payment.Value:N2}").SetFont(normalFont)));
                    }

                document.Add(paymentTable);
                }

            // Footer
            document.Add(new Paragraph($"\nGenerated on: {DateTime.Now:MMM dd, yyyy HH:mm}")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(30));

            document.Close();
            return stream.ToArray();
            }

        /// <summary>
        /// Generates a PDF for inventory report
        /// </summary>
        public static byte[] GenerateInventoryReportPdf(InventoryReport report)
            {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Fonts
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Title
            document.Add(new Paragraph("INVENTORY REPORT")
                .SetFont(titleFont)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Report date
            document.Add(new Paragraph($"Report Date: {report.ReportDate:MMM dd, yyyy}")
                .SetFont(normalFont)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Summary section
            document.Add(new Paragraph("INVENTORY SUMMARY")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetMarginBottom(10));

            var summaryTable = new Table(2);
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            AddTableRow(summaryTable, "Total Products:", report.TotalProducts.ToString("N0"), headerFont, normalFont);
            AddTableRow(summaryTable, "Active Products:", report.ActiveProducts.ToString("N0"), headerFont, normalFont);
            AddTableRow(summaryTable, "Low Stock Products:", report.LowStockProducts.ToString("N0"), headerFont, normalFont);
            AddTableRow(summaryTable, "Out of Stock Products:", report.OutOfStockProducts.ToString("N0"), headerFont, normalFont);
            AddTableRow(summaryTable, "Total Inventory Value:", $"${report.TotalInventoryValue:N2}", headerFont, normalFont);

            document.Add(summaryTable);
            document.Add(new Paragraph("\n"));

            // Inventory by Category Chart
            if (report.InventoryByCategory.Any())
                {
                document.Add(new Paragraph("INVENTORY DISTRIBUTION BY CATEGORY")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                try
                    {
                    var chartBytes = ChartGenerator.GenerateInventoryStatusChart(report.InventoryByCategory);
                    var chartImage = ImageDataFactory.Create(chartBytes);
                    var image = new Image(chartImage);
                    image.SetWidth(UnitValue.CreatePercentValue(80));
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                    document.Add(new Paragraph("\n"));
                    }
                catch (Exception ex)
                    {
                    document.Add(new Paragraph($"Inventory chart generation failed: {ex.Message}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.RED));
                    }
                }

            // Low stock products details
            if (report.ProductInventoryDetails.Any())
                {
                document.Add(new Paragraph("LOW STOCK PRODUCTS")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                var productsTable = new Table(5);
                productsTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Product").SetFont(headerFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("SKU").SetFont(headerFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Current Stock").SetFont(headerFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Min Stock").SetFont(headerFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Status").SetFont(headerFont)));

                // Data rows
                foreach (var product in report.ProductInventoryDetails)
                    {
                    productsTable.AddCell(new Cell().Add(new Paragraph(product.ProductName).SetFont(normalFont)));
                    productsTable.AddCell(new Cell().Add(new Paragraph(product.SKU).SetFont(normalFont)));
                    productsTable.AddCell(new Cell().Add(new Paragraph(product.CurrentStock.ToString("N0")).SetFont(normalFont)));
                    productsTable.AddCell(new Cell().Add(new Paragraph(product.MinimumStock.ToString("N0")).SetFont(normalFont)));

                    var statusCell = new Cell().Add(new Paragraph(product.StockStatus).SetFont(normalFont));
                    if (product.StockStatus == "Out of Stock")
                        statusCell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                    else if (product.StockStatus == "Low")
                        statusCell.SetBackgroundColor(ColorConstants.YELLOW);

                    productsTable.AddCell(statusCell);
                    }

                document.Add(productsTable);
                }

            // Footer
            document.Add(new Paragraph($"\nGenerated on: {DateTime.Now:MMM dd, yyyy HH:mm}")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(30));

            document.Close();
            return stream.ToArray();
            }

        /// <summary>
        /// Generates a PDF for financial report
        /// </summary>
        public static byte[] GenerateFinancialReportPdf(FinancialReport report)
            {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Fonts
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Title
            document.Add(new Paragraph("FINANCIAL REPORT")
                .SetFont(titleFont)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Report period
            document.Add(new Paragraph($"Period: {report.StartDate:MMM dd, yyyy} - {report.EndDate:MMM dd, yyyy}")
                .SetFont(normalFont)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Financial Summary section
            document.Add(new Paragraph("FINANCIAL SUMMARY")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetMarginBottom(10));

            var summaryTable = new Table(2);
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            AddTableRow(summaryTable, "Gross Revenue:", $"${report.GrossRevenue:N2}", headerFont, normalFont);
            AddTableRow(summaryTable, "Total Discounts:", $"${report.TotalDiscounts:N2}", headerFont, normalFont);
            AddTableRow(summaryTable, "Net Revenue:", $"${report.NetRevenue:N2}", headerFont, normalFont);
            AddTableRow(summaryTable, "Total Transactions:", report.TotalTransactions.ToString("N0"), headerFont, normalFont);
            AddTableRow(summaryTable, "Average Transaction Value:", $"${report.AverageTransactionValue:N2}", headerFont, normalFont);

            document.Add(summaryTable);
            document.Add(new Paragraph("\n"));

            // Monthly Revenue Chart
            if (report.MonthlyRevenue.Any())
                {
                document.Add(new Paragraph("MONTHLY REVENUE TREND")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                try
                    {
                    var chartBytes = ChartGenerator.GenerateMonthlyRevenueChart(report.MonthlyRevenue);
                    var chartImage = ImageDataFactory.Create(chartBytes);
                    var image = new Image(chartImage);
                    image.SetWidth(UnitValue.CreatePercentValue(80));
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                    document.Add(new Paragraph("\n"));
                    }
                catch (Exception ex)
                    {
                    document.Add(new Paragraph($"Monthly revenue chart generation failed: {ex.Message}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.RED));
                    }

                // Monthly Revenue Table
                var monthlyTable = new Table(4);
                monthlyTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                monthlyTable.AddHeaderCell(new Cell().Add(new Paragraph("Month").SetFont(headerFont)));
                monthlyTable.AddHeaderCell(new Cell().Add(new Paragraph("Revenue").SetFont(headerFont)));
                monthlyTable.AddHeaderCell(new Cell().Add(new Paragraph("Transactions").SetFont(headerFont)));
                monthlyTable.AddHeaderCell(new Cell().Add(new Paragraph("Growth %").SetFont(headerFont)));

                // Data rows
                foreach (var monthlyRevenue in report.MonthlyRevenue)
                    {
                    monthlyTable.AddCell(new Cell().Add(new Paragraph($"{monthlyRevenue.Year}-{monthlyRevenue.Month:00}").SetFont(normalFont)));
                    monthlyTable.AddCell(new Cell().Add(new Paragraph($"${monthlyRevenue.Revenue:N2}").SetFont(normalFont)));
                    monthlyTable.AddCell(new Cell().Add(new Paragraph(monthlyRevenue.TransactionCount.ToString("N0")).SetFont(normalFont)));
                    monthlyTable.AddCell(new Cell().Add(new Paragraph($"{monthlyRevenue.Growth:F1}%").SetFont(normalFont)));
                    }

                document.Add(monthlyTable);
                document.Add(new Paragraph("\n"));
                }

            // Revenue by Category section
            if (report.RevenueByCategory.Any())
                {
                document.Add(new Paragraph("REVENUE BY CATEGORY")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                var categoryTable = new Table(2);
                categoryTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                categoryTable.AddHeaderCell(new Cell().Add(new Paragraph("Category").SetFont(headerFont)));
                categoryTable.AddHeaderCell(new Cell().Add(new Paragraph("Revenue").SetFont(headerFont)));

                // Data rows
                foreach (var category in report.RevenueByCategory)
                    {
                    categoryTable.AddCell(new Cell().Add(new Paragraph(category.Key).SetFont(normalFont)));
                    categoryTable.AddCell(new Cell().Add(new Paragraph($"${category.Value:N2}").SetFont(normalFont)));
                    }

                document.Add(categoryTable);

                // Revenue by Category Chart
                try
                    {
                    var chartBytes = ChartGenerator.GenerateSalesByCategoryChart(report.RevenueByCategory);
                    var chartImage = ImageDataFactory.Create(chartBytes);
                    var image = new Image(chartImage);
                    image.SetWidth(UnitValue.CreatePercentValue(60));
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                    }
                catch (Exception ex)
                    {
                    document.Add(new Paragraph($"Revenue category chart generation failed: {ex.Message}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.RED));
                    }
                }

            // Footer
            document.Add(new Paragraph($"\nGenerated on: {DateTime.Now:MMM dd, yyyy HH:mm}")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(30));

            document.Close();
            return stream.ToArray();
            }

        /// <summary>
        /// Generates a PDF for custom report
        /// </summary>
        public static byte[] GenerateCustomReportPdf(CustomReport report)
            {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Fonts
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Title
            document.Add(new Paragraph(report.Title.ToUpper())
                .SetFont(titleFont)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Report period and generation info
            document.Add(new Paragraph($"Period: {report.StartDate:MMM dd, yyyy} - {report.EndDate:MMM dd, yyyy}")
                .SetFont(normalFont)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10));

            document.Add(new Paragraph($"Generated: {report.GeneratedAt:MMM dd, yyyy HH:mm}")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Executive Summary
            document.Add(new Paragraph("EXECUTIVE SUMMARY")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetMarginBottom(10));

            var summaryTable = new Table(2);
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            AddTableRow(summaryTable, "Total Revenue:", $"${report.TotalRevenue:N2}", headerFont, normalFont);
            AddTableRow(summaryTable, "Total Transactions:", report.TotalTransactions.ToString("N0"), headerFont, normalFont);
            AddTableRow(summaryTable, "Average Transaction Value:", $"${report.AverageTransactionValue:N2}", headerFont, normalFont);
            AddTableRow(summaryTable, "Unique Customers:", report.UniqueCustomers.ToString("N0"), headerFont, normalFont);
            AddTableRow(summaryTable, "Products Sold:", report.ProductsSold.ToString("N0"), headerFont, normalFont);

            document.Add(summaryTable);
            document.Add(new Paragraph("\n"));

            // Daily Sales Trend
            if (report.FilteredDailySales.Any())
                {
                document.Add(new Paragraph("DAILY SALES TREND")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                try
                    {
                    var chartBytes = ChartGenerator.GenerateDailySalesChart(report.FilteredDailySales);
                    var chartImage = ImageDataFactory.Create(chartBytes);
                    var image = new Image(chartImage);
                    image.SetWidth(UnitValue.CreatePercentValue(80));
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                    document.Add(new Paragraph("\n"));
                    }
                catch (Exception ex)
                    {
                    document.Add(new Paragraph($"Chart generation failed: {ex.Message}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.RED));
                    }
                }

            // Top Products
            if (report.FilteredTopProducts.Any())
                {
                document.Add(new Paragraph("TOP PERFORMING PRODUCTS")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                var productsTable = new Table(4);
                productsTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Product").SetFont(headerFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("SKU").SetFont(headerFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Quantity Sold").SetFont(headerFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Revenue").SetFont(headerFont)));

                // Data rows
                foreach (var product in report.FilteredTopProducts)
                    {
                    productsTable.AddCell(new Cell().Add(new Paragraph(product.ProductName).SetFont(normalFont)));
                    productsTable.AddCell(new Cell().Add(new Paragraph(product.SKU).SetFont(normalFont)));
                    productsTable.AddCell(new Cell().Add(new Paragraph(product.QuantitySold.ToString("N0")).SetFont(normalFont)));
                    productsTable.AddCell(new Cell().Add(new Paragraph($"${product.TotalRevenue:N2}").SetFont(normalFont)));
                    }

                document.Add(productsTable);

                // Top Products Chart
                try
                    {
                    var chartBytes = ChartGenerator.GenerateTopProductsChart(report.FilteredTopProducts);
                    var chartImage = ImageDataFactory.Create(chartBytes);
                    var image = new Image(chartImage);
                    image.SetWidth(UnitValue.CreatePercentValue(80));
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                    }
                catch (Exception ex)
                    {
                    document.Add(new Paragraph($"Product chart generation failed: {ex.Message}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.RED));
                    }

                document.Add(new Paragraph("\n"));
                }

            // Top Customers
            if (report.FilteredTopCustomers.Any())
                {
                document.Add(new Paragraph("TOP CUSTOMERS")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                var customersTable = new Table(3);
                customersTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                customersTable.AddHeaderCell(new Cell().Add(new Paragraph("Customer").SetFont(headerFont)));
                customersTable.AddHeaderCell(new Cell().Add(new Paragraph("Orders").SetFont(headerFont)));
                customersTable.AddHeaderCell(new Cell().Add(new Paragraph("Total Amount").SetFont(headerFont)));

                // Data rows
                foreach (var customer in report.FilteredTopCustomers)
                    {
                    customersTable.AddCell(new Cell().Add(new Paragraph(customer.CustomerName).SetFont(normalFont)));
                    customersTable.AddCell(new Cell().Add(new Paragraph(customer.OrderCount.ToString("N0")).SetFont(normalFont)));
                    customersTable.AddCell(new Cell().Add(new Paragraph($"${customer.TotalAmount:N2}").SetFont(normalFont)));
                    }

                document.Add(customersTable);

                // Top Customers Chart
                try
                    {
                    var chartBytes = ChartGenerator.GenerateTopCustomersChart(report.FilteredTopCustomers);
                    var chartImage = ImageDataFactory.Create(chartBytes);
                    var image = new Image(chartImage);
                    image.SetWidth(UnitValue.CreatePercentValue(80));
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                    }
                catch (Exception ex)
                    {
                    document.Add(new Paragraph($"Customer chart generation failed: {ex.Message}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.RED));
                    }

                document.Add(new Paragraph("\n"));
                }

            // Sales by Category
            if (report.FilteredSalesByCategory.Any())
                {
                document.Add(new Paragraph("SALES BY CATEGORY")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                var categoryTable = new Table(2);
                categoryTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                categoryTable.AddHeaderCell(new Cell().Add(new Paragraph("Category").SetFont(headerFont)));
                categoryTable.AddHeaderCell(new Cell().Add(new Paragraph("Sales Amount").SetFont(headerFont)));

                // Data rows
                foreach (var category in report.FilteredSalesByCategory)
                    {
                    categoryTable.AddCell(new Cell().Add(new Paragraph(category.Key).SetFont(normalFont)));
                    categoryTable.AddCell(new Cell().Add(new Paragraph($"${category.Value:N2}").SetFont(normalFont)));
                    }

                document.Add(categoryTable);

                // Sales by Category Chart
                try
                    {
                    var chartBytes = ChartGenerator.GenerateSalesByCategoryChart(report.FilteredSalesByCategory);
                    var chartImage = ImageDataFactory.Create(chartBytes);
                    var image = new Image(chartImage);
                    image.SetWidth(UnitValue.CreatePercentValue(60));
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                    }
                catch (Exception ex)
                    {
                    document.Add(new Paragraph($"Category chart generation failed: {ex.Message}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.RED));
                    }
                }

            // Footer
            document.Add(new Paragraph($"\nGenerated on: {DateTime.Now:MMM dd, yyyy HH:mm}")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(30));

            document.Close();
            return stream.ToArray();
            }

        /// <summary>
        /// Generates a professional PDF invoice from sale data
        /// </summary>
        public static byte[] GenerateInvoicePdf(SaleDto sale, string companyName = "InventoryPro", string companyAddress = "", string companyPhone = "", string companyEmail = "")
            {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Fonts
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Colors
            var primaryColor = new DeviceRgb(52, 58, 64);
            var secondaryColor = new DeviceRgb(108, 117, 125);
            var accentColor = new DeviceRgb(0, 123, 255);

            // Header section with company info and invoice title
            var headerTable = new Table(2);
            headerTable.SetWidth(UnitValue.CreatePercentValue(100));
            headerTable.SetMarginBottom(30);

            // Company info cell
            var companyCell = new Cell();
            companyCell.Add(new Paragraph(companyName)
                .SetFont(titleFont)
                .SetFontSize(24)
                .SetFontColor(primaryColor)
                .SetMarginBottom(5));
            
            if (!string.IsNullOrEmpty(companyAddress))
                companyCell.Add(new Paragraph(companyAddress).SetFont(normalFont).SetFontSize(10).SetMarginBottom(2));
            if (!string.IsNullOrEmpty(companyPhone))
                companyCell.Add(new Paragraph($"Phone: {companyPhone}").SetFont(normalFont).SetFontSize(10).SetMarginBottom(2));
            if (!string.IsNullOrEmpty(companyEmail))
                companyCell.Add(new Paragraph($"Email: {companyEmail}").SetFont(normalFont).SetFontSize(10));
            
            companyCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
            
            // Invoice title cell
            var invoiceCell = new Cell();
            invoiceCell.Add(new Paragraph("INVOICE")
                .SetFont(titleFont)
                .SetFontSize(28)
                .SetFontColor(accentColor)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginBottom(5));
            invoiceCell.Add(new Paragraph($"#{sale.Id:D6}")
                .SetFont(headerFont)
                .SetFontSize(16)
                .SetFontColor(secondaryColor)
                .SetTextAlignment(TextAlignment.RIGHT));
            invoiceCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            headerTable.AddCell(companyCell);
            headerTable.AddCell(invoiceCell);
            document.Add(headerTable);

            // Invoice details and customer info section
            var detailsTable = new Table(2);
            detailsTable.SetWidth(UnitValue.CreatePercentValue(100));
            detailsTable.SetMarginBottom(30);

            // Customer billing info
            var billingCell = new Cell();
            billingCell.Add(new Paragraph("BILL TO:")
                .SetFont(headerFont)
                .SetFontSize(12)
                .SetFontColor(primaryColor)
                .SetMarginBottom(8));
            
            var customerName = sale.CustomerName ?? "Walk-in Customer";
            billingCell.Add(new Paragraph(customerName)
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetMarginBottom(5));
            
            // Add placeholder for customer details (in real implementation, these would come from customer lookup)
            billingCell.Add(new Paragraph("Customer contact information would be displayed here").SetFont(normalFont).SetFontSize(10).SetMarginBottom(2));
            
            billingCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
            billingCell.SetBackgroundColor(new DeviceRgb(248, 249, 250));
            billingCell.SetPadding(15);

            // Invoice details
            var invoiceDetailsCell = new Cell();
            invoiceDetailsCell.Add(new Paragraph("INVOICE DETAILS:")
                .SetFont(headerFont)
                .SetFontSize(12)
                .SetFontColor(primaryColor)
                .SetMarginBottom(8));

            var detailsInnerTable = new Table(2);
            detailsInnerTable.SetWidth(UnitValue.CreatePercentValue(100));
            
            AddInvoiceDetailRow(detailsInnerTable, "Invoice Date:", sale.Date.ToString("MMM dd, yyyy"), headerFont, normalFont);
            AddInvoiceDetailRow(detailsInnerTable, "Due Date:", sale.Date.AddDays(30).ToString("MMM dd, yyyy"), headerFont, normalFont);
            AddInvoiceDetailRow(detailsInnerTable, "Payment Method:", sale.PaymentMethod ?? "N/A", headerFont, normalFont);
            AddInvoiceDetailRow(detailsInnerTable, "Sales Rep:", "System", headerFont, normalFont);

            invoiceDetailsCell.Add(detailsInnerTable);
            invoiceDetailsCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            detailsTable.AddCell(billingCell);
            detailsTable.AddCell(invoiceDetailsCell);
            document.Add(detailsTable);

            // Items table
            document.Add(new Paragraph("ITEMS")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetFontColor(primaryColor)
                .SetMarginBottom(15));

            var itemsTable = new Table(5);
            itemsTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Header row with styling
            var headerStyle = new Style()
                .SetBackgroundColor(primaryColor)
                .SetFontColor(ColorConstants.WHITE)
                .SetFont(headerFont)
                .SetFontSize(11)
                .SetPadding(12)
                .SetTextAlignment(TextAlignment.CENTER);

            itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("ITEM").SetTextAlignment(TextAlignment.LEFT)).AddStyle(headerStyle));
            itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("SKU")).AddStyle(headerStyle));
            itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("QTY")).AddStyle(headerStyle));
            itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("UNIT PRICE")).AddStyle(headerStyle));
            itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("TOTAL")).AddStyle(headerStyle));

            // Data rows
            var rowStyle = new Style()
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetPadding(10);

            var alternateRowStyle = new Style()
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetPadding(10)
                .SetBackgroundColor(new DeviceRgb(248, 249, 250));

            for (int i = 0; i < sale.Items.Count; i++)
                {
                var item = sale.Items[i];
                var currentRowStyle = i % 2 == 0 ? rowStyle : alternateRowStyle;
                
                itemsTable.AddCell(new Cell().Add(new Paragraph(item.ProductName ?? "Unknown Product")
                    .SetTextAlignment(TextAlignment.LEFT)).AddStyle(currentRowStyle));
                itemsTable.AddCell(new Cell().Add(new Paragraph(item.ProductSKU ?? "N/A")
                    .SetTextAlignment(TextAlignment.CENTER)).AddStyle(currentRowStyle));
                itemsTable.AddCell(new Cell().Add(new Paragraph(item.Quantity.ToString("N0"))
                    .SetTextAlignment(TextAlignment.CENTER)).AddStyle(currentRowStyle));
                itemsTable.AddCell(new Cell().Add(new Paragraph($"${item.UnitPrice:N2}")
                    .SetTextAlignment(TextAlignment.RIGHT)).AddStyle(currentRowStyle));
                itemsTable.AddCell(new Cell().Add(new Paragraph($"${item.FinalAmount:N2}")
                    .SetTextAlignment(TextAlignment.RIGHT)).AddStyle(currentRowStyle));
                }

            document.Add(itemsTable);

            // Totals section
            document.Add(new Paragraph("\n"));
            
            var totalsTable = new Table(2);
            totalsTable.SetWidth(UnitValue.CreatePercentValue(50));
            totalsTable.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
            totalsTable.SetMarginTop(20);

            // Use stored tax information from the sale
            var subtotal = sale.SubtotalAmount;
            var taxAmount = sale.TaxAmount;
            var taxRate = sale.TaxRate;

            AddTotalRow(totalsTable, "Subtotal:", $"${subtotal:N2}", normalFont, normalFont, false);
            AddTotalRow(totalsTable, $"Tax ({taxRate:P0}):", $"${taxAmount:N2}", normalFont, normalFont, false);
            AddTotalRow(totalsTable, "TOTAL:", $"${sale.TotalAmount:N2}", titleFont, titleFont, true);
            AddTotalRow(totalsTable, "Amount Paid:", $"${sale.PaidAmount:N2}", normalFont, normalFont, false);
            
            var changeAmount = sale.PaidAmount - sale.TotalAmount;
            var changeColor = changeAmount >= 0 ? new DeviceRgb(40, 167, 69) : new DeviceRgb(220, 53, 69);
            AddTotalRow(totalsTable, "Change:", $"${changeAmount:N2}", normalFont, normalFont, false, changeColor);

            document.Add(totalsTable);

            // Footer section
            document.Add(new Paragraph("\n\n"));
            
            // Payment terms and notes
            var footerTable = new Table(1);
            footerTable.SetWidth(UnitValue.CreatePercentValue(100));
            footerTable.SetMarginTop(30);
            
            var footerCell = new Cell();
            footerCell.Add(new Paragraph("PAYMENT TERMS & CONDITIONS")
                .SetFont(headerFont)
                .SetFontSize(10)
                .SetFontColor(primaryColor)
                .SetMarginBottom(5));
            footerCell.Add(new Paragraph("• Payment is due within 30 days of invoice date")
                .SetFont(normalFont)
                .SetFontSize(9)
                .SetMarginBottom(2));
            footerCell.Add(new Paragraph("• Late payments may be subject to fees")
                .SetFont(normalFont)
                .SetFontSize(9)
                .SetMarginBottom(2));
            footerCell.Add(new Paragraph("• Please include invoice number with payment")
                .SetFont(normalFont)
                .SetFontSize(9));
            
            if (!string.IsNullOrEmpty(sale.Notes))
                {
                footerCell.Add(new Paragraph($"\nNotes: {sale.Notes}")
                    .SetFont(normalFont)
                    .SetFontSize(9)
                    .SetFontColor(secondaryColor)
                    .SetMarginTop(10));
                }
            
            footerCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
            footerCell.SetBackgroundColor(new DeviceRgb(248, 249, 250));
            footerCell.SetPadding(15);
            footerTable.AddCell(footerCell);
            document.Add(footerTable);

            // Final footer with generation info
            document.Add(new Paragraph($"Invoice generated on {DateTime.Now:MMM dd, yyyy 'at' HH:mm}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetFontColor(secondaryColor)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(20));

            document.Close();
            return stream.ToArray();
            }

        private static void AddInvoiceDetailRow(Table table, string label, string value, PdfFont labelFont, PdfFont valueFont)
            {
            table.AddCell(new Cell().Add(new Paragraph(label).SetFont(labelFont).SetFontSize(10))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(2));
            table.AddCell(new Cell().Add(new Paragraph(value).SetFont(valueFont).SetFontSize(10))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(2));
            }

        private static void AddTotalRow(Table table, string label, string value, PdfFont labelFont, PdfFont valueFont, bool isTotal, DeviceRgb? valueColor = null)
            {
            var labelCell = new Cell().Add(new Paragraph(label).SetFont(labelFont).SetFontSize(isTotal ? 14 : 11))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetPadding(8)
                .SetTextAlignment(TextAlignment.RIGHT);
            
            var valueCell = new Cell().Add(new Paragraph(value).SetFont(valueFont).SetFontSize(isTotal ? 14 : 11))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetPadding(8)
                .SetTextAlignment(TextAlignment.RIGHT);

            if (isTotal)
                {
                labelCell.SetBackgroundColor(new DeviceRgb(52, 58, 64)).SetFontColor(ColorConstants.WHITE);
                valueCell.SetBackgroundColor(new DeviceRgb(52, 58, 64)).SetFontColor(ColorConstants.WHITE);
                }
            else if (valueColor != null)
                {
                valueCell.SetFontColor(valueColor);
                }

            table.AddCell(labelCell);
            table.AddCell(valueCell);
            }

        private static void AddTableRow(Table table, string label, string value, PdfFont labelFont, PdfFont valueFont)
            {
            table.AddCell(new Cell().Add(new Paragraph(label).SetFont(labelFont)));
            table.AddCell(new Cell().Add(new Paragraph(value).SetFont(valueFont)));
            }
        }
    }