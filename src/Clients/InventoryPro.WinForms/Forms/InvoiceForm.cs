using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using InventoryPro.ReportService.Services;
using Microsoft.Extensions.Logging;
using System.Drawing.Printing;
using System.Text;
using System.Diagnostics;

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
        private SaleDto? _currentSale;

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

                _currentSale = sale; // Store the sale data for PDF generation
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
                if (_currentSale == null)
                {
                    MessageBox.Show("No invoice data available for printing.", "Print Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _logger.LogInformation("Generating PDF for printing invoice {SaleId}", _currentSale.Id);

                // Generate PDF in memory
                var pdfBytes = PdfGenerator.GenerateInvoicePdf(
                    _currentSale,
                    _companyInfo.Name,
                    _companyInfo.Address,
                    _companyInfo.Phone,
                    _companyInfo.Email
                );

                // Create a temporary file for printing
                var tempPath = Path.Combine(Path.GetTempPath(), $"Invoice_{_currentSale.Id:D6}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                File.WriteAllBytes(tempPath, pdfBytes);

                _logger.LogInformation("PDF generated successfully. Opening for printing: {TempPath}", tempPath);

                // Open the PDF with the default application for printing
                var startInfo = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    Verb = "print"
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    // Clean up temp file after some time
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromMinutes(2));
                        try
                        {
                            if (File.Exists(tempPath))
                                File.Delete(tempPath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not delete temporary PDF file: {TempPath}", tempPath);
                        }
                    });

                    MessageBox.Show("Invoice PDF has been generated and sent to your default PDF viewer for printing.", 
                        "Print Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"PDF generated successfully but could not open automatically.\nFile saved to: {tempPath}",
                        "Print Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing invoice");
                MessageBox.Show($"Error printing invoice: {ex.Message}", "Print Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_currentSale == null)
                {
                    MessageBox.Show("No invoice data available for saving.", "Save Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Invoice_INV-{_currentSale.Id:D6}.pdf",
                    Title = "Save Invoice as PDF"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    _logger.LogInformation("Generating PDF for saving invoice {SaleId} to {FilePath}", 
                        _currentSale.Id, saveDialog.FileName);

                    // Generate PDF in memory
                    var pdfBytes = PdfGenerator.GenerateInvoicePdf(
                        _currentSale,
                        _companyInfo.Name,
                        _companyInfo.Address,
                        _companyInfo.Phone,
                        _companyInfo.Email
                    );

                    // Save the PDF to the selected file
                    File.WriteAllBytes(saveDialog.FileName, pdfBytes);

                    _logger.LogInformation("PDF saved successfully to {FilePath}", saveDialog.FileName);

                    // Ask if user wants to open the saved PDF
                    var result = MessageBox.Show(
                        $"Invoice saved successfully as PDF!\n\nFile: {saveDialog.FileName}\n\nWould you like to open the file now?",
                        "Invoice Saved", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = saveDialog.FileName,
                                UseShellExecute = true
                            };
                            Process.Start(startInfo);
                        }
                        catch (Exception openEx)
                        {
                            _logger.LogWarning(openEx, "Could not open saved PDF file");
                            MessageBox.Show("PDF saved successfully, but could not open automatically.",
                                "File Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving invoice");
                MessageBox.Show($"Error saving invoice: {ex.Message}", "Save Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEmail_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_currentSale == null)
                {
                    MessageBox.Show("No invoice data available for emailing.", "Email Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _logger.LogInformation("Generating PDF for emailing invoice {SaleId}", _currentSale.Id);

                // Generate PDF in memory
                var pdfBytes = PdfGenerator.GenerateInvoicePdf(
                    _currentSale,
                    _companyInfo.Name,
                    _companyInfo.Address,
                    _companyInfo.Phone,
                    _companyInfo.Email
                );

                // Create a temporary file for emailing
                var tempPath = Path.Combine(Path.GetTempPath(), $"Invoice_INV-{_currentSale.Id:D6}.pdf");
                File.WriteAllBytes(tempPath, pdfBytes);

                // Create mailto URL with attachment (note: many email clients don't support attachments via mailto)
                var customerEmail = ""; // Customer email would be fetched from customer data
                var subject = Uri.EscapeDataString($"Invoice INV-{_currentSale.Id:D6} from {_companyInfo.Name}");
                var body = Uri.EscapeDataString($"Dear {_currentSale.CustomerName ?? "Valued Customer"},\n\n" +
                    $"Please find attached your invoice INV-{_currentSale.Id:D6}.\n\n" +
                    $"Invoice Details:\n" +
                    $"- Invoice Number: INV-{_currentSale.Id:D6}\n" +
                    $"- Date: {_currentSale.Date:MMM dd, yyyy}\n" +
                    $"- Amount: {_currentSale.TotalAmount:C}\n\n" +
                    $"Thank you for your business!\n\n" +
                    $"Best regards,\n{_companyInfo.Name}");

                var mailtoUrl = $"mailto:{customerEmail}?subject={subject}&body={body}";

                // Show instructions to user
                var result = MessageBox.Show(
                    $"Invoice PDF has been generated and saved to:\n{tempPath}\n\n" +
                    $"Click OK to open your default email client.\n" +
                    $"You will need to manually attach the PDF file to your email.\n\n" +
                    $"Note: Some email clients may not auto-populate all fields.",
                    "Email Invoice", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                if (result == DialogResult.OK)
                {
                    try
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = mailtoUrl,
                            UseShellExecute = true
                        };
                        Process.Start(startInfo);

                        // Also open the folder containing the PDF for easy attachment
                        Process.Start("explorer.exe", $"/select,\"{tempPath}\"");
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "Could not open email client");
                        MessageBox.Show(
                            $"Could not open email client automatically.\n\n" +
                            $"The invoice PDF has been saved to:\n{tempPath}\n\n" +
                            $"Please attach this file to your email manually.",
                            "Email Client Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing invoice for email");
                MessageBox.Show($"Error preparing invoice for email: {ex.Message}", "Email Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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