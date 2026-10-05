using Abgerechnet.Core;
using Abgerechnet.Core.Einrichtung;
using Abgerechnet.Core.Help;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.UI.Einrichtung;

/// <summary>How the wizard ended.</summary>
internal enum EinrichtungsErgebnis
{
    /// <summary>Closed (or finished); <see cref="EinrichtungsAssistent.Folder"/> tells whether a folder was chosen.</summary>
    Beendet,

    /// <summary>Abgerechnet was copied to the user folder and the copy was started; this instance should exit.</summary>
    KopieGestartet,
}

/// <summary>
/// Welcome wizard (issue #11): welcome → invoice folder → sender's data → template → done. Opens at the first start
/// (no invoice folder yet) and later via "Datei → Einrichtungsassistent…". Every page saves what it set up when the
/// user goes on, so closing the wizard after the folder page simply opens the main window with that folder.
/// </summary>
internal sealed class EinrichtungsAssistent : Form
{
    private readonly SettingsStore _store;

    private readonly Label _schrittLabel = new() { AutoSize = true, ForeColor = UiStyle.MutedText, Margin = new Padding(3, 0, 3, 2) };
    private readonly Label _titelLabel = new() { AutoSize = true, UseMnemonic = false, Margin = new Padding(0, 0, 3, 0) };
    private readonly Panel _seitenHost = new() { Dock = DockStyle.Fill, Padding = new Padding(20, 14, 20, 6) };
    private readonly Button _zurueck = UiStyle.CreateButton(UiText.EinrichtungZurueck);
    private readonly Button _primaer = UiStyle.CreateButton(UiText.EinrichtungWeiter);
    private readonly Button _sekundaer = UiStyle.CreateButton(string.Empty);
    private readonly Button _abbrechen = UiStyle.CreateButton(UiText.Cancel);

    private EinrichtungsSeite? _seite;

    /// <param name="store">Where the settings are saved once the folder is chosen.</param>
    /// <param name="settings">The settings (shared with the main window).</param>
    /// <param name="aktuellerOrdner">The open invoice folder when started from the main window, otherwise <c>null</c>.</param>
    public EinrichtungsAssistent(SettingsStore store, AppSettings settings, DataFolder? aktuellerOrdner)
    {
        _store = store;
        Settings = settings;
        Folder = aktuellerOrdner;
        BestehenderOrdner = aktuellerOrdner is not null;
        AusHauptfenster = aktuellerOrdner is not null;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = UiText.EinrichtungTitle;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = !AusHauptfenster;
        ShowInTaskbar = !AusHauptfenster;
        StartPosition = AusHauptfenster ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
        ClientSize = new Size(860, 660);
        BuildLayout();
        ResumeLayout(false);
        PerformLayout();

        UiStyle.MakePrimary(_primaer);
        _primaer.Click += (_, _) => _seite?.Primaer();
        _sekundaer.Click += (_, _) => _seite?.Sekundaer();
        _zurueck.Click += (_, _) => Zurueck();
        _abbrechen.Click += (_, _) => Close();
        AcceptButton = _primaer;
        CancelButton = _abbrechen;
        KeyPreview = true;

        Zeigen(EinrichtungsSchritt.Willkommen);
    }

    public AppSettings Settings { get; }

    /// <summary>The chosen and opened invoice folder; <c>null</c> until the folder page was passed.</summary>
    public DataFolder? Folder { get; private set; }

    /// <summary>True when <see cref="Folder"/> already held Abgerechnet data (the next two pages are skipped).</summary>
    public bool BestehenderOrdner { get; private set; }

    /// <summary>Opened via the menu of the main window instead of at the first start.</summary>
    public bool AusHauptfenster { get; }

    public EinrichtungsErgebnis Ergebnis { get; private set; } = EinrichtungsErgebnis.Beendet;

    /// <summary>"Erste Rechnung erstellen" was clicked: the main window opens a new invoice.</summary>
    public bool ErsteRechnung { get; private set; }

    /// <summary>Path of the settings file, so the optional copy of Abgerechnet starts with the same settings.</summary>
    public string SettingsFile => _store.CurrentPath;

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = new Padding(0) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Header: "Schritt 2 von 5" above a large title, on a white band.
        _titelLabel.Font = new Font(Font.FontFamily, 14f, FontStyle.Bold);
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = SystemColors.Window,
            Padding = new Padding(20, 14, 20, 12),
            Margin = new Padding(0),
        };
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.Controls.Add(_schrittLabel, 0, 0);
        header.Controls.Add(_titelLabel, 0, 1);
        root.Controls.Add(header, 0, 0);

        root.Controls.Add(_seitenHost, 0, 1);

        var buttons = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 5, Padding = new Padding(14, 8, 14, 12), Margin = new Padding(0) };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        buttons.Controls.Add(_zurueck, 0, 0);
        buttons.Controls.Add(_sekundaer, 2, 0);
        buttons.Controls.Add(_primaer, 3, 0);
        buttons.Controls.Add(_abbrechen, 4, 0);
        root.Controls.Add(buttons, 0, 2);

        Controls.Add(root);
    }

    // ----- Navigation -----

    public void Weiter()
    {
        if (_seite is not null)
            Zeigen(EinrichtungsAblauf.Weiter(_seite.Schritt, BestehenderOrdner));
    }

    private void Zurueck()
    {
        if (_seite is not null)
            Zeigen(EinrichtungsAblauf.Zurueck(_seite.Schritt, BestehenderOrdner));
    }

    private void Zeigen(EinrichtungsSchritt schritt)
    {
        var seite = Erzeugen(schritt);
        // The window was scaled to the screen's DPI when it was created; pages created later are scaled here
        // (sizes, margins and text widths are written for 96 DPI).
        if (DeviceDpi != 96)
            seite.Scale(new SizeF(DeviceDpi / 96f, DeviceDpi / 96f));

        var alt = _seite;
        _seite = seite;

        _seitenHost.SuspendLayout();
        _seitenHost.Controls.Clear();
        _seitenHost.Controls.Add(seite);
        _seitenHost.ResumeLayout();
        alt?.Dispose();

        AktualisiereRahmen();
        // The first page is told in OnShown; later pages once they are laid out.
        if (!IsHandleCreated)
            return;
        BeginInvoke(() =>
        {
            if (ReferenceEquals(seite, _seite) && !seite.IsDisposed)
                Angezeigt(seite);
        });
    }

    /// <summary>The focus goes to the main button, unless the page puts it into a field.</summary>
    private void Angezeigt(EinrichtungsSeite seite)
    {
        _primaer.Focus();
        seite.Angezeigt();
    }

    private EinrichtungsSeite Erzeugen(EinrichtungsSchritt schritt) => schritt switch
    {
        EinrichtungsSchritt.Willkommen => new WillkommenSeite(this),
        EinrichtungsSchritt.Ordner => new OrdnerSeite(this),
        EinrichtungsSchritt.MeineDaten => new MeineDatenSeite(this),
        EinrichtungsSchritt.Vorlage => new VorlageSeite(this),
        _ => new FertigSeite(this),
    };

    /// <summary>Header and buttons follow the page.</summary>
    public void AktualisiereRahmen()
    {
        if (_seite is not { } seite)
            return;

        _schrittLabel.Text = UiText.EinrichtungSchritt(
            EinrichtungsAblauf.Nummer(seite.Schritt, BestehenderOrdner), EinrichtungsAblauf.Anzahl(BestehenderOrdner));
        _titelLabel.Text = seite.Titel;
        _zurueck.Visible = seite.Schritt != EinrichtungsSchritt.Willkommen;
        _primaer.Text = seite.PrimaerText;
        _sekundaer.Visible = seite.SekundaerText is not null;
        _sekundaer.Text = seite.SekundaerText ?? string.Empty;
        _abbrechen.Visible = seite.Schritt != EinrichtungsSchritt.Fertig;
    }

    // ----- Services for the pages -----

    /// <summary>The folder page opened a folder: remembered right away, so the next start opens it.</summary>
    public void OrdnerGeoeffnet(DataFolder folder, bool bestehend)
    {
        Folder = folder;
        BestehenderOrdner = bestehend;
        HelpForm.Folder = folder;
        Settings.DataFolder = folder.FolderPath;
        SaveSettings();
    }

    public void SaveSettings()
    {
        try
        {
            _store.Save(Settings);
        }
        catch (IOException)
        {
            // Not fatal here: the main window saves the settings again and reports a problem.
        }
    }

    /// <summary>Ends the wizard.</summary>
    public void Beenden(bool ersteRechnung, EinrichtungsErgebnis ergebnis = EinrichtungsErgebnis.Beendet)
    {
        ErsteRechnung = ersteRechnung;
        Ergebnis = ergebnis;
        DialogResult = DialogResult.OK;
        Close();
    }

    /// <summary>F1 opens the help topic that fits the current page.</summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F1)
        {
            HelpForm.Open(this, _seite?.Schritt switch
            {
                EinrichtungsSchritt.Ordner => HelpTopics.Folder,
                EinrichtungsSchritt.MeineDaten => HelpTopics.MeineDaten,
                EinrichtungsSchritt.Vorlage => HelpTopics.Vorlagen,
                EinrichtungsSchritt.Fertig => HelpTopics.FirstSteps,
                _ => HelpTopics.About,
            });
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_seite is not null)
            Angezeigt(_seite);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Releases the preview (WebView2 and its temporary PDF) right away.
        _seite?.Dispose();
        base.OnFormClosed(e);
    }
}
