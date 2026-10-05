using System.Diagnostics;
using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;
using Abgerechnet.Core.Vorlagen;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Abgerechnet.UI;

/// <summary>
/// Preview and creation of an invoice PDF (issue #8). WebView2 renders the filled HTML template and prints it with
/// <see cref="CoreWebView2.PrintToPdfAsync(string, CoreWebView2PrintSettings)"/> to a temporary A4 PDF, which the
/// window then shows in WebView2's PDF viewer – the preview is exactly the file that will be saved. "PDF speichern"
/// copies it into the folder <c>PDF</c>; the invoice then remembers the file, freezes the recipient and a draft becomes
/// open.
/// </summary>
internal sealed class VorschauForm : Form
{
    private readonly DataFolder _folder;
    private readonly Guid _rechnungId;
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private readonly Label _hinweise = new() { Dock = DockStyle.Top, AutoSize = true, BackColor = UiStyle.HintBack, Padding = new Padding(12, 8, 12, 8), Visible = false, UseMnemonic = false };
    private readonly Label _vorlageInfo = UiStyle.CreateCaption(string.Empty);
    private readonly Label _gespeichert = new() { AutoSize = true, ForeColor = UiStyle.Success, Margin = new Padding(3, 8, 12, 3), Visible = false, UseMnemonic = false };
    private readonly Button _speichern = UiStyle.CreateButton(UiText.PdfSpeichern);
    private readonly Button _pdfOeffnen = UiStyle.CreateButton(UiText.PdfOeffnenButton);
    private readonly Button _imOrdner = UiStyle.CreateButton(UiText.ImOrdnerZeigenButton);
    private readonly Button _schliessen = UiStyle.CreateButton(UiText.Close);
    private readonly PdfDrucker _drucker;
    private readonly string _vorschauPdf = PdfDrucker.NeueVorschauDatei();
    private string? _pdfPfad;

    public VorschauForm(DataFolder folder, Guid rechnungId)
    {
        _folder = folder;
        _rechnungId = rechnungId;
        _drucker = new PdfDrucker(_webView);

        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 1000);
        MinimumSize = new Size(640, 480);
        ShowInTaskbar = false;
        MinimizeBox = false;
        Text = UiText.VorschauTitle(Rechnung.Nummer);

        UiStyle.MakePrimary(_speichern);
        _speichern.Enabled = false; // until the preview is shown
        _speichern.Click += (_, _) => PdfSpeichern();
        _pdfOeffnen.Visible = _imOrdner.Visible = false;
        _pdfOeffnen.Click += (_, _) => Oeffnen(_pdfPfad);
        _imOrdner.Click += (_, _) => ImOrdnerZeigen();
        _schliessen.Click += (_, _) => Close();

        var links = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Left, WrapContents = false, Padding = new Padding(8, 10, 0, 0) };
        links.Controls.Add(_vorlageInfo);
        var rechts = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 4, 8, 0) };
        rechts.Controls.AddRange([_schliessen, _speichern, _imOrdner, _pdfOeffnen, _gespeichert]);
        var fuss = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(4) };
        fuss.Controls.Add(rechts);
        fuss.Controls.Add(links);

        Controls.Add(_webView);
        Controls.Add(_hinweise);
        Controls.Add(fuss);
        CancelButton = _schliessen;
        UiStyle.EnableHelpKey(this, Core.Help.HelpTopics.Pdf);
        ResumeLayout(false);
        PerformLayout();
    }

    /// <summary>True when a PDF was saved; the invoice has changed (status, file name).</summary>
    public bool PdfErzeugt { get; private set; }

    private Rechnung Rechnung => _folder.Rechnungen.Rechnungen.First(r => r.Id == _rechnungId);

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UiStyle.FitToScreen(this);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        UseWaitCursor = true;
        try
        {
            await ZeigenAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException)
        {
            MessageBox.Show(this, UiText.VorlageFehler(ex.Message), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task ZeigenAsync()
    {
        if (!await WebViewStartenAsync())
            return;

        var rechnung = Rechnung;
        var empfaenger = rechnung.EmpfaengerAus(_folder.Kunden);
        var gewuenscht = MitgelieferteVorlagen.NameFuer(rechnung, _folder.Einstellungen);
        var vorlage = MitgelieferteVorlagen.Laden(_folder, gewuenscht);
        var ergebnis = Vorlage.Ausfuellen(vorlage.Html, new RechnungsDaten(_folder.Einstellungen, rechnung, empfaenger), PdfDrucker.Basis);
        _vorlageInfo.Text = UiText.VorlageInfo(vorlage.Name, mitgeliefert: vorlage.Datei is null);

        // Not blocking: the user decides whether the invoice may go out like this.
        var hinweise = Pflichtangaben.Fehlende(_folder.Einstellungen, rechnung, empfaenger).Select(UiText.Pflichtangabe).ToList();
        if (vorlage.Ersatz)
            hinweise.Insert(0, UiText.VorlageFehlt(gewuenscht, vorlage.Name));
        if (ergebnis.UnbekanntePlatzhalter.Count > 0)
            hinweise.Add(UiText.UnbekanntePlatzhalter(ergebnis.UnbekanntePlatzhalter));
        if (hinweise.Count > 0)
        {
            _hinweise.Text = UiText.HinweiseTitel + Environment.NewLine + string.Join(Environment.NewLine, hinweise.Select(h => "• " + h));
            _hinweise.Visible = true;
        }

        // 1. Render the HTML and print it to the temporary PDF, 2. show that PDF.
        await _drucker.DruckenAsync(ergebnis.Html, _vorschauPdf);
        await _drucker.PdfZeigenAsync(_vorschauPdf);

        _speichern.Enabled = true;
        _speichern.Focus();
    }

    /// <summary>Creates the WebView2 with its own data folder; explains what to do when the runtime is missing.</summary>
    private async Task<bool> WebViewStartenAsync()
    {
        try
        {
            await _drucker.StartenAsync(_folder.VorlagenPath);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            if (MessageBox.Show(this, UiText.WebView2Fehlt, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                Oeffnen(UiText.WebView2DownloadUrl);
            Close();
            return false;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.Runtime.InteropServices.COMException)
        {
            MessageBox.Show(this, UiText.WebView2Fehler(ex.Message), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return false;
        }

        return true;
    }

    private void PdfSpeichern()
    {
        var rechnung = Rechnung;
        var empfaenger = rechnung.EmpfaengerAus(_folder.Kunden);
        var datei = PdfDateiname.Erzeugen(_folder.Einstellungen.Rechnung.PdfDateiname, rechnung.Nummer, rechnung.Datum, empfaenger);
        var ziel = Path.Combine(_folder.PdfPath, datei);
        if (File.Exists(ziel)
            && MessageBox.Show(this, UiText.PdfExistiert(datei), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            Directory.CreateDirectory(_folder.PdfPath);
            // Copy next to the target first: a PDF that is open in a viewer is never left half-written.
            var temp = ziel + ".tmp";
            File.Copy(_vorschauPdf, temp, overwrite: true);
            File.Move(temp, ziel, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, UiText.PdfFehler(ex.Message), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Remember the PDF in the invoice: file name, frozen address, draft → open.
        var changed = JsonDataFile.Clone(_folder.Rechnungen);
        changed.Rechnungen.First(r => r.Id == _rechnungId).PdfErzeugt(datei, empfaenger);
        try
        {
            _folder.SaveRechnungen(changed);
        }
        catch (DataFileException ex)
        {
            MessageBox.Show(this, UiText.DataFileNotWritable(ex.FilePath, ex.Message), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        PdfErzeugt = true;
        _pdfPfad = ziel;
        _gespeichert.Text = UiText.PdfGespeichert(datei);
        _gespeichert.Visible = _pdfOeffnen.Visible = _imOrdner.Visible = true;
        _pdfOeffnen.Focus();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        // The viewer holds the temporary PDF open until the WebView2 is gone.
        _webView.Dispose();
        PdfDrucker.Loeschen(_vorschauPdf);
    }

    private void ImOrdnerZeigen()
    {
        if (_pdfPfad is not null && File.Exists(_pdfPfad))
            Process.Start("explorer.exe", $"/select,\"{_pdfPfad}\"")?.Dispose();
    }

    private static void Oeffnen(string? ziel) => PdfDrucker.Oeffnen(ziel);
}
