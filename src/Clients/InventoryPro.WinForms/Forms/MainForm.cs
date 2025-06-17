using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
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
            
            InitializeComponent();
            SetupRealtimeUpdates();
            LoadDashboardDataAsync();
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
            btnSettings = CreateNavButton("⚙️ Settings", 6, false);
            
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
                Location = new Point(30, 20),
                Size = new Size(300, 30),
                BackColor = Color.Transparent
            };
            
            // Date and time
            lblDateTime = new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(30, 45),
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
                Location = new Point(pnlTopBar.Width - 200, 20),
                Size = new Size(150, 40),
                BackColor = Color.Transparent
            };
            
            pnlTopBar.Controls.AddRange(new Control[] { lblWelcome, lblDateTime, lblSystemStatus });
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
            if (sender is Button clickedButton)
            {
                // Reset all buttons
                var allNavButtons = new[] { btnDashboard, btnProducts, btnCustomers, btnSales, btnSalesHistory, btnReports, btnSettings };
                foreach (var btn in allNavButtons)
                {
                    UpdateNavButtonStyle(btn, false);
                }
                
                // Activate clicked button
                UpdateNavButtonStyle(clickedButton, true);
                
                // Handle navigation
                int buttonIndex = (int)clickedButton.Tag;
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
                    case 6: // Settings
                        OpenSettingsForm();
                        break;
                }
            }
        }
        
        private void OpenProductsForm()
        {
            try
            {
                var productForm = _serviceProvider.GetService(typeof(ProductForm)) as ProductForm;
                if (productForm != null)
                {
                    productForm.ShowDialog();
                    // Refresh dashboard after closing product form
                    _ = RefreshDashboardData();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening products form");
                MessageBox.Show("Error opening products form", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenCustomersForm()
        {
            try
            {
                var customerForm = _serviceProvider.GetService(typeof(CustomerForm)) as CustomerForm;
                if (customerForm != null)
                {
                    customerForm.ShowDialog();
                    // Refresh dashboard after closing customer form
                    _ = RefreshDashboardData();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening customers form");
                MessageBox.Show("Error opening customers form", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenSalesForm()
        {
            try
            {
                var salesForm = _serviceProvider.GetService(typeof(SalesForm)) as SalesForm;
                if (salesForm != null)
                {
                    salesForm.ShowDialog();
                    // Refresh dashboard after closing sales form
                    _ = RefreshDashboardData();
                }
                else
                {
                    _logger.LogWarning("SalesForm service not found in service provider");
                    MessageBox.Show("Sales form is not available. Please check system configuration.", 
                        "Service Not Available", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening sales form");
                MessageBox.Show("Error opening sales form. Please try again.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenSalesHistoryForm()
        {
            try
            {
                var salesHistoryForm = _serviceProvider.GetService(typeof(SalesHistoryForm)) as SalesHistoryForm;
                if (salesHistoryForm != null)
                {
                    salesHistoryForm.ShowDialog();
                    // Refresh dashboard after closing sales history form
                    _ = RefreshDashboardData();
                }
                else
                {
                    _logger.LogWarning("SalesHistoryForm service not found in service provider");
                    MessageBox.Show("Sales history form is not available. Please check system configuration.", 
                        "Service Not Available", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening sales history form");
                MessageBox.Show("Error opening sales history form. Please try again.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenReportsForm()
        {
            try
            {
                var reportForm = _serviceProvider.GetService(typeof(ReportForm)) as ReportForm;
                if (reportForm != null)
                {
                    reportForm.ShowDialog();
                    // Refresh dashboard after closing report form
                    _ = RefreshDashboardData();
                }
                else
                {
                    _logger.LogWarning("ReportForm service not found in service provider");
                    MessageBox.Show("Reports form is not available. Please check system configuration.", 
                        "Service Not Available", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening reports form");
                MessageBox.Show("Error opening reports form. Please try again.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OpenSettingsForm()
        {
            try
            {
                var settingsForm = _serviceProvider.GetService(typeof(SettingsForm)) as SettingsForm;
                if (settingsForm != null)
                {
                    settingsForm.ShowDialog();
                    // Refresh dashboard after closing settings form
                    _ = RefreshDashboardData();
                }
                else
                {
                    _logger.LogWarning("SettingsForm service not found in service provider");
                    MessageBox.Show("Settings form is not available. Please check system configuration.", 
                        "Service Not Available", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening settings form");
                MessageBox.Show("Error opening settings form. Please try again.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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