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
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create main layout
            var pnlMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            pnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Left panel - Product selection
            pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Customer selection section with modern styling
            lblCustomer = new Label
            {
                Text = "👤 Customer:",
                Location = new Point(10, 10),
                Size = new Size(90, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            cboCustomer = new ComboBox
            {
                Location = new Point(105, 10),
                Size = new Size(280, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White
            };

            btnNewCustomer = new Button
            {
                Text = "👥 New Customer",
                Location = new Point(395, 8),
                Size = new Size(135, 32),
                BackColor = Color.FromArgb(102, 16, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnNewCustomer.FlatAppearance.BorderSize = 0;
            btnNewCustomer.FlatAppearance.MouseOverBackColor = Color.FromArgb(81, 12, 194);
            btnNewCustomer.Click += BtnNewCustomer_Click;

            // Product search section with modern styling
            lblProduct = new Label
            {
                Text = "🔍 Product Search:",
                Location = new Point(10, 50),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            txtProductSearch = new TextBox
            {
                Location = new Point(10, 80),
                Size = new Size(300, 28),
                PlaceholderText = "Search by product name or SKU code...",
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            txtProductSearch.TextChanged += TxtProductSearch_TextChanged;

            btnAddProduct = new Button
            {
                Text = "➕ Add to Cart",
                Location = new Point(320, 78),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnAddProduct.FlatAppearance.BorderSize = 0;
            btnAddProduct.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 142, 58);
            btnAddProduct.Click += BtnAddProduct_Click;

            btnRefreshData = new Button
            {
                Text = "Loading...",
                Location = new Point(440, 78),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Enabled = false
            };
            btnRefreshData.FlatAppearance.BorderSize = 0;
            btnRefreshData.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 105, 217);
            btnRefreshData.Click += BtnRefreshData_Click;

            // Products grid with premium sizing and layout
            dgvProducts = new DataGridView
            {
                Location = new Point(10, 125),
                Size = new Size(540, 385),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ScrollBars = ScrollBars.Both,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(5)
            };
            dgvProducts.DoubleClick += DgvProducts_DoubleClick;
            dgvProducts.CellFormatting += DgvProducts_CellFormatting;

            pnlLeft.Controls.AddRange(new Control[] {
                lblCustomer, cboCustomer, btnNewCustomer,
                lblProduct, txtProductSearch, btnAddProduct, btnRefreshData,
                dgvProducts
            });

            // Right panel - Shopping cart
            pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(248, 248, 248)
            };

            lblCart = new Label
            {
                Text = "🛒 Shopping Cart",
                Location = new Point(10, 10),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            // Cart grid
            dgvCart = new DataGridView
            {
                Location = new Point(10, 50),
                Size = new Size(550, 300),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvCart.CellValueChanged += DgvCart_CellValueChanged;
            dgvCart.UserDeletingRow += DgvCart_UserDeletingRow;

            // Payment section
            var pnlPayment = new Panel
            {
                Location = new Point(10, 360),
                Size = new Size(550, 200),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblSubtotal = new Label
            {
                Text = "📊 Subtotal: $0.00",
                Location = new Point(10, 10),
                Size = new Size(220, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            lblTax = new Label
            {
                Text = "📋 Tax (10%): $0.00",
                Location = new Point(10, 40),
                Size = new Size(220, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            lblTotal = new Label
            {
                Text = "💰 Total: $0.00",
                Location = new Point(10, 70),
                Size = new Size(220, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69)
            };

            lblPaymentMethod = new Label
            {
                Text = "💳 Payment Method:",
                Location = new Point(250, 10),
                Size = new Size(140, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            cboPaymentMethod = new ComboBox
            {
                Location = new Point(400, 10),
                Size = new Size(140, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White
            };
            cboPaymentMethod.Items.AddRange(new object[] { "💵 Cash", "💳 Credit Card", "💸 Debit Card", "📝 Check" });
            cboPaymentMethod.SelectedIndex = 0;

            lblPaidAmount = new Label
            {
                Text = "💵 Paid Amount:",
                Location = new Point(250, 40),
                Size = new Size(140, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            nudPaidAmount = new NumericUpDown
            {
                Location = new Point(400, 40),
                Size = new Size(140, 25),
                DecimalPlaces = 2,
                Maximum = 999999.99m,
                Minimum = 0,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White
            };
            nudPaidAmount.ValueChanged += NudPaidAmount_ValueChanged;

            lblChange = new Label
            {
                Text = "💸 Change: $0.00",
                Location = new Point(250, 70),
                Size = new Size(290, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 162, 184)
            };

            btnCompleteSale = new Button
            {
                Text = "✅ Complete Sale",
                Location = new Point(250, 120),
                Size = new Size(160, 45),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnCompleteSale.FlatAppearance.BorderSize = 0;
            btnCompleteSale.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 142, 58);
            btnCompleteSale.Click += BtnCompleteSale_Click;

            btnCancelSale = new Button
            {
                Text = "❌ Cancel Sale",
                Location = new Point(420, 120),
                Size = new Size(140, 45),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnCancelSale.FlatAppearance.BorderSize = 0;
            btnCancelSale.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 35, 51);
            btnCancelSale.Click += BtnCancelSale_Click;

            pnlPayment.Controls.AddRange(new Control[] {
                lblSubtotal, lblTax, lblTotal,
                lblPaymentMethod, cboPaymentMethod,
                lblPaidAmount, nudPaidAmount, lblChange,
                btnCompleteSale, btnCancelSale
            });

            pnlRight.Controls.AddRange(new Control[] { lblCart, dgvCart, pnlPayment });

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

                // Add columns first
                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "ProductId",
                    HeaderText = "ID",
                    DataPropertyName = "ProductId",
                    Width = 50,
                    ReadOnly = true
                });

                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "ProductName",
                    HeaderText = "Product",
                    DataPropertyName = "ProductName",
                    Width = 200,
                    ReadOnly = true
                });

                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "UnitPrice",
                    HeaderText = "Price",
                    DataPropertyName = "UnitPrice",
                    Width = 80,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
                    ReadOnly = true
                });

                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Quantity",
                    HeaderText = "Qty",
                    DataPropertyName = "Quantity",
                    Width = 60
                });

                dgvCart.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Total",
                    HeaderText = "Total",
                    DataPropertyName = "Total",
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
                    ReadOnly = true
                });

                // Set data source after columns are configured
                dgvCart.DataSource = _cartItems;

                // Add data error handler to gracefully handle any binding issues
                dgvCart.DataError += DgvCart_DataError;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up cart grid");
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
                if (dgvProducts.Columns.Contains("Id"))
                {
                    var column = dgvProducts.Columns["Id"];
                    if (column != null)
                    {
                        column.Width = 95;
                        column.HeaderText = "🆔 ID";
                        column.MinimumWidth = 85;
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        column.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                        column.DefaultCellStyle.ForeColor = Color.FromArgb(94, 108, 132);
                        column.DefaultCellStyle.Padding = new Padding(15, 18, 15, 18);
                        column.DefaultCellStyle.BackColor = Color.FromArgb(250, 251, 252);
                    }
                }

                if (dgvProducts.Columns.Contains("Name"))
                {
                    var column = dgvProducts.Columns["Name"];
                    if (column != null)
                    {
                        column.Width = 320;
                        column.HeaderText = "📦 Product Name";
                        column.MinimumWidth = 280;
                        column.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                        column.DefaultCellStyle.ForeColor = Color.FromArgb(44, 62, 80);
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        column.DefaultCellStyle.Padding = new Padding(20, 18, 18, 18);
                    }
                }

                if (dgvProducts.Columns.Contains("SKU"))
                {
                    var column = dgvProducts.Columns["SKU"];
                    if (column != null)
                    {
                        column.Width = 170;
                        column.HeaderText = "🏷️ SKU Code";
                        column.MinimumWidth = 150;
                        column.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                        column.DefaultCellStyle.ForeColor = Color.FromArgb(108, 117, 125);
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        column.DefaultCellStyle.Padding = new Padding(18, 18, 18, 18);
                        column.DefaultCellStyle.BackColor = Color.FromArgb(253, 254, 255);
                    }
                }

                if (dgvProducts.Columns.Contains("Price"))
                {
                    var column = dgvProducts.Columns["Price"];
                    if (column != null)
                    {
                        column.DefaultCellStyle.Format = "C2";
                        column.Width = 140;
                        column.HeaderText = "💰 Price";
                        column.MinimumWidth = 120;
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                        column.DefaultCellStyle.ForeColor = Color.FromArgb(46, 125, 50);
                        column.DefaultCellStyle.Padding = new Padding(18, 18, 25, 18);
                        column.DefaultCellStyle.BackColor = Color.FromArgb(248, 255, 248);
                    }
                }

                if (dgvProducts.Columns.Contains("Stock"))
                {
                    var column = dgvProducts.Columns["Stock"];
                    if (column != null)
                    {
                        column.Width = 120;
                        column.HeaderText = "📊 Stock";
                        column.MinimumWidth = 100;
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        column.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                        column.DefaultCellStyle.Padding = new Padding(15, 18, 15, 18);
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

            lblSubtotal.Text = $"Subtotal: {subtotal:C}";
            lblTax.Text = $"Tax ({_taxRate:P0}): {tax:C}";
            lblTotal.Text = $"Total: {total:C}";

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