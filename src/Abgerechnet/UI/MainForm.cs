using System.Diagnostics;
using Abgerechnet.Core;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.UI;

/// <summary>
/// Main window: menu, a hint while the sender data is missing, the invoice list (issue #5) and the
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
    private readonly TableLayoutPanel _meineDatenBar = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 2,
        RowCount = 1,
        BackColor = UiStyle.HintBack,
        Padding = new Padding(10, 6, 10, 6),
        Visible = false,
    };

    private readonly RechnungenView _rechnungen = new();

    public MainForm(SettingsStore settingsStore, AppSettings settings, DataFolder folder)
    {
        _settingsStore = settingsStore;
        _settings = settings;
        Folder = folder;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1000, 680);
        MinimumSize = new Size(760, 500);

        var statusStrip = new StatusStrip { SizingGrip = true };
        statusStrip.Items.Add(_folderLabel);
        _folderLabel.Click += (_, _) => OpenFolderInExplorer();

        BuildMeineDatenBar();

        Controls.Add(_rechnungen);
        Controls.Add(_meineDatenBar);
        Controls.Add(statusStrip);
        Controls.Add(CreateMenu());
        ResumeLayout(false);
        PerformLayout();

        RestorePlacement();
        UiStyle.EnableHelpKey(this, Core.Help.HelpTopics.FirstSteps);
        OnFolderChanged();
    }

    /// <summary>The open invoice folder.</summary>
    public DataFolder Folder { get; private set; }

    private MenuStrip CreateMenu()
    {
        var fileMenu = new ToolStripMenuItem(UiText.MenuFile);
        fileMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuNeueRechnung, null, (_, _) => _rechnungen.NeueRechnung(), Keys.Control | Keys.N));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuMeineDaten, null, (_, _) => EditMeineDaten()));
        fileMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuKunden, null, (_, _) => EditKunden()));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuChangeFolder, null, (_, _) => ChangeFolder()));
        fileMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuOpenFolder, null, (_, _) => OpenFolderInExplorer()));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuExit, null, (_, _) => Close()));

        var vorlagenMenu = new ToolStripMenuItem(UiText.MenuVorlagen);
        vorlagenMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuVorlagenOrdner, null, (_, _) => OpenVorlagenOrdner()));
        vorlagenMenu.DropDownItems.Add(new ToolStripSeparator());
        var wiederherstellen = new ToolStripMenuItem(UiText.MenuOriginalWiederherstellen);
        foreach (var name in Core.Vorlagen.MitgelieferteVorlagen.Namen)
            wiederherstellen.DropDownItems.Add(new ToolStripMenuItem(UiText.VorlagenName(name), null, (_, _) => VorlageWiederherstellen(name)));
        vorlagenMenu.DropDownItems.Add(wiederherstellen);
        vorlagenMenu.DropDownItems.Add(new ToolStripSeparator());
        vorlagenMenu.DropDownItems.Add(new ToolStripMenuItem(UiText.MenuKiAnpassen, null, (_, _) => HelpForm.Open(this, Core.Help.HelpTopics.Ki)));

        var aboutItem = new ToolStripMenuItem(UiText.MenuAbout, null, (_, _) =>
            MessageBox.Show(this, UiText.AboutText(AppVersion.Current), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information));
        var hilfeItem = new ToolStripMenuItem(UiText.MenuHilfeOeffnen, null, (_, _) => HelpForm.Open(this, Core.Help.HelpTopics.FirstSteps)) { ShortcutKeyDisplayString = "F1" };
        var helpMenu = new ToolStripMenuItem(UiText.MenuHelp, null, hilfeItem, new ToolStripSeparator(), aboutItem);

        var menu = new MenuStrip { Dock = DockStyle.Top };
        menu.Items.AddRange([fileMenu, vorlagenMenu, helpMenu]);
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

    private void BuildMeineDatenBar()
    {
        _meineDatenBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _meineDatenBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var text = new Label { Text = UiText.MeineDatenMissing, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 12, 3) };
        var button = UiStyle.CreateButton(UiText.MeineDatenButton);
        button.Anchor = AnchorStyles.Right;
        button.Click += (_, _) => EditMeineDaten();
        _meineDatenBar.Controls.Add(text, 0, 0);
        _meineDatenBar.Controls.Add(button, 1, 0);
    }

    private void EditMeineDaten()
    {
        using var dialog = new MeineDatenDialog(Folder);
        if (dialog.ShowDialog(this) == DialogResult.OK)
            OnEinstellungenChanged();
    }

    private void OpenVorlagenOrdner()
    {
        Directory.CreateDirectory(Folder.VorlagenPath);
        Process.Start(new ProcessStartInfo { FileName = Folder.VorlagenPath, UseShellExecute = true })?.Dispose();
    }

    private void VorlageWiederherstellen(string name)
    {
        if (MessageBox.Show(this, UiText.WiederherstellenFrage(name), UiText.AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        try
        {
            var sicherung = Core.Vorlagen.MitgelieferteVorlagen.Wiederherstellen(Folder, name);
            MessageBox.Show(this, UiText.Wiederhergestellt(name, sicherung), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, UiText.DataFileNotWritable(Folder.VorlagenPath, ex.Message), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EditKunden()
    {
        using var dialog = new KundenDialog(Folder);
        if (dialog.ShowDialog(this) == DialogResult.OK)
            _rechnungen.Aktualisieren(); // customer names in the list
    }

    /// <summary>Window title and hint bar follow the sender's data.</summary>
    private void OnEinstellungenChanged()
    {
        var absender = Folder.Einstellungen.Absender;
        Text = absender.Anzeigename.Length > 0
            ? $"{absender.Anzeigename} – {UiText.AppTitle} {AppVersion.Current}"
            : $"{UiText.AppTitle} {AppVersion.Current}";
        _meineDatenBar.Visible = !absender.IstVollstaendig;
    }

    private void OnFolderChanged()
    {
        HelpForm.Folder = Folder;
        OnEinstellungenChanged();
        _rechnungen.SetFolder(Folder);
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
