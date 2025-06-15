using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

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
            // Use Task.Run to avoid async void and not block UI thread
            Task.Run(() => InitializeAsync()).ConfigureAwait(false);
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

            // Customer selection section
            lblCustomer = new Label
                {
                Text = "Customer:",
                Location = new Point(10, 10),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };

            cboCustomer = new ComboBox
                {
                Location = new Point(100, 10),
                Size = new Size(300, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
                };

            btnNewCustomer = new Button
                {
                Text = "New",
                Location = new Point(410, 9),
                Size = new Size(60, 27),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
                };
            btnNewCustomer.FlatAppearance.BorderSize = 0;
            btnNewCustomer.Click += BtnNewCustomer_Click;

            // Product search section
            lblProduct = new Label
                {
                Text = "Product Search:",
                Location = new Point(10, 50),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };

            txtProductSearch = new TextBox
                {
                Location = new Point(10, 80),
                Size = new Size(380, 25),
                PlaceholderText = "Enter product name or SKU..."
                };
            txtProductSearch.TextChanged += TxtProductSearch_TextChanged;

            btnAddProduct = new Button
                {
                Text = "Add",
                Location = new Point(400, 79),
                Size = new Size(60, 27),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
                };
            btnAddProduct.FlatAppearance.BorderSize = 0;
            btnAddProduct.Click += BtnAddProduct_Click;

            btnRefreshData = new Button
                {
                Text = "Refresh",
                Location = new Point(470, 79),
                Size = new Size(70, 27),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
                };
            btnRefreshData.FlatAppearance.BorderSize = 0;
            btnRefreshData.Click += BtnRefreshData_Click;

            // Products grid
            dgvProducts = new DataGridView
                {
                Location = new Point(10, 120),
                Size = new Size(460, 400),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };
            dgvProducts.DoubleClick += DgvProducts_DoubleClick;

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
                Text = "Shopping Cart",
                Location = new Point(10, 10),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51)
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
                Text = "Subtotal: $0.00",
                Location = new Point(10, 10),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11)
                };

            lblTax = new Label
                {
                Text = "Tax (10%): $0.00",
                Location = new Point(10, 40),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11)
                };

            lblTotal = new Label
                {
                Text = "Total: $0.00",
                Location = new Point(10, 70),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
                };

            lblPaymentMethod = new Label
                {
                Text = "Payment Method:",
                Location = new Point(250, 10),
                Size = new Size(120, 25)
                };

            cboPaymentMethod = new ComboBox
                {
                Location = new Point(380, 10),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
                };
            cboPaymentMethod.Items.AddRange(new object[] { "Cash", "Credit Card", "Debit Card", "Check" });
            cboPaymentMethod.SelectedIndex = 0;

            lblPaidAmount = new Label
                {
                Text = "Paid Amount:",
                Location = new Point(250, 40),
                Size = new Size(120, 25)
                };

            nudPaidAmount = new NumericUpDown
                {
                Location = new Point(380, 40),
                Size = new Size(150, 25),
                DecimalPlaces = 2,
                Maximum = 999999.99m,
                Minimum = 0
                };
            nudPaidAmount.ValueChanged += NudPaidAmount_ValueChanged;

            lblChange = new Label
                {
                Text = "Change: $0.00",
                Location = new Point(250, 70),
                Size = new Size(280, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113)
                };

            btnCompleteSale = new Button
                {
                Text = "Complete Sale",
                Location = new Point(250, 120),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
                };
            btnCompleteSale.FlatAppearance.BorderSize = 0;
            btnCompleteSale.Click += BtnCompleteSale_Click;

            btnCancelSale = new Button
                {
                Text = "Cancel",
                Location = new Point(400, 120),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11)
                };
            btnCancelSale.FlatAppearance.BorderSize = 0;
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

        private async Task InitializeAsync()
            {
            await LoadCustomersAsync().ConfigureAwait(false);
            await LoadProductsAsync().ConfigureAwait(false);
            // SetupCartGrid must be called on UI thread
            if (InvokeRequired)
                Invoke(new Action(SetupCartGrid));
            else
                SetupCartGrid();
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
                    // UpdateCustomerComboBox must be called on UI thread
                    if (InvokeRequired)
                        Invoke(new Action(UpdateCustomerComboBox));
                    else
                        UpdateCustomerComboBox();
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
                    // Use a for loop for better performance on large lists
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
                    
                    _products = inStock; // Only show in-stock products for selection
                    
                    // Log out-of-stock items for awareness
                    if (outOfStock.Count > 0)
                        {
                        _logger.LogInformation("Found {OutOfStockCount} out-of-stock products that are hidden from sales selection", 
                            outOfStock.Count);
                        }
                    
                    // UpdateProductGrid must be called on UI thread
                    if (InvokeRequired)
                        Invoke(new Action(UpdateProductGrid));
                    else
                        UpdateProductGrid();
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
                var temp = new List<ProductDto>(_products.Count);
                for (int i = 0; i < _products.Count; i++)
                    {
                    var p = _products[i];
                    if ((p.Name != null && p.Name.ToLowerInvariant().Contains(searchTerm)) ||
                        (p.SKU != null && p.SKU.ToLowerInvariant().Contains(searchTerm)))
                        {
                        temp.Add(p);
                        }
                    }
                filteredProducts = temp;
                }

            dgvProducts.DataSource = null;
            dgvProducts.DataSource = filteredProducts;

            if (dgvProducts.Columns != null && dgvProducts.Columns.Contains("Id"))
                {
                var column = dgvProducts.Columns["Id"];
                if (column != null)
                    column.Width = 50;
                }

            if (dgvProducts.Columns != null && dgvProducts.Columns.Count > 0)
                {
                if (dgvProducts.Columns.Contains("Name"))
                    {
                    var column = dgvProducts.Columns["Name"];
                    if (column != null)
                        column.Width = 200;
                    }
                if (dgvProducts.Columns.Contains("SKU"))
                    {
                    var column = dgvProducts.Columns["SKU"];
                    if (column != null)
                        column.Width = 100;
                    }
                if (dgvProducts.Columns.Contains("Price"))
                    {
                    var column = dgvProducts.Columns["Price"];
                    if (column != null)
                        column.DefaultCellStyle.Format = "C2";
                    }
                if (dgvProducts.Columns.Contains("Stock"))
                    {
                    var column = dgvProducts.Columns["Stock"];
                    if (column != null)
                        column.Width = 60;
                    }
                if (dgvProducts.Columns.Contains("CategoryId"))
                    {
                    var column = dgvProducts.Columns["CategoryId"];
                    if (column != null)
                        column.Visible = false;
                    }
                if (dgvProducts.Columns.Contains("CategoryName"))
                    {
                    var column = dgvProducts.Columns["CategoryName"];
                    if (column != null)
                        column.Visible = false;
                    }
                if (dgvProducts.Columns.Contains("CreatedAt"))
                    {
                    var column = dgvProducts.Columns["CreatedAt"];
                    if (column != null)
                        column.Visible = false;
                    }
                if (dgvProducts.Columns.Contains("UpdatedAt"))
                    {
                    var column = dgvProducts.Columns["UpdatedAt"];
                    if (column != null)
                        column.Visible = false;
                    }
                if (dgvProducts.Columns.Contains("MinStock"))
                    {
                    var column = dgvProducts.Columns["MinStock"];
                    if (column != null)
                        column.Visible = false;
                    }
                if (dgvProducts.Columns.Contains("IsActive"))
                    {
                    var column = dgvProducts.Columns["IsActive"];
                    if (column != null)
                        column.Visible = false;
                    }
                if (dgvProducts.Columns.Contains("ImageUrl"))
                    {
                    var column = dgvProducts.Columns["ImageUrl"];
                    if (column != null)
                        column.Visible = false;
                    }
                if (dgvProducts.Columns.Contains("Description"))
                    {
                    var column = dgvProducts.Columns["Description"];
                    if (column != null)
                        column.Visible = false;
                    }
                }
            }

        private void AddProductToCart(ProductDto product)
            {
            // Check if product has stock
            if (product.Stock <= 0)
                {
                MessageBox.Show($"Product '{product.Name}' is out of stock and cannot be added to cart.",
                    "Out of Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
                }

            CartItem? existingItem = null;
            for (int i = 0; i < _cartItems.Count; i++)
                {
                if (_cartItems[i].ProductId == product.Id)
                    {
                    existingItem = _cartItems[i];
                    break;
                    }
                }

            if (existingItem != null)
                {
                if (existingItem.Quantity < product.Stock)
                    {
                    existingItem.Quantity++;
                    
                    // Show low stock warning if approaching limit
                    if (existingItem.Quantity >= product.Stock * 0.8) // 80% of stock
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
                // Show low stock warning for new items if stock is low
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
                }

            UpdateTotals();
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
                // Temporarily remove event handlers to prevent recursive calls
                dgvCart.CellValueChanged -= DgvCart_CellValueChanged;
                dgvCart.UserDeletingRow -= DgvCart_UserDeletingRow;
                dgvCart.DataError -= DgvCart_DataError;
                
                // Clear everything and start fresh
                dgvCart.DataSource = null;
                dgvCart.Columns.Clear();
                
                // Re-setup the entire grid
                SetupCartGrid();
                
                // Re-attach event handlers
                dgvCart.CellValueChanged += DgvCart_CellValueChanged;
                dgvCart.UserDeletingRow += DgvCart_UserDeletingRow;
                dgvCart.DataError += DgvCart_DataError;
                
                UpdateTotals();
                
                _logger.LogInformation("Cart display refreshed successfully with {Count} items", _cartItems.Count);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error refreshing cart display");
                // If refresh fails, at least try to update totals
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
                
                // Handle the error gracefully
                e.ThrowException = false;
                
                // Try to refresh the cart display to fix any sync issues
                if (e.RowIndex >= 0 && e.RowIndex < _cartItems.Count)
                    {
                    // The row exists in data, refresh the display
                    RefreshCartDisplay();
                    }
                else
                    {
                    // Row doesn't exist in data, this is a binding issue
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
                
                // Refresh both customers and products
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
                // Refresh the cart display to sync with data
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
                // Validate the row index before proceeding
                if (e.Row != null && e.Row.Index >= 0 && e.Row.Index < _cartItems.Count)
                    {
                    // The row will be automatically removed from the BindingList
                    // We just need to update totals after the deletion
                    this.BeginInvoke(new Action(() => UpdateTotals()));
                    }
                else
                    {
                    // Cancel invalid deletion
                    e.Cancel = true;
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error in user deleting row from cart");
                e.Cancel = true; // Cancel the deletion on error
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
                        // Create the customer in the database
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

                            // Refresh the customer list
                            await LoadCustomersAsync();

                            // Notify dashboard of data change
                            SalesDataChanged?.Invoke(this, EventArgs.Empty);

                            // Select the newly added customer
                            if (InvokeRequired)
                                {
                                Invoke(new Action(() => SelectCustomerById(response.Data.Id)));
                                }
                            else
                                {
                                SelectCustomerById(response.Data.Id);
                                }
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
                // Validate cart state
                if (_cartItems.Count == 0)
                    {
                    MessageBox.Show("Cart is empty. Please add products to complete a sale.",
                        "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                    }

                // Ensure cart display is in sync before processing
                if (dgvCart.Rows.Count != _cartItems.Count)
                    {
                    _logger.LogWarning("Cart display out of sync. Refreshing before sale completion.");
                    RefreshCartDisplay();
                    
                    // Wait a moment for the refresh to complete
                    await Task.Delay(100);
                    }

                // Additional validation to ensure all cart items are valid
                foreach (var item in _cartItems)
                    {
                    if (item.Quantity <= 0 || item.UnitPrice <= 0)
                        {
                        MessageBox.Show($"Invalid item in cart: {item.ProductName}. Please remove and re-add.",
                            "Invalid Cart Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                        }
                    }

                // Real-time stock validation before sale completion
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

                var customerId = 1; // Default to walk-in customer
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
                    // Notify dashboard of data change
                    SalesDataChanged?.Invoke(this, EventArgs.Empty);

                    var result = MessageBox.Show($"Sale completed successfully!\n\nSale ID: {response.Data?.Id}\nChange: {(nudPaidAmount.Value - total):C}\n\nWould you like to generate an invoice?",
                        "Sale Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    // Show invoice if requested
                    if (result == DialogResult.Yes && response.Data != null)
                        {
                        try
                            {
                            // Ensure we're on the UI thread for form operations
                            if (InvokeRequired)
                                {
                                Invoke(new Action(() => ShowInvoiceForm(response.Data)));
                                }
                            else
                                {
                                ShowInvoiceForm(response.Data);
                                }
                            }
                        catch (Exception invoiceEx)
                            {
                            _logger.LogError(invoiceEx, "Error opening invoice form: {Error}", invoiceEx.Message);
                            MessageBox.Show($"Error opening invoice form: {invoiceEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                    // Clear cart and reset form - ensure UI operations are on UI thread
                    if (InvokeRequired)
                        {
                        Invoke(new Action(() => ClearCartAndResetForm()));
                        }
                    else
                        {
                        ClearCartAndResetForm();
                        }
                    
                    // Refresh stock levels
                    await LoadProductsAsync().ConfigureAwait(false);
                    }
                else
                    {
                    // Handle specific stock validation errors
                    if (response.Message != null && response.Message.Contains("Insufficient stock"))
                        {
                        var stockError = response.Message;
                        var userMessage = "Stock validation failed during sale processing:\n\n" + stockError + 
                            "\n\nThis can happen if another user purchased the same product simultaneously." +
                            "\n\nPlease refresh the product data and try again.";
                        
                        MessageBox.Show(userMessage, "Stock Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        
                        // Refresh product data automatically
                        Task.Run(async () => 
                            {
                            await LoadProductsAsync();
                            if (InvokeRequired)
                                Invoke(new Action(() => MessageBox.Show("Product data refreshed. Please check quantities and try again.", 
                                    "Data Refreshed", MessageBoxButtons.OK, MessageBoxIcon.Information)));
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
                
                // Ensure we're on UI thread for UI operations
                if (InvokeRequired)
                    {
                    Invoke(new Action(() => HandleSaleCompletionError(ex)));
                    }
                else
                    {
                    HandleSaleCompletionError(ex);
                    }
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
                invoiceForm.Show(); // Use Show() instead of ShowDialog() to avoid parent issues
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error in ShowInvoiceForm method");
                throw; // Re-throw to be caught by calling code
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
                // Don't throw here to prevent cascading errors
                }
            }

        private void HandleSaleCompletionError(Exception ex)
            {
            try
                {
                // Show user-friendly error message
                MessageBox.Show($"Error completing sale: {ex.Message}\n\nThe sale may not have been saved. Please check your cart and try again.",
                    "Sale Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                // Try to refresh cart display in case of data issues
                try
                    {
                    RefreshCartDisplay();
                    }
                catch (Exception refreshEx)
                    {
                    _logger.LogError(refreshEx, "Error refreshing cart display after sale completion error");
                    // Continue execution - don't let refresh errors crash the app
                    }
                }
            catch (Exception handlerEx)
                {
                _logger.LogError(handlerEx, "Error in HandleSaleCompletionError method");
                // Last resort - just log and continue to prevent app crash
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
    }