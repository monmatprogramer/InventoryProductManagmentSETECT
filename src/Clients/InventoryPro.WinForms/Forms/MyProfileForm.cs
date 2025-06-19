using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using InventoryPro.Shared.DTOs;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Modern My Profile form with contemporary UI/UX design
    /// </summary>
    public partial class MyProfileForm : Form
    {
        private readonly ILogger<MyProfileForm> _logger;
        private readonly IApiService _apiService;
        
        // Main layout panels
        private Panel pnlMain;
        private Panel pnlHeader;
        private Panel pnlContent;
        private Panel pnlFooter;
        
        // Header controls
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel pnlAvatar;
        private Button btnChangeAvatar;
        private Image? _avatarImage;
        
        // Profile sections
        private Panel pnlPersonalInfo;
        private Panel pnlAccountInfo;
        private Panel pnlSecurityInfo;
        
        // Personal Information controls
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private DateTimePicker dtpBirthDate;
        private ComboBox cboGender;
        private TextBox txtAddress;
        private TextBox txtCity;
        private TextBox txtPostalCode;
        private ComboBox cboCountry;
        
        // Account Information controls
        private TextBox txtUsername;
        private Label lblUsernameReadonly;
        private ComboBox cboRole;
        private Label lblCreatedDate;
        private Label lblLastLogin;
        private CheckBox chkIsActive;
        
        // Security controls
        private Button btnChangePassword;
        private Button btnTwoFactorAuth;
        private CheckBox chkEmailNotifications;
        private CheckBox chkSmsNotifications;
        
        // Footer buttons
        private Button btnSave;
        private Button btnCancel;
        private Button btnReset;
        
        // Colors for modern theme
        private readonly Color _primaryColor = Color.FromArgb(0, 123, 255);
        private readonly Color _successColor = Color.FromArgb(40, 167, 69);
        private readonly Color _warningColor = Color.FromArgb(255, 193, 7);
        private readonly Color _dangerColor = Color.FromArgb(220, 53, 69);
        private readonly Color _lightGray = Color.FromArgb(248, 249, 250);
        private readonly Color _borderColor = Color.FromArgb(220, 224, 229);
        
        public MyProfileForm(ILogger<MyProfileForm> logger, IApiService apiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _avatarImage = null;
            btnCancel = new Button();
            btnReset = new Button();
            btnSave = new Button();
            btnChangeAvatar = new Button();
            btnChangePassword = new Button();
            btnTwoFactorAuth = new Button();
            pnlAvatar = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            pnlMain = new Panel();
            pnlHeader = new Panel();
            pnlContent = new Panel();
            pnlFooter = new Panel();
            pnlPersonalInfo = new Panel();
            pnlAccountInfo = new Panel();
            pnlSecurityInfo = new Panel();
            txtFirstName = new TextBox();
            txtLastName = new TextBox();
            txtEmail = new TextBox();
            txtPhone = new TextBox();
            dtpBirthDate = new DateTimePicker();
            cboGender = new ComboBox();
            txtAddress = new TextBox();
            txtCity = new TextBox();
            txtPostalCode = new TextBox();
            cboCountry = new ComboBox();
            txtUsername = new TextBox();
            lblUsernameReadonly = new Label();
            cboRole = new ComboBox();
            lblCreatedDate = new Label();
            lblLastLogin = new Label();
            chkIsActive = new CheckBox();
            chkEmailNotifications = new CheckBox();
            chkSmsNotifications = new CheckBox();



            InitializeComponent();
            LoadUserProfile();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "👤 My Profile - InventoryPro";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            // Enable double buffering for smooth rendering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            
            CreateMainLayout();
            CreateHeader();
            CreateContent();
            CreateFooter();
            
            this.ResumeLayout(false);
        }
        
        private void CreateMainLayout()
        {
            pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(20)
            };
            
            this.Controls.Add(pnlMain);
        }
        
        private void CreateHeader()
        {
            pnlHeader = new Panel
            {
                Height = 120,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };
            
            // Header styling with gradient
            pnlHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Gradient background
                using (var brush = new LinearGradientBrush(
                    pnlHeader.ClientRectangle,
                    Color.White,
                    _lightGray,
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, pnlHeader.ClientRectangle);
                }
                
                // Bottom border
                using (var pen = new Pen(_borderColor, 2))
                {
                    g.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
                }
            };
            
            // Avatar panel
            pnlAvatar = new Panel
            {
                Width = 80,
                Height = 80,
                Left = 30,
                Top = 20,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            
            pnlAvatar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Draw avatar circle
                var avatarRect = new Rectangle(0, 0, 80, 80);
                
                if (_avatarImage != null)
                {
                    // Draw the selected image
                    using (var brush = new TextureBrush(_avatarImage))
                    {
                        g.FillEllipse(brush, avatarRect);
                    }
                }
                else
                {
                    // Draw default gradient background
                    using (var brush = new LinearGradientBrush(
                        avatarRect, 
                        _primaryColor, 
                        Color.FromArgb(Math.Max(0, _primaryColor.R - 30), 
                                      Math.Max(0, _primaryColor.G - 30), 
                                      Math.Max(0, _primaryColor.B - 30)),
                        LinearGradientMode.ForwardDiagonal))
                    {
                        g.FillEllipse(brush, avatarRect);
                    }
                    
                    // Draw user icon
                    using (var iconBrush = new SolidBrush(Color.White))
                    using (var iconFont = new Font("Segoe UI", 24, FontStyle.Bold))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("👤", iconFont, iconBrush, avatarRect, sf);
                    }
                }
                
                // Draw border
                using (var pen = new Pen(Color.White, 3))
                {
                    g.DrawEllipse(pen, new Rectangle(1, 1, 78, 78));
                }
            };
            
            pnlAvatar.Click += (s, e) => ChangeAvatar();
            
            // Title
            lblTitle = new Label
            {
                Text = "My Profile",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(130, 10),
                Size = new Size(250, 55),
                BackColor = Color.Transparent
            };
            
            // Subtitle
            lblSubtitle = new Label
            {
                Text = "Manage your personal information and account settings",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(130, 60),
                Size = new Size(400, 25),
                BackColor = Color.Transparent
            };
            
            // Change avatar button
            btnChangeAvatar = new Button
            {
                Text = "Change Photo",
                Size = new Size(135, 35),
                Location = new Point(pnlHeader.Width - 150, 45),
                BackColor = _primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            
            btnChangeAvatar.FlatAppearance.BorderSize = 0;
            btnChangeAvatar.Paint += (s, e) => DrawRoundedButton(e.Graphics, btnChangeAvatar, _primaryColor);
            btnChangeAvatar.Click += (s, e) => ChangeAvatar();
            
            pnlHeader.Controls.AddRange(new Control[] { pnlAvatar, lblTitle, lblSubtitle, btnChangeAvatar });
            pnlMain.Controls.Add(pnlHeader);
        }
        
        private void CreateContent()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 10),
                AutoScroll = true
            };
            
            CreatePersonalInfoSection();
            CreateAccountInfoSection();
            CreateSecuritySection();
            
            pnlMain.Controls.Add(pnlContent);
        }
        
        private void CreatePersonalInfoSection()
        {
            pnlPersonalInfo = new Panel
            {
                Height = 320,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(30)
            };
            
            // Section styling
            pnlPersonalInfo.Paint += (s, e) => DrawSection(e.Graphics, pnlPersonalInfo);
            
            // Section title
            var lblPersonalTitle = new Label
            {
                Text = "👤 Personal Information",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(0, 0),
                Size = new Size(300, 30),
                BackColor = Color.Transparent
            };
            
            // First Name
            CreateLabelAndTextBox(pnlPersonalInfo, "First Name", out txtFirstName, 0, 50);
            
            // Last Name
            CreateLabelAndTextBox(pnlPersonalInfo, "Last Name", out txtLastName, 300, 50);
            
            // Email
            CreateLabelAndTextBox(pnlPersonalInfo, "Email Address", out txtEmail, 0, 110);
            txtEmail.ReadOnly = true;
            txtEmail.BackColor = _lightGray;
            
            // Phone
            CreateLabelAndTextBox(pnlPersonalInfo, "Phone Number", out txtPhone, 300, 110);
            
            // Birth Date
            var lblBirthDate = new Label
            {
                Text = "Date of Birth",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(0, 170),
                Size = new Size(100, 20),
                BackColor = Color.Transparent
            };
            
            dtpBirthDate = new DateTimePicker
            {
                Location = new Point(0, 195),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short
            };
            
            // Gender
            var lblGender = new Label
            {
                Text = "Gender",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(250, 170),
                Size = new Size(100, 20),
                BackColor = Color.Transparent
            };
            
            cboGender = new ComboBox
            {
                Location = new Point(250, 195),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboGender.Items.AddRange(new[] { "Male", "Female", "Other", "Prefer not to say" });
            
            // Address
            CreateLabelAndTextBox(pnlPersonalInfo, "Address", out txtAddress, 0, 240);
            txtAddress.Size = new Size(400, 35);
            
            pnlPersonalInfo.Controls.AddRange(new Control[] {
                lblPersonalTitle, lblBirthDate, dtpBirthDate, lblGender, cboGender
            });
            
            pnlContent.Controls.Add(pnlPersonalInfo);
        }
        
        private void CreateAccountInfoSection()
        {
            pnlAccountInfo = new Panel
            {
                Height = 200,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(30)
            };
            
            pnlAccountInfo.Paint += (s, e) => DrawSection(e.Graphics, pnlAccountInfo);
            
            // Section title
            var lblAccountTitle = new Label
            {
                Text = "🏢 Account Information",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(0, 0),
                Size = new Size(300, 30),
                BackColor = Color.Transparent
            };
            
            // Username (readonly)
            var lblUsername = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(0, 50),
                Size = new Size(100, 20),
                BackColor = Color.Transparent
            };
            
            lblUsernameReadonly = new Label
            {
                Text = "admin",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(0, 75),
                Size = new Size(200, 25),
                BackColor = _lightGray,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            
            // Role
            var lblRole = new Label
            {
                Text = "Role",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(250, 50),
                Size = new Size(100, 20),
                BackColor = Color.Transparent
            };
            
            cboRole = new ComboBox
            {
                Location = new Point(250, 75),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            cboRole.Items.AddRange(new[] { "Administrator", "Manager", "Employee", "Viewer" });
            
            // Created Date
            var lblCreatedLabel = new Label
            {
                Text = "Member Since",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(0, 120),
                Size = new Size(100, 20),
                BackColor = Color.Transparent
            };
            
            lblCreatedDate = new Label
            {
                Text = DateTime.Now.AddYears(-1).ToString("MMMM dd, yyyy"),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(0, 145),
                Size = new Size(200, 25),
                BackColor = Color.Transparent
            };
            
            // Last Login
            var lblLastLoginLabel = new Label
            {
                Text = "Last Login",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(250, 120),
                Size = new Size(100, 20),
                BackColor = Color.Transparent
            };
            
            lblLastLogin = new Label
            {
                Text = DateTime.Now.AddHours(-2).ToString("MMM dd, yyyy HH:mm"),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(250, 145),
                Size = new Size(200, 25),
                BackColor = Color.Transparent
            };
            
            pnlAccountInfo.Controls.AddRange(new Control[] {
                lblAccountTitle, lblUsername, lblUsernameReadonly, lblRole, cboRole,
                lblCreatedLabel, lblCreatedDate, lblLastLoginLabel, lblLastLogin
            });
            
            pnlContent.Controls.Add(pnlAccountInfo);
        }
        
        private void CreateSecuritySection()
        {
            pnlSecurityInfo = new Panel
            {
                Height = 180,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(30)
            };
            
            pnlSecurityInfo.Paint += (s, e) => DrawSection(e.Graphics, pnlSecurityInfo);
            
            // Section title
            var lblSecurityTitle = new Label
            {
                Text = "🔒 Security & Notifications",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(0, 0),
                Size = new Size(300, 30),
                BackColor = Color.Transparent
            };
            
            // Change Password button
            btnChangePassword = new Button
            {
                Text = "🔑 Change Password",
                Size = new Size(180, 40),
                Location = new Point(0, 50),
                BackColor = _warningColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnChangePassword.FlatAppearance.BorderSize = 0;
            btnChangePassword.Paint += (s, e) => DrawRoundedButton(e.Graphics, btnChangePassword, _warningColor);
            btnChangePassword.Click += BtnChangePassword_Click;
            
            // Two Factor Auth button
            btnTwoFactorAuth = new Button
            {
                Text = "🛡️ Enable 2FA",
                Size = new Size(150, 40),
                Location = new Point(200, 50),
                BackColor = _successColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTwoFactorAuth.FlatAppearance.BorderSize = 0;
            btnTwoFactorAuth.Paint += (s, e) => DrawRoundedButton(e.Graphics, btnTwoFactorAuth, _successColor);
            btnTwoFactorAuth.Click += BtnTwoFactorAuth_Click;
            
            // Email notifications
            chkEmailNotifications = new CheckBox
            {
                Text = "📧 Email Notifications",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(0, 110),
                Size = new Size(200, 25),
                BackColor = Color.Transparent,
                Checked = true
            };
            
            // SMS notifications
            chkSmsNotifications = new CheckBox
            {
                Text = "📱 SMS Notifications",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(250, 110),
                Size = new Size(200, 25),
                BackColor = Color.Transparent
            };
            
            pnlSecurityInfo.Controls.AddRange(new Control[] {
                lblSecurityTitle, btnChangePassword, btnTwoFactorAuth, 
                chkEmailNotifications, chkSmsNotifications
            });
            
            pnlContent.Controls.Add(pnlSecurityInfo);
        }
        
        private void CreateFooter()
        {
            pnlFooter = new Panel
            {
                Height = 80,
                Dock = DockStyle.Bottom,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };
            
            pnlFooter.Paint += (s, e) =>
            {
                using (var pen = new Pen(_borderColor, 2))
                {
                    e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
                }
            };
            
            // Save button
            btnSave = new Button
            {
                Text = "Save Changes",
                Size = new Size(140, 40),
                Location = new Point(pnlFooter.Width - 420, 20),
                BackColor = _successColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Paint += (s, e) => DrawRoundedButton(e.Graphics, btnSave, _successColor);
            btnSave.Click += BtnSave_Click;
            
            // Reset button
            btnReset = new Button
            {
                Text = "Reset",
                Size = new Size(100, 40),
                Location = new Point(pnlFooter.Width - 270, 20),
                BackColor = _warningColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Paint += (s, e) => DrawRoundedButton(e.Graphics, btnReset, _warningColor);
            btnReset.Click += BtnReset_Click;
            
            // Cancel button
            btnCancel = new Button
            {
                Text = "❌ Cancel",
                Size = new Size(100, 40),
                Location = new Point(pnlFooter.Width - 160, 20),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Paint += (s, e) => DrawRoundedButton(e.Graphics, btnCancel, Color.FromArgb(108, 117, 125));
            btnCancel.Click += BtnCancel_Click;
            
            pnlFooter.Controls.AddRange(new Control[] { btnSave, btnReset, btnCancel });
            pnlMain.Controls.Add(pnlFooter);
        }
        
        private void CreateLabelAndTextBox(Panel parent, string labelText, out TextBox textBox, int x, int y)
        {
            var label = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(x, y),
                Size = new Size(200, 20),
                BackColor = Color.Transparent
            };
            
            textBox = new TextBox
            {
                Location = new Point(x, y + 25),
                Size = new Size(250, 35),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Custom paint for modern textbox styling
            textBox.Paint += (s, e) =>
            {
                var tb = s as TextBox;
                if (tb != null && tb.BorderStyle == BorderStyle.FixedSingle)
                {
                    using (var pen = new Pen(_borderColor, 1))
                    {
                        e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, tb.Width - 1, tb.Height - 1));
                    }
                }
            };
            
            parent.Controls.AddRange(new Control[] { label, textBox });
        }
        
        private void DrawSection(Graphics g, Panel panel)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw shadow
            using (var shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
            {
                g.FillRoundedRectangle(shadowBrush, new Rectangle(3, 3, panel.Width - 3, panel.Height - 3), 10);
            }
            
            // Draw background
            using (var backgroundBrush = new SolidBrush(Color.White))
            {
                g.FillRoundedRectangle(backgroundBrush, new Rectangle(0, 0, panel.Width - 3, panel.Height - 3), 10);
            }
            
            // Draw border
            using (var borderPen = new Pen(_borderColor, 1))
            {
                g.DrawRoundedRectangle(borderPen, new Rectangle(0, 0, panel.Width - 4, panel.Height - 4), 10);
            }
        }
        
        private void DrawRoundedButton(Graphics g, Button button, Color backgroundColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, button.Width, button.Height);
            
            // Create gradient brush
            using (var brush = new LinearGradientBrush(rect, 
                backgroundColor, 
                Color.FromArgb(Math.Max(0, backgroundColor.R - 20), 
                              Math.Max(0, backgroundColor.G - 20), 
                              Math.Max(0, backgroundColor.B - 20)), 
                LinearGradientMode.Vertical))
            {
                g.FillRoundedRectangle(brush, rect, 8);
            }
            
            // Draw text
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var textBrush = new SolidBrush(button.ForeColor))
            {
                g.DrawString(button.Text, button.Font, textBrush, rect, sf);
            }
        }
        
        private void LoadUserProfile()
        {
            try
            {
                // Sample data - replace with actual API call
                txtFirstName.Text = "John";
                txtLastName.Text = "Doe";
                txtEmail.Text = "john.doe@inventoryPro.com";
                txtPhone.Text = "+1 (555) 123-4567";
                dtpBirthDate.Value = new DateTime(1990, 5, 15);
                cboGender.SelectedItem = "Male";
                txtAddress.Text = "123 Business Street";
                
                lblUsernameReadonly.Text = "john.doe";
                cboRole.SelectedItem = "Administrator";
                
                _logger.LogInformation("User profile loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                MessageBox.Show("Error loading profile data. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void ChangeAvatar()
        {
            using var openFileDialog = new OpenFileDialog
            {
                Title = "Select Profile Picture",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*",
                FilterIndex = 1
            };
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Dispose of the previous image if it exists
                    _avatarImage?.Dispose();
                    
                    // Load the new image and resize it to fit the avatar circle
                    using (var originalImage = Image.FromFile(openFileDialog.FileName))
                    {
                        _avatarImage = new Bitmap(80, 80);
                        using (var g = Graphics.FromImage(_avatarImage))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(originalImage, new Rectangle(0, 0, 80, 80));
                        }
                    }
                    
                    // Here you would upload the image to your server
                    MessageBox.Show("Profile picture updated successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    pnlAvatar.Invalidate(); // Refresh avatar display
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating profile picture");
                    MessageBox.Show("Error updating profile picture. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (ValidateForm())
                {
                    // Save profile changes via API
                    SaveProfileChanges();
                    
                    MessageBox.Show("Profile updated successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving profile");
                MessageBox.Show("Error saving profile changes. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnReset_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to reset all changes?", "Confirm Reset", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                LoadUserProfile();
            }
        }
        
        private void BtnCancel_Click(object? sender, EventArgs? e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        private void BtnChangePassword_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Change password functionality will be implemented in a future update.", 
                "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void BtnTwoFactorAuth_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Two-factor authentication setup will be implemented in a future update.", 
                "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Please enter your first name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Please enter your last name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please enter your email address.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }
            
            return true;
        }
        
        private void SaveProfileChanges()
        {
            // Here you would save the profile changes via API
            // For now, just log the operation
            _logger.LogInformation("Profile changes saved for user: {Username}", lblUsernameReadonly.Text);
        }

        protected override void Dispose(bool disposing)
            {
            if (disposing)
                {
                _avatarImage?.Dispose();
                }
            base.Dispose(disposing);
            }
        }
}