using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Dialogs;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Drawing.Drawing2D;


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

        // Pagination controls
        private Panel pnlPagination;
        private Button btnFirstPage;
        private Button btnPrevPage;
        private Button btnNextPage;
        private Button btnLastPage;
        private Label lblPageInfo;
        private ComboBox cboPageSize;
        private Label lblPageSize;

        // Data
        private List<ProductDto> _products = new();
        private List<CategoryDto> _categories = new();
        
        // Pagination state
        private int _currentPage = 1;
        private int _pageSize = 25;
        private int _totalRecords = 0;
        private int _totalPages = 0;
        
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
            
            // Initialize pagination controls
            pnlPagination = new Panel();
            btnFirstPage = new Button();
            btnPrevPage = new Button();
            btnNextPage = new Button();
            btnLastPage = new Button();
            lblPageInfo = new Label();
            cboPageSize = new ComboBox();
            lblPageSize = new Label();

            // Initialize search timer
            _searchTimer = new System.Windows.Forms.Timer();
            _searchTimer.Interval = 500; // 500ms delay
            _searchTimer.Tick += SearchTimer_Tick;

            InitializeComponent();
            SetupEventHandlers();
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
                RowHeadersVisible = true,
                RowHeadersWidth = 60,
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
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
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
            dgvProducts.RowPostPaint += DgvProducts_RowPostPaint;

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

            // Create pagination panel
            pnlPagination = new Panel
            {
                Height = 70,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(52, 58, 64),
                Padding = new Padding(20, 15, 20, 15)
            };

            // Page size selector
            lblPageSize = new Label
            {
                Text = "Items per page:",
                Location = new Point(20, 20),
                Size = new Size(100, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            cboPageSize = new ComboBox
            {
                Location = new Point(125, 18),
                Size = new Size(80, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(52, 58, 64)
            };
            cboPageSize.Items.AddRange(new object[] { 10, 25, 50, 100 });
            cboPageSize.SelectedItem = _pageSize;
            cboPageSize.SelectedIndexChanged += CboPageSize_SelectedIndexChanged;

            // Page info label
            lblPageInfo = new Label
            {
                Location = new Point(230, 20),
                Size = new Size(200, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Navigation buttons with modern styling
            btnFirstPage = CreatePaginationButton("⏮️ First", new Point(450, 15));
            btnFirstPage.Click += BtnFirstPage_Click;

            btnPrevPage = CreatePaginationButton("⏪ Prev", new Point(540, 15));
            btnPrevPage.Click += BtnPrevPage_Click;

            btnNextPage = CreatePaginationButton("Next ⏩", new Point(630, 15));
            btnNextPage.Click += BtnNextPage_Click;

            btnLastPage = CreatePaginationButton("Last ⏭️", new Point(720, 15));
            btnLastPage.Click += BtnLastPage_Click;

            // Add controls to pagination panel
            pnlPagination.Controls.AddRange(new Control[] {
                lblPageSize, cboPageSize, lblPageInfo,
                btnFirstPage, btnPrevPage, btnNextPage, btnLastPage
            });

            // Add controls to form
            this.Controls.Add(dgvProducts);
            this.Controls.Add(pnlPagination);
            this.Controls.Add(pnlSearch);
            this.Controls.Add(toolStrip);
            this.Controls.Add(statusStrip);
        }

        private void SetupEventHandlers()
        {
            // Setup search panel paint event for bottom border
            var pnlSearch = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Height == 90);
            if (pnlSearch != null)
            {
                pnlSearch.Paint += (s, e) =>
                {
                    using (var pen = new Pen(Color.FromArgb(220, 224, 229), 1))
                    {
                        e.Graphics.DrawLine(pen, 0, pnlSearch.Height - 1, pnlSearch.Width, pnlSearch.Height - 1);
                    }
                };
            }
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
                UpdatePaginationButtons(false); // Disable buttons while loading

                var parameters = new PaginationParameters
                {
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    SearchTerm = txtSearch.Text
                };

                var response = await _apiService.GetProductsAsync(parameters);
                if (response.Success && response.Data != null)
                {
                    _products = response.Data.Items;
                    _totalRecords = response.Data.TotalCount;
                    _totalPages = (int)Math.Ceiling((double)_totalRecords / _pageSize);
                    
                    dgvProducts.DataSource = null;
                    dgvProducts.DataSource = _products;
                    ConfigureGridColumns();
                    
                    UpdatePaginationInfo();
                    UpdatePaginationButtons(true);
                    lblStatus.Text = "Ready";
                }
                else
                {
                    lblStatus.Text = "Error loading products";
                    UpdatePaginationButtons(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                lblStatus.Text = "Error loading products";
                UpdatePaginationButtons(true);
                MessageBox.Show("Error loading products. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task FilterAndUpdateGridAsync()
        {
            // Reset to first page when filtering
            _currentPage = 1;
            await LoadProductsAsync();
        }

        private void ConfigureGridColumns()
        {
            // Configure responsive columns with modern styling and icons
            if (dgvProducts.Columns != null && dgvProducts.Columns.Count > 0)
            {
                // Temporarily disable auto-sizing to set up columns
                dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                // Hide the actual ID column and use row headers for sequential numbers
                var idColumn = dgvProducts.Columns["Id"];
                if (idColumn != null)
                {
                    idColumn.Visible = false;
                }

                // Configure row headers for sequential numbering and add header title
                dgvProducts.RowHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(52, 58, 64),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    SelectionBackColor = Color.FromArgb(52, 58, 64),
                    Padding = new Padding(5, 5, 5, 5),
                    WrapMode = DataGridViewTriState.False
                };

                // Set the row header title
                if (dgvProducts.TopLeftHeaderCell != null)
                {
                    dgvProducts.TopLeftHeaderCell.Value = "#";
                    dgvProducts.TopLeftHeaderCell.Style = new DataGridViewCellStyle
                    {
                        BackColor = Color.FromArgb(52, 58, 64),
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        SelectionBackColor = Color.FromArgb(52, 58, 64),
                        Padding = new Padding(5, 5, 5, 5)
                    };
                }

                var skuColumn = dgvProducts.Columns["SKU"];
                if (skuColumn != null)
                {
                    skuColumn.HeaderText = "🏷️ SKU";
                    skuColumn.MinimumWidth = 80;
                    skuColumn.FillWeight = 15; // 15% of total width (increased since no ID column)
                    skuColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                    skuColumn.DefaultCellStyle.ForeColor = Color.FromArgb(108, 117, 125);
                    skuColumn.DefaultCellStyle.BackColor = Color.FromArgb(253, 254, 255);
                }

                var nameColumn = dgvProducts.Columns["Name"];
                if (nameColumn != null)
                {
                    nameColumn.HeaderText = "📦 Product Name";
                    nameColumn.MinimumWidth = 150;
                    nameColumn.FillWeight = 32; // 32% of total width - largest column
                    nameColumn.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    nameColumn.DefaultCellStyle.ForeColor = Color.FromArgb(44, 62, 80);
                }

                var priceColumn = dgvProducts.Columns["Price"];
                if (priceColumn != null)
                {
                    priceColumn.DefaultCellStyle.Format = "C2";
                    priceColumn.HeaderText = "💰 Price";
                    priceColumn.MinimumWidth = 80;
                    priceColumn.FillWeight = 13; // 13% of total width
                    priceColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    priceColumn.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    priceColumn.DefaultCellStyle.ForeColor = Color.FromArgb(46, 125, 50);
                    priceColumn.DefaultCellStyle.BackColor = Color.FromArgb(248, 255, 248);
                }

                var stockColumn = dgvProducts.Columns["Stock"];
                if (stockColumn != null)
                {
                    stockColumn.HeaderText = "📊 Stock";
                    stockColumn.MinimumWidth = 70;
                    stockColumn.FillWeight = 12; // 12% of total width
                    stockColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    stockColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }

                var categoryNameColumn = dgvProducts.Columns["CategoryName"];
                if (categoryNameColumn != null)
                {
                    categoryNameColumn.HeaderText = "📂 Category";
                    categoryNameColumn.MinimumWidth = 90;
                    categoryNameColumn.FillWeight = 15; // 15% of total width
                    categoryNameColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                    categoryNameColumn.DefaultCellStyle.ForeColor = Color.FromArgb(102, 16, 242);
                    categoryNameColumn.DefaultCellStyle.BackColor = Color.FromArgb(248, 245, 255);
                }

                var descriptionColumn = dgvProducts.Columns["Description"];
                if (descriptionColumn != null)
                {
                    descriptionColumn.HeaderText = "📝 Description";
                    descriptionColumn.MinimumWidth = 120;
                    descriptionColumn.FillWeight = 28; // 28% of total width
                    descriptionColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                    descriptionColumn.DefaultCellStyle.ForeColor = Color.FromArgb(73, 80, 87);
                    descriptionColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }

                // Hide unnecessary columns (Id is already hidden above)
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

        private void DgvProducts_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (sender is not DataGridView grid) return;

            // Draw sequential row numbers starting from 1
            var rowNumber = (e.RowIndex + 1).ToString();
            
            // Calculate the center position for the text
            var centerFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // Get the row header rectangle
            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, 
                grid.RowHeadersWidth, e.RowBounds.Height);

            // Draw the row number with modern styling
            using (var headerBrush = new SolidBrush(Color.White))
            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                e.Graphics.DrawString(rowNumber, font, headerBrush, headerBounds, centerFormat);
            }
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
                SetColumnFillWeight("SKU", 20);
                SetColumnFillWeight("Name", 50);
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
                SetColumnFillWeight("SKU", 15);
                SetColumnFillWeight("Name", 28);
                SetColumnFillWeight("Price", 12);
                SetColumnFillWeight("Stock", 10);
                SetColumnFillWeight("CategoryName", 15);
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
                        // Map ProductDto to CreateProductDto
                        var createProductDto = new CreateProductDto
                        {
                            Name = dialog.Product.Name,
                            SKU = dialog.Product.SKU,
                            Description = dialog.Product.Description,
                            Price = dialog.Product.Price,
                            Stock = dialog.Product.Stock,
                            MinStock = dialog.Product.MinStock,
                            CategoryId = dialog.Product.CategoryId
                        };

                        var response = await _apiService.CreateProductAsync(createProductDto);
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
            if (dgvProducts.SelectedRows.Count > 0)
                {
                var selectedProduct = dgvProducts.SelectedRows[0].DataBoundItem as ProductDto; // Declare 'selectedProduct' here
                if (selectedProduct != null)
                    {
                    using (var dialog = new ProductEditDialog(_categories, selectedProduct))
                        {
                        if (dialog.ShowDialog() == DialogResult.OK)
                            {
                            try
                                {
                                // Map ProductDto to CreateProductDto
                                var updateProductDto = new CreateProductDto
                                    {
                                    Name = dialog.Product.Name,
                                    SKU = dialog.Product.SKU,
                                    Description = dialog.Product.Description,
                                    Price = dialog.Product.Price,
                                    Stock = dialog.Product.Stock,
                                    MinStock = dialog.Product.MinStock,
                                    CategoryId = dialog.Product.CategoryId
                                    };

                                var response = await _apiService.UpdateProductAsync(
                                    selectedProduct.Id, updateProductDto); // Use 'selectedProduct' here
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
                }
            else
                {
                MessageBox.Show("Please select a product to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                var selectedProduct = dgvProducts.SelectedRows[0].DataBoundItem as ProductDto;
                if (selectedProduct != null)
                {
                    var productInfo =
                        $"Product: {selectedProduct.Name}\n" +
                        $"SKU: {selectedProduct.SKU}\n" +
                        $"Category: {selectedProduct.CategoryName}\n" +
                        $"Price: {selectedProduct.Price:C2}\n\n" +
                           "Are you sure you want to delete this product?";
                    using (var dialog = new ModernDeleteConfirmationDialog(productInfo))
                    {
                        if (dialog.ShowDialog() == DialogResult.Yes)
                        {
                            try
                            {
                                var response = await _apiService.DeleteProductAsync(selectedProduct.Id);
                                if (response.Success)
                                {
                                    await LoadProductsAsync();
                                    ProductDataChanged?.Invoke(this, EventArgs.Empty);
                                    using (var dialogSuccess = new ModernSuccessDialog("Product deleted successfully."))
                                        {
                                        dialogSuccess.ShowDialog(this);
                                        }
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
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            _currentPage = 1; // Reset to first page when refreshing
            await LoadProductsAsync();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            // TODO: Implement export functionality
            MessageBox.Show("Export functionality will be implemented soon.",
                "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnSearch_Click(object? sender, EventArgs e)
        {
            await FilterAndUpdateGridAsync();
        }

        private async void BtnClear_Click(object? sender, EventArgs e)
        {
            txtSearch.Clear();
            if (cboCategory.Items.Count > 0)
            {
                cboCategory.SelectedIndex = 0;
            }
            await FilterAndUpdateGridAsync();
        }

        private async void CboCategory_SelectedIndexChanged(object? sender, EventArgs e)
        {
            await FilterAndUpdateGridAsync();
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            // Reset and start the timer for debounced search
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private async void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop();
            await FilterAndUpdateGridAsync();
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

        #region Pagination Methods

        private Button CreatePaginationButton(string text, Point location)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(80, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 105, 217);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 86, 179);
            return button;
        }

        private void UpdatePaginationInfo()
        {
            if (_totalRecords == 0)
            {
                lblPageInfo.Text = "No records found";
                lblRecordCount.Text = "0 records";
            }
            else
            {
                var startRecord = (_currentPage - 1) * _pageSize + 1;
                var endRecord = Math.Min(_currentPage * _pageSize, _totalRecords);
                lblPageInfo.Text = $"Page {_currentPage} of {_totalPages} ({startRecord}-{endRecord} of {_totalRecords})";
                lblRecordCount.Text = $"{_totalRecords} total records";
            }
        }

        private void UpdatePaginationButtons(bool enabled)
        {
            if (btnFirstPage == null || btnPrevPage == null || btnNextPage == null || btnLastPage == null)
                return;

            btnFirstPage.Enabled = enabled && _currentPage > 1;
            btnPrevPage.Enabled = enabled && _currentPage > 1;
            btnNextPage.Enabled = enabled && _currentPage < _totalPages;
            btnLastPage.Enabled = enabled && _currentPage < _totalPages;

            // Update button colors based on enabled state
            var enabledColor = Color.FromArgb(0, 123, 255);
            var disabledColor = Color.FromArgb(108, 117, 125);

            btnFirstPage.BackColor = btnFirstPage.Enabled ? enabledColor : disabledColor;
            btnPrevPage.BackColor = btnPrevPage.Enabled ? enabledColor : disabledColor;
            btnNextPage.BackColor = btnNextPage.Enabled ? enabledColor : disabledColor;
            btnLastPage.BackColor = btnLastPage.Enabled ? enabledColor : disabledColor;
        }

        private async void BtnFirstPage_Click(object? sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage = 1;
                await LoadProductsAsync();
            }
        }

        private async void BtnPrevPage_Click(object? sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadProductsAsync();
            }
        }

        private async void BtnNextPage_Click(object? sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadProductsAsync();
            }
        }

        private async void BtnLastPage_Click(object? sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage = _totalPages;
                await LoadProductsAsync();
            }
        }

        private async void CboPageSize_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboPageSize.SelectedItem != null && int.TryParse(cboPageSize.SelectedItem.ToString(), out int newPageSize))
            {
                _pageSize = newPageSize;
                _currentPage = 1; // Reset to first page when changing page size
                await LoadProductsAsync();
            }
        }

        #endregion
    }

    /// <summary>
    /// Modern Product Edit Dialog with improved layout and styling
    /// </summary>
    public class ProductEditDialog : Form
    {
        private readonly List<CategoryDto> _categories;
        private readonly ProductDto? _existingProduct;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ProductDto Product { get; private set; } = null!;

        private TextBox txtName = null!;
        private TextBox txtSKU = null!;
        private TextBox txtDescription = null!;
        private TextBox txtPrice = null!;
        private NumericUpDown nudStock = null!;
        private NumericUpDown nudMinStock = null!;
        private ComboBox cboCategory = null!;
        private Button btnOK = null!;
        private Button btnCancel = null!;

        public ProductEditDialog(List<CategoryDto> categories)
            : this(categories, null)
        {
        }

        public ProductEditDialog(List<CategoryDto> categories, ProductDto? existingProduct)
        {
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
            _existingProduct = existingProduct;
            
            InitializeComponent();
            SetupForm();
            PopulateCategories();
            
            if (_existingProduct != null)
            {
                PopulateFields();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Initialize controls
            txtName = new TextBox();
            txtSKU = new TextBox();
            txtDescription = new TextBox();
            txtPrice = new TextBox();
            nudStock = new NumericUpDown();
            nudMinStock = new NumericUpDown();
            cboCategory = new ComboBox();
            btnOK = new Button();
            btnCancel = new Button();

            // Form properties - Modern design with larger size and darker background
            this.Text = _existingProduct == null ? "➕ Add New Product" : "✏️ Edit Product";
            this.Size = new Size(580, 780);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(235, 240, 245);
            this.Font = new Font("Segoe UI", 9F);
            this.Padding = new Padding(30, 30, 30, 30);

            // Add form title at the top
            var titleLabel = new Label
            {
                Text = _existingProduct == null ? "🛍️ CREATE NEW PRODUCT" : "📝 EDIT PRODUCT DETAILS",
                Location = new Point(40, 15),
                Size = new Size(480, 45),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 135, 84),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.None
            };
            titleLabel.Paint += (s, e) => {
                var rect = new Rectangle(0, titleLabel.Height - 3, titleLabel.Width, 3);
                using (var brush = new LinearGradientBrush(rect, Color.FromArgb(25, 135, 84), Color.FromArgb(40, 167, 69), LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            };
            this.Controls.Add(titleLabel);

            // Product Name - Improved layout with bigger input and better spacing
            var lblName = new Label
            {
                Text = "📦 Product Name:",
                Location = new Point(40, 80),
                Size = new Size(200, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
            };

            txtName = new TextBox
            {
                Location = new Point(40, 108),
                Size = new Size(480, 42),
                Font = new Font("Segoe UI", 13),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 255, 255),
                ForeColor = Color.FromArgb(33, 37, 41),
                Padding = new Padding(15, 12, 15, 12)
            };
            txtName.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, txtName.ClientRectangle, Color.FromArgb(0, 123, 255), ButtonBorderStyle.Solid);
            txtName.Enter += (s, e) => { txtName.BackColor = Color.FromArgb(245, 251, 255); txtName.Invalidate(); };
            txtName.Leave += (s, e) => { txtName.BackColor = Color.FromArgb(255, 255, 255); txtName.Invalidate(); };

            // SKU - Modern design with more spacing
            var lblSKU = new Label
            {
                Text = "🏷️ SKU (Stock Keeping Unit):",
                Location = new Point(40, 180),
                Size = new Size(250, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
            };

            txtSKU = new TextBox
            {
                Location = new Point(40, 208),
                Size = new Size(480, 42),
                Font = new Font("Segoe UI", 13),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 255, 255),
                ForeColor = Color.FromArgb(33, 37, 41),
                Padding = new Padding(15, 12, 15, 12)
            };
            txtSKU.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, txtSKU.ClientRectangle, Color.FromArgb(0, 123, 255), ButtonBorderStyle.Solid);
            txtSKU.Enter += (s, e) => { txtSKU.BackColor = Color.FromArgb(245, 251, 255); txtSKU.Invalidate(); };
            txtSKU.Leave += (s, e) => { txtSKU.BackColor = Color.FromArgb(255, 255, 255); txtSKU.Invalidate(); };

            // Description - Modern textarea design with better spacing
            var lblDescription = new Label
            {
                Text = "📝 Description:",
                Location = new Point(40, 280),
                Size = new Size(150, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
            };

            txtDescription = new TextBox
            {
                Location = new Point(40, 308),
                Size = new Size(480, 95),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 255, 255),
                ForeColor = Color.FromArgb(33, 37, 41),
                Padding = new Padding(15, 12, 15, 12)
            };
            txtDescription.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, txtDescription.ClientRectangle, Color.FromArgb(0, 123, 255), ButtonBorderStyle.Solid);
            txtDescription.Enter += (s, e) => { txtDescription.BackColor = Color.FromArgb(245, 251, 255); txtDescription.Invalidate(); };
            txtDescription.Leave += (s, e) => { txtDescription.BackColor = Color.FromArgb(255, 255, 255); txtDescription.Invalidate(); };

            // Price - Modern text input with placeholder and better spacing
            var lblPrice = new Label
            {
                Text = "💰 Price ($):",
                Location = new Point(40, 433),
                Size = new Size(120, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
            };

            txtPrice = new TextBox
            {
                Location = new Point(40, 461),
                Size = new Size(220, 42),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 255, 248),
                ForeColor = Color.FromArgb(46, 125, 50),
                TextAlign = HorizontalAlignment.Right,
                PlaceholderText = "0.00",
                Padding = new Padding(15, 12, 15, 12)
            };
            txtPrice.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, txtPrice.ClientRectangle, Color.FromArgb(40, 167, 69), ButtonBorderStyle.Solid);
            txtPrice.Enter += (s, e) => { txtPrice.BackColor = Color.FromArgb(240, 255, 240); txtPrice.Invalidate(); };
            txtPrice.Leave += (s, e) => { txtPrice.BackColor = Color.FromArgb(248, 255, 248); txtPrice.Invalidate(); };
            txtPrice.KeyPress += TxtPrice_KeyPress;

            // Stock - Modern numeric input with better spacing
            var lblStock = new Label
            {
                Text = "📊 Current Stock:",
                Location = new Point(290, 433),
                Size = new Size(150, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
            };

            nudStock = new NumericUpDown
            {
                Location = new Point(290, 461),
                Size = new Size(230, 42),
                Maximum = 999999,
                Minimum = 0,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 250, 255),
                ForeColor = Color.FromArgb(52, 144, 220),
                TextAlign = HorizontalAlignment.Center
            };
            nudStock.Enter += (s, e) => nudStock.BackColor = Color.FromArgb(230, 245, 255);
            nudStock.Leave += (s, e) => nudStock.BackColor = Color.FromArgb(245, 250, 255);

            // Minimum Stock - Modern numeric input with better spacing
            var lblMinStock = new Label
            {
                Text = "⚠️ Minimum Stock Alert:",
                Location = new Point(40, 533),
                Size = new Size(180, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
            };

            nudMinStock = new NumericUpDown
            {
                Location = new Point(40, 561),
                Size = new Size(220, 42),
                Maximum = 999999,
                Minimum = 0,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 248, 240),
                ForeColor = Color.FromArgb(255, 140, 0),
                TextAlign = HorizontalAlignment.Center
            };
            nudMinStock.Enter += (s, e) => nudMinStock.BackColor = Color.FromArgb(255, 240, 220);
            nudMinStock.Leave += (s, e) => nudMinStock.BackColor = Color.FromArgb(255, 248, 240);

            // Category - Modern dropdown with better spacing
            var lblCategory = new Label
            {
                Text = "📂 Category:",
                Location = new Point(290, 533),
                Size = new Size(120, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
            };

            cboCategory = new ComboBox
            {
                Location = new Point(290, 561),
                Size = new Size(230, 42),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(248, 245, 255),
                ForeColor = Color.FromArgb(102, 16, 242),
                FlatStyle = FlatStyle.Standard
            };
            cboCategory.Enter += (s, e) => cboCategory.BackColor = Color.FromArgb(235, 225, 255);
            cboCategory.Leave += (s, e) => cboCategory.BackColor = Color.FromArgb(248, 245, 255);

            // Modern Buttons with round borders and improved positioning
            var buttonPanel = new Panel
            {
                Location = new Point(70, 630),
                Size = new Size(440, 80),
                BackColor = Color.Transparent
            };

            btnOK = new Button
            {
                Text = "💾 SAVE PRODUCT",
                Location = new Point(50, 20),
                Size = new Size(170, 55),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 139, 58);
            btnOK.FlatAppearance.MouseDownBackColor = Color.FromArgb(28, 117, 49);
            btnOK.Click += BtnOK_Click;
            btnOK.Paint += (s, e) => {
                var btn = s as Button;
                if (btn != null)
                {
                    var path = new GraphicsPath();
                    var rect = new Rectangle(0, 0, btn.Width, btn.Height);
                    int radius = 15;
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                    path.CloseFigure();
                    btn.Region = new Region(path);
                    
                    using (var brush = new SolidBrush(btn.BackColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    
                    var textRect = new Rectangle(0, 0, btn.Width, btn.Height);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using (var textBrush = new SolidBrush(btn.ForeColor))
                    {
                        e.Graphics.DrawString(btn.Text, btn.Font, textBrush, textRect, sf);
                    }
                }
            };

            btnCancel = new Button
            {
                Text = "❌ CANCEL",
                Location = new Point(240, 20),
                Size = new Size(170, 55),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 98, 104);
            btnCancel.FlatAppearance.MouseDownBackColor = Color.FromArgb(73, 80, 87);
            btnCancel.Paint += (s, e) => {
                var btn = s as Button;
                if (btn != null)
                {
                    var path = new GraphicsPath();
                    var rect = new Rectangle(0, 0, btn.Width, btn.Height);
                    int radius = 15;
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                    path.CloseFigure();
                    btn.Region = new Region(path);
                    
                    using (var brush = new SolidBrush(btn.BackColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    
                    var textRect = new Rectangle(0, 0, btn.Width, btn.Height);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using (var textBrush = new SolidBrush(btn.ForeColor))
                    {
                        e.Graphics.DrawString(btn.Text, btn.Font, textBrush, textRect, sf);
                    }
                }
            };

            buttonPanel.Controls.AddRange(new Control[] { btnOK, btnCancel });

            // Add all controls to form
            this.Controls.AddRange(new Control[] {
                lblName, txtName,
                lblSKU, txtSKU,
                lblDescription, txtDescription,
                lblPrice, txtPrice,
                lblStock, nudStock,
                lblMinStock, nudMinStock,
                lblCategory, cboCategory,
                buttonPanel
            });

            this.ResumeLayout(false);
        }

        private void SetupForm()
        {
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void PopulateCategories()
        {
            cboCategory.Items.Clear();
            foreach (var category in _categories)
            {
                cboCategory.Items.Add(category);
            }
            cboCategory.DisplayMember = "Name";
            cboCategory.ValueMember = "Id";
        }

        private void PopulateFields()
        {
            if (_existingProduct != null)
            {
                txtName.Text = _existingProduct.Name ?? string.Empty;
                txtSKU.Text = _existingProduct.SKU ?? string.Empty;
                txtDescription.Text = _existingProduct.Description ?? string.Empty;
                txtPrice.Text = _existingProduct.Price.ToString("F2");
                nudStock.Value = _existingProduct.Stock;
                nudMinStock.Value = _existingProduct.MinStock;

                var categoryItem = _categories.FirstOrDefault(c => c.Id == _existingProduct.CategoryId);
                if (categoryItem != null)
                {
                    cboCategory.SelectedItem = categoryItem;
                }
            }
        }

        private void TxtPrice_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow control keys (backspace, delete, etc.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Allow digits
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            // Allow decimal point only if there isn't one already
            if (e.KeyChar == '.' && sender is TextBox txt && !txt.Text.Contains('.'))
            {
                return;
            }

            // Block all other characters
            e.Handled = true;
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a product name.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                this.DialogResult = DialogResult.None;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSKU.Text))
            {
                MessageBox.Show("Please enter a SKU.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSKU.Focus();
                this.DialogResult = DialogResult.None;
                return;
            }

            if (cboCategory.SelectedItem == null)
            {
                MessageBox.Show("Please select a category.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCategory.Focus();
                this.DialogResult = DialogResult.None;
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price greater than 0.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                this.DialogResult = DialogResult.None;
                return;
            }

            // Create product DTO
            var selectedCategory = (CategoryDto)cboCategory.SelectedItem;
            Product = new ProductDto
            {
                Id = _existingProduct?.Id ?? 0,
                Name = txtName.Text.Trim(),
                SKU = txtSKU.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Price = price,
                Stock = (int)nudStock.Value,
                MinStock = (int)nudMinStock.Value,
                CategoryId = selectedCategory.Id,
                CategoryName = selectedCategory.Name,
                IsActive = true
            };
        }
    }
}