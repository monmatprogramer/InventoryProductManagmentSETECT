using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
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
        
        // Performance and cancellation management
        private CancellationTokenSource? _salesReportCancellationTokenSource;
        private readonly object _loadingLock = new object();
        private bool _isLoadingSalesData = false;

        // Data structure for robust sales data handling
        private record SalesDataExtracted(
            decimal TotalSales,
            int TotalOrders,
            decimal AverageOrderValue,
            List<DailySalesPoint> DailySales,
            int DailySalesCount);

        private record DailySalesPoint(DateTime Date, decimal Amount, int OrderCount);

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
        private ProgressBar pbSalesLoading;
        private Label lblSalesLoadingStatus;

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
            pbSalesLoading = new ProgressBar();
            lblSalesLoadingStatus = new Label();
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
                Height = 140,
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

            // Loading indicator controls
            pbSalesLoading = new ProgressBar
                {
                Location = new Point(10, 85),
                Size = new Size(400, 20),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
                };

            lblSalesLoadingStatus = new Label
                {
                Text = "Loading sales data...",
                Location = new Point(420, 85),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(52, 152, 219),
                Visible = false
                };

            pnlControls.Controls.AddRange(new Control[] {
                lblDateRange, dtpSalesStart, lblTo, dtpSalesEnd,
                lblFormat, cboSalesFormat, btnGenerateSales,
                lblSalesTotalValue, lblSalesOrderCount, lblSalesAvgOrder,
                pbSalesLoading, lblSalesLoadingStatus
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
                AxisX = { 
                    Title = "Date", 
                    TitleFont = new Font("Segoe UI", 10, FontStyle.Bold),
                    TitleForeColor = Color.FromArgb(52, 73, 94),
                    LabelStyle = { Format = "MM/dd", Font = new Font("Segoe UI", 9) },
                    MajorGrid = { LineColor = Color.FromArgb(230, 230, 230), LineWidth = 1 },
                    LineColor = Color.FromArgb(149, 165, 166)
                },
                AxisY = { 
                    Title = "Sales ($)", 
                    TitleFont = new Font("Segoe UI", 10, FontStyle.Bold),
                    TitleForeColor = Color.FromArgb(52, 73, 94),
                    LabelStyle = { Format = "C0", Font = new Font("Segoe UI", 9) },
                    MajorGrid = { LineColor = Color.FromArgb(230, 230, 230), LineWidth = 1 },
                    LineColor = Color.FromArgb(149, 165, 166)
                }
                };
            chartSales.ChartAreas.Add(chartArea);

            var series = new Series("Daily Sales")
                {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(41, 128, 185),
                IsValueShownAsLabel = true,
                LabelFormat = "C0",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                BorderWidth = 2,
                BorderColor = Color.FromArgb(31, 97, 141),
                BackGradientStyle = GradientStyle.TopBottom,
                BackSecondaryColor = Color.FromArgb(52, 152, 219),
                LabelForeColor = Color.FromArgb(44, 62, 80)
                };
            chartSales.Series.Add(series);

            // Sales data grid
            dgvSalesData = new DataGridView
                {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                DefaultCellStyle = new DataGridViewCellStyle
                    {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Font = new Font("Segoe UI", 9),
                    SelectionBackColor = Color.FromArgb(52, 152, 219),
                    SelectionForeColor = Color.White
                    },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                    {
                    BackColor = Color.FromArgb(248, 248, 248),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Font = new Font("Segoe UI", 9),
                    SelectionBackColor = Color.FromArgb(52, 152, 219),
                    SelectionForeColor = Color.White
                    },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                    {
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                    },
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(189, 195, 199)
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
                BackColor = Color.White,
                Area3DStyle = { Enable3D = true, Inclination = 15, Rotation = 10 }
                };
            chartInventory.ChartAreas.Add(chartArea);

            var series = new Series("Inventory by Category")
                {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                LabelFormat = "{0}\n{#PERCENT{P1}}",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                LabelForeColor = Color.White,
                BorderWidth = 2,
                BorderColor = Color.White,
                CustomProperties = "PieLabelStyle=Outside"
                };
            chartInventory.Series.Add(series);

            chartInventory.Legends.Add(new Legend("Legend")
                {
                Docking = Docking.Right,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(52, 73, 94),
                BorderColor = Color.FromArgb(189, 195, 199),
                BorderWidth = 1,
                BorderDashStyle = ChartDashStyle.Solid
                });

            // Inventory data grid
            dgvInventoryData = new DataGridView
                {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                DefaultCellStyle = new DataGridViewCellStyle
                    {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Font = new Font("Segoe UI", 9),
                    SelectionBackColor = Color.FromArgb(46, 204, 113),
                    SelectionForeColor = Color.White
                    },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                    {
                    BackColor = Color.FromArgb(248, 248, 248),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Font = new Font("Segoe UI", 9),
                    SelectionBackColor = Color.FromArgb(46, 204, 113),
                    SelectionForeColor = Color.White
                    },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                    {
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                    },
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(189, 195, 199)
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
                AxisX = { 
                    Title = "Month", 
                    TitleFont = new Font("Segoe UI", 10, FontStyle.Bold),
                    TitleForeColor = Color.FromArgb(52, 73, 94),
                    LabelStyle = { Font = new Font("Segoe UI", 9) },
                    MajorGrid = { LineColor = Color.FromArgb(230, 230, 230), LineWidth = 1 },
                    LineColor = Color.FromArgb(149, 165, 166)
                },
                AxisY = { 
                    Title = "Revenue ($)", 
                    TitleFont = new Font("Segoe UI", 10, FontStyle.Bold),
                    TitleForeColor = Color.FromArgb(52, 73, 94),
                    LabelStyle = { Format = "C0", Font = new Font("Segoe UI", 9) },
                    MajorGrid = { LineColor = Color.FromArgb(230, 230, 230), LineWidth = 1 },
                    LineColor = Color.FromArgb(149, 165, 166)
                }
                };
            chartFinancial.ChartAreas.Add(chartArea);

            var series = new Series("Monthly Revenue")
                {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(155, 89, 182),
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 10,
                MarkerColor = Color.FromArgb(142, 68, 173),
                MarkerBorderColor = Color.White,
                MarkerBorderWidth = 2,
                BorderWidth = 4,
                IsValueShownAsLabel = true,
                LabelFormat = "C0",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                LabelForeColor = Color.FromArgb(44, 62, 80),
                ShadowOffset = 2,
                ShadowColor = Color.FromArgb(100, 0, 0, 0)
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                DefaultCellStyle = new DataGridViewCellStyle
                    {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Font = new Font("Segoe UI", 9),
                    SelectionBackColor = Color.FromArgb(155, 89, 182),
                    SelectionForeColor = Color.White
                    },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                    {
                    BackColor = Color.FromArgb(248, 248, 248),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Font = new Font("Segoe UI", 9),
                    SelectionBackColor = Color.FromArgb(155, 89, 182),
                    SelectionForeColor = Color.White
                    },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                    {
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                    },
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(189, 195, 199)
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
            // Prevent multiple concurrent requests
            lock (_loadingLock)
                {
                if (_isLoadingSalesData)
                    {
                    MessageBox.Show("A sales report is already being generated. Please wait for it to complete.",
                        "Operation in Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                    }
                }

            // Cancel any existing request
            _salesReportCancellationTokenSource?.Cancel();
            _salesReportCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _salesReportCancellationTokenSource.Token;

            try
                {
                ShowSalesLoadingState();

                if (cboSalesFormat.Text == "View")
                    {
                    try
                        {

                        // Get sales data for viewing
                        var dataResponse = await _apiService.GetSalesReportDataAsync(dtpSalesStart.Value, dtpSalesEnd.Value);

                        if (dataResponse.Success && dataResponse.Data != null)
                            {
                            // Process and display the data safely
                            var responseData = dataResponse.Data;

                            // Safely extract data using property extraction helper
                            var totalSales = GetPropertyValue<decimal>(responseData, "TotalSales", 0.0m);
                            var totalOrders = GetPropertyValue<int>(responseData, "TotalOrders", 0);
                            var averageOrderValue = GetPropertyValue<decimal>(responseData, "AverageOrderValue", 0.0m);
                            var dailySales = GetPropertyValue<object?>(responseData, "DailySales", null);

                            // Update summary labels with real data
                            lblSalesTotalValue.Text = $"Total Sales: ${totalSales:N2}";
                            lblSalesOrderCount.Text = $"Orders: {totalOrders:N0}";
                            lblSalesAvgOrder.Text = $"Avg Order: ${averageOrderValue:N2}";

                            // Update chart with real data
                            chartSales.Series[0].Points.Clear();
                            if (dailySales != null)
                                {
                                if (dailySales is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                                    {
                                    foreach (var item in jsonElement.EnumerateArray())
                                        {
                                        if (item.TryGetProperty("Date", out var dateProperty) &&
                                            item.TryGetProperty("TotalAmount", out var amountProperty))
                                            {
                                            if (DateTime.TryParse(dateProperty.GetString(), out var date) &&
                                                amountProperty.TryGetDecimal(out var amount))
                                                {
                                                chartSales.Series[0].Points.AddXY(date.ToShortDateString(), amount);
                                                }
                                            }
                                        }
                                    }
                                else if (dailySales is IEnumerable<object> enumerable)
                                    {
                                    foreach (var item in enumerable)
                                        {
                                        var date = GetPropertyValue<DateTime>(item, "Date", DateTime.MinValue);
                                        var amount = GetPropertyValue<decimal>(item, "TotalAmount", 0.0m);
                                        if (date != DateTime.MinValue)
                                            {
                                            chartSales.Series[0].Points.AddXY(date.ToShortDateString(), amount);
                                            }
                                        }
                                    }
                                }

                            // Update grid with real data
                            if (dailySales != null)
                                {
                                try
                                    {
                                    dgvSalesData.DataSource = dailySales;
                                    }
                                catch (Exception ex)
                                    {
                                    _logger.LogWarning(ex, "Failed to bind daily sales to grid");
                                    ShowEmptySalesState("Data Binding Error", "Unable to display sales data in the correct format.");
                                    return;
                                    }
                                }
                            else
                                {
                                ShowEmptySalesState("No Sales Data", "No sales data available for the selected date range.");
                                return;
                                }

                            // If no chart data was added, try to use aggregated data or show empty state
                            if (chartSales.Series[0].Points.Count == 0)
                                {
                                if (totalSales > 0 && totalOrders > 0)
                                    {
                                    // Create single aggregate point if we have totals but no daily breakdown
                                    var avgDaily = totalSales / Math.Max(1, (dtpSalesEnd.Value - dtpSalesStart.Value).Days + 1);
                                    chartSales.Series[0].Points.AddXY("Period Average", avgDaily);
                                    chartSales.ChartAreas[0].AxisX.Title = "Period";
                                    }
                                else
                                    {
                                    // Show empty state message
                                    chartSales.Titles.Clear();
                                    chartSales.Titles.Add(new Title("No sales data available for selected period", 
                                        Docking.Top, new Font("Segoe UI", 12), Color.FromArgb(127, 140, 141)));
                                    }
                                }
                            }
                        else
                            {
                            _logger.LogWarning("Failed to load sales data from API: {Message}", dataResponse.Message);
                            ShowEmptySalesState("Unable to load live sales data", dataResponse.Message);

                            // Provide more specific error messaging based on the type of error
                            var errorTitle = "Data Loading Error";
                            var errorMessage = GetUserFriendlyErrorMessage(dataResponse.Message, dataResponse.StatusCode);
                            
                            var result = MessageBox.Show($"{errorMessage}\n\nWould you like to try refreshing the data?",
                                errorTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            
                            if (result == DialogResult.Yes)
                                {
                                BtnGenerateSales_Click(sender, e);
                                return;
                                }
                            }
                        }
                    catch (Exception ex)
                        {
                        _logger.LogError(ex, "Error in sales view mode");
                        ShowEmptySalesState("Connection error", "Unable to connect to data source");

                        var result = MessageBox.Show("Unable to load sales data from the server.\n\nWould you like to try again?",
                            "Connection Issue", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        
                        if (result == DialogResult.Yes)
                            {
                            BtnGenerateSales_Click(sender, e);
                            return;
                            }
                        }
                    }
                else
                    {
                    try
                        {
                        // Generate and export file (PDF, Excel, or CSV)
                        var exportResponse = await _apiService.GenerateSalesReportAsync(dtpSalesStart.Value, dtpSalesEnd.Value, cboSalesFormat.Text);

                        if (exportResponse.Success && exportResponse.Data != null)
                            {
                            // Determine file extension
                            var extension = cboSalesFormat.Text.ToLower() switch
                                {
                                    "pdf" => ".pdf",
                                    "excel" => ".xlsx",
                                    "csv" => ".csv",
                                    _ => ".txt"
                                    };
                            var fileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmm}{extension}";
                            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            var filePath = Path.Combine(documentsPath, fileName);

                            // Save file
                            await File.WriteAllBytesAsync(filePath, exportResponse.Data);

                            var result = MessageBox.Show(
                                $"Sales report exported successfully!\n\nFile: {fileName}\nLocation: Documents folder\n\nWould you like to open the file?",
                                "Export Complete",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);

                            if (result == DialogResult.Yes)
                                {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath)
                                    {
                                    UseShellExecute = true
                                    });
                                }
                            }
                        else
                            {
                            var errorMessage = exportResponse.Message ?? "Unknown error occurred";

                            // Check if this is a PDF-specific error and offer alternative
                            if (cboSalesFormat.Text.ToLower() == "pdf" &&
                                (errorMessage.Contains("BouncyCastle") || errorMessage.Contains("iText")))
                                {
                                var result = MessageBox.Show(
                                    "PDF generation is currently experiencing technical issues.\n\n" +
                                    "Would you like to export as Excel instead?",
                                    "PDF Export Issue",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);

                                if (result == DialogResult.Yes)
                                    {
                                    // Retry with Excel format
                                    var excelResponse = await _apiService.GenerateSalesReportAsync(dtpSalesStart.Value, dtpSalesEnd.Value, "Excel");
                                    if (excelResponse.Success && excelResponse.Data != null)
                                        {
                                        var fileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                                        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                                        var filePath = Path.Combine(documentsPath, fileName);

                                        await File.WriteAllBytesAsync(filePath, excelResponse.Data);

                                        MessageBox.Show(
                                            $"Sales report exported as Excel!\n\nFile: {fileName}\nLocation: Documents folder",
                                            "Export Complete",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                                        }
                                    else
                                        {
                                        MessageBox.Show("Excel export also failed. Please try again later or contact support.",
                                            "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            else
                                {
                                MessageBox.Show($"Failed to export sales report: {errorMessage}",
                                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    catch (HttpRequestException httpEx)
                        {
                        _logger.LogError(httpEx, "Network error during sales report export");
                        MessageBox.Show(
                            "Network connection error. Please check your connection and try again.",
                            "Connection Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        }
                    catch (TaskCanceledException tcEx)
                        {
                        _logger.LogError(tcEx, "Request timeout during sales report export");
                        MessageBox.Show(
                            "Request timed out. The server may be busy. Please try again.",
                            "Timeout Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        }
                    catch (Exception ex)
                        {
                        _logger.LogError(ex, "Unexpected error during sales report export");
                        MessageBox.Show(
                            "An unexpected error occurred during export. Please try again or contact support.",
                            "Export Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        }
                    }
                }
            catch (OperationCanceledException)
                {
                _logger.LogInformation("Sales report generation was cancelled");
                ShowEmptySalesState("Operation Cancelled", "Sales report generation was cancelled by user");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unexpected error during sales report generation");
                ShowEmptySalesState("System Error", "An unexpected error occurred during report generation");
                MessageBox.Show("An unexpected error occurred while generating the sales report. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            finally
                {
                HideSalesLoadingState();
                }
            }

        /// <summary>
        /// Processes sales view request with proper error handling and cancellation support
        /// </summary>
        private async Task ProcessSalesViewRequestAsync(CancellationToken cancellationToken)
            {
            var startTime = DateTime.UtcNow;
            try
                {
                UpdateSalesLoadingStatus("Connecting to server...");
                
                // Add timeout to the API call
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
                
                _logger.LogInformation("Starting sales data retrieval for period {StartDate} to {EndDate}", 
                    dtpSalesStart.Value, dtpSalesEnd.Value);

                UpdateSalesLoadingStatus("Retrieving sales data...");
                
                // Get sales data for viewing with timeout
                var dataResponse = await _apiService.GetSalesReportDataAsync(dtpSalesStart.Value, dtpSalesEnd.Value);
                
                cancellationToken.ThrowIfCancellationRequested();

                var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogInformation("Sales data retrieval completed in {ElapsedMs}ms. Success: {Success}", 
                    elapsedMs, dataResponse.Success);

                if (dataResponse.Success && dataResponse.Data != null)
                    {
                    UpdateSalesLoadingStatus("Processing sales data...");
                    await ProcessSalesDataAsync(dataResponse.Data, cancellationToken);
                    }
                else
                    {
                    var errorMessage = dataResponse.Message ?? "Unknown error occurred";
                    _logger.LogWarning("Failed to load sales data from API: {Message}", errorMessage);
                    
                    // Provide detailed diagnostics
                    await DiagnoseSalesDataIssueAsync(errorMessage, cancellationToken);
                    }
                }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                _logger.LogInformation("Sales data processing was cancelled by user");
                throw;
                }
            catch (OperationCanceledException)
                {
                _logger.LogWarning("Sales data retrieval timed out after 30 seconds");
                await Task.Run(() =>
                    {
                    ShowEmptySalesState("Request Timeout", "The server took too long to respond. Please try again.");
                    }, CancellationToken.None);
                    
                MessageBox.Show(
                    "The request timed out. This could be due to:\\n• Large amount of data being processed\\n• Server performance issues\\n• Network latency\\n\\nTry selecting a smaller date range or try again later.",
                    "Request Timeout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                }
            catch (Exception ex)
                {
                var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "Error in sales view processing after {ElapsedMs}ms", elapsedMs);
                
                await Task.Run(() =>
                    {
                    ShowEmptySalesState("Connection Error", "Unable to connect to data source");
                    }, CancellationToken.None);

                MessageBox.Show(
                    "Unable to load sales data from the server.\\n\\nThis could be due to:\\n• Network connection issues\\n• Server maintenance\\n• Database connectivity problems\\n\\nPlease check your connection and try again.",
                    "Connection Issue", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                }
            }

        /// <summary>
        /// Processes and displays sales data with performance optimization
        /// </summary>
        private async Task ProcessSalesDataAsync(object responseData, CancellationToken cancellationToken)
            {
            await Task.Run(() =>
                {
                try
                    {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    // Log raw response data for debugging
                    _logger.LogInformation("Raw API Response Type: {Type}, Data: {Data}", 
                        responseData.GetType().Name, 
                        responseData.ToString()?.Substring(0, Math.Min(500, responseData.ToString()?.Length ?? 0)));
                    
                    // Try to convert to JSON for better parsing
                    var jsonData = responseData;
                    if (responseData is string jsonString)
                        {
                        try
                            {
                            jsonData = JsonSerializer.Deserialize<object>(jsonString);
                            }
                        catch (Exception ex)
                            {
                            _logger.LogWarning(ex, "Failed to parse JSON string, using as-is");
                            }
                        }
                    
                    // Safely extract data using improved property extraction
                    var salesData = ExtractSalesDataRobustly(jsonData);
                    
                    _logger.LogInformation("Processed sales data: TotalSales={TotalSales}, TotalOrders={TotalOrders}, AvgOrder={AvgOrder}, DailyPointsCount={DailyCount}", 
                        salesData.TotalSales, salesData.TotalOrders, salesData.AverageOrderValue, salesData.DailySalesCount);

                    cancellationToken.ThrowIfCancellationRequested();

                    // Update UI on main thread
                    Invoke(new Action(() =>
                        {
                        try
                            {
                            UpdateSalesUI(salesData);
                            _logger.LogInformation("Sales data UI update completed successfully");
                            }
                        catch (Exception ex)
                            {
                            _logger.LogError(ex, "Error updating sales UI");
                            throw;
                            }
                        }));
                    }
                catch (OperationCanceledException)
                    {
                    throw;
                    }
                catch (Exception ex)
                    {
                    _logger.LogError(ex, "Error processing sales data");
                    throw;
                    }
                }, cancellationToken);
            }

        /// <summary>
        /// Processes daily sales data for chart display
        /// </summary>
        private void ProcessDailySalesChart(object? dailySales, decimal totalSales, int totalOrders)
            {
            var pointsAdded = 0;
            
            if (dailySales != null)
                {
                if (dailySales is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                    foreach (var item in jsonElement.EnumerateArray())
                        {
                        if (item.TryGetProperty("Date", out var dateProperty) &&
                            item.TryGetProperty("TotalAmount", out var amountProperty))
                            {
                            if (DateTime.TryParse(dateProperty.GetString(), out var date) &&
                                amountProperty.TryGetDecimal(out var amount))
                                {
                                chartSales.Series[0].Points.AddXY(date.ToShortDateString(), amount);
                                pointsAdded++;
                                }
                            }
                        }
                    }
                else if (dailySales is IEnumerable<object> enumerable)
                    {
                    foreach (var item in enumerable)
                        {
                        var date = GetPropertyValue<DateTime>(item, "Date", DateTime.MinValue);
                        var amount = GetPropertyValue<decimal>(item, "TotalAmount", 0.0m);
                        if (date != DateTime.MinValue)
                            {
                            chartSales.Series[0].Points.AddXY(date.ToShortDateString(), amount);
                            pointsAdded++;
                            }
                        }
                    }
                }

            // Handle empty chart data intelligently
            if (pointsAdded == 0)
                {
                if (totalSales > 0 && totalOrders > 0)
                    {
                    // Create single aggregate point if we have totals but no daily breakdown
                    var avgDaily = totalSales / Math.Max(1, (dtpSalesEnd.Value - dtpSalesStart.Value).Days + 1);
                    chartSales.Series[0].Points.AddXY("Period Average", avgDaily);
                    chartSales.ChartAreas[0].AxisX.Title = "Period";
                    _logger.LogInformation("Created aggregate chart point for period average: ${AvgDaily:N2}", avgDaily);
                    }
                else
                    {
                    // Show empty state message
                    chartSales.Titles.Add(new Title("No sales data available for selected period", 
                        Docking.Top, new Font("Segoe UI", 12), Color.FromArgb(127, 140, 141)));
                    _logger.LogInformation("No sales data found for selected period");
                    }
                }
            else
                {
                _logger.LogInformation("Added {PointsAdded} data points to sales chart", pointsAdded);
                }
            }

        /// <summary>
        /// Processes sales data for grid display
        /// </summary>
        private void ProcessSalesDataGrid(object? dailySales)
            {
            if (dailySales != null)
                {
                try
                    {
                    dgvSalesData.DataSource = dailySales;
                    _logger.LogInformation("Sales data grid updated with live data");
                    }
                catch (Exception ex)
                    {
                    _logger.LogWarning(ex, "Failed to bind daily sales to grid, showing empty data");
                    dgvSalesData.DataSource = new List<object>
                        {
                        new { Date = "No data available", TotalAmount = 0.0m, OrderCount = 0, Status = "Empty" }
                        };
                    }
                }
            else
                {
                dgvSalesData.DataSource = new List<object>
                    {
                    new { Date = "No data available", TotalAmount = 0.0m, OrderCount = 0, Status = "Empty" }
                    };
                _logger.LogInformation("No daily sales data available, showing empty grid");
                }
            }

        private async void BtnGenerateInventory_Click(object? sender, EventArgs e)
            {
            try
                {
                btnGenerateInventory.Enabled = false;
                btnGenerateInventory.Text = "Generating...";

                if (cboInventoryFormat.Text == "View")
                    {
                    try
                        {
                        // Get inventory data for viewing
                        var dataResponse = await _apiService.GetInventoryReportDataAsync();

                        if (dataResponse.Success && dataResponse.Data != null)
                            {
                            try
                                {
                                // Handle different response types more robustly
                                var responseData = dataResponse.Data;
                                var inventoryData = responseData as dynamic;

                                // Safely extract data using reflection
                                var totalProducts = GetPropertyValue<int>(responseData, "TotalProducts", 0);
                                var lowStockProducts = GetPropertyValue<int>(responseData, "LowStockProducts", 0);
                                var totalValue = GetPropertyValue<decimal>(responseData, "TotalInventoryValue", 0.0m);
                                var inventoryByCategory = GetPropertyValue<object?>(responseData, "InventoryByCategory", null);
                                var productDetails = GetPropertyValue<object?>(responseData, "ProductInventoryDetails", null);

                                // Update summary labels with real data
                                lblTotalProducts.Text = $"Total Products: {totalProducts}";
                                lblLowStockCount.Text = $"Low Stock: {lowStockProducts}";
                                lblInventoryValue.Text = $"Total Value: ${totalValue:N2}";

                                // Update chart with real data
                                chartInventory.Series[0].Points.Clear();

                                if (inventoryByCategory != null)
                                    {
                                    if (inventoryByCategory is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                                        {
                                        foreach (var property in jsonElement.EnumerateObject())
                                            {
                                            if (property.Value.TryGetInt32(out int value))
                                                {
                                                chartInventory.Series[0].Points.AddXY(property.Name, value);
                                                }
                                            }
                                        }
                                    else if (inventoryByCategory is IDictionary<string, object> dict)
                                        {
                                        foreach (var kvp in dict)
                                            {
                                            if (int.TryParse(kvp.Value?.ToString(), out int value))
                                                {
                                                chartInventory.Series[0].Points.AddXY(kvp.Key, value);
                                                }
                                            }
                                        }
                                    }

                                // If no chart data, show empty state or aggregate view
                                if (chartInventory.Series[0].Points.Count == 0)
                                    {
                                    if (totalProducts > 0)
                                        {
                                        // Show generic distribution if we have totals but no category breakdown
                                        chartInventory.Series[0].Points.AddXY("All Products", totalProducts);
                                        }
                                    else
                                        {
                                        // Show empty state
                                        chartInventory.Titles.Clear();
                                        chartInventory.Titles.Add(new Title("No inventory data available", 
                                            Docking.Top, new Font("Segoe UI", 12), Color.FromArgb(127, 140, 141)));
                                        }
                                    }
                                else
                                    {
                                    // Apply modern color palette to inventory chart
                                    ApplyInventoryChartColors();
                                    }

                                // Update grid with real data
                                if (productDetails != null)
                                    {
                                    try
                                        {
                                        dgvInventoryData.DataSource = productDetails;
                                        }
                                    catch (Exception ex)
                                        {
                                        _logger.LogWarning(ex, "Failed to bind product details to grid, using fallback data");
                                        dgvInventoryData.DataSource = GetFallbackInventoryData();
                                        }
                                    }
                                else
                                    {
                                    dgvInventoryData.DataSource = GetFallbackInventoryData();
                                    }
                                }
                            catch (Exception parseEx)
                                {
                                _logger.LogError(parseEx, "Error parsing inventory data response");
                                LoadFallbackInventoryData();
                                }
                            }
                        else
                            {
                            _logger.LogWarning("Failed to load inventory data from API: {Message}", dataResponse.Message);
                            ShowEmptyInventoryState("Unable to load live inventory data", dataResponse.Message);

                            var result = MessageBox.Show($"Unable to load live inventory data.\n\nReason: {dataResponse.Message ?? "Unknown error"}\n\nWould you like to try refreshing the data?",
                                "Data Loading Error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            
                            if (result == DialogResult.Yes)
                                {
                                BtnGenerateInventory_Click(sender, e);
                                return;
                                }
                            }
                        }
                    catch (Exception ex)
                        {
                        _logger.LogError(ex, "Error in inventory view mode");
                        ShowEmptyInventoryState("Connection error", "Unable to connect to data source");

                        var result = MessageBox.Show("Unable to load inventory data from the server.\n\nWould you like to try again?",
                            "Connection Issue", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        
                        if (result == DialogResult.Yes)
                            {
                            BtnGenerateInventory_Click(sender, e);
                            return;
                            }
                        }
                    }
                else
                    {
                    try
                        {
                        // Generate and export file (PDF or Excel)
                        var exportResponse = await _apiService.GenerateInventoryReportAsync(cboInventoryFormat.Text);

                        if (exportResponse.Success && exportResponse.Data != null)
                            {
                            // Determine file extension
                            var extension = cboInventoryFormat.Text.ToLower() == "pdf" ? ".pdf" : ".xlsx";
                            var fileName = $"InventoryReport_{DateTime.Now:yyyyMMdd_HHmm}{extension}";
                            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            var filePath = Path.Combine(documentsPath, fileName);

                            // Save file
                            await File.WriteAllBytesAsync(filePath, exportResponse.Data);

                            var result = MessageBox.Show(
                                $"Inventory report exported successfully!\n\nFile: {fileName}\nLocation: Documents folder\n\nWould you like to open the file?",
                                "Export Complete",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);

                            if (result == DialogResult.Yes)
                                {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath)
                                    {
                                    UseShellExecute = true
                                    });
                                }
                            }
                        else
                            {
                            var errorMessage = exportResponse.Message ?? "Unknown error occurred";

                            // Check if this is a PDF-specific error and offer alternative
                            if (cboInventoryFormat.Text.ToLower() == "pdf" &&
                                (errorMessage.Contains("BouncyCastle") || errorMessage.Contains("iText")))
                                {
                                var result = MessageBox.Show(
                                    "PDF generation is currently experiencing technical issues.\n\n" +
                                    "Would you like to export as Excel instead?",
                                    "PDF Export Issue",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);

                                if (result == DialogResult.Yes)
                                    {
                                    // Retry with Excel format
                                    var excelResponse = await _apiService.GenerateInventoryReportAsync("Excel");
                                    if (excelResponse.Success && excelResponse.Data != null)
                                        {
                                        var fileName = $"InventoryReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                                        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                                        var filePath = Path.Combine(documentsPath, fileName);

                                        await File.WriteAllBytesAsync(filePath, excelResponse.Data);

                                        MessageBox.Show(
                                            $"Inventory report exported as Excel!\n\nFile: {fileName}\nLocation: Documents folder",
                                            "Export Complete",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                                        }
                                    else
                                        {
                                        MessageBox.Show("Excel export also failed. Please try again later or contact support.",
                                            "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            else
                                {
                                MessageBox.Show($"Failed to export inventory report: {errorMessage}",
                                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    catch (HttpRequestException httpEx)
                        {
                        _logger.LogError(httpEx, "Network error during inventory report export");
                        MessageBox.Show(
                            "Network connection error. Please check your connection and try again.",
                            "Connection Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        }
                    catch (TaskCanceledException tcEx)
                        {
                        _logger.LogError(tcEx, "Request timeout during inventory report export");
                        MessageBox.Show(
                            "Request timed out. The server may be busy. Please try again.",
                            "Timeout Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        }
                    catch (Exception ex)
                        {
                        _logger.LogError(ex, "Unexpected error during inventory report export");
                        MessageBox.Show(
                            "An unexpected error occurred during export. Please try again or contact support.",
                            "Export Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        }
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error generating inventory report");
                MessageBox.Show("An unexpected error occurred while generating the inventory report. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            finally
                {
                btnGenerateInventory.Enabled = true;
                btnGenerateInventory.Text = "Generate Report";
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
            try
                {
                // Clear existing data
                chartFinancial.Series[0].Points.Clear();
                
                // Try to extract financial data from the API response
                var monthlyRevenue = GetPropertyValue<object?>(reportData, "MonthlyRevenue", null);
                var totalRevenue = GetPropertyValue<decimal>(reportData, "TotalRevenue", 0.0m);
                var totalExpenses = GetPropertyValue<decimal>(reportData, "TotalExpenses", 0.0m);
                var netProfit = GetPropertyValue<decimal>(reportData, "NetProfit", 0.0m);
                
                bool hasRealData = false;
                
                // Process monthly revenue data if available
                if (monthlyRevenue != null)
                    {
                    if (monthlyRevenue is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                        foreach (var item in jsonElement.EnumerateArray())
                            {
                            if (item.TryGetProperty("Month", out var monthProperty) &&
                                item.TryGetProperty("Revenue", out var revenueProperty))
                                {
                                var month = monthProperty.GetString();
                                if (revenueProperty.TryGetDecimal(out var revenue) && !string.IsNullOrEmpty(month))
                                    {
                                    chartFinancial.Series[0].Points.AddXY(month, revenue);
                                    hasRealData = true;
                                    }
                                }
                            }
                        }
                    else if (monthlyRevenue is IEnumerable<object> enumerable)
                        {
                        foreach (var item in enumerable)
                            {
                            var month = GetPropertyValue<string>(item, "Month", "");
                            var revenue = GetPropertyValue<decimal>(item, "Revenue", 0.0m);
                            if (!string.IsNullOrEmpty(month) && revenue > 0)
                                {
                                chartFinancial.Series[0].Points.AddXY(month, revenue);
                                hasRealData = true;
                                }
                            }
                        }
                    }
                
                // Create summary data for the grid
                var financialSummary = new List<object>();
                
                if (hasRealData)
                    {
                    // Use real data for grid
                    financialSummary.Add(new 
                        { 
                        Metric = "Total Revenue", 
                        Value = totalRevenue.ToString("C"), 
                        Status = "Actual Data" 
                        });
                    financialSummary.Add(new 
                        { 
                        Metric = "Total Expenses", 
                        Value = totalExpenses.ToString("C"), 
                        Status = "Actual Data" 
                        });
                    financialSummary.Add(new 
                        { 
                        Metric = "Net Profit", 
                        Value = netProfit.ToString("C"), 
                        Status = "Actual Data" 
                        });
                    }
                else
                    {
                    // Show empty state if no real data
                    chartFinancial.Titles.Clear();
                    chartFinancial.Titles.Add(new Title("No financial data available for selected year\\nPlease select a different year or check data availability", 
                        Docking.Top, new Font("Segoe UI", 11), Color.FromArgb(127, 140, 141)));
                    
                    financialSummary.Add(new 
                        { 
                        Metric = "No data available", 
                        Value = "$0.00", 
                        Status = "Empty" 
                        });
                    }
                
                // Update grid
                dgvFinancialData.DataSource = financialSummary;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error processing financial data, falling back to simulated data");
                // Fall back to simulated data only if parsing fails
                Task.Run(async () => await GenerateSimulatedFinancialData());
                }
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

        /// <summary>
        /// Safely extracts property values from dynamic objects
        /// </summary>
        private T GetPropertyValue<T>(object obj, string propertyName, T defaultValue)
            {
            try
                {
                if (obj == null) return defaultValue;

                // Handle JsonElement
                if (obj is System.Text.Json.JsonElement jsonElement)
                    {
                    if (jsonElement.TryGetProperty(propertyName, out var property))
                        {
                        if (typeof(T) == typeof(int) && property.TryGetInt32(out int intValue))
                            return (T)(object)intValue;
                        if (typeof(T) == typeof(decimal) && property.TryGetDecimal(out decimal decValue))
                            return (T)(object)decValue;
                        if (typeof(T) == typeof(string))
                            {
                            var stringValue = property.GetString();
                            return stringValue != null ? (T)(object)stringValue : defaultValue;
                            }

                        var deserializedValue = property.Deserialize<T>();
                        return deserializedValue != null ? deserializedValue : defaultValue;
                        }
                    }

                // Handle regular objects using reflection
                var propInfo = obj.GetType().GetProperty(propertyName);
                if (propInfo != null)
                    {
                    var value = propInfo.GetValue(obj);
                    if (value != null && value is T tValue)
                        return tValue;
                    if (value != null)
                        return (T)Convert.ChangeType(value, typeof(T));
                    }

                return defaultValue;
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Failed to extract property {PropertyName} from object", propertyName);
                return defaultValue;
                }
            }

        /// <summary>
        /// Loads fallback inventory data when API fails
        /// </summary>
        private void LoadFallbackInventoryData()
            {
            // Update summary labels with sample data
            lblTotalProducts.Text = "Total Products: 150 (Sample)";
            lblLowStockCount.Text = "Low Stock: 12 (Sample)";
            lblInventoryValue.Text = "Total Value: $234,567.89 (Sample)";

            // Update chart with sample data
            chartInventory.Series[0].Points.Clear();
            chartInventory.Series[0].Points.AddXY("Electronics", 45);
            chartInventory.Series[0].Points.AddXY("Clothing", 35);
            chartInventory.Series[0].Points.AddXY("Books", 30);
            chartInventory.Series[0].Points.AddXY("Home & Garden", 25);
            chartInventory.Series[0].Points.AddXY("Sports", 15);

            // Update grid with sample data
            dgvInventoryData.DataSource = GetFallbackInventoryData();
            }

        /// <summary>
        /// Gets fallback inventory data for display when API is unavailable
        /// </summary>
        private List<object> GetFallbackInventoryData()
            {
            return new List<object>
            {
                new { ProductName = "Laptop Pro 15", SKU = "LAP-001", Category = "Electronics", CurrentStock = 5, MinStock = 10, UnitPrice = 1299.99m, StockValue = 6499.95m, Status = "Low Stock" },
                new { ProductName = "Wireless Mouse", SKU = "MOU-001", Category = "Electronics", CurrentStock = 0, MinStock = 20, UnitPrice = 29.99m, StockValue = 0.00m, Status = "Out of Stock" },
                new { ProductName = "USB-C Cable", SKU = "CAB-001", Category = "Electronics", CurrentStock = 25, MinStock = 15, UnitPrice = 19.99m, StockValue = 499.75m, Status = "In Stock" },
                new { ProductName = "T-Shirt Basic", SKU = "TSH-001", Category = "Clothing", CurrentStock = 8, MinStock = 20, UnitPrice = 24.99m, StockValue = 199.92m, Status = "Low Stock" },
                new { ProductName = "Jeans Regular", SKU = "JEA-001", Category = "Clothing", CurrentStock = 15, MinStock = 10, UnitPrice = 59.99m, StockValue = 899.85m, Status = "In Stock" },
                new { ProductName = "Programming Book", SKU = "BOO-001", Category = "Books", CurrentStock = 12, MinStock = 5, UnitPrice = 49.99m, StockValue = 599.88m, Status = "In Stock" },
                new { ProductName = "Garden Hose", SKU = "GAR-001", Category = "Home & Garden", CurrentStock = 3, MinStock = 8, UnitPrice = 39.99m, StockValue = 119.97m, Status = "Low Stock" },
                new { ProductName = "Tennis Racket", SKU = "TEN-001", Category = "Sports", CurrentStock = 18, MinStock = 5, UnitPrice = 89.99m, StockValue = 1619.82m, Status = "In Stock" }
            };
            }

        /// <summary>\n        /// Loads fallback sales data when API fails\n        /// </summary>\n       
        private void LoadFallbackSalesData()
            {
            lblSalesTotalValue.Text = "Total Sales: $125,432.67 (Sample)";
            lblSalesOrderCount.Text = "Orders: 342 (Sample)";
            lblSalesAvgOrder.Text = "Avg Order: $366.86 (Sample)";
            chartSales.Series[0].Points.Clear();
            var random = new Random();
            var startDate = dtpSalesStart.Value;
            var endDate = dtpSalesEnd.Value;
            for (var date = startDate.Date; date <= endDate.Date && date <= DateTime.Now.Date; date = date.AddDays(1))
                {
                var amount = random.Next(2000, 8000) + (decimal)random.NextDouble() * 1000;
                chartSales.Series[0].Points.AddXY(date.ToShortDateString(), amount);
                }
            ;
            dgvSalesData.DataSource = new List<object>
                {
                new { Date = "No data available", TotalAmount = 0.0m, OrderCount = 0, Status = "Empty" }
                };
            }

        /// <summary>
        /// Shows empty state for sales reports when no data is available
        /// </summary>
        private void ShowEmptySalesState(string title, string? message = null)
            {
            // Clear existing data
            chartSales.Series[0].Points.Clear();
            chartSales.Titles.Clear();
            
            // Set labels to show empty state
            lblSalesTotalValue.Text = "Total Sales: No data available";
            lblSalesOrderCount.Text = "Orders: No data available";
            lblSalesAvgOrder.Text = "Avg Order: No data available";
            
            // Add title to chart
            chartSales.Titles.Add(new Title(title + "\\n" + (message ?? "Please check your connection and try again"), 
                Docking.Top, new Font("Segoe UI", 11), Color.FromArgb(127, 140, 141)));
            
            // Clear grid
            dgvSalesData.DataSource = new List<object>
                {
                new { Date = "No data", TotalAmount = 0.0m, OrderCount = 0, Status = "Empty" }
                };
            }

        /// <summary>
        /// Shows empty state for inventory reports when no data is available
        /// </summary>
        private void ShowEmptyInventoryState(string title, string? message = null)
            {
            // Clear existing data
            chartInventory.Series[0].Points.Clear();
            chartInventory.Titles.Clear();
            
            // Set labels to show empty state
            lblTotalProducts.Text = "Total Products: No data available";
            lblLowStockCount.Text = "Low Stock: No data available";
            lblInventoryValue.Text = "Total Value: No data available";
            
            // Add title to chart
            chartInventory.Titles.Add(new Title(title + "\\n" + (message ?? "Please check your connection and try again"), 
                Docking.Top, new Font("Segoe UI", 11), Color.FromArgb(127, 140, 141)));
            
            // Clear grid
            dgvInventoryData.DataSource = new List<object>
                {
                new { ProductName = "No data available", SKU = "", Category = "", CurrentStock = 0, Status = "Empty" }
                };
            }

        /// <summary>
        /// Applies modern color palette to inventory pie chart
        /// </summary>
        private void ApplyInventoryChartColors()
            {
            var modernColors = new[]
                {
                Color.FromArgb(52, 152, 219),   // Blue
                Color.FromArgb(46, 204, 113),   // Green
                Color.FromArgb(155, 89, 182),   // Purple
                Color.FromArgb(230, 126, 34),   // Orange
                Color.FromArgb(231, 76, 60),    // Red
                Color.FromArgb(241, 196, 15),   // Yellow
                Color.FromArgb(52, 73, 94),     // Dark Blue
                Color.FromArgb(149, 165, 166)   // Gray
                };

            for (int i = 0; i < chartInventory.Series[0].Points.Count && i < modernColors.Length; i++)
                {
                chartInventory.Series[0].Points[i].Color = modernColors[i];
                chartInventory.Series[0].Points[i].BorderColor = Color.White;
                chartInventory.Series[0].Points[i].BorderWidth = 2;
                }
            }

        /// <summary>
        /// Shows loading indicators and disables controls during data fetch
        /// </summary>
        private void ShowSalesLoadingState(string message = "Loading sales data...")
            {
            if (InvokeRequired)
                {
                Invoke(new Action(() => ShowSalesLoadingState(message)));
                return;
                }

            lock (_loadingLock)
                {
                _isLoadingSalesData = true;
                btnGenerateSales.Enabled = false;
                btnGenerateSales.Text = "Loading...";
                pbSalesLoading.Visible = true;
                lblSalesLoadingStatus.Text = message;
                lblSalesLoadingStatus.Visible = true;
                }
            }

        /// <summary>
        /// Hides loading indicators and re-enables controls
        /// </summary>
        private void HideSalesLoadingState()
            {
            if (InvokeRequired)
                {
                Invoke(new Action(HideSalesLoadingState));
                return;
                }

            lock (_loadingLock)
                {
                _isLoadingSalesData = false;
                btnGenerateSales.Enabled = true;
                btnGenerateSales.Text = "Generate Report";
                pbSalesLoading.Visible = false;
                lblSalesLoadingStatus.Visible = false;
                }
            }

        /// <summary>
        /// Updates loading status message
        /// </summary>
        private void UpdateSalesLoadingStatus(string message)
            {
            if (InvokeRequired)
                {
                Invoke(new Action(() => UpdateSalesLoadingStatus(message)));
                return;
                }

            if (_isLoadingSalesData)
                {
                lblSalesLoadingStatus.Text = message;
                }
            }

        /// <summary>
        /// Robustly extracts sales data from various JSON response formats
        /// </summary>
        private SalesDataExtracted ExtractSalesDataRobustly(object jsonData)
        {
            try
            {
                if (jsonData == null)
                {
                    _logger.LogWarning("Received null data, returning empty sales data");
                    return new SalesDataExtracted(0m, 0, 0m, new List<DailySalesPoint>(), 0);
                }

                // Handle JsonElement from System.Text.Json
                if (jsonData is JsonElement jsonElement)
                {
                    return ExtractFromJsonElement(jsonElement);
                }

                // Handle Dictionary or dynamic object
                if (jsonData is IDictionary<string, object> dict)
                {
                    return ExtractFromDictionary(dict);
                }

                // Try to parse as JSON string if it's a string
                if (jsonData is string jsonString && !string.IsNullOrWhiteSpace(jsonString))
                {
                    var parsed = JsonSerializer.Deserialize<JsonElement>(jsonString);
                    return ExtractFromJsonElement(parsed);
                }

                _logger.LogWarning("Unknown data format: {Type}", jsonData.GetType().Name);
                return new SalesDataExtracted(0m, 0, 0m, new List<DailySalesPoint>(), 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting sales data");
                return new SalesDataExtracted(0m, 0, 0m, new List<DailySalesPoint>(), 0);
            }
        }

        private SalesDataExtracted ExtractFromJsonElement(JsonElement jsonElement)
        {
            var totalSales = GetDecimalProperty(jsonElement, "TotalSales", "totalSales");
            var totalOrders = GetIntProperty(jsonElement, "TotalOrders", "totalOrders");
            var avgOrder = totalOrders > 0 ? totalSales / totalOrders : 0m;

            var dailySales = new List<DailySalesPoint>();

            // Try to extract daily sales data
            if (jsonElement.TryGetProperty("DailySales", out var dailySalesElement) ||
                jsonElement.TryGetProperty("dailySales", out dailySalesElement))
            {
                if (dailySalesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in dailySalesElement.EnumerateArray())
                    {
                        try
                        {
                            var date = GetDateProperty(item, "Date", "date");
                            var amount = GetDecimalProperty(item, "Amount", "amount", "Sales", "sales");
                            var orders = GetIntProperty(item, "OrderCount", "orderCount", "Orders", "orders");

                            if (date != default && amount > 0)
                            {
                                dailySales.Add(new DailySalesPoint(date, amount, orders));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error parsing daily sales item");
                        }
                    }
                }
            }

            return new SalesDataExtracted(totalSales, totalOrders, avgOrder, dailySales, dailySales.Count);
        }

        private SalesDataExtracted ExtractFromDictionary(IDictionary<string, object> dict)
        {
            var totalSales = GetDictionaryDecimal(dict, "TotalSales", "totalSales");
            var totalOrders = GetDictionaryInt(dict, "TotalOrders", "totalOrders");
            var avgOrder = totalOrders > 0 ? totalSales / totalOrders : 0m;

            var dailySales = new List<DailySalesPoint>();

            if (dict.TryGetValue("DailySales", out var dailySalesObj) ||
                dict.TryGetValue("dailySales", out dailySalesObj))
            {
                if (dailySalesObj is IEnumerable<object> dailyArray)
                {
                    foreach (var item in dailyArray)
                    {
                        if (item is IDictionary<string, object> dailyDict)
                        {
                            try
                            {
                                var date = GetDictionaryDate(dailyDict, "Date", "date");
                                var amount = GetDictionaryDecimal(dailyDict, "Amount", "amount", "Sales", "sales");
                                var orders = GetDictionaryInt(dailyDict, "OrderCount", "orderCount", "Orders", "orders");

                                if (date != default && amount > 0)
                                {
                                    dailySales.Add(new DailySalesPoint(date, amount, orders));
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error parsing daily sales dictionary item");
                            }
                        }
                    }
                }
            }

            return new SalesDataExtracted(totalSales, totalOrders, avgOrder, dailySales, dailySales.Count);
        }

        private decimal GetDecimalProperty(JsonElement element, params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var value))
                        return value;
                    if (prop.ValueKind == JsonValueKind.String && decimal.TryParse(prop.GetString(), out var stringValue))
                        return stringValue;
                }
            }
            return 0m;
        }

        private int GetIntProperty(JsonElement element, params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var value))
                        return value;
                    if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var stringValue))
                        return stringValue;
                }
            }
            return 0;
        }

        private DateTime GetDateProperty(JsonElement element, params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.String && DateTime.TryParse(prop.GetString(), out var value))
                        return value;
                }
            }
            return default;
        }

        private decimal GetDictionaryDecimal(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (dict.TryGetValue(key, out var value))
                {
                    if (value is decimal decValue) return decValue;
                    if (value is double doubleValue) return (decimal)doubleValue;
                    if (value is float floatValue) return (decimal)floatValue;
                    if (value is int intValue) return intValue;
                    if (decimal.TryParse(value?.ToString(), out var parsedValue))
                        return parsedValue;
                }
            }
            return 0m;
        }

        private int GetDictionaryInt(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (dict.TryGetValue(key, out var value))
                {
                    if (value is int intValue) return intValue;
                    if (int.TryParse(value?.ToString(), out var parsedValue))
                        return parsedValue;
                }
            }
            return 0;
        }

        private DateTime GetDictionaryDate(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (dict.TryGetValue(key, out var value))
                {
                    if (value is DateTime dateValue) return dateValue;
                    if (DateTime.TryParse(value?.ToString(), out var parsedValue))
                        return parsedValue;
                }
            }
            return default;
        }

        /// <summary>
        /// Updates all sales UI elements with the extracted data
        /// </summary>
        private void UpdateSalesUI(SalesDataExtracted salesData)
        {
            try
            {
                // Update summary labels
                lblSalesTotalValue.Text = $"Total Sales: {salesData.TotalSales:C}";
                lblSalesOrderCount.Text = $"Total Orders: {salesData.TotalOrders:N0}";
                lblSalesAvgOrder.Text = $"Avg Order: {salesData.AverageOrderValue:C}";

                // Update chart
                UpdateSalesChart(salesData.DailySales);

                // Update data grid
                UpdateSalesDataGrid(salesData.DailySales);

                _logger.LogInformation("Sales UI updated with {DailyCount} daily points", salesData.DailySalesCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sales UI");
                throw;
            }
        }

        private void UpdateSalesChart(List<DailySalesPoint> dailySales)
        {
            try
            {
                chartSales.Series.Clear();
                chartSales.ChartAreas.Clear();

                // Create chart area with enhanced styling
                var chartArea = new ChartArea("SalesArea")
                {
                    BackColor = Color.White,
                    BorderColor = Color.FromArgb(189, 195, 199),
                    BorderWidth = 1,
                    BorderDashStyle = ChartDashStyle.Solid
                };

                // Configure axes
                chartArea.AxisX.LineColor = Color.FromArgb(189, 195, 199);
                chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(236, 240, 241);
                chartArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 8);
                chartArea.AxisX.LabelStyle.ForeColor = Color.FromArgb(127, 140, 141);

                chartArea.AxisY.LineColor = Color.FromArgb(189, 195, 199);
                chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(236, 240, 241);
                chartArea.AxisY.LabelStyle.Font = new Font("Segoe UI", 8);
                chartArea.AxisY.LabelStyle.ForeColor = Color.FromArgb(127, 140, 141);
                chartArea.AxisY.LabelStyle.Format = "C0";

                chartSales.ChartAreas.Add(chartArea);

                // Create series with enhanced styling
                var series = new Series("Daily Sales")
                {
                    ChartType = SeriesChartType.Column,
                    Color = Color.FromArgb(41, 128, 185),
                    IsValueShownAsLabel = true,
                    LabelFormat = "C0",
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    BorderWidth = 2,
                    BorderColor = Color.FromArgb(31, 97, 141),
                    BackGradientStyle = GradientStyle.TopBottom,
                    BackSecondaryColor = Color.FromArgb(52, 152, 219),
                    LabelForeColor = Color.FromArgb(44, 62, 80)
                };

                // Add data points
                if (dailySales.Any())
                {
                    foreach (var point in dailySales.OrderBy(ds => ds.Date))
                    {
                        series.Points.AddXY(point.Date.ToString("MM/dd"), point.Amount);
                    }
                }
                else
                {
                    // Add sample point to show chart structure
                    series.Points.AddXY("No Data", 0);
                }

                chartSales.Series.Add(series);

                // Add title
                chartSales.Titles.Clear();
                chartSales.Titles.Add(new Title("Daily Sales Performance")
                {
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sales chart");
            }
        }

        private void UpdateSalesDataGrid(List<DailySalesPoint> dailySales)
        {
            try
            {
                dgvSalesData.DataSource = null;
                dgvSalesData.Columns.Clear();

                if (!dailySales.Any())
                {
                    // Show empty state
                    dgvSalesData.Columns.Add("Message", "Status");
                    dgvSalesData.Rows.Add("No sales data available for the selected date range");
                    return;
                }

                // Create data table
                var dataTable = new System.Data.DataTable();
                dataTable.Columns.Add("Date", typeof(string));
                dataTable.Columns.Add("Sales Amount", typeof(string));
                dataTable.Columns.Add("Order Count", typeof(int));
                dataTable.Columns.Add("Avg per Order", typeof(string));

                foreach (var point in dailySales.OrderBy(ds => ds.Date))
                {
                    var avgPerOrder = point.OrderCount > 0 ? point.Amount / point.OrderCount : 0m;
                    dataTable.Rows.Add(
                        point.Date.ToString("yyyy-MM-dd"),
                        point.Amount.ToString("C"),
                        point.OrderCount,
                        avgPerOrder.ToString("C")
                    );
                }

                dgvSalesData.DataSource = dataTable;

                // Style columns
                if (dgvSalesData.Columns.Count > 0)
                {
                    dgvSalesData.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvSalesData.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvSalesData.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvSalesData.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sales data grid");
            }
        }

        /// <summary>
        /// Diagnoses sales data issues and provides detailed feedback
        /// </summary>
        private async Task DiagnoseSalesDataIssueAsync(string errorMessage, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting sales data diagnosis");

                var diagnosisResults = new List<string>();
                diagnosisResults.Add($"Primary Error: {errorMessage}");

                // Test API connectivity by trying to get dashboard stats
                try
                {
                    var testResponse = await _apiService.GetDashboardStatsAsync();
                    if (testResponse.Success)
                    {
                        diagnosisResults.Add("✓ API Gateway is accessible");
                    }
                    else
                    {
                        var errorMsg = testResponse.Message ?? (testResponse.Errors.Any() ? string.Join(", ", testResponse.Errors) : "Unknown error");
                        diagnosisResults.Add($"✗ API Gateway issue: {errorMsg}");
                    }
                }
                catch (Exception ex)
                {
                    diagnosisResults.Add($"✗ API connectivity test failed: {ex.Message}");
                }

                // Test date range validity
                var dateRange = dtpSalesEnd.Value - dtpSalesStart.Value;
                if (dateRange.TotalDays <= 0)
                {
                    diagnosisResults.Add("✗ Invalid date range: End date must be after start date");
                }
                else if (dateRange.TotalDays > 365)
                {
                    diagnosisResults.Add("⚠ Large date range (>365 days) may cause performance issues");
                }
                else
                {
                    diagnosisResults.Add($"✓ Date range is valid ({dateRange.TotalDays:N0} days)");
                }

                // Update UI with diagnosis
                Invoke(new Action(() =>
                {
                    try
                    {
                        lblSalesLoadingStatus.Text = "Diagnosis complete - Check logs for details";
                        
                        // Show detailed diagnosis in a message box for debugging
                        var diagnosis = string.Join(Environment.NewLine, diagnosisResults);
                        _logger.LogWarning("Sales Data Diagnosis Results:\n{Diagnosis}", diagnosis);
                        
                        // Update summary with helpful message
                        lblSalesTotalValue.Text = "Total Sales: Unable to load data";
                        lblSalesOrderCount.Text = "Total Orders: Check connection";
                        lblSalesAvgOrder.Text = "Avg Order: Service unavailable";
                        
                        // Clear chart and show message
                        chartSales.Series.Clear();
                        chartSales.Titles.Clear();
                        chartSales.Titles.Add(new Title("Sales Data Unavailable")
                        {
                            Font = new Font("Segoe UI", 14, FontStyle.Bold),
                            ForeColor = Color.FromArgb(231, 76, 60)
                        });
                        
                        // Show error in data grid
                        dgvSalesData.DataSource = null;
                        dgvSalesData.Columns.Clear();
                        dgvSalesData.Columns.Add("Issue", "Diagnosis");
                        foreach (var result in diagnosisResults)
                        {
                            dgvSalesData.Rows.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating UI during diagnosis");
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sales data diagnosis");
            }
        }

        /// <summary>
        /// Provides user-friendly error messages based on HTTP status codes and error details
        /// </summary>
        private string GetUserFriendlyErrorMessage(string? originalMessage, int statusCode)
            {
            return statusCode switch
            {
                502 => "The reporting service is currently unavailable.\n\nThis typically means:\n• The report server is temporarily down\n• Network connectivity issues\n• Service is being updated\n\nPlease try again in a few moments.",
                503 => "The reporting service is temporarily overloaded.\n\nPlease wait a moment and try again.",
                504 => "The request timed out while loading sales data.\n\nThis may be due to:\n• Large amount of data being processed\n• Slow network connection\n• Server overload\n\nTry selecting a smaller date range or try again later.",
                404 => "The requested sales data endpoint was not found.\n\nPlease contact support if this issue persists.",
                401 => "Authentication failed. Please log in again.",
                403 => "You don't have permission to access sales reports.\n\nPlease contact your administrator.",
                500 => "An internal server error occurred while processing your request.\n\nPlease try again or contact support if the issue persists.",
                _ => $"Unable to load sales data.\n\nError: {originalMessage ?? "Unknown error"}\n\nPlease check your connection and try again."
            };
            }

        /// <summary>
        /// Clean up resources and cancel any pending operations
        /// </summary>
        protected override void Dispose(bool disposing)
            {
            if (disposing)
                {
                _salesReportCancellationTokenSource?.Cancel();
                _salesReportCancellationTokenSource?.Dispose();
                }
            base.Dispose(disposing);
            }
        #endregion

        }
    }


