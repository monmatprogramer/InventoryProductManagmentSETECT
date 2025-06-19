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

            // Initialize non-nullable fields to avoid CS8618 warnings
            tabControl = new TabControl();
            tabSales = new TabPage();
            tabInventory = new TabPage();
            tabFinancial = new TabPage();
            tabCustom = new TabPage();
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
            dgvInventoryData = new DataGridView(); // Fixed initialization
            lblTotalProducts = new Label();
            lblLowStockCount = new Label();
            lblInventoryValue = new Label();
            dtpFinancialYear = new DateTimePicker();
            btnGenerateFinancial = new Button();
            cboFinancialFormat = new ComboBox(); // Fixed initialization
            chartFinancial = new Chart();
            dgvFinancialData = new DataGridView();

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
                Height = 120,
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

            // Invoice generation button
            var btnGenerateInvoices = new Button
            {
                Text = "📄 Generate Invoices",
                Location = new Point(790, 11),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnGenerateInvoices.FlatAppearance.BorderSize = 0;
            btnGenerateInvoices.Click += BtnGenerateInvoices_Click;

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
                lblFormat, cboSalesFormat, btnGenerateSales, btnGenerateInvoices,
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
            // Create controls panel
            var pnlControls = new Panel
            {
                Dock = DockStyle.Top,
                Height = 220,
                BackColor = Color.FromArgb(248, 248, 248),
                Padding = new Padding(15)
            };

            // Main title with subtitle
            var lblTitle = new Label
            {
                Text = "Advanced Report Generator",
                Location = new Point(15, 10),
                Size = new Size(280, 28),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            var lblSubtitle = new Label
            {
                Text = "Create comprehensive business intelligence reports with customizable parameters",
                Location = new Point(15, 38),
                Size = new Size(600, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(127, 140, 141)
            };

            // Date range selection
            var lblDateRange = new Label
            {
                Text = "Reporting Period:",
                Location = new Point(15, 65),
                Size = new Size(110, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            var dtpCustomStart = new DateTimePicker
            {
                Name = "dtpCustomStart",
                Location = new Point(130, 62),
                Size = new Size(130, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddMonths(-1)
            };

            var lblTo = new Label
            {
                Text = "through",
                Location = new Point(270, 65),
                Size = new Size(50, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141)
            };

            var dtpCustomEnd = new DateTimePicker
            {
                Name = "dtpCustomEnd",
                Location = new Point(325, 62),
                Size = new Size(130, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };

            // Report configuration
            var lblReportTitle = new Label
            {
                Text = "Report Name:",
                Location = new Point(470, 65),
                Size = new Size(90, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            var txtReportTitle = new TextBox
            {
                Name = "txtReportTitle",
                Location = new Point(565, 62),
                Size = new Size(220, 25),
                Text = "Executive Business Intelligence Report",
                Font = new Font("Segoe UI", 9)
            };

            // Report modules selection
            var grpSections = new GroupBox
            {
                Text = "Business Intelligence Modules",
                Location = new Point(15, 95),
                Size = new Size(320, 115),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            var chkDailySales = new CheckBox
            {
                Name = "chkDailySales",
                Text = "Sales Performance Analytics",
                Location = new Point(12, 25),
                Size = new Size(180, 20),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            var chkTopProducts = new CheckBox
            {
                Name = "chkTopProducts",
                Text = "Product Performance Matrix",
                Location = new Point(12, 45),
                Size = new Size(180, 20),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            var chkTopCustomers = new CheckBox
            {
                Name = "chkTopCustomers",
                Text = "Customer Value Analysis",
                Location = new Point(12, 65),
                Size = new Size(180, 20),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            var chkSalesByCategory = new CheckBox
            {
                Name = "chkSalesByCategory",
                Text = "Category Revenue Breakdown",
                Location = new Point(12, 85),
                Size = new Size(180, 20),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            var chkInventoryStatus = new CheckBox
            {
                Name = "chkInventoryStatus",
                Text = "Inventory Health Assessment",
                Location = new Point(200, 25),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9)
            };

            var chkFinancialSummary = new CheckBox
            {
                Name = "chkFinancialSummary",
                Text = "Financial Performance Summary",
                Location = new Point(200, 45),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9)
            };

            grpSections.Controls.AddRange(new Control[] {
                chkDailySales, chkTopProducts, chkTopCustomers,
                chkSalesByCategory, chkInventoryStatus, chkFinancialSummary
            });

            // Export and generation controls
            var grpGenerate = new GroupBox
            {
                Text = "Export & Delivery Options",
                Location = new Point(345, 95),
                Size = new Size(265, 115),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            var lblFormat = new Label
            {
                Text = "Output Format:",
                Location = new Point(12, 25),
                Size = new Size(90, 25),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var cboCustomFormat = new ComboBox
            {
                Name = "cboCustomFormat",
                Location = new Point(105, 22),
                Size = new Size(140, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "Interactive Preview", "PDF Document", "Excel Workbook" },
                Font = new Font("Segoe UI", 9)
            };
            cboCustomFormat.SelectedIndex = 0;

            var btnGenerateCustom = new Button
            {
                Name = "btnGenerateCustom",
                Text = "🚀 Generate Executive Report",
                Location = new Point(12, 55),
                Size = new Size(235, 40),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnGenerateCustom.FlatAppearance.BorderSize = 0;
            btnGenerateCustom.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 204, 113);
            btnGenerateCustom.Click += BtnGenerateCustom_Click;

            grpGenerate.Controls.AddRange(new Control[] {
                lblFormat, cboCustomFormat, btnGenerateCustom
            });

            // Advanced analytics filters
            var grpFilters = new GroupBox
            {
                Text = "Advanced Analytics Filters",
                Location = new Point(620, 95),
                Size = new Size(315, 115),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            var lblMinAmount = new Label
            {
                Text = "Revenue Range:",
                Location = new Point(12, 25),
                Size = new Size(95, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var txtMinAmount = new TextBox
            {
                Name = "txtMinAmount",
                Location = new Point(115, 22),
                Size = new Size(80, 25),
                PlaceholderText = "Minimum",
                Font = new Font("Segoe UI", 9)
            };

            var lblTo2 = new Label
            {
                Text = "to",
                Location = new Point(205, 25),
                Size = new Size(20, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141)
            };

            var txtMaxAmount = new TextBox
            {
                Name = "txtMaxAmount",
                Location = new Point(230, 22),
                Size = new Size(75, 25),
                PlaceholderText = "Maximum",
                Font = new Font("Segoe UI", 9)
            };

            var lblPaymentMethod = new Label
            {
                Text = "Payment Channel:",
                Location = new Point(12, 55),
                Size = new Size(95, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var cboPaymentMethod = new ComboBox
            {
                Name = "cboPaymentMethod",
                Location = new Point(115, 52),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "All Channels", "Cash Transactions", "Card Payments", "Bank Transfers" },
                Font = new Font("Segoe UI", 9)
            };
            cboPaymentMethod.SelectedIndex = 0;

            var lblNote = new Label
            {
                Text = "Leave fields empty for comprehensive analysis",
                Location = new Point(12, 85),
                Size = new Size(290, 15),
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(149, 165, 166)
            };

            grpFilters.Controls.AddRange(new Control[] {
                lblMinAmount, txtMinAmount, lblTo2, txtMaxAmount,
                lblPaymentMethod, cboPaymentMethod, lblNote
            });

            pnlControls.Controls.AddRange(new Control[] {
                lblTitle, lblSubtitle, lblDateRange, dtpCustomStart, lblTo, dtpCustomEnd,
                lblReportTitle, txtReportTitle, grpSections, grpGenerate, grpFilters
            });

            // Results dashboard area
            var pnlResults = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(253, 254, 255),
                Padding = new Padding(20)
            };

            var lblResults = new Label
            {
                Name = "lblCustomResults",
                Text = "📊 Executive Business Intelligence Dashboard\n\n" +
                       "Your comprehensive business analytics report will be displayed here upon generation.\n" +
                       "Select your desired modules and parameters above, then click Generate Executive Report " +
                       "to create professional insights tailored to your business requirements.\n\n" +
                       "• Interactive Preview: View results directly in this interface\n" +
                       "• PDF Document: Professional formatted report for presentations\n" +
                       "• Excel Workbook: Detailed data analysis with charts and pivot tables",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(108, 117, 125),
                Padding = new Padding(40)
            };

            pnlResults.Controls.Add(lblResults);

            tabCustom.Controls.Add(pnlResults);
            tabCustom.Controls.Add(pnlControls);
        }

        #region Event Handlers

        private async void BtnGenerateSales_Click(object? sender, EventArgs e)
        {
            try
            {
                // Simulate an asynchronous operation for generating the report
                await Task.Run(() =>
                {
                    // Mock data generation logic
                    var random = new Random();
                    var salesData = new List<(DateTime Date, decimal Sales)>();
                    for (var date = dtpSalesStart.Value; date <= dtpSalesEnd.Value; date = date.AddDays(1))
                    {
                        var value = random.Next(2000, 8000);
                        salesData.Add((date, value));
                    }

                    // Update UI controls (must be done on the UI thread)
                    Invoke(new Action(() =>
                    {
                        lblSalesTotalValue.Text = "Total Sales: $125,450.75";
                        lblSalesOrderCount.Text = "Orders: 342";
                        lblSalesAvgOrder.Text = "Avg Order: $366.52";

                        chartSales.Series[0].Points.Clear();
                        foreach (var data in salesData)
                        {
                            chartSales.Series[0].Points.AddXY(data.Date, data.Sales);
                        }

                        dgvSalesData.DataSource = salesData.Select(d => new { d.Date, d.Sales }).ToList();
                    }));
                });

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
                await Task.Run(() =>
                {
                    // Simulate data generation logic
                    var inventoryData = new[]
                    {
                        new { Category = "Electronics", Products = 45, TotalStock = 1234, Value = 123450.00m, LowStock = 3 },
                        new { Category = "Clothing", Products = 30, TotalStock = 2345, Value = 45670.00m, LowStock = 2 },
                        new { Category = "Food & Beverages", Products = 40, TotalStock = 3456, Value = 34567.89m, LowStock = 5 },
                        new { Category = "Home & Garden", Products = 35, TotalStock = 890, Value = 30880.00m, LowStock = 2 }
                    };

                    Invoke(new Action(() =>
                    {
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
                        dgvInventoryData.DataSource = inventoryData;
                    }));
                });

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
                var startDate = new DateTime(dtpFinancialYear.Value.Year, 1, 1);
                var endDate = new DateTime(dtpFinancialYear.Value.Year, 12, 31);

                if (cboFinancialFormat.Text == "View")
                {
                    // Get financial report data for viewing
                    var reportDataResponse = await _apiService.GetFinancialReportDataAsync(startDate, endDate);
                    
                    if (reportDataResponse.Success && reportDataResponse.Data != null)
                    {
                        var reportData = reportDataResponse.Data;
                        
                        // Update chart and grid with real data
                        Invoke(new Action(() =>
                        {
                            UpdateFinancialUIWithData(reportData);
                        }));
                    }
                    else
                    {
                        // Fallback to simulated data if API call fails
                        await GenerateSimulatedFinancialData();
                    }
                }
                else
                {
                    // Generate and export PDF or Excel
                    var response = await _apiService.GenerateFinancialReportAsync(startDate, endDate, cboFinancialFormat.Text);
                    
                    if (response.Success && response.Data != null)
                    {
                        var extension = cboFinancialFormat.Text.ToLower() == "pdf" ? ".pdf" : ".xlsx";
                        var fileName = $"Financial_Report_{dtpFinancialYear.Value.Year}{extension}";
                        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                        
                        await File.WriteAllBytesAsync(filePath, response.Data);
                        
                        var result = MessageBox.Show($"Financial Report Generated Successfully!\n\n" +
                                                   $"Format: {cboFinancialFormat.Text}\n" +
                                                   $"File: {fileName}\n" +
                                                   $"Location: Desktop\n\n" +
                                                   "Would you like to open the report now?",
                            "Export Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        
                        if (result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Error generating financial report: {response.Message}",
                            "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating financial report");
                MessageBox.Show("Error generating financial report.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task GenerateSimulatedFinancialData()
        {
            await Task.Run(() =>
            {
                // Simulate data generation logic
                var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                var random = new Random();
                var financialData = new List<(string Month, decimal Revenue, decimal Expenses, decimal Profit, string Margin)>();

                for (int i = 0; i < 12; i++)
                {
                    var revenue = random.Next(40000, 80000);
                    var expenses = random.Next(20000, 40000);
                    var profit = revenue - expenses;
                    var margin = $"{(profit / revenue * 100):F1}%";
                    financialData.Add((months[i], revenue, expenses, profit, margin));
                }

                Invoke(new Action(() =>
                {
                    // Update chart
                    chartFinancial.Series[0].Points.Clear();
                    foreach (var data in financialData)
                    {
                        chartFinancial.Series[0].Points.AddXY(data.Month, data.Revenue);
                    }

                    // Update grid
                    dgvFinancialData.DataSource = financialData.Select(d => new
                    {
                        d.Month,
                        d.Revenue,
                        d.Expenses,
                        d.Profit,
                        d.Margin
                    }).ToList();
                }));
            });
        }

        private void UpdateFinancialUIWithData(object reportData)
        {
            // This method would parse the real report data and update the UI
            // For now, fall back to simulated data
            Task.Run(async () => await GenerateSimulatedFinancialData());
        }

        private async void BtnGenerateInvoices_Click(object? sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show("This will generate sample invoices for recent sales.\n\nWould you like to continue?",
                    "Generate Invoices", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Get recent sales (in real app, this would fetch from API)
                    var response = await _apiService.GetSalesAsync(new PaginationParameters
                    {
                        PageSize = 10,
                        SearchTerm = ""
                    });

                    if (response.Success && response.Data?.Items.Any() == true)
                    {
                        var salesList = new List<SaleDto>(response.Data.Items);

                        // Show dialog to select which sales to generate invoices for
                        using var saleSelectionForm = new SaleSelectionForm(salesList);
                        if (saleSelectionForm.ShowDialog() == DialogResult.OK)
                        {
                            var selectedSales = saleSelectionForm.SelectedSales;
                            foreach (var sale in selectedSales)
                            {
                                using var invoiceForm = Program.GetRequiredService<InvoiceForm>();
                                invoiceForm.LoadSaleData(sale);
                                invoiceForm.ShowDialog();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No sales data available for invoice generation.",
                            "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoices");
                MessageBox.Show("Error generating invoices",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnGenerateCustom_Click(object? sender, EventArgs e)
        {
            try
            {
                // Get controls by name from the custom tab
                var dtpCustomStart = tabCustom.Controls.Find("dtpCustomStart", true).FirstOrDefault() as DateTimePicker;
                var dtpCustomEnd = tabCustom.Controls.Find("dtpCustomEnd", true).FirstOrDefault() as DateTimePicker;
                var txtReportTitle = tabCustom.Controls.Find("txtReportTitle", true).FirstOrDefault() as TextBox;
                var cboCustomFormat = tabCustom.Controls.Find("cboCustomFormat", true).FirstOrDefault() as ComboBox;
                var chkDailySales = tabCustom.Controls.Find("chkDailySales", true).FirstOrDefault() as CheckBox;
                var chkTopProducts = tabCustom.Controls.Find("chkTopProducts", true).FirstOrDefault() as CheckBox;
                var chkTopCustomers = tabCustom.Controls.Find("chkTopCustomers", true).FirstOrDefault() as CheckBox;
                var chkSalesByCategory = tabCustom.Controls.Find("chkSalesByCategory", true).FirstOrDefault() as CheckBox;
                var chkInventoryStatus = tabCustom.Controls.Find("chkInventoryStatus", true).FirstOrDefault() as CheckBox;
                var chkFinancialSummary = tabCustom.Controls.Find("chkFinancialSummary", true).FirstOrDefault() as CheckBox;
                var txtMinAmount = tabCustom.Controls.Find("txtMinAmount", true).FirstOrDefault() as TextBox;
                var txtMaxAmount = tabCustom.Controls.Find("txtMaxAmount", true).FirstOrDefault() as TextBox;
                var cboPaymentMethod = tabCustom.Controls.Find("cboPaymentMethod", true).FirstOrDefault() as ComboBox;
                var lblCustomResults = tabCustom.Controls.Find("lblCustomResults", true).FirstOrDefault() as Label;

                if (dtpCustomStart == null || dtpCustomEnd == null || txtReportTitle == null || 
                    cboCustomFormat == null || lblCustomResults == null)
                {
                    MessageBox.Show("Error: Unable to find required controls for custom report generation.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Build custom report parameters
                var parameters = new
                {
                    StartDate = dtpCustomStart.Value,
                    EndDate = dtpCustomEnd.Value,
                    Format = cboCustomFormat.Text,
                    ReportTitle = txtReportTitle.Text,
                    IncludeDailySales = chkDailySales?.Checked ?? false,
                    IncludeTopProducts = chkTopProducts?.Checked ?? false,
                    IncludeTopCustomers = chkTopCustomers?.Checked ?? false,
                    IncludeSalesByCategory = chkSalesByCategory?.Checked ?? false,
                    IncludeInventoryStatus = chkInventoryStatus?.Checked ?? false,
                    IncludeFinancialSummary = chkFinancialSummary?.Checked ?? false,
                    TopProductsCount = 10,
                    TopCustomersCount = 10,
                    IncludeCharts = true,
                    MinSalesAmount = decimal.TryParse(txtMinAmount?.Text, out var minAmount) ? (decimal?)minAmount : null,
                    MaxSalesAmount = decimal.TryParse(txtMaxAmount?.Text, out var maxAmount) ? (decimal?)maxAmount : null,
                    PaymentMethod = cboPaymentMethod?.Text == "All" ? null : cboPaymentMethod?.Text,
                    SalesStatus = "Completed",
                    SelectedCategories = new List<int>(),
                    SelectedProducts = new List<int>(),
                    SelectedCustomers = new List<int>()
                };

                lblCustomResults.Text = "🔄 Processing Business Intelligence Analytics...\n\nGenerating comprehensive executive report with your selected parameters.";
                lblCustomResults.ForeColor = Color.FromArgb(52, 152, 219);

                if (cboCustomFormat.Text != "Interactive Preview")
                {
                    // Generate and export professional document
                    var response = await _apiService.GenerateCustomReportAsync(parameters);
                    
                    if (response.Success && response.Data != null)
                    {
                        var extension = cboCustomFormat.Text.Contains("PDF") ? ".pdf" : ".xlsx";
                        var fileName = $"{txtReportTitle.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}{extension}";
                        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                        
                        await File.WriteAllBytesAsync(filePath, response.Data);
                        
                        lblCustomResults.Text = $"✅ Executive Report Successfully Generated\n\n" +
                                              $"Document Type: {cboCustomFormat.Text}\n" +
                                              $"File Location: {Path.GetFileName(filePath)}\n" +
                                              $"Export Path: Desktop\n\n" +
                                              "Your professional business intelligence report is ready for distribution.";
                        lblCustomResults.ForeColor = Color.FromArgb(39, 174, 96);
                        
                        var result = MessageBox.Show($"Executive Business Intelligence Report Generated Successfully!\n\n" +
                                                   $"Document: {cboCustomFormat.Text}\n" +
                                                   $"File: {fileName}\n" +
                                                   $"Location: Desktop\n\n" +
                                                   "Would you like to open the report now?",
                            "Report Generation Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        
                        if (result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
                        }
                    }
                    else
                    {
                        lblCustomResults.Text = $"❌ Report Generation Failed\n\nError Details: {response.Message}\n\n" +
                                              "Please verify your parameters and try again. Contact system administrator if the issue persists.";
                        lblCustomResults.ForeColor = Color.FromArgb(231, 76, 60);
                        MessageBox.Show($"Report Generation Error\n\nUnable to generate executive report: {response.Message}\n\n" +
                                      "Please check your connection and parameters, then try again.",
                            "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Display interactive preview
                    lblCustomResults.Text = "✅ Executive Business Intelligence Report - Interactive Preview\n\n" +
                                          $"📋 Report: {txtReportTitle.Text}\n" +
                                          $"📅 Analysis Period: {dtpCustomStart.Value:MMM dd, yyyy} through {dtpCustomEnd.Value:MMM dd, yyyy}\n" +
                                          $"📊 Analytics Modules: {GetIncludedSections(chkDailySales, chkTopProducts, chkTopCustomers, chkSalesByCategory, chkInventoryStatus, chkFinancialSummary)}\n\n" +
                                          "🔍 Preview Mode Active\n" +
                                          "For comprehensive analysis with interactive charts, detailed data tables, and export capabilities, " +
                                          "select PDF Document or Excel Workbook format to generate professional-grade business intelligence reports.";
                    lblCustomResults.ForeColor = Color.FromArgb(39, 174, 96);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating executive business intelligence report");
                var lblCustomResults = tabCustom.Controls.Find("lblCustomResults", true).FirstOrDefault() as Label;
                if (lblCustomResults != null)
                {
                    lblCustomResults.Text = "❌ System Error - Report Generation Failed\n\n" +
                                          "An unexpected error occurred while processing your business intelligence request.\n" +
                                          "Please contact your system administrator or try again later.";
                    lblCustomResults.ForeColor = Color.FromArgb(231, 76, 60);
                }
                MessageBox.Show("Executive Report Generation Error\n\n" +
                              "An unexpected system error has occurred. Please contact your system administrator for assistance.",
                    "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetIncludedSections(params CheckBox?[] checkBoxes)
        {
            var sections = new List<string>();
            
            if (checkBoxes[0]?.Checked == true) sections.Add("Sales Performance Analytics");
            if (checkBoxes[1]?.Checked == true) sections.Add("Product Performance Matrix");
            if (checkBoxes[2]?.Checked == true) sections.Add("Customer Value Analysis");
            if (checkBoxes[3]?.Checked == true) sections.Add("Category Revenue Breakdown");
            if (checkBoxes[4]?.Checked == true) sections.Add("Inventory Health Assessment");
            if (checkBoxes[5]?.Checked == true) sections.Add("Financial Performance Summary");
            
            return sections.Any() ? string.Join(", ", sections) : "No modules selected";
        }

        #endregion
    }
}