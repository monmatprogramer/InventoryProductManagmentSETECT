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