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
                Text = "Add Customer",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                Image = SystemIcons.Shield.ToBitmap()
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new ToolStripButton
            {
                Text = "Edit",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                Image = SystemIcons.Shield.ToBitmap()
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new ToolStripButton
            {
                Text = "Delete",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                Image = SystemIcons.Warning.ToBitmap()
            };
            btnDelete.Click += BtnDelete_Click;

            toolStripSeparator1 = new ToolStripSeparator();

            btnViewPurchases = new ToolStripButton
            {
                Text = "View Purchases",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                Image = SystemIcons.Information.ToBitmap()
            };
            btnViewPurchases.Click += BtnViewPurchases_Click;

            btnRefresh = new ToolStripButton
            {
                Text = "Refresh",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                Image = SystemIcons.Shield.ToBitmap()
            };
            btnRefresh.Click += BtnRefresh_Click;

            btnExport = new ToolStripButton
            {
                Text = "Export",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                Image = SystemIcons.Shield.ToBitmap()
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
                Height = 50,
                Padding = new Padding(10),
                BackColor = Color.WhiteSmoke
            };

            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(10, 15),
                Size = new Size(50, 25)
            };

            txtSearch = new TextBox
            {
                Location = new Point(70, 12),
                Size = new Size(300, 25),
                PlaceholderText = "Search by name, email, or phone..."
            };

            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(380, 11),
                Size = new Size(75, 27),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += BtnSearch_Click;

            btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(465, 11),
                Size = new Size(75, 27)
            };
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
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 248)
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
                    PageSize = 100,
                    SearchTerm = txtSearch.Text
                };

                var response = await _apiService.GetCustomersAsync(parameters);
                if (response.Success && response.Data != null)
                {
                    _customers = response.Data.Items;
                    UpdateGrid();
                    lblRecordCount.Text = $"{_customers.Count} records";
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
                    idColumn.Width = 50;
                }

                var nameColumn = dgvCustomers.Columns["Name"];
                if (nameColumn != null)
                {
                    nameColumn.Width = 200;
                }

                var emailColumn = dgvCustomers.Columns["Email"];
                if (emailColumn != null)
                {
                    emailColumn.Width = 200;
                }

                var phoneColumn = dgvCustomers.Columns["Phone"];
                if (phoneColumn != null)
                {
                    phoneColumn.Width = 120;
                }

                var addressColumn = dgvCustomers.Columns["Address"];
                if (addressColumn != null)
                {
                    addressColumn.Width = 250;
                }

                var totalPurchasesColumn = dgvCustomers.Columns["TotalPurchases"];
                if (totalPurchasesColumn != null)
                {
                    totalPurchasesColumn.DefaultCellStyle.Format = "C2";
                    totalPurchasesColumn.HeaderText = "Total Purchases";
                }

                var orderCountColumn = dgvCustomers.Columns["OrderCount"];
                if (orderCountColumn != null)
                {
                    orderCountColumn.HeaderText = "Orders";
                }

                var lastOrderDateColumn = dgvCustomers.Columns["LastOrderDate"];
                if (lastOrderDateColumn != null)
                {
                    lastOrderDateColumn.HeaderText = "Last Order";
                    lastOrderDateColumn.DefaultCellStyle.Format = "MM/dd/yyyy";
                }

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
                            MessageBox.Show("Customer created successfully.",
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
                            MessageBox.Show("Customer updated successfully.",
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
                    // Correct method name based on the IApiService signature
                    var response = await _apiService.UpdateCustomerAsync(selectedCustomer.Id, new CustomerDto { IsActive = false });
                    if (response.Success)
                    {
                        await LoadCustomersAsync();
                        MessageBox.Show("Customer deleted successfully.",
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
            MessageBox.Show($"Purchase history for {selectedCustomer.Name}:\n\n" +
                $"Total Purchases: {selectedCustomer.TotalPurchases:C}\n" +
                $"Order Count: {selectedCustomer.OrderCount}\n" +
                $"Last Order: {selectedCustomer.LastOrderDate?.ToString("MM/dd/yyyy") ?? "N/A"}",
                "Customer Purchases", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private async void BtnSearch_Click(object? sender, EventArgs e)
        {
            await LoadCustomersAsync();
        }

        private async void BtnClear_Click(object? sender, EventArgs e)
        {
            txtSearch.Clear();
            await LoadCustomersAsync();
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
            this.Text = existingCustomer == null ? "Add Customer" : "Edit Customer";
            this.Size = new Size(450, 400);
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
                Size = new Size(390, 25)
            };

            var lblEmail = new Label
            {
                Text = "Email:",
                Location = new Point(20, 80),
                Size = new Size(100, 25)
            };

            txtEmail = new TextBox
            {
                Location = new Point(20, 105),
                Size = new Size(390, 25)
            };

            var lblPhone = new Label
            {
                Text = "Phone:",
                Location = new Point(20, 140),
                Size = new Size(100, 25)
            };

            txtPhone = new TextBox
            {
                Location = new Point(20, 165),
                Size = new Size(390, 25)
            };

            var lblAddress = new Label
            {
                Text = "Address:",
                Location = new Point(20, 200),
                Size = new Size(100, 25)
            };

            txtAddress = new TextBox
            {
                Location = new Point(20, 225),
                Size = new Size(390, 60),
                Multiline = true
            };

            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(255, 310),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(335, 310),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

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