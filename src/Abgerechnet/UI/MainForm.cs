using Abgerechnet.Core;

namespace Abgerechnet.UI;

/// <summary>
/// Main window. For now only the frame: menu and a placeholder where the invoice list will go (issue #5).
/// </summary>
internal sealed class MainForm : Form
{
    public MainForm()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = $"{UiText.AppTitle} {AppVersion.Current}";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1000, 680);
        MinimumSize = new Size(760, 500);

        var emptyState = new Label
        {
            Text = UiText.EmptyState,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiStyle.MutedText,
            Font = new Font(Font.FontFamily, 11f),
        };

        Controls.Add(emptyState);
        Controls.Add(CreateMenu());
        ResumeLayout(false);
        PerformLayout();
    }

    private MenuStrip CreateMenu()
    {
        var exitItem = new ToolStripMenuItem(UiText.MenuExit, null, (_, _) => Close());
        var fileMenu = new ToolStripMenuItem(UiText.MenuFile, null, exitItem);

        var aboutItem = new ToolStripMenuItem(UiText.MenuAbout, null, (_, _) =>
            MessageBox.Show(this, UiText.AboutText(AppVersion.Current), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information));
        var helpMenu = new ToolStripMenuItem(UiText.MenuHelp, null, aboutItem);

        var menu = new MenuStrip { Dock = DockStyle.Top };
        menu.Items.AddRange([fileMenu, helpMenu]);
        MainMenuStrip = menu;
        return menu;
    }
}
