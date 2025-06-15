using System;
using System.IO;
using InventoryPro.ReportService.Models;
using InventoryPro.ReportService.Services;

// Simple test to demonstrate PDF export functionality
class Program
{
    static void Main()
    {
        Console.WriteLine("Testing PDF Export Functionality...");
        
        // Create sample sales report data
        var salesReport = new SalesReport
        {
            StartDate = DateTime.Now.AddDays(-30),
            EndDate = DateTime.Now,
            TotalSales = 125430.50m,
            TotalOrders = 342,
            AverageOrderValue = 366.72m,
            TopProducts = new List<ProductSales>
            {
                new() { ProductId = 1, ProductName = "Laptop Pro 15", SKU = "LAP-001", QuantitySold = 45, TotalRevenue = 58499.55m },
                new() { ProductId = 2, ProductName = "Wireless Mouse", SKU = "MOU-001", QuantitySold = 234, TotalRevenue = 7019.66m },
                new() { ProductId = 3, ProductName = "USB-C Cable", SKU = "CAB-001", QuantitySold = 456, TotalRevenue = 4560.00m }
            },
            TopCustomers = new List<CustomerSales>
            {
                new() { CustomerId = 2, CustomerName = "John Doe", OrderCount = 15, TotalAmount = 12340.50m },
                new() { CustomerId = 3, CustomerName = "Jane Smith", OrderCount = 8, TotalAmount = 8950.25m },
                new() { CustomerId = 4, CustomerName = "Bob Johnson", OrderCount = 12, TotalAmount = 7230.00m }
            },
            SalesByCategory = new Dictionary<string, decimal>
            {
                { "Electronics", 45230.50m },
                { "Clothing", 23450.25m },
                { "Food & Beverages", 35670.00m }
            },
            SalesByPaymentMethod = new Dictionary<string, decimal>
            {
                { "Cash", 50172.20m },
                { "Credit Card", 43900.68m },
                { "Debit Card", 31357.62m }
            }
        };

        try
        {
            // Generate PDF
            var pdfBytes = PdfGenerator.GenerateSalesReportPdf(salesReport);
            
            // Save to file
            var fileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            File.WriteAllBytes(fileName, pdfBytes);
            
            Console.WriteLine($"✅ PDF generated successfully!");
            Console.WriteLine($"📁 File saved as: {fileName}");
            Console.WriteLine($"📊 File size: {pdfBytes.Length:N0} bytes");
            Console.WriteLine($"🎯 Report covers period: {salesReport.StartDate:MMM dd, yyyy} - {salesReport.EndDate:MMM dd, yyyy}");
            Console.WriteLine($"💰 Total sales: ${salesReport.TotalSales:N2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating PDF: {ex.Message}");
        }
    }
}