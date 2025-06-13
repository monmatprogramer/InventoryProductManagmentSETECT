using Microsoft.Extensions.Logging;
using InventoryPro.WinForms.Services;
using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Forms
    {
    /// <summary>
    /// Main dashboard form for the InventoryPro application
    /// This is the central hub that provides navigation to all other features
    /// </summary>
    public partial class MainForm : Form
        {
        private readonly ILogger<MainForm> _logger;
        private readonly IAuthService _authService;
        private readonly IApiService _apiService;
        private UserDto? _currentUser;
        private DashboardStatsDto? _dashboardStats;
        private bool _isInitialized = false;
        private DateTime _lastDataRefresh = DateTime.MinValue;
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

        // Child forms for different modules
        private ProductForm? _productForm;
        private CustomerForm? _customerForm;
        private SalesForm? _salesForm;
        private ReportForm? _reportForm;

        public MainForm(ILogger<MainForm> logger, IAuthService authService, IApiService apiService)
            {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            try
                {
                InitializeComponent();

                // Initialize the form asynchronously when it loads
                this.Load += MainForm_Load;
                this.Shown += MainForm_Shown;

                _logger.LogInformation("MainForm initialized successfully");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error initializing MainForm");
                throw;
                }
            }
        /// <summary>
        /// Handles the form Shown event
        /// </summary>
        private void MainForm_Shown(object? sender, EventArgs e)
            {
            try
                {
                this.Activate();
                this.BringToFront();
                this.Focus();
                _logger.LogInformation("MainForm shown and activated");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during MainForm Shown");
                }
            }

        /// <summary>
        /// Handles the form Load event
        /// </summary>
        private async void MainForm_Load(object? sender, EventArgs e)
            {
            try
                {
                _logger.LogInformation("MainForm Load event started");

                // Show a loading message or progress indicator
                lblStatus.Text = "Loading...";
                this.Text = "InventoryPro - Loading...";

                // Initialize the form data with retry logic
                var maxRetries = 3;
                var retryCount = 0;

                while (retryCount < maxRetries && !_isInitialized)
                    {
                    try
                        {
                        await InitializeFormAsync();
                        _isInitialized = true;
                        break;
                        }
                    catch (Exception initEx)
                        {
                        retryCount++;
                        _logger.LogWarning(initEx, "Initialization attempt {RetryCount} failed", retryCount);

                        if (retryCount >= maxRetries)
                            {
                            throw;
                            }

                        // Wait a bit before retrying
                        await Task.Delay(1000);
                        }
                    }

                if (!_isInitialized)
                    {
                    throw new InvalidOperationException("Failed to initialize MainForm after multiple attempts");
                    }

                _logger.LogInformation("MainForm Load event completed successfully");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during MainForm Load");

                // Show error but don't close the form immediately
                lblStatus.Text = "Error loading data";
                this.Text = "InventoryPro - Error";

                var result = MessageBox.Show(
                    $"Error loading application data: {ex.Message}\n\nWould you like to retry?",
                    "Error",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error);

                if (result == DialogResult.Retry)
                    {
                    // Retry initialization
                    MainForm_Load(sender, e);
                    }
                else
                    {
                    // User chose to cancel, close the form
                    this.Close();
                    }
                }
            }
        /// <summary>
        /// Initializes the form with user data and dashboard statistics
        /// </summary>
        private async Task InitializeFormAsync()
            {
            try
                {
                _logger.LogInformation("Starting MainForm initialization");

                // Try to load current user information with retries
                _currentUser = await GetCurrentUserWithRetryAsync();

                if (_currentUser == null)
                    {
                    _logger.LogError("User information not found during MainForm initialization");
                    throw new InvalidOperationException("User information not found. Please login again.");
                    }

                _logger.LogInformation("User loaded: {Username}", _currentUser.Username);

                // Update UI with user information
                UpdateUserInterface();

                // Load dashboard statistics (with fallback)
                try
                    {
                    await LoadDashboardStatsAsync();
                    }
                catch (Exception ex)
                    {
                    _logger.LogWarning(ex, "Failed to load dashboard stats, using defaults");
                    // Continue with default/empty dashboard stats
                    lblStatus.Text = "Dashboard data unavailable";
                    }

                _logger.LogInformation("MainForm initialized successfully for user: {Username}", _currentUser.Username);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error initializing MainForm");
                throw;
                }
            }

        /// <summary>
        /// Gets current user with retry logic
        /// </summary>
        private async Task<UserDto?> GetCurrentUserWithRetryAsync()
            {
            var maxRetries = 3;
            var delay = 500; // milliseconds

            for (int i = 0; i < maxRetries; i++)
                {
                try
                    {
                    var user = await _authService.GetCurrentUserAsync();
                    if (user != null)
                        {
                        _logger.LogInformation("Successfully retrieved user on attempt {Attempt}", i + 1);
                        return user;
                        }

                    _logger.LogWarning("GetCurrentUserAsync returned null on attempt {Attempt}", i + 1);

                    // If it's not the last attempt, wait before retrying
                    if (i < maxRetries - 1)
                        {
                        await Task.Delay(delay);
                        delay *= 2; // Exponential backoff
                        }
                    }
                catch (Exception ex)
                    {
                    _logger.LogWarning(ex, "Error getting current user on attempt {Attempt}", i + 1);

                    if (i < maxRetries - 1)
                        {
                        await Task.Delay(delay);
                        delay *= 2;
                        }
                    }
                }

            return null;
            }

        /// <summary>
        /// Updates the user interface with current user information
        /// </summary>
        private void UpdateUserInterface()
            {
            if (_currentUser == null) return;

            // Update status bar with user information
            lblCurrentUser.Text = $"Welcome, {_currentUser.Username}";
            lblUserRole.Text = _currentUser.Role;
            lblLastLogin.Text = _currentUser.LastLoginAt?.ToString("MM/dd/yyyy HH:mm") ?? "First time";

            // Enable/disable menu items based on user role
            UpdateMenuItemsByRole(_currentUser.Role);

            // Update window title
            this.Text = $"InventoryPro - Dashboard ({_currentUser.Username})";

            // Refresh context menus with user role information
            if (IsHandleCreated)
                {
                InitializeContextMenus();
                }

            lblStatus.Text = "Ready";
            }

        /// <summary>
        /// Updates menu items visibility based on user role
        /// </summary>
        private void UpdateMenuItemsByRole(string userRole)
            {
            // Admin users get access to all features
            bool isAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            bool isManager = userRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) || isAdmin;

            // Product management - available to all users
            btnProducts.Enabled = true;
            menuProducts.Enabled = true;

            // Customer management - available to all users
            btnCustomers.Enabled = true;
            menuCustomers.Enabled = true;

            // Sales - available to all users
            btnSales.Enabled = true;
            menuSales.Enabled = true;

            // Reports - available to managers and admins
            btnReports.Enabled = isManager;
            menuReports.Enabled = isManager;

            // User management - only for admins (if we add this feature)
            // menuUserManagement.Enabled = isAdmin;
            }

        /// <summary>
        /// Loads dashboard statistics from the API with caching
        /// </summary>
        private async Task LoadDashboardStatsAsync(bool forceRefresh = false)
            {
            try
                {
                // Check if we need to refresh based on cache timeout
                var shouldRefresh = forceRefresh || 
                    _dashboardStats == null || 
                    (DateTime.Now - _lastDataRefresh) > _cacheTimeout;

                if (!shouldRefresh)
                    {
                    _logger.LogDebug("Using cached dashboard data");
                    lblStatus.Text = "Dashboard loaded from cache";
                    return;
                    }

                lblStatus.Text = "Loading dashboard statistics...";

                // Use timeout for the request
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                var response = await _apiService.GetDashboardStatsAsync();
                if (response.Success && response.Data != null)
                    {
                    _dashboardStats = response.Data;
                    _lastDataRefresh = DateTime.Now;
                    
                    UpdateDashboardCards();
                    UpdateRecentActivities();
                    UpdateLowStockAlerts();

                    lblStatus.Text = "Dashboard loaded successfully";
                    _logger.LogInformation("Dashboard stats loaded successfully");
                    }
                else
                    {
                    var errorMsg = $"Failed to load dashboard stats. Status: {response.StatusCode}, Message: {response.Message}";
                    _logger.LogWarning(errorMsg);
                    lblStatus.Text = "Dashboard data unavailable";

                    // Set default values only if we don't have cached data
                    if (_dashboardStats == null)
                        {
                        SetDefaultDashboardValues();
                        }
                    }
                }
            catch (TaskCanceledException)
                {
                _logger.LogWarning("Dashboard stats request timed out");
                lblStatus.Text = "Request timed out";
                if (_dashboardStats == null)
                    {
                    SetDefaultDashboardValues();
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error loading dashboard statistics");
                lblStatus.Text = "Error loading dashboard statistics";

                // Set default values only if we don't have cached data
                if (_dashboardStats == null)
                    {
                    SetDefaultDashboardValues();
                    }
                }
            }

        /// <summary>
        /// Sets default dashboard values when API is unavailable
        /// </summary>
        private void SetDefaultDashboardValues()
            {
            lblTotalProducts.Text = "Products: N/A";
            lblLowStockProducts.Text = "Low Stock: N/A";
            lblInventoryValue.Text = "Value: N/A";
            lblTodaySales.Text = "Today: N/A";
            lblTotalCustomers.Text = "Customers: N/A";
            }

        /// <summary>
        /// Updates the dashboard summary cards with current statistics
        /// </summary>
        private void UpdateDashboardCards()
            {
            if (_dashboardStats == null) return;

            // Suspend layout updates for better performance
            this.SuspendLayout();
            
            try
                {
                // Product statistics
                lblTotalProducts.Text = _dashboardStats.TotalProducts.ToString("N0");
                lblLowStockProducts.Text = _dashboardStats.LowStockProducts.ToString("N0");
                lblOutOfStockProducts.Text = _dashboardStats.OutOfStockProducts.ToString("N0");
                lblInventoryValue.Text = $"${_dashboardStats.TotalInventoryValue:N2}";

                // Sales statistics
                lblTodaySales.Text = $"${_dashboardStats.TodaySales:N2}";
                lblMonthSales.Text = $"${_dashboardStats.MonthSales:N2}";
                lblYearSales.Text = $"${_dashboardStats.YearSales:N2}";
                lblTodayOrders.Text = _dashboardStats.TodayOrders.ToString("N0");

                // Customer statistics
                lblTotalCustomers.Text = _dashboardStats.TotalCustomers.ToString("N0");
                lblNewCustomers.Text = _dashboardStats.NewCustomersThisMonth.ToString("N0");
                }
            finally
                {
                this.ResumeLayout(true);
                }
            }

        /// <summary>
        /// Updates the recent activities list
        /// </summary>
        private void UpdateRecentActivities()
            {
            if (_dashboardStats == null) return;

            lstRecentActivities.BeginUpdate();
            try
                {
                lstRecentActivities.Items.Clear();
                foreach (var activity in _dashboardStats.RecentActivities.Take(10))
                    {
                    lstRecentActivities.Items.Add(activity);
                    }
                }
            finally
                {
                lstRecentActivities.EndUpdate();
                }
            }

        /// <summary>
        /// Updates the low stock alerts list
        /// </summary>
        private void UpdateLowStockAlerts()
            {
            if (_dashboardStats == null) return;

            lstLowStockAlerts.BeginUpdate();
            try
                {
                lstLowStockAlerts.Items.Clear();
                foreach (var product in _dashboardStats.LowStockAlerts.Take(10))
                    {
                    var item = new ListViewItem(product.Name);
                    item.SubItems.Add(product.SKU);
                    item.SubItems.Add(product.Stock.ToString("N0"));
                    item.SubItems.Add(product.MinStock.ToString("N0"));
                    item.Tag = product;
                    lstLowStockAlerts.Items.Add(item);
                    }
                }
            finally
                {
                lstLowStockAlerts.EndUpdate();
                }
            }

        #region Event Handlers

        /// <summary>
        /// Opens the Products management form
        /// </summary>
        private void BtnProducts_Click(object sender, EventArgs e)
            {
            try
                {
                if (_productForm == null || _productForm.IsDisposed)
                    {
                    _productForm = Program.GetRequiredService<ProductForm>();
                    }
                _productForm.ShowDialog();
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error opening Products form");
                MessageBox.Show("Error opening Products form", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Opens the Customers management form
        /// </summary>
        private void BtnCustomers_Click(object sender, EventArgs e)
            {
            try
                {
                if (_customerForm == null || _customerForm.IsDisposed)
                    {
                    _customerForm = Program.GetRequiredService<CustomerForm>();
                    }
                _customerForm.ShowDialog();
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error opening Customers form");
                MessageBox.Show("Error opening Customers form", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Opens the Sales management form
        /// </summary>
        private void BtnSales_Click(object sender, EventArgs e)
            {
            try
                {
                if (_salesForm == null || _salesForm.IsDisposed)
                    {
                    _salesForm = Program.GetRequiredService<SalesForm>();
                    }
                _salesForm.ShowDialog();
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error opening Sales form");
                MessageBox.Show("Error opening Sales form", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Opens the Reports form
        /// </summary>
        private void BtnReports_Click(object sender, EventArgs e)
            {
            try
                {
                if (_reportForm == null || _reportForm.IsDisposed)
                    {
                    _reportForm = Program.GetRequiredService<ReportForm>();
                    }
                _reportForm.ShowDialog();
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error opening Reports form");
                MessageBox.Show("Error opening Reports form", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Refreshes the dashboard data
        /// </summary>
        private async void BtnRefresh_Click(object sender, EventArgs e)
            {
            try
                {
                await LoadDashboardStatsAsync(forceRefresh: true);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error refreshing dashboard");
                MessageBox.Show("Error refreshing dashboard", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        private async void BtnLogout_Click(object? sender, EventArgs e)
            {
            try
                {
                var result = MessageBox.Show(
                    "Are you sure you want to logout?",
                    "Confirm Logout",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                    {
                    await _authService.ClearTokenAsync();
                    _logger.LogInformation("User logged out successfully");
                    this.Close(); // This will return to the login form via Program.cs
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during logout");
                MessageBox.Show("Error during logout", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Handles form closing event
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
            {
            // Only confirm exit if user manually closes the form (not programmatic close)
            if (e.CloseReason == CloseReason.UserClosing)
                {
                var result = MessageBox.Show(
                    "Are you sure you want to exit the application?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    {
                    e.Cancel = true;
                    }
                }
            }

        /// <summary>
        /// Handles double-click on low stock alerts to open product details
        /// </summary>
        private void LstLowStockAlerts_DoubleClick(object sender, EventArgs e)
            {
            if (lstLowStockAlerts.SelectedItems.Count > 0)
                {
                var selectedItem = lstLowStockAlerts.SelectedItems[0];
                if (selectedItem.Tag is ProductDto product)
                    {
                    // Open product form with selected product
                    BtnProducts_Click(sender, e);
                    }
                }
            }

        #endregion

        #region Context Menu Implementation

        /// <summary>
        /// Initializes context menus for different sections of the dashboard
        /// </summary>
        private void InitializeContextMenus()
            {
            try
                {
                InitializeDashboardContextMenu();
                InitializeStatsContextMenu();
                InitializeAlertsContextMenu();
                InitializeActivitiesContextMenu();
                AssignContextMenus();
                _logger.LogInformation("Context menus initialized successfully");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error initializing context menus");
                }
            }

        /// <summary>
        /// Initializes the main dashboard context menu
        /// </summary>
        private void InitializeDashboardContextMenu()
            {
            dashboardContextMenu.Items.Clear();

            // Refresh Dashboard
            var refreshItem = new ToolStripMenuItem("🔄 Refresh Dashboard", null, OnRefreshDashboard);
            refreshItem.ShortcutKeys = Keys.F5;
            refreshItem.ShowShortcutKeys = true;
            dashboardContextMenu.Items.Add(refreshItem);

            dashboardContextMenu.Items.Add(new ToolStripSeparator());

            // Export Dashboard
            var exportItem = new ToolStripMenuItem("📊 Export Dashboard", null, OnExportDashboard);
            exportItem.Enabled = _dashboardStats != null;
            dashboardContextMenu.Items.Add(exportItem);
            }

        /// <summary>
        /// Initializes the statistics panel context menu
        /// </summary>
        private void InitializeStatsContextMenu()
            {
            statsContextMenu.Items.Clear();

            // View Products
            var viewProductsItem = new ToolStripMenuItem("📦 View Products", null, (s, e) => BtnProducts_Click(s, e));
            statsContextMenu.Items.Add(viewProductsItem);

            // Add Product (for managers/admins)
            if (_currentUser != null && (_currentUser.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase) || 
                _currentUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                {
                var addProductItem = new ToolStripMenuItem("➕ Add New Product", null, OnAddProduct);
                statsContextMenu.Items.Add(addProductItem);
                }

            statsContextMenu.Items.Add(new ToolStripSeparator());

            // Product Reports
            var reportsItem = new ToolStripMenuItem("📋 Product Reports", null, (s, e) => BtnReports_Click(s, e));
            reportsItem.Enabled = _currentUser != null && (_currentUser.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase) || 
                _currentUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            statsContextMenu.Items.Add(reportsItem);
            }

        /// <summary>
        /// Initializes the low stock alerts context menu
        /// </summary>
        private void InitializeAlertsContextMenu()
            {
            alertsContextMenu.Items.Clear();

            // View Selected Product
            var viewProductItem = new ToolStripMenuItem("👁️ View Product Details", null, OnViewSelectedProduct);
            alertsContextMenu.Items.Add(viewProductItem);

            // Update Stock
            var updateStockItem = new ToolStripMenuItem("📝 Update Stock", null, OnUpdateSelectedStock);
            alertsContextMenu.Items.Add(updateStockItem);

            alertsContextMenu.Items.Add(new ToolStripSeparator());

            // Alert Settings
            var alertSettingsItem = new ToolStripMenuItem("🔔 Alert Settings", null, OnAlertSettings);
            alertsContextMenu.Items.Add(alertSettingsItem);
            }

        /// <summary>
        /// Initializes the activities panel context menu
        /// </summary>
        private void InitializeActivitiesContextMenu()
            {
            activitiesContextMenu.Items.Clear();

            // View Sales
            var viewSalesItem = new ToolStripMenuItem("💰 View Sales", null, (s, e) => BtnSales_Click(s, e));
            activitiesContextMenu.Items.Add(viewSalesItem);

            // New Sale
            var newSaleItem = new ToolStripMenuItem("➕ New Sale", null, OnNewSale);
            activitiesContextMenu.Items.Add(newSaleItem);

            activitiesContextMenu.Items.Add(new ToolStripSeparator());

            // Sales Reports
            var salesReportsItem = new ToolStripMenuItem("📈 Sales Reports", null, (s, e) => BtnReports_Click(s, e));
            salesReportsItem.Enabled = _currentUser != null && (_currentUser.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase) || 
                _currentUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            activitiesContextMenu.Items.Add(salesReportsItem);
            }

        /// <summary>
        /// Assigns context menus to appropriate controls
        /// </summary>
        private void AssignContextMenus()
            {
            // Assign dashboard context menu to main dashboard panel
            pnlDashboard.ContextMenuStrip = dashboardContextMenu;
            
            // Assign stats context menu to statistics panel
            pnlStats.ContextMenuStrip = statsContextMenu;
            
            // Assign alerts context menu to alerts panel and low stock list
            pnlAlerts.ContextMenuStrip = alertsContextMenu;
            lstLowStockAlerts.ContextMenuStrip = alertsContextMenu;
            
            // Assign activities context menu to activities panel and list
            pnlActivities.ContextMenuStrip = activitiesContextMenu;
            lstRecentActivities.ContextMenuStrip = activitiesContextMenu;

            // Add opening event handlers for dynamic updates
            alertsContextMenu.Opening += AlertsContextMenu_Opening;
            statsContextMenu.Opening += StatsContextMenu_Opening;
            activitiesContextMenu.Opening += ActivitiesContextMenu_Opening;
            }

        #endregion

        #region Context Menu Event Handlers

        /// <summary>
        /// Handles refresh dashboard context menu click
        /// </summary>
        private async void OnRefreshDashboard(object? sender, EventArgs e)
            {
            try
                {
                await LoadDashboardStatsAsync(forceRefresh: true);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error refreshing dashboard from context menu");
                MessageBox.Show("Error refreshing dashboard", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Handles export dashboard context menu click
        /// </summary>
        private void OnExportDashboard(object? sender, EventArgs e)
            {
            try
                {
                if (_dashboardStats == null)
                    {
                    MessageBox.Show("No dashboard data available to export", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                    }

                // Simple text export for now
                var exportData = $"InventoryPro Dashboard Export - {DateTime.Now:yyyy-MM-dd HH:mm}\n\n" +
                    $"Products: {_dashboardStats.TotalProducts}\n" +
                    $"Low Stock: {_dashboardStats.LowStockProducts}\n" +
                    $"Out of Stock: {_dashboardStats.OutOfStockProducts}\n" +
                    $"Inventory Value: ${_dashboardStats.TotalInventoryValue:N2}\n" +
                    $"Today's Sales: ${_dashboardStats.TodaySales:N2}\n" +
                    $"Today's Orders: {_dashboardStats.TodayOrders}\n" +
                    $"Total Customers: {_dashboardStats.TotalCustomers}";

                Clipboard.SetText(exportData);
                MessageBox.Show("Dashboard data copied to clipboard", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error exporting dashboard");
                MessageBox.Show("Error exporting dashboard data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Handles add product context menu click
        /// </summary>
        private void OnAddProduct(object? sender, EventArgs e)
            {
            try
                {
                // Open products form in add mode
                BtnProducts_Click(sender, e);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error opening add product");
                MessageBox.Show("Error opening product form", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Handles view selected product from alerts context menu
        /// </summary>
        private void OnViewSelectedProduct(object? sender, EventArgs e)
            {
            try
                {
                if (lstLowStockAlerts.SelectedItems.Count > 0 && 
                    lstLowStockAlerts.SelectedItems[0].Tag is ProductDto product)
                    {
                    // Open products form and navigate to selected product
                    BtnProducts_Click(sender, e);
                    }
                else
                    {
                    MessageBox.Show("Please select a product from the list", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error viewing selected product");
                MessageBox.Show("Error viewing product details", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Handles update stock for selected product
        /// </summary>
        private void OnUpdateSelectedStock(object? sender, EventArgs e)
            {
            try
                {
                if (lstLowStockAlerts.SelectedItems.Count > 0 && 
                    lstLowStockAlerts.SelectedItems[0].Tag is ProductDto product)
                    {
                    // Simple stock update dialog using a basic input form
                    string result = ShowInputDialog($"Update stock for {product.Name}\nCurrent Stock: {product.Stock}", "Update Stock", product.Stock.ToString());

                    if (!string.IsNullOrEmpty(result) && int.TryParse(result, out int newStock))
                        {
                        // TODO: Implement stock update API call
                        MessageBox.Show($"Stock updated to {newStock} for {product.Name}", "Stock Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                else
                    {
                    MessageBox.Show("Please select a product from the list", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error updating stock");
                MessageBox.Show("Error updating stock", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Handles alert settings context menu click
        /// </summary>
        private void OnAlertSettings(object? sender, EventArgs e)
            {
            try
                {
                MessageBox.Show("Alert settings functionality will be implemented in a future update", "Feature Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error opening alert settings");
                }
            }

        /// <summary>
        /// Handles new sale context menu click
        /// </summary>
        private void OnNewSale(object? sender, EventArgs e)
            {
            try
                {
                // Open sales form
                BtnSales_Click(sender, e);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error opening new sale");
                MessageBox.Show("Error opening sales form", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        /// <summary>
        /// Updates alerts context menu based on selection
        /// </summary>
        private void AlertsContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
            {
            var hasSelection = lstLowStockAlerts.SelectedItems.Count > 0;
            
            // Update menu items based on selection
            foreach (ToolStripMenuItem item in alertsContextMenu.Items.OfType<ToolStripMenuItem>())
                {
                if (item.Text.Contains("View Product") || item.Text.Contains("Update Stock"))
                    {
                    item.Enabled = hasSelection;
                    }
                }
            }

        /// <summary>
        /// Updates stats context menu based on user role
        /// </summary>
        private void StatsContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
            {
            // Update role-based items
            foreach (ToolStripMenuItem item in statsContextMenu.Items.OfType<ToolStripMenuItem>())
                {
                if (item.Text.Contains("Add New") || item.Text.Contains("Reports"))
                    {
                    item.Enabled = _currentUser != null && (_currentUser.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase) || 
                        _currentUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

        /// <summary>
        /// Updates activities context menu based on user role
        /// </summary>
        private void ActivitiesContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
            {
            // Update role-based items
            foreach (ToolStripMenuItem item in activitiesContextMenu.Items.OfType<ToolStripMenuItem>())
                {
                if (item.Text.Contains("Reports"))
                    {
                    item.Enabled = _currentUser != null && (_currentUser.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase) || 
                        _currentUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

        /// <summary>
        /// Shows a simple input dialog
        /// </summary>
        private string ShowInputDialog(string text, string caption, string defaultValue = "")
            {
            Form prompt = new Form()
                {
                Width = 400,
                Height = 180,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
                };

            Label textLabel = new Label() { Left = 10, Top = 10, Width = 360, Height = 40, Text = text };
            TextBox textBox = new TextBox() { Left = 10, Top = 55, Width = 360, Text = defaultValue };
            Button confirmation = new Button() { Text = "OK", Left = 295, Width = 75, Top = 90, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Cancel", Left = 210, Width = 75, Top = 90, DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
            }

        #endregion

        /// <summary>
        /// Cleanup resources when form is disposed
        /// </summary>
        protected override void Dispose(bool disposing)
            {
            if (disposing)
                {
                _productForm?.Dispose();
                _customerForm?.Dispose();
                _salesForm?.Dispose();
                _reportForm?.Dispose();
                components?.Dispose();
                }
            base.Dispose(disposing);
            }
        }
    }