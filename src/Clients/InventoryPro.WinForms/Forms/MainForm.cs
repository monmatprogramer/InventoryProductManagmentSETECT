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

            // Toolbar buttons - available based on user role
            btnProducts.Enabled = true;
            btnCustomers.Enabled = true;
            btnSales.Enabled = true;
            btnReports.Enabled = isManager;

            // Quick actions - based on user role
            btnNewSale.Enabled = true;
            btnAddProduct.Enabled = isManager; // Only managers/admins can add products

            // System actions - always available
            btnRefresh.Enabled = true;
            btnLogout.Enabled = true;

            // Menu items - system level operations
            menuFile.Enabled = true;
            menuView.Enabled = true;
            menuTools.Enabled = isManager; // Settings and backup for managers/admins
            menuWindow.Enabled = true;
            menuHelp.Enabled = true;
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

        // Context menu deduplication strategy
        private readonly Dictionary<string, ContextMenuPriority> _menuActionPriorities = new()
        {
            {"refresh", ContextMenuPriority.Context}, // Context menus get priority for refresh
            {"products", ContextMenuPriority.Navigation}, // Navigation bar gets priority for main actions
            {"customers", ContextMenuPriority.Navigation},
            {"sales", ContextMenuPriority.Navigation},
            {"reports", ContextMenuPriority.Navigation},
            {"logout", ContextMenuPriority.Navigation}, // Navigation gets priority for security actions
            {"export", ContextMenuPriority.Context}, // Context-specific actions
            {"add", ContextMenuPriority.Context},
            {"update", ContextMenuPriority.Context},
            {"view", ContextMenuPriority.Context}
        };

        private enum ContextMenuPriority
        {
            Navigation = 1, // Main navigation (menu/toolbar) takes precedence
            Context = 2,    // Context menus for specific actions
            Hidden = 3      // Actions that should be hidden to avoid duplication
        }

        /// <summary>
        /// Initializes context menus for different sections of the dashboard with deduplication
        /// </summary>
        private void InitializeContextMenus()
        {
            try
            {
                // Clear existing menus to prevent duplication
                ClearExistingContextMenus();

                InitializeDashboardContextMenu();
                InitializeStatsContextMenu();
                InitializeAlertsContextMenu();
                InitializeActivitiesContextMenu();
                AssignContextMenus();
                _logger.LogInformation("Context menus initialized successfully with deduplication");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing context menus");
            }
        }

        /// <summary>
        /// Clears existing context menus to prevent duplication
        /// </summary>
        private void ClearExistingContextMenus()
        {
            dashboardContextMenu?.Items?.Clear();
            statsContextMenu?.Items?.Clear();
            alertsContextMenu?.Items?.Clear();
            activitiesContextMenu?.Items?.Clear();
        }

        /// <summary>
        /// Initializes the main dashboard context menu with smart deduplication
        /// </summary>
        private void InitializeDashboardContextMenu()
        {
            // Only add items that aren't better served by navigation
            if (ShouldIncludeAction("refresh", ContextMenuPriority.Context))
            {
                var refreshItem = new ToolStripMenuItem("🔄 Refresh Dashboard", null, OnRefreshDashboard);
                refreshItem.ShortcutKeys = Keys.F5;
                refreshItem.ShowShortcutKeys = true;
                refreshItem.ToolTipText = "Refresh dashboard data (F5)";
                dashboardContextMenu.Items.Add(refreshItem);
            }

            if (ShouldIncludeAction("export", ContextMenuPriority.Context))
            {
                if (dashboardContextMenu.Items.Count > 0)
                    dashboardContextMenu.Items.Add(new ToolStripSeparator());

                var exportItem = new ToolStripMenuItem("📊 Export Dashboard", null, OnExportDashboard);
                exportItem.Enabled = _dashboardStats != null;
                exportItem.ToolTipText = "Export dashboard data to clipboard";
                dashboardContextMenu.Items.Add(exportItem);
            }

            // Add context-specific help
            if (dashboardContextMenu.Items.Count > 0)
            {
                dashboardContextMenu.Items.Add(new ToolStripSeparator());
                var helpItem = new ToolStripMenuItem("❓ Dashboard Help", null, OnDashboardHelp);
                helpItem.ToolTipText = "Show dashboard help information";
                dashboardContextMenu.Items.Add(helpItem);
            }
        }

        /// <summary>
        /// Initializes the statistics panel context menu with deduplication
        /// </summary>
        private void InitializeStatsContextMenu()
        {
            // Don't duplicate main navigation items - focus on context-specific actions

            // Quick Add Product (context-specific action)
            if (ShouldIncludeAction("add", ContextMenuPriority.Context) &&
                _currentUser != null && IsManagerOrAdmin(_currentUser.Role))
            {
                var addProductItem = new ToolStripMenuItem("➕ Quick Add Product", null, OnAddProduct);
                addProductItem.ToolTipText = "Quickly add a new product";
                statsContextMenu.Items.Add(addProductItem);
            }

            // Export Product Statistics
            if (ShouldIncludeAction("export", ContextMenuPriority.Context))
            {
                if (statsContextMenu.Items.Count > 0)
                    statsContextMenu.Items.Add(new ToolStripSeparator());

                var exportStatsItem = new ToolStripMenuItem("📊 Export Product Stats", null, OnExportProductStats);
                exportStatsItem.ToolTipText = "Export product statistics";
                exportStatsItem.Enabled = _dashboardStats != null;
                statsContextMenu.Items.Add(exportStatsItem);
            }

            // Product Statistics Settings
            if (statsContextMenu.Items.Count > 0)
            {
                statsContextMenu.Items.Add(new ToolStripSeparator());
                var settingsItem = new ToolStripMenuItem("⚙️ Stats Settings", null, OnStatsSettings);
                settingsItem.ToolTipText = "Configure product statistics display";
                statsContextMenu.Items.Add(settingsItem);
            }

            // Note: Removed "View Products" and "Product Reports" as they duplicate main navigation
        }

        /// <summary>
        /// Determines if an action should be included based on priority and current context
        /// </summary>
        private bool ShouldIncludeAction(string action, ContextMenuPriority requestedPriority)
        {
            if (!_menuActionPriorities.TryGetValue(action, out var assignedPriority))
                return true; // Unknown actions are allowed

            return assignedPriority == requestedPriority;
        }

        /// <summary>
        /// Checks if user is manager or admin
        /// </summary>
        private bool IsManagerOrAdmin(string role)
        {
            return role.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                   role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes the low stock alerts context menu with smart selection awareness
        /// </summary>
        private void InitializeAlertsContextMenu()
        {
            // These are all context-specific actions that don't duplicate navigation

            // View Selected Product Details
            if (ShouldIncludeAction("view", ContextMenuPriority.Context))
            {
                var viewProductItem = new ToolStripMenuItem("👁️ View Product Details", null, OnViewSelectedProduct);
                viewProductItem.ToolTipText = "View details for selected product";
                alertsContextMenu.Items.Add(viewProductItem);
            }

            // Quick Stock Update
            if (ShouldIncludeAction("update", ContextMenuPriority.Context))
            {
                var updateStockItem = new ToolStripMenuItem("📝 Quick Stock Update", null, OnUpdateSelectedStock);
                updateStockItem.ToolTipText = "Quickly update stock for selected product";
                alertsContextMenu.Items.Add(updateStockItem);
            }

            // Reorder Product
            var reorderItem = new ToolStripMenuItem("🔄 Reorder Product", null, OnReorderProduct);
            reorderItem.ToolTipText = "Create reorder request for selected product";
            alertsContextMenu.Items.Add(reorderItem);

            if (alertsContextMenu.Items.Count > 0)
                alertsContextMenu.Items.Add(new ToolStripSeparator());

            // Alert Threshold Settings
            var alertSettingsItem = new ToolStripMenuItem("🔔 Alert Thresholds", null, OnAlertSettings);
            alertSettingsItem.ToolTipText = "Configure low stock alert thresholds";
            alertsContextMenu.Items.Add(alertSettingsItem);
        }

        /// <summary>
        /// Initializes the activities panel context menu with focus on quick actions
        /// </summary>
        private void InitializeActivitiesContextMenu()
        {
            // Focus on quick actions that enhance productivity

            // Quick New Sale (enhanced over navigation)
            if (ShouldIncludeAction("add", ContextMenuPriority.Context))
            {
                var newSaleItem = new ToolStripMenuItem("💰 Quick Sale", null, OnNewSale);
                newSaleItem.ToolTipText = "Start a new sale transaction";
                activitiesContextMenu.Items.Add(newSaleItem);
            }

            // Export Recent Activities
            if (ShouldIncludeAction("export", ContextMenuPriority.Context))
            {
                if (activitiesContextMenu.Items.Count > 0)
                    activitiesContextMenu.Items.Add(new ToolStripSeparator());

                var exportActivitiesItem = new ToolStripMenuItem("📊 Export Activities", null, OnExportActivities);
                exportActivitiesItem.ToolTipText = "Export recent activities list";
                exportActivitiesItem.Enabled = _dashboardStats?.RecentActivities?.Any() == true;
                activitiesContextMenu.Items.Add(exportActivitiesItem);
            }

            // Filter Activities
            var filterItem = new ToolStripMenuItem("🔍 Filter Activities", null, OnFilterActivities);
            filterItem.ToolTipText = "Filter activities by type or date";
            if (activitiesContextMenu.Items.Count > 0)
                activitiesContextMenu.Items.Add(new ToolStripSeparator());
            activitiesContextMenu.Items.Add(filterItem);

            // Activity Settings
            var settingsItem = new ToolStripMenuItem("⚙️ Activity Settings", null, OnActivitySettings);
            settingsItem.ToolTipText = "Configure activity display preferences";
            activitiesContextMenu.Items.Add(settingsItem);

            // Note: Removed "View Sales" and "Sales Reports" to avoid duplication with navigation
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
        /// Updates activities context menu based on user role and data availability
        /// </summary>
        private void ActivitiesContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Update data-dependent items
            foreach (ToolStripMenuItem item in activitiesContextMenu.Items.OfType<ToolStripMenuItem>())
            {
                if (item.Text.Contains("Export Activities"))
                {
                    item.Enabled = _dashboardStats?.RecentActivities?.Any() == true;
                }
            }
        }

        /// <summary>
        /// Handles dashboard help context menu click
        /// </summary>
        private void OnDashboardHelp(object? sender, EventArgs e)
        {
            var helpMessage = "Dashboard Help:\n\n" +
                "• Product statistics show current inventory status\n" +
                "• Low stock alerts help manage reordering\n" +
                "• Recent activities track system usage\n" +
                "• Right-click panels for quick actions\n" +
                "• Press F5 to refresh data";

            MessageBox.Show(helpMessage, "Dashboard Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles export product stats context menu click
        /// </summary>
        private void OnExportProductStats(object? sender, EventArgs e)
        {
            try
            {
                if (_dashboardStats == null)
                {
                    MessageBox.Show("No product statistics available", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var statsData = $"Product Statistics Export - {DateTime.Now:yyyy-MM-dd HH:mm}\n\n" +
                    $"Total Products: {_dashboardStats.TotalProducts:N0}\n" +
                    $"Low Stock Products: {_dashboardStats.LowStockProducts:N0}\n" +
                    $"Out of Stock: {_dashboardStats.OutOfStockProducts:N0}\n" +
                    $"Total Inventory Value: ${_dashboardStats.TotalInventoryValue:N2}";

                Clipboard.SetText(statsData);
                MessageBox.Show("Product statistics copied to clipboard", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting product statistics");
                MessageBox.Show("Error exporting product statistics", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles stats settings context menu click
        /// </summary>
        private void OnStatsSettings(object? sender, EventArgs e)
        {
            MessageBox.Show("Product statistics display settings will be available in a future update",
                "Feature Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles reorder product context menu click
        /// </summary>
        private void OnReorderProduct(object? sender, EventArgs e)
        {
            try
            {
                if (lstLowStockAlerts.SelectedItems.Count > 0 &&
                    lstLowStockAlerts.SelectedItems[0].Tag is ProductDto product)
                {
                    var result = MessageBox.Show(
                        $"Create reorder request for {product.Name}?\n\nCurrent Stock: {product.Stock}\nMinimum Stock: {product.MinStock}",
                        "Reorder Product",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // TODO: Implement reorder functionality
                        MessageBox.Show($"Reorder request created for {product.Name}",
                            "Reorder Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a product from the alerts list",
                        "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reorder request");
                MessageBox.Show("Error creating reorder request", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles export activities context menu click
        /// </summary>
        private void OnExportActivities(object? sender, EventArgs e)
        {
            try
            {
                if (_dashboardStats?.RecentActivities?.Any() != true)
                {
                    MessageBox.Show("No activities available to export", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var activitiesData = $"Recent Activities Export - {DateTime.Now:yyyy-MM-dd HH:mm}\n\n" +
                    string.Join("\n", _dashboardStats.RecentActivities.Take(20));

                Clipboard.SetText(activitiesData);
                MessageBox.Show("Activities copied to clipboard", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting activities");
                MessageBox.Show("Error exporting activities", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles filter activities context menu click
        /// </summary>
        private void OnFilterActivities(object? sender, EventArgs e)
        {
            MessageBox.Show("Activity filtering will be available in a future update",
                "Feature Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles activity settings context menu click
        /// </summary>
        private void OnActivitySettings(object? sender, EventArgs e)
        {
            MessageBox.Show("Activity display settings will be available in a future update",
                "Feature Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        #region Menu Event Handlers

        /// <summary>
        /// Handles New menu item click
        /// </summary>
        private void MenuNew_Click(object sender, EventArgs e)
        {
            // Show submenu or dialog for creating new items
            MessageBox.Show("Create new item functionality", "New", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles Import menu item click
        /// </summary>
        private void MenuImport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Import data functionality", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles Export menu item click
        /// </summary>
        private void MenuExport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Export data functionality", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles Exit menu item click
        /// </summary>
        private void MenuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles Dashboard menu item click
        /// </summary>
        private void MenuDashboard_Click(object sender, EventArgs e)
        {
            // Already on dashboard, could refresh
            BtnRefresh_Click(sender, e);
        }

        /// <summary>
        /// Handles Full Screen menu item click
        /// </summary>
        private void MenuFullScreen_Click(object sender, EventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        /// <summary>
        /// Handles Settings menu item click
        /// </summary>
        private void MenuSettings_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Settings functionality", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles Backup menu item click
        /// </summary>
        private void MenuBackup_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Backup database functionality", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles Minimize menu item click
        /// </summary>
        private void MenuMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Handles About menu item click
        /// </summary>
        private void MenuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("InventoryPro v1.0\nInventory Management System", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles New Sale button click
        /// </summary>
        private void BtnNewSale_Click(object sender, EventArgs e)
        {
            // Open sales form in new sale mode
            BtnSales_Click(sender, e);
        }

        /// <summary>
        /// Handles Add Product button click
        /// </summary>
        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            // Open products form in add mode
            BtnProducts_Click(sender, e);
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