using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Sales management form
    /// Provides Point of Sale (POS) functionality
    /// </summary>
    public partial class SalesForm : Form
    {
        private readonly ILogger<SalesForm> _logger;
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;

        // Event to notify when sales data changes
        public event EventHandler? SalesDataChanged;

        // Form sections
        private Panel pnlLeft = null!;
        private Panel pnlRight = null!;

        // Customer selection
        private Label lblCustomer = null!;
        private ComboBox cboCustomer = null!;
        private Button btnNewCustomer = null!;

        // Product search
        private Label lblProduct = null!;
        private TextBox txtProductSearch = null!;
        private Button btnAddProduct = null!;
        private Button btnRefreshData = null!;
        private DataGridView dgvProducts = null!;

        // Shopping cart
        private Label lblCart = null!;
        private Label lblCartCount = null!;
        private DataGridView dgvCart = null!;
        private Label lblSubtotal = null!;
        private Label lblTax = null!;
        private Label lblTotal = null!;

        // Payment
        private Label lblPaymentMethod = null!;
        private ComboBox cboPaymentMethod = null!;
        private Label lblPaidAmount = null!;
        private NumericUpDown nudPaidAmount = null!;
        private Label lblChange = null!;
        private Button btnCompleteSale = null!;
        private Button btnCancelSale = null!;

        // Data
        private List<CustomerDto> _customers = new();
        private List<ProductDto> _products = new();
        private BindingList<CartItem> _cartItems = new();
        private decimal _taxRate = 0.10m; // 10% tax

        public SalesForm(ILogger<SalesForm> logger, IApiService apiService, IAuthService authService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            InitializeComponent();
            this.Load += SalesForm_Load;
            this.Shown += SalesForm_Shown;
        }

        private async void SalesForm_Load(object? sender, EventArgs e)
        {
            try
            {
                btnRefreshData.Enabled = false;
                btnRefreshData.Text = "Loading...";
                
                await InitializeAsync().ConfigureAwait(false);
                
                RunOnUiThread(() =>
                {
                    btnRefreshData.Enabled = true;
                    btnRefreshData.Text = "🔄 Refresh";
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during form initialization");
                RunOnUiThread(() =>
                {
                    btnRefreshData.Enabled = true;
                    btnRefreshData.Text = "🔄 Refresh";
                    MessageBox.Show($"Error loading data: {ex.Message}", "Initialization Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
            }
        }

        private async void SalesForm_Shown(object? sender, EventArgs e)
        {
            if (_products.Count == 0)
            {
                try
                {
                    btnRefreshData.Enabled = false;
                    btnRefreshData.Text = "Loading...";
                    
                    await Task.Delay(100);
                    await LoadProductsAsync().ConfigureAwait(false);
                    
                    RunOnUiThread(() =>
                    {
                        btnRefreshData.Enabled = true;
                        btnRefreshData.Text = "🔄 Refresh";
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading data on form shown");
                    RunOnUiThread(() =>
                    {
                        btnRefreshData.Enabled = true;
                        btnRefreshData.Text = "🔄 Refresh";
                    });
                }
            }
        }

        private async Task InitializeAsync()
        {
            await LoadCustomersAsync().ConfigureAwait(false);
            await LoadProductsAsync().ConfigureAwait(false);
            RunOnUiThread(SetupCartGrid);
        }

        private void RunOnUiThread(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }

        private void InitializeComponent()
        {
            this.Text = "Sales - Point of Sale";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Create main layout with improved responsiveness
            var pnlMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(245, 246, 250),
                Padding = new Padding(15)
            };
            // Better column distribution: 60% for products, 40% for cart
            pnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            pnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // Left panel - Product selection with modern card-like design
            pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.White,
                Margin = new Padding(5)
            };
            pnlLeft.Paint += (s, e) =>
            {
                // Modern card shadow effect
                using (var brush = new SolidBrush(Color.FromArgb(10, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(brush, 3, 3, pnlLeft.Width - 3, pnlLeft.Height - 3);
                }
                e.Graphics.FillRectangle(Brushes.White, 0, 0, pnlLeft.Width - 3, pnlLeft.Height - 3);
                using (var pen = new Pen(Color.FromArgb(230, 235, 241), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlLeft.Width - 4, pnlLeft.Height - 4);
                }
            };

            // Customer section with modern card layout
            var pnlCustomer = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(0, 80), // Width will be set dynamically
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15)
            };
            pnlCustomer.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 225, 230), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlCustomer.Width - 1, pnlCustomer.Height - 1);
                }
            };

            lblCustomer = new Label
            {
                Text = "👤 Customer Selection",
                Location = new Point(15, 10),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                BackColor = Color.Transparent
            };

            cboCustomer = new ComboBox
            {
                Location = new Point(15, 40),
                Size = new Size(0, 32), // Width will be set dynamically
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };

            btnNewCustomer = new Button
            {
                Text = "👥 New Customer",
                Location = new Point(0, 38), // Position will be set dynamically
                Size = new Size(140, 36),
                BackColor = Color.FromArgb(102, 16, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNewCustomer.FlatAppearance.BorderSize = 0;
            btnNewCustomer.FlatAppearance.MouseOverBackColor = Color.FromArgb(81, 12, 194);
            btnNewCustomer.Click += BtnNewCustomer_Click;

            pnlCustomer.Controls.AddRange(new Control[] { lblCustomer, cboCustomer, btnNewCustomer });

            // Product search section with modern card layout
            var pnlProductSearch = new Panel
            {
                Location = new Point(20, 110),
                Size = new Size(0, 100), // Width will be set dynamically
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15)
            };
            pnlProductSearch.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 225, 230), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlProductSearch.Width - 1, pnlProductSearch.Height - 1);
                }
            };

            lblProduct = new Label
            {
                Text = "🔍 Product Search & Selection",
                Location = new Point(15, 10),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                BackColor = Color.Transparent
            };

            txtProductSearch = new TextBox
            {
                Location = new Point(15, 40),
                Size = new Size(0, 32), // Width will be set dynamically
                PlaceholderText = "Search by product name or SKU code...",
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            txtProductSearch.TextChanged += TxtProductSearch_TextChanged;

            btnAddProduct = new Button
            {
                Text = "➕ Add to Cart",
                Location = new Point(0, 38), // Position will be set dynamically
                Size = new Size(120, 36),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddProduct.FlatAppearance.BorderSize = 0;
            btnAddProduct.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 142, 58);
            btnAddProduct.Click += BtnAddProduct_Click;

            btnRefreshData = new Button
            {
                Text = "Loading...",
                Location = new Point(0, 38), // Position will be set dynamically
                Size = new Size(100, 36),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnRefreshData.FlatAppearance.BorderSize = 0;
            btnRefreshData.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 105, 217);
            btnRefreshData.Click += BtnRefreshData_Click;

            pnlProductSearch.Controls.AddRange(new Control[] { lblProduct, txtProductSearch, btnAddProduct, btnRefreshData });

            // Products grid with responsive modern design
            dgvProducts = new DataGridView
            {
                Location = new Point(20, 220),
                Size = new Size(0, 0), // Size will be set dynamically
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Both,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(5),
                BackgroundColor = Color.White
            };
            dgvProducts.DoubleClick += DgvProducts_DoubleClick;
            dgvProducts.CellFormatting += DgvProducts_CellFormatting;

            // Add resize event handler for responsive layout
            pnlLeft.Resize += (s, e) =>
            {
                var width = pnlLeft.Width - 60; // Account for padding and margins
                var height = pnlLeft.Height - 280; // Account for other controls
                
                // Update customer panel
                pnlCustomer.Width = width;
                cboCustomer.Width = width - 170; // Leave space for button
                btnNewCustomer.Left = width - 155;
                
                // Update product search panel
                pnlProductSearch.Width = width;
                txtProductSearch.Width = width - 260; // Leave space for buttons
                btnAddProduct.Left = width - 240;
                btnRefreshData.Left = width - 115;
                
                // Update products grid
                dgvProducts.Width = width;
                dgvProducts.Height = Math.Max(height, 200); // Minimum height
            };

            pnlLeft.Controls.AddRange(new Control[] {
                pnlCustomer, pnlProductSearch, dgvProducts
            });

            // Right panel - Shopping cart with modern card design
            pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.White,
                Margin = new Padding(5)
            };
            pnlRight.Paint += (s, e) =>
            {
                // Modern card shadow effect
                using (var brush = new SolidBrush(Color.FromArgb(10, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(brush, 3, 3, pnlRight.Width - 3, pnlRight.Height - 3);
                }
                e.Graphics.FillRectangle(Brushes.White, 0, 0, pnlRight.Width - 3, pnlRight.Height - 3);
                using (var pen = new Pen(Color.FromArgb(230, 235, 241), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlRight.Width - 4, pnlRight.Height - 4);
                }
            };

            // Cart header with enhanced modern styling
            var pnlCartHeader = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(0, 50), // Width will be set dynamically, reduced from 60 to 50
                BackColor = Color.FromArgb(52, 58, 64),
                Padding = new Padding(20, 12, 20, 12) // Reduced padding
            };
            pnlCartHeader.Paint += (s, e) =>
            {
                // Add gradient effect to header
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, pnlCartHeader.Width, pnlCartHeader.Height),
                    Color.FromArgb(52, 58, 64),
                    Color.FromArgb(73, 80, 87),
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, pnlCartHeader.Width, pnlCartHeader.Height);
                }
            };

            lblCart = new Label
            {
                Text = "🛒 Shopping Cart",
                Location = new Point(20, 15), // Adjusted for smaller header
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 13, FontStyle.Bold), // Reduced from 15 to 13
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            // Add cart item count label
            lblCartCount = new Label
            {
                Text = "0 items",
                Location = new Point(0, 15), // Position will be set dynamically, adjusted for smaller header
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Regular), // Reduced from 12 to 10
                ForeColor = Color.FromArgb(206, 212, 218),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight
            };
            
            pnlCartHeader.Controls.AddRange(new Control[] { lblCart, lblCartCount });

            // Cart grid with modern design
            dgvCart = new DataGridView
            {
                Location = new Point(20, 75), // Adjusted for smaller header (reduced from 80 to 75)
                Size = new Size(0, 0), // Size will be set dynamically
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                GridColor = Color.FromArgb(230, 235, 241)
            };
            dgvCart.CellValueChanged += DgvCart_CellValueChanged;
            dgvCart.UserDeletingRow += DgvCart_UserDeletingRow;

            // Payment section with modern card design
            var pnlPayment = new Panel
            {
                Location = new Point(20, 0), // Position will be set dynamically
                Size = new Size(0, 200), // Width will be set dynamically
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20)
            };
            pnlPayment.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 225, 230), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlPayment.Width - 1, pnlPayment.Height - 1);
                }
            };

            // Totals section
            var pnlTotals = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(0, 120), // Width will be set dynamically
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            pnlTotals.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 225, 230), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlTotals.Width - 1, pnlTotals.Height - 1);
                }
            };

            lblSubtotal = new Label
            {
                Text = "📊 Subtotal: $0.00",
                Location = new Point(15, 15),
                Size = new Size(0, 25), // Width will be set dynamically
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                BackColor = Color.Transparent
            };

            lblTax = new Label
            {
                Text = "📋 Tax (10%): $0.00",
                Location = new Point(15, 45),
                Size = new Size(0, 25), // Width will be set dynamically
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                BackColor = Color.Transparent
            };

            lblTotal = new Label
            {
                Text = "💰 Total: $0.00",
                Location = new Point(15, 75),
                Size = new Size(0, 35), // Width will be set dynamically
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                BackColor = Color.Transparent
            };

            pnlTotals.Controls.AddRange(new Control[] { lblSubtotal, lblTax, lblTotal });

            // Payment details section
            var pnlPaymentDetails = new Panel
            {
                Location = new Point(20, 150),
                Size = new Size(0, 90), // Width will be set dynamically
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            pnlPaymentDetails.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 225, 230), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlPaymentDetails.Width - 1, pnlPaymentDetails.Height - 1);
                }
            };

            lblPaymentMethod = new Label
            {
                Text = "💳 Payment Method:",
                Location = new Point(15, 15),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                BackColor = Color.Transparent
            };

            cboPaymentMethod = new ComboBox
            {
                Location = new Point(170, 13),
                Size = new Size(0, 30), // Width will be set dynamically
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };
            cboPaymentMethod.Items.AddRange(new object[] { "💵 Cash", "💳 Credit Card", "💸 Debit Card", "📝 Check" });
            cboPaymentMethod.SelectedIndex = 0;

            lblPaidAmount = new Label
            {
                Text = "💵 Paid Amount:",
                Location = new Point(15, 50),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                BackColor = Color.Transparent
            };

            nudPaidAmount = new NumericUpDown
            {
                Location = new Point(170, 48),
                Size = new Size(0, 30), // Width will be set dynamically
                DecimalPlaces = 2,
                Maximum = 999999.99m,
                Minimum = 0,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White
            };
            nudPaidAmount.ValueChanged += NudPaidAmount_ValueChanged;

            lblChange = new Label
            {
                Text = "💸 Change: $0.00",
                Location = new Point(0, 15), // Position will be set dynamically
                Size = new Size(0, 30), // Width will be set dynamically
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 162, 184),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight
            };

            pnlPaymentDetails.Controls.AddRange(new Control[] { lblPaymentMethod, cboPaymentMethod, lblPaidAmount, nudPaidAmount, lblChange });

            pnlPayment.Controls.AddRange(new Control[] { pnlTotals, pnlPaymentDetails });

            // Action buttons panel with enhanced design
            var pnlActionButtons = new Panel
            {
                Location = new Point(20, 0), // Position will be set dynamically  
                Size = new Size(0, 130), // Width will be set dynamically, increased height
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15)
            };
            pnlActionButtons.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 225, 230), 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlActionButtons.Width - 1, pnlActionButtons.Height - 1);
                }
            };

            // Complete Sale button with round modern design and smaller text
            btnCompleteSale = CreateRoundButton("✅ Complete", new Point(15, 15), new Size(0, 45), 
                Color.FromArgb(40, 167, 69), Color.FromArgb(34, 142, 58));
            btnCompleteSale.Click += BtnCompleteSale_Click;

            // Generate Invoice button with round modern design and smaller text
            var btnGenerateInvoice = CreateRoundButton("🧾 Invoice", new Point(0, 15), new Size(0, 45), 
                Color.FromArgb(0, 123, 255), Color.FromArgb(0, 105, 217));
            btnGenerateInvoice.Click += BtnGenerateInvoice_Click;

            // Cancel Sale button with modern professional design and smaller text
            btnCancelSale = CreateRoundButton("❌ Cancel", new Point(15, 70), new Size(0, 35), 
                Color.FromArgb(220, 53, 69), Color.FromArgb(200, 35, 51));
            btnCancelSale.Click += BtnCancelSale_Click;

            pnlActionButtons.Controls.AddRange(new Control[] { btnCompleteSale, btnGenerateInvoice, btnCancelSale });

            // Add resize event handler for right panel responsive layout
            pnlRight.Resize += (s, e) =>
            {
                var width = pnlRight.Width - 60; // Account for padding and margins
                var height = pnlRight.Height;
                
                // Update cart header
                pnlCartHeader.Width = width;
                lblCartCount.Left = width - 120;
                lblCartCount.Top = 15; // Ensure consistent positioning
                
                // Calculate available space for cart grid
                var cartHeight = height - 450; // Adjusted for smaller header and reduced sizes
                dgvCart.Width = width;
                dgvCart.Height = Math.Max(cartHeight, 120); // Reduced minimum height
                
                // Update payment section
                var paymentTop = dgvCart.Bottom + 20;
                pnlPayment.Top = paymentTop;
                pnlPayment.Width = width;
                
                // Update inner panels
                pnlTotals.Width = width - 40;
                pnlPaymentDetails.Width = width - 40;
                
                // Update totals labels
                var halfWidth = (width - 40) / 2;
                lblSubtotal.Width = halfWidth;
                lblTax.Width = halfWidth;
                lblTotal.Width = halfWidth;
                
                // Update payment controls
                var paymentControlWidth = width - 220;
                cboPaymentMethod.Width = Math.Max(paymentControlWidth, 120);
                nudPaidAmount.Width = Math.Max(paymentControlWidth, 120);
                
                // Update change label
                lblChange.Left = halfWidth + 15;
                lblChange.Width = halfWidth - 30;
                
                // Update action buttons panel
                var actionButtonsTop = pnlPayment.Bottom + 15;
                pnlActionButtons.Top = actionButtonsTop;
                pnlActionButtons.Width = width;
                
                // Calculate optimal button sizes for all three buttons to be visible
                var buttonWidth = (width - 60) / 2; // Two buttons per row with margins
                var fullButtonWidth = width - 50; // Full width for cancel button
                
                // Ensure minimum button width for readability
                buttonWidth = Math.Max(buttonWidth, 120);
                fullButtonWidth = Math.Max(fullButtonWidth, 150);
                
                // Position Complete Purchase and Generate Invoice buttons side by side
                btnCompleteSale.Width = buttonWidth;
                btnCompleteSale.Left = 15;
                
                btnGenerateInvoice.Width = buttonWidth;
                btnGenerateInvoice.Left = 30 + buttonWidth;
                
                // Position Cancel button below with proper spacing
                btnCancelSale.Width = fullButtonWidth;
                btnCancelSale.Left = 15;
            };

            pnlRight.Controls.AddRange(new Control[] { pnlCartHeader, dgvCart, pnlPayment, pnlActionButtons });

            // Add panels to main layout
            pnlMain.Controls.Add(pnlLeft, 0, 0);
            pnlMain.Controls.Add(pnlRight, 1, 0);

            this.Controls.Add(pnlMain);
        }

        private void SetupCartGrid()
        {
            try
            {
                // Clear data source and columns first to avoid binding conflicts
                dgvCart.DataSource = null;
                dgvCart.Columns.Clear();

                // Configure modern grid styling
                dgvCart.BackgroundColor = Color.White;
                dgvCart.BorderStyle = BorderStyle.None;
                dgvCart.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvCart.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                dgvCart.EnableHeadersVisualStyles = false;
                dgvCart.RowHeadersVisible = false;
                dgvCart.Font = new Font("Segoe UI", 10);
                dgvCart.GridColor = Color.FromArgb(230, 235, 241);
                dgvCart.RowTemplate.Height = 35; // Reduced from 45 to 35
                dgvCart.ColumnHeadersHeight = 40; // Reduced from 50 to 40
                dgvCart.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dgvCart.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                dgvCart.AllowUserToResizeRows = false;
                dgvCart.AllowUserToResizeColumns = true;

                // Modern header styling with reduced font size
                dgvCart.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(52, 58, 64),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold), // Reduced from 11 to 10
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    SelectionBackColor = Color.FromArgb(52, 58, 64),
                    Padding = new Padding(10, 6, 10, 6), // Reduced padding
                    WrapMode = DataGridViewTriState.False
                };

                // Modern cell styling with reduced font size
                dgvCart.DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(33, 37, 41),
                    SelectionBackColor = Color.FromArgb(74, 144, 226),
                    SelectionForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 6, 10, 6), // Reduced padding
                    Font = new Font("Segoe UI", 9), // Reduced from 10 to 9
                    WrapMode = DataGridViewTriState.False
                };

                dgvCart.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 250, 252),
                    ForeColor = Color.FromArgb(33, 37, 41),
                    SelectionBackColor = Color.FromArgb(74, 144, 226),
                    SelectionForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 6, 10, 6), // Reduced padding
                    Font = new Font("Segoe UI", 9), // Reduced from 10 to 9
                    WrapMode = DataGridViewTriState.False
                };

                // Add columns with responsive design
                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "OrderNumber",
                    HeaderText = "📋 #",
                    FillWeight = 8,
                    MinimumWidth = 40,
                    ReadOnly = true,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Alignment = DataGridViewContentAlignment.MiddleLeft,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold), // Reduced from 12 to 10
                        ForeColor = Color.FromArgb(255, 255, 255),
                        BackColor = Color.FromArgb(52, 58, 64),
                        Padding = new Padding(12, 6, 6, 6) // Reduced padding
                    }
                });

                // Hidden ProductId column for internal use
                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "ProductId",
                    DataPropertyName = "ProductId",
                    Visible = false
                });

                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "ProductName",
                    HeaderText = "📦 Product",
                    DataPropertyName = "ProductName",
                    FillWeight = 42,
                    MinimumWidth = 150,
                    ReadOnly = true,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Font = new Font("Segoe UI", 9, FontStyle.Bold), // Reduced from 11 to 9
                        ForeColor = Color.FromArgb(44, 62, 80)
                    }
                });

                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "UnitPrice",
                    HeaderText = "💰 Price",
                    DataPropertyName = "UnitPrice",
                    FillWeight = 20,
                    MinimumWidth = 80,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "C2",
                        Alignment = DataGridViewContentAlignment.MiddleRight,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold), // Reduced from 11 to 9
                        ForeColor = Color.FromArgb(46, 125, 50),
                        BackColor = Color.FromArgb(240, 248, 255)
                    },
                    ReadOnly = true
                });

                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Quantity",
                    HeaderText = "📊 Qty",
                    DataPropertyName = "Quantity",
                    FillWeight = 15,
                    MinimumWidth = 60,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold), // Reduced from 12 to 10
                        BackColor = Color.FromArgb(255, 248, 220),
                        ForeColor = Color.FromArgb(133, 77, 14)
                    }
                });

                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Total",
                    HeaderText = "💸 Total",
                    DataPropertyName = "Total",
                    FillWeight = 15,
                    MinimumWidth = 80,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "C2",
                        Alignment = DataGridViewContentAlignment.MiddleRight,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold), // Reduced from 13 to 10
                        ForeColor = Color.FromArgb(40, 167, 69),
                        BackColor = Color.FromArgb(248, 255, 248)
                    },
                    ReadOnly = true
                });

                // Set data source after columns are configured
                dgvCart.DataSource = _cartItems;

                // Update order numbers after data binding
                UpdateCartOrderNumbers();

                // Add data error handler to gracefully handle any binding issues
                dgvCart.DataError += DgvCart_DataError;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up cart grid");
            }
        }

        private Button CreateRoundButton(string text, Point location, Size size, Color backColor, Color hoverColor)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold), // Reduced from 12 to 10
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = hoverColor;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(
                Math.Max(0, hoverColor.R - 20),
                Math.Max(0, hoverColor.G - 20),
                Math.Max(0, hoverColor.B - 20));

            // Add round corners using Paint event
            button.Paint += (s, e) =>
            {
                var btn = s as Button;
                if (btn != null)
                {
                    var path = new GraphicsPath();
                    var rect = new Rectangle(0, 0, btn.Width, btn.Height);
                    int radius = 15;
                    
                    // Create rounded rectangle path
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                    path.CloseFigure();
                    
                    btn.Region = new Region(path);
                    
                    // Fill with background color
                    using (var brush = new SolidBrush(btn.BackColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    
                    // Draw text
                    var textRect = new Rectangle(0, 0, btn.Width, btn.Height);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using (var textBrush = new SolidBrush(btn.ForeColor))
                    {
                        e.Graphics.DrawString(btn.Text, btn.Font, textBrush, textRect, sf);
                    }
                }
            };

            return button;
        }

        private void UpdateCartOrderNumbers()
        {
            try
            {
                for (int i = 0; i < dgvCart.Rows.Count && i < _cartItems.Count; i++)
                {
                    dgvCart.Rows[i].Cells["OrderNumber"].Value = (i + 1).ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart order numbers");
            }
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                var response = await _apiService.GetCustomersAsync(new PaginationParameters { PageSize = 100 }).ConfigureAwait(false);
                if (response.Success && response.Data != null)
                {
                    _customers = response.Data.Items;
                    RunOnUiThread(UpdateCustomerComboBox);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers");
            }
        }

        private void UpdateCustomerComboBox()
        {
            cboCustomer.BeginUpdate();
            cboCustomer.Items.Clear();
            cboCustomer.Items.Add("Walk-in Customer");
            foreach (var customer in _customers)
            {
                cboCustomer.Items.Add(customer);
            }
            cboCustomer.DisplayMember = "Name";
            cboCustomer.ValueMember = "Id";
            cboCustomer.SelectedIndex = 0;
            cboCustomer.EndUpdate();
        }

        private void SelectCustomerById(int customerId)
        {
            for (int i = 1; i < cboCustomer.Items.Count; i++) // Start from 1 to skip "Walk-in Customer"
            {
                if (cboCustomer.Items[i] is CustomerDto customer && customer.Id == customerId)
                {
                    cboCustomer.SelectedIndex = i;
                    break;
                }
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var response = await _apiService.GetProductsAsync(new PaginationParameters { PageSize = 100 }).ConfigureAwait(false);
                if (response.Success && response.Data != null)
                {
                    var items = response.Data.Items;
                    var inStock = new List<ProductDto>();
                    var outOfStock = new List<ProductDto>();

                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i].Stock > 0)
                        {
                            inStock.Add(items[i]);
                        }
                        else
                        {
                            outOfStock.Add(items[i]);
                        }
                    }

                    _products = inStock;

                    if (outOfStock.Count > 0)
                    {
                        _logger.LogInformation("Found {OutOfStockCount} out-of-stock products that are hidden from sales selection",
                            outOfStock.Count);
                    }

                    RunOnUiThread(UpdateProductGrid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
            }
        }

        private void UpdateProductGrid()
        {
            var filteredProducts = _products;

            if (!string.IsNullOrWhiteSpace(txtProductSearch.Text))
            {
                var searchTerm = txtProductSearch.Text.ToLowerInvariant();
                filteredProducts = _products
                    .Where(p => (p.Name != null && p.Name.ToLowerInvariant().Contains(searchTerm)) ||
                                (p.SKU != null && p.SKU.ToLowerInvariant().Contains(searchTerm)))
                    .ToList();
            }

            dgvProducts.DataSource = null;
            dgvProducts.DataSource = filteredProducts;

            ConfigureProductGridStyling();

            if (dgvProducts.Columns != null && dgvProducts.Columns.Count > 0)
            {
                // Hide the product ID column - we don't want to show it to users
                if (dgvProducts.Columns.Contains("Id"))
                {
                    var column = dgvProducts.Columns["Id"];
                    if (column != null)
                    {
                        column.Visible = false;
                    }
                }

                if (dgvProducts.Columns.Contains("Name"))
                {
                    var column = dgvProducts.Columns["Name"];
                    if (column != null)
                    {
                        column.FillWeight = 50; // 50% of available width (increased since ID is hidden)
                        column.HeaderText = "📦 Product Name";
                        column.MinimumWidth = 200;
                        column.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                        column.DefaultCellStyle.ForeColor = Color.FromArgb(44, 62, 80);
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        column.DefaultCellStyle.Padding = new Padding(12, 12, 12, 12);
                    }
                }

                if (dgvProducts.Columns.Contains("SKU"))
                {
                    var column = dgvProducts.Columns["SKU"];
                    if (column != null)
                    {
                        column.FillWeight = 25; // 25% of available width (increased)
                        column.HeaderText = "🏷️ SKU Code";
                        column.MinimumWidth = 120;
                        column.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                        column.DefaultCellStyle.ForeColor = Color.FromArgb(108, 117, 125);
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        column.DefaultCellStyle.Padding = new Padding(12, 12, 12, 12);
                        column.DefaultCellStyle.BackColor = Color.FromArgb(253, 254, 255);
                    }
                }

                if (dgvProducts.Columns.Contains("Price"))
                {
                    var column = dgvProducts.Columns["Price"];
                    if (column != null)
                    {
                        column.DefaultCellStyle.Format = "C2";
                        column.FillWeight = 12; // 12% of available width
                        column.HeaderText = "💰 Price";
                        column.MinimumWidth = 90;
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                        column.DefaultCellStyle.ForeColor = Color.FromArgb(46, 125, 50);
                        column.DefaultCellStyle.Padding = new Padding(12, 12, 12, 12);
                        column.DefaultCellStyle.BackColor = Color.FromArgb(248, 255, 248);
                    }
                }

                if (dgvProducts.Columns.Contains("Stock"))
                {
                    var column = dgvProducts.Columns["Stock"];
                    if (column != null)
                    {
                        column.FillWeight = 13; // 13% of available width
                        column.HeaderText = "📊 Stock";
                        column.MinimumWidth = 90;
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        column.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                        column.DefaultCellStyle.Padding = new Padding(8, 12, 8, 12);
                    }
                }

                var columnsToHide = new[] { "CategoryId", "CategoryName", "CreatedAt", "UpdatedAt",
                                          "MinStock", "IsActive", "ImageUrl", "Description" };

                foreach (var columnName in columnsToHide)
                {
                    if (dgvProducts.Columns.Contains(columnName))
                    {
                        var column = dgvProducts.Columns[columnName];
                        if (column != null)
                            column.Visible = false;
                    }
                }
            }
        }

        private void ConfigureProductGridStyling()
        {
            dgvProducts.BackgroundColor = Color.FromArgb(252, 253, 254);
            dgvProducts.BorderStyle = BorderStyle.None;
            dgvProducts.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProducts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.RowHeadersVisible = false;
            dgvProducts.Font = new Font("Segoe UI", 10);
            dgvProducts.GridColor = Color.FromArgb(230, 235, 241);

            dgvProducts.RowTemplate.Height = 55;
            dgvProducts.ColumnHeadersHeight = 60;
            dgvProducts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvProducts.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvProducts.AllowUserToResizeRows = false;
            dgvProducts.AllowUserToResizeColumns = true;
            dgvProducts.MultiSelect = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvProducts.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(44, 62, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = Color.FromArgb(44, 62, 80),
                Padding = new Padding(20, 18, 20, 18),
                WrapMode = DataGridViewTriState.False
            };

            dgvProducts.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                SelectionBackColor = Color.FromArgb(74, 144, 226),
                SelectionForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(18, 16, 18, 16),
                Font = new Font("Segoe UI", 10),
                WrapMode = DataGridViewTriState.False
            };

            dgvProducts.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(33, 37, 41),
                SelectionBackColor = Color.FromArgb(74, 144, 226),
                SelectionForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(18, 16, 18, 16),
                Font = new Font("Segoe UI", 10),
                WrapMode = DataGridViewTriState.False
            };

            dgvProducts.AdvancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.Single;
            dgvProducts.AdvancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.Single;
        }

        private void DgvProducts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is not DataGridView grid || grid.Rows.Count <= e.RowIndex || e.RowIndex < 0)
                return;

            var product = grid.Rows[e.RowIndex].DataBoundItem as ProductDto;
            if (product == null) return;

            if (grid.Columns[e.ColumnIndex].Name == "Stock")
            {
                var stock = product.Stock;
                var minStock = product.MinStock;

                if (stock <= 0)
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 235, 238);
                    e.CellStyle.ForeColor = Color.FromArgb(220, 53, 69);
                    e.Value = "⚠️ OUT";
                }
                else if (stock <= minStock)
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 243, 205);
                    e.CellStyle.ForeColor = Color.FromArgb(255, 140, 0);
                    e.Value = $"⚠️ {stock}";
                }
                else if (stock <= minStock * 2)
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 249, 196);
                    e.CellStyle.ForeColor = Color.FromArgb(197, 138, 4);
                    e.Value = $"⚡ {stock}";
                }
                else
                {
                    e.CellStyle.BackColor = Color.FromArgb(212, 237, 218);
                    e.CellStyle.ForeColor = Color.FromArgb(25, 135, 84);
                    e.Value = $"✅ {stock}";
                }

                e.CellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.FormattingApplied = true;
            }

            if (grid.Columns[e.ColumnIndex].Name == "Name")
            {
                if (product.Stock <= 0)
                {
                    e.CellStyle.ForeColor = Color.FromArgb(108, 117, 125);
                    e.CellStyle.Font = new Font("Segoe UI", 11, FontStyle.Italic);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.FromArgb(44, 62, 80);
                    e.CellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                }
            }

            if (e.RowIndex % 2 == 0)
            {
                if (grid.Columns[e.ColumnIndex].Name != "Stock")
                {
                    e.CellStyle.BackColor = Color.White;
                }
            }
            else
            {
                if (grid.Columns[e.ColumnIndex].Name != "Stock")
                {
                    e.CellStyle.BackColor = Color.FromArgb(248, 250, 252);
                }
            }
        }

        private void AddProductToCart(ProductDto product)
        {
            if (product.Stock <= 0)
            {
                MessageBox.Show($"Product '{product.Name}' is out of stock and cannot be added to cart.",
                    "Out of Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CartItem? existingItem = null;
            int existingIndex = -1;
            for (int i = 0; i < _cartItems.Count; i++)
            {
                if (_cartItems[i].ProductId == product.Id)
                {
                    existingItem = _cartItems[i];
                    existingIndex = i;
                    break;
                }
            }

            if (existingItem != null)
            {
                if (existingItem.Quantity < product.Stock)
                {
                    existingItem.Quantity++;
                    UpdateTotals();
                    UpdateCartOrderNumbers();

                    // Focus the row and quantity cell for quick adjustment
                    dgvCart.ClearSelection();
                    if (existingIndex >= 0 && existingIndex < dgvCart.Rows.Count)
                    {
                        dgvCart.Rows[existingIndex].Selected = true;
                        dgvCart.CurrentCell = dgvCart.Rows[existingIndex].Cells["Quantity"];
                        dgvCart.BeginEdit(true);
                    }

                    if (existingItem.Quantity >= product.Stock * 0.8)
                    {
                        var remaining = product.Stock - existingItem.Quantity;
                        if (remaining > 0)
                        {
                            MessageBox.Show($"Low stock warning: Only {remaining} more '{product.Name}' available after this addition.",
                                "Low Stock Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Cannot add more '{product.Name}'. Only {product.Stock} items in stock.",
                        "Stock Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                if (product.Stock <= product.MinStock)
                {
                    var result = MessageBox.Show($"Warning: '{product.Name}' is low in stock ({product.Stock} remaining).\n\nDo you want to add it to cart?",
                        "Low Stock Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                        return;
                }

                _cartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name ?? string.Empty,
                    ProductSKU = product.SKU ?? string.Empty,
                    UnitPrice = product.Price,
                    Quantity = 1,
                    MaxStock = product.Stock
                });
                UpdateTotals();
                UpdateCartOrderNumbers();

                // Focus the new row for quick quantity adjustment
                int newIndex = _cartItems.Count - 1;
                dgvCart.ClearSelection();
                if (newIndex >= 0 && newIndex < dgvCart.Rows.Count)
                {
                    dgvCart.Rows[newIndex].Selected = true;
                    dgvCart.CurrentCell = dgvCart.Rows[newIndex].Cells["Quantity"];
                    dgvCart.BeginEdit(true);
                }
            }
        }

        private void UpdateTotals()
        {
            decimal subtotal = 0;
            for (int i = 0; i < _cartItems.Count; i++)
                subtotal += _cartItems[i].Total;
            var tax = subtotal * _taxRate;
            var total = subtotal + tax;

            lblSubtotal.Text = $"📊 Subtotal: {subtotal:C}";
            lblTax.Text = $"📋 Tax ({_taxRate:P0}): {tax:C}";
            lblTotal.Text = $"💰 Total: {total:C}";

            // Update cart count
            var itemCount = _cartItems.Count;
            var totalQuantity = _cartItems.Sum(item => item.Quantity);
            
            if (lblCartCount != null)
            {
                lblCartCount.Text = itemCount == 0 ? "Empty cart" : 
                    itemCount == 1 ? "1 item" : $"{itemCount} items ({totalQuantity} qty)";
            }

            nudPaidAmount.Value = total;
            UpdateChange();
        }

        private void UpdateChange()
        {
            decimal total = 0;
            for (int i = 0; i < _cartItems.Count; i++)
                total += _cartItems[i].Total;
            total *= (1 + _taxRate);
            var change = nudPaidAmount.Value - total;
            lblChange.Text = $"Change: {change:C}";
            lblChange.ForeColor = change >= 0 ? Color.FromArgb(46, 204, 113) : Color.Red;
        }

        private void RefreshCartDisplay()
        {
            try
            {
                dgvCart.CellValueChanged -= DgvCart_CellValueChanged;
                dgvCart.UserDeletingRow -= DgvCart_UserDeletingRow;
                dgvCart.DataError -= DgvCart_DataError;

                dgvCart.DataSource = null;
                dgvCart.Columns.Clear();

                SetupCartGrid();

                dgvCart.CellValueChanged += DgvCart_CellValueChanged;
                dgvCart.UserDeletingRow += DgvCart_UserDeletingRow;
                dgvCart.DataError += DgvCart_DataError;

                UpdateTotals();
                UpdateCartOrderNumbers();

                _logger.LogInformation("Cart display refreshed successfully with {Count} items", _cartItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cart display");
                try { UpdateTotals(); } catch { }
            }
        }

        #region Event Handlers

        private void DgvCart_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                _logger.LogWarning("DataGridView data error at Row: {Row}, Column: {Column}, Error: {Error}",
                    e.RowIndex, e.ColumnIndex, e.Exception?.Message);

                e.ThrowException = false;

                if (e.RowIndex >= 0 && e.RowIndex < _cartItems.Count)
                {
                    RefreshCartDisplay();
                }
                else
                {
                    _logger.LogError("Cart data binding issue - row index {RowIndex} exceeds cart items count {Count}",
                        e.RowIndex, _cartItems.Count);
                    RefreshCartDisplay();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling DataGridView data error");
            }
        }

        private void TxtProductSearch_TextChanged(object? sender, EventArgs e)
        {
            UpdateProductGrid();
        }

        private void BtnAddProduct_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                var product = dgvProducts.SelectedRows[0].DataBoundItem as ProductDto;
                if (product != null)
                {
                    AddProductToCart(product);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to add.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DgvProducts_DoubleClick(object? sender, EventArgs e)
        {
            BtnAddProduct_Click(sender, e);
        }

        private async void BtnRefreshData_Click(object? sender, EventArgs e)
        {
            try
            {
                btnRefreshData.Enabled = false;
                btnRefreshData.Text = "Refreshing...";

                var oldProductCount = _products.Count;

                await LoadCustomersAsync();
                await LoadProductsAsync();

                var newProductCount = _products.Count;
                var stockStatusMessage = "";

                if (newProductCount != oldProductCount)
                {
                    var difference = newProductCount - oldProductCount;
                    if (difference > 0)
                    {
                        stockStatusMessage = $"\n{difference} new product(s) now available for sale.";
                    }
                    else if (difference < 0)
                    {
                        stockStatusMessage = $"\n{Math.Abs(difference)} product(s) are now out of stock.";
                    }
                }

                MessageBox.Show($"Data refreshed successfully!\n\nProducts available: {newProductCount}{stockStatusMessage}",
                    "Refresh Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing data");
                MessageBox.Show($"Error refreshing data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefreshData.Enabled = true;
                btnRefreshData.Text = "Refresh";
            }
        }

        private void DgvCart_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                    e.RowIndex < _cartItems.Count && e.ColumnIndex < dgvCart.Columns.Count)
                {
                    var columnName = dgvCart.Columns[e.ColumnIndex].Name;
                    if (columnName == "Quantity")
                    {
                        var item = _cartItems[e.RowIndex];
                        if (item.Quantity > item.MaxStock)
                        {
                            item.Quantity = item.MaxStock;
                            MessageBox.Show($"Only {item.MaxStock} items in stock.",
                                "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else if (item.Quantity < 1)
                        {
                            item.Quantity = 1;
                        }
                        UpdateTotals();
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                _logger.LogError(ex, "Index out of range in cart cell value changed. RowIndex: {RowIndex}, ColumnIndex: {ColumnIndex}, CartItems Count: {Count}",
                    e.RowIndex, e.ColumnIndex, _cartItems.Count);
                RefreshCartDisplay();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cart cell value changed");
            }
        }

        private void DgvCart_UserDeletingRow(object? sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                if (e.Row != null && e.Row.Index >= 0 && e.Row.Index < _cartItems.Count)
                {
                    this.BeginInvoke(new Action(() => UpdateTotals()));
                }
                else
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in user deleting row from cart");
                e.Cancel = true;
            }
        }

        private void NudPaidAmount_ValueChanged(object? sender, EventArgs e)
        {
            UpdateChange();
        }

        private async void BtnNewCustomer_Click(object? sender, EventArgs e)
        {
            using (var dialog = new CustomerEditDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK && dialog.Customer != null)
                {
                    try
                    {
                        var customerDto = new CustomerDto
                        {
                            Name = dialog.Customer.Name,
                            Email = dialog.Customer.Email,
                            Phone = dialog.Customer.Phone,
                            Address = dialog.Customer.Address
                        };

                        var response = await _apiService.CreateCustomerAsync(customerDto);
                        if (response.Success && response.Data != null)
                        {
                            MessageBox.Show($"Customer '{response.Data.Name}' added successfully!",
                                "Customer Added", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            await LoadCustomersAsync();

                            SalesDataChanged?.Invoke(this, EventArgs.Empty);

                            RunOnUiThread(() => SelectCustomerById(response.Data.Id));
                        }
                        else
                        {
                            MessageBox.Show($"Failed to add customer: {response.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error adding new customer");
                        MessageBox.Show($"Error adding customer: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void BtnCompleteSale_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_cartItems.Count == 0)
                {
                    MessageBox.Show("Cart is empty. Please add products to complete a sale.",
                        "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dgvCart.Rows.Count != _cartItems.Count)
                {
                    _logger.LogWarning("Cart display out of sync. Refreshing before sale completion.");
                    RefreshCartDisplay();

                    await Task.Delay(100);
                }

                foreach (var item in _cartItems)
                {
                    if (item.Quantity <= 0 || item.UnitPrice <= 0)
                    {
                        MessageBox.Show($"Invalid item in cart: {item.ProductName}. Please remove and re-add.",
                            "Invalid Cart Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                btnCompleteSale.Enabled = false;
                btnCompleteSale.Text = "Validating Stock...";

                var stockValidationErrors = new List<string>();
                foreach (var item in _cartItems)
                {
                    try
                    {
                        var productResponse = await _apiService.GetProductByIdAsync(item.ProductId);
                        if (productResponse.Success && productResponse.Data != null)
                        {
                            var currentStock = productResponse.Data.Stock;
                            if (currentStock < item.Quantity)
                            {
                                stockValidationErrors.Add($"'{item.ProductName}': Need {item.Quantity}, but only {currentStock} available");
                            }
                        }
                        else
                        {
                            stockValidationErrors.Add($"'{item.ProductName}': Product not found or unavailable");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error validating stock for product {ProductId}", item.ProductId);
                        stockValidationErrors.Add($"'{item.ProductName}': Unable to verify stock");
                    }
                }

                btnCompleteSale.Enabled = true;
                btnCompleteSale.Text = "Complete Sale";

                if (stockValidationErrors.Any())
                {
                    var errorMessage = "Stock validation failed:\n\n" + string.Join("\n", stockValidationErrors) +
                        "\n\nPlease refresh the product data and adjust quantities.";
                    MessageBox.Show(errorMessage, "Stock Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal total = 0;
                for (int i = 0; i < _cartItems.Count; i++)
                    total += _cartItems[i].Total;
                total *= (1 + _taxRate);

                if (nudPaidAmount.Value < total)
                {
                    MessageBox.Show("Paid amount is less than total amount.",
                        "Insufficient Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var customerId = 1;
                if (cboCustomer.SelectedItem is CustomerDto customer)
                {
                    customerId = customer.Id;
                }

                var sale = new CreateSaleDto
                {
                    CustomerId = customerId,
                    PaymentMethod = cboPaymentMethod.Text,
                    PaidAmount = nudPaidAmount.Value,
                    Notes = "",
                    Items = _cartItems.Select(i => new CreateSaleItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductSKU = i.ProductSKU,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = 0
                    }).ToList()
                };

                var response = await _apiService.CreateSaleAsync(sale).ConfigureAwait(false);
                if (response.Success)
                {
                    SalesDataChanged?.Invoke(this, EventArgs.Empty);

                    var saleId = response.Data?.Id ?? 0;
                    var changeAmount = nudPaidAmount.Value - total;
                    var saleData = response.Data;

                    RunOnUiThread(() =>
                    {
                        var confirmDialog = new InvoiceConfirmationDialog(saleId, changeAmount);
                        var result = confirmDialog.ShowDialog(this);

                        if (result == DialogResult.Yes && saleData != null)
                        {
                            try
                            {
                                ShowInvoiceForm(saleData);
                            }
                            catch (Exception invoiceEx)
                            {
                                _logger.LogError(invoiceEx, "Error opening invoice form: {Error}", invoiceEx.Message);
                                MessageBox.Show($"Error opening invoice form: {invoiceEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        ClearCartAndResetForm();
                    });

                    await LoadProductsAsync().ConfigureAwait(false);
                }
                else
                {
                    if (response.Message != null && response.Message.Contains("Insufficient stock"))
                    {
                        var stockError = response.Message;
                        var userMessage = "Stock validation failed during sale processing:\n\n" + stockError +
                            "\n\nThis can happen if another user purchased the same product simultaneously." +
                            "\n\nPlease refresh the product data and try again.";

                        MessageBox.Show(userMessage, "Stock Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        _ = Task.Run(async () =>
                        {
                            await LoadProductsAsync();
                            RunOnUiThread(() => MessageBox.Show("Product data refreshed. Please check quantities and try again.",
                                "Data Refreshed", MessageBoxButtons.OK, MessageBoxIcon.Information));
                        });
                    }
                    else
                    {
                        MessageBox.Show($"Error completing sale: {response.Message}",
                            "Sale Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing sale: {Error}", ex.Message);
                RunOnUiThread(() => HandleSaleCompletionError(ex));
            }
        }

        private void BtnCancelSale_Click(object? sender, EventArgs e)
        {
            if (_cartItems.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to cancel this sale?",
                    "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _cartItems.Clear();
                    cboCustomer.SelectedIndex = 0;
                    cboPaymentMethod.SelectedIndex = 0;
                    UpdateTotals();
                }
            }
        }

        private void ShowInvoiceForm(SaleDto sale)
        {
            try
            {
                var invoiceForm = Program.GetRequiredService<InvoiceForm>();
                invoiceForm.LoadSaleData(sale);
                invoiceForm.Show();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ShowInvoiceForm method");
                throw;
            }
        }

        private void ClearCartAndResetForm()
        {
            try
            {
                _cartItems.Clear();
                cboCustomer.SelectedIndex = 0;
                cboPaymentMethod.SelectedIndex = 0;
                UpdateTotals();
                _logger.LogInformation("Cart cleared and form reset after successful sale");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart and resetting form");
            }
        }

        private void HandleSaleCompletionError(Exception ex)
        {
            try
            {
                MessageBox.Show($"Error completing sale: {ex.Message}\n\nThe sale may not have been saved. Please check your cart and try again.",
                    "Sale Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                try
                {
                    RefreshCartDisplay();
                }
                catch (Exception refreshEx)
                {
                    _logger.LogError(refreshEx, "Error refreshing cart display after sale completion error");
                }
            }
            catch (Exception handlerEx)
            {
                _logger.LogError(handlerEx, "Error in HandleSaleCompletionError method");
            }
        }

        private async void BtnGenerateInvoice_Click(object? sender, EventArgs e)
        {
            // First complete the sale, then automatically generate invoice
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Cart is empty. Please add products before generating an invoice.",
                    "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Complete the sale first
                await CompleteSaleAndGenerateInvoice();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice");
                MessageBox.Show($"Error generating invoice: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CompleteSaleAndGenerateInvoice()
        {
            // This is similar to BtnCompleteSale_Click but automatically generates invoice
            try
            {
                // Validate cart and payment details (same as complete sale logic)
                if (_cartItems.Count == 0)
                {
                    MessageBox.Show("Cart is empty. Please add products to complete a sale.",
                        "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate totals
                decimal total = 0;
                for (int i = 0; i < _cartItems.Count; i++)
                    total += _cartItems[i].Total;
                total *= (1 + _taxRate);

                if (nudPaidAmount.Value < total)
                {
                    MessageBox.Show("Paid amount is less than total amount.",
                        "Insufficient Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Complete the sale
                var customerId = 1;
                if (cboCustomer.SelectedItem is CustomerDto customer)
                {
                    customerId = customer.Id;
                }

                var sale = new CreateSaleDto
                {
                    CustomerId = customerId,
                    PaymentMethod = cboPaymentMethod.Text,
                    PaidAmount = nudPaidAmount.Value,
                    Notes = "Invoice Generated",
                    Items = _cartItems.Select(i => new CreateSaleItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductSKU = i.ProductSKU,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = 0
                    }).ToList()
                };

                var response = await _apiService.CreateSaleAsync(sale).ConfigureAwait(false);
                if (response.Success && response.Data != null)
                {
                    SalesDataChanged?.Invoke(this, EventArgs.Empty);

                    RunOnUiThread(() =>
                    {
                        MessageBox.Show("Sale completed successfully! Generating invoice...",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Automatically show invoice
                        try
                        {
                            ShowInvoiceForm(response.Data);
                        }
                        catch (Exception invoiceEx)
                        {
                            _logger.LogError(invoiceEx, "Error opening invoice form");
                            MessageBox.Show($"Sale completed but error opening invoice: {invoiceEx.Message}",
                                "Invoice Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        ClearCartAndResetForm();
                    });

                    await LoadProductsAsync().ConfigureAwait(false);
                }
                else
                {
                    MessageBox.Show($"Error completing sale: {response.Message}",
                        "Sale Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CompleteSaleAndGenerateInvoice");
                throw;
            }
        }

        #endregion
    }

    /// <summary>
    /// Cart item model
    /// </summary>
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Total)));
                }
            }
        }

        public decimal Total => UnitPrice * Quantity;
        public int MaxStock { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// Modern, professional invoice confirmation dialog
    /// </summary>
    public class InvoiceConfirmationDialog : Form
    {
        private readonly int _saleId;
        private readonly decimal _changeAmount;
        private Button btnGenerateInvoice = null!;
        private Button btnNoThanks = null!;
        private Panel pnlHeader = null!;
        private Panel pnlContent = null!;
        private Panel pnlButtons = null!;
        private Label lblTitle = null!;
        private Label lblMessage = null!;
        private Label lblSaleId = null!;
        private Label lblChange = null!;
        private PictureBox picIcon = null!;
        private System.Windows.Forms.Timer _animationTimer = null!;

        public InvoiceConfirmationDialog(int saleId, decimal changeAmount)
        {
            _saleId = saleId;
            _changeAmount = changeAmount;
            InitializeDialog();
            UpdateDataLabels();
        }

        private void UpdateDataLabels()
        {
            if (lblSaleId != null)
            {
                lblSaleId.Text = $"📋 Sale ID: #{_saleId:D6}";
            }
            
            if (lblChange != null)
            {
                lblChange.Text = $"💰 Change Due: {_changeAmount:C}";
                lblChange.ForeColor = _changeAmount >= 0 ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
            }
        }

        private void InitializeDialog()
        {
            // Form properties
            this.Text = "Sale Completed Successfully";
            this.Size = new Size(500, 430);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.ShowInTaskbar = false;
            this.TopMost = true; // Ensure it appears on top
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Add drop shadow effect
            this.Paint += (s, e) =>
            {
                var shadowSize = 8;
                var shadowColor = Color.FromArgb(50, 0, 0, 0);
                using (var brush = new SolidBrush(shadowColor))
                {
                    e.Graphics.FillRectangle(brush, shadowSize, shadowSize, this.Width - shadowSize, this.Height - shadowSize);
                }
                e.Graphics.FillRectangle(Brushes.White, 0, 0, this.Width - shadowSize, this.Height - shadowSize);
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - shadowSize - 1, this.Height - shadowSize - 1);
                }
            };

            // Header panel with gradient background
            pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(492, 80),
                BackColor = Color.FromArgb(40, 167, 69)
            };
            pnlHeader.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, pnlHeader.Width, pnlHeader.Height),
                    Color.FromArgb(40, 167, 69),
                    Color.FromArgb(25, 135, 84),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, pnlHeader.Width, pnlHeader.Height);
                }
            };

            // Success icon
            picIcon = new PictureBox
            {
                Location = new Point(30, 20),
                Size = new Size(40, 40),
                BackColor = Color.Transparent
            };
            picIcon.Paint += (s, e) =>
            {
                // Draw a checkmark circle
                using (var brush = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.White, 3))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(brush, 5, 5, 30, 30);
                    using (var greenBrush = new SolidBrush(Color.FromArgb(40, 167, 69)))
                    {
                        e.Graphics.FillEllipse(greenBrush, 7, 7, 26, 26);
                    }
                    // Draw checkmark
                    e.Graphics.DrawLines(pen, new Point[] {
                        new Point(15, 20), new Point(18, 23), new Point(25, 16)
                    });
                }
            };

            // Title label
            lblTitle = new Label
            {
                Text = "🎉 Sale Completed Successfully!",
                Location = new Point(85, 20),
                Size = new Size(350, 35),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            pnlHeader.Controls.AddRange(new Control[] { picIcon, lblTitle });

            // Content panel
            pnlContent = new Panel
            {
                Location = new Point(0, 80),
                Size = new Size(492, 250),
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            // Main message
            lblMessage = new Label
            {
                Text = "Your transaction has been processed successfully!\nWould you like to generate an invoice for this sale?",
                Location = new Point(30, 30),
                Size = new Size(430, 60),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.TopLeft
            };

            // Sale ID display
            var pnlSaleInfo = new Panel
            {
                Location = new Point(30, 100),
                Size = new Size(430, 100),
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20)
            };
            pnlSaleInfo.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlSaleInfo.Width - 1, pnlSaleInfo.Height - 1);
                }
            };

            lblSaleId = new Label
            {
                Text = "📋 Sale ID: Loading...",
                Location = new Point(20, 20),
                Size = new Size(390, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblChange = new Label
            {
                Text = "💰 Change Due: Loading...",
                Location = new Point(20, 50),
                Size = new Size(390, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            pnlSaleInfo.Controls.AddRange(new Control[] { lblSaleId, lblChange });
            pnlContent.Controls.AddRange(new Control[] { lblMessage, pnlSaleInfo });

            // Buttons panel
            pnlButtons = new Panel
            {
                Location = new Point(0, 330),
                Size = new Size(492, 90),
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(40, 25, 40, 25)
            };
            pnlButtons.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawLine(pen, 0, 0, pnlButtons.Width, 0);
                }
            };

            // Generate Invoice button (primary)
            btnGenerateInvoice = new Button
            {
                Text = "📄 Generate Invoice",
                Location = new Point(170, 25),
                Size = new Size(180, 45),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Yes,
                Cursor = Cursors.Hand
            };
            btnGenerateInvoice.FlatAppearance.BorderSize = 0;
            btnGenerateInvoice.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 105, 217);
            btnGenerateInvoice.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 86, 179);

            // No Thanks button (secondary)
            btnNoThanks = new Button
            {
                Text = "❌ No Thanks",
                Location = new Point(365, 25),
                Size = new Size(120, 45),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.No,
                Cursor = Cursors.Hand
            };
            btnNoThanks.FlatAppearance.BorderSize = 0;
            btnNoThanks.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 98, 104);
            btnNoThanks.FlatAppearance.MouseDownBackColor = Color.FromArgb(73, 80, 87);

            // Add hover effects
            btnGenerateInvoice.MouseEnter += (s, e) => btnGenerateInvoice.BackColor = Color.FromArgb(0, 105, 217);
            btnGenerateInvoice.MouseLeave += (s, e) => btnGenerateInvoice.BackColor = Color.FromArgb(0, 123, 255);
            btnNoThanks.MouseEnter += (s, e) => btnNoThanks.BackColor = Color.FromArgb(90, 98, 104);
            btnNoThanks.MouseLeave += (s, e) => btnNoThanks.BackColor = Color.FromArgb(108, 117, 125);

            pnlButtons.Controls.AddRange(new Control[] { btnGenerateInvoice, btnNoThanks });

            // Add all panels to form
            this.Controls.AddRange(new Control[] { pnlHeader, pnlContent, pnlButtons });

            // Set up animation timer for entrance effect
            _animationTimer = new System.Windows.Forms.Timer { Interval = 16 }; // ~60 FPS
            var animationStep = 0;
            _animationTimer.Tick += (s, e) =>
            {
                animationStep++;
                if (animationStep <= 10)
                {
                    // Slide in from top with fade
                    this.Opacity = animationStep * 0.1;
                    this.Top = this.Owner?.Top + 50 - (animationStep * 5) ?? this.Top;
                }
                else
                {
                    _animationTimer.Stop();
                    this.Opacity = 1.0;
                }
            };

            // Set default buttons
            this.AcceptButton = btnGenerateInvoice;
            this.CancelButton = btnNoThanks;

            // Handle form activation to ensure it stays on top
            this.Activated += (s, e) => this.BringToFront();
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(value);
            if (value && !this.IsDisposed)
            {
                this.BringToFront();
                this.Focus();
                this.TopMost = true;
                
                // Start entrance animation
                if (_animationTimer != null && !_animationTimer.Enabled)
                {
                    this.Opacity = 0;
                    _animationTimer.Start();
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.BringToFront();
            this.Focus();
            btnGenerateInvoice.Focus(); // Focus the primary button
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}