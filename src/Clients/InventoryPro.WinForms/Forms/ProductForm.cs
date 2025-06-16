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
        }

        private void InitializeComponent()
        {
            this.Text = "Product Management";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar
            toolStrip = new ToolStrip();

            btnAdd = new ToolStripButton
            {
                Text = "Add",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new ToolStripButton
            {
                Text = "Edit",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new ToolStripButton
            {
                Text = "Delete",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };
            btnDelete.Click += BtnDelete_Click;

            toolStripSeparator1 = new ToolStripSeparator();

            btnRefresh = new ToolStripButton
            {
                Text = "Refresh",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };
            btnRefresh.Click += BtnRefresh_Click;

            btnExport = new ToolStripButton
            {
                Text = "Export",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };
            btnExport.Click += BtnExport_Click;

            toolStrip.Items.AddRange(new ToolStripItem[] {
                btnAdd, btnEdit, btnDelete, toolStripSeparator1, btnRefresh, btnExport
            });

            // Create search panel
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(15, 18),
                Size = new Size(65, 25),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            txtSearch = new TextBox
            {
                Location = new Point(85, 15),
                Size = new Size(200, 25),
                PlaceholderText = "Search products...",
                Font = new Font("Segoe UI", 9)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            var lblCategory = new Label
            {
                Text = "Category:",
                Location = new Point(305, 18),
                Size = new Size(75, 25),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            cboCategory = new ComboBox
            {
                Location = new Point(385, 15),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cboCategory.SelectedIndexChanged += CboCategory_SelectedIndexChanged;

            btnSearch = new Button
            {
                Text = "🔍 Search",
                Location = new Point(555, 14),
                Size = new Size(85, 27),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += BtnSearch_Click;

            btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(650, 14),
                Size = new Size(75, 27),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += BtnClear_Click;

            pnlSearch.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, lblCategory, cboCategory, btnSearch, btnClear
            });

            // Create data grid
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvProducts.DoubleClick += DgvProducts_DoubleClick;

            // Status strip
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel { Text = "Ready" };
            lblRecordCount = new ToolStripStatusLabel { Text = "0 records" };
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
            // Configure columns
            if (dgvProducts.Columns != null && dgvProducts.Columns.Count > 0)
            {
                var idColumn = dgvProducts.Columns["Id"];
                if (idColumn != null)
                {
                    idColumn.Width = 50;
                    idColumn.HeaderText = "ID";
                }

                var skuColumn = dgvProducts.Columns["SKU"];
                if (skuColumn != null)
                {
                    skuColumn.Width = 100;
                }

                var nameColumn = dgvProducts.Columns["Name"];
                if (nameColumn != null)
                {
                    nameColumn.Width = 200;
                }

                var priceColumn = dgvProducts.Columns["Price"];
                if (priceColumn != null)
                {
                    priceColumn.DefaultCellStyle.Format = "C2";
                    priceColumn.Width = 100;
                }

                var stockColumn = dgvProducts.Columns["Stock"];
                if (stockColumn != null)
                {
                    stockColumn.Width = 80;
                }

                var categoryNameColumn = dgvProducts.Columns["CategoryName"];
                if (categoryNameColumn != null)
                {
                    categoryNameColumn.HeaderText = "Category";
                    categoryNameColumn.Width = 120;
                }

                var descriptionColumn = dgvProducts.Columns["Description"];
                if (descriptionColumn != null)
                {
                    descriptionColumn.Width = 200;
                }

                // Hide some columns
                var categoryIdColumn = dgvProducts.Columns["CategoryId"];
                if (categoryIdColumn != null)
                {
                    categoryIdColumn.Visible = false;
                }

                var createdAtColumn = dgvProducts.Columns["CreatedAt"];
                if (createdAtColumn != null)
                {
                    createdAtColumn.Visible = false;
                }

                var updatedAtColumn = dgvProducts.Columns["UpdatedAt"];
                if (updatedAtColumn != null)
                {
                    updatedAtColumn.Visible = false;
                }

                var imageUrlColumn = dgvProducts.Columns["ImageUrl"];
                if (imageUrlColumn != null)
                {
                    imageUrlColumn.Visible = false;
                }

                var minStockColumn = dgvProducts.Columns["MinStock"];
                if (minStockColumn != null)
                {
                    minStockColumn.Visible = false;
                }

                var isActiveColumn = dgvProducts.Columns["IsActive"];
                if (isActiveColumn != null)
                {
                    isActiveColumn.Visible = false;
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
}