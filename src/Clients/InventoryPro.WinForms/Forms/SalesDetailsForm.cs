using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Sales details and history form
    /// Shows detailed sales information and allows viewing/managing sales records
    /// </summary>
    public partial class SalesDetailsForm : Form
    {
        private readonly ILogger<SalesDetailsForm> _logger;
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;

        // UI Controls
        private DataGridView dgvSales = null!;
        private Panel pnlDetails = null!;
        private GroupBox grpSaleInfo = null!;
        private GroupBox grpItems = null!;
        private DataGridView dgvSaleItems = null!;
        private Button btnRefresh = null!;
        private Button btnExport = null!;
        private Button btnClose = null!;
        private DateTimePicker dtpFromDate = null!;
        private DateTimePicker dtpToDate = null!;
        private ComboBox cboStatus = null!;
        private TextBox txtSearch = null!;

        // Data
        private List<SaleDto> _sales = new();
        private SaleDto? _selectedSale;

        public SalesDetailsForm(ILogger<SalesDetailsForm> logger, IApiService apiService, IAuthService authService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            InitializeComponent();
            _ = LoadSalesDataAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Sales Details & History";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 600);

            // Create main layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // Left panel - Sales list
            var leftPanel = new Panel { Dock = DockStyle.Fill };
            
            // Filter controls
            var filterPanel = new Panel 
            { 
                Height = 80, 
                Dock = DockStyle.Top,
                Padding = new Padding(5)
            };

            var lblFromDate = new Label
            {
                Text = "From:",
                Location = new Point(10, 15),
                Size = new Size(50, 20)
            };

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(65, 12),
                Size = new Size(120, 25),
                Value = DateTime.Today.AddDays(-30)
            };

            var lblToDate = new Label
            {
                Text = "To:",
                Location = new Point(200, 15),
                Size = new Size(30, 20)
            };

            dtpToDate = new DateTimePicker
            {
                Location = new Point(235, 12),
                Size = new Size(120, 25),
                Value = DateTime.Today
            };

            var lblStatus = new Label
            {
                Text = "Status:",
                Location = new Point(370, 15),
                Size = new Size(50, 20)
            };

            cboStatus = new ComboBox
            {
                Location = new Point(425, 12),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatus.Items.AddRange(new[] { "All", "Completed", "Pending", "Cancelled" });
            cboStatus.SelectedIndex = 0;

            txtSearch = new TextBox
            {
                Location = new Point(10, 45),
                Size = new Size(200, 25),
                PlaceholderText = "Search customer, sale ID..."
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(220, 44),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += BtnRefresh_Click;

            btnExport = new Button
            {
                Text = "Export",
                Location = new Point(310, 44),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;

            filterPanel.Controls.AddRange(new Control[] 
            { 
                lblFromDate, dtpFromDate, lblToDate, dtpToDate, 
                lblStatus, cboStatus, txtSearch, btnRefresh, btnExport 
            });

            // Sales grid
            dgvSales = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvSales.SelectionChanged += DgvSales_SelectionChanged;

            leftPanel.Controls.Add(dgvSales);
            leftPanel.Controls.Add(filterPanel);

            // Right panel - Sale details
            pnlDetails = new Panel 
            { 
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(248, 248, 248)
            };

            grpSaleInfo = new GroupBox
            {
                Text = "Sale Information",
                Location = new Point(10, 10),
                Size = new Size(450, 200),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            grpItems = new GroupBox
            {
                Text = "Sale Items",
                Location = new Point(10, 220),
                Size = new Size(450, 350),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            dgvSaleItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            grpItems.Controls.Add(dgvSaleItems);

            var btnInvoice = new Button
            {
                Text = "📄 Invoice",
                Location = new Point(270, 580),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnInvoice.FlatAppearance.BorderSize = 0;
            btnInvoice.Click += BtnInvoice_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(380, 580),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            pnlDetails.Controls.AddRange(new Control[] { grpSaleInfo, grpItems, btnInvoice, btnClose });

            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(pnlDetails, 1, 0);

            this.Controls.Add(mainPanel);

            // Setup data binding
            SetupDataGrids();
        }

        private void SetupDataGrids()
        {
            // Setup sales grid columns
            dgvSales.Columns.Clear();
            dgvSales.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Sale ID", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "Customer", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "Total", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "PaymentMethod", HeaderText = "Payment", Width = 80 }
            });

            // Setup sale items grid columns
            dgvSaleItems.Columns.Clear();
            dgvSaleItems.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Product", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "ProductSKU", HeaderText = "SKU", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Qty", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "Price", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total", Width = 80 }
            });
        }

        private async Task LoadSalesDataAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                dgvSales.DataSource = null;

                var response = await _apiService.GetSalesAsync(new PaginationParameters
                {
                    PageSize = 100,
                    SearchTerm = txtSearch.Text
                });

                if (response.Success && response.Data != null)
                {
                    _sales = response.Data.Items.ToList();
                    
                    // Apply filters
                    var filteredSales = _sales.AsEnumerable();

                    if (cboStatus.SelectedItem?.ToString() != "All")
                    {
                        filteredSales = filteredSales.Where(s => s.Status == cboStatus.SelectedItem?.ToString());
                    }

                    filteredSales = filteredSales.Where(s => 
                        s.Date.Date >= dtpFromDate.Value.Date && 
                        s.Date.Date <= dtpToDate.Value.Date);

                    // Bind to grid
                    var displayData = filteredSales.Select(s => new
                    {
                        s.Id,
                        Date = s.Date.ToString("yyyy-MM-dd HH:mm"),
                        s.CustomerName,
                        TotalAmount = s.TotalAmount.ToString("C"),
                        s.Status,
                        s.PaymentMethod
                    }).ToList();

                    dgvSales.DataSource = displayData;
                }
                else
                {
                    MessageBox.Show($"Failed to load sales data: {response.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sales data");
                MessageBox.Show("Error loading sales data", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private void DgvSales_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvSales.SelectedRows.Count > 0)
            {
                var selectedRow = dgvSales.SelectedRows[0];
                var saleId = selectedRow.Cells["Id"].Value is int id ? id : 0;
                
                _selectedSale = _sales.FirstOrDefault(s => s.Id == saleId);
                if (_selectedSale != null)
                {
                    DisplaySaleDetails(_selectedSale);
                }
            }
        }

        private void DisplaySaleDetails(SaleDto sale)
        {
            // Clear existing controls in sale info group
            grpSaleInfo.Controls.Clear();

            // Create detailed sale information
            var lblSaleId = new Label
            {
                Text = $"Sale ID: #{sale.Id}",
                Location = new Point(15, 25),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var lblCustomer = new Label
            {
                Text = $"Customer: {sale.CustomerName}",
                Location = new Point(15, 50),
                Size = new Size(300, 20)
            };

            var lblDate = new Label
            {
                Text = $"Date: {sale.Date:yyyy-MM-dd HH:mm:ss}",
                Location = new Point(15, 75),
                Size = new Size(300, 20)
            };

            var lblTotal = new Label
            {
                Text = $"Total Amount: {sale.TotalAmount:C}",
                Location = new Point(15, 100),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Green
            };

            var lblStatus = new Label
            {
                Text = $"Status: {sale.Status}",
                Location = new Point(15, 125),
                Size = new Size(150, 20)
            };

            var lblPayment = new Label
            {
                Text = $"Payment: {sale.PaymentMethod}",
                Location = new Point(15, 150),
                Size = new Size(200, 20)
            };

            grpSaleInfo.Controls.AddRange(new Control[] 
            { 
                lblSaleId, lblCustomer, lblDate, lblTotal, lblStatus, lblPayment 
            });

            // Display sale items
            var itemsDisplay = sale.Items.Select(item => new
            {
                item.ProductName,
                item.ProductSKU,
                item.Quantity,
                UnitPrice = item.UnitPrice.ToString("C"),
                Total = ((item.UnitPrice - item.DiscountAmount) * item.Quantity).ToString("C")
            }).ToList();

            dgvSaleItems.DataSource = itemsDisplay;
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            await LoadSalesDataAsync();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"Sales_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToCSV(saveDialog.FileName);
                    MessageBox.Show("Sales data exported successfully!", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting sales data");
                MessageBox.Show("Error exporting sales data", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(string fileName)
        {
            using var writer = new StreamWriter(fileName);
            
            // Write headers
            writer.WriteLine("Sale ID,Date,Customer,Total Amount,Status,Payment Method,Items Count");
            
            // Write data
            foreach (var sale in _sales)
            {
                writer.WriteLine($"{sale.Id},{sale.Date:yyyy-MM-dd HH:mm},{sale.CustomerName}," +
                    $"{sale.TotalAmount},{sale.Status},{sale.PaymentMethod},{sale.Items.Count}");
            }
        }

        private void BtnInvoice_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_selectedSale != null)
                {
                    using var invoiceForm = Program.GetRequiredService<InvoiceForm>();
                    invoiceForm.LoadSaleData(_selectedSale);
                    invoiceForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Please select a sale to generate invoice", "No Sale Selected", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening invoice form");
                MessageBox.Show("Error opening invoice form", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}