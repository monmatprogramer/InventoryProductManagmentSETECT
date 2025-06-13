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

        // UI Controls
        private Panel pnlHeader = null!;
        private Panel pnlCompanyInfo = null!;
        private Panel pnlCustomerInfo = null!;
        private Panel pnlInvoiceDetails = null!;
        private Panel pnlItems = null!;
        private Panel pnlTotals = null!;
        private Panel pnlFooter = null!;
        private Panel pnlActions = null!;

        private Label lblCompanyName = null!;
        private Label lblCompanyAddress = null!;
        private Label lblCompanyContact = null!;
        private Label lblInvoiceTitle = null!;
        private Label lblInvoiceNumber = null!;
        private Label lblInvoiceDate = null!;
        private Label lblDueDate = null!;

        private Label lblBillTo = null!;
        private Label lblCustomerName = null!;
        private Label lblCustomerAddress = null!;
        private DataGridView dgvItems = null!;
        private Label lblSubtotal = null!;
        private Label lblTax = null!;
        private Label lblTotal = null!;
        private Label lblAmountPaid = null!;

        private Label lblFooterText = null!;
        private Button btnPrint = null!;
        private Button btnSave = null!;
        private Button btnEmail = null!;
        private Button btnClose = null!;

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

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Invoice";
            this.Size = new Size(900, 700);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            this.WindowState = FormWindowState.Normal;
            this.AutoScroll = true;

            // Create main layout using TableLayoutPanel for better responsiveness
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(20),
                BackColor = Color.White,
                AutoScroll = true
            };

            // Configure row styles for responsive layout
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // Company/Customer Info
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));   // Items (main content)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Totals
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));  // Footer
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Actions

            CreateHeaderSection();
            CreateInfoSection();
            CreateItemsSection();
            CreateTotalsSection();
            CreateFooterSection();
            CreateActionsSection();

            // Add all sections to the main layout
            mainLayout.Controls.Add(pnlHeader, 0, 0);
            mainLayout.Controls.Add(pnlCompanyInfo, 0, 1);
            mainLayout.Controls.Add(pnlItems, 0, 2);
            mainLayout.Controls.Add(pnlTotals, 0, 3);
            mainLayout.Controls.Add(pnlFooter, 0, 4);
            mainLayout.Controls.Add(pnlActions, 0, 5);

            this.Controls.Add(mainLayout);
        }

        private void CreateHeaderSection()
        {
            pnlHeader = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(41, 128, 185),
                Padding = new Padding(10)
            };

            lblInvoiceTitle = new Label
            {
                Text = "INVOICE",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            pnlHeader.Controls.Add(lblInvoiceTitle);
        }

        private void CreateInfoSection()
        {
            pnlCompanyInfo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };

            // Create a responsive layout for company and customer info
            var infoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            // Configure column styles for responsive layout
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Company
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Customer  
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Invoice Details

            CreateCompanyInfoPanel();
            CreateCustomerInfoPanel();
            CreateInvoiceDetailsPanel();

            infoLayout.Controls.Add(pnlCompanyInfo, 0, 0);
            infoLayout.Controls.Add(pnlCustomerInfo, 1, 0);
            infoLayout.Controls.Add(pnlInvoiceDetails, 2, 0);

            pnlCompanyInfo.Controls.Clear();
            pnlCompanyInfo.Controls.Add(infoLayout);
        }

        private void CreateCompanyInfoPanel()
        {
            var companyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };

            lblCompanyName = new Label
            {
                Text = _companyInfo.Name,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblCompanyAddress = new Label
            {
                Text = _companyInfo.Address,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(102, 102, 102),
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.TopLeft
            };

            lblCompanyContact = new Label
            {
                Text = $"Phone: {_companyInfo.Phone}\nEmail: {_companyInfo.Email}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(102, 102, 102),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.TopLeft
            };

            companyPanel.Controls.Add(lblCompanyContact);
            companyPanel.Controls.Add(lblCompanyAddress);
            companyPanel.Controls.Add(lblCompanyName);

            pnlCompanyInfo = companyPanel;
        }

        private void CreateCustomerInfoPanel()
        {
            var customerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };

            lblBillTo = new Label
            {
                Text = "BILL TO:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblCustomerName = new Label
            {
                Text = "Customer Name",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblCustomerAddress = new Label
            {
                Text = "Customer Address",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(102, 102, 102),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.TopLeft
            };

            customerPanel.Controls.Add(lblCustomerAddress);
            customerPanel.Controls.Add(lblCustomerName);
            customerPanel.Controls.Add(lblBillTo);

            pnlCustomerInfo = customerPanel;
        }

        private void CreateInvoiceDetailsPanel()
        {
            var detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };

            lblInvoiceNumber = new Label
            {
                Text = "Invoice #: INV-001",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblInvoiceDate = new Label
            {
                Text = $"Date: {DateTime.Now:MMM dd, yyyy}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(102, 102, 102),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblDueDate = new Label
            {
                Text = $"Due Date: {DateTime.Now.AddDays(30):MMM dd, yyyy}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(102, 102, 102),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            detailsPanel.Controls.Add(lblDueDate);
            detailsPanel.Controls.Add(lblInvoiceDate);
            detailsPanel.Controls.Add(lblInvoiceNumber);

            pnlInvoiceDetails = detailsPanel;
        }

        private void CreateItemsSection()
        {
            pnlItems = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            // Items title
            var titleLabel = new Label
            {
                Text = "ITEMS",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(248, 248, 248),
                Padding = new Padding(10, 5, 5, 5)
            };

            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                GridColor = Color.FromArgb(230, 230, 230),
                Font = new Font("Segoe UI", 10),
                RowTemplate = { Height = 30 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 35
            };

            SetupItemsGrid();

            pnlItems.Controls.Add(dgvItems);
            pnlItems.Controls.Add(titleLabel);
        }

        private void SetupItemsGrid()
        {
            dgvItems.Columns.Clear();
            dgvItems.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn
                {
                    Name = "Description",
                    HeaderText = "Description",
                    Width = 300,
                    DefaultCellStyle = new DataGridViewCellStyle { Padding = new Padding(10, 5, 5, 5) }
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Quantity",
                    HeaderText = "Qty",
                    Width = 80,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "UnitPrice",
                    HeaderText = "Price",
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Total",
                    HeaderText = "Total",
                    Width = 120,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
                }
            });
        }

        private void CreateTotalsSection()
        {
            pnlTotals = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };

            // Create layout for totals - align to right
            var totalsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            totalsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Spacer
            totalsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Totals

            var totalsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 248, 248),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            lblSubtotal = new Label
            {
                Text = "Subtotal: $0.00",
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleRight
            };

            lblTax = new Label
            {
                Text = "Tax: $0.00",
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleRight
            };

            lblTotal = new Label
            {
                Text = "TOTAL: $0.00",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight
            };

            lblAmountPaid = new Label
            {
                Text = "Amount Paid: $0.00",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(46, 204, 113),
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleRight
            };

            totalsPanel.Controls.Add(lblAmountPaid);
            totalsPanel.Controls.Add(lblTotal);
            totalsPanel.Controls.Add(lblTax);
            totalsPanel.Controls.Add(lblSubtotal);

            totalsLayout.Controls.Add(new Panel(), 0, 0); // Spacer
            totalsLayout.Controls.Add(totalsPanel, 1, 0);

            pnlTotals.Controls.Add(totalsLayout);
        }

        private void CreateFooterSection()
        {
            pnlFooter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10)
            };

            lblFooterText = new Label
            {
                Text = "Thank you for your business!\n\nPayment is due within 30 days. For questions about this invoice, please contact us at info@inventorypro.com",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(102, 102, 102),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter
            };

            pnlFooter.Controls.Add(lblFooterText);
        }

        private void CreateActionsSection()
        {
            pnlActions = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10)
            };

            // Create centered button layout
            var buttonsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            // Configure columns for button layout
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20)); // Spacer
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // Print
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // Save
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // Email
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));  // Close
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80)); // Spacer

            btnPrint = new Button
            {
                Text = "Print",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(2)
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += BtnPrint_Click;

            btnSave = new Button
            {
                Text = "Save PDF",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(2)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnEmail = new Button
            {
                Text = "Email",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(2)
            };
            btnEmail.FlatAppearance.BorderSize = 0;
            btnEmail.Click += BtnEmail_Click;

            btnClose = new Button
            {
                Text = "Close",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Margin = new Padding(2)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            buttonsLayout.Controls.Add(new Panel(), 0, 0); // Spacer
            buttonsLayout.Controls.Add(btnPrint, 1, 0);
            buttonsLayout.Controls.Add(btnSave, 2, 0);
            buttonsLayout.Controls.Add(btnEmail, 3, 0);
            buttonsLayout.Controls.Add(btnClose, 4, 0);
            buttonsLayout.Controls.Add(new Panel(), 5, 0); // Spacer

            pnlActions.Controls.Add(buttonsLayout);
        }

        public void LoadSaleData(SaleDto sale)
        {
            try
            {
                // Update invoice details
                lblInvoiceNumber.Text = $"Invoice #: INV-{sale.Id:D6}";
                lblInvoiceDate.Text = $"Date: {sale.Date:MMM dd, yyyy}";
                lblDueDate.Text = $"Due Date: {sale.Date.AddDays(30):MMM dd, yyyy}";

                // Update customer info
                lblCustomerName.Text = sale.CustomerName ?? "Walk-in Customer";
                lblCustomerAddress.Text = "Customer Address"; // In real app, get from customer data

                // Load items
                var items = sale.Items.Select(item => new
                {
                    Description = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Total = (item.UnitPrice - item.DiscountAmount) * item.Quantity
                }).ToList();

                dgvItems.DataSource = items;

                // Calculate totals
                var subtotal = sale.Items.Sum(i => (i.UnitPrice - i.DiscountAmount) * i.Quantity);
                var tax = subtotal * 0.10m; // Assuming 10% tax
                var total = subtotal + tax;

                lblSubtotal.Text = $"Subtotal: {subtotal:C}";
                lblTax.Text = $"Tax (10%): {tax:C}";
                lblTotal.Text = $"TOTAL: {total:C}";
                lblAmountPaid.Text = $"Amount Paid: {sale.TotalAmount:C}";

                this.Text = $"Invoice INV-{sale.Id:D6}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sale data for invoice");
                MessageBox.Show("Error loading sale data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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