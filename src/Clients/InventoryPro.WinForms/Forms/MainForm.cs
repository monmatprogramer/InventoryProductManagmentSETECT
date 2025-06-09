using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

using InventoryPro.Services; // Updated namespace

namespace InventoryPro.WinForms.Forms
    {
    /// <summary>
    /// Main form of the application - dashboard with navigation
    /// Modern design with sidebar navigation and content area
    /// </summary>
    public partial class MainForm : Form
        {
        private readonly IServiceProvider _serviceProvider;
        private readonly AuthService _authService;

        // UI Controls
        private Panel sidebarPanel;
        private Panel contentPanel;
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblUser;
        private Button btnDashboard;
        private Button btnProducts;
        private Button btnCustomers;
        private Button btnSales;
        private Button btnReports;
        private Button btnLogout;
        private Button currentButton;

        // Dashboard controls
        private Panel dashboardPanel;
        private Label lblWelcome;
        private Panel[] statsPanels;

        public MainForm(IServiceProvider serviceProvider, AuthService authService)
            {
            _serviceProvider = serviceProvider;
            _authService = authService;
            InitializeComponent();
            SetupForm();
            ShowDashboard();
            }

        private void InitializeComponent()
            {
            this.Text = "InventoryPro - Management System";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 600);

            // Modern flat design
            this.BackColor = Color.FromArgb(245, 245, 245);
            }

        private void SetupForm()
            {
            // Header Panel
            headerPanel = new Panel
                {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(52, 73, 94)
                };

            lblTitle = new Label
                {
                Text = "InventoryPro Management System",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
                };

            lblUser = new Label
                {
                Text = $"Welcome, {_authService.CurrentUsername} ({_authService.CurrentUserRole})",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(this.Width - 250, 20),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblUser });

            // Sidebar Panel
            sidebarPanel = new Panel
                {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(44, 62, 80)
                };

            // Navigation buttons
            int buttonY = 20;
            int buttonHeight = 50;
            int buttonSpacing = 5;

            btnDashboard = CreateNavButton("📊 Dashboard", buttonY);
            buttonY += buttonHeight + buttonSpacing;

            btnProducts = CreateNavButton("📦 Products", buttonY);
            buttonY += buttonHeight + buttonSpacing;

            btnCustomers = CreateNavButton("👥 Customers", buttonY);
            buttonY += buttonHeight + buttonSpacing;

            btnSales = CreateNavButton("💰 Sales", buttonY);
            buttonY += buttonHeight + buttonSpacing;

            btnReports = CreateNavButton("📈 Reports", buttonY);

            // Logout button at bottom
            btnLogout = CreateNavButton("🚪 Logout", 0);
            btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnLogout.Location = new Point(10, sidebarPanel.Height - 60);
            btnLogout.BackColor = Color.FromArgb(192, 57, 43);

            // Add click handlers
            btnDashboard.Click += (s, e) => ShowDashboard();
            btnProducts.Click += (s, e) => ShowProducts();
            btnCustomers.Click += (s, e) => ShowCustomers();
            btnSales.Click += (s, e) => ShowSales();
            btnReports.Click += (s, e) => ShowReports();
            btnLogout.Click += (s, e) => Logout();

            sidebarPanel.Controls.AddRange(new Control[] {
                btnDashboard, btnProducts, btnCustomers, btnSales, btnReports, btnLogout
            });

            // Content Panel
            contentPanel = new Panel
                {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
                };

            // Add panels to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebarPanel);
            this.Controls.Add(headerPanel);

            // Set dashboard as active
            SetActiveButton(btnDashboard);
            }

        /// <summary>
        /// Creates a navigation button with consistent styling
        /// </summary>
        private Button CreateNavButton(string text, int y)
            {
            var button = new Button
                {
                Text = text,
                Location = new Point(10, y),
                Size = new Size(230, 45),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand
                };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);

            return button;
            }

        /// <summary>
        /// Sets the active navigation button
        /// </summary>
        private void SetActiveButton(Button button)
            {
            // Reset previous button
            if (currentButton != null)
                {
                currentButton.BackColor = Color.FromArgb(52, 73, 94);
                }

            // Highlight current button
            button.BackColor = Color.FromArgb(41, 128, 185);
            currentButton = button;
            }

        /// <summary>
        /// Shows dashboard view
        /// </summary>
        private void ShowDashboard()
            {
            SetActiveButton(btnDashboard);
            contentPanel.Controls.Clear();

            // Create dashboard panel
            dashboardPanel = new Panel
                {
                Dock = DockStyle.Fill
                };

            // Welcome message
            lblWelcome = new Label
                {
                Text = $"Welcome back, {_authService.CurrentUsername}!",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(0, 0),
                AutoSize = true
                };

            // Statistics panels
            statsPanels = new Panel[4];
            string[] titles = { "Total Products", "Low Stock Items", "Today's Sales", "Active Customers" };
            string[] values = { "156", "12", "$3,450", "89" };
            Color[] colors = {
                Color.FromArgb(52, 152, 219),    // Blue
                Color.FromArgb(231, 76, 60),     // Red
                Color.FromArgb(46, 204, 113),    // Green
                Color.FromArgb(155, 89, 182)     // Purple
            };

            int panelX = 0;
            int panelY = 80;
            int panelWidth = 250;
            int panelHeight = 120;
            int panelSpacing = 20;

            for (int i = 0; i < 4; i++)
                {
                statsPanels[i] = CreateStatPanel(titles[i], values[i], colors[i]);
                statsPanels[i].Location = new Point(panelX, panelY);
                panelX += panelWidth + panelSpacing;

                if (i == 1) // Start new row after 2 panels
                    {
                    panelX = 0;
                    panelY += panelHeight + panelSpacing;
                    }

                dashboardPanel.Controls.Add(statsPanels[i]);
                }

            dashboardPanel.Controls.Add(lblWelcome);
            contentPanel.Controls.Add(dashboardPanel);
            }

        /// <summary>
        /// Creates a statistics panel for dashboard
        /// </summary>
        private Panel CreateStatPanel(string title, string value, Color color)
            {
            var panel = new Panel
                {
                Size = new Size(250, 120),
                BackColor = color
                };

            var lblTitle = new Label
                {
                Text = title,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true
                };

            var lblValue = new Label
                {
                Text = value,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 45),
                AutoSize = true
                };

            panel.Controls.AddRange(new Control[] { lblTitle, lblValue });
            return panel;
            }

        /// <summary>
        /// Shows products management view
        /// </summary>
        private void ShowProducts()
            {
            SetActiveButton(btnProducts);
            contentPanel.Controls.Clear();

            var productForm = _serviceProvider.GetRequiredService<ProductForm>();
            productForm.TopLevel = false;
            productForm.FormBorderStyle = FormBorderStyle.None;
            productForm.Dock = DockStyle.Fill;

            contentPanel.Controls.Add(productForm);
            productForm.Show();
            }

        /// <summary>
        /// Shows customers management view
        /// </summary>
        private void ShowCustomers()
            {
            SetActiveButton(btnCustomers);
            contentPanel.Controls.Clear();

            // Show message for now
            var label = new Label
                {
                Text = "Customer Management - Coming Soon",
                Font = new Font("Segoe UI", 16),
                AutoSize = true,
                Location = new Point(20, 20)
                };
            contentPanel.Controls.Add(label);
            }

        /// <summary>
        /// Shows sales processing view
        /// </summary>
        private void ShowSales()
            {
            SetActiveButton(btnSales);
            contentPanel.Controls.Clear();

            // Show message for now
            var label = new Label
                {
                Text = "Sales Processing - Coming Soon",
                Font = new Font("Segoe UI", 16),
                AutoSize = true,
                Location = new Point(20, 20)
                };
            contentPanel.Controls.Add(label);
            }

        /// <summary>
        /// Shows reports view
        /// </summary>
        private void ShowReports()
            {
            SetActiveButton(btnReports);
            contentPanel.Controls.Clear();

            // Show message for now
            var label = new Label
                {
                Text = "Reports - Coming Soon",
                Font = new Font("Segoe UI", 16),
                AutoSize = true,
                Location = new Point(20, 20)
                };
            contentPanel.Controls.Add(label);
            }

        /// <summary>
        /// Logs out user and returns to login
        /// </summary>
        private void Logout()
            {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                {
                _authService.Logout();
                this.Close();

                // Restart application to show login
                Application.Restart();
                }
            }
        }
  
    }
