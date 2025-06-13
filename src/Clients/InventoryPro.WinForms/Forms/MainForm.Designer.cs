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
            // Main form components - REDESIGNED TO ELIMINATE DUPLICATION
            this.menuStrip1 = new MenuStrip();
            this.menuFile = new ToolStripMenuItem();
            this.menuNew = new ToolStripMenuItem();
            this.menuImport = new ToolStripMenuItem();
            this.menuExport = new ToolStripMenuItem();
            this.menuSeparator1 = new ToolStripSeparator();
            this.menuExit = new ToolStripMenuItem();
            this.menuView = new ToolStripMenuItem();
            this.menuDashboard = new ToolStripMenuItem();
            this.menuFullScreen = new ToolStripMenuItem();
            this.menuReports = new ToolStripMenuItem();
            this.menuSalesReports = new ToolStripMenuItem();
            this.menuInventoryReports = new ToolStripMenuItem();
            this.menuFinancialReports = new ToolStripMenuItem();
            this.menuCustomReports = new ToolStripMenuItem();
            this.menuTools = new ToolStripMenuItem();
            this.menuSettings = new ToolStripMenuItem();
            this.menuBackup = new ToolStripMenuItem();
            this.menuWindow = new ToolStripMenuItem();
            this.menuMinimize = new ToolStripMenuItem();
            this.menuHelp = new ToolStripMenuItem();
            this.menuAbout = new ToolStripMenuItem();

            // Toolbar - UNIQUE QUICK ACTIONS ONLY
            this.toolStrip1 = new ToolStrip();
            this.btnProducts = new ToolStripButton();
            this.btnCustomers = new ToolStripButton();
            this.btnSales = new ToolStripButton();
            this.btnReports = new ToolStripButton();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.btnNewSale = new ToolStripButton();
            this.btnAddProduct = new ToolStripButton();
            this.toolStripSeparator2 = new ToolStripSeparator();
            this.btnRefresh = new ToolStripButton();
            this.btnLogout = new ToolStripButton();

            // Status bar
            this.statusStrip1 = new StatusStrip();
            this.lblStatus = new ToolStripStatusLabel();
            this.lblCurrentUser = new ToolStripStatusLabel();
            this.lblUserRole = new ToolStripStatusLabel();
            this.lblLastLogin = new ToolStripStatusLabel();

            // Dashboard panels
            this.pnlDashboard = new Panel();
            this.pnlStats = new Panel();
            this.pnlActivities = new Panel();
            this.pnlAlerts = new Panel();

            // Statistics labels
            this.lblTotalProducts = new Label();
            this.lblLowStockProducts = new Label();
            this.lblOutOfStockProducts = new Label();
            this.lblInventoryValue = new Label();
            this.lblTodaySales = new Label();
            this.lblMonthSales = new Label();
            this.lblYearSales = new Label();
            this.lblTodayOrders = new Label();
            this.lblTotalCustomers = new Label();
            this.lblNewCustomers = new Label();

            // Activity and alert lists
            this.lstRecentActivities = new ListBox();
            this.lstLowStockAlerts = new ListView();

            // Context menus
            this.dashboardContextMenu = new ContextMenuStrip();
            this.statsContextMenu = new ContextMenuStrip();
            this.alertsContextMenu = new ContextMenuStrip();
            this.activitiesContextMenu = new ContextMenuStrip();

            this.SuspendLayout();

            #region Menu Strip Configuration - SYSTEM LEVEL OPERATIONS
            // menuStrip1 - Traditional Windows application menu
            this.menuStrip1.Items.AddRange(new ToolStripItem[] {
                this.menuFile,
                this.menuView,
                this.menuReports,
                this.menuTools,
                this.menuWindow,
                this.menuHelp});
            this.menuStrip1.Location = new Point(0, 0);
            this.menuStrip1.Size = new Size(1200, 24);

            // File Menu
            this.menuFile.Text = "&File";
            this.menuFile.DropDownItems.AddRange(new ToolStripItem[] {
                this.menuNew,
                this.menuImport,
                this.menuExport,
                this.menuSeparator1,
                this.menuExit});

            this.menuNew.Text = "&New...";
            this.menuNew.ShortcutKeys = Keys.Control | Keys.N;
            this.menuNew.Click += this.MenuNew_Click;

            this.menuImport.Text = "&Import Data...";
            this.menuImport.Click += this.MenuImport_Click;

            this.menuExport.Text = "&Export Data...";
            this.menuExport.Click += this.MenuExport_Click;

            this.menuExit.Text = "E&xit";
            this.menuExit.ShortcutKeys = Keys.Alt | Keys.F4;
            this.menuExit.Click += this.MenuExit_Click;

            // View Menu
            this.menuView.Text = "&View";
            this.menuView.DropDownItems.AddRange(new ToolStripItem[] {
                this.menuDashboard,
                this.menuFullScreen});

            this.menuDashboard.Text = "&Dashboard";
            this.menuDashboard.ShortcutKeys = Keys.F1;
            this.menuDashboard.Click += this.MenuDashboard_Click;

            this.menuFullScreen.Text = "&Full Screen";
            this.menuFullScreen.ShortcutKeys = Keys.F11;
            this.menuFullScreen.Click += this.MenuFullScreen_Click;

            // Reports Menu
            this.menuReports.Text = "&Reports";
            this.menuReports.DropDownItems.AddRange(new ToolStripItem[] {
                this.menuSalesReports,
                this.menuInventoryReports,
                this.menuFinancialReports,
                this.menuCustomReports});

            this.menuSalesReports.Text = "&Sales Reports";
            this.menuSalesReports.Click += this.MenuSalesReports_Click;

            this.menuInventoryReports.Text = "&Inventory Reports";
            this.menuInventoryReports.Click += this.MenuInventoryReports_Click;

            this.menuFinancialReports.Text = "&Financial Reports";
            this.menuFinancialReports.Click += this.MenuFinancialReports_Click;

            this.menuCustomReports.Text = "&Custom Reports";
            this.menuCustomReports.Click += this.MenuCustomReports_Click;

            // Tools Menu
            this.menuTools.Text = "&Tools";
            this.menuTools.DropDownItems.AddRange(new ToolStripItem[] {
                this.menuSettings,
                this.menuBackup});

            this.menuSettings.Text = "&Settings...";
            this.menuSettings.Click += this.MenuSettings_Click;

            this.menuBackup.Text = "&Backup Database...";
            this.menuBackup.Click += this.MenuBackup_Click;

            // Window Menu
            this.menuWindow.Text = "&Window";
            this.menuWindow.DropDownItems.AddRange(new ToolStripItem[] {
                this.menuMinimize});

            this.menuMinimize.Text = "&Minimize";
            this.menuMinimize.Click += this.MenuMinimize_Click;

            // Help Menu
            this.menuHelp.Text = "&Help";
            this.menuHelp.DropDownItems.AddRange(new ToolStripItem[] {
                this.menuAbout});

            this.menuAbout.Text = "&About InventoryPro...";
            this.menuAbout.Click += this.MenuAbout_Click;
            #endregion

            #region Toolbar Configuration - QUICK ACTIONS ONLY
            // toolStrip1 - Quick access to most-used operations
            this.toolStrip1.Items.AddRange(new ToolStripItem[] {
                this.btnProducts,
                this.btnCustomers,
                this.btnSales,
                this.btnReports,
                this.toolStripSeparator1,
                this.btnNewSale,
                this.btnAddProduct,
                this.toolStripSeparator2,
                this.btnRefresh,
                this.btnLogout});
            this.toolStrip1.Location = new Point(0, 24);
            this.toolStrip1.Size = new Size(1200, 25);
            this.toolStrip1.ImageScalingSize = new Size(16, 16);

            // Navigation buttons - Core modules
            this.btnProducts.Text = "📦 Products";
            this.btnProducts.ToolTipText = "Manage product inventory";
            this.btnProducts.Click += this.BtnProducts_Click;

            this.btnCustomers.Text = "👥 Customers";
            this.btnCustomers.ToolTipText = "Manage customer database";
            this.btnCustomers.Click += this.BtnCustomers_Click;

            this.btnSales.Text = "💰 Sales";
            this.btnSales.ToolTipText = "View and manage sales";
            this.btnSales.Click += this.BtnSales_Click;

            this.btnReports.Text = "📊 Reports";
            this.btnReports.ToolTipText = "Generate business reports";
            this.btnReports.Click += this.BtnReports_Click;

            // Quick action buttons - Enhanced productivity
            this.btnNewSale.Text = "🛒 New Sale";
            this.btnNewSale.ToolTipText = "Start a new sale transaction";
            this.btnNewSale.Click += this.BtnNewSale_Click;

            this.btnAddProduct.Text = "➕ Add Product";
            this.btnAddProduct.ToolTipText = "Quickly add a new product";
            this.btnAddProduct.Click += this.BtnAddProduct_Click;

            // System actions
            this.btnRefresh.Text = "🔄 Refresh";
            this.btnRefresh.ToolTipText = "Refresh dashboard data (F5)";
            this.btnRefresh.Click += this.BtnRefresh_Click;

            this.btnLogout.Text = "🚪 Logout";
            this.btnLogout.ToolTipText = "Logout and return to login screen";
            this.btnLogout.Click += this.BtnLogout_Click;
            #endregion

            #region Status Bar Configuration
            // statusStrip1
            this.statusStrip1.Items.AddRange(new ToolStripItem[] {
                this.lblStatus,
                this.lblCurrentUser,
                this.lblUserRole,
                this.lblLastLogin});
            this.statusStrip1.Location = new Point(0, 700);
            this.statusStrip1.Size = new Size(1200, 22);

            this.lblStatus.Text = "Ready";
            this.lblCurrentUser.Text = "Not logged in";
            this.lblUserRole.Text = "";
            this.lblLastLogin.Text = "";
            #endregion

            #region Dashboard Panels
            // Main dashboard panel
            this.pnlDashboard.Dock = DockStyle.Fill;
            this.pnlDashboard.Location = new Point(0, 49);
            this.pnlDashboard.Size = new Size(1200, 651);

            // Statistics panel
            this.pnlStats.Location = new Point(10, 10);
            this.pnlStats.Size = new Size(1180, 200);
            this.pnlStats.BorderStyle = BorderStyle.FixedSingle;

            // Activities panel
            this.pnlActivities.Location = new Point(10, 220);
            this.pnlActivities.Size = new Size(580, 400);
            this.pnlActivities.BorderStyle = BorderStyle.FixedSingle;

            // Alerts panel
            this.pnlAlerts.Location = new Point(600, 220);
            this.pnlAlerts.Size = new Size(590, 400);
            this.pnlAlerts.BorderStyle = BorderStyle.FixedSingle;

            // Add panels to dashboard
            this.pnlDashboard.Controls.Add(this.pnlStats);
            this.pnlDashboard.Controls.Add(this.pnlActivities);
            this.pnlDashboard.Controls.Add(this.pnlAlerts);
            #endregion

            #region Statistics Labels
            // Configure statistics labels (simplified for brevity)
            this.lblTotalProducts.Location = new Point(20, 20);
            this.lblTotalProducts.Size = new Size(100, 30);
            this.lblTotalProducts.Text = "Total Products: 0";

            this.lblLowStockProducts.Location = new Point(140, 20);
            this.lblLowStockProducts.Size = new Size(100, 30);
            this.lblLowStockProducts.Text = "Low Stock: 0";

            // Add more statistics labels...
            this.pnlStats.Controls.Add(this.lblTotalProducts);
            this.pnlStats.Controls.Add(this.lblLowStockProducts);
            // Add other labels...
            #endregion

            #region Activity and Alert Lists
            // Recent activities list
            this.lstRecentActivities.Location = new Point(10, 30);
            this.lstRecentActivities.Size = new Size(560, 360);
            this.pnlActivities.Controls.Add(this.lstRecentActivities);

            // Low stock alerts list
            this.lstLowStockAlerts.Location = new Point(10, 30);
            this.lstLowStockAlerts.Size = new Size(570, 360);
            this.lstLowStockAlerts.View = View.Details;
            this.lstLowStockAlerts.FullRowSelect = true;
            this.lstLowStockAlerts.GridLines = true;
            this.lstLowStockAlerts.Columns.Add("Product", 200);
            this.lstLowStockAlerts.Columns.Add("SKU", 100);
            this.lstLowStockAlerts.Columns.Add("Stock", 80);
            this.lstLowStockAlerts.Columns.Add("Min Stock", 80);
            this.lstLowStockAlerts.DoubleClick += this.LstLowStockAlerts_DoubleClick;
            this.pnlAlerts.Controls.Add(this.lstLowStockAlerts);
            #endregion

            #region Context Menu Configuration
            this.InitializeContextMenus();
            #endregion

            #region Form Configuration
            // MainForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 722);
            this.Controls.Add(this.pnlDashboard);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "InventoryPro - Dashboard";
            this.WindowState = FormWindowState.Maximized;
            this.FormClosing += this.MainForm_FormClosing;
            #endregion

            this.ResumeLayout(false);
            this.PerformLayout();
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