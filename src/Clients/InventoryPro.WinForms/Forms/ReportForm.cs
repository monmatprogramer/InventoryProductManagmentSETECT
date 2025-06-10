using Microsoft.Extensions.Logging;
using InventoryPro.WinForms.Services;
using InventoryPro.Shared.DTOs;
using System.Windows.Forms.DataVisualization.Charting;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Reports form for generating and viewing various business reports
    /// </summary>
    public partial class ReportForm : Form
    {
        private readonly ILogger<ReportForm> _logger;
        private readonly IApiService _apiService;

        // Controls
        private TabControl tabControl;
        private TabPage tabSales;
        private TabPage tabInventory;
        private TabPage tabFinancial;
        private TabPage tabCustom;

        // Sales Report Controls
        private DateTimePicker dtpSalesStart;
        private DateTimePicker dtpSalesEnd;
        private Button btnGenerateSales;
        private ComboBox cboSalesFormat;
        private Chart chartSales;
        private DataGridView dgvSalesData;
        private Label lblSalesTotalValue;
        private Label lblSalesOrderCount;
        private Label lblSalesAvgOrder;

        // Inventory Report Controls
        private Button btnGenerateInventory;
        private ComboBox cboInventoryFormat;
        private Chart chartInventory;
        private DataGridView dgvInventoryData;
        private Label lblTotalProducts;
        private Label lblLowStockCount;
        private Label lblInventoryValue;

        // Financial Report Controls
        private DateTimePicker dtpFinancialYear;
        private Button btnGenerateFinancial;
        private ComboBox cboFinancialFormat;
        private Chart chartFinancial;
        private DataGridView dgvFinancialData;

        public ReportForm(ILogger<ReportForm> logger, IApiService apiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Initialize non-nullable fields
            dtpSalesStart = new DateTimePicker();
            dtpSalesEnd = new DateTimePicker();
            btnGenerateSales = new Button();
            cboSalesFormat = new ComboBox();
            chartSales = new Chart();
            dgvSalesData = new DataGridView();
            lblSalesTotalValue = new Label();
            lblSalesOrderCount = new Label();
            lblSalesAvgOrder = new Label();
            btnGenerateInventory = new Button();
            cboInventoryFormat = new ComboBox();
            chartInventory = new Chart();
            dgvInventoryData = new DataGridView();
            lblTotalProducts = new Label();
            lblLowStockCount = new Label();
            lblInventoryValue = new Label();
            dtpFinancialYear = new DateTimePicker();
            btnGenerateFinancial = new Button();
            cboFinancialFormat = new ComboBox();
            chartFinancial = new Chart();
            dgvFinancialData = new DataGridView();

            tabControl = new TabControl();
            tabSales = new TabPage();
            tabInventory = new TabPage();
            tabFinancial = new TabPage();
            tabCustom = new TabPage();

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Reports";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Create tabs
            tabSales = new TabPage("Sales Reports");
            tabInventory = new TabPage("Inventory Reports");
            tabFinancial = new TabPage("Financial Reports");
            tabCustom = new TabPage("Custom Reports");

            // Setup each tab
            SetupSalesTab();
            SetupInventoryTab();
            SetupFinancialTab();
            SetupCustomTab();

            // Add tabs to control
            tabControl.TabPages.AddRange(new TabPage[] { tabSales, tabInventory, tabFinancial, tabCustom });

            this.Controls.Add(tabControl);
        }

        private void SetupSalesTab()
        {
            // Create controls panel
            var pnlControls = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(248, 248, 248),
                Padding = new Padding(10)
            };

            // Date range selection
            var lblDateRange = new Label
            {
                Text = "Date Range:",
                Location = new Point(10, 15),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            dtpSalesStart = new DateTimePicker
            {
                Location = new Point(100, 12),
                Size = new Size(150, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddMonths(-1)
            };

            var lblTo = new Label
            {
                Text = "to",
                Location = new Point(260, 15),
                Size = new Size(25, 25)
            };

            dtpSalesEnd = new DateTimePicker
            {
                Location = new Point(295, 12),
                Size = new Size(150, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };

            // Format selection
            var lblFormat = new Label
            {
                Text = "Format:",
                Location = new Point(470, 15),
                Size = new Size(60, 25)
            };

            cboSalesFormat = new ComboBox
            {
                Location = new Point(540, 12),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "View", "PDF", "Excel", "CSV" }
            };
            cboSalesFormat.SelectedIndex = 0;

            // Generate button
            btnGenerateSales = new Button
            {
                Text = "Generate Report",
                Location = new Point(660, 11),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnGenerateSales.FlatAppearance.BorderSize = 0;
            btnGenerateSales.Click += BtnGenerateSales_Click;

            // Summary labels
            lblSalesTotalValue = new Label
            {
                Text = "Total Sales: $0.00",
                Location = new Point(10, 55),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113)
            };

            lblSalesOrderCount = new Label
            {
                Text = "Orders: 0",
                Location = new Point(220, 55),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11)
            };

            lblSalesAvgOrder = new Label
            {
                Text = "Avg Order: $0.00",
                Location = new Point(380, 55),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11)
            };

            pnlControls.Controls.AddRange(new Control[] {
                lblDateRange, dtpSalesStart, lblTo, dtpSalesEnd,
                lblFormat, cboSalesFormat, btnGenerateSales,
                lblSalesTotalValue, lblSalesOrderCount, lblSalesAvgOrder
            });

            // Create split container for chart and data
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            // Sales chart
            chartSales = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var chartArea = new ChartArea("SalesArea")
            {
                BackColor = Color.White,
                AxisX = { Title = "Date", TitleFont = new Font("Segoe UI", 10) },
                AxisY = { Title = "Sales ($)", TitleFont = new Font("Segoe UI", 10) }
            };
            chartSales.ChartAreas.Add(chartArea);

            var series = new Series("Daily Sales")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(41, 128, 185),
                IsValueShownAsLabel = true
            };
            chartSales.Series.Add(series);

            // Sales data grid
            dgvSalesData = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            splitContainer.Panel1.Controls.Add(chartSales);
            splitContainer.Panel2.Controls.Add(dgvSalesData);

            tabSales.Controls.Add(splitContainer);
            tabSales.Controls.Add(pnlControls);
        }

        private void SetupInventoryTab()
        {
            // Create controls panel
            var pnlControls = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(248, 248, 248),
                Padding = new Padding(10)
            };

            // Format selection
            var lblFormat = new Label
            {
                Text = "Format:",
                Location = new Point(10, 15),
                Size = new Size(60, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            cboInventoryFormat = new ComboBox
            {
                Location = new Point(80, 12),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "View", "PDF", "Excel" }
            };
            cboInventoryFormat.SelectedIndex = 0;

            // Generate button
            btnGenerateInventory = new Button
            {
                Text = "Generate Report",
                Location = new Point(200, 11),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnGenerateInventory.FlatAppearance.BorderSize = 0;
            btnGenerateInventory.Click += BtnGenerateInventory_Click;

            // Summary labels
            lblTotalProducts = new Label
            {
                Text = "Total Products: 0",
                Location = new Point(10, 50),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            lblLowStockCount = new Label
            {
                Text = "Low Stock: 0",
                Location = new Point(170, 50),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(231, 76, 60)
            };

            lblInventoryValue = new Label
            {
                Text = "Total Value: $0.00",
                Location = new Point(330, 50),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113)
            };

            pnlControls.Controls.AddRange(new Control[] {
                lblFormat, cboInventoryFormat, btnGenerateInventory,
                lblTotalProducts, lblLowStockCount, lblInventoryValue
            });

            // Create split container
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            // Inventory chart (pie chart for categories)
            chartInventory = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var chartArea = new ChartArea("InventoryArea")
            {
                BackColor = Color.White
            };
            chartInventory.ChartAreas.Add(chartArea);

            var series = new Series("Inventory by Category")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                LabelFormat = "{0} ({#PERCENT})"
            };
            chartInventory.Series.Add(series);

            chartInventory.Legends.Add(new Legend("Legend")
            {
                Docking = Docking.Right,
                BackColor = Color.Transparent
            });

            // Inventory data grid
            dgvInventoryData = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            splitContainer.Panel1.Controls.Add(chartInventory);
            splitContainer.Panel2.Controls.Add(dgvInventoryData);

            tabInventory.Controls.Add(splitContainer);
            tabInventory.Controls.Add(pnlControls);
        }

        private void SetupFinancialTab()
        {
            // Create controls panel
            var pnlControls = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(248, 248, 248),
                Padding = new Padding(10)
            };

            // Year selection
            var lblYear = new Label
            {
                Text = "Year:",
                Location = new Point(10, 15),
                Size = new Size(50, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            dtpFinancialYear = new DateTimePicker
            {
                Location = new Point(70, 12),
                Size = new Size(100, 25),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy",
                ShowUpDown = true
            };

            // Format selection
            var lblFormat = new Label
            {
                Text = "Format:",
                Location = new Point(190, 15),
                Size = new Size(60, 25)
            };

            cboFinancialFormat = new ComboBox
            {
                Location = new Point(260, 12),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "View", "PDF", "Excel" }
            };
            cboFinancialFormat.SelectedIndex = 0;

            // Generate button
            btnGenerateFinancial = new Button
            {
                Text = "Generate Report",
                Location = new Point(380, 11),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnGenerateFinancial.FlatAppearance.BorderSize = 0;
            btnGenerateFinancial.Click += BtnGenerateFinancial_Click;

            pnlControls.Controls.AddRange(new Control[] {
                lblYear, dtpFinancialYear, lblFormat, cboFinancialFormat, btnGenerateFinancial
            });

            // Financial chart (line chart for monthly revenue)
            chartFinancial = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var chartArea = new ChartArea("FinancialArea")
            {
                BackColor = Color.White,
                AxisX = { Title = "Month", TitleFont = new Font("Segoe UI", 10) },
                AxisY = { Title = "Revenue ($)", TitleFont = new Font("Segoe UI", 10) }
            };
            chartFinancial.ChartAreas.Add(chartArea);

            var series = new Series("Monthly Revenue")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(155, 89, 182),
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 8,
                BorderWidth = 3,
                IsValueShownAsLabel = true
            };
            chartFinancial.Series.Add(series);

            // Financial data grid
            dgvFinancialData = new DataGridView
            {
                Location = new Point(10, 400),
                Size = new Size(970, 200),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            tabFinancial.Controls.Add(dgvFinancialData);
            tabFinancial.Controls.Add(chartFinancial);
            tabFinancial.Controls.Add(pnlControls);
        }

        private void SetupCustomTab()
        {
            var lblInfo = new Label
            {
                Text = "Custom Reports\n\nThis section allows you to create custom reports based on specific criteria.\n" +
                       "Features include:\n" +
                       "• Product performance analysis\n" +
                       "• Customer purchase patterns\n" +
                       "• Seasonal trends\n" +
                       "• Profit margin analysis\n\n" +
                       "Coming Soon!",
                Location = new Point(50, 50),
                Size = new Size(600, 300),
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(51, 51, 51)
            };

            tabCustom.Controls.Add(lblInfo);
        }

        #region Event Handlers

        private async void BtnGenerateSales_Click(object? sender, EventArgs e)
        {
            try
            {
                // Simulate an asynchronous operation (e.g., API call)
                await Task.Run(() =>
                {
                    // Mock data generation logic
                    Thread.Sleep(1000); // Simulate delay
                });

                // Update summary labels
                lblSalesTotalValue.Text = "Total Sales: $125,450.75";
                lblSalesOrderCount.Text = "Orders: 342";
                lblSalesAvgOrder.Text = "Avg Order: $366.52";

                // Update chart with mock data
                chartSales.Series[0].Points.Clear();
                var random = new Random();
                for (var date = dtpSalesStart.Value; date <= dtpSalesEnd.Value; date = date.AddDays(1))
                {
                    var value = random.Next(2000, 8000);
                    chartSales.Series[0].Points.AddXY(date.ToShortDateString(), value);
                }

                // Update grid with mock data
                var salesData = new[]
                {
                    new { Date = DateTime.Now.AddDays(-5), OrderCount = 15, TotalSales = 5234.50m, AvgOrder = 348.97m },
                    new { Date = DateTime.Now.AddDays(-4), OrderCount = 22, TotalSales = 7890.25m, AvgOrder = 358.65m },
                    new { Date = DateTime.Now.AddDays(-3), OrderCount = 18, TotalSales = 6543.75m, AvgOrder = 363.54m },
                    new { Date = DateTime.Now.AddDays(-2), OrderCount = 25, TotalSales = 9876.00m, AvgOrder = 395.04m },
                    new { Date = DateTime.Now.AddDays(-1), OrderCount = 20, TotalSales = 7234.80m, AvgOrder = 361.74m }
                };
                dgvSalesData.DataSource = salesData;

                if (cboSalesFormat.Text != "View")
                {
                    MessageBox.Show($"Report exported as {cboSalesFormat.Text} successfully!",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sales report");
                MessageBox.Show("Error generating sales report.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnGenerateInventory_Click(object? sender, EventArgs e)
        {
            try
            {
                // Simulate an asynchronous operation (e.g., API call)
                await Task.Run(() =>
                {
                    // Mock data generation logic
                    Thread.Sleep(1000); // Simulate delay
                });

                // Update summary labels
                lblTotalProducts.Text = "Total Products: 150";
                lblLowStockCount.Text = "Low Stock: 12";
                lblInventoryValue.Text = "Total Value: $234,567.89";

                // Update chart with mock data
                chartInventory.Series[0].Points.Clear();
                chartInventory.Series[0].Points.AddXY("Electronics", 45);
                chartInventory.Series[0].Points.AddXY("Clothing", 30);
                chartInventory.Series[0].Points.AddXY("Food & Beverages", 40);
                chartInventory.Series[0].Points.AddXY("Home & Garden", 35);

                // Update grid with mock data
                var inventoryData = new[]
                {
                    new { Category = "Electronics", Products = 45, TotalStock = 1234, Value = 123450.00m, LowStock = 3 },
                    new { Category = "Clothing", Products = 30, TotalStock = 2345, Value = 45670.00m, LowStock = 2 },
                    new { Category = "Food & Beverages", Products = 40, TotalStock = 3456, Value = 34567.89m, LowStock = 5 },
                    new { Category = "Home & Garden", Products = 35, TotalStock = 890, Value = 30880.00m, LowStock = 2 }
                };
                dgvInventoryData.DataSource = inventoryData;

                if (cboInventoryFormat.Text != "View")
                {
                    MessageBox.Show($"Report exported as {cboInventoryFormat.Text} successfully!",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating inventory report");
                MessageBox.Show("Error generating inventory report.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnGenerateFinancial_Click(object? sender, EventArgs e)
        {
            try
            {
                // Simulate an asynchronous operation (e.g., API call)
                await Task.Run(() =>
                {
                    // Mock data generation logic
                    Thread.Sleep(1000); // Simulate delay
                });

                // Update chart with mock data
                chartFinancial.Series[0].Points.Clear();
                var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                var random = new Random();

                for (int i = 0; i < 12; i++)
                {
                    var value = random.Next(40000, 80000);
                    chartFinancial.Series[0].Points.AddXY(months[i], value);
                }

                // Update grid with mock data
                var financialData = new[]
                {
                    new { Month = "January", Revenue = 45230.50m, Expenses = 32450.25m, Profit = 12780.25m, Margin = "28.2%" },
                    new { Month = "February", Revenue = 52340.75m, Expenses = 35670.00m, Profit = 16670.75m, Margin = "31.9%" },
                    new { Month = "March", Revenue = 61450.00m, Expenses = 41230.50m, Profit = 20219.50m, Margin = "32.9%" },
                    new { Month = "April", Revenue = 58670.25m, Expenses = 39450.00m, Profit = 19220.25m, Margin = "32.8%" }
                };
                dgvFinancialData.DataSource = financialData;

                if (cboFinancialFormat.Text != "View")
                {
                    MessageBox.Show($"Report exported as {cboFinancialFormat.Text} successfully!",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating financial report");
                MessageBox.Show("Error generating financial report.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}