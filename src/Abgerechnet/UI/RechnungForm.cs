using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.UI;

/// <summary>
/// Creating and editing an invoice (issue #6), modelled on the web tool's InvoiceForm: number, date, period,
/// project and status in one line, the customer with its address, the line items and the live totals.
/// Works on a copy; <b>Speichern</b> writes <c>rechnungen.json</c>. Issued invoices open read-only.
/// </summary>
internal sealed class RechnungForm : Form
{
    private readonly DataFolder _folder;
    private readonly Rechnung _rechnung;
    private string _gespeichertJson;
    private bool _readOnly;
    private bool _loading;

    private readonly TextBox _nummer = new() { Width = 120 };
    private readonly DateTimePicker _datum = new() { Format = DateTimePickerFormat.Short, Width = 120 };
    private readonly TextBox _zeitraum = new() { Width = 170 };
    private readonly TextBox _projekt = new() { Dock = DockStyle.Fill };
    private readonly Label _status = new() { AutoSize = true, Margin = new Padding(3, 7, 3, 3), Font = new Font(Control.DefaultFont, FontStyle.Bold) };
    private readonly ComboBox _kunde = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 320 };
    private readonly Button _kundenVerwalten = UiStyle.CreateButton(UiText.KundenVerwalten);
    private readonly Label _anschrift = new() { AutoSize = true, UseMnemonic = false, ForeColor = UiStyle.MutedText, Margin = new Padding(3, 6, 3, 3) };

    private readonly Panel _positionenHost = new() { Dock = DockStyle.Fill, AutoScroll = true };
    /// <summary>One column: the header in row 0, then one editor per line item.</summary>
    private readonly TableLayoutPanel _positionenListe = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 1,
    };
    private TableLayoutPanel? _positionenKopf;
    private readonly List<PositionEditor> _editoren = [];
    private readonly Button _positionHinzufuegen = UiStyle.CreateButton(UiText.PositionHinzufuegen);

    private readonly Label _netto = SumLabel();
    private readonly Label _steuerText = new() { AutoSize = true, Anchor = AnchorStyles.Right };
    private readonly Label _steuer = SumLabel();
    private readonly Label _brutto = SumLabel(bold: true);
    private readonly Label _nettoText = new() { Text = UiText.SummeNetto, AutoSize = true, Anchor = AnchorStyles.Right };
    private readonly Label _kleinunternehmer = UiStyle.CreateCaption(UiText.KleinunternehmerSumme);

    private readonly Panel _gestelltBar = new() { Dock = DockStyle.Top, Height = 44, BackColor = UiStyle.HintBack, Padding = new Padding(12, 6, 12, 6), Visible = false };
    private readonly Button _speichern = UiStyle.CreateButton(UiText.SaveAndClose);
    private readonly Button _schliessen = UiStyle.CreateButton(UiText.Cancel);
    private readonly Button _pdfErzeugen = UiStyle.CreateButton(UiText.PdfErzeugenButton);

    /// <param name="folder">The open invoice folder.</param>
    /// <param name="rechnung">The invoice to edit; a copy is edited. A new invoice is not yet in the folder.</param>
    public RechnungForm(DataFolder folder, Rechnung rechnung)
    {
        _folder = folder;
        _rechnung = JsonDataFile.CloneObject(rechnung);
        _gespeichertJson = string.Empty;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1080, 720);
        MinimumSize = new Size(900, 560);
        ShowInTaskbar = false;
        MinimizeBox = false;
        KeyPreview = true;

        BuildLayout();
        LoadRechnung();
        ResumeLayout(false);
        PerformLayout();
    }

    protected override void OnLoad(EventArgs e)
    {
        // The form has been scaled to the screen's DPI by now; the line item editors convert their sizes with the
        // same DPI themselves (see PositionEditor), so items shown now and items added later look the same.
        base.OnLoad(e);
        UiStyle.FitToScreen(this);

        _positionenKopf = PositionEditor.CreateHeader(DeviceDpi);
        _loading = true;
        foreach (var position in _rechnung.Positionen)
            CreateEditor(position);
        _loading = false;
        RebuildPositionen();
        UpdateSummen();

        SetReadOnly(_rechnung.Status != RechnungsStatus.Entwurf);
        ActiveControl = _readOnly ? _schliessen : _nummer;

        // What the inputs show now counts as unchanged; closing without typing anything asks nothing.
        ReadInputs();
        _gespeichertJson = JsonDataFile.ToJson(_rechnung);
    }

    /// <summary>Id of the saved invoice, or <c>null</c> when nothing was saved.</summary>
    public Guid? GespeicherteRechnung { get; private set; }

    // ----- Layout -----

    private void BuildLayout()
    {
        // Banner for issued invoices.
        var bearbeiten = UiStyle.CreateButton(UiText.Bearbeiten);
        bearbeiten.Dock = DockStyle.Right;
        bearbeiten.Click += (_, _) => BearbeitenNachRueckfrage();
        _gestelltBar.Controls.Add(new Label { Text = UiText.GestelltHinweis, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft });
        _gestelltBar.Controls.Add(bearbeiten);

        // Number, date, period, project, status in one line (captions above).
        var kopf = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 5, RowCount = 2, Padding = new Padding(12, 10, 12, 0) };
        kopf.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        kopf.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        kopf.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        kopf.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        kopf.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        string[] captions = [UiText.Nummer, UiText.Rechnungsdatum, UiText.Leistungszeitraum, UiText.Projekt, UiText.Status];
        Control[] inputs = [_nummer, _datum, _zeitraum, _projekt, _status];
        for (var i = 0; i < captions.Length; i++)
        {
            kopf.Controls.Add(UiStyle.CreateCaption(captions[i]), i, 0);
            inputs[i].Margin = new Padding(3, 3, 12, 3);
            kopf.Controls.Add(inputs[i], i, 1);
        }

        // Customer with "Kunden verwalten…" right next to it, address below.
        var kunde = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, RowCount = 3, Padding = new Padding(12, 8, 12, 4) };
        kunde.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        kunde.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        kunde.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        kunde.Controls.Add(UiStyle.CreateCaption(UiText.Kunde), 0, 0);
        kunde.Controls.Add(_kunde, 0, 1);
        _kundenVerwalten.Margin = new Padding(6, 0, 3, 0);
        kunde.Controls.Add(_kundenVerwalten, 1, 1);
        kunde.Controls.Add(_anschrift, 0, 2);
        kunde.SetColumnSpan(_anschrift, 3);

        // Line items.
        var positionenTitel = new Label { Text = UiText.Positionen, AutoSize = true, Font = new Font(Control.DefaultFont, FontStyle.Bold), Dock = DockStyle.Top, Padding = new Padding(12, 10, 0, 4) };
        _positionenHost.Padding = new Padding(12, 0, 12, 0);
        var addRow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 4, 0, 8) };
        addRow.Controls.Add(_positionHinzufuegen);
        _positionenHost.Controls.Add(addRow);
        _positionenListe.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _positionenHost.Controls.Add(_positionenListe);

        // Totals and buttons.
        var summen = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, RowCount = 4, Anchor = AnchorStyles.Right | AnchorStyles.Top, Margin = new Padding(3, 3, 3, 3) };
        summen.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        summen.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        summen.Controls.Add(_nettoText, 0, 0);
        summen.Controls.Add(_netto, 1, 0);
        summen.Controls.Add(_steuerText, 0, 1);
        summen.Controls.Add(_steuer, 1, 1);
        summen.Controls.Add(new Label { Text = UiText.SummeBrutto, AutoSize = true, Anchor = AnchorStyles.Right, Font = new Font(Control.DefaultFont, FontStyle.Bold) }, 0, 2);
        summen.Controls.Add(_brutto, 1, 2);
        summen.Controls.Add(_kleinunternehmer, 0, 3);
        summen.SetColumnSpan(_kleinunternehmer, 2);
        _kleinunternehmer.Anchor = AnchorStyles.Right;

        UiStyle.MakePrimary(_speichern);
        _speichern.Click += (_, _) => { if (Speichern()) Close(); };
        _schliessen.Click += (_, _) => Close();
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Anchor = AnchorStyles.Right, Margin = new Padding(3, 8, 3, 3) };
        buttons.Controls.Add(_schliessen);
        buttons.Controls.Add(_speichern);
        buttons.Controls.Add(_pdfErzeugen);
        _pdfErzeugen.Click += (_, _) => PdfErzeugen();

        var fuss = new TableLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, ColumnCount = 1, RowCount = 2, Padding = new Padding(12, 6, 12, 10), BackColor = Color.FromArgb(249, 250, 251) };
        fuss.Controls.Add(summen, 0, 0);
        fuss.Controls.Add(buttons, 0, 1);
        fuss.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        Controls.Add(_positionenHost);
        Controls.Add(positionenTitel);
        Controls.Add(kunde);
        Controls.Add(kopf);
        Controls.Add(_gestelltBar);
        Controls.Add(fuss);
        AcceptButton = null; // Enter belongs to the multi-line descriptions
        CancelButton = _schliessen;

        _nummer.TextChanged += (_, _) => { Text = UiText.RechnungTitle(_nummer.Text); };
        _zeitraum.TextChanged += (_, _) => { if (!_loading) UpdateZeitraumPlatzhalter(); };
        _kunde.DisplayMember = nameof(Kunde.Firma); // also shown while the box is disabled
        _kunde.SelectedIndexChanged += (_, _) => OnKundeChanged();
        _kundenVerwalten.Click += (_, _) => KundenVerwalten();
        _positionHinzufuegen.Click += (_, _) => PositionHinzufuegen();
    }

    private static Label SumLabel(bool bold = false) => new()
    {
        AutoSize = false,
        Dock = DockStyle.Fill,
        Height = 22,
        TextAlign = ContentAlignment.MiddleRight,
        Font = bold ? new Font(Control.DefaultFont.FontFamily, 11f, FontStyle.Bold) : Control.DefaultFont,
    };

    // ----- Loading and reading the inputs -----

    private void LoadRechnung()
    {
        _loading = true;
        _nummer.Text = _rechnung.Nummer;
        _datum.Value = _rechnung.Datum.ToDateTime(TimeOnly.MinValue);
        _zeitraum.Text = _rechnung.Zeitraum;
        _projekt.Text = _rechnung.Projekt;
        _status.Text = UiText.StatusName(_rechnung.Status);
        _status.ForeColor = UiStyle.StatusColor(_rechnung.Status);
        Text = UiText.RechnungTitle(_rechnung.Nummer);
        FillKunden(_rechnung.KundeId);
        _loading = false;
    }

    private void FillKunden(Guid? select)
    {
        _kunde.Items.Clear();
        var kunden = _folder.Kunden.Sortiert().ToList();
        // A deleted customer of an issued invoice is still shown with the address the invoice was made out to.
        if (select is { } id && kunden.All(k => k.Id != id) && _rechnung.Empfaenger is { } kopie)
            kunden.Insert(0, kopie);
        foreach (var kunde in kunden)
            _kunde.Items.Add(kunde);
        _kunde.SelectedItem = kunden.FirstOrDefault(k => k.Id == select);
        UpdateAnschrift();
    }

    /// <summary>Copies the inputs into the edited invoice.</summary>
    private void ReadInputs()
    {
        _rechnung.Nummer = _nummer.Text.Trim();
        _rechnung.Datum = DateOnly.FromDateTime(_datum.Value);
        _rechnung.Zeitraum = _zeitraum.Text.Trim();
        _rechnung.Projekt = _projekt.Text.Trim();
        _rechnung.KundeId = (_kunde.SelectedItem as Kunde)?.Id;
        _rechnung.Positionen = _editoren.Select(e => e.Position).ToList();
    }

    private bool HatAenderungen()
    {
        if (_readOnly)
            return false;
        ReadInputs();
        return JsonDataFile.ToJson(_rechnung) != _gespeichertJson;
    }

    // ----- Customer -----

    private void OnKundeChanged()
    {
        if (_loading)
            return;
        // The frozen address belonged to the previous customer; a new PDF freezes the new one.
        if (_rechnung.Empfaenger is not null && (_kunde.SelectedItem as Kunde)?.Id != _rechnung.KundeId)
            _rechnung.Empfaenger = null;
        UpdateAnschrift();
    }

    private void UpdateAnschrift()
    {
        var kunde = _kunde.SelectedItem as Kunde;
        // An issued invoice shows the address it was made out to.
        if (kunde is not null && _rechnung.Empfaenger is { } kopie && kopie.Id == kunde.Id)
            kunde = kopie;
        _anschrift.Text = kunde is null ? UiText.KeinKunde : string.Join(Environment.NewLine, kunde.Anschrift());
    }

    private void KundenVerwalten()
    {
        using var dialog = new KundenDialog(_folder, (_kunde.SelectedItem as Kunde)?.Id);
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        // The customer chosen in the dialog (e.g. a new one) becomes the invoice's customer.
        FillKunden(dialog.SelectedKundeId ?? (_kunde.SelectedItem as Kunde)?.Id);
        OnKundeChanged();
    }

    // ----- Line items -----

    private PositionEditor CreateEditor(Position position)
    {
        var editor = new PositionEditor(DeviceDpi);
        editor.ShowPosition(position);
        editor.Changed += (_, _) => UpdateSummen();
        editor.MoveUpRequested += (_, _) => MovePosition(editor, -1);
        editor.MoveDownRequested += (_, _) => MovePosition(editor, +1);
        editor.RemoveRequested += (_, _) => Remove(editor);
        editor.SetZeitraumPlatzhalter(_zeitraum.Text);
        editor.SetReadOnly(_readOnly);
        _editoren.Add(editor);
        return editor;
    }

    /// <summary>Puts the header and the editors into the list in the order of <see cref="_editoren"/>.</summary>
    private void RebuildPositionen()
    {
        _positionenListe.SuspendLayout();
        _positionenListe.Controls.Clear();
        _positionenListe.RowStyles.Clear();
        _positionenListe.RowCount = _editoren.Count + 1;
        for (var i = 0; i <= _editoren.Count; i++)
            _positionenListe.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        if (_positionenKopf is not null)
            _positionenListe.Controls.Add(_positionenKopf, 0, 0);
        for (var i = 0; i < _editoren.Count; i++)
            _positionenListe.Controls.Add(_editoren[i], 0, i + 1);
        _positionenListe.ResumeLayout(true);
        UpdatePositionButtons();
    }

    private void PositionHinzufuegen()
    {
        var editor = CreateEditor(Position.Neu(_folder.Einstellungen.NeuePosition));
        RebuildPositionen();
        UpdateSummen();
        _positionenHost.ScrollControlIntoView(editor);
        editor.FocusBeschreibung();
    }

    private void MovePosition(PositionEditor editor, int offset)
    {
        var index = _editoren.IndexOf(editor);
        var target = index + offset;
        if (target < 0 || target >= _editoren.Count)
            return;
        _editoren.RemoveAt(index);
        _editoren.Insert(target, editor);
        RebuildPositionen();
        UpdateSummen();
    }

    private void Remove(PositionEditor editor)
    {
        if (_editoren.Count <= 1)
        {
            MessageBox.Show(this, UiText.LetztePosition, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        _editoren.Remove(editor);
        RebuildPositionen();
        editor.Dispose();
        UpdateSummen();
    }

    private void UpdatePositionButtons()
    {
        for (var i = 0; i < _editoren.Count; i++)
            _editoren[i].SetButtons(canMoveUp: i > 0, canMoveDown: i < _editoren.Count - 1, canRemove: _editoren.Count > 1);
    }

    private void UpdateZeitraumPlatzhalter()
    {
        foreach (var editor in _editoren)
            editor.SetZeitraumPlatzhalter(_zeitraum.Text);
    }

    // ----- Totals -----

    private void UpdateSummen()
    {
        _rechnung.Positionen = _editoren.Select(e => e.Position).ToList();
        var betrag = Rechnungsbetrag.Berechnen(_rechnung);
        _netto.Text = UiText.Betrag(betrag.Netto);
        _steuerText.Text = UiText.SummeUmsatzsteuer(_rechnung.Umsatzsteuersatz);
        _steuer.Text = UiText.Betrag(betrag.Umsatzsteuer);
        _brutto.Text = UiText.Betrag(betrag.Brutto);

        // Small businesses: only the invoice amount and the note.
        var klein = _rechnung.Kleinunternehmer;
        _nettoText.Visible = _netto.Visible = !klein;
        _steuerText.Visible = _steuer.Visible = !klein;
        _kleinunternehmer.Visible = klein;
    }

    // ----- Read-only for issued invoices -----

    private void SetReadOnly(bool readOnly)
    {
        _readOnly = readOnly;
        _gestelltBar.Visible = readOnly;
        _nummer.ReadOnly = _zeitraum.ReadOnly = _projekt.ReadOnly = readOnly;
        _datum.Enabled = _kunde.Enabled = _kundenVerwalten.Enabled = !readOnly;
        _positionHinzufuegen.Visible = !readOnly;
        _speichern.Visible = !readOnly;
        _schliessen.Text = readOnly ? UiText.Close : UiText.Cancel;
        foreach (var editor in _editoren)
            editor.SetReadOnly(readOnly);
    }

    private void BearbeitenNachRueckfrage()
    {
        if (MessageBox.Show(this, UiText.BearbeitenFrage(_rechnung.Nummer), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        SetReadOnly(false);
        _nummer.Focus();
    }

    // ----- Saving -----

    /// <summary>Checks and saves the invoice. Returns false when something must be corrected or saving failed.</summary>
    private bool Speichern()
    {
        ReadInputs();
        if (_rechnung.Nummer.Length == 0)
            return Fehler(_nummer, UiText.NummerFehlt);
        if (Nummernkreis.IstVergeben(_rechnung.Nummer, _folder.Rechnungen.Rechnungen, ausser: _rechnung.Id))
            return Fehler(_nummer, UiText.NummerVergeben(_rechnung.Nummer));

        var changed = JsonDataFile.Clone(_folder.Rechnungen);
        var index = changed.Rechnungen.FindIndex(r => r.Id == _rechnung.Id);
        var gespeichert = JsonDataFile.CloneObject(_rechnung);
        if (index >= 0)
            changed.Rechnungen[index] = gespeichert;
        else
            changed.Rechnungen.Add(gespeichert);

        try
        {
            _folder.SaveRechnungen(changed);
        }
        catch (DataFileException ex)
        {
            MessageBox.Show(this, UiText.DataFileNotWritable(ex.FilePath, ex.Message), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        _gespeichertJson = JsonDataFile.ToJson(_rechnung);
        GespeicherteRechnung = _rechnung.Id;
        return true;
    }

    /// <summary>
    /// Saves the invoice (unless it is read-only) and opens the preview. When a PDF was created the invoice is no
    /// longer a draft, so the form closes.
    /// </summary>
    private void PdfErzeugen()
    {
        if (!_readOnly && !Speichern())
            return;

        using var vorschau = new VorschauForm(_folder, _rechnung.Id);
        vorschau.ShowDialog(this);
        if (!vorschau.PdfErzeugt)
            return;

        GespeicherteRechnung = _rechnung.Id;
        _readOnly = true; // nothing left to ask about when closing
        Close();
    }

    private bool Fehler(Control control, string message)
    {
        MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        control.Focus();
        return false;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.S && !_readOnly)
        {
            e.SuppressKeyPress = true;
            if (Speichern())
                Close();
            return;
        }
        base.OnKeyDown(e);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!e.Cancel && HatAenderungen())
        {
            switch (MessageBox.Show(this, UiText.UngespeichertFrage, Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
            {
                case DialogResult.Yes:
                    e.Cancel = !Speichern();
                    break;
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }
        base.OnFormClosing(e);
    }
}
