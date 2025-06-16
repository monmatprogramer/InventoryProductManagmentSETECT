using Microsoft.Extensions.Logging;
using InventoryPro.WinForms.Services;
using InventoryPro.Shared.DTOs;
using System.ComponentModel;

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

        // Data
        private List<CustomerDto> _customers = new();
        private List<CustomerDto> _allCustomers = new();
        
        // Search timer for debouncing
        private System.Windows.Forms.Timer _searchTimer;

        public CustomerForm(ILogger<CustomerForm> logger, IApiService apiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

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

            // Initialize search timer
            _searchTimer = new System.Windows.Forms.Timer();
            _searchTimer.Interval = 500; // 500ms delay
            _searchTimer.Tick += SearchTimer_Tick;

            InitializeComponent();
            InitializeAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Customer Management";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar
            toolStrip = new ToolStrip();

            btnAdd = new ToolStripButton
            {
                Text = "➕ Add Customer",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69)
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new ToolStripButton
            {
                Text = "✏️ Edit",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255)
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new ToolStripButton
            {
                Text = "🗑️ Delete",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69)
            };
            btnDelete.Click += BtnDelete_Click;

            toolStripSeparator1 = new ToolStripSeparator();

            btnViewPurchases = new ToolStripButton
            {
                Text = "📊 View Purchases",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(102, 16, 242)
            };
            btnViewPurchases.Click += BtnViewPurchases_Click;

            btnRefresh = new ToolStripButton
            {
                Text = "🔄 Refresh",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(108, 117, 125)
            };
            btnRefresh.Click += BtnRefresh_Click;

            btnExport = new ToolStripButton
            {
                Text = "📤 Export",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 162, 184)
            };
            btnExport.Click += BtnExport_Click;

            toolStrip.Items.AddRange(new ToolStripItem[] {
                btnAdd, btnEdit, btnDelete, toolStripSeparator1,
                btnViewPurchases, btnRefresh, btnExport
            });

            // Create search panel
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(15, 20),
                Size = new Size(70, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            txtSearch = new TextBox
            {
                Location = new Point(90, 17),
                Size = new Size(350, 25),
                PlaceholderText = "Search by name, email, or phone...",
                Font = new Font("Segoe UI", 10)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            btnSearch = new Button
            {
                Text = "🔍 Search",
                Location = new Point(455, 16),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 105, 217);
            btnSearch.Click += BtnSearch_Click;

            btnClear = new Button
            {
                Text = "🔄 Clear",
                Location = new Point(565, 16),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 98, 104);
            btnClear.Click += BtnClear_Click;

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearch, btnClear });

            // Create data grid
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
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9),
                RowTemplate = { Height = 35 },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(52, 58, 64),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    SelectionBackColor = Color.FromArgb(52, 58, 64),
                    Padding = new Padding(10, 8, 10, 8)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(33, 37, 41),
                    SelectionBackColor = Color.FromArgb(0, 123, 255),
                    SelectionForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 5, 10, 5)
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 249, 250),
                    ForeColor = Color.FromArgb(33, 37, 41),
                    SelectionBackColor = Color.FromArgb(0, 123, 255),
                    SelectionForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 5, 10, 5)
                }
            };
            dgvCustomers.DoubleClick += DgvCustomers_DoubleClick;

            // Status strip
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel { Text = "Ready" };
            lblRecordCount = new ToolStripStatusLabel { Text = "0 records" };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblRecordCount });

            // Add controls to form
            this.Controls.Add(dgvCustomers);
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

                var parameters = new PaginationParameters
                {
                    PageNumber = 1,
                    PageSize = 1000, // Load more records for better filtering
                    SearchTerm = ""
                };

                var response = await _apiService.GetCustomersAsync(parameters);
                if (response.Success && response.Data != null)
                {
                    _allCustomers = response.Data.Items;
                    FilterAndUpdateGrid();
                    lblStatus.Text = "Ready";
                }
                else
                {
                    lblStatus.Text = "Error loading customers";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers");
                lblStatus.Text = "Error loading customers";
                MessageBox.Show("Error loading customers. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterAndUpdateGrid()
        {
            var filteredCustomers = _allCustomers.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchTerm = txtSearch.Text.ToLower();
                filteredCustomers = filteredCustomers.Where(c => 
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm) ||
                    c.Phone.ToLower().Contains(searchTerm) ||
                    c.Address.ToLower().Contains(searchTerm));
            }

            _customers = filteredCustomers.ToList();
            UpdateGrid();
            lblRecordCount.Text = $"{_customers.Count} of {_allCustomers.Count} records";
        }

        private void UpdateGrid()
        {
            dgvCustomers.DataSource = null;
            dgvCustomers.DataSource = _customers;

            // Configure columns
            if (dgvCustomers.Columns != null && dgvCustomers.Columns.Count > 0)
            {
                var idColumn = dgvCustomers.Columns["Id"];
                if (idColumn != null)
                {
                    idColumn.Width = 60;
                    idColumn.HeaderText = "ID";
                    idColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                var nameColumn = dgvCustomers.Columns["Name"];
                if (nameColumn != null)
                {
                    nameColumn.Width = 180;
                    nameColumn.HeaderText = "👤 Customer Name";
                }

                var emailColumn = dgvCustomers.Columns["Email"];
                if (emailColumn != null)
                {
                    emailColumn.Width = 220;
                    emailColumn.HeaderText = "📧 Email Address";
                }

                var phoneColumn = dgvCustomers.Columns["Phone"];
                if (phoneColumn != null)
                {
                    phoneColumn.Width = 130;
                    phoneColumn.HeaderText = "📞 Phone";
                }

                var addressColumn = dgvCustomers.Columns["Address"];
                if (addressColumn != null)
                {
                    addressColumn.Width = 200;
                    addressColumn.HeaderText = "🏠 Address";
                }

                var totalPurchasesColumn = dgvCustomers.Columns["TotalPurchases"];
                if (totalPurchasesColumn != null)
                {
                    totalPurchasesColumn.DefaultCellStyle.Format = "C2";
                    totalPurchasesColumn.HeaderText = "💰 Total Purchases";
                    totalPurchasesColumn.Width = 140;
                    totalPurchasesColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    totalPurchasesColumn.DefaultCellStyle.ForeColor = Color.FromArgb(40, 167, 69);
                    totalPurchasesColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }

                var orderCountColumn = dgvCustomers.Columns["OrderCount"];
                if (orderCountColumn != null)
                {
                    orderCountColumn.HeaderText = "🛒 Orders";
                    orderCountColumn.Width = 80;
                    orderCountColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                var lastOrderDateColumn = dgvCustomers.Columns["LastOrderDate"];
                if (lastOrderDateColumn != null)
                {
                    lastOrderDateColumn.HeaderText = "📅 Last Order";
                    lastOrderDateColumn.DefaultCellStyle.Format = "MMM dd, yyyy";
                    lastOrderDateColumn.Width = 120;
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

            // TODO: Open a form to show customer purchase history
            MessageBox.Show($"📊 Purchase History for {selectedCustomer.Name}\n\n" +
                $"💰 Total Purchases: {selectedCustomer.TotalPurchases:C}\n" +
                $"🛒 Order Count: {selectedCustomer.OrderCount}\n" +
                $"📅 Last Order: {selectedCustomer.LastOrderDate?.ToString("MMM dd, yyyy") ?? "No orders yet"}",
                "Customer Purchase Analytics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            await LoadCustomersAsync();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
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
            FilterAndUpdateGrid();
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
            if (_allCustomers.Any()) // Only filter if we have loaded customers
            {
                FilterAndUpdateGrid();
            }
        }

        private void DgvCustomers_DoubleClick(object? sender, EventArgs e)
        {
            BtnEdit_Click(sender, e);
        }
    }

    /// <summary>
    /// Dialog for editing customer information
    /// </summary>
    public class CustomerEditDialog : Form
    {
        private TextBox txtName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtAddress;
        private Button btnOK;
        private Button btnCancel;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CustomerDto Customer { get; private set; } = new CustomerDto();

        public CustomerEditDialog(CustomerDto? existingCustomer = null)
        {
            txtName = new TextBox();
            txtEmail = new TextBox();
            txtPhone = new TextBox();
            txtAddress = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();

            InitializeComponent(existingCustomer);
        }

        private void InitializeComponent(CustomerDto? existingCustomer)
        {
            this.Text = existingCustomer == null ? "➕ Add New Customer" : "✏️ Edit Customer";
            this.Size = new Size(480, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);

            var lblName = new Label
            {
                Text = "👤 Customer Name:",
                Location = new Point(25, 25),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            txtName = new TextBox
            {
                Location = new Point(25, 50),
                Size = new Size(410, 25),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };

            var lblEmail = new Label
            {
                Text = "📧 Email Address:",
                Location = new Point(25, 90),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            txtEmail = new TextBox
            {
                Location = new Point(25, 115),
                Size = new Size(410, 25),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };

            var lblPhone = new Label
            {
                Text = "📞 Phone Number:",
                Location = new Point(25, 155),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            txtPhone = new TextBox
            {
                Location = new Point(25, 180),
                Size = new Size(410, 25),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };

            var lblAddress = new Label
            {
                Text = "🏠 Address:",
                Location = new Point(25, 220),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            txtAddress = new TextBox
            {
                Location = new Point(25, 245),
                Size = new Size(410, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };

            btnOK = new Button
            {
                Text = "✅ Save Customer",
                Location = new Point(265, 330),
                Size = new Size(120, 35),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 142, 58);
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "❌ Cancel",
                Location = new Point(395, 330),
                Size = new Size(90, 35),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 98, 104);

            this.Controls.AddRange(new Control[] {
                lblName, txtName, lblEmail, txtEmail,
                lblPhone, txtPhone, lblAddress, txtAddress,
                btnOK, btnCancel
            });

            // Load existing customer data
            if (existingCustomer != null)
            {
                txtName.Text = existingCustomer.Name;
                txtEmail.Text = existingCustomer.Email;
                txtPhone.Text = existingCustomer.Phone;
                txtAddress.Text = existingCustomer.Address;
            }
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a customer name.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(txtEmail.Text);
                }
                catch
                {
                    MessageBox.Show("Please enter a valid email address.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }
            }

            // Create customer DTO
            Customer = new CustomerDto
            {
                Name = txtName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Address = txtAddress.Text.Trim()
            };
        }
    }
}