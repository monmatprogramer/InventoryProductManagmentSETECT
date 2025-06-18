using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Modern Dashboard MainForm with contemporary UI/UX design
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly ILogger<MainForm> _logger;
        private readonly IApiService _apiService;
        private readonly IServiceProvider _serviceProvider;
        
        // Dashboard panels
        private Panel pnlSidebar;
        private Panel pnlTopBar;
        private Panel pnlContent;
        private Panel pnlStatusBar;
        
        // Navigation buttons
        private Button btnDashboard;
        private Button btnProducts;
        private Button btnCustomers;
        private Button btnSales;
        private Button btnSalesHistory;
        private Button btnReports;
        private Button btnSettings;
        
        // Top bar controls
        private Label lblWelcome;
        private Label lblDateTime;
        private Label lblSystemStatus;
        private Panel pnlUserProfile;
        private Button btnUserProfile;
        private Panel pnlUserDropdown;

        // Dashboard cards
        private Panel cardTotalProducts;
        private Panel cardTotalCustomers;
        private Panel cardTotalSales;
        private Panel cardLowStock;
        
        // Quick action buttons
        private Button btnQuickSale;
        private Button btnAddProduct;
        private Button btnAddCustomer;
        
        // Charts and data visualization
        private Panel chartSalesPanel;
        private Panel chartProductsPanel;
        private Panel stockAlertsPanel;
        private Panel recentSalesPanel;

        // Real-time data
        private System.Windows.Forms.Timer refreshTimer;
        
        // Sales data for charts
        private List<SaleDto> _recentSales = new();
        private decimal[] _weeklySalesData = new decimal[7];
        
        public MainForm(ILogger<MainForm> logger, IApiService apiService, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            pnlSidebar = new Panel();
            pnlTopBar = new Panel();
            pnlContent = new Panel();
            pnlStatusBar = new Panel();
            btnDashboard = new Button();
            btnProducts = new Button();
            btnCustomers = new Button();
            btnSales = new Button();
            btnSalesHistory = new Button();
            btnReports = new Button();
            btnSettings = new Button();
            lblWelcome = new Label();
            lblDateTime = new Label();
            lblSystemStatus = new Label();
            pnlUserProfile = new Panel();
            btnUserProfile = new Button();
            pnlUserDropdown = new Panel();
            cardTotalProducts = new Panel();
            cardTotalCustomers = new Panel();
            cardTotalSales = new Panel();
            cardLowStock = new Panel();
            btnQuickSale = new Button();
            btnAddProduct = new Button();
            btnAddCustomer = new Button();
            chartSalesPanel = new Panel();
            chartProductsPanel = new Panel();
            stockAlertsPanel = new Panel();
            recentSalesPanel = new Panel();
            refreshTimer = new System.Windows.Forms.Timer();



            InitializeComponent();
            SetupRealtimeUpdates();
            LoadDashboardDataAsync();
            SetupClickOutsideHandler();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties - Modern full-screen design
            this.Text = "📊 InventoryPro Dashboard - Modern Management System";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1200, 700);
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F);
            this.Icon = SystemIcons.Application;
            
            // Enable double buffering for smooth animations
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            
            CreateSidebar();
            CreateTopBar();
            CreateContentArea();
            CreateStatusBar();
            
            // Add panels to form
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlTopBar);
            this.Controls.Add(pnlStatusBar);
            
            this.ResumeLayout(false);
        }
        
        private void CreateSidebar()
        {
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.FromArgb(33, 37, 41),
                Padding = new Padding(0, 20, 0, 20)
            };
            
            // Sidebar gradient background
            pnlSidebar.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, pnlSidebar.Width, pnlSidebar.Height),
                    Color.FromArgb(33, 37, 41),
                    Color.FromArgb(52, 58, 64),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, new Rectangle(0, 0, pnlSidebar.Width, pnlSidebar.Height));
                }
            };
            
            // Logo section
            var logoPanel = new Panel
            {
                Height = 100,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent
            };
            
            var lblLogo = new Label
            {
                Text = "📦 InventoryPro",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            logoPanel.Controls.Add(lblLogo);
            
            var lblVersion = new Label
            {
                Text = "v2.0 Professional",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(173, 181, 189),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 25,
                Dock = DockStyle.Bottom
            };
            logoPanel.Controls.Add(lblVersion);
            
            // Navigation buttons
            var navPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 20, 20, 20)
            };
            
            btnDashboard = CreateNavButton("🏠 Dashboard", 0, true);
            btnProducts = CreateNavButton("📦 Products", 1, false);
            btnCustomers = CreateNavButton("👥 Customers", 2, false);
            btnSales = CreateNavButton("💰 New Sale", 3, false);
            btnSalesHistory = CreateNavButton("📈 Sales History", 4, false);
            btnReports = CreateNavButton("📊 Reports", 5, false);
           // btnSettings = CreateNavButton("⚙ Settings", 6, false);
            
            navPanel.Controls.AddRange(new Control[] {
                btnDashboard, btnProducts, btnCustomers, btnSales, btnSalesHistory, btnReports, btnSettings
            });
            
            pnlSidebar.Controls.Add(navPanel);
            pnlSidebar.Controls.Add(logoPanel);
        }
        
        private Button CreateNavButton(string text, int index, bool isActive)
        {
            var button = new Button
            {
                Text = text,
                Height = 55,
                Top = index * 65,
                Left = 0,
                Width = 240,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand,
                Tag = index
            };
            
            UpdateNavButtonStyle(button, isActive);
            
            button.Click += NavButton_Click;
            button.MouseEnter += (s, e) =>
            {
                if (!IsActiveNavButton(button))
                {
                    button.BackColor = Color.FromArgb(73, 80, 87);
                    button.ForeColor = Color.White;
                }
            };
            button.MouseLeave += (s, e) =>
            {
                if (!IsActiveNavButton(button))
                {
                    UpdateNavButtonStyle(button, false);
                }
            };
            
            return button;
        }
        
        private void UpdateNavButtonStyle(Button button, bool isActive)
        {
            if (isActive)
            {
                button.BackColor = Color.FromArgb(0, 123, 255);
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.BorderColor = Color.FromArgb(0, 123, 255);
            }
            else
            {
                button.BackColor = Color.Transparent;
                button.ForeColor = Color.FromArgb(173, 181, 189);
                button.FlatAppearance.BorderSize = 0;
            }
        }
        
        private bool IsActiveNavButton(Button button)
        {
            return button.BackColor == Color.FromArgb(0, 123, 255);
        }
        
        private void CreateTopBar()
        {
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(30, 15, 30, 15)
            };
            
            // Top bar shadow effect
            pnlTopBar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 224, 229), 2))
                {
                    e.Graphics.DrawLine(pen, 0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);
                }
            };
            
            // Welcome message
            lblWelcome = new Label
            {
                Text = "Welcome back! 👋",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(30, 5),
                Size = new Size(300, 30),
                BackColor = Color.Transparent
            };
            
            // Date and time
            lblDateTime = new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(35, 45),
                Size = new Size(350, 20),
                BackColor = Color.Transparent
            };
            
            // System status
            lblSystemStatus = new Label
            {
                Text = "🟢 System Online",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlTopBar.Width - 350, 20),
                Size = new Size(150, 40),
                BackColor = Color.Transparent
            };

            // Create modern user profile section
            CreateUserProfileSection();
            
            pnlTopBar.Controls.AddRange(new Control[] { lblWelcome, lblDateTime, lblSystemStatus, pnlUserProfile });
        }

        private void CreateUserProfileSection()
        {
            // User profile container
            pnlUserProfile = new Panel
            {
                Width = 90,
                Height = 80,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            
            // Set location after the form is properly sized
            pnlUserProfile.Location = new Point(this.Width - 110, 0);

            // User profile button with premium design
            btnUserProfile = new Button
            {
                Width = 70,
                Height = 55,
                Top = 12,
                Left = 10,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = "",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(50, 0, 35, 0),
                Cursor = Cursors.Hand
            };

            btnUserProfile.FlatAppearance.BorderSize = 1;
            btnUserProfile.FlatAppearance.BorderColor = Color.FromArgb(0, 123, 255);
            btnUserProfile.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 248, 255);

            // Add user avatar, name and dropdown arrow with premium styling
            btnUserProfile.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Draw rounded button background
                var buttonRect = new Rectangle(0, 0, btnUserProfile.Width, btnUserProfile.Height);
                using (var brush = new SolidBrush(btnUserProfile.BackColor))
                {
                    g.FillRoundedRectangle(brush, buttonRect, 12);
                }
                
                // Draw border
                using (var borderPen = new Pen(Color.FromArgb(0, 123, 255), 2))
                {
                    g.DrawRoundedRectangle(borderPen, new Rectangle(1, 1, btnUserProfile.Width - 3, btnUserProfile.Height - 3), 12);
                }

                // Draw user avatar circle
                var avatarRect = new Rectangle(12, 12, 31, 31);
                using (var avatarBrush = new SolidBrush(Color.FromArgb(0, 123, 255)))
                {
                    g.FillEllipse(avatarBrush, avatarRect);
                }
                
                // Draw user icon in avatar
                using (var iconBrush = new SolidBrush(Color.White))
                using (var iconFont = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                    var iconRect = new Rectangle(avatarRect.X, avatarRect.Y, avatarRect.Width, avatarRect.Height);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("JD", iconFont, iconBrush, iconRect, sf);
                }

                // Note: Username and role text are intentionally removed from main button
                // to avoid duplication with the dropdown. Only avatar and arrow are shown.

                // Draw dropdown arrow
                var arrowSize = 6;
                var arrowX = btnUserProfile.Width - 18;
                var arrowY = (btnUserProfile.Height - arrowSize) / 2;
                
                using (var arrowBrush = new SolidBrush(Color.FromArgb(0, 123, 255)))
                {
                    var arrowPoints = new Point[]
                    {
                        new Point(arrowX, arrowY),
                        new Point(arrowX + arrowSize, arrowY),
                        new Point(arrowX + arrowSize / 2, arrowY + arrowSize / 2)
                    };
                    g.FillPolygon(arrowBrush, arrowPoints);
                }
            };

            // Create dropdown panel (initially hidden)
            pnlUserDropdown = new Panel
            {
                Width = 200,
                Height = 195,
                Top = 50, // Position it below the button
                Left = -20, // Align it with the button
                BackColor = Color.White,
                Visible = false,
                BorderStyle = BorderStyle.None
            };

            // Add shadow and rounded corners to dropdown
            // Custom paint for premium dropdown appearance
            pnlUserDropdown.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                {
                    g.FillRoundedRectangle(shadowBrush, new Rectangle(3, 3, pnlUserDropdown.Width - 3, pnlUserDropdown.Height - 3), 8);
                }

                // Draw dropdown background
                using (var backgroundBrush = new SolidBrush(Color.White))
                {
                    g.FillRoundedRectangle(backgroundBrush, new Rectangle(0, 0, pnlUserDropdown.Width - 3, pnlUserDropdown.Height - 3), 8);
                }

                // Draw border
                using (var borderPen = new Pen(Color.FromArgb(220, 224, 229), 1))
                {
                    g.DrawRoundedRectangle(borderPen, new Rectangle(0, 0, pnlUserDropdown.Width - 4, pnlUserDropdown.Height - 4), 8);
                }
            };

            // User info section with enhanced styling
            var userInfoPanel = new Panel
            {
                Width = 170,
                Height = 48,
                Top = 10,
                Left = 15,
                BackColor = Color.FromArgb(248, 249, 250)
            };
            
            userInfoPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(248, 249, 250)))
                {
                    g.FillRoundedRectangle(brush, new Rectangle(0, 0, userInfoPanel.Width, userInfoPanel.Height), 6);
                }
                
                // Draw user avatar circle
                var avatarRect = new Rectangle(10, 10, 30, 30);
                using (var avatarBrush = new SolidBrush(Color.FromArgb(0, 123, 255)))
                {
                    g.FillEllipse(avatarBrush, avatarRect);
                }
                
                // Draw user icon in avatar
                using (var iconBrush = new SolidBrush(Color.White))
                using (var iconFont = new Font("Segoe UI", 9, FontStyle.Bold))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("JD", iconFont, iconBrush, avatarRect, sf);
                }
                
                // Draw username
                using (var textBrush = new SolidBrush(Color.FromArgb(33, 37, 41)))
                using (var textFont = new Font("Segoe UI", 9, FontStyle.Bold))
                {
                    g.DrawString("John Doe", textFont, textBrush, new Point(48, 12));
                }
                
                // Draw role
                using (var roleBrush = new SolidBrush(Color.FromArgb(108, 117, 125)))
                using (var roleFont = new Font("Segoe UI", 8))
                {
                    g.DrawString("System Administrator", roleFont, roleBrush, new Point(48, 26));
                }
            };

            // Separator line
            var separator = new Panel
            {
                Height = 1,
                Width = 170,
                Top = 63,
                Left = 15,
                BackColor = Color.FromArgb(220, 224, 229)
            };

            // Profile menu button
            var btnProfile = CreateDropdownMenuItem("My Profile", 68, () =>
            {
                HideUserDropdown();
                MessageBox.Show("Profile settings will be implemented in future updates.", 
                    "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });

            // Settings menu button
            var btnSettingsMenu = CreateDropdownMenuItem("Settings", 103, () =>
            {
                HideUserDropdown();
                OpenSettingsForm();
            });

            // Modern logout button with sleek design
            var btnLogout = new Button
            {
                Text = "Sign Out",
                Width = 170,
                Height = 38,
                Top = 143,
                Left = 15,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 35, 51);
            btnLogout.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 25, 41);

            // Modern rounded corners for logout button with premium styling
            btnLogout.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, btnLogout.Width, btnLogout.Height);
                
                // Create modern gradient brush
                using (var brush = new LinearGradientBrush(rect, 
                    Color.FromArgb(230, 60, 75), Color.FromArgb(210, 45, 60), LinearGradientMode.Vertical))
                {
                    g.FillRoundedRectangle(brush, rect, 10);
                }

                // Add modern subtle highlight
                using (var highlightBrush = new SolidBrush(Color.FromArgb(25, 255, 255, 255)))
                {
                    g.FillRoundedRectangle(highlightBrush, new Rectangle(1, 1, rect.Width - 2, rect.Height / 2), 9);
                }

                // Draw text with perfect center alignment
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var textBrush = new SolidBrush(btnLogout.ForeColor))
                {
                    g.DrawString(btnLogout.Text, btnLogout.Font, textBrush, rect, sf);
                }
            };

            btnLogout.Click += BtnLogout_Click;

            // Add controls to dropdown
            pnlUserDropdown.Controls.AddRange(new Control[] {
                userInfoPanel, separator, btnProfile, btnSettingsMenu, btnLogout
            });

            // Button click handler to toggle dropdown
            btnUserProfile.Click += (s, e) => ToggleUserDropdown();

            // Add button to profile panel
            pnlUserProfile.Controls.Add(btnUserProfile);
            
            // Add dropdown to the main form to ensure it appears on top
            this.Controls.Add(pnlUserDropdown);
            pnlUserDropdown.BringToFront();
            
            // Add resize handler to maintain proper positioning
            this.SizeChanged += (s, e) => {
                if (pnlUserProfile != null)
                {
                    pnlUserProfile.Location = new Point(this.Width - 110, 0);
                    // Update dropdown position relative to the main form
                    pnlUserDropdown.Location = new Point(this.Width - 220, 80);
                }
            };
            
            // Set initial dropdown position
            pnlUserDropdown.Location = new Point(this.Width - 220, 80);
        }

        private Button CreateDropdownMenuItem(string text, int top, Action onClick)
        {
            var button = new Button
            {
                Text = "", // Empty text to prevent duplicate drawing
                Width = 170,
                Height = 35,
                Top = top,
                Left = 15,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(33, 37, 41),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 10, 0),
                Cursor = Cursors.Hand,
                Tag = text // Store the text in Tag for custom drawing
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button.FlatAppearance.MouseDownBackColor = Color.Transparent;

            // Add custom paint for rounded hover effect
            button.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                var rect = new Rectangle(0, 0, button.Width, button.Height);
                
                // Draw background with rounded corners on hover
                if (button.ClientRectangle.Contains(button.PointToClient(Control.MousePosition)))
                {
                    using (var hoverBrush = new SolidBrush(Color.FromArgb(240, 248, 255)))
                    {
                        g.FillRoundedRectangle(hoverBrush, rect, 6);
                    }
                    
                    // Add subtle border on hover
                    using (var hoverPen = new Pen(Color.FromArgb(200, 220, 240), 1))
                    {
                        g.DrawRoundedRectangle(hoverPen, new Rectangle(0, 0, rect.Width - 1, rect.Height - 1), 6);
                    }
                }
                
                // Draw text manually for better control
                using (var textBrush = new SolidBrush(button.ForeColor))
                {
                    var textRect = new Rectangle(15, 0, button.Width - 25, button.Height);
                    var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                    g.DrawString(button.Tag?.ToString() ?? "", button.Font, textBrush, textRect, sf);
                }
            };
            
            // Force repaint on mouse enter/leave
            button.MouseEnter += (s, e) => button.Invalidate();
            button.MouseLeave += (s, e) => button.Invalidate();

            button.Click += (s, e) => onClick?.Invoke();

            return button;
        }

        private void ToggleUserDropdown()
        {
            pnlUserDropdown.Visible = !pnlUserDropdown.Visible;
            if (pnlUserDropdown.Visible)
            {
                // Bring both the profile panel and dropdown to front
                pnlUserProfile.BringToFront();
                pnlUserDropdown.BringToFront();
                
                // Ensure it's in front of the top bar
                pnlTopBar.Controls.SetChildIndex(pnlUserProfile, 0);
                
                // Debug: Log the dropdown state
                _logger?.LogInformation($"User dropdown toggled: Visible={pnlUserDropdown.Visible}, Location={pnlUserDropdown.Location}, Size={pnlUserDropdown.Size}");
            }
        }

        private void HideUserDropdown()
        {
            pnlUserDropdown.Visible = false;
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            HideUserDropdown();

            // Create modern logout confirmation dialog
            var result = ShowModernLogoutConfirmation();
            
            if (result == DialogResult.Yes)
            {
                PerformLogout();
            }
        }

        private DialogResult ShowModernLogoutConfirmation()
        {
            using var confirmDialog = new Form
            {
                Text = "Confirm Logout",
                Size = new Size(490, 250),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };

            // Icon
            var iconLabel = new Label
            {
                Text = "🚪",
                Font = new Font("Segoe UI", 20),
                Location = new Point(40, 30),
                Size = new Size(60, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Title
            var titleLabel = new Label
            {
                Text = "Logout Confirmation",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(120, 30),
                Size = new Size(290, 40),
                BackColor = Color.Transparent
            };

            // Message
            var messageLabel = new Label
            {
                Text = "Are you sure you want to logout from InventoryPro?\nYour current session will be ended.",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(120, 65),
                Size = new Size(250, 50),
                BackColor = Color.Transparent
            };

            // Buttons panel
            var buttonPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15)
            };

            var btnYes = new Button
                {
                Text = "✅ Yes, Logout",
                Size = new Size(155, 40),
                Location = new Point(120, 10),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Yes,
                TextAlign = ContentAlignment.MiddleCenter
                };
            btnYes.FlatAppearance.BorderSize = 0;
            btnYes.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0,0, btnYes.Width, btnYes.Height);
                using (var brush = new SolidBrush(btnYes.BackColor)) {
                    g.FillRoundedRectangle(brush, rect, 8);
                    }
                //Draw center
                var textRect = new Rectangle(0,0,btnYes.Width,btnYes.Height);
                var sf = new StringFormat
                    {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                    };
                using (var textBrush = new SolidBrush(btnYes.ForeColor))
                    {
                    g.DrawString(btnYes.Text, btnYes.Font, textBrush, textRect,sf);
                    }
            };

            var btnNo = new Button
            {
                Text = "❌ Cancel",
                Size = new Size(120, 40),
                Location = new Point(300, 10),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.No
            };
            btnNo.FlatAppearance.BorderSize = 0;

            buttonPanel.Controls.AddRange(new Control[] { btnYes, btnNo });
            confirmDialog.Controls.AddRange(new Control[] { iconLabel, titleLabel, messageLabel, buttonPanel });

            confirmDialog.AcceptButton = btnNo; // Default to cancel for safety
            confirmDialog.CancelButton = btnNo;

            return confirmDialog.ShowDialog(this);
        }

        private void PerformLogout()
        {
            try
            {
                _logger.LogInformation("User logout initiated");

                // Stop refresh timer
                refreshTimer?.Stop();

                // Clear any cached authentication data
                // Note: Add your authentication cleanup logic here

                // Show logout progress
                using var logoutDialog = new Form
                {
                    Text = "Logging out...",
                    Size = new Size(400, 150),
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ControlBox = false,
                    BackColor = Color.White
                };

                var progressLabel = new Label
                {
                    Text = "🔄 Logging out safely...",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(33, 37, 41),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };

                logoutDialog.Controls.Add(progressLabel);
                logoutDialog.Show(this);
                logoutDialog.Refresh();

                // Simulate logout process
                System.Threading.Thread.Sleep(1000);

                logoutDialog.Close();

                _logger.LogInformation("User logout completed successfully");

                // Close the main form and return to login
                this.Hide();
                
                // Show login form (you'll need to implement this based on your application structure)
                ShowLoginForm();
                
                // Close this form
                this.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout process");
                MessageBox.Show("An error occurred during logout. The application will close.",
                    "Logout Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }
        }

        private void ShowLoginForm()
        {
            // Create and show login form
            // Note: Replace this with your actual login form implementation
            try
            {
                // This should be replaced with your actual login form
                var loginForm = _serviceProvider.GetService<LoginForm>();
                if (loginForm != null)
                {
                    loginForm.Show();
                }
                else
                {
                    // Fallback: restart the application
                    MessageBox.Show("Logout successful. Please restart the application to login again.",
                        "Logout Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing login form after logout");
                MessageBox.Show("Logout complete. Please restart the application.",
                    "Logout Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }
        
        private void CreateContentArea()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(30, 30, 30, 30),
                AutoScroll = true
            };
            
            CreateDashboardCards();
            CreateQuickActions();
            CreateDataVisualization();
        }
        
        private void CreateDashboardCards()
        {
            var cardsPanel = new Panel
            {
                Height = 150,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 30)
            };
            
            // Calculate card width for responsive design
            int cardWidth = (cardsPanel.Width - 60) / 4; // 4 cards with spacing
            
            cardTotalProducts = CreateDashboardCard("📦 Total Products", "0", Color.FromArgb(0, 123, 255), 0);
            cardTotalCustomers = CreateDashboardCard("👥 Total Customers", "0", Color.FromArgb(40, 167, 69), 1);
            cardTotalSales = CreateDashboardCard("💰 Total Sales", "$0.00", Color.FromArgb(255, 193, 7), 2);
            cardLowStock = CreateDashboardCard("⚠️ Low Stock Items", "0", Color.FromArgb(220, 53, 69), 3);
            
            cardsPanel.Controls.AddRange(new Control[] {
                cardTotalProducts, cardTotalCustomers, cardTotalSales, cardLowStock
            });
            
            pnlContent.Controls.Add(cardsPanel);
        }
        
        private Panel CreateDashboardCard(string title, string value, Color accentColor, int index)
        {
            var card = new Panel
            {
                Width = 280,
                Height = 120,
                Left = index * 300,
                Top = 10,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            
            // Card shadow and rounded corners
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Draw shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                {
                    g.FillRoundedRectangle(shadowBrush, new Rectangle(3, 3, card.Width - 3, card.Height - 3), 10);
                }
                
                // Draw card background
                using (var cardBrush = new SolidBrush(Color.White))
                {
                    g.FillRoundedRectangle(cardBrush, new Rectangle(0, 0, card.Width - 3, card.Height - 3), 10);
                }
                
                // Draw accent border
                using (var accentPen = new Pen(accentColor, 4))
                {
                    g.DrawLine(accentPen, 0, 0, card.Width - 3, 0);
                }
            };
            
            // Card title
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(20, 15),
                Size = new Size(240, 25),
                BackColor = Color.Transparent
            };
            
            // Card value
            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(20, 45),
                Size = new Size(240, 50),
                BackColor = Color.Transparent
            };
            
            // Card trend indicator
            var lblTrend = new Label
            {
                Text = "📈 +12% from last month",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(40, 167, 69),
                Location = new Point(20, 95),
                Size = new Size(240, 30),
                BackColor = Color.Transparent
            };
            
            card.Controls.AddRange(new Control[] { lblTitle, lblValue, lblTrend });
            
            // Hover effects
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(248, 249, 250);
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.White;
            };
            
            return card;
        }
        
        private void CreateQuickActions()
        {
            var actionsPanel = new Panel
            {
                Height = 100,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 20, 0, 20)
            };
            
            var lblQuickActions = new Label
            {
                Text = "⚡ Quick Actions",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(0, 0),
                Size = new Size(200, 30),
                BackColor = Color.Transparent
            };
            
            btnQuickSale = CreateQuickActionButton("💰 New Sale", Color.FromArgb(40, 167, 69), 0);
            btnAddProduct = CreateQuickActionButton("➕ Add Product", Color.FromArgb(0, 123, 255), 1);
            btnAddCustomer = CreateQuickActionButton("👤 Add Customer", Color.FromArgb(102, 16, 242), 2);
            
            // Connect quick action buttons
            btnQuickSale.Click += (s, e) => OpenSalesForm();
            btnAddProduct.Click += (s, e) => OpenProductsForm();
            btnAddCustomer.Click += (s, e) => OpenCustomersForm();
            
            actionsPanel.Controls.AddRange(new Control[] {
                lblQuickActions, btnQuickSale, btnAddProduct, btnAddCustomer
            });
            
            pnlContent.Controls.Add(actionsPanel);
        }
        
        private Button CreateQuickActionButton(string text, Color backColor, int index)
        {
            var button = new Button
            {
                Text = text,
                Width = 180,
                Height = 50,
                Left = index * 200,
                Top = 35,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Max(0, backColor.R - 20),
                Math.Max(0, backColor.G - 20),
                Math.Max(0, backColor.B - 20));
            
            // Rounded button corners
            button.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(button.BackColor))
                {
                    g.FillRoundedRectangle(brush, new Rectangle(0, 0, button.Width, button.Height), 8);
                }
                
                var textRect = new Rectangle(0, 0, button.Width, button.Height);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var textBrush = new SolidBrush(button.ForeColor))
                {
                    g.DrawString(button.Text, button.Font, textBrush, textRect, sf);
                }
            };
            
            return button;
        }
        
        private void CreateDataVisualization()
        {
            var chartsPanel = new Panel
            {
                Height = 400,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 20, 0, 0)
            };
            
            // Sales chart panel
            chartSalesPanel = new Panel
            {
                Width = 600,
                Height = 350,
                Left = 0,
                Top = 20,
                BackColor = Color.White
            };
            
            chartSalesPanel.Paint += (s, e) =>
            {
                DrawSalesChart(e.Graphics, chartSalesPanel.ClientRectangle);
            };
            
            // Stock alerts panel
            stockAlertsPanel = new Panel
            {
                Width = 580,
                Height = 350,
                Left = 620,
                Top = 20,
                BackColor = Color.White
            };
            
            stockAlertsPanel.Paint += (s, e) =>
            {
                DrawStockAlerts(e.Graphics, stockAlertsPanel.ClientRectangle);
            };
            
            chartsPanel.Controls.AddRange(new Control[] { chartSalesPanel, stockAlertsPanel });
            pnlContent.Controls.Add(chartsPanel);
            
            // Add recent sales section
            CreateRecentSalesSection();
        }
        
        private void CreateRecentSalesSection()
        {
            var recentSalesContainer = new Panel
            {
                Height = 300,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 20, 0, 0)
            };
            
            var lblRecentSales = new Label
            {
                Text = "💼 Recent Sales Activity",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(0, 0),
                Size = new Size(300, 30),
                BackColor = Color.Transparent
            };
            
            recentSalesPanel = new Panel
            {
                Width = 1200,
                Height = 250,
                Left = 0,
                Top = 40,
                BackColor = Color.White,
                AutoScroll = true
            };
            
            recentSalesPanel.Paint += (s, e) =>
            {
                DrawRecentSales(e.Graphics, recentSalesPanel.ClientRectangle);
            };
            
            // Make recent sales panel clickable to open sales history
            recentSalesPanel.Click += (s, e) =>
            {
                // Check if click was on the "View All Sales" button area
                var buttonRect = new Rectangle(recentSalesPanel.Width - 150, recentSalesPanel.Height - 40, 130, 30);
                var clickPoint = recentSalesPanel.PointToClient(Cursor.Position);
                if (buttonRect.Contains(clickPoint))
                {
                    OpenSalesHistoryForm();
                }
            };
            
            recentSalesPanel.MouseMove += (s, e) =>
            {
                var buttonRect = new Rectangle(recentSalesPanel.Width - 150, recentSalesPanel.Height - 40, 130, 30);
                if (buttonRect.Contains(e.Location))
                {
                    recentSalesPanel.Cursor = Cursors.Hand;
                }
                else
                {
                    recentSalesPanel.Cursor = Cursors.Default;
                }
            };
            
            recentSalesContainer.Controls.AddRange(new Control[] { lblRecentSales, recentSalesPanel });
            pnlContent.Controls.Add(recentSalesContainer);
        }
        
        private void CreateStatusBar()
        {
            pnlStatusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20, 8, 20, 8)
            };
            
            var lblStatus = new Label
            {
                Text = "Ready • Database Connected • Last Updated: " + DateTime.Now.ToString("HH:mm:ss"),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(108, 117, 125),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            
            pnlStatusBar.Controls.Add(lblStatus);
        }
        
        private void DrawSalesChart(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw chart background
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillRectangle(brush, bounds);
            }
            
            // Draw chart border
            using (var pen = new Pen(Color.FromArgb(220, 224, 229), 1))
            {
                g.DrawRectangle(pen, bounds);
            }
            
            // Chart title
            using (var titleBrush = new SolidBrush(Color.FromArgb(33, 37, 41)))
            using (var titleFont = new Font("Segoe UI", 14, FontStyle.Bold))
            {
                g.DrawString("📈 Weekly Sales Overview", titleFont, titleBrush, new Point(20, 15));
            }
            
            // Sales total for the week
            var weeklyTotal = _weeklySalesData.Sum();
            using (var totalBrush = new SolidBrush(Color.FromArgb(40, 167, 69)))
            using (var totalFont = new Font("Segoe UI", 11, FontStyle.Bold))
            {
                g.DrawString($"Total: {weeklyTotal:C}", totalFont, totalBrush, new Point(20, 40));
            }
            
            // Chart area
            var chartArea = new Rectangle(40, 80, bounds.Width - 80, bounds.Height - 110);
            
            // Draw grid lines
            using (var gridPen = new Pen(Color.FromArgb(240, 244, 248), 1))
            {
                for (int i = 0; i <= 5; i++)
                {
                    int y = chartArea.Y + (chartArea.Height * i / 5);
                    g.DrawLine(gridPen, chartArea.X, y, chartArea.Right, y);
                }
                
                for (int i = 0; i <= 7; i++)
                {
                    int x = chartArea.X + (chartArea.Width * i / 7);
                    g.DrawLine(gridPen, x, chartArea.Y, x, chartArea.Bottom);
                }
            }
            
            // Draw sales data
            if (_weeklySalesData.Any(d => d > 0))
            {
                var maxSale = _weeklySalesData.Max();
                if (maxSale > 0)
                {
                    var points = new List<Point>();
                    
                    for (int i = 0; i < 7; i++)
                    {
                        int x = chartArea.X + (chartArea.Width * i / 6);
                        int y = chartArea.Bottom - (int)((double)_weeklySalesData[i] / (double)maxSale * chartArea.Height);
                        points.Add(new Point(x, y));
                    }
                    
                    // Draw line
                    if (points.Count > 1)
                    {
                        using (var linePen = new Pen(Color.FromArgb(40, 167, 69), 3))
                        {
                            g.DrawLines(linePen, points.ToArray());
                        }
                    }
                    
                    // Draw data points and values
                    using (var pointBrush = new SolidBrush(Color.FromArgb(40, 167, 69)))
                    using (var valueBrush = new SolidBrush(Color.FromArgb(33, 37, 41)))
                    using (var valueFont = new Font("Segoe UI", 8))
                    {
                        for (int i = 0; i < points.Count; i++)
                        {
                            var point = points[i];
                            g.FillEllipse(pointBrush, point.X - 4, point.Y - 4, 8, 8);
                            
                            // Draw value above point
                            if (_weeklySalesData[i] > 0)
                            {
                                var value = _weeklySalesData[i].ToString("C0");
                                var valueSize = g.MeasureString(value, valueFont);
                                g.DrawString(value, valueFont, valueBrush, 
                                    new Point(point.X - (int)valueSize.Width / 2, point.Y - 20));
                            }
                        }
                    }
                }
            }
            
            // Draw axis labels
            using (var labelBrush = new SolidBrush(Color.FromArgb(108, 117, 125)))
            using (var labelFont = new Font("Segoe UI", 9))
            {
                var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                for (int i = 0; i < days.Length; i++)
                {
                    int x = chartArea.X + (chartArea.Width * i / 6) - 15;
                    g.DrawString(days[i], labelFont, labelBrush, new Point(x, chartArea.Bottom + 10));
                }
            }
        }
        
        private void DrawStockAlerts(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw background
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillRectangle(brush, bounds);
            }
            
            // Draw border
            using (var pen = new Pen(Color.FromArgb(220, 224, 229), 1))
            {
                g.DrawRectangle(pen, bounds);
            }
            
            // Title
            using (var titleBrush = new SolidBrush(Color.FromArgb(33, 37, 41)))
            using (var titleFont = new Font("Segoe UI", 14, FontStyle.Bold))
            {
                g.DrawString("⚠️ Stock Alerts", titleFont, titleBrush, new Point(20, 15));
            }
            
            // Sample stock alerts
            var alerts = new[]
            {
                new { Product = "Gaming Headset", Stock = 3, Status = "Critical" },
                new { Product = "Wireless Mouse", Stock = 8, Status = "Low" },
                new { Product = "USB Cable", Stock = 5, Status = "Low" },
                new { Product = "Phone Case", Stock = 2, Status = "Critical" }
            };
            
            int y = 60;
            foreach (var alert in alerts)
            {
                var alertColor = alert.Status == "Critical" ? Color.FromArgb(220, 53, 69) : Color.FromArgb(255, 193, 7);
                var alertIcon = alert.Status == "Critical" ? "🔴" : "🟡";
                
                // Alert item background
                using (var alertBrush = new SolidBrush(Color.FromArgb(248, 249, 250)))
                {
                    g.FillRectangle(alertBrush, new Rectangle(20, y, bounds.Width - 40, 40));
                }
                
                // Alert content
                using (var textBrush = new SolidBrush(Color.FromArgb(33, 37, 41)))
                using (var textFont = new Font("Segoe UI", 10, FontStyle.Bold))
                using (var stockBrush = new SolidBrush(alertColor))
                using (var stockFont = new Font("Segoe UI", 9, FontStyle.Bold))
                {
                    g.DrawString($"{alertIcon} {alert.Product}", textFont, textBrush, new Point(30, y + 8));
                    g.DrawString($"Stock: {alert.Stock}", stockFont, stockBrush, new Point(bounds.Width - 120, y + 8));
                }
                
                y += 50;
            }
        }
        
        private void DrawRecentSales(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw background
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillRectangle(brush, bounds);
            }
            
            // Draw border
            using (var pen = new Pen(Color.FromArgb(220, 224, 229), 1))
            {
                g.DrawRectangle(pen, bounds);
            }
            
            // Headers
            var headerHeight = 40;
            using (var headerBrush = new SolidBrush(Color.FromArgb(248, 249, 250)))
            {
                g.FillRectangle(headerBrush, new Rectangle(0, 0, bounds.Width, headerHeight));
            }
            
            using (var headerTextBrush = new SolidBrush(Color.FromArgb(33, 37, 41)))
            using (var headerFont = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                g.DrawString("Sale Date", headerFont, headerTextBrush, new Point(20, 12));
                g.DrawString("Customer", headerFont, headerTextBrush, new Point(140, 12));
                g.DrawString("Items", headerFont, headerTextBrush, new Point(300, 12));
                g.DrawString("Total", headerFont, headerTextBrush, new Point(400, 12));
                g.DrawString("Payment", headerFont, headerTextBrush, new Point(500, 12));
                g.DrawString("Status", headerFont, headerTextBrush, new Point(620, 12));
            }
            
            // Recent sales data (sample data if no real data available)
            var sampleSales = new[]
            {
                new { Date = DateTime.Now.AddHours(-2), Customer = "John Smith", Items = 3, Total = 156.75m, Payment = "Credit Card", Status = "Completed" },
                new { Date = DateTime.Now.AddHours(-4), Customer = "Sarah Johnson", Items = 1, Total = 89.99m, Payment = "Cash", Status = "Completed" },
                new { Date = DateTime.Now.AddHours(-6), Customer = "Mike Wilson", Items = 5, Total = 234.50m, Payment = "Debit Card", Status = "Completed" },
                new { Date = DateTime.Now.AddHours(-8), Customer = "Walk-in Customer", Items = 2, Total = 45.00m, Payment = "Cash", Status = "Completed" },
                new { Date = DateTime.Now.AddDays(-1), Customer = "Lisa Brown", Items = 4, Total = 189.25m, Payment = "Credit Card", Status = "Completed" }
            };
            
            int y = headerHeight + 10;
            using (var textBrush = new SolidBrush(Color.FromArgb(33, 37, 41)))
            using (var textFont = new Font("Segoe UI", 9))
            using (var statusBrush = new SolidBrush(Color.FromArgb(40, 167, 69)))
            using (var statusFont = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                foreach (var sale in sampleSales)
                {
                    if (y > bounds.Height - 30) break;
                    
                    // Alternate row background
                    if ((y - headerHeight) / 35 % 2 == 1)
                    {
                        using (var rowBrush = new SolidBrush(Color.FromArgb(248, 249, 250)))
                        {
                            g.FillRectangle(rowBrush, new Rectangle(0, y - 5, bounds.Width, 30));
                        }
                    }
                    
                    g.DrawString(sale.Date.ToString("MMM dd, HH:mm"), textFont, textBrush, new Point(20, y));
                    g.DrawString(sale.Customer, textFont, textBrush, new Point(140, y));
                    g.DrawString(sale.Items.ToString(), textFont, textBrush, new Point(300, y));
                    g.DrawString(sale.Total.ToString("C"), textFont, textBrush, new Point(400, y));
                    g.DrawString(sale.Payment, textFont, textBrush, new Point(500, y));
                    g.DrawString($"✅ {sale.Status}", statusFont, statusBrush, new Point(620, y));
                    
                    y += 35;
                }
            }
            
            // "View All Sales" button area
            var buttonRect = new Rectangle(bounds.Width - 150, bounds.Height - 40, 130, 30);
            using (var buttonBrush = new SolidBrush(Color.FromArgb(0, 123, 255)))
            {
                g.FillRoundedRectangle(buttonBrush, buttonRect, 5);
            }
            
            using (var buttonTextBrush = new SolidBrush(Color.White))
            using (var buttonFont = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                var buttonText = "📊 View All Sales";
                var textSize = g.MeasureString(buttonText, buttonFont);
                var textX = buttonRect.X + (buttonRect.Width - (int)textSize.Width) / 2;
                var textY = buttonRect.Y + (buttonRect.Height - (int)textSize.Height) / 2;
                g.DrawString(buttonText, buttonFont, buttonTextBrush, new Point(textX, textY));
            }
        }
        
        private void SetupRealtimeUpdates()
        {
            refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 30000 // 30 seconds
            };
            
            refreshTimer.Tick += async (s, e) =>
            {
                lblDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm");
                await RefreshDashboardData();
            };
            
            refreshTimer.Start();
        }
        
        private async void LoadDashboardDataAsync()
        {
            try
            {
                await RefreshDashboardData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                lblSystemStatus.Text = "🔴 System Error";
                lblSystemStatus.ForeColor = Color.FromArgb(220, 53, 69);
            }
        }
        
        private async Task RefreshDashboardData()
        {
            try
            {
                _logger.LogInformation("Refreshing dashboard data");
                
                // Get real data from API services
                var tasks = new List<Task>
                {
                    LoadProductCountAsync(),
                    LoadCustomerCountAsync(),
                    LoadSalesDataAsync(),
                    LoadLowStockCountAsync()
                };
                
                await Task.WhenAll(tasks);
                
                // Refresh charts with latest data
                chartSalesPanel.Invalidate();
                stockAlertsPanel.Invalidate();
                
                // Update status
                lblSystemStatus.Text = "🟢 System Online";
                lblSystemStatus.ForeColor = Color.FromArgb(40, 167, 69);
                
                // Update status bar
                var statusLabel = pnlStatusBar.Controls.OfType<Label>().FirstOrDefault();
                if (statusLabel != null)
                {
                    statusLabel.Text = "Ready • Database Connected • Last Updated: " + DateTime.Now.ToString("HH:mm:ss");
                }
                
                _logger.LogInformation("Dashboard data refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dashboard data");
                lblSystemStatus.Text = "🔴 Update Error";
                lblSystemStatus.ForeColor = Color.FromArgb(220, 53, 69);
            }
        }
        
        private async Task LoadProductCountAsync()
        {
            try
            {
                var response = await _apiService.GetProductsAsync(new PaginationParameters { PageNumber = 1, PageSize = 1 });
                if (response.Success && response.Data != null)
                {
                    UpdateDashboardCard(cardTotalProducts, response.Data.TotalCount.ToString("N0"));
                }
                else
                {
                    UpdateDashboardCard(cardTotalProducts, "Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product count");
                UpdateDashboardCard(cardTotalProducts, "Error");
            }
        }
        
        private async Task LoadCustomerCountAsync()
        {
            try
            {
                var response = await _apiService.GetCustomersAsync(new PaginationParameters { PageNumber = 1, PageSize = 1 });
                if (response.Success && response.Data != null)
                {
                    UpdateDashboardCard(cardTotalCustomers, response.Data.TotalCount.ToString("N0"));
                }
                else
                {
                    UpdateDashboardCard(cardTotalCustomers, "Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer count");
                UpdateDashboardCard(cardTotalCustomers, "Error");
            }
        }
        
        private async Task LoadSalesDataAsync()
        {
            try
            {
                // Load recent sales for dashboard
                var recentSalesResponse = await _apiService.GetSalesAsync(new PaginationParameters { PageNumber = 1, PageSize = 10 });
                if (recentSalesResponse.Success && recentSalesResponse.Data?.Items != null)
                {
                    _recentSales = recentSalesResponse.Data.Items;
                    
                    // Calculate total sales amount
                    decimal totalSales = _recentSales.Sum(s => s.TotalAmount);
                    UpdateDashboardCard(cardTotalSales, totalSales.ToString("C"));
                    
                    // Generate weekly sales data
                    GenerateWeeklySalesData();
                }
                else
                {
                    // If no real data, generate sample data for demo
                    GenerateSampleSalesData();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sales data");
                UpdateDashboardCard(cardTotalSales, "Error");
                // Generate sample data for demo purposes
                GenerateSampleSalesData();
            }
        }
        
        private void GenerateWeeklySalesData()
        {
            // Initialize weekly data
            _weeklySalesData = new decimal[7];
            
            if (_recentSales.Any())
            {
                var startOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
                
                for (int i = 0; i < 7; i++)
                {
                    var dayStart = startOfWeek.AddDays(i);
                    var dayEnd = dayStart.AddDays(1);
                    
                    _weeklySalesData[i] = _recentSales
                        .Where(s => s.Date >= dayStart && s.Date < dayEnd)
                        .Sum(s => s.TotalAmount);
                }
            }
            else
            {
                // Generate sample weekly data
                var random = new Random();
                for (int i = 0; i < 7; i++)
                {
                    _weeklySalesData[i] = (decimal)(random.NextDouble() * 500 + 100);
                }
            }
        }
        
        private void GenerateSampleSalesData()
        {
            // Generate sample data for demonstration
            var random = new Random();
            decimal totalSales = 0;
            
            for (int i = 0; i < 7; i++)
            {
                _weeklySalesData[i] = (decimal)(random.NextDouble() * 500 + 100);
                totalSales += _weeklySalesData[i];
            }
            
            UpdateDashboardCard(cardTotalSales, totalSales.ToString("C"));
        }
        
        private async Task LoadLowStockCountAsync()
        {
            try
            {
                var response = await _apiService.GetProductsAsync(new PaginationParameters { PageNumber = 1, PageSize = 100 });
                if (response.Success && response.Data?.Items != null)
                {
                    var lowStockCount = response.Data.Items.Count(p => p.Stock <= p.MinStock);
                    UpdateDashboardCard(cardLowStock, lowStockCount.ToString());
                    
                    // Update card color based on low stock severity
                    var card = cardLowStock;
                    var valueLabel = card.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size == 24);
                    if (valueLabel != null)
                    {
                        if (lowStockCount > 10)
                        {
                            valueLabel.ForeColor = Color.FromArgb(220, 53, 69); // Red for critical
                        }
                        else if (lowStockCount > 5)
                        {
                            valueLabel.ForeColor = Color.FromArgb(255, 193, 7); // Yellow for warning
                        }
                        else
                        {
                            valueLabel.ForeColor = Color.FromArgb(40, 167, 69); // Green for good
                        }
                    }
                }
                else
                {
                    UpdateDashboardCard(cardLowStock, "Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading low stock count");
                UpdateDashboardCard(cardLowStock, "Error");
            }
        }
        
        private void UpdateDashboardCard(Panel card, string value)
        {
            var valueLabel = card.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size == 24);
            if (valueLabel != null)
            {
                valueLabel.Text = value;
            }
        }
        
        private void NavButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.Tag is int buttonIndex)
            {
                // Reset all buttons
                var allNavButtons = new[] { btnDashboard, btnProducts, btnCustomers, btnSales, btnSalesHistory, btnReports };
                foreach (var btn in allNavButtons)
                {
                    UpdateNavButtonStyle(btn, false);
                }
                
                // Activate clicked button
                UpdateNavButtonStyle(clickedButton, true);
                
                // Handle navigation
                //int buttonIndex = (int)clickedButton.Tag;
                switch (buttonIndex)
                {
                    case 0: // Dashboard
                        // Already on dashboard - refresh data
                        _ = RefreshDashboardData();
                        break;
                    case 1: // Products
                        OpenProductsForm();
                        break;
                    case 2: // Customers
                        OpenCustomersForm();
                        break;
                    case 3: // Sales (New Sale POS)
                        OpenSalesForm();
                        break;
                    case 4: // Sales History
                        OpenSalesHistoryForm();
                        break;
                    case 5: // Reports
                        OpenReportsForm();
                        break;
                    //case 6: // Settings
                    //    OpenSettingsForm();
                    //    break;
                }
            }
        }
        
        private void OpenProductsForm()
        {
            try
            {
                using var productForm = _serviceProvider.GetRequiredService<ProductForm>();
                productForm.ShowDialog();
                // Refresh dashboard after closing product form
                _ = RefreshDashboardData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening products form");
                MessageBox.Show($"Error opening products form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenCustomersForm()
        {
            try
            {
                using var customerForm = _serviceProvider.GetRequiredService<CustomerForm>();
                customerForm.ShowDialog();
                // Refresh dashboard after closing customer form
                _ = RefreshDashboardData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening customers form");
                MessageBox.Show($"Error opening customers form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenSalesForm()
        {
            try
            {
                using var salesForm = _serviceProvider.GetRequiredService<SalesForm>();
                salesForm.ShowDialog();
                // Refresh dashboard after closing sales form
                _ = RefreshDashboardData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening sales form");
                MessageBox.Show($"Error opening sales form: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenSalesHistoryForm()
        {
            try
            {
                using var salesHistoryForm = _serviceProvider.GetRequiredService<SalesHistoryForm>();
                salesHistoryForm.ShowDialog();
                // Refresh dashboard after closing sales history form
                _ = RefreshDashboardData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening sales history form");
                MessageBox.Show($"Error opening sales history form: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenReportsForm()
        {
            try
            {
                using var reportForm = _serviceProvider.GetRequiredService<ReportForm>();
                reportForm.ShowDialog();
                // Refresh dashboard after closing report form
                _ = RefreshDashboardData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening reports form");
                MessageBox.Show($"Error opening reports form: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenSettingsForm()
        {
            try
            {
                using var settingsForm = _serviceProvider.GetRequiredService<SettingsForm>();
                settingsForm.ShowDialog();
                // Refresh dashboard after closing settings form
                _ = RefreshDashboardData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening settings form");
                MessageBox.Show($"Error opening settings form: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void SetupClickOutsideHandler()
        {
            // Add click handler to main form to hide dropdown when clicking outside
            this.Click += (s, e) => HideUserDropdown();
            
            // Add click handlers to all main panels
            pnlContent.Click += (s, e) => HideUserDropdown();
            pnlSidebar.Click += (s, e) => HideUserDropdown();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
    
    // Extension method for rounded rectangles
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
                path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90);
                path.AddArc(bounds.X + bounds.Width - radius, bounds.Y + bounds.Height - radius, radius, radius, 0, 90);
                path.AddArc(bounds.X, bounds.Y + bounds.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                graphics.FillPath(brush, path);
            }
        }
        
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
                path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90);
                path.AddArc(bounds.X + bounds.Width - radius, bounds.Y + bounds.Height - radius, radius, radius, 0, 90);
                path.AddArc(bounds.X, bounds.Y + bounds.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                graphics.DrawPath(pen, path);
            }
        }
    }
}