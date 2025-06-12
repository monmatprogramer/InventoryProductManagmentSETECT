namespace InventoryPro.WinForms.Forms
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlLogin = new System.Windows.Forms.Panel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlForm = new System.Windows.Forms.Panel();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.pnlPasswordContainer = new System.Windows.Forms.Panel();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnShowPassword = new System.Windows.Forms.Button();
            this.chkRememberMe = new System.Windows.Forms.CheckBox();
            this.lblError = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblForgotPassword = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.pnlMain.SuspendLayout();
            this.pnlLogin.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlForm.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlPasswordContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();

            // 
            // Main Panel
            // 
            this.pnlMain.BackColor = Color.FromArgb(240, 240, 240);
            this.pnlMain.Dock = DockStyle.Fill;
            this.pnlMain.Padding = new Padding(40);
            this.pnlMain.Controls.Add(this.pnlLogin);

            // 
            // Login Panel
            // 
            this.pnlLogin.BackColor = Color.White;
            this.pnlLogin.Size = new Size(400, 520);
            this.pnlLogin.Location = new Point(40, 40);
            this.pnlLogin.Anchor = AnchorStyles.None;
            this.pnlLogin.Controls.Add(this.pnlHeader);
            this.pnlLogin.Controls.Add(this.pnlForm);
            this.pnlLogin.Controls.Add(this.pnlButtons);
            this.pnlLogin.Controls.Add(this.lblForgotPassword);
            this.pnlLogin.Controls.Add(this.lblVersion);

            // Add shadow effect (simplified)
            this.pnlLogin.BorderStyle = BorderStyle.None;

            // 
            // Header Panel
            // 
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 120;
            this.pnlHeader.BackColor = Color.White;
            this.pnlHeader.Controls.Add(this.picLogo);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSubtitle);

            // 
            // Logo
            // 
            this.picLogo.Size = new Size(48, 48);
            this.picLogo.Location = new Point(176, 20);
            this.picLogo.BackColor = Color.FromArgb(41, 128, 185);
            this.picLogo.SizeMode = PictureBoxSizeMode.CenterImage;
            // Note: The custom Paint event handler for picLogo was removed from here to fix a designer issue.
            // Please re-add this logic in LoginForm.cs (e.g., in ConfigureForm or the constructor).

            // 
            // Title
            // 
            this.lblTitle.Text = "InventoryPro";
            this.lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.FromArgb(51, 51, 51);
            this.lblTitle.Location = new Point(0, 75);
            this.lblTitle.Size = new Size(400, 30);
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // Subtitle
            // 
            this.lblSubtitle.Text = "Sign in to your account";
            this.lblSubtitle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.lblSubtitle.ForeColor = Color.FromArgb(128, 128, 128);
            this.lblSubtitle.Location = new Point(0, 100);
            this.lblSubtitle.Size = new Size(400, 20);
            this.lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // Form Panel
            // 
            this.pnlForm.Location = new Point(40, 120);
            this.pnlForm.Size = new Size(320, 260);
            this.pnlForm.BackColor = Color.White;
            this.pnlForm.Controls.Add(this.lblUsername);
            this.pnlForm.Controls.Add(this.txtUsername);
            this.pnlForm.Controls.Add(this.lblPassword);
            this.pnlForm.Controls.Add(this.pnlPasswordContainer);
            this.pnlForm.Controls.Add(this.chkRememberMe);
            this.pnlForm.Controls.Add(this.lblError);
            this.pnlForm.Controls.Add(this.progressBar);

            // 
            // Username Label
            // 
            this.lblUsername.Text = "Username";
            this.lblUsername.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.lblUsername.ForeColor = Color.FromArgb(51, 51, 51);
            this.lblUsername.Location = new Point(0, 20);
            this.lblUsername.Size = new Size(320, 20);

            // 
            // Username TextBox
            // 
            this.txtUsername.Location = new Point(0, 45);
            this.txtUsername.Size = new Size(320, 25);
            this.txtUsername.Font = new Font("Segoe UI", 11F);
            this.txtUsername.BorderStyle = BorderStyle.FixedSingle;
            this.txtUsername.BackColor = Color.White;
            this.txtUsername.ForeColor = Color.FromArgb(51, 51, 51);

            // 
            // Password Label
            // 
            this.lblPassword.Text = "Password";
            this.lblPassword.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.lblPassword.ForeColor = Color.FromArgb(51, 51, 51);
            this.lblPassword.Location = new Point(0, 85);
            this.lblPassword.Size = new Size(320, 20);

            // 
            // Password Container Panel
            // 
            this.pnlPasswordContainer.Location = new Point(0, 110);
            this.pnlPasswordContainer.Size = new Size(320, 25);
            this.pnlPasswordContainer.BorderStyle = BorderStyle.FixedSingle;
            this.pnlPasswordContainer.BackColor = Color.White;
            this.pnlPasswordContainer.Controls.Add(this.txtPassword);
            this.pnlPasswordContainer.Controls.Add(this.btnShowPassword);

            // 
            // Password TextBox
            // 
            this.txtPassword.Location = new Point(2, 2);
            this.txtPassword.Size = new Size(285, 21);
            this.txtPassword.Font = new Font("Segoe UI", 11F);
            this.txtPassword.BorderStyle = BorderStyle.None;
            this.txtPassword.BackColor = Color.White;
            this.txtPassword.ForeColor = Color.FromArgb(51, 51, 51);

            // 
            // Show Password Button
            // 
            this.btnShowPassword.Text = "👁";
            this.btnShowPassword.Location = new Point(290, 0);
            this.btnShowPassword.Size = new Size(28, 25);
            this.btnShowPassword.FlatStyle = FlatStyle.Flat;
            this.btnShowPassword.FlatAppearance.BorderSize = 0;
            this.btnShowPassword.BackColor = Color.White;
            this.btnShowPassword.ForeColor = Color.FromArgb(128, 128, 128);
            this.btnShowPassword.Font = new Font("Segoe UI", 9F);
            this.btnShowPassword.Cursor = Cursors.Hand;
            this.btnShowPassword.AccessibleDescription = "Show password";
            this.btnShowPassword.Click += BtnShowPassword_Click;

            // 
            // Remember Me CheckBox
            // 
            this.chkRememberMe.Text = "Remember me";
            this.chkRememberMe.Location = new Point(0, 150);
            this.chkRememberMe.Size = new Size(150, 25);
            this.chkRememberMe.Font = new Font("Segoe UI", 9F);
            this.chkRememberMe.ForeColor = Color.FromArgb(51, 51, 51);
            this.chkRememberMe.UseVisualStyleBackColor = true;

            // 
            // Error Label
            // 
            this.lblError.Location = new Point(0, 185);
            this.lblError.Size = new Size(320, 40);
            this.lblError.Font = new Font("Segoe UI", 9F);
            this.lblError.ForeColor = Color.FromArgb(231, 76, 60);
            this.lblError.TextAlign = ContentAlignment.TopLeft;
            this.lblError.Visible = false;
            this.lblError.AutoSize = false;

            // 
            // Progress Bar
            // 
            this.progressBar.Location = new Point(0, 235);
            this.progressBar.Size = new Size(320, 4);
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 30;
            this.progressBar.Visible = false;

            // 
            // Buttons Panel
            // 
            this.pnlButtons.Location = new Point(40, 390);
            this.pnlButtons.Size = new Size(320, 50);
            this.pnlButtons.BackColor = Color.White;
            this.pnlButtons.Controls.Add(this.btnLogin);
            this.pnlButtons.Controls.Add(this.btnCancel);

            // 
            // Login Button
            // 
            this.btnLogin.Text = "Sign In";
            this.btnLogin.Location = new Point(0, 0);
            this.btnLogin.Size = new Size(155, 40);
            this.btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnLogin.BackColor = Color.FromArgb(41, 128, 185);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.Cursor = Cursors.Hand;
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += BtnLogin_Click;

            // 
            // Cancel Button
            // 
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Location = new Point(165, 0);
            this.btnCancel.Size = new Size(155, 40);
            this.btnCancel.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            this.btnCancel.BackColor = Color.FromArgb(248, 248, 248);
            this.btnCancel.ForeColor = Color.FromArgb(51, 51, 51);
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.FlatAppearance.BorderSize = 1;
            this.btnCancel.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            this.btnCancel.Cursor = Cursors.Hand;
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += BtnCancel_Click;

            // 
            // Forgot Password Label
            // 
            this.lblForgotPassword.Text = "Forgot your password?";
            this.lblForgotPassword.Location = new Point(0, 460);
            this.lblForgotPassword.Size = new Size(400, 20);
            this.lblForgotPassword.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            this.lblForgotPassword.ForeColor = Color.FromArgb(41, 128, 185);
            this.lblForgotPassword.TextAlign = ContentAlignment.MiddleCenter;
            this.lblForgotPassword.Cursor = Cursors.Hand;
            this.lblForgotPassword.Click += LblForgotPassword_Click;

            // 
            // Version Label
            // 
            this.lblVersion.Text = "Version 1.0.0";
            this.lblVersion.Location = new Point(0, 490);
            this.lblVersion.Size = new Size(400, 20);
            this.lblVersion.Font = new Font("Segoe UI", 8F);
            this.lblVersion.ForeColor = Color.FromArgb(180, 180, 180);
            this.lblVersion.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(480, 600);
            this.Controls.Add(this.pnlMain);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "InventoryPro - Sign In";
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.ShowInTaskbar = true;
            this.TopMost = false;

            // Set icon (optional - you can add an icon resource)
            // this.Icon = Properties.Resources.AppIcon;

            // Resume layout
            this.pnlMain.ResumeLayout(false);
            this.pnlLogin.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlForm.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlPasswordContainer.ResumeLayout(false);
            this.pnlPasswordContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        #region Control Declarations

        // Main containers
        private Panel pnlMain;
        private Panel pnlLogin;
        private Panel pnlHeader;
        private Panel pnlForm;
        private Panel pnlButtons;
        private Panel pnlPasswordContainer;

        // Header elements
        private Label lblTitle;
        private Label lblSubtitle;
        private PictureBox picLogo;

        // Form elements
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnShowPassword;
        private CheckBox chkRememberMe;
        private Label lblError;
        private ProgressBar progressBar;

        // Buttons
        private Button btnLogin;
        private Button btnCancel;

        // Footer elements
        private Label lblForgotPassword;
        private Label lblVersion;

        #endregion
    }
}