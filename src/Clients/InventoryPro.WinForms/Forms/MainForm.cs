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

            InitializeComponent();
            _ = InitializeFormAsync();
        }

        /// <summary>
        /// Initializes the form with user data and dashboard statistics
        /// </summary>
        private async Task InitializeFormAsync()
        {
            try
            {
                // Load current user information
                _currentUser = await _authService.GetCurrentUserAsync();
                if (_currentUser == null)
                    {
                    MessageBox.Show("User information not found. Please restart the application.",
                        "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                    }

                // Update UI with user information
                UpdateUserInterface();

                // Load dashboard statistics
                await LoadDashboardStatsAsync();

                _logger.LogInformation("MainForm initialized successfully for user: {Username}", _currentUser.Username);
                }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing MainForm");
                MessageBox.Show(
                    "Error loading application. Please try logging in again.",
                    "Application Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                ShowLoginForm();
            }
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
        /// Loads dashboard statistics from the API
        /// </summary>
        private async Task LoadDashboardStatsAsync()
        {
            try
            {
                lblStatus.Text = "Loading dashboard statistics...";

                var response = await _apiService.GetDashboardStatsAsync();
                if (response.Success && response.Data != null)
                {
                    _dashboardStats = response.Data;
                    UpdateDashboardCards();
                    UpdateRecentActivities();
                    UpdateLowStockAlerts();
                }
                else
                {
                    _logger.LogWarning("Failed to load dashboard stats: {Message}", response.Message);
                    lblStatus.Text = "Failed to load dashboard statistics";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard statistics");
                lblStatus.Text = "Error loading dashboard statistics";
            }
        }

        /// <summary>
        /// Updates the dashboard summary cards with current statistics
        /// </summary>
        private void UpdateDashboardCards()
        {
            if (_dashboardStats == null) return;

            // Product statistics
            lblTotalProducts.Text = _dashboardStats.TotalProducts.ToString();
            lblLowStockProducts.Text = _dashboardStats.LowStockProducts.ToString();
            lblOutOfStockProducts.Text = _dashboardStats.OutOfStockProducts.ToString();
            lblInventoryValue.Text = $"${_dashboardStats.TotalInventoryValue:N2}";

            // Sales statistics
            lblTodaySales.Text = $"${_dashboardStats.TodaySales:N2}";
            lblMonthSales.Text = $"${_dashboardStats.MonthSales:N2}";
            lblYearSales.Text = $"${_dashboardStats.YearSales:N2}";
            lblTodayOrders.Text = _dashboardStats.TodayOrders.ToString();

            // Customer statistics
            lblTotalCustomers.Text = _dashboardStats.TotalCustomers.ToString();
            lblNewCustomers.Text = _dashboardStats.NewCustomersThisMonth.ToString();

            // Update status
            lblStatus.Text = "Dashboard updated successfully";
        }

        /// <summary>
        /// Updates the recent activities list
        /// </summary>
        private void UpdateRecentActivities()
        {
            if (_dashboardStats == null) return;

            lstRecentActivities.Items.Clear();
            foreach (var activity in _dashboardStats.RecentActivities.Take(10))
            {
                lstRecentActivities.Items.Add(activity);
            }
        }

        /// <summary>
        /// Updates the low stock alerts list
        /// </summary>
        private void UpdateLowStockAlerts()
        {
            if (_dashboardStats == null) return;

            lstLowStockAlerts.Items.Clear();
            foreach (var product in _dashboardStats.LowStockAlerts.Take(10))
            {
                var item = new ListViewItem(product.Name);
                item.SubItems.Add(product.SKU);
                item.SubItems.Add(product.Stock.ToString());
                item.SubItems.Add(product.MinStock.ToString());
                item.Tag = product;
                lstLowStockAlerts.Items.Add(item);
            }
        }

        /// <summary>
        /// Shows the login form and hides the main form
        /// </summary>
        private void ShowLoginForm()
            {
            this.Hide();
            var loginForm = Program.GetRequiredService<LoginForm>();

            // Subscribe to a custom event or method for login success
            loginForm.FormClosed += async (sender, args) =>
            {
                if (loginForm.DialogResult == DialogResult.OK)
                {
                    await InitializeFormAsync();
                    this.Show();
                }
            };

            loginForm.ShowDialog();
        }

        /// <summary>
        /// Handles successful login event
        /// </summary>
        private async void OnLoginSuccessful(object? sender, EventArgs e)
        {
            this.Show();
            await InitializeFormAsync();
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
            await LoadDashboardStatsAsync();
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        private async void BtnLogout_Click(object sender, EventArgs e)
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
                    this.Close();
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
            // Confirm exit if user tries to close the form
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