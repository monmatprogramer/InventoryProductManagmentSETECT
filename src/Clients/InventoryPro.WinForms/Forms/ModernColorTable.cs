using System.Drawing;
using System.Windows.Forms;

public class ModernColorTable : ProfessionalColorTable
{
    public override Color ToolStripGradientBegin => Color.FromArgb(52, 58, 64);
    public override Color ToolStripGradientMiddle => Color.FromArgb(52, 58, 64);
    public override Color ToolStripGradientEnd => Color.FromArgb(52, 58, 64);
    public override Color MenuItemSelected => Color.FromArgb(0, 123, 255);
    public override Color MenuItemBorder => Color.FromArgb(0, 105, 217);
}