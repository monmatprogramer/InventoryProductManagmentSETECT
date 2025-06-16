using Microsoft.Extensions.Logging;
using InventoryPro.WinForms.Services;
using InventoryPro.Shared.DTOs;
using System.ComponentModel;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Product management form
    /// </summary>
    public partial class ProductForm : Form
    {
        private readonly ILogger<ProductForm> _logger;
        private readonly IApiService _apiService;

        // Event to notify when product data changes
        public event EventHandler? ProductDataChanged;

        // Controls
        private DataGridView dgvProducts;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private TextBox txtSearch;
        private ComboBox cboCategory;
        private Button btnSearch;
        private Button btnClear;

        // Toolbar buttons
        private ToolStripButton btnAdd;
        private ToolStripButton btnEdit;
        private ToolStripButton btnDelete;
        private ToolStripButton btnRefresh;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnExport;

        // Status bar
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblRecordCount;

        // Data
        private List<ProductDto> _products = new();
        private List<CategoryDto> _categories = new();
        
        // Search timer for debouncing
        private System.Windows.Forms.Timer _searchTimer;

        public ProductForm(ILogger<ProductForm> logger, IApiService apiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Initialize non-nullable fields
            dgvProducts = new DataGridView();
            toolStrip = new ToolStrip();
            statusStrip = new StatusStrip();
            txtSearch = new TextBox();
            cboCategory = new ComboBox();
            btnSearch = new Button();
            btnClear = new Button();
            btnAdd = new ToolStripButton();
            btnEdit = new ToolStripButton();
            btnDelete = new ToolStripButton();
            btnRefresh = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnExport = new ToolStripButton();
            lblStatus = new ToolStripStatusLabel();
            lblRecordCount = new ToolStripStatusLabel();

            // Initialize search timer
            _searchTimer = new System.Windows.Forms.Timer();
            _searchTimer.Interval = 500; // 500ms delay
            _searchTimer.Tick += SearchTimer_Tick;

            InitializeComponent();
            InitializeAsync();
            
            // Add form resize handler for overall responsiveness
            this.SizeChanged += ProductForm_SizeChanged;
        }

        private void InitializeComponent()
        {
            this.Text = "📦 Product Management - Inventory Pro";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F);

            // Create modern toolbar with better styling
            toolStrip = new ToolStrip
            {
                BackColor = Color.FromArgb(52, 58, 64),
                ForeColor = Color.White,
                GripStyle = ToolStripGripStyle.Hidden,
                Renderer = new ToolStripProfessionalRenderer(new ModernColorTable()),
                ImageScalingSize = new Size(24, 24),
                Padding = new Padding(20, 8, 20, 8),
                Height = 60
            };

            btnAdd = new ToolStripButton
            {
                Text = "➕ Add Product",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                Margin = new Padding(5, 0, 15, 0),
                Padding = new Padding(15, 8, 15, 8)
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new ToolStripButton
            {
                Text = "✏️ Edit Product",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255),
                Margin = new Padding(5, 0, 15, 0),
                Padding = new Padding(15, 8, 15, 8)
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new ToolStripButton
            {
                Text = "🗑️ Delete",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69),
                Margin = new Padding(5, 0, 15, 0),
                Padding = new Padding(15, 8, 15, 8)
            };
            btnDelete.Click += BtnDelete_Click;

            toolStripSeparator1 = new ToolStripSeparator
            {
                BackColor = Color.FromArgb(73, 80, 87),
                ForeColor = Color.FromArgb(73, 80, 87),
                Margin = new Padding(10, 0, 10, 0)
            };

            btnRefresh = new ToolStripButton
            {
                Text = "🔄 Refresh",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 162, 184),
                Margin = new Padding(5, 0, 15, 0),
                Padding = new Padding(15, 8, 15, 8)
            };
            btnRefresh.Click += BtnRefresh_Click;

            btnExport = new ToolStripButton
            {
                Text = "📤 Export Data",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(102, 16, 242),
                Margin = new Padding(5, 0, 15, 0),
                Padding = new Padding(15, 8, 15, 8)
            };
            btnExport.Click += BtnExport_Click;

            toolStrip.Items.AddRange(new ToolStripItem[] {
                btnAdd, btnEdit, btnDelete, toolStripSeparator1, btnRefresh, btnExport
            });

            // Create modern search panel with card-like appearance
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(25, 15, 25, 15),
                BackColor = Color.White,
                Margin = new Padding(20)
            };
            pnlSearch.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 224, 229), 1))
                {
                    e.Graphics.DrawLine(pen, 0, pnlSearch.Height - 1, pnlSearch.Width, pnlSearch.Height - 1);
                }
            };

            // Search section
            var lblSearch = new Label
            {
                Text = "🔍 Search Products:",
                Location = new Point(25, 20),
                Size = new Size(140, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            txtSearch = new TextBox
            {
                Location = new Point(175, 18),
                Size = new Size(280, 28),
                PlaceholderText = "Search by name, SKU, or description...",
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // Category filter section
            var lblCategory = new Label
            {
                Text = "📂 Category:",
                Location = new Point(480, 20),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            cboCategory = new ComboBox
            {
                Location = new Point(590, 18),
                Size = new Size(200, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };
            cboCategory.SelectedIndexChanged += CboCategory_SelectedIndexChanged;

            // Action buttons
            btnSearch = new Button
            {
                Text = "🔎 Search",
                Location = new Point(810, 16),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 105, 217);
            btnSearch.Click += BtnSearch_Click;

            btnClear = new Button
            {
                Text = "🧹 Clear",
                Location = new Point(935, 16),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 98, 104);
            btnClear.Click += BtnClear_Click;

            // Add hover effects
            btnSearch.MouseEnter += (s, e) => btnSearch.BackColor = Color.FromArgb(0, 105, 217);
            btnSearch.MouseLeave += (s, e) => btnSearch.BackColor = Color.FromArgb(0, 123, 255);
            btnClear.MouseEnter += (s, e) => btnClear.BackColor = Color.FromArgb(90, 98, 104);
            btnClear.MouseLeave += (s, e) => btnClear.BackColor = Color.FromArgb(108, 117, 125);

            pnlSearch.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, lblCategory, cboCategory, btnSearch, btnClear
            });

            // Create modern data grid with premium styling and responsiveness
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                GridColor = Color.FromArgb(230, 235, 241),
                Margin = new Padding(20),
                RowTemplate = { Height = 55 },
                ColumnHeadersHeight = 65,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = true,
                ScrollBars = ScrollBars.Both,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(52, 58, 64),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    SelectionBackColor = Color.FromArgb(52, 58, 64),
                    Padding = new Padding(15, 18, 15, 18),
                    WrapMode = DataGridViewTriState.False
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(33, 37, 41),
                    SelectionBackColor = Color.FromArgb(74, 144, 226),
                    SelectionForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 16, 12, 16),
                    Font = new Font("Segoe UI", 10),
                    WrapMode = DataGridViewTriState.False
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 250, 252),
                    ForeColor = Color.FromArgb(33, 37, 41),
                    SelectionBackColor = Color.FromArgb(74, 144, 226),
                    SelectionForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 16, 12, 16),
                    Font = new Font("Segoe UI", 10),
                    WrapMode = DataGridViewTriState.False
                }
            };
            dgvProducts.DoubleClick += DgvProducts_DoubleClick;
            dgvProducts.CellFormatting += DgvProducts_CellFormatting;
            dgvProducts.SizeChanged += DgvProducts_SizeChanged;
            dgvProducts.DataBindingComplete += DgvProducts_DataBindingComplete;

            // Modern status strip
            statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(73, 80, 87),
                Font = new Font("Segoe UI", 9),
                SizingGrip = false
            };
            
            lblStatus = new ToolStripStatusLabel 
            { 
                Text = "Ready",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69)
            };
            
            lblRecordCount = new ToolStripStatusLabel 
            { 
                Text = "0 records",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(73, 80, 87),
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblRecordCount });

            // Add controls to form
            this.Controls.Add(dgvProducts);
            this.Controls.Add(pnlSearch);
            this.Controls.Add(toolStrip);
            this.Controls.Add(statusStrip);
        }

        private async void InitializeAsync()
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Starting to load categories");
                var response = await _apiService.GetCategoriesAsync();
                
                if (response.Success && response.Data != null)
                {
                    _categories = response.Data;
                    _logger.LogInformation("Successfully loaded {CategoryCount} categories", _categories.Count);

                    // Update category combo box
                    cboCategory.Items.Clear();
                    cboCategory.Items.Add("All Categories");
                    foreach (var category in _categories)
                    {
                        cboCategory.Items.Add(category.Name);
                        _logger.LogDebug("Added category: {CategoryName}", category.Name);
                    }
                    cboCategory.SelectedIndex = 0;
                    _logger.LogInformation("Category dropdown populated successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to load categories. Success: {Success}, Data: {Data}, Message: {Message}", 
                        response.Success, response.Data?.Count ?? 0, response.Message);
                    
                    // Show user-friendly error message
                    lblStatus.Text = "Failed to load categories";
                    MessageBox.Show($"Unable to load categories: {response.Message}", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                lblStatus.Text = "Error loading categories";
                MessageBox.Show($"Error loading categories: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                lblStatus.Text = "Loading products...";

                var parameters = new PaginationParameters
                {
                    PageNumber = 1,
                    PageSize = 100,
                    SearchTerm = txtSearch.Text
                };

                var response = await _apiService.GetProductsAsync(parameters);
                if (response.Success && response.Data != null)
                {
                    _products = response.Data.Items;
                    FilterAndUpdateGrid();
                    lblStatus.Text = "Ready";
                }
                else
                {
                    lblStatus.Text = "Error loading products";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                lblStatus.Text = "Error loading products";
                MessageBox.Show("Error loading products. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterAndUpdateGrid()
        {
            var filteredProducts = _products.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchTerm = txtSearch.Text.ToLower();
                filteredProducts = filteredProducts.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.CategoryName.ToLower().Contains(searchTerm));
            }

            // Apply category filter
            if (cboCategory.SelectedIndex > 0 && cboCategory.SelectedItem != null)
            {
                var selectedCategory = cboCategory.SelectedItem.ToString();
                if (selectedCategory != "All Categories")
                {
                    filteredProducts = filteredProducts.Where(p => p.CategoryName == selectedCategory);
                }
            }

            var filteredList = filteredProducts.ToList();
            dgvProducts.DataSource = null;
            dgvProducts.DataSource = filteredList;
            
            ConfigureGridColumns();
            lblRecordCount.Text = $"{filteredList.Count} of {_products.Count} records";
        }

        private void ConfigureGridColumns()
        {
            // Configure responsive columns with modern styling and icons
            if (dgvProducts.Columns != null && dgvProducts.Columns.Count > 0)
            {
                // Temporarily disable auto-sizing to set up columns
                dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                var idColumn = dgvProducts.Columns["Id"];
                if (idColumn != null)
                {
                    idColumn.HeaderText = "🆔 ID";
                    idColumn.MinimumWidth = 60;
                    idColumn.FillWeight = 8; // 8% of total width
                    idColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    idColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    idColumn.DefaultCellStyle.ForeColor = Color.FromArgb(94, 108, 132);
                    idColumn.DefaultCellStyle.BackColor = Color.FromArgb(250, 251, 252);
                }

                var skuColumn = dgvProducts.Columns["SKU"];
                if (skuColumn != null)
                {
                    skuColumn.HeaderText = "🏷️ SKU";
                    skuColumn.MinimumWidth = 80;
                    skuColumn.FillWeight = 12; // 12% of total width
                    skuColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                    skuColumn.DefaultCellStyle.ForeColor = Color.FromArgb(108, 117, 125);
                    skuColumn.DefaultCellStyle.BackColor = Color.FromArgb(253, 254, 255);
                }

                var nameColumn = dgvProducts.Columns["Name"];
                if (nameColumn != null)
                {
                    nameColumn.HeaderText = "📦 Product Name";
                    nameColumn.MinimumWidth = 150;
                    nameColumn.FillWeight = 30; // 30% of total width - largest column
                    nameColumn.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    nameColumn.DefaultCellStyle.ForeColor = Color.FromArgb(44, 62, 80);
                }

                var priceColumn = dgvProducts.Columns["Price"];
                if (priceColumn != null)
                {
                    priceColumn.DefaultCellStyle.Format = "C2";
                    priceColumn.HeaderText = "💰 Price";
                    priceColumn.MinimumWidth = 80;
                    priceColumn.FillWeight = 12; // 12% of total width
                    priceColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    priceColumn.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    priceColumn.DefaultCellStyle.ForeColor = Color.FromArgb(46, 125, 50);
                    priceColumn.DefaultCellStyle.BackColor = Color.FromArgb(248, 255, 248);
                }

                var stockColumn = dgvProducts.Columns["Stock"];
                if (stockColumn != null)
                {
                    stockColumn.HeaderText = "📊 Stock";
                    stockColumn.MinimumWidth = 70;
                    stockColumn.FillWeight = 10; // 10% of total width
                    stockColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    stockColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }

                var categoryNameColumn = dgvProducts.Columns["CategoryName"];
                if (categoryNameColumn != null)
                {
                    categoryNameColumn.HeaderText = "📂 Category";
                    categoryNameColumn.MinimumWidth = 90;
                    categoryNameColumn.FillWeight = 13; // 13% of total width
                    categoryNameColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                    categoryNameColumn.DefaultCellStyle.ForeColor = Color.FromArgb(102, 16, 242);
                    categoryNameColumn.DefaultCellStyle.BackColor = Color.FromArgb(248, 245, 255);
                }

                var descriptionColumn = dgvProducts.Columns["Description"];
                if (descriptionColumn != null)
                {
                    descriptionColumn.HeaderText = "📝 Description";
                    descriptionColumn.MinimumWidth = 120;
                    descriptionColumn.FillWeight = 25; // 25% of total width
                    descriptionColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                    descriptionColumn.DefaultCellStyle.ForeColor = Color.FromArgb(73, 80, 87);
                    descriptionColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }

                // Hide unnecessary columns
                var columnsToHide = new[] { "CategoryId", "CreatedAt", "UpdatedAt", "ImageUrl", "MinStock", "IsActive" };
                foreach (var columnName in columnsToHide)
                {
                    if (dgvProducts.Columns.Contains(columnName))
                    {
                        var column = dgvProducts.Columns[columnName];
                        if (column != null)
                            column.Visible = false;
                    }
                }

                // Re-enable auto-sizing with Fill mode for responsiveness
                dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        private void DgvProducts_SizeChanged(object? sender, EventArgs e)
        {
            // Handle responsive behavior when grid is resized
            if (dgvProducts.Columns != null && dgvProducts.Columns.Count > 0)
            {
                var totalWidth = dgvProducts.ClientSize.Width;
                
                // Adjust column behavior based on available width
                if (totalWidth < 800)
                {
                    // Small screens - hide description column and adjust others
                    HideColumnIfExists("Description");
                    AdjustColumnsForSmallScreen();
                }
                else if (totalWidth < 1200)
                {
                    // Medium screens - show all but adjust proportions
                    ShowColumnIfExists("Description");
                    AdjustColumnsForMediumScreen();
                }
                else
                {
                    // Large screens - show all columns with full proportions
                    ShowColumnIfExists("Description");
                    ConfigureGridColumns();
                }
            }
        }

        private void DgvProducts_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Reconfigure columns after data binding
            ConfigureGridColumns();
        }

        private void HideColumnIfExists(string columnName)
        {
            if (dgvProducts.Columns.Contains(columnName))
            {
                var column = dgvProducts.Columns[columnName];
                if (column != null)
                    column.Visible = false;
            }
        }

        private void ShowColumnIfExists(string columnName)
        {
            if (dgvProducts.Columns.Contains(columnName))
            {
                var column = dgvProducts.Columns[columnName];
                if (column != null)
                    column.Visible = true;
            }
        }

        private void AdjustColumnsForSmallScreen()
        {
            // Adjust fill weights for small screens (without description)
            if (dgvProducts.Columns != null)
            {
                SetColumnFillWeight("Id", 10);
                SetColumnFillWeight("SKU", 15);
                SetColumnFillWeight("Name", 45);
                SetColumnFillWeight("Price", 15);
                SetColumnFillWeight("Stock", 15);
                SetColumnFillWeight("CategoryName", 0); // Hide category on very small screens
            }
        }

        private void AdjustColumnsForMediumScreen()
        {
            // Adjust fill weights for medium screens
            if (dgvProducts.Columns != null)
            {
                SetColumnFillWeight("Id", 8);
                SetColumnFillWeight("SKU", 12);
                SetColumnFillWeight("Name", 25);
                SetColumnFillWeight("Price", 12);
                SetColumnFillWeight("Stock", 10);
                SetColumnFillWeight("CategoryName", 13);
                SetColumnFillWeight("Description", 20);
            }
        }

        private void SetColumnFillWeight(string columnName, float weight)
        {
            if (dgvProducts.Columns.Contains(columnName))
            {
                var column = dgvProducts.Columns[columnName];
                if (column != null)
                {
                    if (weight == 0)
                    {
                        column.Visible = false;
                    }
                    else
                    {
                        column.Visible = true;
                        column.FillWeight = weight;
                    }
                }
            }
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

                e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.FormattingApplied = true;
            }

            if (grid.Columns[e.ColumnIndex].Name == "Name")
            {
                if (product.Stock <= 0)
                {
                    e.CellStyle.ForeColor = Color.FromArgb(108, 117, 125);
                    e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Italic);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.FromArgb(44, 62, 80);
                    e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
            }

            // Alternating row colors but respect stock formatting
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

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            using (var dialog = new ProductEditDialog(_categories))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var response = await _apiService.CreateProductAsync(dialog.Product);
                        if (response.Success)
                        {
                            await LoadProductsAsync();
                            ProductDataChanged?.Invoke(this, EventArgs.Empty);
                            MessageBox.Show("Product created successfully.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Error creating product: {response.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating product");
                        MessageBox.Show("Error creating product.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to edit.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedProduct = dgvProducts.SelectedRows[0].DataBoundItem as ProductDto;
            if (selectedProduct == null)
            {
                MessageBox.Show("Selected product is invalid.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var dialog = new ProductEditDialog(_categories, selectedProduct))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var response = await _apiService.UpdateProductAsync(
                            selectedProduct.Id, dialog.Product);
                        if (response.Success)
                        {
                            await LoadProductsAsync();
                            ProductDataChanged?.Invoke(this, EventArgs.Empty);
                            MessageBox.Show("Product updated successfully.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Error updating product: {response.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating product");
                        MessageBox.Show("Error updating product.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to delete.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedProduct = dgvProducts.SelectedRows[0].DataBoundItem as ProductDto;
            if (selectedProduct == null)
            {
                MessageBox.Show("Selected product is invalid.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{selectedProduct.Name}'?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var response = await _apiService.DeleteProductAsync(selectedProduct.Id);
                    if (response.Success)
                    {
                        await LoadProductsAsync();
                        ProductDataChanged?.Invoke(this, EventArgs.Empty);
                        MessageBox.Show("Product deleted successfully.",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Error deleting product: {response.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting product");
                    MessageBox.Show("Error deleting product.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            await LoadProductsAsync();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            // TODO: Implement export functionality
            MessageBox.Show("Export functionality will be implemented soon.",
                "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            FilterAndUpdateGrid();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            txtSearch.Clear();
            cboCategory.SelectedIndex = 0;
            FilterAndUpdateGrid();
        }

        private void CboCategory_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_products.Any()) // Only filter if we have loaded products
            {
                FilterAndUpdateGrid();
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            // Reset and start the timer for debounced search
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop();
            if (_products.Any()) // Only filter if we have loaded products
            {
                FilterAndUpdateGrid();
            }
        }

        private void DgvProducts_DoubleClick(object? sender, EventArgs e)
        {
            BtnEdit_Click(sender, e);
        }

        private void ProductForm_SizeChanged(object? sender, EventArgs e)
        {
            // Handle overall form responsiveness
            if (this.WindowState != FormWindowState.Minimized)
            {
                AdjustSearchPanelLayout();
                
                // Trigger grid resize handling
                DgvProducts_SizeChanged(dgvProducts, EventArgs.Empty);
            }
        }

        private void AdjustSearchPanelLayout()
        {
            // Make search panel responsive based on form width
            if (this.Width < 1000)
            {
                // Small form - stack search elements vertically
                if (txtSearch != null && cboCategory != null && btnSearch != null && btnClear != null)
                {
                    // Adjust positions for smaller screens
                    txtSearch.Size = new Size(200, 28);
                    cboCategory.Size = new Size(150, 28);
                    btnSearch.Size = new Size(90, 32);
                    btnClear.Size = new Size(80, 32);
                }
            }
            else if (this.Width < 1300)
            {
                // Medium form - compact layout
                if (txtSearch != null && cboCategory != null && btnSearch != null && btnClear != null)
                {
                    txtSearch.Size = new Size(240, 28);
                    cboCategory.Size = new Size(180, 28);
                    btnSearch.Size = new Size(100, 32);
                    btnClear.Size = new Size(90, 32);
                }
            }
            else
            {
                // Large form - full layout
                if (txtSearch != null && cboCategory != null && btnSearch != null && btnClear != null)
                {
                    txtSearch.Size = new Size(280, 28);
                    cboCategory.Size = new Size(200, 28);
                    btnSearch.Size = new Size(110, 32);
                    btnClear.Size = new Size(100, 32);
                }
            }
        }
    }

    /// <summary>
    /// Dialog for editing products
    /// </summary>
    public class ProductEditDialog : Form
    {
        private TextBox txtName;
        private TextBox txtSKU;
        private TextBox txtDescription;
        private NumericUpDown nudPrice;
        private NumericUpDown nudStock;
        private NumericUpDown nudMinStock;
        private ComboBox cboCategory;
        private CheckBox chkActive;
        private Button btnOK;
        private Button btnCancel;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CreateProductDto Product { get; private set; } = new CreateProductDto();

        public ProductEditDialog(List<CategoryDto> categories, ProductDto? existingProduct = null)
        {
            txtName = new TextBox();
            txtSKU = new TextBox();
            txtDescription = new TextBox();
            nudPrice = new NumericUpDown();
            nudStock = new NumericUpDown();
            nudMinStock = new NumericUpDown();
            cboCategory = new ComboBox();
            chkActive = new CheckBox();
            btnOK = new Button();
            btnCancel = new Button();

            InitializeComponent(categories, existingProduct);
        }

        private void InitializeComponent(List<CategoryDto> categories, ProductDto? existingProduct)
        {
            this.Text = existingProduct == null ? "Add Product" : "Edit Product";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblName = new Label
            {
                Text = "Name:",
                Location = new Point(20, 20),
                Size = new Size(100, 25)
            };

            txtName = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(340, 25)
            };

            var lblSKU = new Label
            {
                Text = "SKU:",
                Location = new Point(20, 80),
                Size = new Size(100, 25)
            };

            txtSKU = new TextBox
            {
                Location = new Point(20, 105),
                Size = new Size(340, 25)
            };

            var lblDescription = new Label
            {
                Text = "Description:",
                Location = new Point(20, 140),
                Size = new Size(100, 25)
            };

            txtDescription = new TextBox
            {
                Location = new Point(20, 165),
                Size = new Size(340, 60),
                Multiline = true
            };

            var lblPrice = new Label
            {
                Text = "Price:",
                Location = new Point(20, 235),
                Size = new Size(100, 25)
            };

            nudPrice = new NumericUpDown
            {
                Location = new Point(20, 260),
                Size = new Size(160, 25),
                DecimalPlaces = 2,
                Maximum = 999999.99m,
                Minimum = 0.01m
            };

            var lblStock = new Label
            {
                Text = "Stock:",
                Location = new Point(200, 235),
                Size = new Size(100, 25)
            };

            nudStock = new NumericUpDown
            {
                Location = new Point(200, 260),
                Size = new Size(160, 25),
                Maximum = 999999,
                Minimum = 0
            };

            var lblMinStock = new Label
            {
                Text = "Minimum Stock:",
                Location = new Point(20, 295),
                Size = new Size(100, 25)
            };

            nudMinStock = new NumericUpDown
            {
                Location = new Point(20, 320),
                Size = new Size(160, 25),
                Maximum = 999999,
                Minimum = 0,
                Value = 10
            };

            var lblCategory = new Label
            {
                Text = "Category:",
                Location = new Point(200, 295),
                Size = new Size(100, 25)
            };

            cboCategory = new ComboBox
            {
                Location = new Point(200, 320),
                Size = new Size(160, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (var category in categories)
            {
                cboCategory.Items.Add(category);
            }
            cboCategory.DisplayMember = "Name";
            cboCategory.ValueMember = "Id";
            if (cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;

            chkActive = new CheckBox
            {
                Text = "Active",
                Location = new Point(20, 360),
                Size = new Size(100, 25),
                Checked = true
            };

            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(205, 410),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(285, 410),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] {
                lblName, txtName, lblSKU, txtSKU, lblDescription, txtDescription,
                lblPrice, nudPrice, lblStock, nudStock, lblMinStock, nudMinStock,
                lblCategory, cboCategory, chkActive, btnOK, btnCancel
            });

            // Load existing product data
            if (existingProduct != null)
            {
                txtName.Text = existingProduct.Name;
                txtSKU.Text = existingProduct.SKU;
                txtDescription.Text = existingProduct.Description;
                nudPrice.Value = existingProduct.Price;
                nudStock.Value = existingProduct.Stock;
                nudMinStock.Value = existingProduct.MinStock;
                chkActive.Checked = existingProduct.IsActive;

                // Select category
                for (int i = 0; i < cboCategory.Items.Count; i++)
                {
                    var cat = cboCategory.Items[i] as CategoryDto;
                    if (cat != null && cat.Id == existingProduct.CategoryId)
                    {
                        cboCategory.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a product name.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSKU.Text))
            {
                MessageBox.Show("Please enter a SKU.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (cboCategory.SelectedItem == null)
            {
                MessageBox.Show("Please select a category.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            // Create product DTO
            var selectedCategory = (CategoryDto)cboCategory.SelectedItem;
            Product = new CreateProductDto
            {
                Name = txtName.Text.Trim(),
                SKU = txtSKU.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Price = nudPrice.Value,
                Stock = (int)nudStock.Value,
                MinStock = (int)nudMinStock.Value,
                CategoryId = selectedCategory.Id
            };
        }
    }

    /// <summary>
    /// Modern color table for toolbar styling
    /// </summary>
    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Color.FromArgb(52, 58, 64);
        public override Color ImageMarginGradientBegin => Color.FromArgb(52, 58, 64);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(52, 58, 64);
        public override Color ImageMarginGradientEnd => Color.FromArgb(52, 58, 64);
        public override Color MenuBorder => Color.FromArgb(73, 80, 87);
        public override Color MenuItemBorder => Color.FromArgb(0, 123, 255);
        public override Color MenuItemSelected => Color.FromArgb(73, 80, 87);
        public override Color MenuStripGradientBegin => Color.FromArgb(52, 58, 64);
        public override Color MenuStripGradientEnd => Color.FromArgb(52, 58, 64);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(73, 80, 87);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(73, 80, 87);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(90, 98, 104);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(90, 98, 104);
        public override Color ButtonSelectedHighlight => Color.FromArgb(73, 80, 87);
        public override Color ButtonSelectedBorder => Color.FromArgb(0, 123, 255);
        public override Color ButtonPressedHighlight => Color.FromArgb(90, 98, 104);
        public override Color ButtonPressedBorder => Color.FromArgb(0, 105, 217);
        public override Color ButtonCheckedHighlight => Color.FromArgb(73, 80, 87);
        public override Color ButtonCheckedGradientBegin => Color.FromArgb(73, 80, 87);
        public override Color ButtonCheckedGradientEnd => Color.FromArgb(73, 80, 87);
        public override Color ButtonCheckedGradientMiddle => Color.FromArgb(73, 80, 87);
        public override Color ButtonSelectedGradientBegin => Color.FromArgb(73, 80, 87);
        public override Color ButtonSelectedGradientEnd => Color.FromArgb(73, 80, 87);
        public override Color ButtonSelectedGradientMiddle => Color.FromArgb(73, 80, 87);
        public override Color ButtonPressedGradientBegin => Color.FromArgb(90, 98, 104);
        public override Color ButtonPressedGradientEnd => Color.FromArgb(90, 98, 104);
        public override Color ButtonPressedGradientMiddle => Color.FromArgb(90, 98, 104);
        public override Color CheckBackground => Color.FromArgb(73, 80, 87);
        public override Color CheckSelectedBackground => Color.FromArgb(90, 98, 104);
        public override Color CheckPressedBackground => Color.FromArgb(90, 98, 104);
        public override Color GripDark => Color.FromArgb(73, 80, 87);
        public override Color GripLight => Color.FromArgb(108, 117, 125);
        public override Color ImageMarginRevealedGradientBegin => Color.FromArgb(52, 58, 64);
        public override Color ImageMarginRevealedGradientEnd => Color.FromArgb(52, 58, 64);
        public override Color ImageMarginRevealedGradientMiddle => Color.FromArgb(52, 58, 64);
        public override Color SeparatorDark => Color.FromArgb(73, 80, 87);
        public override Color SeparatorLight => Color.FromArgb(108, 117, 125);
        public override Color StatusStripGradientBegin => Color.FromArgb(248, 249, 250);
        public override Color StatusStripGradientEnd => Color.FromArgb(248, 249, 250);
        public override Color ToolStripBorder => Color.FromArgb(73, 80, 87);
        public override Color ToolStripContentPanelGradientBegin => Color.FromArgb(52, 58, 64);
        public override Color ToolStripContentPanelGradientEnd => Color.FromArgb(52, 58, 64);
        public override Color ToolStripGradientBegin => Color.FromArgb(52, 58, 64);
        public override Color ToolStripGradientEnd => Color.FromArgb(52, 58, 64);
        public override Color ToolStripGradientMiddle => Color.FromArgb(52, 58, 64);
        public override Color ToolStripPanelGradientBegin => Color.FromArgb(52, 58, 64);
        public override Color ToolStripPanelGradientEnd => Color.FromArgb(52, 58, 64);
        public override Color OverflowButtonGradientBegin => Color.FromArgb(73, 80, 87);
        public override Color OverflowButtonGradientEnd => Color.FromArgb(73, 80, 87);
        public override Color OverflowButtonGradientMiddle => Color.FromArgb(73, 80, 87);
        public override Color RaftingContainerGradientBegin => Color.FromArgb(52, 58, 64);
        public override Color RaftingContainerGradientEnd => Color.FromArgb(52, 58, 64);
    }
}