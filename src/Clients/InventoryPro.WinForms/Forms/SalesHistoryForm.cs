using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Drawing.Drawing2D;

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

        // Status bar
        private ToolStripStatusLabel lblStatus = null!;
        private ToolStripStatusLabel lblCount = null!;
        private ToolStripStatusLabel lblTotal = null!;

        // Data
        private List<SaleItemDisplayDto> _salesData = new();
        private List<SaleItemDisplayDto> _filteredData = new();
        private SaleItemDisplayDto? _selectedItem;

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

            InitializeComponent();
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
            btnPrintInvoice = CreateModernButton("🖨️", "Print Invoice", _accentPurple, new Point(25, 550), new Size(340, 40));
            btnPrintInvoice.Click += BtnPrintInvoice_Click;

            pnlDetailsContainer.Controls.AddRange(new Control[] {
                lblDetailsTitle, btnCloseDetails,
                lblProductTitle, lblProductDetails,
                lblCustomerTitle, lblCustomerDetails,
                lblTransactionTitle, lblTransactionDetails,
                lblPaymentTitle, lblPaymentDetails,
                btnPrintInvoice
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

        private void BtnPrintInvoice_Click(object? sender, EventArgs e)
        {
            if (_selectedItem != null)
            {
                MessageBox.Show($"🖨️ Print Invoice #{_selectedItem.InvoiceNumber}\n\n" +
                              $"Product: {_selectedItem.ProductName}\n" +
                              $"Customer: {_selectedItem.CustomerName}\n" +
                              $"Amount: {_selectedItem.TotalPrice:C}\n\n" +
                              "Print functionality will be implemented soon!",
                              "Print Invoice", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            MessageBox.Show($"📊 Export Feature\n\nRecords to export: {_filteredData.Count}\nTotal amount: {_filteredData.Sum(s => s.TotalPrice):C}\n\nThis feature will be implemented soon!", 
                "Export Sales Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
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