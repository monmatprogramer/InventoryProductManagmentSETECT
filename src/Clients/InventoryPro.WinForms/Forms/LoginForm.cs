using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using InventoryPro.WinForms.Services;
using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Forms
{
    /// <summary>
    /// Modern login form with professional design
    /// Features gradient background, custom controls, and animations
    /// </summary>
    public partial class LoginForm : Form
    {
        private readonly ILogger<LoginForm> _logger;
        private readonly IAuthService _authService;
        private readonly IApiService _apiService;

        // Form controls
        private Panel pnlMain;
        private Panel pnlLeft;
        private Panel pnlRight;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblUsername;
        private Label lblPassword;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCancel;
        private CheckBox chkRememberMe;
        private LinkLabel lnkForgotPassword;
        private PictureBox pbLogo;
        private Label lblError;
        private ProgressBar progressBar;

        // Loading overlay
        private Panel pnlLoading;
        private Label lblLoading;

        public LoginForm(ILogger<LoginForm> logger, IAuthService authService, IApiService apiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Initialize non-nullable fields
            pnlMain = new Panel();
            pnlLeft = new Panel();
            pnlRight = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            lblUsername = new Label();
            lblPassword = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            btnCancel = new Button();
            chkRememberMe = new CheckBox();
            lnkForgotPassword = new LinkLabel();
            pbLogo = new PictureBox();
            lblError = new Label();
            progressBar = new ProgressBar();
            pnlLoading = new Panel();
            lblLoading = new Label();

            InitializeComponent();
            SetupForm();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form settings
            this.Text = "InventoryPro - Login";
            this.Size = new Size(900, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            // Main panel
            pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Left panel with gradient background
            pnlLeft = new Panel
            {
                Width = 350,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(41, 128, 185)
            };

            // Company logo placeholder
            pbLogo = new PictureBox
            {
                Size = new Size(120, 120),
                Location = new Point(115, 80),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            // Draw a simple logo placeholder
            var logo = new Bitmap(120, 120);
            using (var g = Graphics.FromImage(logo))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var brush = new SolidBrush(Color.White))
                {
                    g.FillEllipse(brush, 10, 10, 100, 100);
                }
                using (var font = new Font("Segoe UI", 36, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(41, 128, 185)))
                {
                    var text = "IP";
                    var textSize = g.MeasureString(text, font);
                    g.DrawString(text, font, brush,
                        (120 - textSize.Width) / 2,
                        (120 - textSize.Height) / 2);
                }
            }
            pbLogo.Image = logo;

            // Title label
            lblTitle = new Label
            {
                Text = "InventoryPro",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(50, 220),
                Size = new Size(250, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Subtitle label
            lblSubtitle = new Label
            {
                Text = "Professional Inventory\nManagement System",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(50, 270),
                Size = new Size(250, 50),
                TextAlign = ContentAlignment.TopCenter
            };

            pnlLeft.Controls.AddRange(new Control[] { pbLogo, lblTitle, lblSubtitle });

            // Right panel
            pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(50)
            };

            // Login form title
            var lblFormTitle = new Label
            {
                Text = "Welcome Back!",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(50, 50),
                Size = new Size(250, 35),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblFormSubtitle = new Label
            {
                Text = "Please login to your account",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(128, 128, 128),
                Location = new Point(50, 90),
                Size = new Size(250, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Username label and textbox
            lblUsername = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(50, 140),
                Size = new Size(100, 20)
            };

            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(50, 165),
                Size = new Size(350, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtUsername.Text = "admin"; // Default for testing

            // Password label and textbox
            lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(50, 210),
                Size = new Size(100, 20)
            };

            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(50, 235),
                Size = new Size(350, 30),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            txtPassword.Text = "admin123"; // Default for testing

            // Remember me checkbox
            chkRememberMe = new CheckBox
            {
                Text = "Remember me",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(50, 280),
                Size = new Size(120, 20)
            };

            // Forgot password link
            lnkForgotPassword = new LinkLabel
            {
                Text = "Forgot Password?",
                Font = new Font("Segoe UI", 9),
                Location = new Point(290, 280),
                Size = new Size(110, 20),
                TextAlign = ContentAlignment.MiddleRight,
                LinkColor = Color.FromArgb(41, 128, 185)
            };

            // Error label (hidden by default)
            lblError = new Label
            {
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                Location = new Point(50, 310),
                Size = new Size(350, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false
            };

            // Login button
            btnLogin = new Button
            {
                Text = "LOGIN",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(41, 128, 185),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(50, 360),
                Size = new Size(170, 45),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Cancel button
            btnCancel = new Button
            {
                Text = "CANCEL",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(41, 128, 185),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(230, 360),
                Size = new Size(170, 45),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(41, 128, 185);
            btnCancel.FlatAppearance.BorderSize = 2;
            btnCancel.Click += BtnCancel_Click;

            // Progress bar (hidden by default)
            progressBar = new ProgressBar
            {
                Location = new Point(50, 420),
                Size = new Size(350, 5),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // Loading overlay
            pnlLoading = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(200, 255, 255, 255),
                Visible = false
            };

            lblLoading = new Label
            {
                Text = "Logging in...",
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            pnlLoading.Controls.Add(lblLoading);

            // Add controls to right panel
            pnlRight.Controls.AddRange(new Control[] {
                lblFormTitle, lblFormSubtitle,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                chkRememberMe, lnkForgotPassword,
                lblError, btnLogin, btnCancel, progressBar
            });

            // Add panels to main panel
            pnlMain.Controls.Add(pnlRight);
            pnlMain.Controls.Add(pnlLeft);

            // Add loading overlay last so it's on top
            this.Controls.Add(pnlLoading);
            this.Controls.Add(pnlMain);

            this.ResumeLayout(false);
        }

        private void SetupForm()
        {
            // Add custom paint for gradient background
            pnlLeft.Paint += PnlLeft_Paint;

            // Add hover effects for buttons
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(52, 152, 219);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(41, 128, 185);

            btnCancel.MouseEnter += (s, e) => btnCancel.BackColor = Color.FromArgb(240, 240, 240);
            btnCancel.MouseLeave += (s, e) => btnCancel.BackColor = Color.White;

            // Set focus to username textbox
            this.Load += (s, e) => txtUsername.Focus();

            // Handle Enter key
            txtUsername.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    txtPassword.Focus();
                    e.SuppressKeyPress = true;
                }
            };

            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnLogin_Click(s, e);
                    e.SuppressKeyPress = true;
                }
            };

            // Handle forgot password link
            lnkForgotPassword.LinkClicked += (s, e) =>
            {
                MessageBox.Show("Please contact your system administrator to reset your password.",
                    "Forgot Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void PnlLeft_Paint(object? sender, PaintEventArgs e)
        {
            // Create gradient background
            using (LinearGradientBrush brush = new LinearGradientBrush(
                pnlLeft.ClientRectangle,
                Color.FromArgb(41, 128, 185),
                Color.FromArgb(109, 213, 250),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, pnlLeft.ClientRectangle);
            }
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            // Clear any previous errors
            lblError.Visible = false;
            lblError.Text = "";

            // Validate input
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("Please enter your username.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowError("Please enter your password.");
                txtPassword.Focus();
                return;
            }

            // Show loading state
            SetLoadingState(true);

            try
            {
                var loginRequest = new LoginRequestDto
                {
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text,
                    RememberMe = chkRememberMe.Checked
                };

                _logger.LogInformation("Attempting login for user: {Username}", loginRequest.Username);

                var response = await _apiService.LoginAsync(loginRequest);

                if (response.Success && response.Data != null)
                {
                    _logger.LogInformation("Login successful for user: {Username}", loginRequest.Username);

                    // Close the login form with success result
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError(response.Message ?? "Invalid username or password.");
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ShowError("Unable to connect to the server. Please check your connection and try again.");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        private void SetLoadingState(bool isLoading)
        {
            pnlLoading.Visible = isLoading;
            progressBar.Visible = isLoading;
            btnLogin.Enabled = !isLoading;
            btnCancel.Enabled = !isLoading;
            txtUsername.Enabled = !isLoading;
            txtPassword.Enabled = !isLoading;
            chkRememberMe.Enabled = !isLoading;

            if (isLoading)
            {
                this.Cursor = Cursors.WaitCursor;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }
        }

        // Allow form to be dragged from the left panel
        private Point mouseLocation;
        private bool isMouseDown = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Add drag functionality
            pnlLeft.MouseDown += (s, args) =>
            {
                isMouseDown = true;
                mouseLocation = args.Location;
            };

            pnlLeft.MouseMove += (s, args) =>
            {
                if (isMouseDown)
                {
                    this.Location = new Point(
                        this.Location.X + args.Location.X - mouseLocation.X,
                        this.Location.Y + args.Location.Y - mouseLocation.Y);
                }
            };

            pnlLeft.MouseUp += (s, args) =>
            {
                isMouseDown = false;
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw a thin border around the form
            using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    }
}