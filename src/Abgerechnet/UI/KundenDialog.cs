using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.UI;

/// <summary>
/// Dialog "Kunden": list on the left, the selected customer's fields on the right (as in the web tool).
/// Works on a copy of <c>kunden.json</c>; <b>Speichern</b> writes it, <b>Abbrechen</b> discards all changes.
/// </summary>
internal sealed class KundenDialog : Form
{
    private readonly DataFolder _folder;
    private readonly KundenDatei _kunden;

    private readonly ListBox _list = new() { Dock = DockStyle.Fill, IntegralHeight = false, FormattingEnabled = true };
    private readonly Button _neuButton = UiStyle.CreateButton(UiText.KundeNeu);
    private readonly Button _loeschenButton = UiStyle.CreateButton(UiText.KundeLoeschen);

    private readonly TextBox _firma = new();
    private readonly TextBox _kurzname = new() { Width = 200 };
    private readonly TextBox _ansprechpartner = new();
    private readonly TextBox _strasse = new();
    private readonly TextBox _plz = new() { Width = 70 };
    private readonly TextBox _ort = new();
    private readonly TextBox _land = new();
    private readonly TextBox _ustIdNr = new() { Width = 200 };
    private readonly Label _vorschau = new() { AutoSize = true, UseMnemonic = false, Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(3, 2, 3, 3) };
    private readonly Panel _editor = new() { Dock = DockStyle.Fill };
    private readonly Label _leer = UiStyle.CreateCaption(UiText.KundenLeer);

    private bool _loading;
    private bool _changed;
    private string _lastSuggestion = string.Empty;

    /// <param name="folder">The open invoice folder.</param>
    /// <param name="selectKundeId">Customer to select at the start, e.g. the one chosen in the invoice.</param>
    public KundenDialog(DataFolder folder, Guid? selectKundeId = null)
    {
        _folder = folder;
        _kunden = JsonDataFile.Clone(folder.Kunden);
        _kunden.Kunden = _kunden.Sortiert().ToList();

        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = UiText.KundenTitle;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(820, 520);
        MinimumSize = new Size(680, 420);
        ShowInTaskbar = false;
        MinimizeBox = false;

        BuildLayout(out var saveButton, out var cancelButton);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        ResumeLayout(false);
        PerformLayout();

        _list.Format += (_, e) => e.Value = e.ListItem is Kunde k && k.Firma.Trim().Length > 0 ? k.Firma.Trim() : UiText.KundeOhneNamen;
        _list.SelectedIndexChanged += (_, _) => ShowSelected();
        _neuButton.Click += (_, _) => AddKunde();
        _loeschenButton.Click += (_, _) => DeleteSelected();
        foreach (var box in new[] { _firma, _kurzname, _ansprechpartner, _strasse, _plz, _ort, _land, _ustIdNr })
            box.TextChanged += (_, _) => OnFieldChanged(box);

        foreach (var kunde in _kunden.Kunden)
            _list.Items.Add(kunde);
        var start = _kunden.Finden(selectKundeId) ?? _kunden.Kunden.FirstOrDefault();
        if (start is not null)
            _list.SelectedItem = start;
        ShowSelected();
    }

    /// <summary>The customer selected when the dialog was saved, e.g. a newly created one.</summary>
    public Guid? SelectedKundeId { get; private set; }

    private Kunde? Selected => _list.SelectedItem as Kunde;

    private void BuildLayout(out Button saveButton, out Button cancelButton)
    {
        // Left: list with New / Delete below.
        var listButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(0, 6, 0, 0) };
        listButtons.Controls.Add(_neuButton);
        listButtons.Controls.Add(_loeschenButton);
        var left = new Panel { Dock = DockStyle.Left, Width = 250, Padding = new Padding(10, 10, 6, 0) };
        left.Controls.Add(_list);
        left.Controls.Add(listButtons);

        // Right: the fields of the selected customer.
        var form = new FormLayout();
        form.Add(UiText.Firma, _firma);
        form.Add(UiText.Kurzname, _kurzname);
        form.AddHint(UiText.KurznameHint);
        form.Add(UiText.Ansprechpartner, _ansprechpartner);
        form.Add(UiText.Strasse, _strasse);
        form.Add(UiText.PlzOrt, FormLayout.Row(_plz, _ort));
        form.Add(UiText.Land, _land);
        form.Add(UiText.KundeUstIdNr, _ustIdNr);
        form.AddHint(UiText.KundeUstIdNrHint);
        form.AddHeading(UiText.AnschriftVorschau);
        form.AddWide(_vorschau).Margin = new Padding(16, 0, 3, 3);
        _editor.Controls.Add(form.Host);

        _leer.Dock = DockStyle.Fill;
        _leer.TextAlign = ContentAlignment.MiddleCenter;
        var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 6, 0) };
        right.Controls.Add(_editor);
        right.Controls.Add(_leer);

        saveButton = UiStyle.CreateButton(UiText.Save);
        UiStyle.MakePrimary(saveButton);
        saveButton.Click += (_, _) => Save();
        cancelButton = UiStyle.CreateButton(UiText.Cancel);
        cancelButton.DialogResult = DialogResult.Cancel;
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(8),
        };
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(saveButton);

        Controls.Add(right);
        Controls.Add(left);
        Controls.Add(buttons);
    }

    private void ShowSelected()
    {
        var kunde = Selected;
        _editor.Visible = kunde is not null;
        _leer.Visible = kunde is null;
        _loeschenButton.Enabled = kunde is not null;
        if (kunde is null)
            return;

        _loading = true;
        _firma.Text = kunde.Firma;
        _kurzname.Text = kunde.Kurzname;
        _ansprechpartner.Text = kunde.Ansprechpartner;
        _strasse.Text = kunde.Strasse;
        _plz.Text = kunde.Plz;
        _ort.Text = kunde.Ort;
        _land.Text = kunde.Land;
        _ustIdNr.Text = kunde.UstIdNr;
        _loading = false;

        _lastSuggestion = Kunde.KurznameVorschlag(kunde.Firma);
        UpdateVorschau(kunde);
    }

    private void OnFieldChanged(TextBox box)
    {
        if (_loading || Selected is not { } kunde)
            return;

        // While the short name is empty or still the suggestion, it follows the company name.
        if (box == _firma && (_kurzname.Text.Length == 0 || _kurzname.Text == _lastSuggestion))
        {
            _lastSuggestion = Kunde.KurznameVorschlag(_firma.Text);
            _kurzname.Text = _lastSuggestion; // raises OnFieldChanged for the short name
        }

        kunde.Firma = _firma.Text;
        kunde.Kurzname = _kurzname.Text;
        kunde.Ansprechpartner = _ansprechpartner.Text;
        kunde.Strasse = _strasse.Text;
        kunde.Plz = _plz.Text;
        kunde.Ort = _ort.Text;
        kunde.Land = _land.Text;
        kunde.UstIdNr = _ustIdNr.Text;
        _changed = true;

        if (box == _firma)
            RefreshListText();
        UpdateVorschau(kunde);
    }

    private void RefreshListText()
    {
        // Reassigning the item makes the list box ask for its text again.
        _loading = true;
        var index = _list.SelectedIndex;
        _list.Items[index] = _list.Items[index];
        _list.SelectedIndex = index;
        _loading = false;
    }

    private void UpdateVorschau(Kunde kunde)
    {
        var lines = kunde.Anschrift();
        _vorschau.Text = lines.Count > 0 ? string.Join(Environment.NewLine, lines) : "–";
    }

    private void AddKunde()
    {
        var kunde = new Kunde();
        _kunden.Kunden.Add(kunde);
        _list.Items.Add(kunde);
        _list.SelectedItem = kunde;
        _changed = true;
        _firma.Focus();
    }

    private void DeleteSelected()
    {
        if (Selected is not { } kunde)
            return;

        var name = kunde.Firma.Trim().Length > 0 ? kunde.Firma.Trim() : UiText.KundeOhneNamen;
        if (_folder.Rechnungen.VerwendetKunde(kunde.Id))
        {
            MessageBox.Show(this, UiText.KundeWirdVerwendet(name), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (MessageBox.Show(this, UiText.KundeLoeschenFrage(name), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var index = _list.SelectedIndex;
        _kunden.Kunden.Remove(kunde);
        _list.Items.RemoveAt(index);
        if (_list.Items.Count > 0)
            _list.SelectedIndex = Math.Min(index, _list.Items.Count - 1);
        ShowSelected();
        _changed = true;
    }

    private void Save()
    {
        if (_kunden.Kunden.FirstOrDefault(k => k.Firma.Trim().Length == 0) is { } unnamed)
        {
            _list.SelectedItem = unnamed;
            _firma.Focus();
            MessageBox.Show(this, UiText.KundeFirmaRequired, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (var kunde in _kunden.Kunden)
            Trim(kunde);

        try
        {
            _folder.SaveKunden(_kunden);
        }
        catch (DataFileException ex)
        {
            MessageBox.Show(this, UiText.DataFileNotWritable(ex.FilePath, ex.Message), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SelectedKundeId = Selected?.Id;
        _changed = false;
        DialogResult = DialogResult.OK;
    }

    private static void Trim(Kunde kunde)
    {
        kunde.Firma = kunde.Firma.Trim();
        kunde.Kurzname = kunde.Kurzname.Trim();
        kunde.Ansprechpartner = kunde.Ansprechpartner.Trim();
        kunde.Strasse = kunde.Strasse.Trim();
        kunde.Plz = kunde.Plz.Trim();
        kunde.Ort = kunde.Ort.Trim();
        kunde.Land = kunde.Land.Trim();
        kunde.UstIdNr = kunde.UstIdNr.Trim().ToUpperInvariant();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult != DialogResult.OK && _changed
            && MessageBox.Show(this, UiText.AenderungenVerwerfen, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            e.Cancel = true;
        base.OnFormClosing(e);
    }
}
