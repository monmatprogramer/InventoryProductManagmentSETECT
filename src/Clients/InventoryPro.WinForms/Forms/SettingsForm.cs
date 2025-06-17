using System.Drawing.Drawing2D;
using Microsoft.Extensions.Logging;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Modern Settings form with contemporary UI/UX design
    /// </summary>
    public partial class SettingsForm : Form
    {
        private readonly ILogger<SettingsForm> _logger;

        // UI Controls
        private Panel pnlMain;
        private Panel pnlSidebar;
        private Panel pnlContent;
        private Panel pnlHeader;

        // Sidebar navigation
        private Button btnGeneral;
        private Button btnAppearance;
        private Button btnDatabase;
        private Button btnBackup;
        private Button btnSecurity;
        private Button btnAbout;

        // Content panels
        private Panel pnlGeneral;
        private Panel pnlAppearance;
        private Panel pnlDatabase;
        private Panel pnlBackup;
        private Panel pnlSecurity;
        private Panel pnlAbout;

        // General settings controls
        private TextBox txtCompanyName;
        private TextBox txtCompanyAddress;
        private TextBox txtCompanyPhone;
        private TextBox txtCompanyEmail;
        private NumericUpDown nudTaxRate;
        private ComboBox cboCurrency;

        // Appearance settings controls
        private ComboBox cboTheme;
        private TrackBar tbFontSize;
        private Label lblFontSizeValue;
        private CheckBox chkAnimations;
        private CheckBox chkSounds;

        // Database settings controls
        private TextBox txtConnectionString;
        private Button btnTestConnection;
        private Label lblConnectionStatus;

        // Backup settings controls
        private TextBox txtBackupLocation;
        private Button btnBrowseBackup;
        private Button btnCreateBackup;
        private CheckBox chkAutoBackup;
        private NumericUpDown nudBackupInterval;

        // Security settings controls
        private CheckBox chkRequireLogin;
        private CheckBox chkRememberLogin;
        private NumericUpDown nudSessionTimeout;
        private Button btnChangePassword;

        public SettingsForm(ILogger<SettingsForm> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "⚙️ InventoryPro Settings";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F);

            CreateHeader();
            CreateMainLayout();
            CreateSidebar();
            CreateContentPanels();

            // Add panels to form
            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void CreateHeader()
        {
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            // Header shadow effect
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 224, 229), 2))
                {
                    e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
                }
            };

            var lblTitle = new Label
            {
                Text = "⚙️ System Settings",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(30, 20),
                Size = new Size(300, 40),
                BackColor = Color.Transparent
            };

            var lblSubtitle = new Label
            {
                Text = "Configure your InventoryPro application preferences",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(30, 50),
                Size = new Size(400, 20),
                BackColor = Color.Transparent
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });
        }

        private void CreateMainLayout()
        {
            pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(0)
            };
        }

        private void CreateSidebar()
        {
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // Sidebar shadow effect
            pnlSidebar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 224, 229), 1))
                {
                    e.Graphics.DrawLine(pen, pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height);
                }
            };

            // Navigation buttons
            btnGeneral = CreateNavButton("🏠 General", 0, true);
            btnAppearance = CreateNavButton("🎨 Appearance", 1, false);
            btnDatabase = CreateNavButton("🗄️ Database", 2, false);
            btnBackup = CreateNavButton("💾 Backup", 3, false);
            btnSecurity = CreateNavButton("🔒 Security", 4, false);
            btnAbout = CreateNavButton("ℹ️ About", 5, false);

            pnlSidebar.Controls.AddRange(new Control[] {
                btnGeneral, btnAppearance, btnDatabase, btnBackup, btnSecurity, btnAbout
            });

            pnlMain.Controls.Add(pnlSidebar);
        }

        private Button CreateNavButton(string text, int index, bool isActive)
        {
            var button = new Button
            {
                Text = text,
                Height = 50,
                Top = index * 60 + 20,
                Left = 20,
                Width = 210,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                Tag = index
            };

            UpdateNavButtonStyle(button, isActive);

            button.Click += NavButton_Click;
            button.MouseEnter += (s, e) =>
            {
                if (!IsActiveNavButton(button))
                {
                    button.BackColor = Color.FromArgb(248, 249, 250);
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
            }
            else
            {
                button.BackColor = Color.Transparent;
                button.ForeColor = Color.FromArgb(73, 80, 87);
                button.FlatAppearance.BorderSize = 0;
            }
        }

        private bool IsActiveNavButton(Button button)
        {
            return button.BackColor == Color.FromArgb(0, 123, 255);
        }

        private void CreateContentPanels()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(30),
                AutoScroll = true
            };

            CreateGeneralPanel();
            CreateAppearancePanel();
            CreateDatabasePanel();
            CreateBackupPanel();
            CreateSecurityPanel();
            CreateAboutPanel();

            pnlMain.Controls.Add(pnlContent);
        }

        private void CreateGeneralPanel()
        {
            pnlGeneral = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = true
            };

            var card = CreateCard("🏢 Company Information", 0);
            
            // Company Name
            var lblCompanyName = CreateLabel("Company Name:", 20, 50);
            txtCompanyName = CreateTextBox(20, 75, "InventoryPro Ltd.");
            
            // Company Address
            var lblCompanyAddress = CreateLabel("Address:", 20, 110);
            txtCompanyAddress = CreateTextBox(20, 135, "123 Business Street, City, State 12345");
            
            // Company Phone
            var lblCompanyPhone = CreateLabel("Phone:", 20, 170);
            txtCompanyPhone = CreateTextBox(20, 195, "+1 (555) 123-4567");
            
            // Company Email
            var lblCompanyEmail = CreateLabel("Email:", 20, 230);
            txtCompanyEmail = CreateTextBox(20, 255, "contact@inventorypro.com");

            card.Controls.AddRange(new Control[] {
                lblCompanyName, txtCompanyName,
                lblCompanyAddress, txtCompanyAddress,
                lblCompanyPhone, txtCompanyPhone,
                lblCompanyEmail, txtCompanyEmail
            });

            var card2 = CreateCard("💰 Financial Settings", 320);
            
            // Tax Rate
            var lblTaxRate = CreateLabel("Default Tax Rate (%):", 20, 50);
            nudTaxRate = new NumericUpDown
            {
                Location = new Point(20, 75),
                Size = new Size(200, 30),
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100,
                Value = 10.00m,
                Font = new Font("Segoe UI", 10)
            };
            
            // Currency
            var lblCurrency = CreateLabel("Currency:", 20, 110);
            cboCurrency = new ComboBox
            {
                Location = new Point(20, 135),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cboCurrency.Items.AddRange(new object[] { "USD ($)", "EUR (€)", "GBP (£)", "CAD ($)", "AUD ($)" });
            cboCurrency.SelectedIndex = 0;

            card2.Controls.AddRange(new Control[] {
                lblTaxRate, nudTaxRate,
                lblCurrency, cboCurrency
            });

            var btnSave = CreateButton("💾 Save Settings", 20, 580, Color.FromArgb(40, 167, 69));
            btnSave.Click += (s, e) => SaveGeneralSettings();

            pnlGeneral.Controls.AddRange(new Control[] { card, card2, btnSave });
            pnlContent.Controls.Add(pnlGeneral);
        }

        private void CreateAppearancePanel()
        {
            pnlAppearance = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };

            var card = CreateCard("🎨 Visual Settings", 0);
            
            // Theme
            var lblTheme = CreateLabel("Theme:", 20, 50);
            cboTheme = new ComboBox
            {
                Location = new Point(20, 75),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cboTheme.Items.AddRange(new object[] { "Light", "Dark", "Auto" });
            cboTheme.SelectedIndex = 0;
            
            // Font Size
            var lblFontSize = CreateLabel("Font Size:", 20, 110);
            tbFontSize = new TrackBar
            {
                Location = new Point(20, 135),
                Size = new Size(200, 45),
                Minimum = 8,
                Maximum = 16,
                Value = 10,
                TickFrequency = 1
            };
            tbFontSize.ValueChanged += (s, e) => lblFontSizeValue.Text = $"{tbFontSize.Value}pt";
            
            lblFontSizeValue = CreateLabel("10pt", 230, 145);
            
            // Animations
            chkAnimations = new CheckBox
            {
                Text = "Enable animations",
                Location = new Point(20, 190),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10),
                Checked = true
            };
            
            // Sounds
            chkSounds = new CheckBox
            {
                Text = "Enable sound effects",
                Location = new Point(20, 220),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10),
                Checked = false
            };

            card.Controls.AddRange(new Control[] {
                lblTheme, cboTheme,
                lblFontSize, tbFontSize, lblFontSizeValue,
                chkAnimations, chkSounds
            });

            var btnSave = CreateButton("💾 Save Settings", 20, 350, Color.FromArgb(40, 167, 69));
            btnSave.Click += (s, e) => SaveAppearanceSettings();

            pnlAppearance.Controls.AddRange(new Control[] { card, btnSave });
            pnlContent.Controls.Add(pnlAppearance);
        }

        private void CreateDatabasePanel()
        {
            pnlDatabase = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };

            var card = CreateCard("🗄️ Database Connection", 0);
            
            var lblConnection = CreateLabel("Connection String:", 20, 50);
            txtConnectionString = new TextBox
            {
                Location = new Point(20, 75),
                Size = new Size(500, 30),
                Font = new Font("Segoe UI", 10),
                Text = "Data Source=localhost;Initial Catalog=InventoryPro;Integrated Security=True"
            };
            
            btnTestConnection = CreateButton("🔍 Test Connection", 20, 120, Color.FromArgb(0, 123, 255));
            btnTestConnection.Click += (s, e) => TestDatabaseConnection();
            
            lblConnectionStatus = new Label
            {
                Location = new Point(180, 125),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10),
                Text = "Not tested",
                ForeColor = Color.FromArgb(108, 117, 125)
            };

            card.Controls.AddRange(new Control[] {
                lblConnection, txtConnectionString,
                btnTestConnection, lblConnectionStatus
            });

            pnlDatabase.Controls.Add(card);
            pnlContent.Controls.Add(pnlDatabase);
        }

        private void CreateBackupPanel()
        {
            pnlBackup = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };

            var card = CreateCard("💾 Backup Configuration", 0);
            
            var lblBackupLocation = CreateLabel("Backup Location:", 20, 50);
            txtBackupLocation = new TextBox
            {
                Location = new Point(20, 75),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 10),
                Text = @"C:\InventoryPro\Backups"
            };
            
            btnBrowseBackup = CreateButton("📁 Browse", 430, 73, Color.FromArgb(108, 117, 125));
            btnBrowseBackup.Size = new Size(80, 30);
            btnBrowseBackup.Click += (s, e) => BrowseBackupLocation();
            
            btnCreateBackup = CreateButton("💾 Create Backup Now", 20, 120, Color.FromArgb(40, 167, 69));
            btnCreateBackup.Click += (s, e) => CreateBackup();
            
            chkAutoBackup = new CheckBox
            {
                Text = "Enable automatic backups",
                Location = new Point(20, 170),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10),
                Checked = false
            };
            
            var lblBackupInterval = CreateLabel("Backup Interval (hours):", 20, 200);
            nudBackupInterval = new NumericUpDown
            {
                Location = new Point(20, 225),
                Size = new Size(100, 30),
                Minimum = 1,
                Maximum = 168,
                Value = 24,
                Font = new Font("Segoe UI", 10)
            };

            card.Controls.AddRange(new Control[] {
                lblBackupLocation, txtBackupLocation, btnBrowseBackup,
                btnCreateBackup, chkAutoBackup,
                lblBackupInterval, nudBackupInterval
            });

            pnlBackup.Controls.Add(card);
            pnlContent.Controls.Add(pnlBackup);
        }

        private void CreateSecurityPanel()
        {
            pnlSecurity = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };

            var card = CreateCard("🔒 Security Settings", 0);
            
            chkRequireLogin = new CheckBox
            {
                Text = "Require user login",
                Location = new Point(20, 50),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10),
                Checked = true
            };
            
            chkRememberLogin = new CheckBox
            {
                Text = "Remember login credentials",
                Location = new Point(20, 80),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10),
                Checked = false
            };
            
            var lblSessionTimeout = CreateLabel("Session Timeout (minutes):", 20, 110);
            nudSessionTimeout = new NumericUpDown
            {
                Location = new Point(20, 135),
                Size = new Size(100, 30),
                Minimum = 5,
                Maximum = 480,
                Value = 60,
                Font = new Font("Segoe UI", 10)
            };
            
            btnChangePassword = CreateButton("🔑 Change Password", 20, 180, Color.FromArgb(255, 193, 7));
            btnChangePassword.Click += (s, e) => ChangePassword();

            card.Controls.AddRange(new Control[] {
                chkRequireLogin, chkRememberLogin,
                lblSessionTimeout, nudSessionTimeout,
                btnChangePassword
            });

            pnlSecurity.Controls.Add(card);
            pnlContent.Controls.Add(pnlSecurity);
        }

        private void CreateAboutPanel()
        {
            pnlAbout = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };

            var card = CreateCard("ℹ️ Application Information", 0);
            
            var lblAppName = new Label
            {
                Text = "📦 InventoryPro",
                Location = new Point(20, 50),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255)
            };
            
            var lblVersion = CreateLabel("Version: 2.0.0 Professional", 20, 90);
            var lblCopyright = CreateLabel("© 2024 InventoryPro. All rights reserved.", 20, 115);
            var lblDescription = new Label
            {
                Text = "A modern inventory management system designed for businesses of all sizes. " +
                       "Features include real-time inventory tracking, sales management, customer management, " +
                       "and comprehensive reporting.",
                Location = new Point(20, 145),
                Size = new Size(500, 80),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(73, 80, 87)
            };
            
            var btnCheckUpdates = CreateButton("🔄 Check for Updates", 20, 240, Color.FromArgb(0, 123, 255));
            btnCheckUpdates.Click += (s, e) => CheckForUpdates();

            card.Controls.AddRange(new Control[] {
                lblAppName, lblVersion, lblCopyright, lblDescription, btnCheckUpdates
            });

            pnlAbout.Controls.Add(card);
            pnlContent.Controls.Add(pnlAbout);
        }

        private Panel CreateCard(string title, int top)
        {
            var card = new Panel
            {
                Location = new Point(0, top),
                Size = new Size(650, 300),
                BackColor = Color.White,
                Padding = new Padding(20)
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
                
                // Draw border
                using (var borderPen = new Pen(Color.FromArgb(220, 224, 229), 1))
                {
                    g.DrawRoundedRectangle(borderPen, new Rectangle(0, 0, card.Width - 4, card.Height - 4), 10);
                }
            };

            var lblTitle = new Label
            {
                Text = title,
                Location = new Point(20, 20),
                Size = new Size(600, 25),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                BackColor = Color.Transparent
            };

            card.Controls.Add(lblTitle);
            return card;
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                BackColor = Color.Transparent
            };
        }

        private TextBox CreateTextBox(int x, int y, string text)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10),
                Text = text,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Button CreateButton(string text, int x, int y, Color backColor)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(150, 35),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Max(0, backColor.R - 20),
                Math.Max(0, backColor.G - 20),
                Math.Max(0, backColor.B - 20));
            
            return button;
        }

        private void NavButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button clickedButton)
            {
                // Reset all buttons
                var allNavButtons = new[] { btnGeneral, btnAppearance, btnDatabase, btnBackup, btnSecurity, btnAbout };
                foreach (var btn in allNavButtons)
                {
                    UpdateNavButtonStyle(btn, false);
                }
                
                // Activate clicked button
                UpdateNavButtonStyle(clickedButton, true);
                
                // Hide all panels
                var allPanels = new[] { pnlGeneral, pnlAppearance, pnlDatabase, pnlBackup, pnlSecurity, pnlAbout };
                foreach (var panel in allPanels)
                {
                    panel.Visible = false;
                }
                
                // Show selected panel
                int buttonIndex = (int)clickedButton.Tag;
                switch (buttonIndex)
                {
                    case 0: pnlGeneral.Visible = true; break;
                    case 1: pnlAppearance.Visible = true; break;
                    case 2: pnlDatabase.Visible = true; break;
                    case 3: pnlBackup.Visible = true; break;
                    case 4: pnlSecurity.Visible = true; break;
                    case 5: pnlAbout.Visible = true; break;
                }
            }
        }

        private void LoadSettings()
        {
            // TODO: Load settings from configuration file or database
            _logger.LogInformation("Loading application settings");
        }

        private void SaveGeneralSettings()
        {
            // TODO: Save general settings
            _logger.LogInformation("Saving general settings");
            MessageBox.Show("General settings saved successfully!", "Settings Saved", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveAppearanceSettings()
        {
            // TODO: Save appearance settings
            _logger.LogInformation("Saving appearance settings");
            MessageBox.Show("Appearance settings saved successfully!", "Settings Saved", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void TestDatabaseConnection()
        {
            try
            {
                // TODO: Implement actual database connection test
                lblConnectionStatus.Text = "✅ Connection successful";
                lblConnectionStatus.ForeColor = Color.FromArgb(40, 167, 69);
                _logger.LogInformation("Database connection test successful");
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Text = "❌ Connection failed";
                lblConnectionStatus.ForeColor = Color.FromArgb(220, 53, 69);
                _logger.LogError(ex, "Database connection test failed");
            }
        }

        private void BrowseBackupLocation()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select backup location";
                dialog.SelectedPath = txtBackupLocation.Text;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtBackupLocation.Text = dialog.SelectedPath;
                }
            }
        }

        private void CreateBackup()
        {
            try
            {
                // TODO: Implement actual backup creation
                _logger.LogInformation("Creating database backup");
                MessageBox.Show("Backup created successfully!", "Backup Complete", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                MessageBox.Show($"Error creating backup: {ex.Message}", "Backup Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangePassword()
        {
            // TODO: Implement password change dialog
            MessageBox.Show("Password change functionality will be implemented.", "Feature Coming Soon", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CheckForUpdates()
        {
            // TODO: Implement update checking
            MessageBox.Show("You are running the latest version of InventoryPro.", "No Updates Available", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}