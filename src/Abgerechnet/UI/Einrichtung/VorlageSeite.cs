using Abgerechnet.Core.Einrichtung;
using Abgerechnet.Core.Storage;
using Abgerechnet.Core.Vorlagen;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Abgerechnet.UI.Einrichtung;

/// <summary>
/// Choose one of the built-in templates and optionally a logo. The preview is a real PDF of an invented invoice
/// with the user's data, created the same way as the invoices later.
/// </summary>
internal sealed class VorlageSeite : EinrichtungsSeite
{
    /// <summary>DIN A4 in CSS pixels (595 × 842 pt).</summary>
    private static readonly SizeF A4 = new(794, 1123);

    private readonly List<RadioButton> _vorlagen = [];
    private readonly Label _logoStatus = new() { AutoSize = true, UseMnemonic = false, ForeColor = UiStyle.MutedText, Margin = new Padding(3, 2, 3, 6) };
    private readonly Button _logoEntfernen = UiStyle.CreateButton(UiText.LogoEntfernen);
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private readonly Label _vorschauHinweis = new()
    {
        Dock = DockStyle.Fill,
        Text = UiText.VorschauWirdErstellt,
        TextAlign = ContentAlignment.MiddleCenter,
        ForeColor = UiStyle.MutedText,
        UseMnemonic = false,
    };
    private readonly PdfDrucker _drucker;
    private bool _bereit;
    private bool _laeuft;
    private bool _nochmal;
    private string? _vorschauPdf;

    public VorlageSeite(EinrichtungsAssistent assistent)
        : base(assistent)
    {
        _drucker = new PdfDrucker(_webView);

        var stapel = Stapel();
        Hinzufuegen(stapel, Absatz(UiText.VorlageSeiteText, new Padding(3, 0, 3, 10)));

        var spalten = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        spalten.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        spalten.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        spalten.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        spalten.Controls.Add(BuildAuswahl(), 0, 0);

        var rahmen = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, BackColor = SystemColors.ControlDark, Margin = new Padding(3, 0, 3, 3) };
        // The hint lies above the WebView2 until the first preview is there (a hidden WebView2 would not start).
        rahmen.Controls.Add(_vorschauHinweis);
        rahmen.Controls.Add(_webView);
        _vorschauHinweis.BringToFront();
        spalten.Controls.Add(rahmen, 1, 0);
        HinzufuegenFuellend(stapel, spalten);
        Controls.Add(stapel);

        LogoAnzeigen();
    }

    public override EinrichtungsSchritt Schritt => EinrichtungsSchritt.Vorlage;

    public override string Titel => UiText.VorlageSeiteTitel;

    private DataFolder Folder => Assistent.Folder ?? throw new InvalidOperationException("No invoice folder.");

    private string Gewaehlt => _vorlagen.FirstOrDefault(r => r.Checked)?.Tag as string ?? MitgelieferteVorlagen.Standard;

    private Control BuildAuswahl()
    {
        var auswahl = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0, 0, 12, 0),
        };

        var aktuell = Folder.Einstellungen.Rechnung.Vorlage;
        if (!MitgelieferteVorlagen.IstMitgeliefert(aktuell))
            aktuell = MitgelieferteVorlagen.Standard;
        foreach (var name in MitgelieferteVorlagen.Namen)
        {
            var option = new RadioButton
            {
                Text = UiText.VorlagenName(name),
                Tag = name,
                AutoSize = true,
                Checked = string.Equals(name, aktuell, StringComparison.OrdinalIgnoreCase),
                Margin = new Padding(3, 3, 3, 6),
            };
            option.CheckedChanged += (_, _) =>
            {
                if (option.Checked)
                    _ = VorschauAsync();
            };
            _vorlagen.Add(option);
            auswahl.Controls.Add(option);
        }

        auswahl.Controls.Add(new Label { Text = UiText.LogoLabel, AutoSize = true, Margin = new Padding(3, 24, 3, 2) });
        auswahl.Controls.Add(_logoStatus);
        var waehlen = UiStyle.CreateButton(UiText.LogoWaehlen);
        waehlen.Click += (_, _) => LogoWaehlen();
        _logoEntfernen.Click += (_, _) => LogoEntfernen();
        auswahl.Controls.Add(waehlen);
        auswahl.Controls.Add(_logoEntfernen);
        return auswahl;
    }

    public override async void Angezeigt()
    {
        _vorlagen.First(r => r.Checked).Focus();
        try
        {
            await _drucker.StartenAsync(Folder.VorlagenPath);
            _bereit = true;
        }
        catch (Exception ex) when (ex is WebView2RuntimeNotFoundException or IOException or UnauthorizedAccessException or InvalidOperationException or System.Runtime.InteropServices.COMException)
        {
            if (!IsDisposed)
                _vorschauHinweis.Text = UiText.VorschauNichtMoeglich;
            return;
        }

        await VorschauAsync();
    }

    /// <summary>
    /// Creates the preview of the chosen template. One at a time: a choice made meanwhile is shown when the running
    /// one is done.
    /// </summary>
    private async Task VorschauAsync()
    {
        if (!_bereit)
            return;
        if (_laeuft)
        {
            _nochmal = true;
            return;
        }

        _laeuft = true;
        try
        {
            do
            {
                _nochmal = false;
                await VorschauErstellenAsync(Gewaehlt);
            }
            while (_nochmal && !IsDisposed);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ObjectDisposedException or System.Runtime.InteropServices.COMException)
        {
            // The page was left meanwhile, or the preview failed – choosing a template still works.
            if (!IsDisposed)
            {
                _vorschauHinweis.Text = ex.Message;
                _vorschauHinweis.BringToFront();
            }
        }
        finally
        {
            _laeuft = false;
        }
    }

    private async Task VorschauErstellenAsync(string name)
    {
        var vorlage = MitgelieferteVorlagen.Laden(Folder, name);
        var daten = Beispielrechnung.Erzeugen(Folder.Einstellungen, DateOnly.FromDateTime(DateTime.Today));
        var html = Vorlage.Ausfuellen(vorlage.Html, daten, PdfDrucker.Basis).Html;

        var pdf = PdfDrucker.NeueVorschauDatei();
        await _drucker.DruckenAsync(html, pdf);
        await _drucker.PdfZeigenAsync(pdf, Ansicht());
        if (IsDisposed)
            return;

        _webView.BringToFront();
        if (_vorschauPdf is not null)
            PdfDrucker.Loeschen(_vorschauPdf);
        _vorschauPdf = pdf;
    }

    /// <summary>
    /// The PDF viewer without toolbar, zoomed so the whole page is visible ("view=Fit" is ignored). Measured: at 125 %
    /// screen scaling the viewer shows a page at zoom 100 % 1.5 times as large as its CSS size (scaling × 1.2). Should
    /// the factor be smaller elsewhere, the page only appears smaller, never cut off.
    /// </summary>
    private string Ansicht()
    {
        var scale = DeviceDpi / 96f;
        var faktor = scale * 1.2f;
        var rand = 32 * scale; // the viewer's margins around the page
        var zoom = Math.Min(
            (_webView.ClientSize.Width - rand) / (A4.Width * faktor),
            (_webView.ClientSize.Height - rand) / (A4.Height * faktor));
        return $"#toolbar=0&navpanes=0&zoom={Math.Clamp((int)(zoom * 100), 10, 100)}";
    }

    // ----- Logo -----

    private void LogoAnzeigen()
    {
        var vorhanden = Logo.Vorhanden(Folder);
        _logoStatus.Text = vorhanden ? UiText.LogoVorhanden : UiText.LogoKeins;
        _logoEntfernen.Visible = vorhanden;
    }

    private void LogoWaehlen()
    {
        using var dialog = new OpenFileDialog
        {
            Title = UiText.LogoDialogTitel,
            Filter = UiText.LogoDialogFilter,
            CheckFileExists = true,
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            Logo.Uebernehmen(Folder, dialog.FileName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, UiText.LogoFehler(ex.Message), UiText.EinrichtungTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LogoGeaendert();
    }

    private void LogoEntfernen()
    {
        try
        {
            Logo.Entfernen(Folder);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, UiText.LogoFehler(ex.Message), UiText.EinrichtungTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LogoGeaendert();
    }

    private async void LogoGeaendert()
    {
        LogoAnzeigen();
        if (!_bereit)
            return;
        // The old logo must not come from WebView2's cache.
        await _webView.CoreWebView2.Profile.ClearBrowsingDataAsync(CoreWebView2BrowsingDataKinds.DiskCache);
        await VorschauAsync();
    }

    // ----- Saving -----

    public override void Primaer()
    {
        var changed = JsonDataFile.Clone(Folder.Einstellungen);
        changed.Rechnung.Vorlage = Gewaehlt;
        try
        {
            Folder.SaveEinstellungen(changed);
        }
        catch (DataFileException ex)
        {
            MessageBox.Show(this, UiText.DataFileNotWritable(ex.FilePath, ex.Message), UiText.EinrichtungTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Assistent.Weiter();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // The viewer holds the temporary PDF open until the WebView2 is gone.
            _webView.Dispose();
            if (_vorschauPdf is not null)
                PdfDrucker.Loeschen(_vorschauPdf);
        }
        base.Dispose(disposing);
    }
}
