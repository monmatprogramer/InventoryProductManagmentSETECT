using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using CsvHelper;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using Color = System.Drawing.Color;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Modern Professional Sales History Form with Details Panel
    /// Shows "Product name was sold to Customer Name, QTY, Price, Date" with modern UI and details view
    /// </summary>
    public partial class SalesHistoryForm : Form
    {
        private readonly ILogger<SalesHistoryForm> _logger;
        private readonly IApiService _apiService;

        // Main controls
        private DataGridView dgvSalesHistory = null!;
        private Panel pnlFilters = null!;
        private Panel pnlMain = null!;
        private Panel pnlHeader = null!;
        private Panel pnlDetails = null!;
        private Panel pnlGridContainer = null!;
        private Splitter splitterDetails = null!;
        private StatusStrip statusStrip = null!;

        // Filter controls
        private DateTimePicker dtpStartDate = null!;
        private DateTimePicker dtpEndDate = null!;
        private TextBox txtSearch = null!;
        private Button btnSearch = null!;
        private Button btnClear = null!;
        private Button btnRefresh = null!;
        private Button btnExport = null!;

        // Details panel controls
        private Label lblDetailsTitle = null!;
        private Label lblProductDetails = null!;
        private Label lblCustomerDetails = null!;
        private Label lblTransactionDetails = null!;
        private Label lblPaymentDetails = null!;
        private Button btnCloseDetails = null!;
        private Button btnPrintInvoice = null!;
        private Button btnPrintPreview = null!;

        // Status bar
        private ToolStripStatusLabel lblStatus = null!;
        private ToolStripStatusLabel lblCount = null!;
        private ToolStripStatusLabel lblTotal = null!;

        // Data
        private List<SaleItemDisplayDto> _salesData = new();
        private List<SaleItemDisplayDto> _filteredData = new();
        private SaleItemDisplayDto? _selectedItem;

        // Printing
        private PrintDocument _printDocument = null!;
        private PrintPreviewDialog _printPreviewDialog = null!;
        private PrintDialog _printDialog = null!;

        // Modern colors
        private readonly Color _primaryBlue = Color.FromArgb(59, 130, 246);
        private readonly Color _primaryBlueHover = Color.FromArgb(37, 99, 235);
        private readonly Color _backgroundGray = Color.FromArgb(248, 250, 252);
        private readonly Color _cardBackground = Color.White;
        private readonly Color _textPrimary = Color.FromArgb(31, 41, 55);
        private readonly Color _textSecondary = Color.FromArgb(107, 114, 128);
        private readonly Color _borderColor = Color.FromArgb(229, 231, 235);
        private readonly Color _successGreen = Color.FromArgb(16, 185, 129);
        private readonly Color _warningOrange = Color.FromArgb(245, 158, 11);
        private readonly Color _errorRed = Color.FromArgb(239, 68, 68);
        private readonly Color _accentPurple = Color.FromArgb(139, 92, 246);

        public SalesHistoryForm(ILogger<SalesHistoryForm> logger, IApiService apiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            InitializeComponent();
            InitializePrintComponents();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form setup
            this.Text = "💼 Sales History Analytics - Modern Dashboard";
            this.Size = new Size(1600, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _backgroundGray;
            this.Font = new Font("Segoe UI", 9F);
            this.Padding = new Padding(20);

            SetupHeader();
            SetupFilters();
            SetupMainPanel();
            SetupDetailsPanel();
            SetupDataGrid();
            SetupStatusBar();

            // Layout - Details panel on the left, main content on the right
            this.Controls.Add(pnlMain);
            this.Controls.Add(splitterDetails);
            this.Controls.Add(pnlDetails);
            this.Controls.Add(pnlFilters);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(statusStrip);

            // Event handlers
            this.Load += SalesHistoryForm_Load;

            this.ResumeLayout(false);
        }

        private void SetupHeader()
        {
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 10)
            };

            var lblTitle = new Label
            {
                Text = "📊 Sales History Analytics",
                Location = new Point(0, 0),
                Size = new Size(400, 40),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = _textPrimary,
                BackColor = Color.Transparent
            };

            var lblSubtitle = new Label
            {
                Text = "Track product sales to customers with detailed insights - Click any row for details",
                Location = new Point(0, 45),
                Size = new Size(600, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = _textSecondary,
                BackColor = Color.Transparent
            };

            // Modern action buttons on the right
            btnRefresh = CreateModernButton("🔄", "Refresh", _primaryBlue, new Point(1150, 20), new Size(100, 35));
            btnRefresh.Click += BtnRefresh_Click;

            btnExport = CreateModernButton("📤", "Export", _successGreen, new Point(1260, 20), new Size(100, 35));
            btnExport.Click += BtnExport_Click;

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, btnRefresh, btnExport });
        }

        private void SetupFilters()
        {
            pnlFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 10)
            };

            // Create rounded filter container
            var pnlFilterContainer = new Panel
            {
                Location = new Point(0, 10),
                Size = new Size(1560, 80),
                BackColor = _cardBackground
            };
            pnlFilterContainer.Paint += (s, e) => DrawRoundedPanel(e.Graphics, pnlFilterContainer.ClientRectangle, 20, _cardBackground, _borderColor);

            // Date range section
            var lblDateIcon = CreateModernLabel("📅", new Point(25, 25), new Size(30, 25), 12);
            var lblDateRange = CreateModernLabel("Date Range", new Point(58, 28), new Size(90, 20), 10, FontStyle.Bold);

            dtpStartDate = CreateModernDatePicker(new Point(148, 25), new Size(200, 30));
            dtpStartDate.Value = DateTime.Today.AddDays(-30);

            var lblTo = CreateModernLabel("→", new Point(370, 28), new Size(20, 20), 10);
            lblTo.TextAlign = ContentAlignment.MiddleCenter;
            lblTo.ForeColor = _textSecondary;

            dtpEndDate = CreateModernDatePicker(new Point(400, 25), new Size(200, 30));
            dtpEndDate.Value = DateTime.Today;

            // Search section
            var lblSearchIcon = CreateModernLabel("🔍", new Point(650, 25), new Size(30, 25), 12);
            var lblSearch = CreateModernLabel("Quick Search", new Point(685, 28), new Size(100, 20), 10, FontStyle.Bold);

            txtSearch = CreateModernTextBox(new Point(790, 25), new Size(300, 30), "Search products, customers...");

            // Action buttons
            btnSearch = CreateModernButton("🔍", "Search", _primaryBlue, new Point(1110, 25), new Size(100, 30));
            btnSearch.Click += BtnSearch_Click;

            btnClear = CreateModernButton("🧹", "Clear", _textSecondary, new Point(1220, 25), new Size(90, 30));
            btnClear.Click += BtnClear_Click;

            pnlFilterContainer.Controls.AddRange(new Control[] {
                lblDateIcon, lblDateRange, dtpStartDate, lblTo, dtpEndDate,
                lblSearchIcon, lblSearch, txtSearch, btnSearch, btnClear
            });

            pnlFilters.Controls.Add(pnlFilterContainer);
        }

        private void SetupDetailsPanel()
        {
            pnlDetails = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 10, 10),
                Visible = false
            };

            // Create rounded details container
            var pnlDetailsContainer = new Panel
            {
                Location = new Point(0, 10),
                Size = new Size(390, pnlDetails.Height - 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = _cardBackground
            };
            pnlDetailsContainer.Paint += (s, e) => DrawRoundedPanel(e.Graphics, pnlDetailsContainer.ClientRectangle, 20, _cardBackground, _borderColor);

            // Header with close button
            lblDetailsTitle = new Label
            {
                Text = "📋 Transaction Details",
                Location = new Point(25, 20),
                Size = new Size(280, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = _textPrimary,
                BackColor = Color.Transparent
            };

            btnCloseDetails = CreateModernButton("✕", "", _errorRed, new Point(320, 20), new Size(35, 30));
            btnCloseDetails.Click += BtnCloseDetails_Click;

            // Product details section
            var lblProductTitle = CreateSectionLabel("📦 Product Information", new Point(25, 70));
            lblProductDetails = CreateDetailsLabel(new Point(25, 100), new Size(340, 80));

            // Customer details section  
            var lblCustomerTitle = CreateSectionLabel("👤 Customer Information", new Point(25, 200));
            lblCustomerDetails = CreateDetailsLabel(new Point(25, 230), new Size(340, 60));

            // Transaction details section
            var lblTransactionTitle = CreateSectionLabel("🧾 Transaction Information", new Point(25, 310));
            lblTransactionDetails = CreateDetailsLabel(new Point(25, 340), new Size(340, 80));

            // Payment details section
            var lblPaymentTitle = CreateSectionLabel("💳 Payment Information", new Point(25, 440));
            lblPaymentDetails = CreateDetailsLabel(new Point(25, 470), new Size(340, 60));

            // Action buttons
            btnPrintPreview = CreateModernButton("👁️", "Print Preview", _primaryBlue, new Point(25, 550), new Size(165, 40));
            btnPrintPreview.Click += BtnPrintPreview_Click;

            btnPrintInvoice = CreateModernButton("🖨️", "Print Invoice", _accentPurple, new Point(200, 550), new Size(165, 40));
            btnPrintInvoice.Click += BtnPrintInvoice_Click;

            pnlDetailsContainer.Controls.AddRange(new Control[] {
                lblDetailsTitle, btnCloseDetails,
                lblProductTitle, lblProductDetails,
                lblCustomerTitle, lblCustomerDetails,
                lblTransactionTitle, lblTransactionDetails,
                lblPaymentTitle, lblPaymentDetails,
                btnPrintPreview, btnPrintInvoice
            });

            pnlDetails.Controls.Add(pnlDetailsContainer);

            // Splitter
            splitterDetails = new Splitter
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = _borderColor,
                Visible = false
            };
        }

        private void SetupMainPanel()
        {
            pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 10, 0, 10)
            };

            // Grid container with rounded corners
            pnlGridContainer = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(pnlMain.Width - 20, pnlMain.Height - 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = _cardBackground
            };
            pnlGridContainer.Paint += (s, e) => DrawRoundedPanel(e.Graphics, pnlGridContainer.ClientRectangle, 20, _cardBackground, _borderColor);

            pnlMain.Controls.Add(pnlGridContainer);
        }

        private void SetupDataGrid()
        {
            dgvSalesHistory = new DataGridView
            {
                Location = new Point(15, 15),
                Size = new Size(pnlGridContainer.Width - 30, pnlGridContainer.Height - 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = _cardBackground,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                GridColor = _borderColor,
                RowTemplate = { Height = 45 },
                ColumnHeadersHeight = 50
            };

            // Modern header styling
            dgvSalesHistory.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251),
                ForeColor = _textPrimary,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = Color.FromArgb(249, 250, 251),
                Padding = new Padding(15, 12, 15, 12)
            };

            dgvSalesHistory.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = _cardBackground,
                ForeColor = _textPrimary,
                SelectionBackColor = Color.FromArgb(239, 246, 255),
                SelectionForeColor = _primaryBlue,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(15, 8, 15, 8),
                Font = new Font("Segoe UI", 10)
            };

            dgvSalesHistory.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251),
                ForeColor = _textPrimary,
                SelectionBackColor = Color.FromArgb(239, 246, 255),
                SelectionForeColor = _primaryBlue,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(15, 8, 15, 8),
                Font = new Font("Segoe UI", 10)
            };

            dgvSalesHistory.CellFormatting += DgvSalesHistory_CellFormatting;
            dgvSalesHistory.SelectionChanged += DgvSalesHistory_SelectionChanged;
            dgvSalesHistory.CellClick += DgvSalesHistory_CellClick;

            pnlGridContainer.Controls.Add(dgvSalesHistory);
        }

        private void SetupStatusBar()
        {
            statusStrip = new StatusStrip
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9),
                Padding = new Padding(0, 0, 0, 5)
            };

            lblStatus = new ToolStripStatusLabel
            {
                Text = "✅ Ready",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = _successGreen
            };

            lblCount = new ToolStripStatusLabel
            {
                Text = "📊 0 transactions",
                ForeColor = _textSecondary
            };

            lblTotal = new ToolStripStatusLabel
            {
                Text = "💰 Total: $0.00",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = _successGreen,
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblCount, lblTotal });
        }

        // Helper methods for creating modern controls
        private Button CreateModernButton(string icon, string text, Color backgroundColor, Point location, Size size)
        {
            var button = new Button
            {
                Text = string.IsNullOrEmpty(text) ? icon : $"{icon} {text}",
                Location = location,
                Size = size,
                BackColor = backgroundColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = AdjustBrightness(backgroundColor, -0.1f);

            // Add rounded corners
            button.Paint += (s, e) =>
            {
                var btn = s as Button;
                if (btn != null)
                {
                    DrawRoundedButton(e.Graphics, btn.ClientRectangle, 10, btn.BackColor);
                    
                    var textRect = btn.ClientRectangle;
                    TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, textRect, btn.ForeColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };

            return button;
        }

        private Label CreateModernLabel(string text, Point location, Size size, int fontSize, FontStyle style = FontStyle.Regular)
        {
            return new Label
            {
                Text = text,
                Location = location,
                Size = size,
                Font = new Font("Segoe UI", fontSize, style),
                ForeColor = _textPrimary,
                BackColor = Color.Transparent
            };
        }

        private Label CreateSectionLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Location = location,
                Size = new Size(340, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _textPrimary,
                BackColor = Color.Transparent
            };
        }

        private Label CreateDetailsLabel(Point location, Size size)
        {
            return new Label
            {
                Location = location,
                Size = size,
                Font = new Font("Segoe UI", 9),
                ForeColor = _textSecondary,
                BackColor = Color.FromArgb(249, 250, 251),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10),
                Text = "Select a transaction to view details"
            };
        }

        private DateTimePicker CreateModernDatePicker(Point location, Size size)
        {
            var dtp = new DateTimePicker
            {
                Location = location,
                Size = size,
                Format = DateTimePickerFormat.Long,
                Font = new Font("Segoe UI", 9),
                CalendarForeColor = _textPrimary
            };

            return dtp;
        }

        private TextBox CreateModernTextBox(Point location, Size size, string placeholder)
        {
            var textBox = new TextBox
            {
                Location = location,
                Size = size,
                PlaceholderText = placeholder,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(249, 250, 251)
            };

            return textBox;
        }

        // Custom painting methods
        private void DrawRoundedPanel(Graphics graphics, Rectangle rect, int radius, Color backgroundColor, Color borderColor)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;

            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = CreateRoundedPath(rect, radius);
            using var brush = new SolidBrush(backgroundColor);
            using var pen = new Pen(borderColor, 1);

            graphics.FillPath(brush, path);
            graphics.DrawPath(pen, path);
        }

        private void DrawRoundedButton(Graphics graphics, Rectangle rect, int radius, Color backgroundColor)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;

            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = CreateRoundedPath(rect, radius);
            using var brush = new SolidBrush(backgroundColor);

            graphics.FillPath(brush, path);
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;

            // Ensure radius doesn't exceed rectangle dimensions
            radius = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height / 2));
            diameter = radius * 2;

            if (diameter > 0)
            {
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            }
            else
            {
                path.AddRectangle(rect);
            }

            path.CloseFigure();
            return path;
        }

        private Color AdjustBrightness(Color color, float factor)
        {
            var r = (int)Math.Max(0, Math.Min(255, color.R + (255 * factor)));
            var g = (int)Math.Max(0, Math.Min(255, color.G + (255 * factor)));
            var b = (int)Math.Max(0, Math.Min(255, color.B + (255 * factor)));
            return Color.FromArgb(color.A, r, g, b);
        }

        // Data loading and filtering methods
        private async void SalesHistoryForm_Load(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                lblStatus.Text = "⏳ Loading sales data...";
                lblStatus.ForeColor = _warningOrange;
                Application.DoEvents();

                var response = await _apiService.GetSalesAsync(new PaginationParameters
                {
                    PageNumber = 1,
                    PageSize = 500
                });

                if (response.Success && response.Data?.Items != null)
                {
                    _salesData = new List<SaleItemDisplayDto>();
                    
                    foreach (var sale in response.Data.Items)
                    {
                        if (sale.Items != null)
                        {
                            foreach (var item in sale.Items)
                            {
                                _salesData.Add(new SaleItemDisplayDto
                                {
                                    Date = sale.Date,
                                    ProductName = item.ProductName,
                                    CustomerName = sale.CustomerName,
                                    Quantity = item.Quantity,
                                    UnitPrice = item.UnitPrice,
                                    TotalPrice = item.Subtotal,
                                    InvoiceNumber = sale.Id,
                                    PaymentMethod = sale.PaymentMethod,
                                    Status = sale.Status
                                });
                            }
                        }
                    }

                    ApplyFilters();
                    lblStatus.Text = "✅ Ready";
                    lblStatus.ForeColor = _successGreen;
                }
                else
                {
                    lblStatus.Text = "⚠️ No data found";
                    lblStatus.ForeColor = _warningOrange;
                    _salesData = new List<SaleItemDisplayDto>();
                    UpdateGrid();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sales data");
                lblStatus.Text = "❌ Error loading data";
                lblStatus.ForeColor = _errorRed;
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                var filtered = _salesData.AsEnumerable();

                // Date filter
                filtered = filtered.Where(s => s.Date.Date >= dtpStartDate.Value.Date && 
                                             s.Date.Date <= dtpEndDate.Value.Date);

                // Search filter
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    var searchTerm = txtSearch.Text.ToLower();
                    filtered = filtered.Where(s => 
                        s.ProductName.ToLower().Contains(searchTerm) ||
                        s.CustomerName.ToLower().Contains(searchTerm));
                }

                _filteredData = filtered.OrderByDescending(s => s.Date).ToList();
                UpdateGrid();
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying filters");
            }
        }

        private void UpdateGrid()
        {
            try
            {
                dgvSalesHistory.DataSource = null;
                dgvSalesHistory.DataSource = _filteredData;

                if (dgvSalesHistory.Columns.Count > 0)
                {
                    ConfigureColumn("Date", "📅 Date", 140, "MMM dd, yyyy");
                    ConfigureColumn("ProductName", "📦 Product", 200);
                    ConfigureColumn("CustomerName", "👤 Customer", 180);
                    ConfigureColumn("Quantity", "📊 QTY", 80);
                    ConfigureColumn("UnitPrice", "💰 Unit Price", 110, "C2");
                    ConfigureColumn("TotalPrice", "💵 Total", 110, "C2");
                    ConfigureColumn("InvoiceNumber", "🧾 Invoice", 100);
                    ConfigureColumn("PaymentMethod", "💳 Payment", 110);
                    ConfigureColumn("Status", "📊 Status", 110);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating grid");
            }
        }

        private void UpdateStatusBar()
        {
            lblCount.Text = $"📊 {_filteredData.Count} transactions found";
            var totalAmount = _filteredData.Sum(s => s.TotalPrice);
            lblTotal.Text = $"💰 Total Sales: {totalAmount:C}";
        }

        private void ConfigureColumn(string columnName, string headerText, int width, string format = "")
        {
            if (dgvSalesHistory.Columns.Contains(columnName))
            {
                var column = dgvSalesHistory.Columns[columnName];
                if (column != null)
                {
                    column.HeaderText = headerText;
                    column.Width = width;
                    column.MinimumWidth = width - 20;
                    
                    if (!string.IsNullOrEmpty(format))
                    {
                        column.DefaultCellStyle.Format = format;
                    }

                    // Alignment - Updated to MiddleLeft for better visual presentation
                    if (columnName == "Quantity" || columnName == "InvoiceNumber" || 
                        columnName == "UnitPrice" || columnName == "TotalPrice")
                    {
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    }
                }
            }
        }

        private void ShowDetailsPanel(SaleItemDisplayDto item)
        {
            _selectedItem = item;

            // Update product details
            lblProductDetails.Text = $"Product: {item.ProductName}\n" +
                                   $"Quantity Sold: {item.Quantity} units\n" +
                                   $"Unit Price: {item.UnitPrice:C}\n" +
                                   $"Line Total: {item.TotalPrice:C}";

            // Update customer details
            lblCustomerDetails.Text = $"Customer: {item.CustomerName}\n" +
                                    $"Purchase Date: {item.Date:dddd, MMMM dd, yyyy}";

            // Update transaction details
            lblTransactionDetails.Text = $"Invoice #: {item.InvoiceNumber}\n" +
                                       $"Date & Time: {item.Date:MMM dd, yyyy hh:mm tt}\n" +
                                       $"Status: {item.Status}\n" +
                                       $"Total Amount: {item.TotalPrice:C}";

            // Update payment details
            lblPaymentDetails.Text = $"Payment Method: {item.PaymentMethod}\n" +
                                   $"Amount: {item.TotalPrice:C}";

            // Show the details panel
            pnlDetails.Visible = true;
            splitterDetails.Visible = true;

            lblStatus.Text = $"📋 Viewing details for {item.ProductName}";
        }

        private void HideDetailsPanel()
        {
            pnlDetails.Visible = false;
            splitterDetails.Visible = false;
            _selectedItem = null;
            lblStatus.Text = "✅ Ready";
        }

        private void DgvSalesHistory_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is not DataGridView grid || grid.Rows.Count <= e.RowIndex || e.RowIndex < 0)
                return;

            var item = grid.Rows[e.RowIndex].DataBoundItem as SaleItemDisplayDto;
            if (item == null) return;

            // Modern status formatting
            if (grid.Columns[e.ColumnIndex].Name == "Status")
            {
                switch (item.Status.ToLower())
                {
                    case "completed":
                        e.CellStyle.BackColor = Color.FromArgb(220, 252, 231);
                        e.CellStyle.ForeColor = Color.FromArgb(22, 163, 74);
                        e.Value = "✅ Completed";
                        break;
                    case "pending":
                        e.CellStyle.BackColor = Color.FromArgb(254, 249, 195);
                        e.CellStyle.ForeColor = Color.FromArgb(161, 98, 7);
                        e.Value = "⏳ Pending";
                        break;
                    case "cancelled":
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                        e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                        e.Value = "❌ Cancelled";
                        break;
                }
                e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                e.FormattingApplied = true;
            }

            // Payment method formatting
            if (grid.Columns[e.ColumnIndex].Name == "PaymentMethod")
            {
                switch (item.PaymentMethod.ToLower())
                {
                    case "cash":
                        e.Value = "💵 Cash";
                        e.CellStyle.ForeColor = _successGreen;
                        break;
                    case "card":
                        e.Value = "💳 Card";
                        e.CellStyle.ForeColor = _primaryBlue;
                        break;
                    case "bank":
                        e.Value = "🏦 Bank";
                        e.CellStyle.ForeColor = _textSecondary;
                        break;
                    default:
                        e.Value = $"💰 {item.PaymentMethod}";
                        break;
                }
                e.FormattingApplied = true;
            }
        }

        // Event handlers
        private void DgvSalesHistory_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvSalesHistory.SelectedRows.Count > 0)
            {
                var selectedItem = dgvSalesHistory.SelectedRows[0].DataBoundItem as SaleItemDisplayDto;
                if (selectedItem != null)
                {
                    ShowDetailsPanel(selectedItem);
                }
            }
        }

        private void DgvSalesHistory_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _filteredData.Count)
            {
                ShowDetailsPanel(_filteredData[e.RowIndex]);
            }
        }

        private void BtnCloseDetails_Click(object? sender, EventArgs e)
        {
            HideDetailsPanel();
        }

        private void InitializePrintComponents()
        {
            try
            {
                _printDocument = new PrintDocument();
                _printDocument.PrintPage += PrintDocument_PrintPage;
                _printDocument.DocumentName = "Invoice";

                _printPreviewDialog = new PrintPreviewDialog
                {
                    Document = _printDocument,
                    Width = 800,
                    Height = 600,
                    UseAntiAlias = true
                };

                _printDialog = new PrintDialog
                {
                    Document = _printDocument,
                    UseEXDialog = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing print components");
            }
        }

        private void BtnPrintPreview_Click(object? sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Please select a transaction to preview the invoice.", 
                    "No Transaction Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _printPreviewDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing print preview");
                MessageBox.Show($"Error showing print preview: {ex.Message}", 
                    "Print Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrintInvoice_Click(object? sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Please select a transaction to print the invoice.", 
                    "No Transaction Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_printDialog.ShowDialog(this) == DialogResult.OK)
                {
                    _printDocument.Print();
                    
                    MessageBox.Show($"Invoice #{_selectedItem.InvoiceNumber} has been sent to the printer successfully!", 
                        "Print Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing invoice");
                MessageBox.Show($"Error printing invoice: {ex.Message}", 
                    "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            if (_selectedItem == null || e.Graphics == null) return;

            try
            {
                var graphics = e.Graphics;
                var pageWidth = e.PageBounds.Width;
                var pageHeight = e.PageBounds.Height;
                var margin = 60;
                var yPosition = margin;
                var leftMargin = margin;
                var rightMargin = pageWidth - margin;
                var contentWidth = rightMargin - leftMargin;

                // Define fonts
                var titleFont = new Font("Segoe UI", 28, FontStyle.Bold);
                var headerFont = new Font("Segoe UI", 16, FontStyle.Bold);
                var subHeaderFont = new Font("Segoe UI", 14, FontStyle.Bold);
                var normalFont = new Font("Segoe UI", 11);
                var smallFont = new Font("Segoe UI", 10);
                var boldFont = new Font("Segoe UI", 11, FontStyle.Bold);

                // Colors
                var primaryColor = Color.FromArgb(59, 130, 246);
                var darkGray = Color.FromArgb(55, 65, 81);
                var lightGray = Color.FromArgb(156, 163, 175);

                // Company Header
                using (var brush = new SolidBrush(primaryColor))
                {
                    graphics.FillRectangle(brush, leftMargin, yPosition, contentWidth, 100);
                }

                using (var brush = new SolidBrush(Color.White))
                {
                    graphics.DrawString("INVENTORY PRO", titleFont, brush, leftMargin + 20, yPosition + 20);
                    graphics.DrawString("Professional Inventory Management System", normalFont, brush, leftMargin + 20, yPosition + 60);
                }

                yPosition += 120;

                // Invoice Title and Number
                using (var brush = new SolidBrush(darkGray))
                {
                    graphics.DrawString("SALES INVOICE", headerFont, brush, leftMargin, yPosition);
                    var invoiceText = $"Invoice #: {_selectedItem.InvoiceNumber}";
                    var invoiceSize = graphics.MeasureString(invoiceText, headerFont);
                    graphics.DrawString(invoiceText, headerFont, brush, rightMargin - invoiceSize.Width, yPosition);
                }

                yPosition += 50;

                // Date and Status
                using (var brush = new SolidBrush(darkGray))
                {
                    graphics.DrawString($"Date: {_selectedItem.Date:dddd, MMMM dd, yyyy}", normalFont, brush, leftMargin, yPosition);
                    graphics.DrawString($"Time: {_selectedItem.Date:hh:mm tt}", normalFont, brush, leftMargin, yPosition + 25);
                    
                    var statusText = $"Status: {_selectedItem.Status}";
                    var statusSize = graphics.MeasureString(statusText, boldFont);
                    graphics.DrawString(statusText, boldFont, brush, rightMargin - statusSize.Width, yPosition);
                }

                yPosition += 70;

                // Customer Information Section
                DrawSectionHeader(graphics, "BILL TO:", subHeaderFont, darkGray, leftMargin, yPosition);
                yPosition += 35;

                using (var lightBrush = new SolidBrush(Color.FromArgb(249, 250, 251)))
                {
                    graphics.FillRectangle(lightBrush, leftMargin, yPosition, contentWidth / 2 - 10, 80);
                }

                using (var pen = new Pen(Color.FromArgb(229, 231, 235)))
                {
                    graphics.DrawRectangle(pen, leftMargin, yPosition, contentWidth / 2 - 10, 80);
                }

                using (var brush = new SolidBrush(darkGray))
                {
                    graphics.DrawString(_selectedItem.CustomerName, boldFont, brush, leftMargin + 15, yPosition + 15);
                    graphics.DrawString($"Purchase Date: {_selectedItem.Date:MMM dd, yyyy}", normalFont, brush, leftMargin + 15, yPosition + 40);
                }

                yPosition += 100;

                // Product Details Section
                DrawSectionHeader(graphics, "PRODUCT DETAILS:", subHeaderFont, darkGray, leftMargin, yPosition);
                yPosition += 35;

                // Table Header
                using (var headerBrush = new SolidBrush(Color.FromArgb(243, 244, 246)))
                {
                    graphics.FillRectangle(headerBrush, leftMargin, yPosition, contentWidth, 40);
                }

                using (var pen = new Pen(Color.FromArgb(229, 231, 235)))
                {
                    graphics.DrawRectangle(pen, leftMargin, yPosition, contentWidth, 40);
                }

                using (var brush = new SolidBrush(darkGray))
                {
                    graphics.DrawString("PRODUCT", boldFont, brush, leftMargin + 15, yPosition + 12);
                    graphics.DrawString("QTY", boldFont, brush, leftMargin + 300, yPosition + 12);
                    graphics.DrawString("UNIT PRICE", boldFont, brush, leftMargin + 380, yPosition + 12);
                    graphics.DrawString("TOTAL", boldFont, brush, leftMargin + 500, yPosition + 12);
                }

                yPosition += 40;

                // Product Row
                using (var pen = new Pen(Color.FromArgb(229, 231, 235)))
                {
                    graphics.DrawRectangle(pen, leftMargin, yPosition, contentWidth, 50);
                }

                using (var brush = new SolidBrush(darkGray))
                {
                    graphics.DrawString(_selectedItem.ProductName, normalFont, brush, leftMargin + 15, yPosition + 15);
                    graphics.DrawString(_selectedItem.Quantity.ToString(), normalFont, brush, leftMargin + 315, yPosition + 15);
                    graphics.DrawString(_selectedItem.UnitPrice.ToString("C"), normalFont, brush, leftMargin + 395, yPosition + 15);
                    graphics.DrawString(_selectedItem.TotalPrice.ToString("C"), boldFont, brush, leftMargin + 515, yPosition + 15);
                }

                yPosition += 70;

                // Payment Information
                DrawSectionHeader(graphics, "PAYMENT INFORMATION:", subHeaderFont, darkGray, leftMargin, yPosition);
                yPosition += 35;

                using (var lightBrush = new SolidBrush(Color.FromArgb(249, 250, 251)))
                {
                    graphics.FillRectangle(lightBrush, leftMargin, yPosition, contentWidth, 60);
                }

                using (var pen = new Pen(Color.FromArgb(229, 231, 235)))
                {
                    graphics.DrawRectangle(pen, leftMargin, yPosition, contentWidth, 60);
                }

                using (var brush = new SolidBrush(darkGray))
                {
                    graphics.DrawString($"Payment Method: {_selectedItem.PaymentMethod}", normalFont, brush, leftMargin + 15, yPosition + 15);
                    
                    var totalText = $"TOTAL AMOUNT: {_selectedItem.TotalPrice:C}";
                    var totalSize = graphics.MeasureString(totalText, headerFont);
                    graphics.DrawString(totalText, headerFont, brush, rightMargin - totalSize.Width - 15, yPosition + 20);
                }

                yPosition += 80;

                // Footer
                yPosition = pageHeight - 120;
                using (var brush = new SolidBrush(lightGray))
                {
                    var footerText = $"Generated on {DateTime.Now:dddd, MMMM dd, yyyy 'at' hh:mm tt} by InventoryPro System";
                    var footerSize = graphics.MeasureString(footerText, smallFont);
                    graphics.DrawString(footerText, smallFont, brush, (pageWidth - footerSize.Width) / 2, yPosition);
                    
                    graphics.DrawString("Thank you for your business!", smallFont, brush, 
                        (pageWidth - graphics.MeasureString("Thank you for your business!", smallFont).Width) / 2, yPosition + 25);
                }

                // Dispose fonts
                titleFont.Dispose();
                headerFont.Dispose();
                subHeaderFont.Dispose();
                normalFont.Dispose();
                smallFont.Dispose();
                boldFont.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during invoice printing");
            }
        }

        private void DrawSectionHeader(Graphics graphics, string text, Font font, Color color, int x, int y)
        {
            using (var brush = new SolidBrush(color))
            {
                graphics.DrawString(text, font, brush, x, y);
            }
            
            using (var pen = new Pen(Color.FromArgb(229, 231, 235), 2))
            {
                var textSize = graphics.MeasureString(text, font);
                graphics.DrawLine(pen, x + (int)textSize.Width + 10, y + (int)(textSize.Height / 2), 
                    x + 600, y + (int)(textSize.Height / 2));
            }
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            txtSearch.Clear();
            dtpStartDate.Value = DateTime.Today.AddDays(-30);
            dtpEndDate.Value = DateTime.Today;
            ApplyFilters();
        }

        private async void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_filteredData == null || !_filteredData.Any())
                {
                    MessageBox.Show("No sales data to export.", "No Data", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var exportDialog = new SalesHistoryExportDialog();
                if (exportDialog.ShowDialog() == DialogResult.OK)
                {
                    lblStatus.Text = "⏳ Exporting sales data...";
                    lblStatus.ForeColor = _warningOrange;
                    
                    bool success = false;
                    switch (exportDialog.SelectedFormat)
                    {
                        case SalesExportFormat.CSV:
                            success = await ExportSalesToCsvAsync(exportDialog.ExportOptions);
                            break;
                        case SalesExportFormat.Excel:
                            success = await ExportSalesToExcelAsync(exportDialog.ExportOptions);
                            break;
                        case SalesExportFormat.PDF:
                            success = await ExportSalesToPdfAsync(exportDialog.ExportOptions);
                            break;
                    }

                    if (success)
                    {
                        lblStatus.Text = "✅ Export completed successfully";
                        lblStatus.ForeColor = _successGreen;
                        var result = MessageBox.Show(
                            $"Sales data exported successfully!\n\nRecords exported: {_filteredData.Count}\nTotal amount: {_filteredData.Sum(s => s.TotalPrice):C}\n\nWould you like to open the file location?",
                            "Export Successful", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        
                        if (result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start("explorer.exe", 
                                $"/select,\"{exportDialog.ExportOptions.FilePath}\"");
                        }
                    }
                    else
                    {
                        lblStatus.Text = "❌ Export failed";
                        lblStatus.ForeColor = _errorRed;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sales export");
                MessageBox.Show("An error occurred during export. Please try again.",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Export failed";
                lblStatus.ForeColor = _errorRed;
            }
        }

        #region Export Methods

        private async Task<bool> ExportSalesToCsvAsync(SalesExportOptions options)
        {
            try
            {
                using var writer = new StreamWriter(options.FilePath);
                using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

                if (options.IncludeHeaders)
                {
                    csv.WriteField("Date");
                    csv.WriteField("Invoice Number");
                    csv.WriteField("Product Name");
                    csv.WriteField("Customer Name");
                    csv.WriteField("Quantity");
                    csv.WriteField("Unit Price");
                    csv.WriteField("Total Price");
                    csv.WriteField("Payment Method");
                    csv.WriteField("Status");
                    await csv.NextRecordAsync();
                }

                foreach (var sale in _filteredData)
                {
                    csv.WriteField(sale.Date.ToString("yyyy-MM-dd"));
                    csv.WriteField(sale.InvoiceNumber.ToString());
                    csv.WriteField(sale.ProductName ?? "");
                    csv.WriteField(sale.CustomerName ?? "");
                    csv.WriteField(sale.Quantity.ToString());
                    csv.WriteField(sale.UnitPrice.ToString("C2"));
                    csv.WriteField(sale.TotalPrice.ToString("C2"));
                    csv.WriteField(sale.PaymentMethod ?? "");
                    csv.WriteField(sale.Status ?? "");
                    await csv.NextRecordAsync();
                }

                if (options.IncludeSummary)
                {
                    await csv.NextRecordAsync();
                    csv.WriteField("SUMMARY");
                    await csv.NextRecordAsync();
                    csv.WriteField($"Total Transactions: {_filteredData.Count}");
                    await csv.NextRecordAsync();
                    csv.WriteField($"Total Revenue: {_filteredData.Sum(s => s.TotalPrice):C2}");
                    await csv.NextRecordAsync();
                    csv.WriteField($"Average Transaction: {(_filteredData.Any() ? _filteredData.Average(s => s.TotalPrice) : 0):C2}");
                    await csv.NextRecordAsync();
                    csv.WriteField($"Date Range: {dtpStartDate.Value:yyyy-MM-dd} to {dtpEndDate.Value:yyyy-MM-dd}");
                    await csv.NextRecordAsync();
                }

                if (options.IncludeTimestamp)
                {
                    await csv.NextRecordAsync();
                    csv.WriteField($"Exported on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    await csv.NextRecordAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting sales to CSV");
                MessageBox.Show($"Error exporting sales to CSV: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> ExportSalesToExcelAsync(SalesExportOptions options)
        {
            try
            {
                var fileInfo = new FileInfo(options.FilePath);
                using var package = new ExcelPackage(fileInfo);
                
                var worksheet = package.Workbook.Worksheets.Add("Sales History");

                int row = 1;

                if (options.IncludeHeaders)
                {
                    var headers = new[] { "Date", "Invoice #", "Product Name", "Customer Name", 
                        "Quantity", "Unit Price", "Total Price", "Payment Method", "Status" };
                    
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        worksheet.Cells[row, col].Value = headers[col - 1];
                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(59, 130, 246));
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.White);
                    }
                    row++;
                }

                foreach (var sale in _filteredData)
                {
                    worksheet.Cells[row, 1].Value = sale.Date.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 2].Value = sale.InvoiceNumber;
                    worksheet.Cells[row, 3].Value = sale.ProductName ?? "";
                    worksheet.Cells[row, 4].Value = sale.CustomerName ?? "";
                    worksheet.Cells[row, 5].Value = sale.Quantity;
                    worksheet.Cells[row, 6].Value = sale.UnitPrice;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
                    worksheet.Cells[row, 7].Value = sale.TotalPrice;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";
                    worksheet.Cells[row, 8].Value = sale.PaymentMethod ?? "";
                    worksheet.Cells[row, 9].Value = sale.Status ?? "";

                    // Color coding for status
                    switch (sale.Status?.ToLower())
                    {
                        case "completed":
                            worksheet.Cells[row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, 9].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 252, 231));
                            break;
                        case "pending":
                            worksheet.Cells[row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, 9].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(254, 249, 195));
                            break;
                        case "cancelled":
                            worksheet.Cells[row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, 9].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(254, 226, 226));
                            break;
                    }

                    row++;
                }

                if (options.IncludeSummary)
                {
                    row += 2;
                    worksheet.Cells[row, 1].Value = "SUMMARY";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 14;
                    row++;

                    worksheet.Cells[row, 1].Value = "Total Transactions:";
                    worksheet.Cells[row, 2].Value = _filteredData.Count;
                    row++;

                    worksheet.Cells[row, 1].Value = "Total Revenue:";
                    worksheet.Cells[row, 2].Value = _filteredData.Sum(s => s.TotalPrice);
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                    row++;

                    worksheet.Cells[row, 1].Value = "Average Transaction:";
                    worksheet.Cells[row, 2].Value = _filteredData.Any() ? _filteredData.Average(s => s.TotalPrice) : 0;
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                    row++;

                    worksheet.Cells[row, 1].Value = "Date Range:";
                    worksheet.Cells[row, 2].Value = $"{dtpStartDate.Value:yyyy-MM-dd} to {dtpEndDate.Value:yyyy-MM-dd}";
                    row++;
                }

                if (options.IncludeTimestamp)
                {
                    row += 2;
                    worksheet.Cells[row, 1].Value = $"Exported on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    worksheet.Cells[row, 1].Style.Font.Italic = true;
                }

                worksheet.Cells.AutoFitColumns();
                
                for (int col = 1; col <= 9; col++)
                {
                    worksheet.Column(col).Width = Math.Max(worksheet.Column(col).Width, 12);
                }

                await package.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting sales to Excel");
                MessageBox.Show($"Error exporting sales to Excel: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> ExportSalesToPdfAsync(SalesExportOptions options)
        {
            try
            {
                // PDF export with fallback to Excel if PDF fails
                try
                {
                    using var writer = new PdfWriter(options.FilePath);
                    using var pdf = new PdfDocument(writer);
                    using var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());
                    document.SetMargins(30, 20, 30, 20);

                    var title = new Paragraph("Sales History Report")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetBold()
                        .SetMarginBottom(20);
                    document.Add(title);

                    var table = new Table(9)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginTop(10)
                        .SetMarginBottom(10);

                    if (options.IncludeHeaders)
                    {
                        var headers = new[] { "Date", "Invoice #", "Product", "Customer", 
                            "Qty", "Unit Price", "Total", "Payment", "Status" };
                        
                        foreach (var header in headers)
                        {
                            var cell = new Cell()
                                .Add(new Paragraph(header))
                                .SetBackgroundColor(DeviceRgb.BLACK)
                                .SetFontColor(DeviceRgb.WHITE)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetBold()
                                .SetFontSize(8)
                                .SetPadding(4);
                            table.AddCell(cell);
                        }
                    }

                    foreach (var sale in _filteredData)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(sale.Date.ToString("MM/dd/yyyy"))).SetPadding(3).SetFontSize(7));
                        table.AddCell(new Cell().Add(new Paragraph(sale.InvoiceNumber.ToString())).SetPadding(3).SetFontSize(7));
                        table.AddCell(new Cell().Add(new Paragraph(sale.ProductName ?? "")).SetPadding(3).SetFontSize(7));
                        table.AddCell(new Cell().Add(new Paragraph(sale.CustomerName ?? "")).SetPadding(3).SetFontSize(7));
                        table.AddCell(new Cell().Add(new Paragraph(sale.Quantity.ToString()))
                            .SetPadding(3).SetFontSize(7).SetTextAlignment(TextAlignment.CENTER));
                        table.AddCell(new Cell().Add(new Paragraph(sale.UnitPrice.ToString("C")))
                            .SetPadding(3).SetFontSize(7).SetTextAlignment(TextAlignment.RIGHT));
                        table.AddCell(new Cell().Add(new Paragraph(sale.TotalPrice.ToString("C")))
                            .SetPadding(3).SetFontSize(7).SetTextAlignment(TextAlignment.RIGHT));
                        table.AddCell(new Cell().Add(new Paragraph(sale.PaymentMethod ?? "")).SetPadding(3).SetFontSize(7));
                        table.AddCell(new Cell().Add(new Paragraph(sale.Status ?? "")).SetPadding(3).SetFontSize(7));
                    }

                    document.Add(table);

                    if (options.IncludeSummary)
                    {
                        var summaryTitle = new Paragraph("Summary")
                            .SetBold()
                            .SetFontSize(12)
                            .SetMarginTop(20)
                            .SetMarginBottom(10);
                        document.Add(summaryTitle);

                        var summaryTable = new Table(2)
                            .SetWidth(UnitValue.CreatePercentValue(50));

                        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Transactions:")).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));
                        summaryTable.AddCell(new Cell().Add(new Paragraph(_filteredData.Count.ToString())).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));

                        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Revenue:")).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));
                        summaryTable.AddCell(new Cell().Add(new Paragraph(_filteredData.Sum(s => s.TotalPrice).ToString("C2"))).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));

                        summaryTable.AddCell(new Cell().Add(new Paragraph("Average Transaction:")).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));
                        summaryTable.AddCell(new Cell().Add(new Paragraph((_filteredData.Any() ? _filteredData.Average(s => s.TotalPrice) : 0).ToString("C2"))).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));

                        document.Add(summaryTable);
                    }

                    if (options.IncludeTimestamp)
                    {
                        var timestamp = new Paragraph($"Exported on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetFontSize(8)
                            .SetItalic()
                            .SetMarginTop(20);
                        document.Add(timestamp);
                    }

                    return true;
                }
                catch (Exception pdfEx)
                {
                    _logger.LogWarning(pdfEx, "PDF export failed, falling back to Excel");
                    
                    // Change file extension to .xlsx for fallback
                    var excelPath = Path.ChangeExtension(options.FilePath, ".xlsx");
                    var excelOptions = new SalesExportOptions
                    {
                        FilePath = excelPath,
                        IncludeHeaders = options.IncludeHeaders,
                        IncludeSummary = options.IncludeSummary,
                        IncludeTimestamp = options.IncludeTimestamp
                    };
                    
                    var excelSuccess = await ExportSalesToExcelAsync(excelOptions);
                    if (excelSuccess)
                    {
                        MessageBox.Show($"PDF export encountered an issue. Data has been exported to Excel format instead:\n{excelPath}",
                            "Export Format Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return excelSuccess;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting sales to PDF");
                MessageBox.Show($"Error exporting sales to PDF: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Export format options for sales history
    /// </summary>
    public enum SalesExportFormat
    {
        CSV,
        Excel,
        PDF
    }

    /// <summary>
    /// Export options configuration for sales history
    /// </summary>
    public class SalesExportOptions
    {
        public string FilePath { get; set; } = string.Empty;
        public bool IncludeHeaders { get; set; } = true;
        public bool IncludeSummary { get; set; } = true;
        public bool IncludeTimestamp { get; set; } = true;
    }

    /// <summary>
    /// Modern Sales History Export Dialog with professional styling
    /// </summary>
    public class SalesHistoryExportDialog : Form
    {
        public SalesExportFormat SelectedFormat { get; private set; } = SalesExportFormat.CSV;
        public SalesExportOptions ExportOptions { get; private set; } = new();

        private RadioButton rbCSV = null!;
        private RadioButton rbExcel = null!;
        private RadioButton rbPDF = null!;
        private CheckBox chkIncludeHeaders = null!;
        private CheckBox chkIncludeSummary = null!;
        private CheckBox chkIncludeTimestamp = null!;
        private TextBox txtFilePath = null!;
        private Button btnBrowse = null!;
        private Button btnExport = null!;
        private Button btnCancel = null!;
        private Label lblPreview = null!;

        public SalesHistoryExportDialog()
        {
            InitializeComponent();
            SetupEventHandlers();
            UpdatePreview();
        }

        private void InitializeComponent()
        {
            this.Text = "Export Sales History Data";
            this.Size = new Size(680, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F);

            var titleLabel = new Label
            {
                Text = "EXPORT SALES HISTORY DATA",
                Location = new Point(30, 20),
                Size = new Size(590, 35),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(59, 130, 246),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var formatGroupBox = new GroupBox
            {
                Text = "📄 Export Format",
                Location = new Point(30, 70),
                Size = new Size(280, 120),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            rbCSV = new RadioButton
            {
                Text = "CSV (Comma Separated Values)",
                Location = new Point(15, 25),
                Size = new Size(250, 25),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            rbExcel = new RadioButton
            {
                Text = "Excel Workbook (.xlsx)",
                Location = new Point(15, 50),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 9)
            };

            rbPDF = new RadioButton
            {
                Text = "PDF Document",
                Location = new Point(15, 75),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 9)
            };

            formatGroupBox.Controls.AddRange(new Control[] { rbCSV, rbExcel, rbPDF });

            var optionsGroupBox = new GroupBox
            {
                Text = "⚙️ Export Options",
                Location = new Point(340, 70),
                Size = new Size(280, 120),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            chkIncludeHeaders = new CheckBox
            {
                Text = "Include column headers",
                Location = new Point(15, 25),
                Size = new Size(250, 25),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            chkIncludeSummary = new CheckBox
            {
                Text = "Include summary statistics",
                Location = new Point(15, 50),
                Size = new Size(250, 25),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            chkIncludeTimestamp = new CheckBox
            {
                Text = "Include export timestamp",
                Location = new Point(15, 75),
                Size = new Size(250, 25),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            optionsGroupBox.Controls.AddRange(new Control[] { chkIncludeHeaders, chkIncludeSummary, chkIncludeTimestamp });

            var fileGroupBox = new GroupBox
            {
                Text = "💾 Save Location",
                Location = new Point(30, 210),
                Size = new Size(590, 80),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            txtFilePath = new TextBox
            {
                Location = new Point(15, 30),
                Size = new Size(460, 25),
                Font = new Font("Segoe UI", 9),
                PlaceholderText = "Select file location..."
            };

            btnBrowse = new Button
            {
                Text = "Browse...",
                Location = new Point(490, 28),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnBrowse.FlatAppearance.BorderSize = 0;

            fileGroupBox.Controls.AddRange(new Control[] { txtFilePath, btnBrowse });

            var previewGroupBox = new GroupBox
            {
                Text = "👁️ Export Preview",
                Location = new Point(30, 310),
                Size = new Size(590, 130),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            lblPreview = new Label
            {
                Location = new Point(15, 25),
                Size = new Size(500, 95),
                Font = new Font("Consolas", 8),
                ForeColor = Color.FromArgb(73, 80, 87),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10),
                Text = "Preview will appear here..."
            };

            previewGroupBox.Controls.Add(lblPreview);

            btnExport = new Button
            {
                Text = "📤 EXPORT DATA",
                Location = new Point(350, 460),
                Size = new Size(165, 40),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(50)
            };
            btnExport.FlatAppearance.BorderSize = 0;

            btnCancel = new Button
            {
                Text = "CANCEL",
                Location = new Point(530, 460),
                Size = new Size(110, 40),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(50)
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            Controls.AddRange(new Control[] {
                titleLabel, formatGroupBox, optionsGroupBox, fileGroupBox, previewGroupBox, btnExport, btnCancel
            });

            this.AcceptButton = btnExport;
            this.CancelButton = btnCancel;

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            txtFilePath.Text = Path.Combine(desktopPath, $"SalesHistory_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private void SetupEventHandlers()
        {
            rbCSV.CheckedChanged += (s, e) => { if (rbCSV.Checked) UpdateFileExtension(); UpdatePreview(); };
            rbExcel.CheckedChanged += (s, e) => { if (rbExcel.Checked) UpdateFileExtension(); UpdatePreview(); };
            rbPDF.CheckedChanged += (s, e) => { if (rbPDF.Checked) UpdateFileExtension(); UpdatePreview(); };
            
            chkIncludeHeaders.CheckedChanged += (s, e) => UpdatePreview();
            chkIncludeSummary.CheckedChanged += (s, e) => UpdatePreview();
            chkIncludeTimestamp.CheckedChanged += (s, e) => UpdatePreview();

            btnBrowse.Click += BtnBrowse_Click;
            btnExport.Click += BtnExport_Click;
        }

        private void UpdateFileExtension()
        {
            if (string.IsNullOrEmpty(txtFilePath.Text)) return;

            var currentPath = txtFilePath.Text;
            var directory = Path.GetDirectoryName(currentPath) ?? "";
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(currentPath);

            string newExtension = "";
            if (rbCSV.Checked) newExtension = ".csv";
            else if (rbExcel.Checked) newExtension = ".xlsx";
            else if (rbPDF.Checked) newExtension = ".pdf";

            txtFilePath.Text = Path.Combine(directory, fileNameWithoutExt + newExtension);
        }

        private void UpdatePreview()
        {
            var format = rbCSV.Checked ? "CSV" : rbExcel.Checked ? "Excel" : "PDF";
            var preview = $"Format: {format}\n";
            preview += $"Headers: {(chkIncludeHeaders.Checked ? "Yes" : "No")}\n";
            preview += $"Summary: {(chkIncludeSummary.Checked ? "Yes" : "No")}\n";
            preview += $"Timestamp: {(chkIncludeTimestamp.Checked ? "Yes" : "No")}\n\n";
            preview += "Columns to export:\n";
            preview += "• Date\n• Invoice Number\n• Product Name\n• Customer Name\n";
            preview += "• Quantity\n• Unit Price\n• Total Price\n• Payment Method\n• Status";

            lblPreview.Text = preview;
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            var filter = rbCSV.Checked ? "CSV files (*.csv)|*.csv" :
                        rbExcel.Checked ? "Excel files (*.xlsx)|*.xlsx" :
                        "PDF files (*.pdf)|*.pdf";

            using var dialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = rbCSV.Checked ? "csv" : rbExcel.Checked ? "xlsx" : "pdf",
                FileName = Path.GetFileName(txtFilePath.Text)
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = dialog.FileName;
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                MessageBox.Show("Please select a file location.", "Missing File Path",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            try
            {
                var directory = Path.GetDirectoryName(txtFilePath.Text);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating directory: {ex.Message}", "Directory Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
                return;
            }

            SelectedFormat = rbCSV.Checked ? SalesExportFormat.CSV :
                           rbExcel.Checked ? SalesExportFormat.Excel :
                           SalesExportFormat.PDF;

            ExportOptions = new SalesExportOptions
            {
                FilePath = txtFilePath.Text,
                IncludeHeaders = chkIncludeHeaders.Checked,
                IncludeSummary = chkIncludeSummary.Checked,
                IncludeTimestamp = chkIncludeTimestamp.Checked
            };
        }
    }

    /// <summary>
    /// Display DTO for sales history showing "Product sold to Customer" format
    /// </summary>
    public class SaleItemDisplayDto
    {
        public DateTime Date { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int InvoiceNumber { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}