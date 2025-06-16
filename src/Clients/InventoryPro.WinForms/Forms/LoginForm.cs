using InventoryPro.Shared.DTOs;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
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
        
        // Animation variables (reserved for future use)
        #pragma warning disable CS0414
        private float _animationProgress = 0f;
        private bool _animationDirection = true;
        private float _overlayOpacity = 0f;
        #pragma warning restore CS0414
        
        private float _buttonAnimationProgress = 0f;
        private float _loadingSpinnerAngle = 0f;
        private bool _isAnimatingButton = false;
        private string _originalButtonText = "Sign In";
        private Timer _buttonAnimationTimer;
        private Timer _spinnerTimer;

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
        public                                                                                                                                                              LoginForm(ILogger<LoginForm> logger, IAuthService authService, IApiService apiService)
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

            // Initialize animation timers
            _buttonAnimationTimer = new Timer(); // Ensure non-null initialization
            _spinnerTimer = new Timer();         // Ensure non-null initialization
            InitializeAnimationTimers();

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
        /// Applies simplified styling for performance
        /// </summary>
        private void ApplyModernStyling()
        {
            // Use native styling for maximum performance
            // Set background colors directly instead of custom painting
            
            // Set left panel background color
            if (pnlLeftPanel != null)
            {
                pnlLeftPanel.BackColor = Color.FromArgb(59, 130, 246);
            }

            // Configure button styling without custom paint
            ConfigureButtonStyling();

            // Configure input field styling
            ConfigureInputStyling();

            _logger.LogDebug("Simplified styling applied to LoginForm for performance");
        }

        /// <summary>
        /// Initializes animation timers (disabled for performance)
        /// </summary>
        private void InitializeAnimationTimers()
        {
            // Disable all animation timers for maximum performance
            _buttonAnimationTimer.Interval = 1000; // Set to very slow interval
            _buttonAnimationTimer.Tick += ButtonAnimationTimer_Tick;

            _spinnerTimer.Interval = 1000; // Set to very slow interval
            _spinnerTimer.Tick += SpinnerTimer_Tick;

            // Don't start any timers by default
            _logger.LogDebug("Animation timers initialized but disabled for performance");
        }

        #endregion

        #region Custom Painting Methods

        /// <summary>
        /// Disabled custom painting for maximum performance
        /// </summary>
        private void PnlLeftPanel_Paint(object? sender, PaintEventArgs e)
        {
            // Disable custom painting - use panel's native BackColor instead
            // This eliminates all custom drawing CPU overhead
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
        /// Disabled input container custom painting for performance
        /// </summary>
        private void InputContainer_Paint(object? sender, PaintEventArgs e)
        {
            // Disable custom painting for maximum performance
            // Use native control styling instead
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
        /// Disabled custom button painting for maximum performance
        /// </summary>
        private void BtnLogin_Paint(object? sender, PaintEventArgs e)
        {
            // Disable custom painting - use native button appearance
            // This eliminates the most CPU-intensive custom drawing
        }

        /// <summary>
        /// Draws a smooth loading spinner on the button
        /// </summary>
        private void DrawLoadingSpinner(Graphics graphics, Rectangle rect)
        {
            var centerX = rect.X + 30; // Position spinner to the left of text
            var centerY = rect.Y + rect.Height / 2;
            var radius = 8;

            // Enable anti-aliasing for smooth spinner
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw spinner arcs with varying opacity for smooth rotation effect
            for (int i = 0; i < 12; i++)
            {
                var angle = (_loadingSpinnerAngle + i * 30) % 360;
                var opacity = (int)(255 * (1.0f - (i / 12.0f))); // Fade effect

                using var pen = new Pen(Color.FromArgb(opacity, Color.White), 2);

                var startAngle = angle;
                var endX = centerX + (int)(radius * Math.Cos(startAngle * Math.PI / 180));
                var endY = centerY + (int)(radius * Math.Sin(startAngle * Math.PI / 180));
                var startX = centerX + (int)((radius - 4) * Math.Cos(startAngle * Math.PI / 180));
                var startY = centerY + (int)((radius - 4) * Math.Sin(startAngle * Math.PI / 180));

                graphics.DrawLine(pen, startX, startY, endX, endY);
            }
        }

        /// <summary>
        /// Draws loading text with smooth transitions
        /// </summary>
        private void DrawLoadingText(Graphics graphics, Rectangle rect, Font font)
        {
            var loadingText = "Signing In...";
            var textRect = new Rectangle(rect.X + 50, rect.Y, rect.Width - 50, rect.Height); // Make room for spinner

            // Calculate text opacity for smooth fade effect
            var textOpacity = _isAnimatingButton ? (int)(255 * EaseInOutSine(_buttonAnimationProgress)) : 255;
            var textColor = Color.FromArgb(textOpacity, Color.White);

            // Draw text with smooth rendering
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            TextRenderer.DrawText(graphics, loadingText, font, textRect, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
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

                _logger.LogInformation("Login response received: Success={Success}", response?.Success);

                if (response?.Success == true && response.Data != null)
                {
                    // Verify that authentication data was stored properly
                    await Task.Delay(100); // Give a moment for the auth service to store data

                    var storedUser = await _authService.GetCurrentUserAsync();
                    var storedToken = await _authService.GetTokenAsync();

                    _logger.LogInformation("Stored user after login: {HasUser}, Token: {HasToken}",
                        storedUser != null, !string.IsNullOrEmpty(storedToken));

                    if (storedUser != null && !string.IsNullOrEmpty(storedToken))
                    {
                        // Save credentials if remember me is checked
                        SaveCredentials();

                        _logger.LogInformation("Login successful for user: {Username}", loginRequest.Username);

                        // Show success message briefly
                        ShowSuccess("Login successful!");

                        // Brief pause to show success message
                        await Task.Delay(100);

                        // Set dialog result and close
                        _logger.LogInformation("Setting DialogResult.OK and closing login form");
                        this.DialogResult = DialogResult.OK;
                        this.Hide();//Fix
                    }
                    else
                    {
                        _logger.LogError("Authentication data not properly stored");
                        ShowError("Authentication error. Please try again.");
                        ClearPasswordField();
                    }
                }
                else
                {
                    // Handle login failure
                    var errorMessage = !string.IsNullOrEmpty(response?.Message)
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
        /// Sets the login state without animations for maximum performance
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
                // Store original button text and change to loading state
                _originalButtonText = btnLogin.Text;
                btnLogin.Text = "Signing In...";
                btnLogin.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                // Show progress indicator without animations
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                // Restore original state
                btnLogin.Text = _originalButtonText;
                btnLogin.Enabled = true;
                this.Cursor = Cursors.Default;

                // Hide progress indicator
                progressBar.Visible = false;
                progressBar.Style = ProgressBarStyle.Continuous;
            }

            // Stop all timers to prevent CPU overhead
            _buttonAnimationTimer?.Stop();
            _spinnerTimer?.Stop();
            animationTimer?.Stop();
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
        /// Disabled login container animation for performance
        /// </summary>
        private void AnimateLoginContainer(bool isLoggingIn)
        {
            // Disable all animation for maximum performance
            // Keep static white background
        }

        /// <summary>
        /// Simplified error flash effect
        /// </summary>
        private void FlashErrorLabel()
        {
            // Simple flash without complex animation for performance
            var originalColor = lblError.ForeColor;
            lblError.ForeColor = Color.FromArgb(255, 100, 100);
            
            // Use a simple timer for quick flash back
            var flashTimer = new Timer();
            flashTimer.Interval = 200;
            flashTimer.Tick += (s, e) =>
            {
                lblError.ForeColor = originalColor;
                flashTimer.Stop();
                flashTimer.Dispose();
            };
            flashTimer.Start();
        }

        /// <summary>
        /// Disabled welcome animation for performance
        /// </summary>
        private void StartWelcomeAnimation()
        {
            // No animation - just set static appearance for performance
            pnlLoginContainer.BackColor = Color.White;
            // Don't start any animation timers
        }

        /// <summary>
        /// Disabled control animation for performance
        /// </summary>
        private void AnimateControl(Control? control, bool expand)
        {
            // Completely disable animations for performance
            // No visual changes to prevent CPU overhead
        }

        /// <summary>
        /// Disabled animation timer for performance
        /// </summary>
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            // Disable all animations to eliminate CPU overhead
            // Stop the timer immediately to prevent further calls
            animationTimer?.Stop();
        }

        /// <summary>
        /// Disabled button animation timer for performance
        /// </summary>
        private void ButtonAnimationTimer_Tick(object? sender, EventArgs e)
        {
            // Disable button animations to eliminate CPU overhead
            _buttonAnimationTimer?.Stop();
            _isAnimatingButton = false;
        }

        /// <summary>
        /// Disabled spinner timer for performance
        /// </summary>
        private void SpinnerTimer_Tick(object? sender, EventArgs e)
        {
            // Disable spinner to eliminate CPU overhead
            _spinnerTimer?.Stop();
        }

        /// <summary>
        /// Easing function for smooth animations
        /// </summary>
        private static float EaseOutCubic(float t)
        {
            return 1f - (float)Math.Pow(1 - t, 3);
        }

        /// <summary>
        /// Advanced easing functions for smoother animations
        /// </summary>
        private static float EaseInOutQuart(float t)
        {
            return t < 0.5f ? 8f * t * t * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 4f) / 2f;
        }

        private static float EaseInOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            return t < 0.5f
                ? (float)(Math.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
                : (float)(Math.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
        }

        private static float EaseOutElastic(float t)
        {
            const float c4 = (2f * (float)Math.PI) / 3f;

            return t == 0f ? 0f : t == 1f ? 1f : (float)(Math.Pow(2f, -10f * t) * Math.Sin((t * 10f - 0.75f) * c4) + 1f);
        }

        private static float EaseInOutSine(float t)
        {
            return -(float)(Math.Cos(Math.PI * t) - 1f) / 2f;
        }

        private static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2f / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            else
            {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
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

            // Stop and dispose all animation timers
            animationTimer?.Stop();
            _buttonAnimationTimer?.Stop();
            _spinnerTimer?.Stop();

            _logger.LogInformation("LoginForm closing with result: {Result}", this.DialogResult);

            base.OnFormClosing(e);
        }


        #endregion
    }
}