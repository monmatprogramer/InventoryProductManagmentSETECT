namespace InventoryPro.WinForms.Forms
    {
    partial class MainForm
        {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
            {
            components = new System.ComponentModel.Container();
            menuStrip1 = new MenuStrip();
            menuFile = new ToolStripMenuItem();
            menuNew = new ToolStripMenuItem();
            menuImport = new ToolStripMenuItem();
            menuExport = new ToolStripMenuItem();
            menuSeparator1 = new ToolStripSeparator();
            menuExit = new ToolStripMenuItem();
            menuView = new ToolStripMenuItem();
            menuDashboard = new ToolStripMenuItem();
            menuFullScreen = new ToolStripMenuItem();
            menuReports = new ToolStripMenuItem();
            menuSalesReports = new ToolStripMenuItem();
            menuInventoryReports = new ToolStripMenuItem();
            menuFinancialReports = new ToolStripMenuItem();
            menuCustomReports = new ToolStripMenuItem();
            menuTools = new ToolStripMenuItem();
            menuSettings = new ToolStripMenuItem();
            menuBackup = new ToolStripMenuItem();
            menuWindow = new ToolStripMenuItem();
            menuMinimize = new ToolStripMenuItem();
            menuHelp = new ToolStripMenuItem();
            menuAbout = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            btnProducts = new ToolStripButton();
            btnCustomers = new ToolStripButton();
            btnSales = new ToolStripButton();
            btnReports = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnNewSale = new ToolStripButton();
            btnAddProduct = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnRefresh = new ToolStripButton();
            btnLogout = new ToolStripButton();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            lblCurrentUser = new ToolStripStatusLabel();
            lblUserRole = new ToolStripStatusLabel();
            lblLastLogin = new ToolStripStatusLabel();
            pnlDashboard = new Panel();
            pnlStats = new Panel();
            lblTotalProducts = new Label();
            lblLowStockProducts = new Label();
            lblOutOfStockProducts = new Label();
            lblInventoryValue = new Label();
            lblTodaySales = new Label();
            lblTodayOrders = new Label();
            lblTotalCustomers = new Label();
            lblMonthSales = new Label();
            lblYearSales = new Label();
            lblNewCustomers = new Label();
            pnlActivities = new Panel();
            lstRecentActivities = new ListBox();
            pnlAlerts = new Panel();
            lstLowStockAlerts = new ListView();
            dashboardContextMenu = new ContextMenuStrip(components);
            statsContextMenu = new ContextMenuStrip(components);
            alertsContextMenu = new ContextMenuStrip(components);
            activitiesContextMenu = new ContextMenuStrip(components);
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            pnlDashboard.SuspendLayout();
            pnlStats.SuspendLayout();
            pnlActivities.SuspendLayout();
            pnlAlerts.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { menuFile, menuView, menuReports, menuTools, menuWindow, menuHelp });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 3, 0, 3);
            menuStrip1.Size = new Size(1371, 30);
            menuStrip1.TabIndex = 7;
            // 
            // menuFile
            // 
            menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuNew, menuImport, menuExport, menuSeparator1, menuExit });
            menuFile.Name = "menuFile";
            menuFile.Size = new Size(46, 24);
            menuFile.Text = "&File";
            // 
            // menuNew
            // 
            menuNew.Name = "menuNew";
            menuNew.ShortcutKeys = Keys.Control | Keys.N;
            menuNew.Size = new Size(184, 26);
            menuNew.Text = "&New...";
            menuNew.Click += MenuNew_Click;
            // 
            // menuImport
            // 
            menuImport.Name = "menuImport";
            menuImport.Size = new Size(184, 26);
            menuImport.Text = "&Import Data...";
            menuImport.Click += MenuImport_Click;
            // 
            // menuExport
            // 
            menuExport.Name = "menuExport";
            menuExport.Size = new Size(184, 26);
            menuExport.Text = "&Export Data...";
            menuExport.Click += MenuExport_Click;
            // 
            // menuSeparator1
            // 
            menuSeparator1.Name = "menuSeparator1";
            menuSeparator1.Size = new Size(181, 6);
            // 
            // menuExit
            // 
            menuExit.Name = "menuExit";
            menuExit.ShortcutKeys = Keys.Alt | Keys.F4;
            menuExit.Size = new Size(184, 26);
            menuExit.Text = "E&xit";
            menuExit.Click += MenuExit_Click;
            // 
            // menuView
            // 
            menuView.DropDownItems.AddRange(new ToolStripItem[] { menuDashboard, menuFullScreen });
            menuView.Name = "menuView";
            menuView.Size = new Size(55, 24);
            menuView.Text = "&View";
            // 
            // menuDashboard
            // 
            menuDashboard.Name = "menuDashboard";
            menuDashboard.ShortcutKeys = Keys.F1;
            menuDashboard.Size = new Size(195, 26);
            menuDashboard.Text = "&Dashboard";
            menuDashboard.Click += MenuDashboard_Click;
            // 
            // menuFullScreen
            // 
            menuFullScreen.Name = "menuFullScreen";
            menuFullScreen.ShortcutKeys = Keys.F11;
            menuFullScreen.Size = new Size(195, 26);
            menuFullScreen.Text = "&Full Screen";
            menuFullScreen.Click += MenuFullScreen_Click;
            // 
            // menuReports
            // 
            menuReports.DropDownItems.AddRange(new ToolStripItem[] { menuSalesReports, menuInventoryReports, menuFinancialReports, menuCustomReports });
            menuReports.Name = "menuReports";
            menuReports.Size = new Size(74, 24);
            menuReports.Text = "&Reports";
            // 
            // menuSalesReports
            // 
            menuSalesReports.Name = "menuSalesReports";
            menuSalesReports.Size = new Size(208, 26);
            menuSalesReports.Text = "&Sales Reports";
            menuSalesReports.Click += MenuSalesReports_Click;
            // 
            // menuInventoryReports
            // 
            menuInventoryReports.Name = "menuInventoryReports";
            menuInventoryReports.Size = new Size(208, 26);
            menuInventoryReports.Text = "&Inventory Reports";
            menuInventoryReports.Click += MenuInventoryReports_Click;
            // 
            // menuFinancialReports
            // 
            menuFinancialReports.Name = "menuFinancialReports";
            menuFinancialReports.Size = new Size(208, 26);
            menuFinancialReports.Text = "&Financial Reports";
            menuFinancialReports.Click += MenuFinancialReports_Click;
            // 
            // menuCustomReports
            // 
            menuCustomReports.Name = "menuCustomReports";
            menuCustomReports.Size = new Size(208, 26);
            menuCustomReports.Text = "&Custom Reports";
            menuCustomReports.Click += MenuCustomReports_Click;
            // 
            // menuTools
            // 
            menuTools.DropDownItems.AddRange(new ToolStripItem[] { menuSettings, menuBackup });
            menuTools.Name = "menuTools";
            menuTools.Size = new Size(58, 24);
            menuTools.Text = "&Tools";
            // 
            // menuSettings
            // 
            menuSettings.Name = "menuSettings";
            menuSettings.Size = new Size(216, 26);
            menuSettings.Text = "&Settings...";
            menuSettings.Click += MenuSettings_Click;
            // 
            // menuBackup
            // 
            menuBackup.Name = "menuBackup";
            menuBackup.Size = new Size(216, 26);
            menuBackup.Text = "&Backup Database...";
            menuBackup.Click += MenuBackup_Click;
            // 
            // menuWindow
            // 
            menuWindow.DropDownItems.AddRange(new ToolStripItem[] { menuMinimize });
            menuWindow.Name = "menuWindow";
            menuWindow.Size = new Size(78, 24);
            menuWindow.Text = "&Window";
            // 
            // menuMinimize
            // 
            menuMinimize.Name = "menuMinimize";
            menuMinimize.Size = new Size(153, 26);
            menuMinimize.Text = "&Minimize";
            menuMinimize.Click += MenuMinimize_Click;
            // 
            // menuHelp
            // 
            menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuAbout });
            menuHelp.Name = "menuHelp";
            menuHelp.Size = new Size(55, 24);
            menuHelp.Text = "&Help";
            // 
            // menuAbout
            // 
            menuAbout.Name = "menuAbout";
            menuAbout.Size = new Size(229, 26);
            menuAbout.Text = "&About InventoryPro...";
            menuAbout.Click += MenuAbout_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnProducts, btnCustomers, btnSales, btnReports, toolStripSeparator1, btnNewSale, btnAddProduct, toolStripSeparator2, btnRefresh, btnLogout });
            toolStrip1.Location = new Point(0, 30);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1371, 27);
            toolStrip1.TabIndex = 5;
            // 
            // btnProducts
            // 
            btnProducts.Name = "btnProducts";
            btnProducts.Size = new Size(95, 24);
            btnProducts.Text = "📦 Products";
            btnProducts.ToolTipText = "Manage product inventory";
            btnProducts.Click += BtnProducts_Click;
            // 
            // btnCustomers
            // 
            btnCustomers.Name = "btnCustomers";
            btnCustomers.Size = new Size(107, 24);
            btnCustomers.Text = "👥 Customers";
            btnCustomers.ToolTipText = "Manage customer database";
            btnCustomers.Click += BtnCustomers_Click;
            // 
            // btnSales
            // 
            btnSales.Name = "btnSales";
            btnSales.Size = new Size(72, 24);
            btnSales.Text = "💰 Sales";
            btnSales.ToolTipText = "View and manage sales";
            btnSales.Click += BtnSales_Click;
            // 
            // btnReports
            // 
            btnReports.Name = "btnReports";
            btnReports.Size = new Size(89, 24);
            btnReports.Text = "📊 Reports";
            btnReports.ToolTipText = "Generate business reports";
            btnReports.Click += BtnReports_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 27);
            // 
            // btnNewSale
            // 
            btnNewSale.Name = "btnNewSale";
            btnNewSale.Size = new Size(100, 24);
            btnNewSale.Text = "\U0001f6d2 New Sale";
            btnNewSale.ToolTipText = "Start a new sale transaction";
            btnNewSale.Click += BtnNewSale_Click;
            // 
            // btnAddProduct
            // 
            btnAddProduct.Name = "btnAddProduct";
            btnAddProduct.Size = new Size(121, 24);
            btnAddProduct.Text = "➕ Add Product";
            btnAddProduct.ToolTipText = "Quickly add a new product";
            btnAddProduct.Click += BtnAddProduct_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 27);
            // 
            // btnRefresh
            // 
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(87, 24);
            btnRefresh.Text = "🔄 Refresh";
            btnRefresh.ToolTipText = "Refresh dashboard data (F5)";
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // btnLogout
            // 
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(85, 24);
            btnLogout.Text = "🚪 Logout";
            btnLogout.ToolTipText = "Logout and return to login screen";
            btnLogout.Click += BtnLogout_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus, lblCurrentUser, lblUserRole, lblLastLogin });
            statusStrip1.Location = new Point(0, 937);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 16, 0);
            statusStrip1.Size = new Size(1371, 26);
            statusStrip1.TabIndex = 6;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 20);
            lblStatus.Text = "Ready";
            // 
            // lblCurrentUser
            // 
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new Size(102, 20);
            lblCurrentUser.Text = "Not logged in";
            // 
            // lblUserRole
            // 
            lblUserRole.Name = "lblUserRole";
            lblUserRole.Size = new Size(0, 20);
            // 
            // lblLastLogin
            // 
            lblLastLogin.Name = "lblLastLogin";
            lblLastLogin.Size = new Size(0, 20);
            // 
            // pnlDashboard
            // 
            pnlDashboard.BackColor = Color.FromArgb(248, 249, 250);
            pnlDashboard.Controls.Add(pnlStats);
            pnlDashboard.Controls.Add(pnlActivities);
            pnlDashboard.Controls.Add(pnlAlerts);
            pnlDashboard.Dock = DockStyle.Fill;
            pnlDashboard.Location = new Point(0, 57);
            pnlDashboard.Margin = new Padding(3, 4, 3, 4);
            pnlDashboard.Name = "pnlDashboard";
            pnlDashboard.Padding = new Padding(23, 27, 23, 27);
            pnlDashboard.Size = new Size(1371, 880);
            pnlDashboard.TabIndex = 4;
            // 
            // pnlStats
            // 
            pnlStats.BackColor = Color.Transparent;
            pnlStats.Controls.Add(lblTotalProducts);
            pnlStats.Controls.Add(lblLowStockProducts);
            pnlStats.Controls.Add(lblOutOfStockProducts);
            pnlStats.Controls.Add(lblInventoryValue);
            pnlStats.Controls.Add(lblTodaySales);
            pnlStats.Controls.Add(lblTodayOrders);
            pnlStats.Controls.Add(lblTotalCustomers);
            pnlStats.Controls.Add(lblMonthSales);
            pnlStats.Controls.Add(lblYearSales);
            pnlStats.Controls.Add(lblNewCustomers);
            pnlStats.Location = new Point(23, 27);
            pnlStats.Margin = new Padding(0, 0, 0, 27);
            pnlStats.Name = "pnlStats";
            pnlStats.Size = new Size(1326, 347);
            pnlStats.TabIndex = 0;
            // 
            // lblTotalProducts
            // 
            lblTotalProducts.BackColor = Color.White;
            lblTotalProducts.BorderStyle = BorderStyle.FixedSingle;
            lblTotalProducts.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalProducts.ForeColor = Color.FromArgb(75, 85, 99);
            lblTotalProducts.Location = new Point(29, 80);
            lblTotalProducts.Name = "lblTotalProducts";
            lblTotalProducts.Size = new Size(205, 106);
            lblTotalProducts.TabIndex = 0;
            lblTotalProducts.Text = "📦\nN/A\nProducts";
            lblTotalProducts.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblLowStockProducts
            // 
            lblLowStockProducts.BackColor = Color.White;
            lblLowStockProducts.BorderStyle = BorderStyle.FixedSingle;
            lblLowStockProducts.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblLowStockProducts.ForeColor = Color.FromArgb(251, 146, 60);
            lblLowStockProducts.Location = new Point(257, 80);
            lblLowStockProducts.Name = "lblLowStockProducts";
            lblLowStockProducts.Size = new Size(205, 106);
            lblLowStockProducts.TabIndex = 1;
            lblLowStockProducts.Text = "⚠️\nN/A\nLow Stock";
            lblLowStockProducts.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblOutOfStockProducts
            // 
            lblOutOfStockProducts.BackColor = Color.White;
            lblOutOfStockProducts.BorderStyle = BorderStyle.FixedSingle;
            lblOutOfStockProducts.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblOutOfStockProducts.ForeColor = Color.FromArgb(239, 68, 68);
            lblOutOfStockProducts.Location = new Point(485, 80);
            lblOutOfStockProducts.Name = "lblOutOfStockProducts";
            lblOutOfStockProducts.Size = new Size(205, 106);
            lblOutOfStockProducts.TabIndex = 2;
            lblOutOfStockProducts.Text = "❌\nN/A\nOut of Stock";
            lblOutOfStockProducts.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblInventoryValue
            // 
            lblInventoryValue.BackColor = Color.White;
            lblInventoryValue.BorderStyle = BorderStyle.FixedSingle;
            lblInventoryValue.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblInventoryValue.ForeColor = Color.FromArgb(34, 197, 94);
            lblInventoryValue.Location = new Point(713, 80);
            lblInventoryValue.Name = "lblInventoryValue";
            lblInventoryValue.Size = new Size(205, 106);
            lblInventoryValue.TabIndex = 3;
            lblInventoryValue.Text = "💰\nN/A\nTotal Value";
            lblInventoryValue.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTodaySales
            // 
            lblTodaySales.BackColor = Color.White;
            lblTodaySales.BorderStyle = BorderStyle.FixedSingle;
            lblTodaySales.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTodaySales.ForeColor = Color.FromArgb(59, 130, 246);
            lblTodaySales.Location = new Point(29, 209);
            lblTodaySales.Name = "lblTodaySales";
            lblTodaySales.Size = new Size(205, 106);
            lblTodaySales.TabIndex = 4;
            lblTodaySales.Text = "💵\nN/A\nToday's Sales";
            lblTodaySales.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTodayOrders
            // 
            lblTodayOrders.BackColor = Color.White;
            lblTodayOrders.BorderStyle = BorderStyle.FixedSingle;
            lblTodayOrders.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTodayOrders.ForeColor = Color.FromArgb(75, 85, 99);
            lblTodayOrders.Location = new Point(257, 209);
            lblTodayOrders.Name = "lblTodayOrders";
            lblTodayOrders.Size = new Size(205, 106);
            lblTodayOrders.TabIndex = 5;
            lblTodayOrders.Text = "🛒\nN/A\nToday's Orders";
            lblTodayOrders.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTotalCustomers
            // 
            lblTotalCustomers.BackColor = Color.White;
            lblTotalCustomers.BorderStyle = BorderStyle.FixedSingle;
            lblTotalCustomers.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalCustomers.ForeColor = Color.FromArgb(168, 85, 247);
            lblTotalCustomers.Location = new Point(485, 209);
            lblTotalCustomers.Name = "lblTotalCustomers";
            lblTotalCustomers.Size = new Size(205, 106);
            lblTotalCustomers.TabIndex = 6;
            lblTotalCustomers.Text = "👥\nN/A\nCustomers";
            lblTotalCustomers.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblMonthSales
            // 
            lblMonthSales.Location = new Point(0, 0);
            lblMonthSales.Name = "lblMonthSales";
            lblMonthSales.Size = new Size(114, 31);
            lblMonthSales.TabIndex = 7;
            lblMonthSales.Visible = false;
            // 
            // lblYearSales
            // 
            lblYearSales.Location = new Point(0, 0);
            lblYearSales.Name = "lblYearSales";
            lblYearSales.Size = new Size(114, 31);
            lblYearSales.TabIndex = 8;
            lblYearSales.Visible = false;
            // 
            // lblNewCustomers
            // 
            lblNewCustomers.Location = new Point(0, 0);
            lblNewCustomers.Name = "lblNewCustomers";
            lblNewCustomers.Size = new Size(114, 31);
            lblNewCustomers.TabIndex = 9;
            lblNewCustomers.Visible = false;
            // 
            // pnlActivities
            // 
            pnlActivities.BackColor = Color.Transparent;
            pnlActivities.Controls.Add(lstRecentActivities);
            pnlActivities.Location = new Point(23, 386);
            pnlActivities.Margin = new Padding(0, 0, 11, 0);
            pnlActivities.Name = "pnlActivities";
            pnlActivities.Size = new Size(646, 428);
            pnlActivities.TabIndex = 1;
            // 
            // lstRecentActivities
            // 
            lstRecentActivities.BackColor = Color.White;
            lstRecentActivities.BorderStyle = BorderStyle.None;
            lstRecentActivities.Font = new Font("Segoe UI", 10F);
            lstRecentActivities.ForeColor = Color.FromArgb(75, 85, 99);
            lstRecentActivities.Location = new Point(17, 67);
            lstRecentActivities.Margin = new Padding(3, 4, 3, 4);
            lstRecentActivities.Name = "lstRecentActivities";
            lstRecentActivities.Size = new Size(611, 368);
            lstRecentActivities.TabIndex = 0;
            // 
            // pnlAlerts
            // 
            pnlAlerts.BackColor = Color.Transparent;
            pnlAlerts.Controls.Add(lstLowStockAlerts);
            pnlAlerts.Location = new Point(691, 386);
            pnlAlerts.Margin = new Padding(11, 0, 0, 0);
            pnlAlerts.Name = "pnlAlerts";
            pnlAlerts.Size = new Size(657, 428);
            pnlAlerts.TabIndex = 2;
            // 
            // lstLowStockAlerts
            // 
            lstLowStockAlerts.BackColor = Color.White;
            lstLowStockAlerts.BorderStyle = BorderStyle.None;
            lstLowStockAlerts.Font = new Font("Segoe UI", 10F);
            lstLowStockAlerts.ForeColor = Color.FromArgb(75, 85, 99);
            lstLowStockAlerts.FullRowSelect = true;
            lstLowStockAlerts.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lstLowStockAlerts.Location = new Point(17, 67);
            lstLowStockAlerts.Margin = new Padding(3, 4, 3, 4);
            lstLowStockAlerts.Name = "lstLowStockAlerts";
            lstLowStockAlerts.Size = new Size(623, 380);
            lstLowStockAlerts.TabIndex = 0;
            lstLowStockAlerts.UseCompatibleStateImageBehavior = false;
            lstLowStockAlerts.View = View.Details;
            lstLowStockAlerts.DoubleClick += LstLowStockAlerts_DoubleClick;
            // 
            // dashboardContextMenu
            // 
            dashboardContextMenu.ImageScalingSize = new Size(20, 20);
            dashboardContextMenu.Name = "dashboardContextMenu";
            dashboardContextMenu.Size = new Size(61, 4);
            // 
            // statsContextMenu
            // 
            statsContextMenu.ImageScalingSize = new Size(20, 20);
            statsContextMenu.Name = "statsContextMenu";
            statsContextMenu.Size = new Size(61, 4);
            // 
            // alertsContextMenu
            // 
            alertsContextMenu.ImageScalingSize = new Size(20, 20);
            alertsContextMenu.Name = "alertsContextMenu";
            alertsContextMenu.Size = new Size(61, 4);
            // 
            // activitiesContextMenu
            // 
            activitiesContextMenu.ImageScalingSize = new Size(20, 20);
            activitiesContextMenu.Name = "activitiesContextMenu";
            activitiesContextMenu.Size = new Size(61, 4);
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1371, 963);
            Controls.Add(pnlDashboard);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "InventoryPro - Dashboard";
            WindowState = FormWindowState.Maximized;
            FormClosing += MainForm_FormClosing;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            pnlDashboard.ResumeLayout(false);
            pnlStats.ResumeLayout(false);
            pnlActivities.ResumeLayout(false);
            pnlAlerts.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
            }

        #endregion

        #region Control Declarations
        // MenuStrip - System level operations
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuFile;
        private ToolStripMenuItem menuNew;
        private ToolStripMenuItem menuImport;
        private ToolStripMenuItem menuExport;
        private ToolStripSeparator menuSeparator1;
        private ToolStripMenuItem menuExit;
        private ToolStripMenuItem menuView;
        private ToolStripMenuItem menuDashboard;
        private ToolStripMenuItem menuFullScreen;
        private ToolStripMenuItem menuReports;
        private ToolStripMenuItem menuSalesReports;
        private ToolStripMenuItem menuInventoryReports;
        private ToolStripMenuItem menuFinancialReports;
        private ToolStripMenuItem menuCustomReports;
        private ToolStripMenuItem menuTools;
        private ToolStripMenuItem menuSettings;
        private ToolStripMenuItem menuBackup;
        private ToolStripMenuItem menuWindow;
        private ToolStripMenuItem menuMinimize;
        private ToolStripMenuItem menuHelp;
        private ToolStripMenuItem menuAbout;

        // ToolStrip - Quick actions
        private ToolStrip toolStrip1;
        private ToolStripButton btnProducts;
        private ToolStripButton btnCustomers;
        private ToolStripButton btnSales;
        private ToolStripButton btnReports;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnNewSale;
        private ToolStripButton btnAddProduct;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnRefresh;
        private ToolStripButton btnLogout;

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblCurrentUser;
        private ToolStripStatusLabel lblUserRole;
        private ToolStripStatusLabel lblLastLogin;

        private Panel pnlDashboard;
        private Panel pnlStats;
        private Panel pnlActivities;
        private Panel pnlAlerts;

        private Label lblTotalProducts;
        private Label lblLowStockProducts;
        private Label lblOutOfStockProducts;
        private Label lblInventoryValue;
        private Label lblTodaySales;
        private Label lblMonthSales;
        private Label lblYearSales;
        private Label lblTodayOrders;
        private Label lblTotalCustomers;
        private Label lblNewCustomers;

        private ListBox lstRecentActivities;
        private ListView lstLowStockAlerts;

        // Context menus
        private ContextMenuStrip dashboardContextMenu;
        private ContextMenuStrip statsContextMenu;
        private ContextMenuStrip alertsContextMenu;
        private ContextMenuStrip activitiesContextMenu;
        #endregion
        }
    }