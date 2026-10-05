using System.Diagnostics;
using Abgerechnet.Core.Einrichtung;

namespace Abgerechnet.UI.Einrichtung;

/// <summary>
/// "Abgerechnet ist eingerichtet": "Erste Rechnung erstellen", and optionally copy Abgerechnet out of the downloads
/// folder with shortcuts on the desktop and in the start menu.
/// </summary>
internal sealed class FertigSeite : EinrichtungsSeite
{
    private readonly CheckBox? _kopieren;
    private readonly Label _meldung = new() { AutoSize = true, UseMnemonic = false, MaximumSize = new Size(TextWidth, 0), ForeColor = UiStyle.Danger, Margin = new Padding(3, 10, 3, 3), Visible = false };
    private readonly bool _ersteRechnungAnbieten;

    public FertigSeite(EinrichtungsAssistent assistent)
        : base(assistent)
    {
        var folder = assistent.Folder ?? throw new InvalidOperationException("No invoice folder.");
        _ersteRechnungAnbieten = folder.Rechnungen.Rechnungen.Count == 0;

        var stapel = Stapel();
        var ueberschrift = Absatz(UiText.FertigUeberschrift);
        ueberschrift.Font = new Font(ueberschrift.Font.FontFamily, 11f, FontStyle.Bold);
        ueberschrift.ForeColor = UiStyle.Success;
        Hinzufuegen(stapel, ueberschrift);
        Hinzufuegen(stapel, Absatz(_ersteRechnungAnbieten ? UiText.FertigText : UiText.FertigVorhanden));
        if (!folder.Einstellungen.Absender.IstVollstaendig)
        {
            var fehlt = Absatz(UiText.FertigMeineDatenFehlen);
            fehlt.ForeColor = UiStyle.StatusOffen;
            Hinzufuegen(stapel, fehlt);
        }

        var processPath = Environment.ProcessPath;
        if (LocalInstall.CanOffer(processPath, LocalInstall.DefaultRoot))
        {
            _kopieren = new CheckBox
            {
                Text = UiText.KopierenOption,
                AutoSize = true,
                MaximumSize = new Size(TextWidth, 0),
                Checked = LocalInstall.IsTemporaryLocation(processPath!, LocalInstall.DefaultTemporaryFolders()),
                Margin = new Padding(3, 8, 3, 0),
            };
            Hinzufuegen(stapel, _kopieren);
            var hinweis = Absatz(UiText.KopierenHinweis(LocalInstall.TargetPath(LocalInstall.DefaultRoot)), new Padding(22, 2, 3, 3));
            hinweis.ForeColor = UiStyle.MutedText;
            Hinzufuegen(stapel, hinweis);
        }

        Hinzufuegen(stapel, _meldung);
        Controls.Add(stapel);
    }

    public override EinrichtungsSchritt Schritt => EinrichtungsSchritt.Fertig;

    public override string Titel => UiText.FertigTitel;

    public override string PrimaerText => _ersteRechnungAnbieten ? UiText.ErsteRechnung : UiText.ZumHauptfenster;

    /// <summary>"Fertig" without a first invoice, next to "Erste Rechnung erstellen".</summary>
    public override string? SekundaerText => _ersteRechnungAnbieten ? UiText.ZumHauptfenster : null;

    public override void Primaer() => Beenden(_ersteRechnungAnbieten);

    public override void Sekundaer() => Beenden(ersteRechnung: false);

    private void Beenden(bool ersteRechnung)
    {
        if (_kopieren is not { Checked: true } || Environment.ProcessPath is not { } processPath)
        {
            Assistent.Beenden(ersteRechnung);
            return;
        }

        try
        {
            // Saved first, so the copy opens the invoice folder chosen here.
            Assistent.SaveSettings();
            var root = LocalInstall.DefaultRoot;
            var kopie = LocalInstall.Copy(processPath, root, Assistent.SettingsFile);
            LocalInstall.CreateShortcuts(kopie);
            var start = new ProcessStartInfo(kopie) { UseShellExecute = true, WorkingDirectory = root };
            if (ersteRechnung)
                start.Arguments = LocalInstall.NeueRechnungArgument;
            Process.Start(start)?.Dispose();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception)
        {
            _meldung.Text = UiText.KopierenFehler(ex.Message);
            _meldung.Visible = true;
            _kopieren.Checked = false; // a second click goes on from here
            return;
        }

        Assistent.Beenden(ersteRechnung, EinrichtungsErgebnis.KopieGestartet);
    }
}
