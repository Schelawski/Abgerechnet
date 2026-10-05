using Abgerechnet.Core.Einrichtung;
using Abgerechnet.Core.Storage;
using Abgerechnet.Core.Vorlagen;

namespace Abgerechnet.UI.Einrichtung;

/// <summary>
/// The sender's data and the small-business choice – the same fields as in "Meine Daten", without the invoice
/// settings. "Später eintragen" goes on without saving; the main window then reminds of the missing data.
/// </summary>
internal sealed class MeineDatenSeite : EinrichtungsSeite
{
    private readonly MeineDatenPanel _panel = new(mitRechnungen: false);

    public MeineDatenSeite(EinrichtungsAssistent assistent)
        : base(assistent)
    {
        var stapel = Stapel();
        Hinzufuegen(stapel, Absatz(UiText.MeineDatenSeiteText, new Padding(3, 0, 3, 8)));
        HinzufuegenFuellend(stapel, _panel);
        Controls.Add(stapel);

        var folder = Folder;
        _panel.LoadFrom(folder.Einstellungen, MitgelieferteVorlagen.Verfuegbare(folder));
    }

    public override EinrichtungsSchritt Schritt => EinrichtungsSchritt.MeineDaten;

    public override string Titel => UiText.MeineDatenSeiteTitel;

    public override string? SekundaerText => UiText.MeineDatenSpaeter;

    private DataFolder Folder => Assistent.Folder ?? throw new InvalidOperationException("No invoice folder.");

    public override void Angezeigt() => _panel.FocusFirstField();

    public override void Primaer()
    {
        if (_panel.CheckInput() is { } problem)
        {
            MessageBox.Show(this, problem, UiText.EinrichtungTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_panel.SteuernummerFehlt
            && MessageBox.Show(this, UiText.SteuernummerMissingQuestion, UiText.EinrichtungTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var changed = JsonDataFile.Clone(Folder.Einstellungen);
        _panel.ApplyTo(changed);
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

    public override void Sekundaer() => Assistent.Weiter();
}
