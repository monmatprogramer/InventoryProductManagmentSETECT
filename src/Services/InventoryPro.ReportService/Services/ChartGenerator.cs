using ScottPlot;
using InventoryPro.ReportService.Models;

namespace InventoryPro.ReportService.Services
    {
    /// <summary>
    /// Chart generation service for creating X/Y axis visualizations
    /// </summary>
    public static class ChartGenerator
        {
        /// <summary>
        /// Generates daily sales line chart (X: Date, Y: Sales Amount)
        /// </summary>
        public static byte[] GenerateDailySalesChart(List<DailySales> dailySales, int width = 800, int height = 400)
            {
            var plt = new Plot(width, height);

            if (dailySales.Any())
                {
                // Use simple numeric positions instead of date serial numbers
                var positions = Enumerable.Range(0, dailySales.Count).Select(i => (double)i).ToArray();
                var amounts = dailySales.Select(d => (double)d.TotalAmount).ToArray();

                // Create readable date labels
                var dateLabels = dailySales.Select(d => d.Date.ToString("MMM dd")).ToArray();

                // Add line plot with markers
                var scatter = plt.AddScatter(positions, amounts, markerSize: 6, lineWidth: 2);
                scatter.Color = System.Drawing.Color.SteelBlue;
                scatter.MarkerShape = MarkerShape.filledCircle;

                // Professional formatting
                plt.Title("Daily Sales Trend");
                plt.XLabel("Date");
                plt.YLabel("Sales Amount ($)");

                // Set custom X-axis labels
                plt.XTicks(positions, dateLabels);

                // Show only every few dates if too many points
                if (dailySales.Count > 10)
                    {
                    var step = Math.Max(1, dailySales.Count / 8);
                    var thinPositions = positions.Where((x, i) => i % step == 0).ToArray();
                    var thinLabels = dateLabels.Where((x, i) => i % step == 0).ToArray();
                    plt.XTicks(thinPositions, thinLabels);
                    }

                plt.Grid(true);

                // Format Y-axis to show currency
                plt.YAxis.TickLabelFormat(x => $"${x:N0}");
                }
            else
                {
                plt.Title("No Sales Data Available");
                }

            return plt.GetImageBytes();
            }

        /// <summary>
        /// Generates top products bar chart (X: Products, Y: Revenue)
        /// </summary>
        public static byte[] GenerateTopProductsChart(List<ProductSales> topProducts, int width = 800, int height = 400)
            {
            var plt = new Plot(width, height);

            if (topProducts.Any())
                {
                var productNames = topProducts.Select(p => p.ProductName.Length > 15 ?
                    p.ProductName.Substring(0, 12) + "..." : p.ProductName).ToArray();
                var revenues = topProducts.Select(p => (double)p.TotalRevenue).ToArray();
                var positions = Enumerable.Range(0, productNames.Length).Select(i => (double)i).ToArray();

                // Add bar plot
                var bars = plt.AddBar(positions, revenues);
                bars.FillColor = System.Drawing.Color.SteelBlue;
                bars.BorderColor = System.Drawing.Color.Navy;

                // Formatting
                plt.Title("Top Selling Products by Revenue");
                plt.XLabel("Products");
                plt.YLabel("Revenue ($)");
                plt.XTicks(positions, productNames);
                plt.Grid(true);

                // Format Y-axis to show currency
                plt.YAxis.TickLabelFormat(x => $"${x:N0}");

                // Add value labels on bars (only if not too many bars)
                if (revenues.Length <= 8)
                    {
                    for (int i = 0; i < revenues.Length; i++)
                        {
                        plt.AddText($"${revenues[i]:N0}", positions[i], revenues[i] + revenues.Max() * 0.02);
                        }
                    }
                }
            else
                {
                plt.Title("No Product Sales Data Available");
                }

            return plt.GetImageBytes();
            }

        /// <summary>
        /// Generates sales by category pie chart
        /// </summary>
        public static byte[] GenerateSalesByCategoryChart(Dictionary<string, decimal> salesByCategory, int width = 600, int height = 600)
            {
            var plt = new Plot(width, height);

            if (salesByCategory.Any())
                {
                var values = salesByCategory.Values.Select(v => Math.Round((double)v, 2)).ToArray();
                //var labels = salesByCategory.Keys.ToArray();
                //            var labels = salesByCategory.Keys
                //.Zip(values, (category, value) => $"{category}\n${value:N2}")
                //.ToArray();
                var categories = salesByCategory.Keys.ToArray();

                // Add pie chart
                var pie = plt.AddPie(values);
                pie.SliceLabels = salesByCategory.Keys
                    .Zip(values, (category, value) => $"{category}\n${value:N2}")
                    .ToArray();
                pie.ShowValues = false;
                pie.ShowLabels = true;
                //pie.ValueFormatter = v => $"${v:N2}";
                pie.ShowPercentages = true;

                // Formatting
                plt.Title("Sales Distribution by Category");
                plt.Grid(false);
                plt.Frameless();
                }
            else
                {
                plt.Title("No Category Sales Data Available");
                }

            return plt.GetImageBytes();
            }

        /// <summary>
        /// Generates monthly revenue trend chart (X: Month, Y: Revenue)
        /// </summary>
        public static byte[] GenerateMonthlyRevenueChart(List<MonthlyRevenue> monthlyRevenue, int width = 800, int height = 400)
            {
            var plt = new Plot(width, height);

            if (monthlyRevenue.Any())
                {
                var months = monthlyRevenue.Select(m => $"{GetMonthName(m.Month)}").ToArray();
                var revenues = monthlyRevenue.Select(m => (double)m.Revenue).ToArray();
                var positions = Enumerable.Range(0, months.Length).Select(i => (double)i).ToArray();

                // Add line plot with markers
                var scatter = plt.AddScatter(positions, revenues, markerSize: 8, lineWidth: 3);
                scatter.Color = System.Drawing.Color.DarkGreen;
                scatter.MarkerShape = MarkerShape.filledCircle;

                // Formatting
                plt.Title("Monthly Revenue Trend");
                plt.XLabel("Month");
                plt.YLabel("Revenue ($)");
                plt.XTicks(positions, months);
                plt.Grid(true);

                // Format Y-axis to show currency
                plt.YAxis.TickLabelFormat(x => $"${x:N0}");

                // Add growth indicators (simplified)
                for (int i = 1; i < monthlyRevenue.Count && i < positions.Length; i++)
                    {
                    var growth = monthlyRevenue[i].Growth;
                    if (Math.Abs(growth) > 1) // Only show significant growth
                        {
                        var symbol = growth >= 0 ? "↑" : "↓";
                        plt.AddText($"{symbol}{Math.Abs(growth):F0}%", positions[i], revenues[i] + revenues.Max() * 0.05);
                        }
                    }
                }
            else
                {
                plt.Title("No Monthly Revenue Data Available");
                }

            return plt.GetImageBytes();
            }

        /// <summary>
        /// Generates inventory status chart (X: Categories, Y: Stock Levels)
        /// </summary>
        public static byte[] GenerateInventoryStatusChart(List<CategoryInventory> inventoryByCategory, int width = 800, int height = 400)
            {
            var plt = new Plot(width, height);

            if (inventoryByCategory.Any())
                {
                var categories = inventoryByCategory.Select(c => c.CategoryName).ToArray();
                var stocks = inventoryByCategory.Select(c => (double)c.TotalStock).ToArray();
                var values = inventoryByCategory.Select(c => (double)c.TotalValue).ToArray();
                var positions = Enumerable.Range(0, categories.Length).Select(i => (double)i).ToArray();

                // Add grouped bar chart
                var stockBars = plt.AddBar(positions.Select(p => p - 0.2).ToArray(), stocks);
                stockBars.FillColor = System.Drawing.Color.SkyBlue;
                stockBars.Label = "Stock Quantity";

                // Scale values appropriately for visualization
                var maxStock = stocks.Max();
                var scaledValues = values.Select(v => v / Math.Max(1, values.Max() / maxStock)).ToArray();

                var valueBars = plt.AddBar(positions.Select(p => p + 0.2).ToArray(), scaledValues);
                valueBars.FillColor = System.Drawing.Color.Orange;
                valueBars.Label = "Value (Scaled)";

                // Formatting
                plt.Title("Inventory Distribution by Category");
                plt.XLabel("Categories");
                plt.YLabel("Stock Quantity");
                plt.XTicks(positions, categories);
                plt.Legend();
                plt.Grid(true);

                // Format Y-axis
                plt.YAxis.TickLabelFormat(x => $"{x:N0}");
                }
            else
                {
                plt.Title("No Inventory Data Available");
                }

            return plt.GetImageBytes();
            }

        /// <summary>
        /// Generates customer orders chart (X: Top Customers, Y: Order Count)
        /// </summary>
        public static byte[] GenerateTopCustomersChart(List<CustomerSales> topCustomers, int width = 800, int height = 400)
            {
            var plt = new Plot(width, height);

            if (topCustomers.Any())
                {
                var customerNames = topCustomers.Select(c => c.CustomerName.Length > 15 ?
                    c.CustomerName.Substring(0, 12) + "..." : c.CustomerName).ToArray();
                var amounts = topCustomers.Select(c => (double)c.TotalAmount).ToArray();
                var positions = Enumerable.Range(0, customerNames.Length).Select(i => (double)i).ToArray();

                // Add bar plot
                var bars = plt.AddBar(positions, amounts);
                bars.FillColor = System.Drawing.Color.MediumSeaGreen;
                bars.BorderColor = System.Drawing.Color.DarkGreen;

                // Formatting
                plt.Title("Top Customers by Purchase Amount");
                plt.XLabel("Customers");
                plt.YLabel("Total Purchases ($)");
                plt.XTicks(positions, customerNames);
                plt.Grid(true);

                // Format Y-axis to show currency
                plt.YAxis.TickLabelFormat(x => $"${x:N0}");

                // Add value labels (only if not too many bars)
                if (amounts.Length <= 8)
                    {
                    for (int i = 0; i < amounts.Length; i++)
                        {
                        plt.AddText($"${amounts[i]:N0}", positions[i], amounts[i] + amounts.Max() * 0.02);
                        }
                    }
                }
            else
                {
                plt.Title("No Customer Data Available");
                }

            return plt.GetImageBytes();
            }

        private static string GetMonthName(int month)
            {
            return month switch
                {
                    1 => "Jan",
                    2 => "Feb",
                    3 => "Mar",
                    4 => "Apr",
                    5 => "May",
                    6 => "Jun",
                    7 => "Jul",
                    8 => "Aug",
                    9 => "Sep",
                    10 => "Oct",
                    11 => "Nov",
                    12 => "Dec",
                    _ => "Unknown"
                    };
            }
        }
    }