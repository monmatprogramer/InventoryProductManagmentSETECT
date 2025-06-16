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
        // private Panel pnlDetails = null!; // Unused field
        private GroupBox grpSaleInfo = null!;
        // private GroupBox grpItems = null!; // Unused field
        private DataGridView dgvSaleItems = null!;
        private Button btnRefresh = null!;
        // private Button btnExport = null!; // Unused field
        // private Button btnClose = null!; // Unused field
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

            SetupControls(); // Your manual control setup
            this.Load += SalesDetailsForm_Load;
        }

        private void SetupControls()
        {
            // Your manual control initialization code here
        }

        private async void SalesDetailsForm_Load(object? sender, EventArgs e)
        {
            await LoadSalesDataAsync();
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