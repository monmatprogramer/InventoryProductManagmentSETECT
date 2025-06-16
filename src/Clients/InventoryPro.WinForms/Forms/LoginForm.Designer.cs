using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace InventoryPro.WinForms.Forms
    {
    partial class LoginForm
        {
        /// <summary>
        /// Required designer variable for managing form components
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used when form is disposed
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
            {
            if (disposing)
                {
                // Dispose animation timers
                animationTimer?.Dispose();
                _buttonAnimationTimer?.Dispose();
                _spinnerTimer?.Dispose();
                
                // Dispose components
                components?.Dispose();
                }
            base.Dispose(disposing);
            }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// This method initializes all the UI components and sets their properties
        /// </summary>
        private void InitializeComponent()
            {
            components = new System.ComponentModel.Container();
            pnlBackground = new Panel();
            pnlLeftPanel = new Panel();
            pnlHeader = new Panel();
            picLogo = new PictureBox();
            lblAppTitle = new Label();
            lblAppSubtitle = new Label();
            pnlRightPanel = new Panel();
            pnlLoginContainer = new Panel();
            pnlLoginForm = new Panel();
            pnlUsernameContainer = new Panel();
            lblUsernameIcon = new Label();
            txtUsername = new TextBox();
            pnlPasswordContainer = new Panel();
            lblPasswordIcon = new Label();
            txtPassword = new TextBox();
            btnTogglePassword = new Button();
            chkRememberMe = new CheckBox();
            lblForgotPassword = new Label();
            btnLogin = new Button();
            btnCancel = new Button();
            lblError = new Label();
            progressBar = new ProgressBar();
            pnlFooter = new Panel();
            lblVersion = new Label();
            lblCopyright = new Label();
            lblUsername = new Label();
            lblPassword = new Label();
            animationTimer = new Timer(components);
            pnlBackground.SuspendLayout();
            pnlLeftPanel.SuspendLayout();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            pnlRightPanel.SuspendLayout();
            pnlLoginContainer.SuspendLayout();
            pnlLoginForm.SuspendLayout();
            pnlUsernameContainer.SuspendLayout();
            pnlPasswordContainer.SuspendLayout();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            // 
            // pnlBackground
            // 
            pnlBackground.BackColor = Color.FromArgb(240, 244, 248);
            pnlBackground.Controls.Add(pnlLeftPanel);
            pnlBackground.Controls.Add(pnlRightPanel);
            pnlBackground.Dock = DockStyle.Fill;
            pnlBackground.Location = new Point(0, 0);
            pnlBackground.Name = "pnlBackground";
            pnlBackground.Size = new Size(1395, 752);
            pnlBackground.TabIndex = 0;
            // 
            // pnlLeftPanel
            // 
            pnlLeftPanel.BackColor = Color.FromArgb(45, 108, 175);
            pnlLeftPanel.Controls.Add(pnlHeader);
            pnlLeftPanel.Dock = DockStyle.Left;
            pnlLeftPanel.Location = new Point(0, 0);
            pnlLeftPanel.Name = "pnlLeftPanel";
            pnlLeftPanel.Padding = new Padding(60, 80, 60, 80);
            pnlLeftPanel.Size = new Size(500, 752);
            pnlLeftPanel.TabIndex = 0;
            pnlLeftPanel.Paint += PnlLeftPanel_Paint;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.Transparent;
            pnlHeader.Controls.Add(picLogo);
            pnlHeader.Controls.Add(lblAppTitle);
            pnlHeader.Controls.Add(lblAppSubtitle);
            pnlHeader.Dock = DockStyle.Fill;
            pnlHeader.Location = new Point(60, 80);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(380, 592);
            pnlHeader.TabIndex = 0;
            // 
            // picLogo
            // 
            picLogo.Anchor = AnchorStyles.None;
            picLogo.BackColor = Color.White;
            picLogo.Location = new Point(118, 75);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(80, 80);
            picLogo.SizeMode = PictureBoxSizeMode.CenterImage;
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            picLogo.Paint += PicLogo_Paint;
            // 
            // lblAppTitle
            // 
            lblAppTitle.Anchor = AnchorStyles.None;
            lblAppTitle.AutoSize = true;
            lblAppTitle.BackColor = Color.Transparent;
            lblAppTitle.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblAppTitle.ForeColor = Color.White;
            lblAppTitle.Location = new Point(62, 199);
            lblAppTitle.Name = "lblAppTitle";
            lblAppTitle.Size = new Size(318, 62);
            lblAppTitle.TabIndex = 1;
            lblAppTitle.Text = "InventoryPro";
            lblAppTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblAppSubtitle
            // 
            lblAppSubtitle.Anchor = AnchorStyles.None;
            lblAppSubtitle.AutoSize = true;
            lblAppSubtitle.BackColor = Color.Transparent;
            lblAppSubtitle.Font = new Font("Segoe UI", 12F);
            lblAppSubtitle.ForeColor = Color.FromArgb(220, 230, 240);
            lblAppSubtitle.Location = new Point(51, 282);
            lblAppSubtitle.Name = "lblAppSubtitle";
            lblAppSubtitle.Size = new Size(326, 28);
            lblAppSubtitle.TabIndex = 2;
            lblAppSubtitle.Text = "Professional Inventory Management";
            lblAppSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlRightPanel
            // 
            pnlRightPanel.BackColor = Color.White;
            pnlRightPanel.Controls.Add(pnlLoginContainer);
            pnlRightPanel.Controls.Add(pnlFooter);
            pnlRightPanel.Dock = DockStyle.Fill;
            pnlRightPanel.Location = new Point(0, 0);
            pnlRightPanel.Name = "pnlRightPanel";
            pnlRightPanel.Padding = new Padding(60, 40, 60, 40);
            pnlRightPanel.Size = new Size(1395, 752);
            pnlRightPanel.TabIndex = 1;
            // 
            // pnlLoginContainer
            // 
            pnlLoginContainer.Anchor = AnchorStyles.None;
            pnlLoginContainer.BackColor = Color.White;
            pnlLoginContainer.Controls.Add(pnlLoginForm);
            pnlLoginContainer.Location = new Point(671, 80);
            pnlLoginContainer.Name = "pnlLoginContainer";
            pnlLoginContainer.Padding = new Padding(30);
            pnlLoginContainer.Size = new Size(602, 492);
            pnlLoginContainer.TabIndex = 0;
            pnlLoginContainer.Paint += PnlLoginContainer_Paint;
            // 
            // pnlLoginForm
            // 
            pnlLoginForm.BackColor = Color.Transparent;
            pnlLoginForm.Controls.Add(pnlUsernameContainer);
            pnlLoginForm.Controls.Add(pnlPasswordContainer);
            pnlLoginForm.Controls.Add(chkRememberMe);
            pnlLoginForm.Controls.Add(lblForgotPassword);
            pnlLoginForm.Controls.Add(btnLogin);
            pnlLoginForm.Controls.Add(btnCancel);
            pnlLoginForm.Controls.Add(lblError);
            pnlLoginForm.Controls.Add(progressBar);
            pnlLoginForm.Dock = DockStyle.Fill;
            pnlLoginForm.Location = new Point(30, 30);
            pnlLoginForm.Name = "pnlLoginForm";
            pnlLoginForm.Padding = new Padding(0, 20, 0, 0);
            pnlLoginForm.Size = new Size(542, 432);
            pnlLoginForm.TabIndex = 0;
            // 
            // pnlUsernameContainer
            // 
            pnlUsernameContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlUsernameContainer.BackColor = Color.FromArgb(248, 249, 250);
            pnlUsernameContainer.Controls.Add(lblUsernameIcon);
            pnlUsernameContainer.Controls.Add(txtUsername);
            pnlUsernameContainer.Location = new Point(0, 80);
            pnlUsernameContainer.Name = "pnlUsernameContainer";
            pnlUsernameContainer.Size = new Size(542, 50);
            pnlUsernameContainer.TabIndex = 0;
            pnlUsernameContainer.Paint += InputContainer_Paint;
            // 
            // lblUsernameIcon
            // 
            lblUsernameIcon.BackColor = Color.Transparent;
            lblUsernameIcon.Font = new Font("Segoe UI", 12F);
            lblUsernameIcon.ForeColor = Color.FromArgb(107, 114, 128);
            lblUsernameIcon.Location = new Point(15, 12);
            lblUsernameIcon.Name = "lblUsernameIcon";
            lblUsernameIcon.Size = new Size(25, 25);
            lblUsernameIcon.TabIndex = 0;
            lblUsernameIcon.Text = "👤";
            lblUsernameIcon.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.FromArgb(248, 249, 250);
            txtUsername.BorderStyle = BorderStyle.None;
            txtUsername.Font = new Font("Segoe UI", 11F);
            txtUsername.ForeColor = Color.FromArgb(17, 24, 39);
            txtUsername.Location = new Point(50, 12);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Enter your username";
            txtUsername.Size = new Size(489, 25);
            txtUsername.TabIndex = 1;
            txtUsername.Enter += TextBox_Enter;
            txtUsername.Leave += TextBox_Leave;
            // 
            // pnlPasswordContainer
            // 
            pnlPasswordContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlPasswordContainer.BackColor = Color.FromArgb(248, 249, 250);
            pnlPasswordContainer.Controls.Add(lblPasswordIcon);
            pnlPasswordContainer.Controls.Add(txtPassword);
            pnlPasswordContainer.Controls.Add(btnTogglePassword);
            pnlPasswordContainer.Location = new Point(0, 180);
            pnlPasswordContainer.Name = "pnlPasswordContainer";
            pnlPasswordContainer.Size = new Size(539, 50);
            pnlPasswordContainer.TabIndex = 1;
            pnlPasswordContainer.Paint += InputContainer_Paint;
            // 
            // lblPasswordIcon
            // 
            lblPasswordIcon.BackColor = Color.Transparent;
            lblPasswordIcon.Font = new Font("Segoe UI", 12F);
            lblPasswordIcon.ForeColor = Color.FromArgb(107, 114, 128);
            lblPasswordIcon.Location = new Point(15, 12);
            lblPasswordIcon.Name = "lblPasswordIcon";
            lblPasswordIcon.Size = new Size(25, 25);
            lblPasswordIcon.TabIndex = 0;
            lblPasswordIcon.Text = "🔒";
            lblPasswordIcon.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(248, 249, 250);
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Segoe UI", 11F);
            txtPassword.ForeColor = Color.FromArgb(17, 24, 39);
            txtPassword.Location = new Point(50, 12);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Enter your password";
            txtPassword.Size = new Size(443, 25);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Enter += TextBox_Enter;
            txtPassword.Leave += TextBox_Leave;
            // 
            // btnTogglePassword
            // 
            btnTogglePassword.BackColor = Color.Transparent;
            btnTogglePassword.Cursor = Cursors.Hand;
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            btnTogglePassword.FlatStyle = FlatStyle.Flat;
            btnTogglePassword.Font = new Font("Segoe UI", 10F);
            btnTogglePassword.ForeColor = Color.FromArgb(107, 114, 128);
            btnTogglePassword.Location = new Point(499, 11);
            btnTogglePassword.Name = "btnTogglePassword";
            btnTogglePassword.Size = new Size(30, 30);
            btnTogglePassword.TabIndex = 2;
            btnTogglePassword.Text = "👁";
            btnTogglePassword.UseVisualStyleBackColor = false;
            btnTogglePassword.Click += BtnTogglePassword_Click;
            // 
            // chkRememberMe
            // 
            chkRememberMe.AutoSize = true;
            chkRememberMe.Font = new Font("Segoe UI", 9F);
            chkRememberMe.ForeColor = Color.FromArgb(75, 85, 99);
            chkRememberMe.Location = new Point(0, 250);
            chkRememberMe.Name = "chkRememberMe";
            chkRememberMe.Size = new Size(129, 24);
            chkRememberMe.TabIndex = 3;
            chkRememberMe.Text = "Remember me";
            chkRememberMe.UseVisualStyleBackColor = true;
            // 
            // lblForgotPassword
            // 
            lblForgotPassword.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblForgotPassword.AutoSize = true;
            lblForgotPassword.Cursor = Cursors.Hand;
            lblForgotPassword.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            lblForgotPassword.ForeColor = Color.FromArgb(59, 130, 246);
            lblForgotPassword.Location = new Point(415, 250);
            lblForgotPassword.Name = "lblForgotPassword";
            lblForgotPassword.Size = new Size(127, 20);
            lblForgotPassword.TabIndex = 4;
            lblForgotPassword.Text = "Forgot password?";
            lblForgotPassword.Click += LblForgotPassword_Click;
            lblForgotPassword.MouseEnter += LinkLabel_MouseEnter;
            lblForgotPassword.MouseLeave += LinkLabel_MouseLeave;
            // 
            // btnLogin
            // 
            btnLogin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnLogin.BackColor = Color.FromArgb(59, 130, 246);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(0, 325);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(542, 45);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "Sign In";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += BtnLogin_Click;
            btnLogin.Paint += BtnLogin_Paint;
            btnLogin.MouseEnter += Button_MouseEnter;
            btnLogin.MouseLeave += Button_MouseLeave;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnCancel.BackColor = Color.FromArgb(249, 250, 251);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 11F);
            btnCancel.ForeColor = Color.FromArgb(75, 85, 99);
            btnCancel.Location = new Point(0, 380);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(542, 45);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += BtnCancel_Click;
            btnCancel.MouseEnter += Button_MouseEnter;
            btnCancel.MouseLeave += Button_MouseLeave;
            // 
            // lblError
            // 
            lblError.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblError.Font = new Font("Segoe UI", 9F);
            lblError.ForeColor = Color.FromArgb(239, 68, 68);
            lblError.Location = new Point(0, 280);
            lblError.Name = "lblError";
            lblError.Size = new Size(542, 40);
            lblError.TabIndex = 7;
            lblError.TextAlign = ContentAlignment.MiddleLeft;
            lblError.Visible = false;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(0, 747);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(622, 3);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 8;
            progressBar.Visible = false;
            // 
            // pnlFooter
            // 
            pnlFooter.BackColor = Color.Transparent;
            pnlFooter.Controls.Add(lblVersion);
            pnlFooter.Controls.Add(lblCopyright);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(60, 652);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(1275, 60);
            pnlFooter.TabIndex = 1;
            // 
            // lblVersion
            // 
            lblVersion.Dock = DockStyle.Top;
            lblVersion.Font = new Font("Segoe UI", 8F);
            lblVersion.ForeColor = Color.FromArgb(156, 163, 175);
            lblVersion.Location = new Point(0, 20);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(1275, 20);
            lblVersion.TabIndex = 0;
            lblVersion.Text = "Version 1.0.0";
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCopyright
            // 
            lblCopyright.Dock = DockStyle.Top;
            lblCopyright.Font = new Font("Segoe UI", 8F);
            lblCopyright.ForeColor = Color.FromArgb(156, 163, 175);
            lblCopyright.Location = new Point(0, 0);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Size = new Size(1275, 20);
            lblCopyright.TabIndex = 1;
            lblCopyright.Text = "© 2025 InventoryPro. All rights reserved.";
            lblCopyright.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new Font("Segoe UI", 10F);
            lblUsername.ForeColor = Color.FromArgb(55, 65, 81);
            lblUsername.Location = new Point(0, 50);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(87, 23);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Username";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Segoe UI", 10F);
            lblPassword.ForeColor = Color.FromArgb(55, 65, 81);
            lblPassword.Location = new Point(0, 150);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(80, 23);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Password";
            // 
            // animationTimer
            // 
            animationTimer.Interval = 16;
            animationTimer.Tick += AnimationTimer_Tick;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1395, 752);
            Controls.Add(pnlBackground);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "InventoryPro - Sign In";
            pnlBackground.ResumeLayout(false);
            pnlLeftPanel.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            pnlRightPanel.ResumeLayout(false);
            pnlLoginContainer.ResumeLayout(false);
            pnlLoginForm.ResumeLayout(false);
            pnlLoginForm.PerformLayout();
            pnlUsernameContainer.ResumeLayout(false);
            pnlUsernameContainer.PerformLayout();
            pnlPasswordContainer.ResumeLayout(false);
            pnlPasswordContainer.PerformLayout();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
            }

        #endregion

        #region Control Declarations

        // Main layout containers
        private Panel pnlBackground;
        private Panel pnlLeftPanel;
        private Panel pnlRightPanel;
        private Panel pnlLoginContainer;
        private Panel pnlHeader;
        private Panel pnlLoginForm;
        private Panel pnlFooter;

        // Input containers for modern styling
        private Panel pnlUsernameContainer;
        private Panel pnlPasswordContainer;

        // Header elements
        private PictureBox picLogo;
        private Label lblAppTitle;
        private Label lblAppSubtitle;

        // Form input elements
        private Label lblUsername;
        private Label lblUsernameIcon;
        private TextBox txtUsername;
        private Label lblPassword;
        private Label lblPasswordIcon;
        private TextBox txtPassword;
        private Button btnTogglePassword;

        // Form controls
        private CheckBox chkRememberMe;
        private Label lblForgotPassword;
        private Button btnLogin;
        private Button btnCancel;
        private Label lblError;
        private ProgressBar progressBar;

        // Footer elements
        private Label lblVersion;
        private Label lblCopyright;

        // Animation timer for smooth UI transitions
        private Timer animationTimer;

        #endregion
        }
    }