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
            pnlMain = new Panel();
            pnlLogin = new Panel();
            pnlHeader = new Panel();
            picLogo = new PictureBox();
            lblTitle = new Label();
            lblSubtitle = new Label();
            lblVersion = new Label();
            lblForgotPassword = new Label();
            pnlButtons = new Panel();
            tlpButtons = new TableLayoutPanel();
            btnLogin = new Button();
            btnCancel = new Button();
            pnlForm = new Panel();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            pnlPasswordContainer = new Panel();
            txtPassword = new TextBox();
            btnShowPassword = new Button();
            chkRememberMe = new CheckBox();
            lblError = new Label();
            progressBar = new ProgressBar();
            pnlMain.SuspendLayout();
            pnlLogin.SuspendLayout();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            pnlButtons.SuspendLayout();
            tlpButtons.SuspendLayout();
            pnlForm.SuspendLayout();
            pnlPasswordContainer.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(240, 240, 240);
            pnlMain.Controls.Add(pnlLogin);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Margin = new Padding(3, 4, 3, 4);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(46, 53, 46, 53);
            pnlMain.Size = new Size(437, 604);
            pnlMain.TabIndex = 0;
            // 
            // pnlLogin
            // 
            pnlLogin.BackColor = Color.White;
            pnlLogin.Controls.Add(pnlHeader);
            pnlLogin.Controls.Add(lblVersion);
            pnlLogin.Controls.Add(lblForgotPassword);
            pnlLogin.Controls.Add(pnlButtons);
            pnlLogin.Controls.Add(pnlForm);
            pnlLogin.Dock = DockStyle.Fill;
            pnlLogin.Location = new Point(46, 53);
            pnlLogin.Margin = new Padding(3, 4, 3, 4);
            pnlLogin.Name = "pnlLogin";
            pnlLogin.Size = new Size(345, 498);
            pnlLogin.TabIndex = 0;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(picLogo);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 4, 3, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(345, 160);
            pnlHeader.TabIndex = 0;
            // 
            // picLogo
            // 
            picLogo.Anchor = AnchorStyles.None;
            picLogo.BackColor = Color.FromArgb(41, 128, 185);
            picLogo.Location = new Point(145, 27);
            picLogo.Margin = new Padding(3, 4, 3, 4);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(55, 64);
            picLogo.SizeMode = PictureBoxSizeMode.CenterImage;
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(51, 51, 51);
            lblTitle.Location = new Point(0, 100);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(345, 40);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "InventoryPro";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.ForeColor = Color.FromArgb(128, 128, 128);
            lblSubtitle.Location = new Point(0, 133);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(345, 27);
            lblSubtitle.TabIndex = 2;
            lblSubtitle.Text = "Sign in to your account";
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblVersion
            // 
            lblVersion.Dock = DockStyle.Bottom;
            lblVersion.Font = new Font("Segoe UI", 8F);
            lblVersion.ForeColor = Color.FromArgb(180, 180, 180);
            lblVersion.Location = new Point(0, 377);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(345, 27);
            lblVersion.TabIndex = 4;
            lblVersion.Text = "Version 1.0.0";
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblForgotPassword
            // 
            lblForgotPassword.Cursor = Cursors.Hand;
            lblForgotPassword.Dock = DockStyle.Bottom;
            lblForgotPassword.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            lblForgotPassword.ForeColor = Color.FromArgb(41, 128, 185);
            lblForgotPassword.Location = new Point(0, 404);
            lblForgotPassword.Name = "lblForgotPassword";
            lblForgotPassword.Size = new Size(345, 27);
            lblForgotPassword.TabIndex = 3;
            lblForgotPassword.Text = "Forgot your password?";
            lblForgotPassword.TextAlign = ContentAlignment.MiddleCenter;
            lblForgotPassword.Click += LblForgotPassword_Click;
            // 
            // pnlButtons
            // 
            pnlButtons.BackColor = Color.White;
            pnlButtons.Controls.Add(tlpButtons);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 431);
            pnlButtons.Margin = new Padding(3, 4, 3, 4);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Padding = new Padding(46, 7, 46, 7);
            pnlButtons.Size = new Size(345, 67);
            pnlButtons.TabIndex = 2;
            // 
            // tlpButtons
            // 
            tlpButtons.ColumnCount = 2;
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpButtons.Controls.Add(btnLogin, 0, 0);
            tlpButtons.Controls.Add(btnCancel, 1, 0);
            tlpButtons.Dock = DockStyle.Fill;
            tlpButtons.Location = new Point(46, 7);
            tlpButtons.Margin = new Padding(3, 4, 3, 4);
            tlpButtons.Name = "tlpButtons";
            tlpButtons.RowCount = 1;
            tlpButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpButtons.Size = new Size(253, 53);
            tlpButtons.TabIndex = 0;
            // 
            // btnLogin
            // 
            btnLogin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnLogin.BackColor = Color.FromArgb(41, 128, 185);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(0, 0);
            btnLogin.Margin = new Padding(0, 0, 6, 0);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(120, 31);
            btnLogin.TabIndex = 0;
            btnLogin.Text = "Sign In";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += BtnLogin_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnCancel.BackColor = Color.FromArgb(248, 248, 248);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 11F);
            btnCancel.ForeColor = Color.FromArgb(51, 51, 51);
            btnCancel.Location = new Point(132, 0);
            btnCancel.Margin = new Padding(6, 0, 0, 0);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(121, 31);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += BtnCancel_Click;
            // 
            // pnlForm
            // 
            pnlForm.BackColor = Color.White;
            pnlForm.Controls.Add(lblUsername);
            pnlForm.Controls.Add(txtUsername);
            pnlForm.Controls.Add(lblPassword);
            pnlForm.Controls.Add(pnlPasswordContainer);
            pnlForm.Controls.Add(chkRememberMe);
            pnlForm.Controls.Add(lblError);
            pnlForm.Controls.Add(progressBar);
            pnlForm.Dock = DockStyle.Fill;
            pnlForm.Location = new Point(0, 0);
            pnlForm.Margin = new Padding(3, 4, 3, 4);
            pnlForm.Name = "pnlForm";
            pnlForm.Padding = new Padding(46, 27, 46, 27);
            pnlForm.Size = new Size(345, 498);
            pnlForm.TabIndex = 1;
            // 
            // lblUsername
            // 
            lblUsername.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblUsername.Font = new Font("Segoe UI", 10F);
            lblUsername.ForeColor = Color.FromArgb(51, 51, 51);
            lblUsername.Location = new Point(0, 0);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(254, 27);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Username";
            // 
            // txtUsername
            // 
            txtUsername.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUsername.BackColor = Color.White;
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 11F);
            txtUsername.ForeColor = Color.FromArgb(51, 51, 51);
            txtUsername.Location = new Point(0, 33);
            txtUsername.Margin = new Padding(3, 4, 3, 4);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(253, 32);
            txtUsername.TabIndex = 1;
            // 
            // lblPassword
            // 
            lblPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblPassword.Font = new Font("Segoe UI", 10F);
            lblPassword.ForeColor = Color.FromArgb(51, 51, 51);
            lblPassword.Location = new Point(0, 80);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(254, 27);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Password";
            // 
            // pnlPasswordContainer
            // 
            pnlPasswordContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlPasswordContainer.BackColor = Color.White;
            pnlPasswordContainer.BorderStyle = BorderStyle.FixedSingle;
            pnlPasswordContainer.Controls.Add(txtPassword);
            pnlPasswordContainer.Controls.Add(btnShowPassword);
            pnlPasswordContainer.Location = new Point(0, 113);
            pnlPasswordContainer.Margin = new Padding(3, 4, 3, 4);
            pnlPasswordContainer.Name = "pnlPasswordContainer";
            pnlPasswordContainer.Padding = new Padding(2, 3, 2, 3);
            pnlPasswordContainer.Size = new Size(253, 35);
            pnlPasswordContainer.TabIndex = 3;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Dock = DockStyle.Fill;
            txtPassword.Font = new Font("Segoe UI", 11F);
            txtPassword.ForeColor = Color.FromArgb(51, 51, 51);
            txtPassword.Location = new Point(2, 3);
            txtPassword.Margin = new Padding(3, 4, 3, 4);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(215, 25);
            txtPassword.TabIndex = 0;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnShowPassword
            // 
            btnShowPassword.AccessibleDescription = "Show password";
            btnShowPassword.BackColor = Color.White;
            btnShowPassword.Cursor = Cursors.Hand;
            btnShowPassword.Dock = DockStyle.Right;
            btnShowPassword.FlatAppearance.BorderSize = 0;
            btnShowPassword.FlatStyle = FlatStyle.Flat;
            btnShowPassword.Font = new Font("Segoe UI", 9F);
            btnShowPassword.ForeColor = Color.FromArgb(128, 128, 128);
            btnShowPassword.Location = new Point(217, 3);
            btnShowPassword.Margin = new Padding(3, 4, 3, 4);
            btnShowPassword.Name = "btnShowPassword";
            btnShowPassword.Size = new Size(32, 27);
            btnShowPassword.TabIndex = 1;
            btnShowPassword.Text = "👁";
            btnShowPassword.UseVisualStyleBackColor = false;
            btnShowPassword.Click += BtnShowPassword_Click;
            // 
            // chkRememberMe
            // 
            chkRememberMe.Font = new Font("Segoe UI", 9F);
            chkRememberMe.ForeColor = Color.FromArgb(51, 51, 51);
            chkRememberMe.Location = new Point(0, 160);
            chkRememberMe.Margin = new Padding(3, 4, 3, 4);
            chkRememberMe.Name = "chkRememberMe";
            chkRememberMe.Size = new Size(171, 33);
            chkRememberMe.TabIndex = 4;
            chkRememberMe.Text = "Remember me";
            chkRememberMe.UseVisualStyleBackColor = true;
            // 
            // lblError
            // 
            lblError.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblError.Font = new Font("Segoe UI", 9F);
            lblError.ForeColor = Color.FromArgb(231, 76, 60);
            lblError.Location = new Point(0, 207);
            lblError.Name = "lblError";
            lblError.Size = new Size(254, 53);
            lblError.TabIndex = 5;
            lblError.Visible = false;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(0, 466);
            progressBar.Margin = new Padding(3, 4, 3, 4);
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(254, 5);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 6;
            progressBar.Visible = false;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 240, 240);
            ClientSize = new Size(437, 604);
            Controls.Add(pnlMain);
            Font = new Font("Segoe UI", 9F);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(455, 651);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "InventoryPro - Sign In";
            pnlMain.ResumeLayout(false);
            pnlLogin.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            pnlButtons.ResumeLayout(false);
            tlpButtons.ResumeLayout(false);
            pnlForm.ResumeLayout(false);
            pnlForm.PerformLayout();
            pnlPasswordContainer.ResumeLayout(false);
            pnlPasswordContainer.PerformLayout();
            ResumeLayout(false);
            }

        #endregion

        #region Control Declarations

        // Main containers
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlLogin;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlForm;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Panel pnlPasswordContainer;

        // Header elements
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.PictureBox picLogo;

        // Form elements
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnShowPassword;
        private System.Windows.Forms.CheckBox chkRememberMe;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.ProgressBar progressBar;

        // Buttons
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnCancel;

        // Footer elements
        private System.Windows.Forms.Label lblForgotPassword;
        private System.Windows.Forms.Label lblVersion;

        #endregion

        private TableLayoutPanel tlpButtons;
        }
}