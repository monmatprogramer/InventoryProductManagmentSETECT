using System.Drawing;
using System.Windows.Forms;

namespace InventoryPro.WinForms.Forms
{
    partial class InvoiceForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
            {
            tableLayoutPanelMain = new TableLayoutPanel();
            panelHeader = new Panel();
            lblInvoiceTitle = new Label();
            panelInfo = new Panel();
            tableLayoutPanelInfo = new TableLayoutPanel();
            panelCompany = new Panel();
            lblCompanyContact = new Label();
            lblCompanyAddress = new Label();
            lblCompanyName = new Label();
            panelCustomer = new Panel();
            lblCustomerAddress = new Label();
            lblCustomerName = new Label();
            lblBillTo = new Label();
            panelInvoiceDetails = new Panel();
            lblDueDate = new Label();
            lblInvoiceDate = new Label();
            lblInvoiceNumber = new Label();
            panelItems = new Panel();
            dgvItems = new DataGridView();
            colDescription = new DataGridViewTextBoxColumn();
            colQuantity = new DataGridViewTextBoxColumn();
            colUnitPrice = new DataGridViewTextBoxColumn();
            colTotal = new DataGridViewTextBoxColumn();
            lblItemsTitle = new Label();
            panelTotals = new Panel();
            tableLayoutPanelTotals = new TableLayoutPanel();
            panelTotalsData = new Panel();
            lblAmountPaid = new Label();
            lblTotal = new Label();
            lblTax = new Label();
            lblSubtotal = new Label();
            panelFooter = new Panel();
            lblFooterText = new Label();
            panelActions = new Panel();
            btnClose = new Button();
            btnEmail = new Button();
            btnSave = new Button();
            btnPrint = new Button();
            tableLayoutPanelMain.SuspendLayout();
            panelHeader.SuspendLayout();
            panelInfo.SuspendLayout();
            tableLayoutPanelInfo.SuspendLayout();
            panelCompany.SuspendLayout();
            panelCustomer.SuspendLayout();
            panelInvoiceDetails.SuspendLayout();
            panelItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvItems).BeginInit();
            panelTotals.SuspendLayout();
            tableLayoutPanelTotals.SuspendLayout();
            panelTotalsData.SuspendLayout();
            panelFooter.SuspendLayout();
            panelActions.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            tableLayoutPanelMain.BackColor = Color.White;
            tableLayoutPanelMain.ColumnCount = 1;
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanelMain.Controls.Add(panelHeader, 0, 0);
            tableLayoutPanelMain.Controls.Add(panelInfo, 0, 1);
            tableLayoutPanelMain.Controls.Add(panelTotals, 0, 3);
            tableLayoutPanelMain.Controls.Add(panelActions, 0, 5);
            tableLayoutPanelMain.Controls.Add(panelFooter, 0, 4);
            tableLayoutPanelMain.Controls.Add(panelItems, 0, 2);
            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.Location = new Point(0, 0);
            tableLayoutPanelMain.Margin = new Padding(20);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.Padding = new Padding(20);
            tableLayoutPanelMain.RowCount = 6;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 235F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 213F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            tableLayoutPanelMain.Size = new Size(1231, 1108);
            tableLayoutPanelMain.TabIndex = 0;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(41, 128, 185);
            panelHeader.Controls.Add(lblInvoiceTitle);
            panelHeader.Dock = DockStyle.Fill;
            panelHeader.Location = new Point(23, 23);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(10);
            panelHeader.Size = new Size(1185, 74);
            panelHeader.TabIndex = 0;
            // 
            // lblInvoiceTitle
            // 
            lblInvoiceTitle.BackColor = Color.Transparent;
            lblInvoiceTitle.Dock = DockStyle.Fill;
            lblInvoiceTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblInvoiceTitle.ForeColor = Color.White;
            lblInvoiceTitle.Location = new Point(10, 10);
            lblInvoiceTitle.Name = "lblInvoiceTitle";
            lblInvoiceTitle.Size = new Size(1165, 54);
            lblInvoiceTitle.TabIndex = 0;
            lblInvoiceTitle.Text = "INVOICE";
            lblInvoiceTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelInfo
            // 
            panelInfo.BackColor = Color.Transparent;
            panelInfo.Controls.Add(tableLayoutPanelInfo);
            panelInfo.Dock = DockStyle.Fill;
            panelInfo.Location = new Point(23, 103);
            panelInfo.Name = "panelInfo";
            panelInfo.Padding = new Padding(5);
            panelInfo.Size = new Size(1185, 229);
            panelInfo.TabIndex = 1;
            // 
            // tableLayoutPanelInfo
            // 
            tableLayoutPanelInfo.BackColor = Color.Transparent;
            tableLayoutPanelInfo.ColumnCount = 3;
            tableLayoutPanelInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanelInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayoutPanelInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayoutPanelInfo.Controls.Add(panelCompany, 0, 0);
            tableLayoutPanelInfo.Controls.Add(panelCustomer, 1, 0);
            tableLayoutPanelInfo.Controls.Add(panelInvoiceDetails, 2, 0);
            tableLayoutPanelInfo.Dock = DockStyle.Fill;
            tableLayoutPanelInfo.Location = new Point(5, 5);
            tableLayoutPanelInfo.Name = "tableLayoutPanelInfo";
            tableLayoutPanelInfo.RowCount = 1;
            tableLayoutPanelInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelInfo.Size = new Size(1175, 219);
            tableLayoutPanelInfo.TabIndex = 0;
            // 
            // panelCompany
            // 
            panelCompany.BackColor = Color.Transparent;
            panelCompany.Controls.Add(lblCompanyContact);
            panelCompany.Controls.Add(lblCompanyAddress);
            panelCompany.Controls.Add(lblCompanyName);
            panelCompany.Dock = DockStyle.Fill;
            panelCompany.Location = new Point(3, 3);
            panelCompany.Name = "panelCompany";
            panelCompany.Padding = new Padding(5);
            panelCompany.Size = new Size(464, 213);
            panelCompany.TabIndex = 0;
            // 
            // lblCompanyContact
            // 
            lblCompanyContact.Dock = DockStyle.Top;
            lblCompanyContact.Font = new Font("Segoe UI", 11F);
            lblCompanyContact.ForeColor = Color.FromArgb(102, 102, 102);
            lblCompanyContact.Location = new Point(5, 125);
            lblCompanyContact.Name = "lblCompanyContact";
            lblCompanyContact.Size = new Size(454, 80);
            lblCompanyContact.TabIndex = 2;
            lblCompanyContact.Text = "📞 Phone: (555) 123-4567\r\n📧 Email: info@inventorypro.com\r\n🌐 Web: www.inventorypro.com";
            // 
            // lblCompanyAddress
            // 
            lblCompanyAddress.Dock = DockStyle.Top;
            lblCompanyAddress.Font = new Font("Segoe UI", 11F);
            lblCompanyAddress.ForeColor = Color.FromArgb(102, 102, 102);
            lblCompanyAddress.Location = new Point(5, 45);
            lblCompanyAddress.Name = "lblCompanyAddress";
            lblCompanyAddress.Size = new Size(454, 80);
            lblCompanyAddress.TabIndex = 1;
            lblCompanyAddress.Text = "123 Business Street\r\nSuite 100\r\nCity, State 12345";
            // 
            // lblCompanyName
            // 
            lblCompanyName.Dock = DockStyle.Top;
            lblCompanyName.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblCompanyName.ForeColor = Color.FromArgb(51, 51, 51);
            lblCompanyName.Location = new Point(5, 5);
            lblCompanyName.Name = "lblCompanyName";
            lblCompanyName.Size = new Size(454, 40);
            lblCompanyName.TabIndex = 0;
            lblCompanyName.Text = "InventoryPro Solutions";
            lblCompanyName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelCustomer
            // 
            panelCustomer.BackColor = Color.Transparent;
            panelCustomer.Controls.Add(lblCustomerAddress);
            panelCustomer.Controls.Add(lblCustomerName);
            panelCustomer.Controls.Add(lblBillTo);
            panelCustomer.Dock = DockStyle.Fill;
            panelCustomer.Location = new Point(473, 3);
            panelCustomer.Name = "panelCustomer";
            panelCustomer.Padding = new Padding(5);
            panelCustomer.Size = new Size(346, 213);
            panelCustomer.TabIndex = 1;
            // 
            // lblCustomerAddress
            // 
            lblCustomerAddress.Dock = DockStyle.Top;
            lblCustomerAddress.Font = new Font("Segoe UI", 9F);
            lblCustomerAddress.ForeColor = Color.FromArgb(102, 102, 102);
            lblCustomerAddress.Location = new Point(5, 60);
            lblCustomerAddress.Name = "lblCustomerAddress";
            lblCustomerAddress.Size = new Size(336, 40);
            lblCustomerAddress.TabIndex = 2;
            lblCustomerAddress.Text = "Customer Address";
            // 
            // lblCustomerName
            // 
            lblCustomerName.Dock = DockStyle.Top;
            lblCustomerName.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblCustomerName.ForeColor = Color.FromArgb(51, 51, 51);
            lblCustomerName.Location = new Point(5, 30);
            lblCustomerName.Name = "lblCustomerName";
            lblCustomerName.Size = new Size(336, 30);
            lblCustomerName.TabIndex = 1;
            lblCustomerName.Text = "Customer Name";
            lblCustomerName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblBillTo
            // 
            lblBillTo.Dock = DockStyle.Top;
            lblBillTo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblBillTo.ForeColor = Color.FromArgb(51, 51, 51);
            lblBillTo.Location = new Point(5, 5);
            lblBillTo.Name = "lblBillTo";
            lblBillTo.Size = new Size(336, 25);
            lblBillTo.TabIndex = 0;
            lblBillTo.Text = "BILL TO:";
            lblBillTo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelInvoiceDetails
            // 
            panelInvoiceDetails.BackColor = Color.Transparent;
            panelInvoiceDetails.Controls.Add(lblDueDate);
            panelInvoiceDetails.Controls.Add(lblInvoiceDate);
            panelInvoiceDetails.Controls.Add(lblInvoiceNumber);
            panelInvoiceDetails.Dock = DockStyle.Fill;
            panelInvoiceDetails.Location = new Point(825, 3);
            panelInvoiceDetails.Name = "panelInvoiceDetails";
            panelInvoiceDetails.Padding = new Padding(5);
            panelInvoiceDetails.Size = new Size(347, 213);
            panelInvoiceDetails.TabIndex = 2;
            // 
            // lblDueDate
            // 
            lblDueDate.Dock = DockStyle.Top;
            lblDueDate.Font = new Font("Segoe UI", 9F);
            lblDueDate.ForeColor = Color.FromArgb(102, 102, 102);
            lblDueDate.Location = new Point(5, 55);
            lblDueDate.Name = "lblDueDate";
            lblDueDate.Size = new Size(337, 25);
            lblDueDate.TabIndex = 2;
            lblDueDate.Text = "Due Date: Dec 13, 2024";
            lblDueDate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblInvoiceDate
            // 
            lblInvoiceDate.Dock = DockStyle.Top;
            lblInvoiceDate.Font = new Font("Segoe UI", 9F);
            lblInvoiceDate.ForeColor = Color.FromArgb(102, 102, 102);
            lblInvoiceDate.Location = new Point(5, 30);
            lblInvoiceDate.Name = "lblInvoiceDate";
            lblInvoiceDate.Size = new Size(337, 25);
            lblInvoiceDate.TabIndex = 1;
            lblInvoiceDate.Text = "Date: Nov 13, 2024";
            lblInvoiceDate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblInvoiceNumber
            // 
            lblInvoiceNumber.Dock = DockStyle.Top;
            lblInvoiceNumber.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInvoiceNumber.ForeColor = Color.FromArgb(51, 51, 51);
            lblInvoiceNumber.Location = new Point(5, 5);
            lblInvoiceNumber.Name = "lblInvoiceNumber";
            lblInvoiceNumber.Size = new Size(337, 25);
            lblInvoiceNumber.TabIndex = 0;
            lblInvoiceNumber.Text = "Invoice #: INV-001";
            lblInvoiceNumber.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelItems
            // 
            panelItems.BackColor = Color.Transparent;
            panelItems.BorderStyle = BorderStyle.FixedSingle;
            panelItems.Controls.Add(dgvItems);
            panelItems.Controls.Add(lblItemsTitle);
            panelItems.Dock = DockStyle.Fill;
            panelItems.Location = new Point(23, 338);
            panelItems.Name = "panelItems";
            panelItems.Padding = new Padding(10);
            panelItems.Size = new Size(1185, 394);
            panelItems.TabIndex = 2;
            // 
            // dgvItems
            // 
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItems.BackgroundColor = Color.White;
            dgvItems.BorderStyle = BorderStyle.None;
            dgvItems.ColumnHeadersHeight = 40;
            dgvItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvItems.Columns.AddRange(new DataGridViewColumn[] { colDescription, colQuantity, colUnitPrice, colTotal });
            dgvItems.Dock = DockStyle.Fill;
            dgvItems.Font = new Font("Segoe UI", 11F);
            dgvItems.GridColor = Color.FromArgb(230, 230, 230);
            dgvItems.Location = new Point(10, 61);
            dgvItems.Margin = new Padding(5);
            dgvItems.Name = "dgvItems";
            dgvItems.ReadOnly = true;
            dgvItems.RowHeadersVisible = false;
            dgvItems.RowHeadersWidth = 51;
            dgvItems.RowTemplate.Height = 35;
            dgvItems.Size = new Size(1163, 321);
            dgvItems.TabIndex = 1;
            // 
            // colDescription
            // 
            colDescription.FillWeight = 50F;
            colDescription.HeaderText = "Description";
            colDescription.MinimumWidth = 6;
            colDescription.Name = "colDescription";
            colDescription.ReadOnly = true;
            // 
            // colQuantity
            // 
            colQuantity.FillWeight = 15F;
            colQuantity.HeaderText = "Qty";
            colQuantity.MinimumWidth = 6;
            colQuantity.Name = "colQuantity";
            colQuantity.ReadOnly = true;
            // 
            // colUnitPrice
            // 
            colUnitPrice.FillWeight = 20F;
            colUnitPrice.HeaderText = "Unit Price";
            colUnitPrice.MinimumWidth = 6;
            colUnitPrice.Name = "colUnitPrice";
            colUnitPrice.ReadOnly = true;
            // 
            // colTotal
            // 
            colTotal.FillWeight = 25F;
            colTotal.HeaderText = "Total";
            colTotal.MinimumWidth = 6;
            colTotal.Name = "colTotal";
            colTotal.ReadOnly = true;
            // 
            // lblItemsTitle
            // 
            lblItemsTitle.BackColor = Color.FromArgb(248, 248, 248);
            lblItemsTitle.Dock = DockStyle.Top;
            lblItemsTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblItemsTitle.ForeColor = Color.FromArgb(51, 51, 51);
            lblItemsTitle.Location = new Point(10, 10);
            lblItemsTitle.Name = "lblItemsTitle";
            lblItemsTitle.Padding = new Padding(15, 8, 5, 5);
            lblItemsTitle.Size = new Size(1163, 51);
            lblItemsTitle.TabIndex = 0;
            lblItemsTitle.Text = "ITEMS";
            lblItemsTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelTotals
            // 
            panelTotals.BackColor = Color.Transparent;
            panelTotals.Controls.Add(tableLayoutPanelTotals);
            panelTotals.Dock = DockStyle.Fill;
            panelTotals.Location = new Point(23, 738);
            panelTotals.Name = "panelTotals";
            panelTotals.Padding = new Padding(5);
            panelTotals.Size = new Size(1185, 207);
            panelTotals.TabIndex = 3;
            // 
            // tableLayoutPanelTotals
            // 
            tableLayoutPanelTotals.BackColor = Color.Transparent;
            tableLayoutPanelTotals.ColumnCount = 2;
            tableLayoutPanelTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelTotals.Controls.Add(panelTotalsData, 1, 0);
            tableLayoutPanelTotals.Dock = DockStyle.Fill;
            tableLayoutPanelTotals.Location = new Point(5, 5);
            tableLayoutPanelTotals.Name = "tableLayoutPanelTotals";
            tableLayoutPanelTotals.RowCount = 1;
            tableLayoutPanelTotals.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelTotals.Size = new Size(1175, 197);
            tableLayoutPanelTotals.TabIndex = 0;
            // 
            // panelTotalsData
            // 
            panelTotalsData.BackColor = Color.FromArgb(248, 248, 248);
            panelTotalsData.BorderStyle = BorderStyle.FixedSingle;
            panelTotalsData.Controls.Add(lblAmountPaid);
            panelTotalsData.Controls.Add(lblTotal);
            panelTotalsData.Controls.Add(lblTax);
            panelTotalsData.Controls.Add(lblSubtotal);
            panelTotalsData.Dock = DockStyle.Fill;
            panelTotalsData.Location = new Point(590, 3);
            panelTotalsData.Name = "panelTotalsData";
            panelTotalsData.Padding = new Padding(20);
            panelTotalsData.Size = new Size(582, 191);
            panelTotalsData.TabIndex = 0;
            // 
            // lblAmountPaid
            // 
            lblAmountPaid.Dock = DockStyle.Top;
            lblAmountPaid.Font = new Font("Segoe UI", 12F);
            lblAmountPaid.ForeColor = Color.FromArgb(46, 204, 113);
            lblAmountPaid.Location = new Point(20, 127);
            lblAmountPaid.Name = "lblAmountPaid";
            lblAmountPaid.Size = new Size(540, 35);
            lblAmountPaid.TabIndex = 3;
            lblAmountPaid.Text = "Amount Paid: $0.00";
            lblAmountPaid.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotal
            // 
            lblTotal.BackColor = Color.White;
            lblTotal.BorderStyle = BorderStyle.FixedSingle;
            lblTotal.Dock = DockStyle.Top;
            lblTotal.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTotal.ForeColor = Color.FromArgb(41, 128, 185);
            lblTotal.Location = new Point(20, 90);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(540, 37);
            lblTotal.TabIndex = 2;
            lblTotal.Text = "TOTAL: $0.00";
            lblTotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTax
            // 
            lblTax.Dock = DockStyle.Top;
            lblTax.Font = new Font("Segoe UI", 12F);
            lblTax.ForeColor = Color.FromArgb(51, 51, 51);
            lblTax.Location = new Point(20, 55);
            lblTax.Name = "lblTax";
            lblTax.Size = new Size(540, 35);
            lblTax.TabIndex = 1;
            lblTax.Text = "Tax: $0.00";
            lblTax.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblSubtotal
            // 
            lblSubtotal.Dock = DockStyle.Top;
            lblSubtotal.Font = new Font("Segoe UI", 12F);
            lblSubtotal.ForeColor = Color.FromArgb(51, 51, 51);
            lblSubtotal.Location = new Point(20, 20);
            lblSubtotal.Name = "lblSubtotal";
            lblSubtotal.Size = new Size(540, 35);
            lblSubtotal.TabIndex = 0;
            lblSubtotal.Text = "Subtotal: $0.00";
            lblSubtotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // panelFooter
            // 
            panelFooter.BackColor = Color.FromArgb(248, 248, 248);
            panelFooter.BorderStyle = BorderStyle.FixedSingle;
            panelFooter.Controls.Add(lblFooterText);
            panelFooter.Dock = DockStyle.Fill;
            panelFooter.Location = new Point(23, 951);
            panelFooter.Name = "panelFooter";
            panelFooter.Padding = new Padding(20);
            panelFooter.Size = new Size(1185, 62);
            panelFooter.TabIndex = 4;
            // 
            // lblFooterText
            // 
            lblFooterText.Dock = DockStyle.Fill;
            lblFooterText.Font = new Font("Segoe UI", 11F);
            lblFooterText.ForeColor = Color.FromArgb(51, 51, 51);
            lblFooterText.Location = new Point(20, 20);
            lblFooterText.Name = "lblFooterText";
            lblFooterText.Size = new Size(1143, 20);
            lblFooterText.TabIndex = 0;
            lblFooterText.Text = "💼 Thank you for your business!\r\n\r\n⏱️ Payment is due within 30 days.\r\n📞 For questions about this invoice, please contact us at:\r\n📧 info@inventorypro.com";
            lblFooterText.TextAlign = ContentAlignment.TopCenter;
            // 
            // panelActions
            // 
            panelActions.BackColor = Color.Transparent;
            panelActions.Controls.Add(btnClose);
            panelActions.Controls.Add(btnEmail);
            panelActions.Controls.Add(btnSave);
            panelActions.Controls.Add(btnPrint);
            panelActions.Dock = DockStyle.Fill;
            panelActions.Location = new Point(23, 1019);
            panelActions.Name = "panelActions";
            panelActions.Padding = new Padding(10);
            panelActions.Size = new Size(1185, 66);
            panelActions.TabIndex = 5;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.None;
            btnClose.BackColor = Color.FromArgb(149, 165, 166);
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 10F);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(727, 18);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(80, 30);
            btnClose.TabIndex = 3;
            btnClose.Text = "❌ Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += BtnClose_Click;
            // 
            // btnEmail
            // 
            btnEmail.Anchor = AnchorStyles.None;
            btnEmail.BackColor = Color.FromArgb(155, 89, 182);
            btnEmail.FlatAppearance.BorderSize = 0;
            btnEmail.FlatStyle = FlatStyle.Flat;
            btnEmail.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnEmail.ForeColor = Color.White;
            btnEmail.Location = new Point(627, 18);
            btnEmail.Name = "btnEmail";
            btnEmail.Size = new Size(100, 30);
            btnEmail.TabIndex = 2;
            btnEmail.Text = "📧 Email";
            btnEmail.UseVisualStyleBackColor = false;
            btnEmail.Click += BtnEmail_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.None;
            btnSave.BackColor = Color.FromArgb(46, 204, 113);
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(477, 18);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 30);
            btnSave.TabIndex = 1;
            btnSave.Text = "💾 Save PDF";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += BtnSave_Click;
            // 
            // btnPrint
            // 
            btnPrint.Anchor = AnchorStyles.None;
            btnPrint.BackColor = Color.FromArgb(52, 152, 219);
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPrint.ForeColor = Color.White;
            btnPrint.Location = new Point(377, 18);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(100, 30);
            btnPrint.TabIndex = 0;
            btnPrint.Text = "🖨️ Print";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += BtnPrint_Click;
            // 
            // InvoiceForm
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = Color.White;
            ClientSize = new Size(1231, 1108);
            Controls.Add(tableLayoutPanelMain);
            Font = new Font("Segoe UI", 10F);
            MaximumSize = new Size(1400, 1200);
            MinimumSize = new Size(1100, 900);
            Name = "InvoiceForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Invoice";
            tableLayoutPanelMain.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            panelInfo.ResumeLayout(false);
            tableLayoutPanelInfo.ResumeLayout(false);
            panelCompany.ResumeLayout(false);
            panelCustomer.ResumeLayout(false);
            panelInvoiceDetails.ResumeLayout(false);
            panelItems.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvItems).EndInit();
            panelTotals.ResumeLayout(false);
            tableLayoutPanelTotals.ResumeLayout(false);
            panelTotalsData.ResumeLayout(false);
            panelFooter.ResumeLayout(false);
            panelActions.ResumeLayout(false);
            ResumeLayout(false);

            }

        #endregion

        private TableLayoutPanel tableLayoutPanelMain;
        private Panel panelHeader;
        private Label lblInvoiceTitle;
        private Panel panelInfo;
        private TableLayoutPanel tableLayoutPanelInfo;
        private Panel panelCompany;
        private Label lblCompanyContact;
        private Label lblCompanyAddress;
        private Label lblCompanyName;
        private Panel panelCustomer;
        private Label lblCustomerAddress;
        private Label lblCustomerName;
        private Label lblBillTo;
        private Panel panelInvoiceDetails;
        private Label lblDueDate;
        private Label lblInvoiceDate;
        private Label lblInvoiceNumber;
        private Panel panelItems;
        private DataGridView dgvItems;
        private DataGridViewTextBoxColumn colDescription;
        private DataGridViewTextBoxColumn colQuantity;
        private DataGridViewTextBoxColumn colUnitPrice;
        private DataGridViewTextBoxColumn colTotal;
        private Label lblItemsTitle;
        private Panel panelTotals;
        private TableLayoutPanel tableLayoutPanelTotals;
        private Panel panelTotalsData;
        private Label lblAmountPaid;
        private Label lblTotal;
        private Label lblTax;
        private Label lblSubtotal;
        private Panel panelFooter;
        private Label lblFooterText;
        private Panel panelActions;
        private Button btnClose;
        private Button btnEmail;
        private Button btnSave;
        private Button btnPrint;
    }
}