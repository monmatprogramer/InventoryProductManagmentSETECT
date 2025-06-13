using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using System.Drawing.Printing;
using System.Text;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Modern invoice form for generating and printing professional invoices
    /// </summary>
    public partial class InvoiceForm : Form
    {
        private readonly ILogger<InvoiceForm> _logger;
        private readonly IApiService _apiService;
        private CompanyInfoDto _companyInfo;

        public InvoiceForm(ILogger<InvoiceForm> logger, IApiService apiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            
            // Default company info (in real app, this would come from settings)
            _companyInfo = new CompanyInfoDto
            {
                Name = "InventoryPro Solutions",
                Address = "123 Business Street\nSuite 100\nCity, State 12345",
                Phone = "(555) 123-4567",
                Email = "info@inventorypro.com",
                Website = "www.inventorypro.com"
            };

            InitializeComponent(); // This calls the method from the Designer file
            SetupDataGridViewColumns();
            LoadCompanyInfo(); // Load company info into the form
        }

        private void SetupDataGridViewColumns()
        {
            // Configure column styles for better display
            colDescription.DefaultCellStyle.Padding = new Padding(12, 8, 5, 8);
            colDescription.DefaultCellStyle.Font = new Font("Segoe UI", 11);

            colQuantity.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colQuantity.DefaultCellStyle.Font = new Font("Segoe UI", 11);

            colUnitPrice.DefaultCellStyle.Format = "C2";
            colUnitPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colUnitPrice.DefaultCellStyle.Font = new Font("Segoe UI", 11);

            colTotal.DefaultCellStyle.Format = "C2";
            colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTotal.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        private void LoadCompanyInfo()
        {
            // Load company information into the form labels
            lblCompanyName.Text = _companyInfo.Name;
            lblCompanyAddress.Text = _companyInfo.Address;
            lblCompanyContact.Text = $"📞 Phone: {_companyInfo.Phone}\n📧 Email: {_companyInfo.Email}\n🌐 Web: {_companyInfo.Website}";
        }

        public void LoadSaleData(SaleDto sale)
        {
            try
            {
                if (sale == null)
                {
                    throw new ArgumentNullException(nameof(sale), "Sale data cannot be null");
                }

                _logger.LogInformation("Loading sale data for invoice. Sale ID: {SaleId}", sale.Id);

                // Update invoice details
                lblInvoiceNumber.Text = $"Invoice #: INV-{sale.Id:D6}";
                lblInvoiceDate.Text = $"Date: {sale.Date:MMM dd, yyyy}";
                lblDueDate.Text = $"Due Date: {sale.Date.AddDays(30):MMM dd, yyyy}";

                // Update customer info
                lblCustomerName.Text = sale.CustomerName ?? "Walk-in Customer";
                lblCustomerAddress.Text = "Customer Address"; // In real app, get from customer data

                // Validate items exist
                if (sale.Items == null || !sale.Items.Any())
                {
                    _logger.LogWarning("Sale {SaleId} has no items", sale.Id);
                    dgvItems.Rows.Clear();
                }
                else
                {
                    // Load items into DataGridView
                    dgvItems.Rows.Clear();
                    foreach (var item in sale.Items)
                    {
                        var itemTotal = (item.UnitPrice - item.DiscountAmount) * item.Quantity;
                        dgvItems.Rows.Add(
                            item.ProductName ?? "Unknown Product",
                            item.Quantity,
                            item.UnitPrice,
                            itemTotal
                        );
                    }

                    // Calculate totals
                    var subtotal = sale.Items.Sum(i => (i.UnitPrice - i.DiscountAmount) * i.Quantity);
                    var tax = subtotal * 0.10m; // Assuming 10% tax
                    var total = subtotal + tax;

                    lblSubtotal.Text = $"Subtotal: {subtotal:C}";
                    lblTax.Text = $"Tax (10%): {tax:C}";
                    lblTotal.Text = $"TOTAL: {total:C}";
                    lblAmountPaid.Text = $"Amount Paid: {sale.TotalAmount:C}";
                }

                this.Text = $"Invoice INV-{sale.Id:D6}";
                
                _logger.LogInformation("Successfully loaded sale data for invoice {SaleId}", sale.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sale data for invoice. Sale ID: {SaleId}", sale?.Id ?? 0);
                MessageBox.Show($"Error loading sale data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Re-throw to allow caller to handle
            }
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                var printDialog = new PrintDialog();
                var printDocument = new PrintDocument();
                printDocument.PrintPage += PrintDocument_PrintPage;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.PrinterSettings = printDialog.PrinterSettings;
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing invoice");
                MessageBox.Show("Error printing invoice", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            // Create a bitmap of the invoice form
            var bmp = new Bitmap(this.Width, this.Height);
            this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));

            // Scale and draw the bitmap to fit the page
            var graphics = e.Graphics!;
            var pageRect = e.PageBounds;
            var scale = Math.Min((float)pageRect.Width / bmp.Width, (float)pageRect.Height / bmp.Height);
            
            var scaledWidth = (int)(bmp.Width * scale);
            var scaledHeight = (int)(bmp.Height * scale);
            var x = (pageRect.Width - scaledWidth) / 2;
            var y = (pageRect.Height - scaledHeight) / 2;

            graphics.DrawImage(bmp, x, y, scaledWidth, scaledHeight);
            bmp.Dispose();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Invoice_{lblInvoiceNumber.Text.Replace("Invoice #: ", "").Replace("-", "_")}.pdf"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // For now, save as image since PDF generation requires additional libraries
                    var bmp = new Bitmap(this.Width, this.Height);
                    this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));
                    
                    var pngPath = saveDialog.FileName.Replace(".pdf", ".png");
                    bmp.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);
                    bmp.Dispose();
                    
                    MessageBox.Show($"Invoice saved as PNG: {pngPath}\n\n(PDF generation requires additional libraries)",
                        "Invoice Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving invoice");
                MessageBox.Show("Error saving invoice", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEmail_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Email functionality will be implemented in a future update",
                "Email Invoice", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
    }

    /// <summary>
    /// Company information DTO for invoice
    /// </summary>
    public class CompanyInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
    }
}