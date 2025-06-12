using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace InventoryPro.WinForms.Forms
    {
    /// <summary>
    /// Modern login form for InventoryPro application
    /// Provides secure authentication with modern UI design
    /// </summary>
    public partial class LoginForm : Form
        {
        private readonly ILogger<LoginForm> _logger;
        private readonly IAuthService _authService;
        private readonly IApiService _apiService;
        private bool _isLoggingIn = false;

        public LoginForm(ILogger<LoginForm> logger, IAuthService authService, IApiService apiService)
            {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            InitializeComponent();
            ConfigureForm();
            }

        /// <summary>
        /// Configures additional form properties and events
        /// </summary>
        private void ConfigureForm()
            {
            // Set form properties
            this.KeyPreview = true;
            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;

            // Add event handlers
            this.KeyDown += LoginForm_KeyDown;
            txtUsername.KeyDown += TextBox_KeyDown;
            txtPassword.KeyDown += TextBox_KeyDown;

            // Set initial focus
            this.Shown += (s, e) => txtUsername.Focus();

            // Configure password textbox
            txtPassword.UseSystemPasswordChar = true;

            // Load saved username if remember me was checked
            LoadSavedCredentials();
            }

        /// <summary>
        /// Handles key down events for the form
        /// </summary>
        private void LoginForm_KeyDown(object? sender, KeyEventArgs e)
            {
            if (e.KeyCode == Keys.Enter && !_isLoggingIn)
                {
                e.Handled = true;
                _ = PerformLoginAsync();
                }
            else if (e.KeyCode == Keys.Escape)
                {
                e.Handled = true;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                }
            }

        /// <summary>
        /// Handles key down events for text boxes
        /// </summary>
        private void TextBox_KeyDown(object? sender, KeyEventArgs e)
            {
            if (e.KeyCode == Keys.Enter && !_isLoggingIn)
                {
                e.Handled = true;
                _ = PerformLoginAsync();
                }
            }

        /// <summary>
        /// Loads saved credentials if remember me was previously checked
        /// </summary>
        private void LoadSavedCredentials()
            {
            try
                {
                // Check if username was saved (simple implementation)
                var savedUsername = Properties.Settings.Default.SavedUsername;
                var rememberMe = Properties.Settings.Default.RememberMe;

                if (rememberMe && !string.IsNullOrEmpty(savedUsername))
                    {
                    txtUsername.Text = savedUsername;
                    chkRememberMe.Checked = true;
                    txtPassword.Focus(); // Focus on password if username is already filled
                    }
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Error loading saved credentials");
                }
            }

        /// <summary>
        /// Saves credentials if remember me is checked
        /// </summary>
        private void SaveCredentials()
            {
            try
                {
                if (chkRememberMe.Checked)
                    {
                    Properties.Settings.Default.SavedUsername = txtUsername.Text.Trim();
                    Properties.Settings.Default.RememberMe = true;
                    }
                else
                    {
                    Properties.Settings.Default.SavedUsername = string.Empty;
                    Properties.Settings.Default.RememberMe = false;
                    }
                Properties.Settings.Default.Save();
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Error saving credentials");
                }
            }

        /// <summary>
        /// Handles login button click event
        /// </summary>
        private async void BtnLogin_Click(object? sender, EventArgs e)
            {
            await PerformLoginAsync();
            }

        /// <summary>
        /// Performs the login operation
        /// </summary>
        private async Task PerformLoginAsync()
            {
            if (_isLoggingIn)
                return;

            // Validate input
            if (!ValidateInput())
                return;

            try
                {
                _isLoggingIn = true;
                SetLoginState(true);

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
                    // Save credentials if remember me is checked
                    SaveCredentials();

                    _logger.LogInformation("Login successful for user: {Username}", loginRequest.Username);

                    // Close the form with success result
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    }
                else
                    {
                    // Show error message
                    var errorMessage = !string.IsNullOrEmpty(response.Message)
                        ? response.Message
                        : "Invalid username or password";

                    ShowError(errorMessage);

                    // Clear password field
                    txtPassword.Clear();
                    txtPassword.Focus();

                    _logger.LogWarning("Login failed for user: {Username} - {Error}",
                        loginRequest.Username, errorMessage);
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during login");
                ShowError("Connection error. Please check your network connection and try again.");
                txtPassword.Clear();
                txtPassword.Focus();
                }
            finally
                {
                _isLoggingIn = false;
                SetLoginState(false);
                }
            }

        /// <summary>
        /// Validates user input
        /// </summary>
        private bool ValidateInput()
            {
            // Clear previous error
            lblError.Visible = false;

            // Validate username
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                ShowError("Please enter your username.");
                txtUsername.Focus();
                return false;
                }

            // Validate password
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                ShowError("Please enter your password.");
                txtPassword.Focus();
                return false;
                }

            // Validate username length
            if (txtUsername.Text.Trim().Length > 50)
                {
                ShowError("Username cannot exceed 50 characters.");
                txtUsername.Focus();
                return false;
                }

            // Validate password length
            if (txtPassword.Text.Length < 6)
                {
                ShowError("Password must be at least 6 characters long.");
                txtPassword.Focus();
                return false;
                }

            return true;
            }

        /// <summary>
        /// Shows an error message to the user
        /// </summary>
        private void ShowError(string message)
            {
            lblError.Text = message;
            lblError.Visible = true;

            // Optional: Flash the error label
            FlashErrorLabel();
            }

        /// <summary>
        /// Creates a subtle flash effect for the error label
        /// </summary>
        private async void FlashErrorLabel()
            {
            try
                {
                var originalColor = lblError.ForeColor;
                lblError.ForeColor = Color.FromArgb(220, 50, 47); // Bright red
                await Task.Delay(100);
                lblError.ForeColor = originalColor;
                }
            catch
                {
                // Ignore any errors in the flash effect
                }
            }

        /// <summary>
        /// Sets the login state (enables/disables controls during login)
        /// </summary>
        private void SetLoginState(bool isLoggingIn)
            {
            // Disable/enable controls
            txtUsername.Enabled = !isLoggingIn;
            txtPassword.Enabled = !isLoggingIn;
            chkRememberMe.Enabled = !isLoggingIn;
            btnCancel.Enabled = !isLoggingIn;

            if (isLoggingIn)
                {
                btnLogin.Text = "Signing In...";
                btnLogin.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                // Show progress indicator
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                }
            else
                {
                btnLogin.Text = "Sign In";
                btnLogin.Enabled = true;
                this.Cursor = Cursors.Default;

                // Hide progress indicator
                progressBar.Visible = false;
                progressBar.Style = ProgressBarStyle.Continuous;
                }
            }

        /// <summary>
        /// Handles cancel button click event
        /// </summary>
        private void BtnCancel_Click(object? sender, EventArgs e)
            {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
            }

        /// <summary>
        /// Handles show/hide password button click
        /// </summary>
        private void BtnShowPassword_Click(object? sender, EventArgs e)
            {
            if (txtPassword.UseSystemPasswordChar)
                {
                txtPassword.UseSystemPasswordChar = false;
                btnShowPassword.Text = "🙈"; // Hide icon
                btnShowPassword.AccessibleDescription = "Hide password";
                }
            else
                {
                txtPassword.UseSystemPasswordChar = true;
                btnShowPassword.Text = "👁"; // Show icon
                btnShowPassword.AccessibleDescription = "Show password";
                }
            }

        /// <summary>
        /// Handles forgot password link click
        /// </summary>
        private void LblForgotPassword_Click(object? sender, EventArgs e)
            {
            // Show forgot password information
            MessageBox.Show(
                "Please contact your system administrator to reset your password.\n\n" +
                "Default credentials:\n" +
                "Username: admin\n" +
                "Password: admin123",
                "Password Reset",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            }

        /// <summary>
        /// Handles form closing event
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
            {
            if (_isLoggingIn)
                {
                e.Cancel = true;
                return;
                }

            base.OnFormClosing(e);
            }

        /// <summary>
        /// Clean up any resources being used
        /// </summary>
        //protected override void Dispose(bool disposing)
        //    {
        //    if (disposing && (components != null))
        //        {
        //        components.Dispose();
        //        }
        //    base.Dispose(disposing);
        //    }
        }
    }