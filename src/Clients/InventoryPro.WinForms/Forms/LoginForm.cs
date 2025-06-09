using Microsoft.Extensions.Logging;
using InventoryPro.WinForms.Services;
using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Forms
    {
    /// <summary>
    /// Login form for user authentication
    /// </summary>
    public partial class LoginForm : Form
        {
        private readonly ILogger<LoginForm> _logger;
        private readonly IApiService _apiService;

        // Controls
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCancel;
        private CheckBox chkRememberMe;
        private Label lblStatus;
        private PictureBox picLogo;
        private Panel pnlLogin;

        public event EventHandler? LoginSuccessful;

        public LoginForm(ILogger<LoginForm> logger, IApiService apiService)
            {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            InitializeComponent();
            InitializeFormStyle();
            }

        private void InitializeComponent()
            {
            this.Text = "InventoryPro - Login";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Main panel
            pnlLogin = new Panel
                {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40)
                };

            // Logo placeholder
            picLogo = new PictureBox
                {
                Size = new Size(150, 150),
                Location = new Point(125, 40),
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.CenterImage
                };

            // Username
            var lblUsername = new Label
                {
                Text = "Username:",
                Location = new Point(40, 220),
                Size = new Size(80, 25)
                };

            txtUsername = new TextBox
                {
                Location = new Point(40, 245),
                Size = new Size(320, 30),
                Font = new Font("Segoe UI", 10F)
                };

            // Password
            var lblPassword = new Label
                {
                Text = "Password:",
                Location = new Point(40, 285),
                Size = new Size(80, 25)
                };

            txtPassword = new TextBox
                {
                Location = new Point(40, 310),
                Size = new Size(320, 30),
                Font = new Font("Segoe UI", 10F),
                UseSystemPasswordChar = true
                };

            // Remember me
            chkRememberMe = new CheckBox
                {
                Text = "Remember me",
                Location = new Point(40, 350),
                Size = new Size(150, 25)
                };

            // Buttons
            btnLogin = new Button
                {
                Text = "Login",
                Location = new Point(40, 390),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
            btnLogin.Click += BtnLogin_Click;

            btnCancel = new Button
                {
                Text = "Cancel",
                Location = new Point(210, 390),
                Size = new Size(150, 40),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F)
                };
            btnCancel.Click += (s, e) => Application.Exit();

            // Status label
            lblStatus = new Label
                {
                Location = new Point(40, 440),
                Size = new Size(320, 25),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter
                };

            // Add controls
            pnlLogin.Controls.AddRange(new Control[] {
                picLogo, lblUsername, txtUsername, lblPassword, txtPassword,
                chkRememberMe, btnLogin, btnCancel, lblStatus
            });

            this.Controls.Add(pnlLogin);

            // Set tab order
            txtUsername.TabIndex = 0;
            txtPassword.TabIndex = 1;
            chkRememberMe.TabIndex = 2;
            btnLogin.TabIndex = 3;
            btnCancel.TabIndex = 4;

            // Set accept and cancel buttons
            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;
            }

        private void InitializeFormStyle()
            {
            // Add some styling
            btnLogin.FlatAppearance.BorderSize = 0;
            btnCancel.FlatAppearance.BorderSize = 0;

            // Add hover effects
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(0, 102, 184);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(0, 122, 204);

            btnCancel.MouseEnter += (s, e) => btnCancel.BackColor = Color.DarkGray;
            btnCancel.MouseLeave += (s, e) => btnCancel.BackColor = Color.LightGray;

            // Default values for testing
#if DEBUG
            txtUsername.Text = "admin";
            txtPassword.Text = "admin123";
#endif
            }

        private async void BtnLogin_Click(object? sender, EventArgs e)
            {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                lblStatus.Text = "Please enter username and password";
                return;
                }

            try
                {
                // Disable controls during login
                SetControlsEnabled(false);
                lblStatus.Text = "Logging in...";
                lblStatus.ForeColor = Color.Blue;

                var loginRequest = new LoginRequestDto
                    {
                    Username = txtUsername.Text,
                    Password = txtPassword.Text,
                    RememberMe = chkRememberMe.Checked
                    };

                var response = await _apiService.LoginAsync(loginRequest);

                if (response.Success && response.Data != null)
                    {
                    _logger.LogInformation("Login successful for user: {Username}", txtUsername.Text);

                    // Raise login successful event
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);

                    // Close the login form
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    }
                else
                    {
                    lblStatus.Text = response.Message ?? "Login failed";
                    lblStatus.ForeColor = Color.Red;
                    _logger.LogWarning("Login failed for user: {Username}", txtUsername.Text);
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during login");
                lblStatus.Text = "Error connecting to server";
                lblStatus.ForeColor = Color.Red;
                }
            finally
                {
                SetControlsEnabled(true);
                }
            }

        private void SetControlsEnabled(bool enabled)
            {
            txtUsername.Enabled = enabled;
            txtPassword.Enabled = enabled;
            chkRememberMe.Enabled = enabled;
            btnLogin.Enabled = enabled;
            btnCancel.Enabled = enabled;
            }

        protected override void OnFormClosing(FormClosingEventArgs e)
            {
            if (this.DialogResult != DialogResult.OK)
                {
                // User closed the form without logging in
                Application.Exit();
                }
            base.OnFormClosing(e);
            }
        }
    }