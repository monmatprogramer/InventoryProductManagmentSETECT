using System;
using System.Drawing;
using System.Drawing.Imaging.Effects;
using System.Windows.Forms;

namespace InventoryPro.WinForms.Dialogs
    {
    /// <summary>
    /// A modern and professional delete confirmation dialog with polished UI/UX.
    /// </summary>
    public class ModernDeleteConfirmationDialog : Form
        {
        private readonly Label lblMessage;
        private readonly Button btnYes;
        private readonly Button btnNo;
        private readonly Panel headerPanel;
        private readonly PictureBox icon;
        private readonly Panel buttonPanel;

        public ModernDeleteConfirmationDialog(string message)
            {
            this.Text = "Confirm Delete";
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(420, 280);//240
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.OrangeRed;
            this.ForeColor = Color.White;
            
            this.Padding = new Padding(1);

            // Rounded corners (Windows 11 style)
            this.Region = System.Drawing.Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, Width, Height, 12, 12));

            headerPanel = new Panel
                {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(245, 245, 245)
                };

            icon = new PictureBox
                {
                Image = SystemIcons.Warning.ToBitmap(),
                Size = new Size(32, 32),
                Location = new Point(20, 9),
                SizeMode = PictureBoxSizeMode.StretchImage
                };

            var lblTitle = new Label
                {
                Text = "Are you sure?",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(60, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(33, 37, 41)
                };

            lblMessage = new Label
                {
                Text = message,
                MaximumSize = new Size(360, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                Location = new Point(30, 70),
                TextAlign = ContentAlignment.MiddleLeft
                };

            btnYes = new Button
                {
                Text = "Delete",
                DialogResult = DialogResult.Yes,
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
                };
            btnYes.FlatAppearance.BorderSize = 0;

            btnNo = new Button
                {
                Text = "Cancel",
                DialogResult = DialogResult.No,
                Size = new Size(120, 40),//40
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
                };
            btnNo.FlatAppearance.BorderSize = 0;

            buttonPanel = new Panel
                {
                Dock = DockStyle.Bottom,
                Height = 80,
                Padding = new Padding(0, 10, 0, 10)
                };

            Controls.Add(headerPanel);
            Controls.Add(lblMessage);
            Controls.Add(buttonPanel);

            headerPanel.Controls.Add(icon);
            headerPanel.Controls.Add(lblTitle);
            buttonPanel.Controls.Add(btnYes);
            buttonPanel.Controls.Add(btnNo);

            // Initial layout
            this.Load += (s, e) =>
            {
                int labelBottom = lblMessage.Bottom;
                int centerX = (ClientSize.Width - btnYes.Width - btnNo.Width - 20) / 2;
                btnYes.Location = new Point(centerX, 20);
                btnNo.Location = new Point(centerX + btnYes.Width + 20, 20);
                lblMessage.Location = new Point((ClientSize.Width - lblMessage.Width) / 2, 70);
            };

            this.AcceptButton = btnYes;
            this.CancelButton = btnNo;
            }

        private static class NativeMethods
            {
            [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
            public static extern IntPtr CreateRoundRectRgn
            (
                int nLeftRect, int nTopRect,
                int nRightRect, int nBottomRect,
                int nWidthEllipse, int nHeightEllipse
            );
            }
        }
    }
