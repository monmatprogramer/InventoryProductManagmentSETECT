using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Form for selecting sales to generate invoices
    /// </summary>
    public partial class SaleSelectionForm : Form
    {
        private readonly List<SaleDto> _sales;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public List<SaleDto> SelectedSales { get; } = new();

        public SaleSelectionForm(List<SaleDto> sales)
        {
            _sales = sales ?? throw new ArgumentNullException(nameof(sales));
            InitializeComponent();
            LoadSalesData();
        }

        private void LoadSalesData()
        {
            try
            {
                // Use array for better performance and memory usage
                var salesData = new object[_sales.Count][];
                for (int i = 0; i < _sales.Count; i++)
                {
                    var sale = _sales[i];
                    salesData[i] = new object[]
                    {
                        false,
                        sale.Id,
                        sale.Date.ToString("yyyy-MM-dd HH:mm"),
                        sale.CustomerName,
                        sale.TotalAmount.ToString("C"),
                        sale.Status,
                        sale.PaymentMethod,
                        sale // SaleObject (hidden)
                    };
                }

                dgvSales.Rows.Clear();
                dgvSales.Columns["SaleObject"]?.Dispose();
                if (dgvSales.Columns["SaleObject"] == null)
                {
                    var hiddenCol = new DataGridViewTextBoxColumn
                    {
                        Name = "SaleObject",
                        Visible = false
                    };
                    dgvSales.Columns.Add(hiddenCol);
                }

                dgvSales.Rows.AddRange(salesData.Select(row => new DataGridViewRow
                {
                    Cells =
                    {
                        new DataGridViewCheckBoxCell { Value = row[0] },
                        new DataGridViewTextBoxCell { Value = row[1] },
                        new DataGridViewTextBoxCell { Value = row[2] },
                        new DataGridViewTextBoxCell { Value = row[3] },
                        new DataGridViewTextBoxCell { Value = row[4] },
                        new DataGridViewTextBoxCell { Value = row[5] },
                        new DataGridViewTextBoxCell { Value = row[6] },
                        new DataGridViewTextBoxCell { Value = row[7] }
                    }
                }).ToArray());
                dgvSales.Columns["SaleObject"]!.Visible = false;
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
                foreach (DataGridViewRow row in dgvSales.Rows)
                {
                    if (!row.IsNewRow)
                        row.Cells["Selected"].Value = true;
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
                foreach (DataGridViewRow row in dgvSales.Rows)
                {
                    if (!row.IsNewRow)
                        row.Cells["Selected"].Value = false;
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

                foreach (DataGridViewRow row in dgvSales.Rows)
                {
                    if (row.IsNewRow) continue;
                    var isSelected = row.Cells["Selected"].Value;
                    if (isSelected is bool selected && selected)
                    {
                        // Use the hidden SaleObject column for direct reference
                        if (row.Cells["SaleObject"].Value is SaleDto sale)
                        {
                            SelectedSales.Add(sale);
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