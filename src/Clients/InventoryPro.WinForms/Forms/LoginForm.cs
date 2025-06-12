using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace InventoryPro.WinForms.Forms
    {
    /// <summary>
    /// Modern, professional login form for InventoryPro application
    /// Features: Gradient backgrounds, smooth animations, modern UI elements,
    /// enhanced security, and professional user experience
    /// </summary>
    public partial class LoginForm : Form
        {
        #region Private Fields

        private readonly ILogger<LoginForm> _logger;
        private readonly IAuthService _authService;
        private readonly IApiService _apiService;

        // Animation and UI state variables
        private bool _isLoggingIn = false;
        private bool _passwordVisible = false;
        private float _animationProgress = 0f;
        private bool _animationDirection = true;

        // Colors for modern theme
        private readonly Color _primaryBlue = Color.FromArgb(59, 130, 246);
        private readonly Color _primaryBlueHover = Color.FromArgb(37, 99, 235);
        private readonly Color _backgroundGray = Color.FromArgb(248, 249, 250);
        private readonly Color _textGray = Color.FromArgb(75, 85, 99);
        private readonly Color _borderGray = Color.FromArgb(209, 213, 219);
        private readonly Color _errorRed = Color.FromArgb(239, 68, 68);
        private readonly Color _successGreen = Color.FromArgb(34, 197, 94);

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Initializes a new instance of the LoginForm with dependency injection
        /// </summary>
        /// <param name="logger">Logger service for tracking application events</param>
        /// <param name="authService">Authentication service for user validation</param>
        /// <param name="apiService">API service for backend communication</param>
        public LoginForm(ILogger<LoginForm> logger, IAuthService authService, IApiService apiService)
            {
            // Validate dependencies - throw exception if any are null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Initialize the form components
            InitializeComponent();

            // Configure additional form properties
            ConfigureForm();

            // Set up modern styling
            ApplyModernStyling();

            _logger.LogInformation("LoginForm initialized successfully");
            }

        /// <summary>
        /// Configures additional form properties, events, and initial state
        /// </summary>
        private void ConfigureForm()
            {
            // Enable form to receive key events before controls
            this.KeyPreview = true;

            // Set default buttons for Enter and Escape keys
            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;

            // Configure form appearance
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.DoubleBuffer, true);

            // Add event handlers for better user experience
            this.KeyDown += LoginForm_KeyDown;
            this.Shown += LoginForm_Shown;
            this.Load += LoginForm_Load;

            // Configure text boxes for better UX
            txtUsername.KeyDown += TextBox_KeyDown;
            txtPassword.KeyDown += TextBox_KeyDown;

            // Load saved user preferences
            LoadSavedCredentials();
            }

        /// <summary>
        /// Applies modern styling to all form elements
        /// </summary>
        private void ApplyModernStyling()
            {
            // Enable anti-aliasing for smoother text rendering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            // Configure button styling
            ConfigureButtonStyling();

            // Configure input field styling
            ConfigureInputStyling();

            _logger.LogDebug("Modern styling applied to LoginForm");
            }

        #endregion

        #region Custom Painting Methods

        /// <summary>
        /// Creates a custom gradient background for the left panel
        /// This gives the form a modern, professional appearance
        /// </summary>
        private void PnlLeftPanel_Paint(object? sender, PaintEventArgs e)
            {
            if (sender is not Panel panel) return;

            // Create a linear gradient from top-left to bottom-right
            using var gradientBrush = new LinearGradientBrush(
                panel.ClientRectangle,
                Color.FromArgb(45, 108, 175),   // Start color (darker blue)
                Color.FromArgb(79, 172, 254),   // End color (lighter blue)
                LinearGradientMode.ForwardDiagonal);

            // Fill the panel with the gradient
            e.Graphics.FillRectangle(gradientBrush, panel.ClientRectangle);

            // Add subtle animation effect
            if (_animationProgress > 0)
                {
                using var overlayBrush = new SolidBrush(
                    Color.FromArgb((int)(30 * _animationProgress), Color.White));
                e.Graphics.FillRectangle(overlayBrush, panel.ClientRectangle);
                }
            }

        /// <summary>
        /// Paints a subtle shadow effect around the login container
        /// This creates depth and makes the login form stand out
        /// </summary>
        private void PnlLoginContainer_Paint(object? sender, PaintEventArgs e)
            {
            if (sender is not Panel panel) return;

            // Create shadow effect
            var shadowOffset = 5;
            var shadowColor = Color.FromArgb(50, 0, 0, 0);

            using var shadowBrush = new SolidBrush(shadowColor);
            var shadowRect = new Rectangle(
                shadowOffset, shadowOffset,
                panel.Width - shadowOffset, panel.Height - shadowOffset);

            // Draw shadow
            e.Graphics.FillRectangle(shadowBrush, shadowRect);

            // Draw main container
            using var containerBrush = new SolidBrush(Color.White);
            var containerRect = new Rectangle(0, 0, panel.Width - shadowOffset, panel.Height - shadowOffset);
            e.Graphics.FillRectangle(containerBrush, containerRect);

            // Draw border
            using var borderPen = new Pen(_borderGray, 1);
            e.Graphics.DrawRectangle(borderPen, containerRect);
            }

        /// <summary>
        /// Custom painting for input containers to create modern bordered fields
        /// </summary>
        private void InputContainer_Paint(object? sender, PaintEventArgs e)
            {
            if (sender is not Panel panel) return;

            var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            var borderColor = panel.Focused || panel.ContainsFocus ? _primaryBlue : _borderGray;
            var borderWidth = panel.Focused || panel.ContainsFocus ? 2 : 1;

            // Draw background
            using var backgroundBrush = new SolidBrush(_backgroundGray);
            e.Graphics.FillRectangle(backgroundBrush, panel.ClientRectangle);

            // Draw border with rounded corners
            using var borderPen = new Pen(borderColor, borderWidth);
            DrawRoundedRectangle(e.Graphics, borderPen, rect, 8);
            }

        /// <summary>
        /// Custom logo painting - creates a modern app icon
        /// </summary>
        private void PicLogo_Paint(object? sender, PaintEventArgs e)
            {
            if (sender is not PictureBox pic) return;

            var rect = pic.ClientRectangle;

            // Create circular background
            using var backgroundBrush = new SolidBrush(Color.White);
            e.Graphics.FillEllipse(backgroundBrush, rect);

            // Draw icon elements (simplified inventory icon)
            var iconRect = new Rectangle(rect.X + 15, rect.Y + 15, rect.Width - 30, rect.Height - 30);
            using var iconBrush = new SolidBrush(_primaryBlue);
            e.Graphics.FillRectangle(iconBrush, iconRect);

            // Add details to the icon
            using var detailPen = new Pen(Color.White, 2);
            var centerX = rect.Width / 2;
            var centerY = rect.Height / 2;

            // Draw simple inventory lines
            e.Graphics.DrawLine(detailPen, centerX - 10, centerY - 10, centerX + 10, centerY - 10);
            e.Graphics.DrawLine(detailPen, centerX - 10, centerY, centerX + 10, centerY);
            e.Graphics.DrawLine(detailPen, centerX - 10, centerY + 10, centerX + 10, centerY + 10);
            }

        /// <summary>
        /// Custom button painting for modern gradient buttons
        /// </summary>
        private void BtnLogin_Paint(object? sender, PaintEventArgs e)
            {
            if (sender is not Button button) return;

            var rect = button.ClientRectangle;
            var isHovered = button.ClientRectangle.Contains(button.PointToClient(Cursor.Position));
            var buttonColor = isHovered ? _primaryBlueHover : _primaryBlue;

            // Create gradient brush for button
            using var gradientBrush = new LinearGradientBrush(
                rect, buttonColor, Color.FromArgb(buttonColor.R - 20, buttonColor.G - 20, buttonColor.B - 20),
                LinearGradientMode.Vertical);

            // Fill button with rounded corners
            using var path = CreateRoundedPath(rect, 8);
            e.Graphics.FillPath(gradientBrush, path);

            // Draw text
            var textColor = button.Enabled ? Color.White : Color.Gray;
            TextRenderer.DrawText(e.Graphics, button.Text, button.Font, rect, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles form load event - sets initial focus and prepares UI
        /// </summary>
        private void LoginForm_Load(object? sender, EventArgs e)
            {
            // Set initial focus to appropriate field
            if (string.IsNullOrEmpty(txtUsername.Text))
                txtUsername.Focus();
            else
                txtPassword.Focus();

            _logger.LogDebug("LoginForm loaded and ready for user input");
            }

        /// <summary>
        /// Handles form shown event - starts any initial animations
        /// </summary>
        private void LoginForm_Shown(object? sender, EventArgs e)
            {
            // Start welcome animation
            StartWelcomeAnimation();
            }

        /// <summary>
        /// Handles global key events for the form
        /// </summary>
        private void LoginForm_KeyDown(object? sender, KeyEventArgs e)
            {
            switch (e.KeyCode)
                {
                case Keys.Enter when !_isLoggingIn:
                    e.Handled = true;
                    _ = PerformLoginAsync();
                    break;

                case Keys.Escape:
                    e.Handled = true;
                    if (_isLoggingIn)
                        {
                        // Could implement login cancellation here
                        return;
                        }
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    break;

                case Keys.F1:
                    e.Handled = true;
                    ShowHelpDialog();
                    break;
                }
            }

        /// <summary>
        /// Handles key events for text boxes (Enter key navigation)
        /// </summary>
        private void TextBox_KeyDown(object? sender, KeyEventArgs e)
            {
            if (e.KeyCode == Keys.Enter && !_isLoggingIn)
                {
                e.Handled = true;

                if (sender == txtUsername && string.IsNullOrEmpty(txtPassword.Text))
                    {
                    txtPassword.Focus();
                    }
                else
                    {
                    _ = PerformLoginAsync();
                    }
                }
            }

        /// <summary>
        /// Handles text box focus events for modern styling
        /// </summary>
        private void TextBox_Enter(object? sender, EventArgs e)
            {
            if (sender is TextBox textBox)
                {
                textBox.Parent?.Invalidate(); // Trigger repaint for focus styling

                // Add subtle animation
                AnimateControl(textBox.Parent, true);
                }
            }

        /// <summary>
        /// Handles text box leave events
        /// </summary>
        private void TextBox_Leave(object? sender, EventArgs e)
            {
            if (sender is TextBox textBox)
                {
                textBox.Parent?.Invalidate(); // Trigger repaint

                // Validate field on leave
                ValidateField(textBox);

                // Remove focus animation
                AnimateControl(textBox.Parent, false);
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
        /// Handles cancel button click event
        /// </summary>
        private void BtnCancel_Click(object? sender, EventArgs e)
            {
            _logger.LogInformation("User cancelled login");
            this.DialogResult = DialogResult.Cancel;
            this.Close();
            }

        /// <summary>
        /// Handles password visibility toggle
        /// </summary>
        private void BtnTogglePassword_Click(object? sender, EventArgs e)
            {
            _passwordVisible = !_passwordVisible;
            txtPassword.UseSystemPasswordChar = !_passwordVisible;

            // Update button icon
            btnTogglePassword.Text = _passwordVisible ? "🙈" : "👁";
            btnTogglePassword.AccessibleDescription = _passwordVisible ? "Hide password" : "Show password";

            // Keep focus on password field
            txtPassword.Focus();

            _logger.LogDebug("Password visibility toggled: {Visible}", _passwordVisible);
            }

        /// <summary>
        /// Handles forgot password link click
        /// </summary>
        private void LblForgotPassword_Click(object? sender, EventArgs e)
            {
            ShowForgotPasswordDialog();
            }

        /// <summary>
        /// Handles button mouse enter events for hover effects
        /// </summary>
        private void Button_MouseEnter(object? sender, EventArgs e)
            {
            if (sender is Button button && button.Enabled)
                {
                button.Invalidate(); // Trigger repaint for hover effect
                AnimateControl(button, true);
                }
            }

        /// <summary>
        /// Handles button mouse leave events
        /// </summary>
        private void Button_MouseLeave(object? sender, EventArgs e)
            {
            if (sender is Button button)
                {
                button.Invalidate(); // Trigger repaint
                AnimateControl(button, false);
                }
            }

        /// <summary>
        /// Handles link label mouse enter events
        /// </summary>
        private void LinkLabel_MouseEnter(object? sender, EventArgs e)
            {
            if (sender is Label label)
                {
                label.ForeColor = _primaryBlueHover;
                }
            }

        /// <summary>
        /// Handles link label mouse leave events
        /// </summary>
        private void LinkLabel_MouseLeave(object? sender, EventArgs e)
            {
            if (sender is Label label)
                {
                label.ForeColor = _primaryBlue;
                }
            }

        #endregion

        #region Authentication Methods

        /// <summary>
        /// Performs the login operation with comprehensive error handling
        /// </summary>
        /// <summary>
        /// Performs the login operation with comprehensive error handling
        /// </summary>
        private async Task PerformLoginAsync()
            {
            // Prevent multiple login attempts
            if (_isLoggingIn) return;

            // Validate input before attempting login
            if (!ValidateInput()) return;

            try
                {
                _isLoggingIn = true;
                SetLoginState(true);
                HideError();

                var loginRequest = new LoginRequestDto
                    {
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text,
                    RememberMe = chkRememberMe.Checked
                    };

                _logger.LogInformation("Login attempt for user: {Username}", loginRequest.Username);

                // Perform login with timeout
                var response = await _apiService.LoginAsync(loginRequest);

                if (response.Success && response.Data != null)
                    {
                    // Save credentials if remember me is checked
                    SaveCredentials();

                    _logger.LogInformation("Login successful for user: {Username}", loginRequest.Username);

                    // Show success message briefly
                    ShowSuccess("Login successful!");
                    await Task.Delay(1000); // Brief pause to show success

                    // Close form with success result
                    this.DialogResult = DialogResult.OK;
                    this.Close(); // Changed from Hide() to Close()
                    }
                else
                    {
                    // Handle login failure
                    var errorMessage = !string.IsNullOrEmpty(response.Message)
                        ? response.Message
                        : "Invalid username or password. Please try again.";

                    ShowError(errorMessage);
                    ClearPasswordField();

                    _logger.LogWarning("Login failed for user: {Username} - {Error}",
                        loginRequest.Username, errorMessage);
                    }
                }
            catch (OperationCanceledException)
                {
                _logger.LogWarning("Login operation timed out");
                ShowError("Login request timed out. Please check your connection and try again.");
                ClearPasswordField();
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unexpected error during login");
                ShowError("An unexpected error occurred. Please try again or contact support.");
                ClearPasswordField();
                }
            finally
                {
                _isLoggingIn = false;
                SetLoginState(false);
                }
            }

        /// <summary>
        /// Validates user input with comprehensive checks
        /// </summary>
        private bool ValidateInput()
            {
            HideError();

            // Validate username
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                ShowError("Please enter your username.");
                txtUsername.Focus();
                return false;
                }

            // Validate username format
            var username = txtUsername.Text.Trim();
            if (username.Length < 3)
                {
                ShowError("Username must be at least 3 characters long.");
                txtUsername.Focus();
                return false;
                }

            if (username.Length > 50)
                {
                ShowError("Username cannot exceed 50 characters.");
                txtUsername.Focus();
                return false;
                }

            // Check for valid username characters
            if (!IsValidUsername(username))
                {
                ShowError("Username contains invalid characters. Use only letters, numbers, and underscores.");
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

            if (txtPassword.Text.Length < 6)
                {
                ShowError("Password must be at least 6 characters long.");
                txtPassword.Focus();
                return false;
                }

            if (txtPassword.Text.Length > 128)
                {
                ShowError("Password cannot exceed 128 characters.");
                txtPassword.Focus();
                return false;
                }

            return true;
            }

        /// <summary>
        /// Validates individual fields as user types
        /// </summary>
        private void ValidateField(TextBox textBox)
            {
            if (textBox == txtUsername)
                {
                var username = textBox.Text.Trim();
                if (!string.IsNullOrEmpty(username) && !IsValidUsername(username))
                    {
                    ShowError("Username contains invalid characters.");
                    }
                }
            }

        /// <summary>
        /// Checks if username contains only valid characters
        /// </summary>
        private static bool IsValidUsername(string username)
            {
            return username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '-');
            }

        #endregion

        #region UI State Management

        /// <summary>
        /// Sets the login state (enables/disables controls during login)
        /// </summary>
        private void SetLoginState(bool isLoggingIn)
            {
            // Update control states
            txtUsername.Enabled = !isLoggingIn;
            txtPassword.Enabled = !isLoggingIn;
            chkRememberMe.Enabled = !isLoggingIn;
            btnCancel.Enabled = !isLoggingIn;
            btnTogglePassword.Enabled = !isLoggingIn;
            lblForgotPassword.Enabled = !isLoggingIn;

            if (isLoggingIn)
                {
                btnLogin.Text = "Signing In...";
                btnLogin.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                // Show progress indicator
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;

                // Start animation
                animationTimer.Start();
                }
            else
                {
                btnLogin.Text = "Sign In";
                btnLogin.Enabled = true;
                this.Cursor = Cursors.Default;

                // Hide progress indicator
                progressBar.Visible = false;
                progressBar.Style = ProgressBarStyle.Continuous;

                // Stop animation
                animationTimer.Stop();
                _animationProgress = 0f;
                }

            // Refresh affected controls
            btnLogin.Invalidate();
            pnlLeftPanel.Invalidate();
            }

        /// <summary>
        /// Shows an error message with modern styling
        /// </summary>
        private void ShowError(string message)
            {
            lblError.Text = $"❌ {message}";
            lblError.ForeColor = _errorRed;
            lblError.Visible = true;

            // Create error flash animation
            FlashErrorLabel();

            _logger.LogDebug("Error message displayed: {Message}", message);
            }

        /// <summary>
        /// Shows a success message
        /// </summary>
        private void ShowSuccess(string message)
            {
            lblError.Text = $"✅ {message}";
            lblError.ForeColor = _successGreen;
            lblError.Visible = true;
            }

        /// <summary>
        /// Hides the error/success message
        /// </summary>
        private void HideError()
            {
            lblError.Visible = false;
            }

        /// <summary>
        /// Clears the password field for security
        /// </summary>
        private void ClearPasswordField()
            {
            txtPassword.Clear();
            txtPassword.Focus();
            }

        #endregion

        #region Animation Methods

        /// <summary>
        /// Creates a subtle flash effect for the error label
        /// </summary>
        private async void FlashErrorLabel()
            {
            try
                {
                var originalColor = lblError.ForeColor;

                // Flash sequence
                for (int i = 0; i < 3; i++)
                    {
                    lblError.ForeColor = Color.FromArgb(255, 200, 200);
                    await Task.Delay(100);
                    lblError.ForeColor = originalColor;
                    await Task.Delay(100);
                    }
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Error during flash animation");
                }
            }

        /// <summary>
        /// Starts the welcome animation when form is first shown
        /// </summary>
        private void StartWelcomeAnimation()
            {
            // Animate the login container sliding in
            var originalLocation = pnlLoginContainer.Location;
            pnlLoginContainer.Location = new Point(originalLocation.X + 50, originalLocation.Y);

            var timer = new Timer();
            timer.Interval = 16; // ~60 FPS
            var steps = 0;
            var maxSteps = 20;

            timer.Tick += (s, e) =>
            {
                steps++;
                var progress = (float)steps / maxSteps;
                var easeProgress = EaseOutCubic(progress);

                var currentX = originalLocation.X + (int)((1 - easeProgress) * 50);
                pnlLoginContainer.Location = new Point(currentX, originalLocation.Y);

                if (steps >= maxSteps)
                    {
                    timer.Stop();
                    timer.Dispose();
                    pnlLoginContainer.Location = originalLocation;
                    }
            };

            timer.Start();
            }

        /// <summary>
        /// Animates a control with smooth transitions
        /// </summary>
        private void AnimateControl(Control? control, bool expand)
            {
            if (control == null) return;

            // Simple scale animation could be implemented here
            // For now, just trigger repaints for color transitions
            control.Invalidate();
            }

        /// <summary>
        /// Animation timer tick event for ongoing animations
        /// </summary>
        private void AnimationTimer_Tick(object? sender, EventArgs e)
            {
            if (_animationDirection)
                {
                _animationProgress += 0.02f;
                if (_animationProgress >= 1.0f)
                    {
                    _animationProgress = 1.0f;
                    _animationDirection = false;
                    }
                }
            else
                {
                _animationProgress -= 0.02f;
                if (_animationProgress <= 0.0f)
                    {
                    _animationProgress = 0.0f;
                    _animationDirection = true;
                    }
                }

            // Refresh animated elements
            pnlLeftPanel.Invalidate();
            }

        /// <summary>
        /// Easing function for smooth animations
        /// </summary>
        private static float EaseOutCubic(float t)
            {
            return 1f - (float)Math.Pow(1 - t, 3);
            }

        #endregion

        #region Settings and Preferences

        /// <summary>
        /// Loads saved user credentials and preferences
        /// </summary>
        private void LoadSavedCredentials()
            {
            try
                {
                var savedUsername = Properties.Settings.Default.SavedUsername;
                var rememberMe = Properties.Settings.Default.RememberMe;

                if (rememberMe && !string.IsNullOrEmpty(savedUsername))
                    {
                    txtUsername.Text = savedUsername;
                    chkRememberMe.Checked = true;

                    _logger.LogDebug("Loaded saved username: {Username}", savedUsername);
                    }
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Failed to load saved credentials");
                }
            }

        /// <summary>
        /// Saves user credentials if remember me is checked
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
                _logger.LogDebug("User preferences saved");
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Failed to save user preferences");
                }
            }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Configures modern styling for buttons
        /// </summary>
        private void ConfigureButtonStyling()
            {
            // Configure login button
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.BackColor = _primaryBlue;
            btnLogin.ForeColor = Color.White;
            btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            // Configure cancel button
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = _borderGray;
            btnCancel.BackColor = Color.White;
            btnCancel.ForeColor = _textGray;
            }

        /// <summary>
        /// Configures modern styling for input fields
        /// </summary>
        private void ConfigureInputStyling()
            {
            // Configure text boxes
            foreach (var textBox in new[] { txtUsername, txtPassword })
                {
                textBox.BorderStyle = BorderStyle.None;
                textBox.Font = new Font("Segoe UI", 11F);
                textBox.BackColor = _backgroundGray;
                }
            }

        /// <summary>
        /// Shows the forgot password dialog
        /// </summary>
        private void ShowForgotPasswordDialog()
            {
            var message = "To reset your password, please contact your system administrator.\n\n" +
                         "Default credentials for testing:\n" +
                         "Username: admin\n" +
                         "Password: admin123\n\n" +
                         "Username: user\n" +
                         "Password: user123";

            MessageBox.Show(message, "Password Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _logger.LogInformation("User accessed forgot password dialog");
            }

        /// <summary>
        /// Shows the help dialog
        /// </summary>
        private void ShowHelpDialog()
            {
            var helpMessage = "InventoryPro Login Help\n\n" +
                             "• Enter your username and password\n" +
                             "• Check 'Remember me' to save your username\n" +
                             "• Use the eye icon to show/hide your password\n" +
                             "• Press Enter to login\n" +
                             "• Press Escape to cancel\n" +
                             "• Press F1 for this help\n\n" +
                             "For technical support, contact your system administrator.";

            MessageBox.Show(helpMessage, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        /// <summary>
        /// Creates a rounded rectangle path for custom painting
        /// </summary>
        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
            {
            var path = new GraphicsPath();
            var diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
            }

        /// <summary>
        /// Draws a rounded rectangle
        /// </summary>
        private static void DrawRoundedRectangle(Graphics graphics, Pen pen, Rectangle rect, int radius)
            {
            using var path = CreateRoundedPath(rect, radius);
            graphics.DrawPath(pen, path);
            }

        /// <summary>
        /// Creates a custom application icon
        /// </summary>
        private Icon CreateApplicationIcon()
            {
            // Create a simple icon programmatically
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
                {
                g.Clear(Color.Transparent);
                using var brush = new SolidBrush(_primaryBlue);
                g.FillEllipse(brush, 4, 4, 24, 24);

                using var pen = new Pen(Color.White, 2);
                g.DrawLine(pen, 12, 10, 20, 10);
                g.DrawLine(pen, 12, 16, 20, 16);
                g.DrawLine(pen, 12, 22, 20, 22);
                }

            return Icon.FromHandle(bitmap.GetHicon());
            }

        #endregion

        #region Form Events

        /// <summary>
        /// Handles form closing event with proper cleanup
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
            {
            // Prevent closing during login operation
            if (_isLoggingIn)
                {
                e.Cancel = true;
                return;
                }

            // Stop any running animations
            animationTimer?.Stop();

            _logger.LogInformation("LoginForm closing with result: {Result}", this.DialogResult);

            base.OnFormClosing(e);
            }

       
        //protected override void Dispose(bool disposing)
        //    {
        //    if (disposing)
        //        {
        //        animationTimer?.Dispose();
        //        components?.Dispose();
        //        }
        //    base.Dispose(disposing);
        //    }

        #endregion
        }
    }