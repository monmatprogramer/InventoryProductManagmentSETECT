using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Reflection;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using InventoryPro.Shared.DTOs;
using OfficeOpenXml;
using Polly;

// Extension methods for modern UI drawing
public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
    {
        if (graphics == null) throw new ArgumentNullException(nameof(graphics));
        if (brush == null) throw new ArgumentNullException(nameof(brush));
        
        using (GraphicsPath path = CreateRoundedRectanglePath(bounds, cornerRadius))
        {
            graphics.FillPath(brush, path);
        }
    }
    
    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
    {
        if (graphics == null) throw new ArgumentNullException(nameof(graphics));
        if (pen == null) throw new ArgumentNullException(nameof(pen));
        
        using (GraphicsPath path = CreateRoundedRectanglePath(bounds, cornerRadius))
        {
            graphics.DrawPath(pen, path);
        }
    }
    
    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int cornerRadius)
    {
        int diameter = cornerRadius * 2;
        Size size = new Size(diameter, diameter);
        Rectangle arc = new Rectangle(bounds.Location, size);
        GraphicsPath path = new GraphicsPath();
        
        if (cornerRadius == 0)
        {
            path.AddRectangle(bounds);
            return path;
        }
        
        // top left arc
        path.AddArc(arc, 180, 90);
        
        // top right arc
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        
        // bottom right arc
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        
        // bottom left arc
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        
        path.CloseFigure();
        return path;
    }
}

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
        private Button btnAbout;
        
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
        private Button btnRefreshDashboard;
        
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
        
        // Low stock data
        private List<ProductDto> _lowStockItems = new();
        
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
            btnAbout = new Button();
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
            btnRefreshDashboard = new Button();
            chartSalesPanel = new Panel();
            chartProductsPanel = new Panel();
            stockAlertsPanel = new Panel();
            recentSalesPanel = new Panel();
            refreshTimer = new System.Windows.Forms.Timer();



            InitializeComponent();
            SetupRealtimeUpdates();
            LoadDashboardDataAsync();
            SetupClickOutsideHandler();
            
            // Ensure data is loaded when form is shown
            this.Shown += MainForm_Shown;
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties - Ultra-Modern Premium Design
            this.Text = "🚀 InventoryPro v2.0 - Professional Inventory Management System";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1400, 800);
            this.BackColor = Color.FromArgb(248, 250, 252); // Modern light gray background
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular); // Premium font family
            this.Icon = SystemIcons.Application;
            this.FormBorderStyle = FormBorderStyle.None; // Borderless modern design
            
            // Enable double buffering for smooth animations and scrolling
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint | 
                         ControlStyles.DoubleBuffer | 
                         ControlStyles.ResizeRedraw | 
                         ControlStyles.OptimizedDoubleBuffer, true);
            
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
                Width = 300, // Slightly wider for better spacing
                BackColor = Color.FromArgb(30, 33, 38), // Darker, more premium sidebar
                Padding = new Padding(0, 25, 0, 25) // Increased padding for better spacing
            };
            
            // Sidebar gradient background
            pnlSidebar.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, pnlSidebar.Width, pnlSidebar.Height),
                    Color.FromArgb(30, 33, 38), // Modern dark gradient start
                    Color.FromArgb(45, 50, 57), // Modern dark gradient end
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, new Rectangle(0, 0, pnlSidebar.Width, pnlSidebar.Height));
                }
            };
            
            // Logo section
            var logoPanel = new Panel
            {
                Height = 110, // Increased height for better proportions
                Dock = DockStyle.Top,
                BackColor = Color.Transparent
            };
            
            var lblLogo = new Label
            {
                Text = "🚀 InventoryPro",
                Font = new Font("Segoe UI", 20, FontStyle.Bold), // Larger, more modern font
                ForeColor = Color.FromArgb(255, 255, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            logoPanel.Controls.Add(lblLogo);
            
            var lblVersion = new Label
            {
                Text = "v2.0 • Professional Edition",
                Font = new Font("Segoe UI", 10, FontStyle.Regular), // Modern subtle styling
                ForeColor = Color.FromArgb(190, 197, 208), // Softer version text
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
            
            btnDashboard = CreateNavButton("🏠  Dashboard", 0, true);
            btnProducts = CreateNavButton("📦  Products", 1, false);
            btnCustomers = CreateNavButton("👥  Customers", 2, false);
            btnSales = CreateNavButton("💰  New Sale", 3, false);
            btnSalesHistory = CreateNavButton("📈  Sales History", 4, false);
            btnReports = CreateNavButton("📊  Reports", 5, false);
            btnAbout = CreateNavButton("ℹ️  About", 6, false);
            
            navPanel.Controls.AddRange(new Control[] {
                btnDashboard, btnProducts, btnCustomers, btnSales, btnSalesHistory, btnReports, btnAbout
            });
            
            pnlSidebar.Controls.Add(navPanel);
            pnlSidebar.Controls.Add(logoPanel);
        }
        
        private Button CreateNavButton(string text, int index, bool isActive)
        {
            var button = new Button
            {
                Text = text,
                Height = 60, // Increased height for better touch targets
                Top = index * 70, // Increased spacing between buttons
                Left = 0,
                Width = 260, // Increased width to match new sidebar width
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), // Modern font with medium weight
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(25, 0, 0, 0), // Increased left padding
                Cursor = Cursors.Hand,
                Tag = index
            };
            
            UpdateNavButtonStyle(button, isActive);
            
            button.Click += NavButton_Click;
            button.MouseEnter += (s, e) =>
            {
                if (!IsActiveNavButton(button))
                {
                    button.BackColor = Color.FromArgb(65, 72, 80); // Modern hover state
                    button.ForeColor = Color.FromArgb(255, 255, 255);
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
                button.BackColor = Color.FromArgb(16, 185, 129); // Modern teal/green accent
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.BorderColor = Color.FromArgb(16, 185, 129);
            }
            else
            {
                button.BackColor = Color.Transparent;
                button.ForeColor = Color.FromArgb(190, 197, 208); // Softer inactive text
                button.FlatAppearance.BorderSize = 0;
            }
        }
        
        private bool IsActiveNavButton(Button button)
        {
            return button.BackColor == Color.FromArgb(16, 185, 129);
        }
        
        private void CreateTopBar()
        {
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90, // Increased height for better proportions
                BackColor = Color.FromArgb(255, 255, 255), // Pure white for modern look
                Padding = new Padding(35, 20, 35, 20) // Increased padding
            };
            
            // Top bar shadow effect
            pnlTopBar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 224, 229), 2))
                {
                    e.Graphics.DrawLine(pen, 0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);
                }
            };
            
            // Welcome message with modern styling
            lblWelcome = new Label
            {
                Text = "Welcome back! 👋",
                Font = new Font("Segoe UI", 18, FontStyle.Bold), // Larger, modern font
                ForeColor = Color.FromArgb(17, 24, 39), // Darker, modern text color
                Location = new Point(35, 8),
                Size = new Size(350, 35),
                BackColor = Color.Transparent
            };
            
            // Date and time with enhanced styling
            lblDateTime = new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy • HH:mm"),
                Font = new Font("Segoe UI", 11, FontStyle.Regular), // Slightly larger
                ForeColor = Color.FromArgb(75, 85, 99), // Modern muted color
                Location = new Point(40, 50),
                Size = new Size(400, 25),
                BackColor = Color.Transparent
            };
            
            // System status with modern indicator
            lblSystemStatus = new Label
            {
                Text = "✓ System Online", // Check mark instead of green circle
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129), // Matching brand color
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlTopBar.Width - 380, 25),
                Size = new Size(180, 45),
                BackColor = Color.Transparent
            };

            // Create modern user profile section
            CreateUserProfileSection();
            
            pnlTopBar.Controls.AddRange(new Control[] { lblWelcome, lblDateTime, lblSystemStatus, pnlUserProfile });
        }

        private void CreateUserProfileSection()
        {
            // User profile container with modern dimensions
            pnlUserProfile = new Panel
            {
                Width = 100, // Slightly wider
                Height = 90, // Increased height to match new top bar
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            
            // Set location after the form is properly sized
            pnlUserProfile.Location = new Point(this.Width - 120, 0);

            // User profile button with ultra-modern design
            btnUserProfile = new Button
            {
                Width = 80, // Increased width
                Height = 60, // Increased height
                Top = 15,
                Left = 10,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(249, 250, 251), // Very light background
                ForeColor = Color.FromArgb(17, 24, 39),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = "",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(55, 0, 40, 0),
                Cursor = Cursors.Hand
            };

            btnUserProfile.FlatAppearance.BorderSize = 2;
            btnUserProfile.FlatAppearance.BorderColor = Color.FromArgb(16, 185, 129); // Brand color border
            btnUserProfile.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 253, 250); // Subtle hover

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
                
                // Draw modern border with brand color
                using (var borderPen = new Pen(Color.FromArgb(16, 185, 129), 2))
                {
                    g.DrawRoundedRectangle(borderPen, new Rectangle(1, 1, btnUserProfile.Width - 3, btnUserProfile.Height - 3), 15); // More rounded corners
                }

                // Draw modern user avatar circle
                var avatarRect = new Rectangle(15, 15, 35, 35); // Larger avatar
                using (var avatarBrush = new SolidBrush(Color.FromArgb(16, 185, 129)))
                {
                    g.FillEllipse(avatarBrush, avatarRect);
                }
                
                // Draw user icon in avatar with modern font
                using (var iconBrush = new SolidBrush(Color.White))
                using (var iconFont = new Font("Segoe UI", 11, FontStyle.Bold))
                {
                    var iconRect = new Rectangle(avatarRect.X, avatarRect.Y, avatarRect.Width, avatarRect.Height);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("JD", iconFont, iconBrush, iconRect, sf);
                }

                // Note: Username and role text are intentionally removed from main button
                // to avoid duplication with the dropdown. Only avatar and arrow are shown.

                // Draw modern dropdown arrow
                var arrowSize = 7; // Slightly larger
                var arrowX = btnUserProfile.Width - 20;
                var arrowY = (btnUserProfile.Height - arrowSize) / 2;
                
                using (var arrowBrush = new SolidBrush(Color.FromArgb(16, 185, 129)))
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

            // Create modern dropdown panel (initially hidden)
            pnlUserDropdown = new Panel
            {
                Width = 220, // Wider for better content spacing
                Height = 210, // Taller for better proportions
                Top = 55, // Position it below the button
                Left = -25, // Align it with the button
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

                // Draw modern shadow with blur effect
                using (var shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                {
                    g.FillRoundedRectangle(shadowBrush, new Rectangle(4, 4, pnlUserDropdown.Width - 4, pnlUserDropdown.Height - 4), 12);
                }

                // Draw modern dropdown background
                using (var backgroundBrush = new SolidBrush(Color.White))
                {
                    g.FillRoundedRectangle(backgroundBrush, new Rectangle(0, 0, pnlUserDropdown.Width - 4, pnlUserDropdown.Height - 4), 12);
                }

                // Draw modern subtle border
                using (var borderPen = new Pen(Color.FromArgb(229, 231, 235), 1.5f))
                {
                    g.DrawRoundedRectangle(borderPen, new Rectangle(0, 0, pnlUserDropdown.Width - 5, pnlUserDropdown.Height - 5), 12);
                }
            };

            // User info section with ultra-modern styling
            var userInfoPanel = new Panel
            {
                Width = 190, // Wider to match new dropdown width
                Height = 52, // Slightly taller
                Top = 12,
                Left = 15,
                BackColor = Color.FromArgb(249, 250, 251) // Lighter modern background
            };
            
            userInfoPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(249, 250, 251)))
                {
                    g.FillRoundedRectangle(brush, new Rectangle(0, 0, userInfoPanel.Width, userInfoPanel.Height), 8); // More rounded
                }
                
                // Draw modern user avatar circle
                var avatarRect = new Rectangle(12, 12, 32, 32); // Slightly larger
                using (var avatarBrush = new SolidBrush(Color.FromArgb(16, 185, 129)))
                {
                    g.FillEllipse(avatarBrush, avatarRect);
                }
                
                // Draw user icon in avatar with modern font
                using (var iconBrush = new SolidBrush(Color.White))
                using (var iconFont = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("JD", iconFont, iconBrush, avatarRect, sf);
                }
                
                // Draw username with modern typography
                using (var textBrush = new SolidBrush(Color.FromArgb(17, 24, 39)))
                using (var textFont = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                    g.DrawString("John Doe", textFont, textBrush, new Point(52, 14));
                }
                
                // Draw role with modern typography
                using (var roleBrush = new SolidBrush(Color.FromArgb(75, 85, 99)))
                using (var roleFont = new Font("Segoe UI", 9, FontStyle.Regular))
                {
                    g.DrawString("System Administrator", roleFont, roleBrush, new Point(52, 30));
                }
            };

            // Modern separator line
            var separator = new Panel
            {
                Height = 1,
                Width = 190, // Match new dropdown width
                Top = 70,
                Left = 15,
                BackColor = Color.FromArgb(229, 231, 235) // Softer separator color
            };

            // Profile menu button with modern styling
            var btnProfile = CreateDropdownMenuItem("👤  My Profile", 75, () =>
            {
                HideUserDropdown();
                OpenMyProfileForm();
            });


            // Ultra-modern logout button with premium design
            var btnLogout = new Button
            {
                Text = "🚪  Sign Out",
                Width = 190, // Match new dropdown width
                Height = 42, // Slightly taller
                Top = 150,
                Left = 15,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(239, 68, 68), // Modern red color
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 55, 55); // Modern hover state
            btnLogout.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 45, 45); // Modern pressed state

            // Modern rounded corners for logout button with premium styling
            btnLogout.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, btnLogout.Width, btnLogout.Height);
                
                // Create ultra-modern gradient brush
                using (var brush = new LinearGradientBrush(rect, 
                    Color.FromArgb(245, 75, 75), Color.FromArgb(225, 60, 60), LinearGradientMode.Vertical))
                {
                    g.FillRoundedRectangle(brush, rect, 12); // More rounded corners
                }

                // Add ultra-modern subtle highlight
                using (var highlightBrush = new SolidBrush(Color.FromArgb(35, 255, 255, 255)))
                {
                    g.FillRoundedRectangle(highlightBrush, new Rectangle(1, 1, rect.Width - 2, rect.Height / 2), 11); // Match rounded corners
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
                userInfoPanel, separator, btnProfile, btnLogout
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
                    pnlUserProfile.Location = new Point(this.Width - 120, 0); // Adjusted for new width
                    // Update dropdown position relative to the main form
                    pnlUserDropdown.Location = new Point(this.Width - 245, 90); // Adjusted for new dimensions
                }
            };
            
            // Set initial dropdown position
            pnlUserDropdown.Location = new Point(this.Width - 245, 90);
        }

        private Button CreateDropdownMenuItem(string text, int top, Action onClick)
        {
            var button = new Button
            {
                Text = "", // Empty text to prevent duplicate drawing
                Width = 190, // Match new dropdown width
                Height = 40, // Increased height for better touch targets
                Top = top,
                Left = 15,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(17, 24, 39), // Modern text color
                Font = new Font("Segoe UI", 11, FontStyle.Bold), // Modern font
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 12, 0), // Increased padding
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
                
                // Draw modern background with rounded corners on hover
                if (button.ClientRectangle.Contains(button.PointToClient(Control.MousePosition)))
                {
                    using (var hoverBrush = new SolidBrush(Color.FromArgb(240, 253, 250))) // Modern teal-tinted hover
                    {
                        g.FillRoundedRectangle(hoverBrush, rect, 8); // More rounded corners
                    }
                    
                    // Add subtle modern border on hover
                    using (var hoverPen = new Pen(Color.FromArgb(187, 247, 221), 1.5f)) // Soft teal border
                    {
                        g.DrawRoundedRectangle(hoverPen, new Rectangle(0, 0, rect.Width - 1, rect.Height - 1), 8);
                    }
                }
                
                // Draw text manually with modern styling
                using (var textBrush = new SolidBrush(button.ForeColor))
                {
                    var textRect = new Rectangle(18, 0, button.Width - 30, button.Height);
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
                Text = "✖ Cancel", // Modern X mark
                Size = new Size(140, 45), // Larger to match Yes button
                Location = new Point(320, 12),
                BackColor = Color.FromArgb(107, 114, 128), // Modern gray
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), // Modern font
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.No
            };
            btnNo.FlatAppearance.BorderSize = 0;
            
            // Add modern rounded corners to Cancel button
            btnNo.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, btnNo.Width, btnNo.Height);
                using (var brush = new SolidBrush(btnNo.BackColor))
                {
                    g.FillRoundedRectangle(brush, rect, 10);
                }
                var textRect = new Rectangle(0, 0, btnNo.Width, btnNo.Height);
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using (var textBrush = new SolidBrush(btnNo.ForeColor))
                {
                    g.DrawString(btnNo.Text, btnNo.Font, textBrush, textRect, sf);
                }
            };

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

                // Show modern logout progress
                using var logoutDialog = new Form
                {
                    Text = "Logging out...",
                    Size = new Size(450, 180), // Larger for better appearance
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ControlBox = false,
                    BackColor = Color.FromArgb(249, 250, 251) // Modern background
                };

                var progressLabel = new Label
                {
                    Text = "🔄 Logging out safely...",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold), // Modern font and size
                    ForeColor = Color.FromArgb(17, 24, 39), // Modern text color
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
                BackColor = Color.FromArgb(248, 250, 252), // Modern lighter background
                Padding = new Padding(35, 35, 35, 35), // Increased padding for better spacing
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
            
            cardTotalProducts = CreateDashboardCard("📦 Total Products", "0", Color.FromArgb(16, 185, 129), 0); // Brand teal
            cardTotalCustomers = CreateDashboardCard("👥 Total Customers", "0", Color.FromArgb(34, 197, 94), 1); // Modern green
            cardTotalSales = CreateDashboardCard("💰 Total Sales", "$0.00", Color.FromArgb(251, 191, 36), 2); // Modern amber
            cardLowStock = CreateDashboardCard("⚠️ Low Stock Items", "0", Color.FromArgb(239, 68, 68), 3); // Modern red
            
            cardsPanel.Controls.AddRange(new Control[] {
                cardTotalProducts, cardTotalCustomers, cardTotalSales, cardLowStock
            });
            
            pnlContent.Controls.Add(cardsPanel);
        }
        
        private Panel CreateDashboardCard(string title, string value, Color accentColor, int index)
        {
            var card = new Panel
            {
                Width = 300, // Slightly wider for better content
                Height = 130, // Taller for better proportions
                Left = index * 320, // Increased spacing between cards
                Top = 10,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            
            // Card shadow and rounded corners
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Draw modern subtle shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    g.FillRoundedRectangle(shadowBrush, new Rectangle(4, 4, card.Width - 4, card.Height - 4), 12);
                }
                
                // Draw modern card background
                using (var cardBrush = new SolidBrush(Color.White))
                {
                    g.FillRoundedRectangle(cardBrush, new Rectangle(0, 0, card.Width - 4, card.Height - 4), 12);
                }
                
                // Draw modern accent border with rounded top
                using (var accentBrush = new SolidBrush(accentColor))
                {
                    g.FillRoundedRectangle(accentBrush, new Rectangle(0, 0, card.Width - 4, 6), 12);
                }
            };
            
            // Modern card title
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold), // Modern font
                ForeColor = Color.FromArgb(75, 85, 99), // Modern muted text
                Location = new Point(25, 20), // Adjusted for new size
                Size = new Size(250, 28),
                BackColor = Color.Transparent
            };
            
            // Modern card value
            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 26, FontStyle.Bold), // Larger, modern font
                ForeColor = accentColor,
                Location = new Point(25, 50),
                Size = new Size(250, 55),
                BackColor = Color.Transparent
            };
            
            // Modern card trend indicator
            var lblTrend = new Label
            {
                Text = "↗️ +12% from last month", // Modern arrow emoji
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(34, 197, 94), // Modern green for positive trend
                Location = new Point(25, 105),
                Size = new Size(250, 20),
                BackColor = Color.Transparent
            };
            
            card.Controls.AddRange(new Control[] { lblTitle, lblValue, lblTrend });
            
            // Modern hover effects with smooth transitions
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(249, 250, 251); // Subtle hover color
                card.Invalidate(); // Ensure repaint
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.White;
                card.Invalidate(); // Ensure repaint
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
                Font = new Font("Segoe UI", 16, FontStyle.Bold), // Larger, modern font
                ForeColor = Color.FromArgb(17, 24, 39), // Modern dark text
                Location = new Point(0, 0),
                Size = new Size(250, 35),
                BackColor = Color.Transparent
            };
            
            btnQuickSale = CreateQuickActionButton("💰 New Sale", Color.FromArgb(16, 185, 129), 0); // Brand teal
            btnAddProduct = CreateQuickActionButton("📦 Add Product", Color.FromArgb(59, 130, 246), 1); // Modern blue
            btnAddCustomer = CreateQuickActionButton("👥 Add Customer", Color.FromArgb(139, 92, 246), 2); // Modern purple
            btnRefreshDashboard = CreateQuickActionButton("🔄 Refresh", Color.FromArgb(234, 88, 12), 3); // Orange refresh button
            
            // Connect quick action buttons
            btnQuickSale.Click += (s, e) => OpenSalesForm();
            btnAddProduct.Click += (s, e) => OpenProductsForm();
            btnAddCustomer.Click += (s, e) => OpenCustomersForm();
            btnRefreshDashboard.Click += async (s, e) => await RefreshDashboardData();
            
            actionsPanel.Controls.AddRange(new Control[] {
                lblQuickActions, btnQuickSale, btnAddProduct, btnAddCustomer, btnRefreshDashboard
            });
            
            pnlContent.Controls.Add(actionsPanel);
        }
        
        private Button CreateQuickActionButton(string text, Color backColor, int index)
        {
            var button = new Button
            {
                Text = text,
                Width = 200, // Wider buttons
                Height = 55, // Taller for better touch targets
                Left = index * 220, // Increased spacing
                Top = 40,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold), // Modern font
                Cursor = Cursors.Hand
            };
            
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Min(255, backColor.R + 15), // Lighter on hover instead of darker
                Math.Min(255, backColor.G + 15),
                Math.Min(255, backColor.B + 15));
            
            // Modern rounded button corners with gradient
            button.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                var rect = new Rectangle(0, 0, button.Width, button.Height);
                
                // Draw gradient background
                using (var brush = new LinearGradientBrush(rect, 
                    button.BackColor, 
                    Color.FromArgb(Math.Max(0, button.BackColor.R - 10),
                                 Math.Max(0, button.BackColor.G - 10),
                                 Math.Max(0, button.BackColor.B - 10)), 
                    LinearGradientMode.Vertical))
                {
                    g.FillRoundedRectangle(brush, rect, 12); // More rounded corners
                }
                
                // Add subtle highlight
                using (var highlightBrush = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                {
                    g.FillRoundedRectangle(highlightBrush, new Rectangle(1, 1, rect.Width - 2, rect.Height / 2), 11);
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
            var weeklyTotal = _weeklySalesData?.Sum() ?? 0;
            using (var totalBrush = new SolidBrush(Color.FromArgb(40, 167, 69)))
            using (var totalFont = new Font("Segoe UI", 11, FontStyle.Bold))
            {
                g.DrawString($"Total: {weeklyTotal:C}", totalFont, totalBrush, new Point(20, 40));
            }
            
            // Chart area
            var chartArea = new Rectangle(40, 80, bounds.Width - 80, bounds.Height - 110);
            
            // Check if we have any real sales data
            bool hasRealData = _weeklySalesData != null && _weeklySalesData.Any(d => d > 0);
            
            if (!hasRealData)
            {
                // Draw empty state
                DrawEmptySalesChart(g, chartArea);
                return;
            }
            
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
            var maxSale = _weeklySalesData?.Max() ?? 0;
            if (maxSale > 0 && _weeklySalesData != null)
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
        
        private void DrawEmptySalesChart(Graphics g, Rectangle chartArea)
        {
            // Draw a subtle message indicating no data
            using (var emptyBrush = new SolidBrush(Color.FromArgb(108, 117, 125)))
            using (var emptyFont = new Font("Segoe UI", 12, FontStyle.Italic))
            using (var iconBrush = new SolidBrush(Color.FromArgb(173, 181, 189)))
            using (var iconFont = new Font("Segoe UI", 24))
            {
                var centerX = chartArea.X + chartArea.Width / 2;
                var centerY = chartArea.Y + chartArea.Height / 2;
                
                // Draw icon
                var iconText = "📊";
                var iconSize = g.MeasureString(iconText, iconFont);
                g.DrawString(iconText, iconFont, iconBrush, 
                    new Point(centerX - (int)iconSize.Width / 2, centerY - 60));
                
                // Draw message
                var message = "No sales data available";
                var messageSize = g.MeasureString(message, emptyFont);
                g.DrawString(message, emptyFont, emptyBrush, 
                    new Point(centerX - (int)messageSize.Width / 2, centerY - 10));
                
                var subMessage = "Sales data will appear here once transactions are recorded";
                var subMessageFont = new Font("Segoe UI", 9);
                var subMessageSize = g.MeasureString(subMessage, subMessageFont);
                g.DrawString(subMessage, subMessageFont, emptyBrush, 
                    new Point(centerX - (int)subMessageSize.Width / 2, centerY + 15));
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
            
            // Check if we have real low stock data
            if (_lowStockItems == null || !_lowStockItems.Any())
            {
                // Draw empty state
                DrawEmptyStockAlerts(g, bounds);
                return;
            }
            
            // Display count of low stock items
            using (var countBrush = new SolidBrush(Color.FromArgb(220, 53, 69)))
            using (var countFont = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                g.DrawString($"{_lowStockItems.Count} items need attention", countFont, countBrush, new Point(20, 40));
            }
            
            int y = 65;
            int maxDisplayItems = Math.Min(_lowStockItems.Count, 4); // Show max 4 items
            
            for (int i = 0; i < maxDisplayItems; i++)
            {
                var item = _lowStockItems[i];
                var isCritical = item.Stock <= (item.MinStock * 0.5); // Critical if stock is half of minimum
                var alertColor = isCritical ? Color.FromArgb(220, 53, 69) : Color.FromArgb(255, 193, 7);
                var alertIcon = isCritical ? "🔴" : "🟡";
                var status = isCritical ? "Critical" : "Low";
                
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
                    // Truncate product name if too long
                    var productName = item.Name.Length > 20 ? item.Name.Substring(0, 20) + "..." : item.Name;
                    g.DrawString($"{alertIcon} {productName}", textFont, textBrush, new Point(30, y + 8));
                    g.DrawString($"Stock: {item.Stock}", stockFont, stockBrush, new Point(bounds.Width - 120, y + 8));
                    g.DrawString($"Min: {item.MinStock}", stockFont, stockBrush, new Point(bounds.Width - 120, y + 22));
                }
                
                y += 50;
            }
            
            // If there are more items, show a summary
            if (_lowStockItems.Count > 4)
            {
                using (var moreBrush = new SolidBrush(Color.FromArgb(108, 117, 125)))
                using (var moreFont = new Font("Segoe UI", 9, FontStyle.Italic))
                {
                    g.DrawString($"...and {_lowStockItems.Count - 4} more items", moreFont, moreBrush, new Point(30, y + 10));
                }
            }
        }
        
        private void DrawEmptyStockAlerts(Graphics g, Rectangle bounds)
        {
            // Draw empty state message
            using (var emptyBrush = new SolidBrush(Color.FromArgb(108, 117, 125)))
            using (var emptyFont = new Font("Segoe UI", 12, FontStyle.Italic))
            using (var iconBrush = new SolidBrush(Color.FromArgb(173, 181, 189)))
            using (var iconFont = new Font("Segoe UI", 24))
            {
                var centerX = bounds.X + bounds.Width / 2;
                var centerY = bounds.Y + bounds.Height / 2;
                
                // Draw icon
                var iconText = "✅";
                var iconSize = g.MeasureString(iconText, iconFont);
                g.DrawString(iconText, iconFont, iconBrush, 
                    new Point(centerX - (int)iconSize.Width / 2, centerY - 60));
                
                // Draw message
                var message = "All stock levels are healthy";
                var messageSize = g.MeasureString(message, emptyFont);
                g.DrawString(message, emptyFont, emptyBrush, 
                    new Point(centerX - (int)messageSize.Width / 2, centerY - 10));
                
                var subMessage = "Low stock alerts will appear here when items need restocking";
                var subMessageFont = new Font("Segoe UI", 9);
                var subMessageSize = g.MeasureString(subMessage, subMessageFont);
                g.DrawString(subMessage, subMessageFont, emptyBrush, 
                    new Point(centerX - (int)subMessageSize.Width / 2, centerY + 15));
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
            
            // Check if we have real sales data
            if (_recentSales == null || !_recentSales.Any())
            {
                // Draw empty state
                DrawEmptyRecentSales(g, bounds);
                return;
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
            
            // Display real recent sales data
            int y = headerHeight + 10;
            int maxDisplaySales = Math.Min(_recentSales.Count, 5); // Show max 5 sales
            
            using (var textBrush = new SolidBrush(Color.FromArgb(33, 37, 41)))
            using (var textFont = new Font("Segoe UI", 9))
            using (var statusBrush = new SolidBrush(Color.FromArgb(40, 167, 69)))
            using (var statusFont = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                for (int i = 0; i < maxDisplaySales; i++)
                {
                    if (y > bounds.Height - 70) break; // Leave space for button
                    
                    var sale = _recentSales[i];
                    
                    // Alternate row background
                    if (i % 2 == 1)
                    {
                        using (var rowBrush = new SolidBrush(Color.FromArgb(248, 249, 250)))
                        {
                            g.FillRectangle(rowBrush, new Rectangle(0, y - 5, bounds.Width, 30));
                        }
                    }
                    
                    // Sale date
                    g.DrawString(sale.Date.ToString("MMM dd, HH:mm"), textFont, textBrush, new Point(20, y));
                    
                    // Customer name (truncate if too long)
                    var customerName = !string.IsNullOrEmpty(sale.CustomerName) ? sale.CustomerName : "Walk-in Customer";
                    if (customerName.Length > 18)
                        customerName = customerName.Substring(0, 15) + "...";
                    g.DrawString(customerName, textFont, textBrush, new Point(140, y));
                    
                    // Number of items (count sale items if available, otherwise show "-")
                    var itemCount = sale.Items?.Count ?? 0;
                    g.DrawString(itemCount > 0 ? itemCount.ToString() : "-", textFont, textBrush, new Point(300, y));
                    
                    // Total amount
                    g.DrawString(sale.TotalAmount.ToString("C"), textFont, textBrush, new Point(400, y));
                    
                    // Payment method (if available)
                    var paymentMethod = !string.IsNullOrEmpty(sale.PaymentMethod) ? sale.PaymentMethod : "N/A";
                    if (paymentMethod.Length > 12)
                        paymentMethod = paymentMethod.Substring(0, 10) + "...";
                    g.DrawString(paymentMethod, textFont, textBrush, new Point(500, y));
                    
                    // Status - assume completed for existing sales
                    g.DrawString("✅ Completed", statusFont, statusBrush, new Point(620, y));
                    
                    y += 35;
                }
            }
            
            // Show count if there are more sales
            if (_recentSales.Count > 5)
            {
                using (var moreBrush = new SolidBrush(Color.FromArgb(108, 117, 125)))
                using (var moreFont = new Font("Segoe UI", 9, FontStyle.Italic))
                {
                    g.DrawString($"...and {_recentSales.Count - 5} more sales", moreFont, moreBrush, new Point(20, y + 5));
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
                var buttonText = "View All Sales";
                var textSize = g.MeasureString(buttonText, buttonFont);
                var textX = buttonRect.X + (buttonRect.Width - (int)textSize.Width) / 2;
                var textY = buttonRect.Y + (buttonRect.Height - (int)textSize.Height) / 2;
                g.DrawString(buttonText, buttonFont, buttonTextBrush, new Point(textX, textY));
            }
        }
        
        private void DrawEmptyRecentSales(Graphics g, Rectangle bounds)
        {
            // Draw empty state message
            using (var emptyBrush = new SolidBrush(Color.FromArgb(108, 117, 125)))
            using (var emptyFont = new Font("Segoe UI", 12, FontStyle.Italic))
            using (var iconBrush = new SolidBrush(Color.FromArgb(173, 181, 189)))
            using (var iconFont = new Font("Segoe UI", 24))
            {
                var centerX = bounds.X + bounds.Width / 2;
                var centerY = bounds.Y + bounds.Height / 2;
                
                // Draw icon
                var iconText = "🛒";
                var iconSize = g.MeasureString(iconText, iconFont);
                g.DrawString(iconText, iconFont, iconBrush, 
                    new Point(centerX - (int)iconSize.Width / 2, centerY - 60));
                
                // Draw message
                var message = "No recent sales activity";
                var messageSize = g.MeasureString(message, emptyFont);
                g.DrawString(message, emptyFont, emptyBrush, 
                    new Point(centerX - (int)messageSize.Width / 2, centerY - 10));
                
                var subMessage = "Recent sales transactions will be displayed here";
                var subMessageFont = new Font("Segoe UI", 9);
                var subMessageSize = g.MeasureString(subMessage, subMessageFont);
                g.DrawString(subMessage, subMessageFont, emptyBrush, 
                    new Point(centerX - (int)subMessageSize.Width / 2, centerY + 15));
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
        
        private async void MainForm_Shown(object? sender, EventArgs e)
        {
            // Force refresh of dashboard data when form is shown
            await RefreshDashboardData();
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
                
                // Refresh charts and panels with latest data
                chartSalesPanel.Invalidate();
                stockAlertsPanel.Invalidate();
                recentSalesPanel.Invalidate();
                
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
                _logger.LogInformation("Loading product count...");
                var response = await _apiService.GetProductsAsync(new PaginationParameters { PageNumber = 1, PageSize = 1 });
                
                _logger.LogInformation($"Product API Response - Success: {response.Success}, Data: {response.Data != null}, Count: {response.Data?.TotalCount ?? 0}");
                
                if (response.Success && response.Data != null)
                {
                    var count = response.Data.TotalCount;
                    _logger.LogInformation($"Updating product card with count: {count}");
                    UpdateDashboardCard(cardTotalProducts, count.ToString("N0"));
                }
                else
                {
                    _logger.LogWarning($"Product API call failed - Success: {response.Success}, Data is null: {response.Data == null}, Message: {response.Message}");
                    UpdateDashboardCard(cardTotalProducts, "Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while loading product count");
                UpdateDashboardCard(cardTotalProducts, "Error");
            }
        }
        
        private async Task LoadCustomerCountAsync()
        {
            try
            {
                _logger.LogInformation("Loading customer count...");
                var response = await _apiService.GetCustomersAsync(new PaginationParameters { PageNumber = 1, PageSize = 1 });
                
                _logger.LogInformation($"Customer API Response - Success: {response.Success}, Data: {response.Data != null}, Count: {response.Data?.TotalCount ?? 0}");
                
                if (response.Success && response.Data != null)
                {
                    var count = response.Data.TotalCount;
                    _logger.LogInformation($"Updating customer card with count: {count}");
                    UpdateDashboardCard(cardTotalCustomers, count.ToString("N0"));
                }
                else
                {
                    _logger.LogWarning($"Customer API call failed - Success: {response.Success}, Data is null: {response.Data == null}, Message: {response.Message}");
                    UpdateDashboardCard(cardTotalCustomers, "Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while loading customer count");
                UpdateDashboardCard(cardTotalCustomers, "Error");
            }
        }
        
        private async Task LoadSalesDataAsync()
        {
            try
            {
                _logger.LogInformation("Loading sales data...");
                // Load recent sales for dashboard
                var recentSalesResponse = await _apiService.GetSalesAsync(new PaginationParameters { PageNumber = 1, PageSize = 10 });
                
                _logger.LogInformation($"Sales API Response - Success: {recentSalesResponse.Success}, Data: {recentSalesResponse.Data != null}, Items: {recentSalesResponse.Data?.Items?.Count ?? 0}");
                
                if (recentSalesResponse.Success && recentSalesResponse.Data?.Items != null)
                {
                    _recentSales = recentSalesResponse.Data.Items;
                    
                    // Calculate total sales amount
                    decimal totalSales = _recentSales.Sum(s => s.TotalAmount);
                    _logger.LogInformation($"Updating sales card with total: {totalSales:C}");
                    UpdateDashboardCard(cardTotalSales, totalSales.ToString("C"));
                    
                    // Generate weekly sales data
                    GenerateWeeklySalesData();
                }
                else
                {
                    _logger.LogWarning($"Sales API call failed or no data - Success: {recentSalesResponse.Success}, Message: {recentSalesResponse.Message}");
                    // No real data available - clear the collections
                    _recentSales.Clear();
                    _weeklySalesData = new decimal[7];
                    UpdateDashboardCard(cardTotalSales, "$0.00");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while loading sales data");
                UpdateDashboardCard(cardTotalSales, "Error");
                // Clear data on error
                _recentSales.Clear();
                _weeklySalesData = new decimal[7];
            }
        }
        
        private void GenerateWeeklySalesData()
        {
            // Initialize weekly data
            _weeklySalesData = new decimal[7];
            
            if (_recentSales != null && _recentSales.Any())
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
            // If no real sales data, leave the array with zeros (empty state will be shown)
        }
        
        
        private async Task LoadLowStockCountAsync()
        {
            try
            {
                var response = await _apiService.GetProductsAsync(new PaginationParameters { PageNumber = 1, PageSize = 100 });
                if (response.Success && response.Data?.Items != null)
                {
                    // Filter products with low stock and populate the low stock items list
                    _lowStockItems = response.Data.Items
                        .Where(p => p.Stock <= p.MinStock)
                        .OrderBy(p => p.Stock) // Order by stock level (most critical first)
                        .ToList();
                    
                    var lowStockCount = _lowStockItems.Count;
                    UpdateDashboardCard(cardLowStock, lowStockCount.ToString());
                    
                    // Update card color based on low stock severity
                    var card = cardLowStock;
                    var valueLabel = card.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size == 26);
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
                        else if (lowStockCount > 0)
                        {
                            valueLabel.ForeColor = Color.FromArgb(255, 193, 7); // Yellow for any low stock
                        }
                        else
                        {
                            valueLabel.ForeColor = Color.FromArgb(40, 167, 69); // Green for good
                        }
                    }
                }
                else
                {
                    // No data available - clear the low stock items
                    _lowStockItems.Clear();
                    UpdateDashboardCard(cardLowStock, "0");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading low stock count");
                UpdateDashboardCard(cardLowStock, "Error");
                // Clear data on error
                _lowStockItems.Clear();
            }
        }
        
        private void UpdateDashboardCard(Panel card, string value)
        {
            if (card.InvokeRequired)
            {
                card.Invoke(new Action(() => UpdateDashboardCard(card, value)));
                return;
            }
            
            var valueLabel = card.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size == 26);
            if (valueLabel != null)
            {
                _logger.LogInformation($"Updating dashboard card '{card.Name}' with value '{value}'");
                valueLabel.Text = value;
                valueLabel.Invalidate(); // Force the label to repaint
                card.Invalidate(); // Force the card to repaint
            }
            else
            {
                _logger.LogWarning($"Could not find value label in card '{card.Name}' - label count: {card.Controls.OfType<Label>().Count()}");
                // Debug: Log all labels and their font sizes
                foreach (var label in card.Controls.OfType<Label>())
                {
                    _logger.LogInformation($"Label in card '{card.Name}': Text='{label.Text}', FontSize={label.Font.Size}");
                }
            }
        }
        
        private void NavButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.Tag is int buttonIndex)
            {
                // Reset all buttons
                var allNavButtons = new[] { btnDashboard, btnProducts, btnCustomers, btnSales, btnSalesHistory, btnReports, btnAbout };
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
                    case 6: // About
                        OpenAboutDialog();
                        break;
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
        
        
        private void OpenMyProfileForm()
        {
            try
            {
                using var profileForm = _serviceProvider.GetRequiredService<MyProfileForm>();
                profileForm.ShowDialog();
                // Refresh dashboard after closing profile form
                _ = RefreshDashboardData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening my profile form");
                MessageBox.Show($"Error opening profile form: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenAboutDialog()
        {
            try
            {
                using var aboutDialog = new Form
                {
                    Text = "",
                    Size = new Size(900, 650),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.None,
                    BackColor = Color.FromArgb(245, 247, 250),
                    Font = new Font("Segoe UI", 9F)
                };

                // Main container with rounded border
                var mainContainer = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    Padding = new Padding(20)
                };

                // Create the card container
                var cardPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White
                };

                cardPanel.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    // Draw shadow
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                    {
                        g.FillRoundedRectangle(shadowBrush, new Rectangle(8, 8, cardPanel.Width - 8, cardPanel.Height - 8), 20);
                    }
                    
                    // Draw main card background
                    using (var cardBrush = new SolidBrush(Color.White))
                    {
                        g.FillRoundedRectangle(cardBrush, new Rectangle(0, 0, cardPanel.Width - 8, cardPanel.Height - 8), 20);
                    }
                    
                    // Draw subtle border
                    using (var borderPen = new Pen(Color.FromArgb(220, 224, 229), 2))
                    {
                        g.DrawRoundedRectangle(borderPen, new Rectangle(1, 1, cardPanel.Width - 10, cardPanel.Height - 10), 20);
                    }
                };

                // Header section with gradient background
                var headerPanel = new Panel
                {
                    Height = 120,
                    Dock = DockStyle.Top,
                    BackColor = Color.Transparent,
                    Padding = new Padding(40, 30, 40, 20)
                };

                headerPanel.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    var headerRect = new Rectangle(0, 0, headerPanel.Width - 8, 120);
                    using (var brush = new LinearGradientBrush(
                        headerRect,
                        Color.FromArgb(74, 144, 226),
                        Color.FromArgb(143, 148, 251),
                        LinearGradientMode.Horizontal))
                    {
                        g.FillRoundedRectangleTop(brush, headerRect, 20);
                    }
                    
                    // Add subtle overlay
                    using (var overlayBrush = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                    {
                        g.FillRoundedRectangleTop(overlayBrush, headerRect, 20);
                    }
                };

                // Close button (X)
                var closeBtn = new Button
                {
                    Text = "✕",
                    Size = new Size(35, 35),
                    Location = new Point(headerPanel.Width - 75, 15),
                    BackColor = Color.Transparent,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                closeBtn.FlatAppearance.BorderSize = 0;
                closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 255, 255, 255);

                closeBtn.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    if (closeBtn.ClientRectangle.Contains(closeBtn.PointToClient(Control.MousePosition)))
                    {
                        using (var hoverBrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                        {
                            g.FillEllipse(hoverBrush, new Rectangle(0, 0, closeBtn.Width, closeBtn.Height));
                        }
                    }
                    
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using (var textBrush = new SolidBrush(closeBtn.ForeColor))
                    {
                        g.DrawString(closeBtn.Text, closeBtn.Font, textBrush, new Rectangle(0, 0, closeBtn.Width, closeBtn.Height), sf);
                    }
                };

                closeBtn.Click += (s, e) => aboutDialog.Close();

                // App icon and title
                var iconLabel = new Label
                {
                    Text = "📦",
                    Font = new Font("Segoe UI", 32),
                    ForeColor = Color.White,
                    Location = new Point(40, 25),
                    Size = new Size(60, 60),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                var titleLabel = new Label
                {
                    Text = "InventoryPro",
                    Font = new Font("Segoe UI", 28, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(120, 20),
                    Size = new Size(330, 60),//40
                    BackColor = Color.Transparent
                };

                var subtitleLabel = new Label
                {
                    Text = "Professional Inventory Management System",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.FromArgb(240, 248, 255),
                    Location = new Point(120, 80),
                    Size = new Size(400, 30),
                    BackColor = Color.Transparent
                };

                var versionBadge = new Panel
                {
                    Size = new Size(90, 30),
                    Location = new Point(headerPanel.Width - 180, 35),
                    BackColor = Color.Transparent,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                versionBadge.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    using (var badgeBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
                    {
                        g.FillRoundedRectangle(badgeBrush, new Rectangle(0, 0, versionBadge.Width, versionBadge.Height), 15);
                    }
                    
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using (var textBrush = new SolidBrush(Color.FromArgb(74, 144, 226)))
                    using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
                    {
                        g.DrawString("v2.0", font, textBrush, new Rectangle(0, 0, versionBadge.Width, versionBadge.Height), sf);
                    }
                };

                headerPanel.Controls.AddRange(new Control[] { iconLabel, titleLabel, subtitleLabel, versionBadge, closeBtn });

                // Content area with scroll - optimized for smooth scrolling
                var contentPanel = new OptimizedScrollPanel
                    {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    Height = 300,
                    Padding = new Padding(40, 30, 40, 20),//20
                    AutoScroll = true,
                    MinimumSize = new Size(0, 100)//600
                    };
                
                // Apply additional scrolling optimizations
                OptimizePanelScrolling(contentPanel);
                

                // Purpose card
                var purposeCard = CreateInfoCard("🎯", "Project Purpose", 
                    "InventoryPro is a comprehensive inventory management system designed to streamline business operations through modern technology. Our platform provides efficient product tracking, customer management, sales processing, and comprehensive reporting capabilities specifically tailored for small to medium businesses seeking digital transformation.",
                    0, Color.FromArgb(52, 152, 219));
                purposeCard.Top = 120;//0
                

                // Features card  
                var featuresCard = CreateInfoCard("⚡", "Key Features\n",
                    "• Real-time inventory tracking and management\n• Advanced customer relationship management\n• Comprehensive sales and POS system\n• Detailed analytics and reporting dashboard\n• Multi-user support with role-based access & Modern and intuitive user interface",
                    0, Color.FromArgb(155, 89, 182));
                featuresCard.Top = 310;//170

                // Team section title
                var teamTitlePanel = new Panel
                {
                    Height = 70,
                    Width = 800,
                    Top = 500,//340
                    BackColor = Color.Transparent
                };

                var teamIcon = new Label
                {
                    Text = "👥",
                    Font = new Font("Segoe UI", 20, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(0, 10),
                    Size = new Size(50, 60),
                    BackColor = Color.Transparent
                };

                var teamTitle = new Label
                {
                    Text = "Development Team",
                    Font = new Font("Segoe UI", 20, FontStyle.Bold),
                    ForeColor = Color.FromArgb(44, 62, 80),
                    Location = new Point(95, 15),//50
                    Size = new Size(360, 90),//30
                    BackColor = Color.Transparent
                };

                teamTitlePanel.Controls.AddRange(new Control[] { teamIcon, teamTitle });

                // Team members grid
                var teamGridPanel = new Panel
                {
                    Top = 590,//410
                    Width = 800,
                    Height = 390,//250
                    BackColor = Color.Transparent
                };

                var teamMembers = new[]
                {
                    new { Name = "រស្មី ស៊ុនឆាយ", Role = "Developer", Icon = "👥", Color = Color.FromArgb(52, 152, 219) },
                    new { Name = "ម៉ុន ម៉េត", Role = "Developer", Icon = "👨‍💻", Color = Color.FromArgb(52, 152, 219) },
                    new { Name = "ឈន អង្គារឰ", Role = "Front-end Designer", Icon = "🎨", Color = Color.FromArgb(230, 126, 34) },
                    new { Name = "កញ្ញា ហេល ស្រីអិត", Role = "Team Leader", Icon = "👑", Color = Color.FromArgb(241, 196, 15) },
                    new { Name = "សាត ច័ន្ទភក្តី", Role = "Supporter", Icon = "🤝", Color = Color.FromArgb(46, 204, 113) },
                    new { Name = "ប៊ុន ឡេង", Role = "Supporter", Icon = "🤝", Color = Color.FromArgb(46, 204, 113) },
                    new { Name = "សាំង សិលា", Role = "Tester", Icon = "🧪", Color = Color.FromArgb(155, 89, 182) },
                    new { Name = "គង់ វិច្ឆិកា", Role = "Supporter", Icon = "🤝", Color = Color.FromArgb(46, 204, 113) },
                    new { Name = "មូល ចាន់ថន", Role = "Supporter", Icon = "🤝", Color = Color.FromArgb(46, 204, 113) },
                    new { Name = "នួន ចំណាន", Role = "Supporter", Icon = "🤝", Color = Color.FromArgb(46, 204, 113) }
                };

                int col = 0, row = 0;
                foreach (var member in teamMembers)
                {
                    var memberCard = CreateTeamMemberCard(member.Name, member.Role, member.Icon, member.Color);
                    memberCard.Location = new Point(col * 200, row * 50);//200,50
                    teamGridPanel.Controls.Add(memberCard);
                    
                    col++;
                    if (col >= 4) { col = 0; row++; }
                }

                // Footer
                var footerPanel = new Panel
                {
                    Height = 80,
                    Width = 800,
                    Top = 680,
                    BackColor = Color.Transparent,
                    Padding = new Padding(40, 20, 40, 20)
                };

                footerPanel.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    using (var brush = new SolidBrush(Color.FromArgb(248, 249, 250)))
                    {
                        var footerRect = new Rectangle(0, 0, footerPanel.Width - 8, footerPanel.Height);
                        g.FillRoundedRectangleBottom(brush, footerRect, 20);
                    }
                };

                var copyrightLabel = new Label
                {
                    Text = "© 2024 InventoryPro Team • Built with passion for modern business solutions",
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    ForeColor = Color.FromArgb(108, 117, 125),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };

                footerPanel.Controls.Add(copyrightLabel);

                // Add all controls to content panel (scrollable area)
                contentPanel.Controls.AddRange(new Control[] { purposeCard, featuresCard, teamTitlePanel, teamGridPanel, footerPanel });
                cardPanel.Controls.Add(headerPanel);
                cardPanel.Controls.Add(contentPanel);
                mainContainer.Controls.Add(cardPanel);
                aboutDialog.Controls.Add(mainContainer);

                aboutDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening about dialog");
                MessageBox.Show($"Error opening about dialog: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //Card
        private Panel CreateInfoCard(string icon, string title, string content, int topPosition, Color accentColor)
        {
            var card = new Panel
            {
                Width = 800,
                Height = 200,//140
                Top = topPosition,
                BackColor = Color.Transparent
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Card shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    g.FillRoundedRectangle(shadowBrush, new Rectangle(5, 5, card.Width - 5, card.Height - 5), 15);
                }
                
                // Card background
                using (var cardBrush = new SolidBrush(Color.White))
                {
                    g.FillRoundedRectangle(cardBrush, new Rectangle(0, 0, card.Width - 5, card.Height - 5), 15);
                }
                
                // Left accent border
                using (var accentBrush = new SolidBrush(accentColor))
                {
                    g.FillRoundedRectangle(accentBrush, new Rectangle(0, 0, 5, card.Height - 5), 15);
                }
                
                // Icon background circle
                using (var iconBrush = new SolidBrush(Color.FromArgb(30, accentColor.R, accentColor.G, accentColor.B)))
                {
                    g.FillEllipse(iconBrush, new Rectangle(25, 20, 50, 50));
                }
                
                // Draw icon
                using (var iconTextBrush = new SolidBrush(accentColor))
                using (var iconFont = new Font("Segoe UI", 10))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(icon, iconFont, iconTextBrush, new Rectangle(25, 20, 50, 50), sf);
                }
                
                // Draw title
                using (var titleBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                using (var titleFont = new Font("Segoe UI", 14, FontStyle.Bold))
                {
                    g.DrawString(title, titleFont, titleBrush, new Point(95, 25));
                }
                
                // Draw content
                using (var contentBrush = new SolidBrush(Color.FromArgb(74, 84, 102)))
                using (var contentFont = new Font("Segoe UI", 10))
                {
                    var contentRect = new Rectangle(95, 70, card.Width - 120, card.Height - 0);//55,75
                    g.DrawString(content, contentFont, contentBrush, contentRect);
                }
            };

            return card;
        }

        private Panel CreateTeamMemberCard(string name, string role, string icon, Color roleColor)
        {
            var card = new Panel
            {
                Width = 190,
                Height = 45,
                Padding = new Padding(20),
                Margin = new Padding(20),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                var isHovered = card.ClientRectangle.Contains(card.PointToClient(Control.MousePosition));
                
                // Card background
                using (var cardBrush = new SolidBrush(isHovered ? Color.FromArgb(248, 249, 250) : Color.White))
                {
                    g.FillRoundedRectangle(cardBrush, new Rectangle(0, 5, card.Width, card.Height), 10);
                }
                
                // Border
                using (var borderPen = new Pen(isHovered ? roleColor : Color.FromArgb(220, 224, 229), 1))
                {
                    g.DrawRoundedRectangle(borderPen, new Rectangle(0, 0, card.Width - 1, card.Height - 1), 10);
                }
                
                // Icon circle
                using (var iconBrush = new SolidBrush(Color.FromArgb(30, roleColor.R, roleColor.G, roleColor.B)))
                {
                    g.FillEllipse(iconBrush, new Rectangle(8, 8, 28, 28));
                }
                
                // Icon
                using (var iconTextBrush = new SolidBrush(roleColor))
                using (var iconFont = new Font("Segoe UI", 14, FontStyle.Bold))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(icon, iconFont, iconTextBrush, new Rectangle(10, 10, 28, 28), sf);
                }
                
                // Name
                using (var nameBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                using (var nameFont = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                    g.DrawString(name, nameFont, nameBrush, new Point(44, 8));
                }
                
                // Role
                using (var roleBrush = new SolidBrush(roleColor))
                using (var roleFont = new Font("Segoe UI", 7))
                {
                    g.DrawString(role, roleFont, roleBrush, new Point(44, 30));
                }
            };

            card.MouseEnter += (s, e) => card.Invalidate();
            card.MouseLeave += (s, e) => card.Invalidate();

            return card;
        }
        
        private void OptimizePanelScrolling(Panel panel)
        {
            // Enable double buffering via reflection for smooth scrolling
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, panel, new object[] { true });
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
        
        private void CreateStatusBar()
        {
            pnlStatusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 35, // Modern minimal height
                BackColor = Color.FromArgb(249, 250, 251), // Light background
                Padding = new Padding(20, 8, 20, 8)
            };
            
            // Modern status bar styling
            pnlStatusBar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                // Draw subtle top border
                using (var borderPen = new Pen(Color.FromArgb(229, 231, 235), 1))
                {
                    g.DrawLine(borderPen, 0, 0, pnlStatusBar.Width, 0);
                }
            };
            
            // Status information labels
            var lblStatus = new Label
            {
                Text = "🟢 Ready",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(20, 8),
                Size = new Size(100, 20),
                BackColor = Color.Transparent
            };
            
            var lblVersion = new Label
            {
                Text = "InventoryPro v2.0",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(107, 114, 128),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(this.Width - 150, 8),
                Size = new Size(120, 20),
                BackColor = Color.Transparent
            };
            
            // Update version label position on resize
            this.SizeChanged += (s, e) => {
                if (lblVersion != null)
                {
                    lblVersion.Location = new Point(this.Width - 150, 8);
                }
            };
            
            pnlStatusBar.Controls.AddRange(new Control[] { lblStatus, lblVersion });
        }
    }
    
    // Custom panel with optimized scrolling behavior
    public class OptimizedScrollPanel : Panel
    {
        public OptimizedScrollPanel()
        {
            // Enable double buffering and smooth scrolling
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer | 
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);
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
        
        public static void FillRoundedRectangleTop(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
                path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90);
                path.AddLine(bounds.X + bounds.Width, bounds.Y + radius, bounds.X + bounds.Width, bounds.Y + bounds.Height);
                path.AddLine(bounds.X + bounds.Width, bounds.Y + bounds.Height, bounds.X, bounds.Y + bounds.Height);
                path.AddLine(bounds.X, bounds.Y + bounds.Height, bounds.X, bounds.Y + radius);
                path.CloseFigure();
                graphics.FillPath(brush, path);
            }
        }
        
        public static void FillRoundedRectangleBottom(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddLine(bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y);
                path.AddLine(bounds.X + bounds.Width, bounds.Y, bounds.X + bounds.Width, bounds.Y + bounds.Height - radius);
                path.AddArc(bounds.X + bounds.Width - radius, bounds.Y + bounds.Height - radius, radius, radius, 0, 90);
                path.AddArc(bounds.X, bounds.Y + bounds.Height - radius, radius, radius, 90, 90);
                path.AddLine(bounds.X, bounds.Y + bounds.Height - radius, bounds.X, bounds.Y);
                path.CloseFigure();
                graphics.FillPath(brush, path);
            }
        }
    }
}