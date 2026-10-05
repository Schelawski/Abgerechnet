using Abgerechnet.Core.Model;

namespace Abgerechnet.UI;

/// <summary>
/// "Zahlungseingang erfassen" (issue #17): a checklist of the overdue open invoices, each with its payment date
/// (today by default). One click marks all checked invoices as paid. The dialog only collects the choice; the
/// invoice list saves it.
/// </summary>
internal sealed class ZahlungseingangDialog : Form
{
    private readonly List<(Rechnung Rechnung, CheckBox Haken, DateTimePicker Datum)> _zeilen = [];
    private readonly CheckBox _alle = new() { Text = UiText.AlleMarkieren, AutoSize = true, Margin = new Padding(3, 4, 3, 8) };
    private readonly Button _speichern = UiStyle.CreateButton(UiText.AlsBezahltSpeichern);
    private bool _aendertAlle;

    public ZahlungseingangDialog(IReadOnlyList<Rechnung> rechnungen, KundenDatei kunden, DateOnly heute)
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = UiText.ZahlungseingangTitel;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(780, 440);
        MinimumSize = new Size(620, 300);
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;

        var text = new Label { Text = UiText.ZahlungseingangText, AutoSize = true, UseMnemonic = false, Dock = DockStyle.Top, Padding = new Padding(3, 0, 3, 8) };
        text.MaximumSize = new Size(740, 0);

        var tabelle = new TableLayoutPanel
        {
            ColumnCount = 6,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
        };
        tabelle.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // Nr. with check box
        tabelle.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // Kunde
        tabelle.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // Zeitraum
        tabelle.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // Betrag
        tabelle.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // Offen seit
        tabelle.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // Bezahlt am

        string[] kopf = [UiText.SpalteNummer, UiText.SpalteKunde, UiText.SpalteZeitraum, UiText.SpalteBetrag, UiText.SpalteSeit, UiText.SpalteZahlungsdatum];
        tabelle.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        for (var i = 0; i < kopf.Length; i++)
        {
            var label = new Label { Text = kopf[i], AutoSize = true, ForeColor = UiStyle.MutedText, Margin = new Padding(i == 0 ? 22 : 3, 3, 12, 6) };
            label.Font = new Font(label.Font, FontStyle.Bold);
            if (i == 3)
                label.Anchor = AnchorStyles.Right;
            tabelle.Controls.Add(label, i, 0);
        }
        tabelle.RowCount = 1;

        foreach (var rechnung in rechnungen)
            AddZeile(tabelle, rechnung, kunden, heute);

        var host = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BorderStyle = BorderStyle.FixedSingle, BackColor = SystemColors.Window };
        host.Controls.Add(tabelle);

        UiStyle.MakePrimary(_speichern);
        _speichern.Click += (_, _) => { DialogResult = DialogResult.OK; };
        var abbrechen = UiStyle.CreateButton(UiText.Cancel);
        abbrechen.DialogResult = DialogResult.Cancel;
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
        buttons.Controls.Add(abbrechen);
        buttons.Controls.Add(_speichern);

        var inhalt = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 12, 12, 10) };
        inhalt.Controls.Add(host);
        inhalt.Controls.Add(_alle);
        _alle.Dock = DockStyle.Top;
        inhalt.Controls.Add(text);
        inhalt.Controls.Add(buttons);
        Controls.Add(inhalt);

        AcceptButton = _speichern;
        CancelButton = abbrechen;
        _alle.CheckedChanged += (_, _) => AlleSetzen(_alle.Checked);
        UiStyle.EnableHelpKey(this, Core.Help.HelpTopics.Rechnungen);
        ResumeLayout(false);
        PerformLayout();
        Aktualisieren();
    }

    /// <summary>The checked invoices with their payment dates.</summary>
    public IReadOnlyDictionary<Guid, DateOnly> Bezahlt =>
        _zeilen.Where(z => z.Haken.Checked).ToDictionary(z => z.Rechnung.Id, z => DateOnly.FromDateTime(z.Datum.Value));

    private void AddZeile(TableLayoutPanel tabelle, Rechnung rechnung, KundenDatei kunden, DateOnly heute)
    {
        var zeile = tabelle.RowCount++;
        tabelle.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var haken = new CheckBox { Text = rechnung.Nummer, AutoSize = true, UseMnemonic = false, Anchor = AnchorStyles.Left, Margin = new Padding(3, 4, 12, 4) };
        var kunde = rechnung.EmpfaengerAus(kunden);
        var datum = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 110,
            MaxDate = heute.ToDateTime(TimeOnly.MinValue),
            Value = heute.ToDateTime(TimeOnly.MinValue),
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3, 2, 3, 2),
        };

        tabelle.Controls.Add(haken, 0, zeile);
        var kundenName = Zelle(kunde is null || kunde.Firma.Length == 0 ? UiText.KundeUnbekannt : kunde.Firma);
        kundenName.MaximumSize = new Size(240, 0);
        tabelle.Controls.Add(kundenName, 1, zeile);
        tabelle.Controls.Add(Zelle(rechnung.Zeitraum), 2, zeile);
        var betrag = Zelle(UiText.Betrag(Rechnungsbetrag.Berechnen(rechnung).Brutto));
        betrag.Anchor = AnchorStyles.Right;
        tabelle.Controls.Add(betrag, 3, zeile);
        var seit = Zelle(UiText.SeitTagen(Zahlungserinnerung.TageSeit(rechnung, heute)));
        seit.ForeColor = UiStyle.Danger;
        tabelle.Controls.Add(seit, 4, zeile);
        tabelle.Controls.Add(datum, 5, zeile);

        haken.CheckedChanged += (_, _) => Aktualisieren();
        // Changing the date means: this one is paid.
        datum.ValueChanged += (_, _) => haken.Checked = true;
        _zeilen.Add((rechnung, haken, datum));
    }

    private static Label Zelle(string text) =>
        new() { Text = text, AutoSize = true, UseMnemonic = false, Anchor = AnchorStyles.Left, Margin = new Padding(3, 4, 12, 4) };

    private void AlleSetzen(bool an)
    {
        if (_aendertAlle)
            return;
        _aendertAlle = true;
        foreach (var zeile in _zeilen)
            zeile.Haken.Checked = an;
        _aendertAlle = false;
        Aktualisieren();
    }

    private void Aktualisieren()
    {
        _speichern.Enabled = _zeilen.Any(z => z.Haken.Checked);
        if (_aendertAlle)
            return;
        _aendertAlle = true;
        _alle.Checked = _zeilen.Count > 0 && _zeilen.All(z => z.Haken.Checked);
        _aendertAlle = false;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UiStyle.FitToScreen(this);
    }
}

/// <summary>"Als bezahlt markieren…": asks only for the date of the payment.</summary>
internal sealed class BezahltAmDialog : Form
{
    private readonly DateTimePicker _datum;

    public BezahltAmDialog(IReadOnlyList<string> nummern, DateOnly heute)
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = UiText.AppTitle;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;

        _datum = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 120,
            MaxDate = heute.ToDateTime(TimeOnly.MinValue),
            Value = heute.ToDateTime(TimeOnly.MinValue),
            Margin = new Padding(3, 2, 3, 3),
        };

        var tabelle = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Padding = new Padding(12) };
        var frage = new Label { Text = UiText.BezahltFrage(nummern), AutoSize = true, UseMnemonic = false, MaximumSize = new Size(420, 0), Margin = new Padding(3, 0, 3, 12) };
        tabelle.Controls.Add(frage, 0, 0);
        tabelle.SetColumnSpan(frage, 2);
        tabelle.Controls.Add(new Label { Text = UiText.BezahltAmLabel, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 5, 8, 3) }, 0, 1);
        tabelle.Controls.Add(_datum, 1, 1);

        var ok = UiStyle.CreateButton(UiText.AlsBezahltSpeichern);
        UiStyle.MakePrimary(ok);
        ok.DialogResult = DialogResult.OK;
        var abbrechen = UiStyle.CreateButton(UiText.Cancel);
        abbrechen.DialogResult = DialogResult.Cancel;
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Anchor = AnchorStyles.Right, Margin = new Padding(0, 16, 0, 0) };
        buttons.Controls.Add(abbrechen);
        buttons.Controls.Add(ok);
        tabelle.Controls.Add(buttons, 0, 2);
        tabelle.SetColumnSpan(buttons, 2);

        Controls.Add(tabelle);
        AcceptButton = ok;
        CancelButton = abbrechen;
        ResumeLayout(false);
        PerformLayout();
    }

    public DateOnly BezahltAm => DateOnly.FromDateTime(_datum.Value);
}
