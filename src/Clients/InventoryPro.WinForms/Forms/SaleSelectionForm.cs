using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Form for selecting sales to generate invoices
    /// </summary>
    public partial class SaleSelectionForm : Form
    {
        private DataGridView dgvSales = null!;
        private Button btnSelectAll = null!;
        private Button btnDeselectAll = null!;
        private Button btnGenerate = null!;
        private Button btnCancel = null!;
        private Label lblInstructions = null!;

        private List<SaleDto> _sales;
        
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public List<SaleDto> SelectedSales { get; private set; } = new();

        public SaleSelectionForm(List<SaleDto> sales)
        {
            _sales = sales ?? throw new ArgumentNullException(nameof(sales));
            InitializeComponent();
            LoadSalesData();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Sales for Invoice Generation";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Instructions
            lblInstructions = new Label
            {
                Text = "Select the sales for which you want to generate invoices:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(760, 25)
            };

            // Sales grid
            dgvSales = new DataGridView
            {
                Location = new Point(20, 55),
                Size = new Size(760, 350),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };

            // Setup columns
            dgvSales.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewCheckBoxColumn
                {
                    Name = "Selected",
                    HeaderText = "Select",
                    Width = 60,
                    TrueValue = true,
                    FalseValue = false
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Id",
                    HeaderText = "Sale ID",
                    Width = 80,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Date",
                    HeaderText = "Date",
                    Width = 120,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "CustomerName",
                    HeaderText = "Customer",
                    Width = 200,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "TotalAmount",
                    HeaderText = "Total",
                    Width = 100,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Status",
                    Width = 80,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "PaymentMethod",
                    HeaderText = "Payment",
                    Width = 100,
                    ReadOnly = true
                }
            });

            // Selection buttons
            btnSelectAll = new Button
            {
                Text = "Select All",
                Location = new Point(20, 420),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSelectAll.FlatAppearance.BorderSize = 0;
            btnSelectAll.Click += BtnSelectAll_Click;

            btnDeselectAll = new Button
            {
                Text = "Deselect All",
                Location = new Point(130, 420),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDeselectAll.FlatAppearance.BorderSize = 0;
            btnDeselectAll.Click += BtnDeselectAll_Click;

            // Action buttons
            btnGenerate = new Button
            {
                Text = "Generate Invoices",
                Location = new Point(580, 420),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += BtnGenerate_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(710, 420),
                Size = new Size(70, 30),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblInstructions, dgvSales, btnSelectAll, btnDeselectAll, btnGenerate, btnCancel
            });
        }

        private void LoadSalesData()
        {
            try
            {
                var salesData = _sales.Select(sale => new
                {
                    Selected = false,
                    sale.Id,
                    Date = sale.Date.ToString("yyyy-MM-dd HH:mm"),
                    sale.CustomerName,
                    TotalAmount = sale.TotalAmount.ToString("C"),
                    sale.Status,
                    sale.PaymentMethod,
                    SaleObject = sale // Keep reference to original object
                }).ToList();

                dgvSales.DataSource = salesData;

                // Hide the SaleObject column
                var saleObjectColumn = dgvSales.Columns["SaleObject"];
                if (saleObjectColumn != null)
                {
                    saleObjectColumn.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSelectAll_Click(object? sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dgvSales.Rows.Count; i++)
                {
                    dgvSales.Rows[i].Cells["Selected"].Value = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting all sales: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeselectAll_Click(object? sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dgvSales.Rows.Count; i++)
                {
                    dgvSales.Rows[i].Cells["Selected"].Value = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deselecting all sales: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            try
            {
                SelectedSales.Clear();

                for (int i = 0; i < dgvSales.Rows.Count; i++)
                {
                    var isSelected = dgvSales.Rows[i].Cells["Selected"].Value;
                    if (isSelected is bool selected && selected)
                    {
                        var saleIdValue = dgvSales.Rows[i].Cells["Id"].Value;
                        if (saleIdValue is int saleId)
                        {
                            var sale = _sales.FirstOrDefault(s => s.Id == saleId);
                            if (sale != null)
                            {
                                SelectedSales.Add(sale);
                            }
                        }
                    }
                }

                if (SelectedSales.Count == 0)
                {
                    MessageBox.Show("Please select at least one sale to generate invoices.", 
                        "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing selection: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}