using Abgerechnet.Core.Model;

namespace Abgerechnet.UI;

/// <summary>
/// Input for the sender's data and the invoice settings (<c>abgerechnet.json</c>). Used by the dialog
/// "Meine Daten" and later by the welcome wizard (issue #11).
/// </summary>
internal sealed class MeineDatenPanel : UserControl
{
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill, Padding = new Point(12, 4) };

    // Absender
    private readonly TextBox _firma = new();
    private readonly TextBox _unterzeile = new();
    private readonly TextBox _name = new();
    private readonly TextBox _strasse = new();
    private readonly TextBox _plz = new() { Width = 70 };
    private readonly TextBox _ort = new();
    private readonly TextBox _land = new();
    private readonly TextBox _telefon = new();
    private readonly TextBox _email = new();
    private readonly TextBox _website = new();

    // Steuer und Bank
    private readonly TextBox _steuernummer = new();
    private readonly TextBox _ustIdNr = new();
    private readonly CheckBox _kleinunternehmer = new() { Text = UiText.Kleinunternehmer, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
    private readonly TextBox _kleinunternehmerHinweis = new() { Multiline = true, Height = 44, ScrollBars = ScrollBars.Vertical };
    private readonly NumericUpDown _umsatzsteuersatz = new() { Minimum = 0, Maximum = 100, DecimalPlaces = 1, Width = 70, TextAlign = HorizontalAlignment.Right };
    private readonly TextBox _kontoinhaber = new();
    private readonly TextBox _bank = new();
    private readonly TextBox _iban = new() { Width = 260 };
    private readonly Label _ibanStatus = new() { AutoSize = true, Margin = new Padding(8, 6, 3, 3) };
    private readonly TextBox _bic = new() { Width = 160 };

    // Rechnungen
    private readonly NumericUpDown _zahlungsziel = new() { Minimum = 0, Maximum = 365, Width = 70, TextAlign = HorizontalAlignment.Right };
    private readonly TextBox _naechsteNummer = new() { Width = 160 };
    private readonly TextBox _pdfDateiname = new();
    private readonly Label _pdfBeispiel = UiStyle.CreateCaption(string.Empty);
    private readonly TextBox _beschreibung = new() { Multiline = true, Height = 60, ScrollBars = ScrollBars.Vertical };
    private readonly ComboBox _einheit = new() { DropDownStyle = ComboBoxStyle.DropDown, Width = 120 };
    private readonly NumericUpDown _einzelpreis = new()
    {
        Minimum = 0,
        Maximum = 1_000_000,
        DecimalPlaces = 2,
        ThousandsSeparator = true,
        Width = 110,
        TextAlign = HorizontalAlignment.Right,
    };

    private readonly ToolTip _toolTip = new() { AutoPopDelay = 15000 };

    public MeineDatenPanel()
    {
        Dock = DockStyle.Fill;
        foreach (var box in new[] { _umsatzsteuersatz, _zahlungsziel, _einzelpreis })
            UiStyle.SelectAllOnEnter(box);
        _tabs.TabPages.Add(CreatePage(UiText.TabAbsender, BuildAbsender()));
        _tabs.TabPages.Add(CreatePage(UiText.TabSteuerBank, BuildSteuerBank()));
        _tabs.TabPages.Add(CreatePage(UiText.TabRechnungen, BuildRechnungen()));
        Controls.Add(_tabs);

        _einheit.Items.AddRange(UiText.Einheiten);
        _kleinunternehmer.CheckedChanged += (_, _) => UpdateKleinunternehmer();
        _iban.TextChanged += (_, _) => UpdateIbanStatus();
        _iban.Leave += (_, _) =>
        {
            if (Iban.IsValid(_iban.Text))
                _iban.Text = Iban.Format(_iban.Text);
        };
        _pdfDateiname.TextChanged += (_, _) => UpdatePdfBeispiel();
        _naechsteNummer.TextChanged += (_, _) => UpdatePdfBeispiel();
    }

    private static TabPage CreatePage(string title, FormLayout layout)
    {
        var page = new TabPage(title) { UseVisualStyleBackColor = true };
        page.Controls.Add(layout.Host);
        return page;
    }

    private FormLayout BuildAbsender()
    {
        var form = new FormLayout();
        form.Add(UiText.Firma, _firma);
        form.Add(UiText.Unterzeile, _unterzeile);
        form.AddHint(UiText.UnterzeileHint);
        form.Add(UiText.Name, _name);
        form.Add(UiText.Strasse, _strasse);
        form.Add(UiText.PlzOrt, FormLayout.Row(_plz, _ort));
        form.Add(UiText.Land, _land);
        form.Add(UiText.Telefon, _telefon);
        form.Add(UiText.Email, _email);
        form.Add(UiText.Website, _website);
        return form;
    }

    private FormLayout BuildSteuerBank()
    {
        var form = new FormLayout();
        form.AddHeading(UiText.HeadingSteuer);
        form.Add(UiText.Steuernummer, _steuernummer);
        form.Add(UiText.UstIdNr, _ustIdNr);
        form.AddHint(UiText.SteuernummerHint);
        form.AddWide(_kleinunternehmer);
        form.Add(UiText.KleinunternehmerHinweis, _kleinunternehmerHinweis);
        form.Add(UiText.Umsatzsteuersatz, FormLayout.Row(_umsatzsteuersatz, new Label { Text = UiText.Prozent, AutoSize = true }));

        form.AddHeading(UiText.HeadingBank);
        form.Add(UiText.Kontoinhaber, _kontoinhaber);
        form.Add(UiText.Bank, _bank);
        form.Add(UiText.Iban, FormLayout.Row(_iban, _ibanStatus));
        form.Add(UiText.Bic, _bic);
        return form;
    }

    private FormLayout BuildRechnungen()
    {
        var form = new FormLayout();
        form.AddHeading(UiText.HeadingZahlung);
        form.Add(UiText.Zahlungsziel, FormLayout.Row(_zahlungsziel, new Label { Text = UiText.Tage, AutoSize = true }));
        form.AddHint(UiText.ZahlungszielHint);
        form.Add(UiText.NaechsteNummer, _naechsteNummer);
        form.AddHint(UiText.NaechsteNummerHint);
        form.Add(UiText.PdfDateiname, _pdfDateiname);
        form.AddHint(_pdfBeispiel);
        form.AddHint(UiText.PdfDateinameHint(string.Join(" ", PdfDateiname.Platzhalter)));

        form.AddHeading(UiText.HeadingNeuePosition);
        form.Add(UiText.Beschreibung, _beschreibung);
        form.Add(UiText.Einheit, _einheit);
        form.Add(UiText.Einzelpreis, FormLayout.Row(_einzelpreis, new Label { Text = UiText.Euro, AutoSize = true }));
        return form;
    }

    /// <summary>Shows the values of <paramref name="einstellungen"/>.</summary>
    public void LoadFrom(Einstellungen einstellungen)
    {
        var a = einstellungen.Absender;
        _firma.Text = a.Firma;
        _unterzeile.Text = a.Unterzeile;
        _name.Text = a.Name;
        _strasse.Text = a.Strasse;
        _plz.Text = a.Plz;
        _ort.Text = a.Ort;
        _land.Text = a.Land;
        _telefon.Text = a.Telefon;
        _email.Text = a.Email;
        _website.Text = a.Website;
        _steuernummer.Text = a.Steuernummer;
        _ustIdNr.Text = a.UstIdNr;

        var r = einstellungen.Rechnung;
        _kleinunternehmer.Checked = r.Kleinunternehmer;
        _kleinunternehmerHinweis.Text = r.KleinunternehmerHinweis;
        _umsatzsteuersatz.Value = r.Umsatzsteuersatz;
        _zahlungsziel.Value = Math.Clamp(r.ZahlungszielTage, 0, 365);
        _naechsteNummer.Text = r.NaechsteNummer;
        _pdfDateiname.Text = r.PdfDateiname;

        var b = einstellungen.Bank;
        _kontoinhaber.Text = b.Kontoinhaber;
        _bank.Text = b.Bank;
        _iban.Text = b.Iban;
        _bic.Text = b.Bic;

        var p = einstellungen.NeuePosition;
        _beschreibung.Text = p.Beschreibung;
        _einheit.Text = p.Einheit;
        _einzelpreis.Value = Math.Clamp(p.Einzelpreis, _einzelpreis.Minimum, _einzelpreis.Maximum);

        UpdateKleinunternehmer();
        UpdateIbanStatus();
        UpdatePdfBeispiel();
    }

    /// <summary>
    /// Checks the input. Returns a message and selects the field when something must be corrected, otherwise
    /// <c>null</c>.
    /// </summary>
    public string? CheckInput()
    {
        if (IsEmpty(_firma) && IsEmpty(_name))
            return Fail(_firma, UiText.NameRequired);
        if (IsEmpty(_strasse) || IsEmpty(_plz) || IsEmpty(_ort))
            return Fail(IsEmpty(_strasse) ? _strasse : IsEmpty(_plz) ? _plz : _ort, UiText.AddressRequired);
        if (!IsEmpty(_iban) && !Iban.IsValid(_iban.Text))
            return Fail(_iban, UiText.IbanInvalidMessage);
        var unknown = PdfDateiname.UnbekanntePlatzhalter(_pdfDateiname.Text);
        if (unknown.Count > 0)
            return Fail(_pdfDateiname, UiText.UnknownPlaceholders(string.Join(", ", unknown)));
        return null;
    }

    /// <summary>Shows the first tab and puts the cursor into the first field.</summary>
    public void FocusFirstField()
    {
        _tabs.SelectedIndex = 0;
        _firma.Focus();
    }

    /// <summary>True when neither a tax number nor a VAT ID was entered (only a warning).</summary>
    public bool SteuernummerFehlt => IsEmpty(_steuernummer) && IsEmpty(_ustIdNr);

    /// <summary>Writes the input into <paramref name="einstellungen"/>.</summary>
    public void ApplyTo(Einstellungen einstellungen)
    {
        var a = einstellungen.Absender;
        a.Firma = _firma.Text.Trim();
        a.Unterzeile = _unterzeile.Text.Trim();
        a.Name = _name.Text.Trim();
        a.Strasse = _strasse.Text.Trim();
        a.Plz = _plz.Text.Trim();
        a.Ort = _ort.Text.Trim();
        a.Land = _land.Text.Trim();
        a.Telefon = _telefon.Text.Trim();
        a.Email = _email.Text.Trim();
        a.Website = _website.Text.Trim();
        a.Steuernummer = _steuernummer.Text.Trim();
        a.UstIdNr = _ustIdNr.Text.Trim();

        var r = einstellungen.Rechnung;
        r.Kleinunternehmer = _kleinunternehmer.Checked;
        r.KleinunternehmerHinweis = _kleinunternehmerHinweis.Text.Trim();
        r.Umsatzsteuersatz = _umsatzsteuersatz.Value;
        r.ZahlungszielTage = (int)_zahlungsziel.Value;
        r.NaechsteNummer = _naechsteNummer.Text.Trim();
        r.PdfDateiname = string.IsNullOrWhiteSpace(_pdfDateiname.Text) ? PdfDateiname.DefaultMuster : _pdfDateiname.Text.Trim();

        var b = einstellungen.Bank;
        b.Kontoinhaber = _kontoinhaber.Text.Trim();
        b.Bank = _bank.Text.Trim();
        b.Iban = IsEmpty(_iban) ? string.Empty : Iban.Format(_iban.Text);
        b.Bic = _bic.Text.Trim().ToUpperInvariant();

        var p = einstellungen.NeuePosition;
        p.Beschreibung = _beschreibung.Text.Trim();
        p.Einheit = _einheit.Text.Trim();
        p.Einzelpreis = _einzelpreis.Value;
    }

    private void UpdateKleinunternehmer()
    {
        _kleinunternehmerHinweis.Enabled = _kleinunternehmer.Checked;
        _umsatzsteuersatz.Enabled = !_kleinunternehmer.Checked;
        if (_kleinunternehmer.Checked && IsEmpty(_kleinunternehmerHinweis))
            _kleinunternehmerHinweis.Text = RechnungsEinstellungen.DefaultKleinunternehmerHinweis;
    }

    private void UpdateIbanStatus()
    {
        if (IsEmpty(_iban))
        {
            _ibanStatus.Text = string.Empty;
            return;
        }

        var valid = Iban.IsValid(_iban.Text);
        _ibanStatus.Text = valid ? UiText.IbanValid : UiText.IbanInvalid;
        _ibanStatus.ForeColor = valid ? UiStyle.Success : UiStyle.Danger;
    }

    private void UpdatePdfBeispiel()
    {
        var nummer = IsEmpty(_naechsteNummer) ? "111412" : _naechsteNummer.Text.Trim();
        var beispielKunde = new Kunde { Firma = "Nordlicht GmbH", Kurzname = "Nordlicht" };
        var name = PdfDateiname.Erzeugen(_pdfDateiname.Text, nummer, DateOnly.FromDateTime(DateTime.Today), beispielKunde);
        _pdfBeispiel.Text = UiText.PdfDateinameExample(name);
    }

    private string Fail(Control control, string message)
    {
        var page = _tabs.TabPages.Cast<TabPage>().First(p => p.Contains(control));
        _tabs.SelectedTab = page;
        control.Focus();
        return message;
    }

    private static bool IsEmpty(TextBox box) => string.IsNullOrWhiteSpace(box.Text);
}
