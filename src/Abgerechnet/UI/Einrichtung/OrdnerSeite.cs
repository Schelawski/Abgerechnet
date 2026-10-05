using Abgerechnet.Core.Einrichtung;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.UI.Einrichtung;

/// <summary>
/// Choose the invoice folder: a new one (suggested: "Dokumente\Rechnungen") or one that already holds Abgerechnet data.
/// The text below the path says which it is. "Weiter" creates and opens the folder.
/// </summary>
internal sealed class OrdnerSeite : EinrichtungsSeite
{
    private readonly TextBox _pfad = new() { Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(3, 4, 6, 3) };
    private readonly Label _art = new() { AutoSize = true, UseMnemonic = false, MaximumSize = new Size(TextWidth, 0), Margin = new Padding(3, 6, 3, 16) };

    public OrdnerSeite(EinrichtungsAssistent assistent)
        : base(assistent)
    {
        var stapel = Stapel();
        Hinzufuegen(stapel, Absatz(UiText.OrdnerText));

        var zeile = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, Margin = new Padding(0) };
        zeile.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        zeile.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        zeile.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        zeile.Controls.Add(new Label { Text = UiText.OrdnerLabel, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 4, 8, 3) }, 0, 0);
        zeile.Controls.Add(_pfad, 1, 0);
        var aendern = UiStyle.CreateButton(UiText.OrdnerAendern);
        aendern.Click += (_, _) => Waehlen();
        zeile.Controls.Add(aendern, 2, 0);
        Hinzufuegen(stapel, zeile);
        Hinzufuegen(stapel, _art);

        var tipp = Absatz(UiText.OrdnerSichern);
        tipp.ForeColor = UiStyle.MutedText;
        Hinzufuegen(stapel, tipp);
        Controls.Add(stapel);

        _pfad.Text = assistent.Folder?.FolderPath
            ?? EinrichtungsAblauf.OrdnerVorschlag(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        _pfad.TextChanged += (_, _) => ArtAnzeigen();
        ArtAnzeigen();
    }

    public override EinrichtungsSchritt Schritt => EinrichtungsSchritt.Ordner;

    public override string Titel => UiText.OrdnerTitel;

    public override void Angezeigt()
    {
        _pfad.Focus();
        _pfad.SelectionStart = _pfad.TextLength;
    }

    private string Pfad => _pfad.Text.Trim().Trim('"');

    /// <summary>True when the path is the folder the wizard already opened (e.g. after "Zurück").</summary>
    private bool IstGeoeffneterOrdner(string pfad)
    {
        if (Assistent.Folder is not { } folder || pfad.Length == 0)
            return false;
        try
        {
            return string.Equals(Path.GetFullPath(pfad), folder.FolderPath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private void ArtAnzeigen()
    {
        var pfad = Pfad;
        if (pfad.Length == 0)
        {
            _art.Text = string.Empty;
            return;
        }

        // The folder opened earlier now holds data files – what counts is what it was before.
        var art = IstGeoeffneterOrdner(pfad)
            ? (Assistent.BestehenderOrdner ? OrdnerArt.Rechnungsordner : OrdnerArt.Leer)
            : EinrichtungsAblauf.ArtDesOrdners(pfad);
        _art.Text = art switch
        {
            OrdnerArt.Neu => UiText.OrdnerNeu,
            OrdnerArt.Leer => UiText.OrdnerLeer,
            OrdnerArt.AndereDateien => UiText.OrdnerAndereDateien,
            _ => UiText.OrdnerVorhanden,
        };
        _art.ForeColor = art == OrdnerArt.AndereDateien ? UiStyle.StatusOffen : UiStyle.Success;
    }

    private void Waehlen()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = UiText.ChooseFolderDescription,
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
        };
        // Start at the suggested folder, or at its parent while it does not exist yet.
        var pfad = Pfad;
        if (Directory.Exists(pfad))
            dialog.InitialDirectory = pfad;
        else if (Path.GetDirectoryName(pfad) is { } parent && Directory.Exists(parent))
            dialog.InitialDirectory = parent;

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _pfad.Text = dialog.SelectedPath;
    }

    public override void Primaer()
    {
        var pfad = Pfad;
        if (pfad.Length == 0)
        {
            MessageBox.Show(this, UiText.OrdnerLeerFehler, UiText.EinrichtungTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            _pfad.Focus();
            return;
        }

        if (!IstGeoeffneterOrdner(pfad))
        {
            var bestehend = EinrichtungsAblauf.ArtDesOrdners(pfad) == OrdnerArt.Rechnungsordner;
            DataFolder folder;
            try
            {
                Directory.CreateDirectory(pfad);
                folder = DataFolder.Open(pfad);
            }
            catch (DataFileException ex)
            {
                MessageBox.Show(this, DataFolderDialogs.Describe(ex, pfad), UiText.EinrichtungTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                MessageBox.Show(this, UiText.OrdnerFehler(ex.Message), UiText.EinrichtungTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Assistent.OrdnerGeoeffnet(folder, bestehend);
        }

        Assistent.Weiter();
    }
}
