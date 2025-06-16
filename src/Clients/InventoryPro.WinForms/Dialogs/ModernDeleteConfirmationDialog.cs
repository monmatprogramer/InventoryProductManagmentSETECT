using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryPro.WinForms.Dialogs
{
    /// <summary>
    /// A modern confirmation dialog for delete actions.
    /// </summary>
    public class ModernDeleteConfirmationDialog : Form
    {
        private readonly Label lblMessage;
        private readonly Button btnYes;
        private readonly Button btnNo;

        public ModernDeleteConfirmationDialog(string message)
        {
            this.Text = "Confirm Delete";
            this.Size = new Size(400, 220);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            lblMessage = new Label
            {
                Text = message,
                AutoSize = true,
                MaximumSize = new Size(340, 0),
                Location = new Point(30, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69)
            };

            btnYes = new Button
            {
                Text = "Yes",
                DialogResult = DialogResult.Yes,
                Size = new Size(100, 36),
                Location = new Point(80, 90),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnYes.FlatAppearance.BorderSize = 0;

            btnNo = new Button
            {
                Text = "No",
                DialogResult = DialogResult.No,
                Size = new Size(100, 36),
                Location = new Point(220, 90),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNo.FlatAppearance.BorderSize = 0;

            this.Controls.Add(lblMessage);
            this.Controls.Add(btnYes);
            this.Controls.Add(btnNo);

            

            lblMessage.AutoSize = true;
            lblMessage.MaximumSize = new Size(340, 0);
            lblMessage.SizeChanged += (s, e) =>
            {
                int buttonTop = lblMessage.Bottom + 20;
                btnYes.Location = new Point(80, buttonTop);
                btnNo.Location = new Point(220, buttonTop);
                this.Height = btnYes.Bottom + 50;
            };
            // Trigger initial layout
            lblMessage.PerformLayout();
            int initialButtonTop = lblMessage.Bottom + 20;
            btnYes.Location = new Point(80, initialButtonTop);
            btnNo.Location = new Point(220, initialButtonTop);
            this.Height = btnYes.Bottom + 50;

            this.AcceptButton = btnYes;
            this.CancelButton = btnNo;
            }
    }
}