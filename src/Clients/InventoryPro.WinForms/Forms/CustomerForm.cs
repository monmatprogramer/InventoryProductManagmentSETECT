using CsvHelper;
using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using LicenseContext = OfficeOpenXml.LicenseContext;
using Rectangle = System.Drawing.Rectangle;


namespace InventoryPro.WinForms.Forms
    {
    /// <summary>
    /// Customer management form
    /// Provides CRUD operations for customer data
    /// </summary>
    public partial class CustomerForm : Form
        {
        private readonly ILogger<CustomerForm> _logger;
        private readonly IApiService _apiService;

        // Controls
        private DataGridView dgvCustomers;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnClear;

        // Toolbar buttons
        private ToolStripButton btnAdd;
        private ToolStripButton btnEdit;
        private ToolStripButton btnDelete;
        private ToolStripButton btnRefresh;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnViewPurchases;
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
        private List<CustomerDto> _customers = new();
        private List<CustomerDto> _allCustomers = new();

        // Pagination state
        private int _currentPage = 1;
        private int _pageSize = 25;
        private int _totalRecords = 0;
        private int _totalPages = 0;

        // Search timer for debouncing
        private System.Windows.Forms.Timer _searchTimer;

        public CustomerForm(ILogger<CustomerForm> logger, IApiService apiService)
            {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Initialize non-nullable fields
            dgvCustomers = new DataGridView();
            toolStrip = new ToolStrip();
            statusStrip = new StatusStrip();
            txtSearch = new TextBox();
            btnSearch = new Button();
            btnClear = new Button();
            btnAdd = new ToolStripButton();
            btnEdit = new ToolStripButton();
            btnDelete = new ToolStripButton();
            btnRefresh = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnViewPurchases = new ToolStripButton();
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
            InitializeAsync();
            }

        private void InitializeComponent()
            {
            this.Text = "Customer Management - Inventory Pro";
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
                Height = 60,
                Dock = DockStyle.Top
                };

            btnAdd = new ToolStripButton
                {
                Text = "➕ Add Customer",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                Margin = new Padding(5, 0, 15, 0),
                Padding = new Padding(15, 8, 15, 8)
                };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new ToolStripButton
                {
                Text = "✏️ Edit Customer",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255),
                Margin = new Padding(5, 0, 15, 0),
                Padding = new Padding(15, 8, 15, 8)
                };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new ToolStripButton
                {
                Text = "🗑️ Delete Customer",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69),
                Margin = new Padding(5, 0, 15, 0),
                Padding = new Padding(15, 8, 15, 8)
                };
            btnDelete.Click += BtnDelete_Click;

            toolStripSeparator1 = new ToolStripSeparator();

            btnViewPurchases = new ToolStripButton
                {
                Text = "📊 View Purchases",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(102, 16, 242),
                Margin = new Padding(5, 0, 15, 0),
                Padding = new Padding(15, 8, 15, 8)
                };
            btnViewPurchases.Click += BtnViewPurchases_Click;

            btnRefresh = new ToolStripButton
                {
                Text = "🔄 Refresh Data",
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
                btnAdd, btnEdit, btnDelete, toolStripSeparator1, btnViewPurchases, btnRefresh, btnExport
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
                Text = "🔍 Search Customers:",
                Location = new Point(25, 20),
                Size = new Size(140, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
                };

            txtSearch = new TextBox
                {
                Location = new Point(175, 18),
                Size = new Size(350, 28),
                PlaceholderText = "Search by name, email, phone, or address...",
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
                };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // Action buttons
            btnSearch = new Button
                {
                Text = "🔎 Search",
                Location = new Point(550, 16),
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
                Location = new Point(675, 16),
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
                lblSearch, txtSearch, btnSearch, btnClear
            });

            // Create modern data grid with premium styling and responsiveness
            dgvCustomers = new DataGridView
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
            dgvCustomers.DoubleClick += DgvCustomers_DoubleClick;
            dgvCustomers.RowPostPaint += DgvCustomers_RowPostPaint;
            dgvCustomers.CellFormatting += DgvCustomers_CellFormatting;

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
            this.Controls.Add(dgvCustomers);
            this.Controls.Add(pnlPagination);
            this.Controls.Add(pnlSearch);
            this.Controls.Add(toolStrip);
            this.Controls.Add(statusStrip);

            }

        private async void InitializeAsync()
            {
            await LoadCustomersAsync();
            }

        private async Task LoadCustomersAsync()
            {
            try
                {
                lblStatus.Text = "Loading customers...";
                UpdatePaginationButtons(false); // Disable buttons while loading

                var parameters = new PaginationParameters
                    {
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    SearchTerm = txtSearch.Text
                    };

                var response = await _apiService.GetCustomersAsync(parameters);
                if (response.Success && response.Data != null)
                    {
                    _customers = response.Data.Items;
                    _totalRecords = response.Data.TotalCount;
                    _totalPages = (int)Math.Ceiling((double)_totalRecords / _pageSize);

                    dgvCustomers.DataSource = null;
                    
                    if (_customers == null || !_customers.Any())
                    {
                        // Show "No data found" message
                        var emptyData = new List<CustomerDto>
                        {
                            new CustomerDto
                            {
                                Id = 0,
                                Name = "No customers found",
                                Email = "Please adjust your search criteria or add new customers",
                                Phone = "",
                                Address = "",
                                CreatedAt = DateTime.MinValue
                            }
                        };
                        dgvCustomers.DataSource = emptyData;
                    }
                    else
                    {
                        dgvCustomers.DataSource = _customers;
                    }
                    
                    ConfigureGridColumns();

                    UpdatePaginationInfo();
                    UpdatePaginationButtons(true);
                    lblStatus.Text = "Ready";
                    }
                else
                    {
                    lblStatus.Text = "Error loading customers";
                    UpdatePaginationButtons(true);
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error loading customers");
                lblStatus.Text = "Error loading customers";
                UpdatePaginationButtons(true);
                MessageBox.Show("Error loading customers. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        private async Task FilterAndUpdateGridAsync()
            {
            // Reset to first page when filtering
            _currentPage = 1;
            await LoadCustomersAsync();
            }

        private void ConfigureGridColumns()
            {
            // Configure responsive columns with modern styling and icons
            if (dgvCustomers.Columns != null && dgvCustomers.Columns.Count > 0)
                {
                // Hide the actual ID column and use row headers for sequential numbers
                var idColumn = dgvCustomers.Columns["Id"];
                if (idColumn != null)
                    {
                    idColumn.Visible = false;
                    }

                // Configure row headers for sequential numbering and add header title
                dgvCustomers.RowHeadersDefaultCellStyle = new DataGridViewCellStyle
                    {
                    BackColor = Color.FromArgb(52, 58, 64),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    SelectionBackColor = Color.FromArgb(52, 58, 64),
                    Padding = new Padding(5, 5, 5, 5),
                    WrapMode = DataGridViewTriState.False
                    };

                // Set the row header title
                if (dgvCustomers.TopLeftHeaderCell != null)
                    {
                    dgvCustomers.TopLeftHeaderCell.Value = "#";
                    dgvCustomers.TopLeftHeaderCell.Style = new DataGridViewCellStyle
                        {
                        BackColor = Color.FromArgb(52, 58, 64),
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        Alignment = DataGridViewContentAlignment.MiddleLeft,
                        SelectionBackColor = Color.FromArgb(52, 58, 64),
                        Padding = new Padding(5, 5, 5, 5)
                        };
                    }

                var nameColumn = dgvCustomers.Columns["Name"];
                if (nameColumn != null)
                    {
                    nameColumn.HeaderText = "👤 Customer Name";
                    nameColumn.MinimumWidth = 180;
                    nameColumn.FillWeight = 25;
                    nameColumn.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    nameColumn.DefaultCellStyle.ForeColor = Color.FromArgb(44, 62, 80);
                    }

                var emailColumn = dgvCustomers.Columns["Email"];
                if (emailColumn != null)
                    {
                    emailColumn.HeaderText = "📧 Email Address";
                    emailColumn.MinimumWidth = 200;
                    emailColumn.FillWeight = 30;
                    emailColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                    emailColumn.DefaultCellStyle.ForeColor = Color.FromArgb(0, 86, 179);
                    }

                var phoneColumn = dgvCustomers.Columns["Phone"];
                if (phoneColumn != null)
                    {
                    phoneColumn.HeaderText = "📞 Phone";
                    phoneColumn.MinimumWidth = 130;
                    phoneColumn.FillWeight = 15;
                    phoneColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                    phoneColumn.DefaultCellStyle.ForeColor = Color.FromArgb(73, 80, 87);
                    }

                var addressColumn = dgvCustomers.Columns["Address"];
                if (addressColumn != null)
                    {
                    addressColumn.HeaderText = "🏠 Address";
                    addressColumn.MinimumWidth = 150;
                    addressColumn.FillWeight = 20;
                    addressColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                    addressColumn.DefaultCellStyle.ForeColor = Color.FromArgb(73, 80, 87);
                    addressColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    }

                var totalPurchasesColumn = dgvCustomers.Columns["TotalPurchases"];
                if (totalPurchasesColumn != null)
                    {
                    totalPurchasesColumn.DefaultCellStyle.Format = "C2";
                    totalPurchasesColumn.HeaderText = "💰 Total Purchases";
                    totalPurchasesColumn.MinimumWidth = 130;
                    totalPurchasesColumn.FillWeight = 15;
                    totalPurchasesColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    totalPurchasesColumn.DefaultCellStyle.ForeColor = Color.FromArgb(46, 125, 50);
                    totalPurchasesColumn.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    totalPurchasesColumn.DefaultCellStyle.BackColor = Color.FromArgb(248, 255, 248);
                    }

                var orderCountColumn = dgvCustomers.Columns["OrderCount"];
                if (orderCountColumn != null)
                    {
                    orderCountColumn.HeaderText = "🛒 Orders";
                    orderCountColumn.MinimumWidth = 80;
                    orderCountColumn.FillWeight = 10;
                    orderCountColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    orderCountColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    orderCountColumn.DefaultCellStyle.ForeColor = Color.FromArgb(0, 123, 255);

                    // Force center alignment for all cell styles
                    if (orderCountColumn.CellTemplate != null)
                        {
                        orderCountColumn.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        orderCountColumn.CellTemplate.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                        orderCountColumn.CellTemplate.Style.ForeColor = Color.FromArgb(0, 123, 255);
                        }

                    }

                var lastOrderDateColumn = dgvCustomers.Columns["LastOrderDate"];
                if (lastOrderDateColumn != null)
                    {
                    lastOrderDateColumn.HeaderText = "📅 Last Order";
                    lastOrderDateColumn.DefaultCellStyle.Format = "MMM dd, yyyy";
                    lastOrderDateColumn.MinimumWidth = 120;
                    lastOrderDateColumn.FillWeight = 15;
                    lastOrderDateColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                    lastOrderDateColumn.DefaultCellStyle.ForeColor = Color.FromArgb(102, 16, 242);
                    }

                // Hide unnecessary columns
                var createdAtColumn = dgvCustomers.Columns["CreatedAt"];
                if (createdAtColumn != null)
                    {
                    createdAtColumn.Visible = false;
                    }

                var isActiveColumn = dgvCustomers.Columns["IsActive"];
                if (isActiveColumn != null)
                    {
                    isActiveColumn.Visible = false;
                    }
                }
            }

        private async void BtnAdd_Click(object? sender, EventArgs e)
            {
            using (var dialog = new CustomerEditDialog())
                {
                if (dialog.ShowDialog() == DialogResult.OK)
                    {
                    try
                        {
                        var response = await _apiService.CreateCustomerAsync(dialog.Customer);
                        if (response.Success)
                            {
                            await LoadCustomersAsync();
                            MessageBox.Show("✅ Customer created successfully!",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        else
                            {
                            MessageBox.Show($"Error creating customer: {response.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    catch (Exception ex)
                        {
                        _logger.LogError(ex, "Error creating customer");
                        MessageBox.Show("Error creating customer.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

        private async void BtnEdit_Click(object? sender, EventArgs e)
            {
            if (dgvCustomers.SelectedRows.Count == 0)
                {
                MessageBox.Show("Please select a customer to edit.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
                }

            var selectedCustomer = dgvCustomers.SelectedRows[0].DataBoundItem as CustomerDto;
            if (selectedCustomer == null) return;
            
            // Prevent editing the "No customers found" placeholder
            if (selectedCustomer.Name == "No customers found")
            {
                return;
            }

            using (var dialog = new CustomerEditDialog(selectedCustomer))
                {
                if (dialog.ShowDialog() == DialogResult.OK)
                    {
                    try
                        {
                        var response = await _apiService.UpdateCustomerAsync(
                            selectedCustomer.Id, dialog.Customer);
                        if (response.Success)
                            {
                            await LoadCustomersAsync();
                            MessageBox.Show("✅ Customer updated successfully!",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        else
                            {
                            MessageBox.Show($"Error updating customer: {response.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    catch (Exception ex)
                        {
                        _logger.LogError(ex, "Error updating customer");
                        MessageBox.Show("Error updating customer.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

        private async void BtnDelete_Click(object? sender, EventArgs e)
            {
            if (dgvCustomers.SelectedRows.Count == 0)
                {
                MessageBox.Show("Please select a customer to delete.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
                }

            var selectedCustomer = dgvCustomers.SelectedRows[0].DataBoundItem as CustomerDto;
            if (selectedCustomer == null) return;
            
            // Prevent deleting the "No customers found" placeholder
            if (selectedCustomer.Name == "No customers found")
            {
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete customer '{selectedCustomer.Name}'?\n\n" +
                $"Total Purchases: {selectedCustomer.TotalPurchases:C}\n" +
                $"Order Count: {selectedCustomer.OrderCount}",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                {
                try
                    {
                    var response = await _apiService.DeleteCustomerAsync(selectedCustomer.Id);
                    if (response.Success)
                        {
                        await LoadCustomersAsync();
                        MessageBox.Show("✅ Customer deleted successfully!",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    else
                        {
                        MessageBox.Show($"Error deleting customer: {response.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                catch (Exception ex)
                    {
                    _logger.LogError(ex, "Error deleting customer");
                    MessageBox.Show("Error deleting customer.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        private void BtnViewPurchases_Click(object? sender, EventArgs e)
            {
            if (dgvCustomers.SelectedRows.Count == 0)
                {
                MessageBox.Show("Please select a customer to view purchases.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
                }

            var selectedCustomer = dgvCustomers.SelectedRows[0].DataBoundItem as CustomerDto;
            if (selectedCustomer == null) return;
            
            // Prevent viewing purchases for the "No customers found" placeholder
            if (selectedCustomer.Name == "No customers found")
            {
                return;
            }

            // TODO: Open a form to show customer purchase history
            MessageBox.Show($"📊 Purchase History for {selectedCustomer.Name}\n\n" +
                $"💰 Total Purchases: {selectedCustomer.TotalPurchases:C}\n" +
                $"🛒 Order Count: {selectedCustomer.OrderCount}\n" +
                $"📅 Last Order: {selectedCustomer.LastOrderDate?.ToString("MMM dd, yyyy") ?? "No orders yet"}",
                "Customer Purchase Analytics", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
            {
            _currentPage = 1; // Reset to first page when refreshing
            await LoadCustomersAsync();
            }

        private async void BtnExport_Click(object? sender, EventArgs e)
            {
            try
                {
                if (_customers == null || !_customers.Any())
                    {
                    MessageBox.Show("No customer data to export.", "No Data",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                    }

                using var exportDialog = new CustomerExportDialog();
                if (exportDialog.ShowDialog() == DialogResult.OK)
                    {
                    lblStatus.Text = "Exporting customers...";

                    bool success = false;
                    switch (exportDialog.SelectedFormat)
                        {
                        case ExportFormat.CSV:
                            success = await ExportToCsvAsync(exportDialog.ExportOptions);
                            break;
                        case ExportFormat.Excel:
                            success = await ExportToExcelAsync(exportDialog.ExportOptions);
                            break;
                        case ExportFormat.PDF:
                            success = await ExportToPdfAsync(exportDialog.ExportOptions);
                            break;
                        }

                    if (success)
                        {
                        lblStatus.Text = "Export completed successfully";
                        var result = MessageBox.Show(
                            $"Customer data exported successfully!\n\nWould you like to open the file location?",
                            "Export Successful", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                            {
                            System.Diagnostics.Process.Start("explorer.exe",
                                $"/select,\"{exportDialog.ExportOptions.FilePath}\"");
                            }
                        }
                    else
                        {
                        lblStatus.Text = "Export failed";
                        }
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during customer export");
                MessageBox.Show("An error occurred during export. Please try again.",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Export failed";
                }
            }

        private async void BtnSearch_Click(object? sender, EventArgs e)
            {
            await FilterAndUpdateGridAsync();
            }

        private async void BtnClear_Click(object? sender, EventArgs e)
            {
            txtSearch.Clear();
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

        private void DgvCustomers_DoubleClick(object? sender, EventArgs e)
            {
            BtnEdit_Click(sender, e);
            }

        private void DgvCustomers_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
            {
            // Draw sequential row numbers in row headers
            var dgv = sender as DataGridView;
            if (dgv == null) return;

            var rowNumber = ((_currentPage - 1) * _pageSize + e.RowIndex + 1).ToString();

            var bounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, dgv.RowHeadersWidth, e.RowBounds.Height);

            using (var brush = new SolidBrush(Color.White))
            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                var sf = new StringFormat
                    {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                    };

                e.Graphics.DrawString(rowNumber, font, brush, bounds, sf);
                }
            }

        private void DgvCustomers_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
            {
            var dgv = sender as DataGridView;
            if (dgv == null || dgv.Rows.Count <= e.RowIndex || e.RowIndex < 0) return;

            var customer = dgv.Rows[e.RowIndex].DataBoundItem as CustomerDto;
            if (customer == null) return;

            // Check if this is the "No data found" row
            if (customer.Name == "No customers found")
            {
                e.CellStyle.BackColor = Color.FromArgb(249, 250, 251);
                e.CellStyle.ForeColor = Color.FromArgb(107, 114, 128);
                e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Italic);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                
                // Hide values for numeric/date columns in empty data row
                if (dgv.Columns[e.ColumnIndex].Name == "CreatedAt" && customer.CreatedAt == DateTime.MinValue)
                {
                    e.Value = "";
                    e.FormattingApplied = true;
                }
                else if (dgv.Columns[e.ColumnIndex].Name == "OrderCount")
                {
                    e.Value = "";
                    e.FormattingApplied = true;
                }
                return;
            }

            // Force center alignment for OrderCount column regardless of row style
            if (dgv.Columns[e.ColumnIndex].Name == "OrderCount")
                {
                var cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                cell.Style.ForeColor = Color.FromArgb(0, 123, 255);
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
                await LoadCustomersAsync();
                }
            }

        private async void BtnPrevPage_Click(object? sender, EventArgs e)
            {
            if (_currentPage > 1)
                {
                _currentPage--;
                await LoadCustomersAsync();
                }
            }

        private async void BtnNextPage_Click(object? sender, EventArgs e)
            {
            if (_currentPage < _totalPages)
                {
                _currentPage++;
                await LoadCustomersAsync();
                }
            }

        private async void BtnLastPage_Click(object? sender, EventArgs e)
            {
            if (_currentPage < _totalPages)
                {
                _currentPage = _totalPages;
                await LoadCustomersAsync();
                }
            }

        private async void CboPageSize_SelectedIndexChanged(object? sender, EventArgs e)
            {
            if (cboPageSize.SelectedItem != null && int.TryParse(cboPageSize.SelectedItem.ToString(), out int newPageSize))
                {
                _pageSize = newPageSize;
                _currentPage = 1; // Reset to first page when changing page size
                await LoadCustomersAsync();
                }
            }

        #endregion

        #region Export Methods

        private async Task<bool> ExportToCsvAsync(ExportOptions options)
            {
            try
                {
                using var writer = new StreamWriter(options.FilePath);
                using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

                if (options.IncludeHeaders)
                    {
                    csv.WriteField("Customer Name");
                    csv.WriteField("Email Address");
                    csv.WriteField("Phone Number");
                    csv.WriteField("Address");
                    csv.WriteField("Total Purchases");
                    csv.WriteField("Order Count");
                    csv.WriteField("Last Order Date");
                    await csv.NextRecordAsync();
                    }

                foreach (var customer in _customers)
                    {
                    csv.WriteField(customer.Name ?? "");
                    csv.WriteField(customer.Email ?? "");
                    csv.WriteField(customer.Phone ?? "");
                    csv.WriteField(customer.Address ?? "");
                    csv.WriteField(customer.TotalPurchases.ToString("C2"));
                    csv.WriteField(customer.OrderCount.ToString());
                    csv.WriteField(customer.LastOrderDate?.ToString("yyyy-MM-dd") ?? "");
                    await csv.NextRecordAsync();
                    }

                if (options.IncludeSummary)
                    {
                    await csv.NextRecordAsync();
                    csv.WriteField("SUMMARY");
                    await csv.NextRecordAsync();
                    csv.WriteField($"Total Customers: {_customers.Count}");
                    await csv.NextRecordAsync();
                    csv.WriteField($"Total Customer Value: {_customers.Sum(c => c.TotalPurchases):C2}");
                    await csv.NextRecordAsync();
                    csv.WriteField($"Average Customer Value: {(_customers.Any() ? _customers.Average(c => c.TotalPurchases) : 0):C2}");
                    await csv.NextRecordAsync();
                    csv.WriteField($"Active Customers: {_customers.Count(c => c.OrderCount > 0)}");
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
                _logger.LogError(ex, "Error exporting customers to CSV");
                MessageBox.Show($"Error exporting customers to CSV: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
                }
            }

        private async Task<bool> ExportToExcelAsync(ExportOptions options)
            {
            try
                {
                var fileInfo = new FileInfo(options.FilePath);
                using var package = new ExcelPackage(fileInfo);

                var worksheet = package.Workbook.Worksheets.Add("Customers");

                int row = 1;

                if (options.IncludeHeaders)
                    {
                    var headers = new[] { "Customer Name", "Email Address", "Phone Number",
                        "Address", "Total Purchases", "Order Count", "Last Order Date" };

                    for (int col = 1; col <= headers.Length; col++)
                        {
                        worksheet.Cells[row, col].Value = headers[col - 1];
                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 58, 64));
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.White);
                        }
                    row++;
                    }

                foreach (var customer in _customers)
                    {
                    worksheet.Cells[row, 1].Value = customer.Name ?? "";
                    worksheet.Cells[row, 2].Value = customer.Email ?? "";
                    worksheet.Cells[row, 3].Value = customer.Phone ?? "";
                    worksheet.Cells[row, 4].Value = customer.Address ?? "";
                    worksheet.Cells[row, 5].Value = customer.TotalPurchases;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                    worksheet.Cells[row, 6].Value = customer.OrderCount;
                    worksheet.Cells[row, 7].Value = customer.LastOrderDate?.ToString("yyyy-MM-dd") ?? "";

                    if (customer.OrderCount == 0)
                        {
                        worksheet.Cells[row, 1, row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 242, 242));
                        }
                    else if (customer.TotalPurchases > 1000)
                        {
                        worksheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 255, 240));
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

                    worksheet.Cells[row, 1].Value = "Total Customers:";
                    worksheet.Cells[row, 2].Value = _customers.Count;
                    row++;

                    worksheet.Cells[row, 1].Value = "Total Customer Value:";
                    worksheet.Cells[row, 2].Value = _customers.Sum(c => c.TotalPurchases);
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                    row++;

                    worksheet.Cells[row, 1].Value = "Average Customer Value:";
                    worksheet.Cells[row, 2].Value = _customers.Any() ? _customers.Average(c => c.TotalPurchases) : 0;
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                    row++;

                    worksheet.Cells[row, 1].Value = "Active Customers:";
                    worksheet.Cells[row, 2].Value = _customers.Count(c => c.OrderCount > 0);
                    row++;
                    }

                if (options.IncludeTimestamp)
                    {
                    row += 2;
                    worksheet.Cells[row, 1].Value = $"Exported on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    worksheet.Cells[row, 1].Style.Font.Italic = true;
                    }

                worksheet.Cells.AutoFitColumns();

                for (int col = 1; col <= 7; col++)
                    {
                    worksheet.Column(col).Width = Math.Max(worksheet.Column(col).Width, 15);
                    }

                await package.SaveAsync();
                return true;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error exporting customers to Excel");
                MessageBox.Show($"Error exporting customers to Excel: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
                }
            }

        private async Task<bool> ExportToPdfAsync(ExportOptions options)
            {
            try
                {
                using var writer = new PdfWriter(options.FilePath);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());
                document.SetMargins(30, 20, 30, 20);

                var title = new Paragraph("Customer Report")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetBold()
                    .SetMarginBottom(20);
                document.Add(title);

                var table = new Table(7)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginTop(10)
                    .SetMarginBottom(10);

                table.SetWidth(UnitValue.CreatePercentValue(100));

                if (options.IncludeHeaders)
                    {
                    var headers = new[] { "Customer Name", "Email Address", "Phone",
                        "Address", "Total Purchases", "Orders", "Last Order" };

                    foreach (var header in headers)
                        {
                        var cell = new Cell()
                            .Add(new Paragraph(header))
                            .SetBackgroundColor(DeviceRgb.BLACK)
                            .SetFontColor(DeviceRgb.WHITE)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetBold()
                            .SetFontSize(10)
                            .SetPadding(8);
                        table.AddCell(cell);
                        }
                    }

                foreach (var customer in _customers)
                    {
                    table.AddCell(new Cell().Add(new Paragraph(customer.Name ?? "")).SetPadding(5).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(customer.Email ?? "")).SetPadding(5).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(customer.Phone ?? "")).SetPadding(5).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(customer.Address ?? "")).SetPadding(5).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(customer.TotalPurchases.ToString("C2")))
                        .SetPadding(5).SetFontSize(9).SetTextAlignment(TextAlignment.RIGHT));
                    table.AddCell(new Cell().Add(new Paragraph(customer.OrderCount.ToString()))
                        .SetPadding(5).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(customer.LastOrderDate?.ToString("MMM dd, yyyy") ?? ""))
                        .SetPadding(5).SetFontSize(9));
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

                    summaryTable.AddCell(new Cell().Add(new Paragraph("Total Customers:")).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));
                    summaryTable.AddCell(new Cell().Add(new Paragraph(_customers.Count.ToString())).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));

                    summaryTable.AddCell(new Cell().Add(new Paragraph("Total Customer Value:")).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));
                    summaryTable.AddCell(new Cell().Add(new Paragraph(_customers.Sum(c => c.TotalPurchases).ToString("C2"))).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));

                    summaryTable.AddCell(new Cell().Add(new Paragraph("Average Customer Value:")).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));
                    summaryTable.AddCell(new Cell().Add(new Paragraph((_customers.Any() ? _customers.Average(c => c.TotalPurchases) : 0).ToString("C2"))).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));

                    summaryTable.AddCell(new Cell().Add(new Paragraph("Active Customers:")).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));
                    summaryTable.AddCell(new Cell().Add(new Paragraph(_customers.Count(c => c.OrderCount > 0).ToString())).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(3).SetFontSize(9));

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
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error exporting customers to PDF");
                MessageBox.Show($"Error exporting customers to PDF: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
                }
            }

        private string GetFileExtension(ExportFormat format)
            {
            return format switch
                {
                    ExportFormat.CSV => ".csv",
                    ExportFormat.Excel => ".xlsx",
                    ExportFormat.PDF => ".pdf",
                    _ => ".csv"
                    };
            }

        #endregion
        }

    /// <summary>
    /// Modern Customer Edit Dialog with improved layout and styling
    /// </summary>
    public class CustomerEditDialog : Form
        {
        private readonly CustomerDto? _existingCustomer;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CustomerDto Customer { get; private set; } = null!;

        private TextBox txtName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private TextBox txtAddress = null!;
        private Button btnOK = null!;
        private Button btnCancel = null!;

        public CustomerEditDialog() : this(null)
            {
            }

        public CustomerEditDialog(CustomerDto? existingCustomer)
            {
            _existingCustomer = existingCustomer;

            InitializeComponent();
            SetupForm();

            if (_existingCustomer != null)
                {
                PopulateFields();
                }
            }

        private void InitializeComponent()
            {
            this.SuspendLayout();

            // Initialize controls
            txtName = new TextBox();
            txtEmail = new TextBox();
            txtPhone = new TextBox();
            txtAddress = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();

            // Form properties - Modern design with larger size and professional styling
            this.Text = _existingCustomer == null ? "Add New Customer" : "Edit Customer";
            this.Size = new Size(620, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(235, 240, 245);
            this.Font = new Font("Segoe UI", 9F);
            this.Padding = new Padding(30, 30, 30, 40);

            // Add form title at the top
            var titleLabel = new Label
                {
                Text = _existingCustomer == null ? "CREATE NEW CUSTOMER" : "EDIT CUSTOMER DETAILS",
                Location = new Point(40, 15),
                Size = new Size(520, 45),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.None
                };
            titleLabel.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, titleLabel.Height - 3, titleLabel.Width, 3);
                using (var brush = new LinearGradientBrush(rect, Color.FromArgb(0, 123, 255), Color.FromArgb(74, 144, 226), LinearGradientMode.Horizontal))
                    {
                    e.Graphics.FillRectangle(brush, rect);
                    }
            };
            this.Controls.Add(titleLabel);

            // Customer Name - Improved layout with bigger input and better spacing
            var lblName = new Label
                {
                Text = "",
                Location = new Point(40, 80),
                Size = new Size(250, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
                };
            lblName.Paint += (s, e) =>
            {
                var label = s as Label;
                if (label != null)
                    {
                    var fullText = "Customer Name: *";
                    var asteriskIndex = fullText.LastIndexOf('*');
                    if (asteriskIndex >= 0)
                        {
                        var beforeAsterisk = fullText.Substring(0, asteriskIndex);
                        var asterisk = fullText.Substring(asteriskIndex);

                        using (var normalBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                        using (var redBrush = new SolidBrush(Color.Red))
                            {
                            var normalSize = e.Graphics.MeasureString(beforeAsterisk, label.Font);
                            e.Graphics.DrawString(beforeAsterisk, label.Font, normalBrush, 0, 0);
                            e.Graphics.DrawString(asterisk, label.Font, redBrush, normalSize.Width, 0);
                            }
                        }
                    }
            };



            this.Controls.Add(lblName);
            txtName = new TextBox
                {
                Location = new Point(40, 108),
                Size = new Size(520, 42),
                Font = new Font("Segoe UI", 13),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 255, 255),
                ForeColor = Color.FromArgb(33, 37, 41),
                Padding = new Padding(15, 12, 15, 12)
                };
            txtName.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, txtName.ClientRectangle, Color.FromArgb(0, 123, 255), ButtonBorderStyle.Solid);
            txtName.Enter += (s, e) => { txtName.BackColor = Color.FromArgb(245, 251, 255); txtName.Invalidate(); };
            txtName.Leave += (s, e) => { txtName.BackColor = Color.FromArgb(255, 255, 255); txtName.Invalidate(); };

            // Email - Modern design with more spacing
            var lblEmail = new Label
                {
                Text = "📧 Email Address:",
                Location = new Point(40, 180),
                Size = new Size(200, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
                };

            txtEmail = new TextBox
                {
                Location = new Point(40, 208),
                Size = new Size(520, 42),
                Font = new Font("Segoe UI", 13),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 255, 255),
                ForeColor = Color.FromArgb(33, 37, 41),
                Padding = new Padding(15, 12, 15, 12),
                PlaceholderText = "customer@example.com"
                };
            txtEmail.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, txtEmail.ClientRectangle, Color.FromArgb(0, 123, 255), ButtonBorderStyle.Solid);
            txtEmail.Enter += (s, e) => { txtEmail.BackColor = Color.FromArgb(245, 251, 255); txtEmail.Invalidate(); };
            txtEmail.Leave += (s, e) => { txtEmail.BackColor = Color.FromArgb(255, 255, 255); txtEmail.Invalidate(); };

            // Phone - Modern design with better spacing
            var lblPhone = new Label
                {
                Text = "📞 Phone Number:",
                Location = new Point(40, 280),
                Size = new Size(200, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
                };

            txtPhone = new TextBox
                {
                Location = new Point(40, 308),
                Size = new Size(520, 42),
                Font = new Font("Segoe UI", 13),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 255, 255),
                ForeColor = Color.FromArgb(33, 37, 41),
                Padding = new Padding(15, 12, 15, 12),
                PlaceholderText = "+1 (555) 123-4567"
                };
            txtPhone.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, txtPhone.ClientRectangle, Color.FromArgb(0, 123, 255), ButtonBorderStyle.Solid);
            txtPhone.Enter += (s, e) => { txtPhone.BackColor = Color.FromArgb(245, 251, 255); txtPhone.Invalidate(); };
            txtPhone.Leave += (s, e) => { txtPhone.BackColor = Color.FromArgb(255, 255, 255); txtPhone.Invalidate(); };

            // Address - Modern textarea design with better spacing
            var lblAddress = new Label
                {
                Text = "🏠 Address:",
                Location = new Point(40, 380),
                Size = new Size(150, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.Transparent
                };

            txtAddress = new TextBox
                {
                Location = new Point(40, 408),
                Size = new Size(520, 95),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 255, 255),
                ForeColor = Color.FromArgb(33, 37, 41),
                Padding = new Padding(15, 12, 15, 12),
                PlaceholderText = "Street address, city, state, zip code..."
                };
            txtAddress.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, txtAddress.ClientRectangle, Color.FromArgb(0, 123, 255), ButtonBorderStyle.Solid);
            txtAddress.Enter += (s, e) => { txtAddress.BackColor = Color.FromArgb(245, 251, 255); txtAddress.Invalidate(); };
            txtAddress.Leave += (s, e) => { txtAddress.BackColor = Color.FromArgb(255, 255, 255); txtAddress.Invalidate(); };

            // Modern Buttons with round borders and improved positioning
            var buttonPanel = new Panel
                {
                Location = new Point(90, 545),
                Size = new Size(460, 85),
                BackColor = Color.Transparent
                };

            btnOK = new Button
                {
                Text = "SAVE CUSTOMER",
                Location = new Point(50, 20),
                Size = new Size(180, 55),
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
            btnOK.Paint += (s, e) =>
            {
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
                Text = "CANCEL",
                Location = new Point(250, 20),
                Size = new Size(180, 55),
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
            btnCancel.Paint += (s, e) =>
            {
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
                lblName,
                txtName,
                lblEmail, txtEmail,
                lblPhone, txtPhone,
                lblAddress, txtAddress,
                buttonPanel
            });

            this.ResumeLayout(false);
            }

        private void SetupForm()
            {
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
            }

        private void PopulateFields()
            {
            if (_existingCustomer != null)
                {
                txtName.Text = _existingCustomer.Name ?? string.Empty;
                txtEmail.Text = _existingCustomer.Email ?? string.Empty;
                txtPhone.Text = _existingCustomer.Phone ?? string.Empty;
                txtAddress.Text = _existingCustomer.Address ?? string.Empty;
                }
            }

        private void BtnOK_Click(object? sender, EventArgs e)
            {
            // Clear any existing validation errors first
            ClearValidationErrors();

            var validationErrors = new List<(Control control, string message)>();

            // Validate input with modern inline validation
            if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                validationErrors.Add((txtName, "Customer name is required"));
                }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                try
                    {
                    var addr = new System.Net.Mail.MailAddress(txtEmail.Text);
                    }
                catch
                    {
                    validationErrors.Add((txtEmail, "Please enter a valid email address"));
                    }
                }

            // Show validation errors with modern UI
            if (validationErrors.Any())
                {
                ShowValidationErrors(validationErrors);
                this.DialogResult = DialogResult.None;
                return;
                }

            // Create customer DTO
            Customer = new CustomerDto
                {
                Id = _existingCustomer?.Id ?? 0,
                Name = txtName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Address = txtAddress.Text.Trim()
                };
            }

        private void ShowValidationErrors(List<(Control control, string message)> errors)
            {
            foreach (var (control, message) in errors)
                {
                // Highlight the control with error styling
                HighlightErrorControl(control);

                // Create and show inline error message
                ShowInlineError(control, message);
                }

            // Focus on the first error control
            errors.First().control.Focus();

            // Show modern notification toast
            ShowValidationToast(errors.Count > 1
                ? $"{errors.Count} validation errors found. Please check the highlighted fields."
                : errors.First().message);
            }

        private void HighlightErrorControl(Control control)
            {
            // Add red border and background tint
            control.BackColor = Color.FromArgb(255, 242, 242);

            // Store original paint handler to restore later
            if (!control.Tag?.ToString()?.Contains("error-highlighted") ?? true)
                {
                control.Tag = "error-highlighted";

                // Add red border effect
                control.Paint += ErrorBorderPaint;
                control.Invalidate();
                }
            }

        private void ErrorBorderPaint(object? sender, PaintEventArgs e)
            {
            var control = sender as Control;
            if (control != null)
                {
                using (var pen = new Pen(Color.FromArgb(220, 53, 69), 2))
                    {
                    var rect = new Rectangle(0, 0, control.Width - 1, control.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                    }
                }
            }

        private void ShowInlineError(Control control, string message)
            {
            // Remove existing error label if any
            var existingError = this.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Name == $"error_{control.Name}");
            if (existingError != null)
                {
                this.Controls.Remove(existingError);
                existingError.Dispose();
                }

            // Create modern error label
            var errorLabel = new Label
                {
                Name = $"error_{control.Name}",
                Text = $"⚠️ {message}",
                Location = new Point(control.Left, control.Bottom + 5),
                Size = new Size(control.Width, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69),
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
                };

            this.Controls.Add(errorLabel);
            errorLabel.BringToFront();

            // Auto-remove error after user starts typing
            if (control is TextBox textBox)
                {
                textBox.TextChanged += (s, e) => ClearFieldError(control);
                }
            }

        private void ShowValidationToast(string message)
            {
            // Create modern toast notification
            var toast = new Form
                {
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                ShowInTaskbar = false,
                Size = new Size(400, 80)
                };

            // Position at top of parent form
            var parentLocation = this.PointToScreen(Point.Empty);
            toast.Location = new Point(
                parentLocation.X + (this.Width - toast.Width) / 2,
                parentLocation.Y + 60
            );

            // Add content to toast
            var iconLabel = new Label
                {
                Text = "⚠️",
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.White,
                Location = new Point(15, 25),
                Size = new Size(30, 30),
                TextAlign = ContentAlignment.MiddleCenter
                };

            var messageLabel = new Label
                {
                Text = message,
                Location = new Point(50, 15),
                Size = new Size(335, 50),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
                };

            toast.Controls.AddRange(new Control[] { iconLabel, messageLabel });

            // Round corners
            toast.Paint += (s, e) =>
            {
                var path = new GraphicsPath();
                var rect = new Rectangle(0, 0, toast.Width, toast.Height);
                int radius = 10;
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                toast.Region = new Region(path);
            };

            // Show with fade-in animation
            toast.Show();
            toast.Opacity = 0;

            var fadeTimer = new System.Windows.Forms.Timer { Interval = 30 };
            var opacity = 0.0;
            fadeTimer.Tick += (s, e) =>
            {
                opacity += 0.05;
                toast.Opacity = Math.Min(opacity, 0.95);
                if (opacity >= 0.95)
                    {
                    fadeTimer.Stop();
                    fadeTimer.Dispose();
                    }
            };
            fadeTimer.Start();

            // Auto-hide after 3 seconds with fade-out
            var hideTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            hideTimer.Tick += (s, e) =>
            {
                hideTimer.Stop();
                hideTimer.Dispose();

                var fadeOutTimer = new System.Windows.Forms.Timer { Interval = 30 };
                fadeOutTimer.Tick += (s2, e2) =>
                {
                    toast.Opacity -= 0.05;
                    if (toast.Opacity <= 0)
                        {
                        fadeOutTimer.Stop();
                        fadeOutTimer.Dispose();
                        toast.Close();
                        toast.Dispose();
                        }
                };
                fadeOutTimer.Start();
            };
            hideTimer.Start();
            }

        private void ClearFieldError(Control control)
            {
            // Remove error styling
            if (control.Tag?.ToString()?.Contains("error-highlighted") ?? false)
                {
                control.BackColor = Color.FromArgb(255, 255, 255);
                control.Paint -= ErrorBorderPaint;
                control.Tag = null;
                control.Invalidate();
                }

            // Remove error label
            var errorLabel = this.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Name == $"error_{control.Name}");
            if (errorLabel != null)
                {
                this.Controls.Remove(errorLabel);
                errorLabel.Dispose();
                }
            }

        private void ClearValidationErrors()
            {
            // Clear all error styling and labels
            var textBoxes = new[] { txtName, txtEmail, txtPhone, txtAddress };
            foreach (var textBox in textBoxes)
                {
                ClearFieldError(textBox);
                }
            }
        }

    /// <summary>
    /// Export format options for customer data
    /// </summary>
    public enum ExportFormatForCustomer
        {
        CSV,
        Excel,
        PDF
        }

    /// <summary>
    /// Export options configuration
    /// </summary>
    public class ExportOptions
        {

        public string FilePath { get; set; } = string.Empty;
        public bool IncludeHeaders { get; set; } = true;
        public bool IncludeSummary { get; set; } = true;
        public bool IncludeTimestamp { get; set; } = true;
        }

    /// <summary>
    /// Modern Customer Export Dialog with professional styling
    /// </summary>
    public class CustomerExportDialog : Form
        {
        public ExportFormat SelectedFormat { get; private set; } = ExportFormat.CSV;
        public ExportOptions ExportOptions { get; private set; } = new();

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

        public CustomerExportDialog()
            {
            InitializeComponent();
            SetupEventHandlers();
            UpdatePreview();
            }

        private void InitializeComponent()
            {
            this.Text = "Export Customer Data";
            this.Size = new Size(680, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F);

            var titleLabel = new Label
                {
                Text = "EXPORT CUSTOMER DATA",
                Location = new Point(30, 20),
                Size = new Size(590, 35),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255),
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
                BackColor = Color.FromArgb(40, 167, 69),
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
            txtFilePath.Text = Path.Combine(desktopPath, $"Customers_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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
            preview += "• Customer Name\n• Email Address\n• Phone Number\n• Address\n";
            preview += "• Total Purchases\n• Order Count\n• Last Order Date";

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

            SelectedFormat = rbCSV.Checked ? ExportFormat.CSV :
                           rbExcel.Checked ? ExportFormat.Excel :
                           ExportFormat.PDF;

            ExportOptions = new ExportOptions
                {
                FilePath = txtFilePath.Text,
                IncludeHeaders = chkIncludeHeaders.Checked,
                IncludeSummary = chkIncludeSummary.Checked,
                IncludeTimestamp = chkIncludeTimestamp.Checked
                };
            }
        }
    }