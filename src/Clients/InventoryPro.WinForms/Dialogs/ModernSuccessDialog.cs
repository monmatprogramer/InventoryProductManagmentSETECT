using System.Drawing;
using System.Windows.Forms;

namespace InventoryPro.WinForms.Dialogs
{
    public class ModernSuccessDialog : Form
    {
        private Label lblIcon = null!;
        private Label lblTitle = null!;
        private Label lblMessage = null!;
        private Button btnOK = null!;

        public ModernSuccessDialog(string message, string? title = null)
        {
            InitializeComponent();
            lblMessage.Text = message;
            lblTitle.Text = title ?? "Success";
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(400, 200);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);
            this.Padding = new Padding(0);
            this.ShowInTaskbar = false;

            // Rounded corners
            this.Region = System.Drawing.Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, this.Width, this.Height, 18, 18));

            lblIcon = new Label
            {
                Text = "✅",
                Font = new Font("Segoe UI Emoji", 36F, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 167, 69),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                MaximumSize = new Size(10, 0),
                Location = new Point(30, 30)
            };

            lblTitle = new Label
            {
                Text = "Success",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(100, 35),
                Size = new Size(270, 35)
            };

            lblMessage = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(52, 58, 64),
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(40, 80),
                Size = new Size(320, 50)
            };

            btnOK = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Size = new Size(100, 38),
                Location = new Point(150, 140),
                TabStop = false
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += (s, e) => this.Close();

            this.Controls.Add(lblIcon);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblMessage);
            this.Controls.Add(btnOK);
        }

        // Native method for rounded corners
        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll", SetLastError = true)]
            public static extern IntPtr CreateRoundRectRgn(
                int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        }
    }
}