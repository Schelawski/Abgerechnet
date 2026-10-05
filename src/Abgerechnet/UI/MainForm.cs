using System.Diagnostics;
using Abgerechnet.Core;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.UI;

/// <summary>
/// Main window. For now the frame: menu, a placeholder where the invoice list will go (issue #5) and the
/// invoice folder in the status bar.
/// </summary>
internal sealed class MainForm : Form
{
    private readonly SettingsStore _settingsStore;
    private readonly AppSettings _settings;
    private readonly ToolStripStatusLabel _folderLabel = new()
    {
        IsLink = true,
        Spring = true,
        TextAlign = ContentAlignment.MiddleLeft,
        ToolTipText = UiText.FolderStatusTooltip,
    };

    public MainForm(SettingsStore settingsStore, AppSettings settings, DataFolder folder)
    {
        _settingsStore = settingsStore;
        _settings = settings;
        Folder = folder;

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

        var statusStrip = new StatusStrip { SizingGrip = true };
        statusStrip.Items.Add(_folderLabel);
        _folderLabel.Click += (_, _) => OpenFolderInExplorer();

        Controls.Add(emptyState);
        Controls.Add(statusStrip);
        Controls.Add(CreateMenu());
        ResumeLayout(false);
        PerformLayout();

        RestorePlacement();
        OnFolderChanged();
    }

    /// <summary>The open invoice folder.</summary>
    public DataFolder Folder { get; private set; }

    private MenuStrip CreateMenu()
    {
        var fileMenu = new ToolStripMenuItem(UiText.MenuFile);
        fileMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuChangeFolder, null, (_, _) => ChangeFolder()));
        fileMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuOpenFolder, null, (_, _) => OpenFolderInExplorer()));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuExit, null, (_, _) => Close()));

        var aboutItem = new ToolStripMenuItem(UiText.MenuAbout, null, (_, _) =>
            MessageBox.Show(this, UiText.AboutText(AppVersion.Current), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information));
        var helpMenu = new ToolStripMenuItem(UiText.MenuHelp, null, aboutItem);

        var menu = new MenuStrip { Dock = DockStyle.Top };
        menu.Items.AddRange([fileMenu, helpMenu]);
        MainMenuStrip = menu;
        return menu;
    }

    private void ChangeFolder()
    {
        var folder = DataFolderDialogs.ChooseAndOpen(this, Folder.FolderPath);
        if (folder is null)
            return;

        Folder = folder;
        OnFolderChanged();
    }

    private void OnFolderChanged()
    {
        _folderLabel.Text = UiText.FolderStatus(Folder.FolderPath);
        if (!string.Equals(_settings.DataFolder, Folder.FolderPath, StringComparison.OrdinalIgnoreCase))
        {
            _settings.DataFolder = Folder.FolderPath;
            SaveSettings();
        }
    }

    private void OpenFolderInExplorer()
    {
        Process.Start(new ProcessStartInfo { FileName = Folder.FolderPath, UseShellExecute = true })?.Dispose();
    }

    private void SaveSettings()
    {
        try
        {
            _settingsStore.Save(_settings);
        }
        catch (IOException ex)
        {
            MessageBox.Show(this, UiText.SettingsNotSaved(ex.Message), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ----- Window position -----

    private void RestorePlacement()
    {
        if (_settings.MainWindow is not { } placement)
            return;

        var bounds = new Rectangle(placement.X, placement.Y, placement.Width, placement.Height);
        // Only restore when the window would be visible, e.g. not on a monitor that was unplugged.
        if (!Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(bounds)))
            return;

        StartPosition = FormStartPosition.Manual;
        Bounds = bounds;
        if (placement.Maximized)
            WindowState = FormWindowState.Maximized;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.Cancel)
            return;

        var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        _settings.MainWindow = new WindowPlacement(bounds.X, bounds.Y, bounds.Width, bounds.Height, WindowState == FormWindowState.Maximized);
        SaveSettings();
    }
}
