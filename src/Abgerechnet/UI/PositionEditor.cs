using System.Globalization;
using Abgerechnet.Core.Model;

namespace Abgerechnet.UI;

/// <summary>
/// One line item in the invoice form: period, description with an optional detail line, quantity, unit, unit price
/// and the amount, which follows every keystroke. Buttons move the item or remove it.
/// </summary>
/// <remarks>
/// The editors are created after the form has been scaled (see <see cref="RechnungForm"/>), so all sizes are
/// converted with the form's DPI here – the same for editors shown at the start and those added later.
/// </remarks>
internal sealed class PositionEditor : TableLayoutPanel
{
    /// <summary>Column widths (logical pixels) shared with the header row; 0 = the description fills the rest.</summary>
    private static readonly int[] Widths = [130, 0, 85, 95, 110, 115, 96];

    private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

    private readonly TextBox _zeitraum = new() { Dock = DockStyle.Fill };
    private readonly TextBox _beschreibung = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, AcceptsReturn = true };
    private readonly TextBox _detail = new() { Dock = DockStyle.Fill, PlaceholderText = UiText.PositionDetail };
    private readonly NumericUpDown _menge = new() { Dock = DockStyle.Top, DecimalPlaces = 2, Maximum = 1_000_000, TextAlign = HorizontalAlignment.Right };
    private readonly ComboBox _einheit = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDown };
    private readonly NumericUpDown _einzelpreis = new() { Dock = DockStyle.Top, DecimalPlaces = 2, Maximum = 10_000_000, ThousandsSeparator = true, TextAlign = HorizontalAlignment.Right };
    private readonly Label _betrag = new() { Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleRight, Font = new Font(Control.DefaultFont, FontStyle.Bold) };
    private readonly Button _hoch;
    private readonly Button _runter;
    private readonly Button _entfernen;
    private readonly ToolTip _toolTip = new();
    private bool _loading;

    /// <param name="dpi">DPI of the form the editor is shown in.</param>
    public PositionEditor(int dpi)
    {
        Configure(this, dpi);
        UiStyle.SelectAllOnEnter(_menge);
        UiStyle.SelectAllOnEnter(_einzelpreis);
        Anchor = AnchorStyles.Left | AnchorStyles.Right;
        Margin = new Padding(0, Scale(4, dpi), 0, Scale(8, dpi));
        _beschreibung.Height = Scale(46, dpi);
        _betrag.Height = Scale(26, dpi);
        _hoch = SmallButton("▲", UiText.PositionNachOben, dpi);
        _runter = SmallButton("▼", UiText.PositionNachUnten, dpi);
        _entfernen = SmallButton("✕", UiText.PositionEntfernen, dpi);

        Controls.Add(_zeitraum, 0, 0);
        Controls.Add(_beschreibung, 1, 0);
        Controls.Add(_menge, 2, 0);
        Controls.Add(_einheit, 3, 0);
        Controls.Add(_einzelpreis, 4, 0);
        Controls.Add(_betrag, 5, 0);
        var buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty, Dock = DockStyle.Top };
        buttons.Controls.AddRange([_hoch, _runter, _entfernen]);
        Controls.Add(buttons, 6, 0);
        Controls.Add(_detail, 1, 1);

        _einheit.Items.AddRange(UiText.Einheiten);
        foreach (var button in new[] { _hoch, _runter, _entfernen })
            _toolTip.SetToolTip(button, button.AccessibleName);

        _hoch.Click += (_, _) => MoveUpRequested?.Invoke(this, EventArgs.Empty);
        _runter.Click += (_, _) => MoveDownRequested?.Invoke(this, EventArgs.Empty);
        _entfernen.Click += (_, _) => RemoveRequested?.Invoke(this, EventArgs.Empty);
        foreach (Control input in new Control[] { _zeitraum, _beschreibung, _detail, _menge, _einheit, _einzelpreis })
            input.TextChanged += (_, _) => OnInputChanged();
        _menge.ValueChanged += (_, _) => OnInputChanged();
        _einzelpreis.ValueChanged += (_, _) => OnInputChanged();
    }

    /// <summary>Any input changed; the amount is already updated.</summary>
    public event EventHandler? Changed;

    public event EventHandler? MoveUpRequested;

    public event EventHandler? MoveDownRequested;

    public event EventHandler? RemoveRequested;

    /// <summary>The line item being edited (its id is kept).</summary>
    public Position Position { get; private set; } = new();

    /// <summary>The header row with the column captions, aligned with the editors.</summary>
    public static TableLayoutPanel CreateHeader(int dpi)
    {
        var table = new TableLayoutPanel();
        Configure(table, dpi);
        table.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        table.Margin = Padding.Empty;
        string[] captions = [UiText.PositionZeitraum, UiText.PositionBeschreibung, UiText.PositionMenge, UiText.PositionEinheit, UiText.PositionEinzelpreis, UiText.PositionBetrag];
        for (var i = 0; i < captions.Length; i++)
        {
            var label = UiStyle.CreateCaption(captions[i]);
            label.Margin = new Padding(3, 0, 3, 0);
            if (i is 2 or 4 or 5)
                label.Anchor = AnchorStyles.Right; // numbers are right-aligned
            table.Controls.Add(label, i, 0);
        }
        return table;
    }

    private static void Configure(TableLayoutPanel table, int dpi)
    {
        table.AutoSize = true;
        table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        table.ColumnCount = Widths.Length;
        table.RowCount = 2;
        foreach (var width in Widths)
            table.ColumnStyles.Add(width == 0 ? new ColumnStyle(SizeType.Percent, 100) : new ColumnStyle(SizeType.Absolute, Scale(width, dpi)));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    }

    private static int Scale(int logical, int dpi) => (int)Math.Round(logical * dpi / 96.0);

    private static Button SmallButton(string text, string accessibleName, int dpi) => new()
    {
        Text = text,
        AccessibleName = accessibleName,
        Width = Scale(28, dpi),
        Height = Scale(26, dpi),
        Margin = new Padding(1, 0, 1, 0),
        FlatStyle = FlatStyle.Flat,
        FlatAppearance = { BorderSize = 0 },
        ForeColor = UiStyle.MutedText,
        TabStop = false,
    };

    public void ShowPosition(Position position)
    {
        _loading = true;
        Position = position;
        _zeitraum.Text = position.Zeitraum;
        _beschreibung.Text = position.Beschreibung;
        _detail.Text = position.Detail;
        _menge.Value = Math.Clamp(position.Menge, _menge.Minimum, _menge.Maximum);
        _einheit.Text = position.Einheit;
        _einzelpreis.Value = Math.Clamp(position.Einzelpreis, _einzelpreis.Minimum, _einzelpreis.Maximum);
        _loading = false;
        UpdateBetrag();
    }

    /// <summary>The invoice's period, shown greyed in the period box when the item has none of its own.</summary>
    public void SetZeitraumPlatzhalter(string rechnungsZeitraum) =>
        _zeitraum.PlaceholderText = UiText.ZeitraumPlatzhalter(rechnungsZeitraum);

    /// <summary>Enables or disables moving and removing (the first cannot go up, the last item cannot be removed).</summary>
    public void SetButtons(bool canMoveUp, bool canMoveDown, bool canRemove)
    {
        _hoch.Enabled = canMoveUp;
        _runter.Enabled = canMoveDown;
        _entfernen.Enabled = canRemove;
    }

    public void SetReadOnly(bool readOnly)
    {
        foreach (var box in new[] { _zeitraum, _beschreibung, _detail })
            box.ReadOnly = readOnly;
        _menge.ReadOnly = readOnly;
        _menge.Increment = readOnly ? 0 : 1;
        _einzelpreis.ReadOnly = readOnly;
        _einzelpreis.Increment = readOnly ? 0 : 1;
        _einheit.Enabled = !readOnly;
        _hoch.Visible = _runter.Visible = _entfernen.Visible = !readOnly;
    }

    public void FocusBeschreibung() => _beschreibung.Focus();

    private void OnInputChanged()
    {
        if (_loading)
            return;

        Position.Zeitraum = _zeitraum.Text.Trim();
        Position.Beschreibung = _beschreibung.Text.Trim();
        Position.Detail = _detail.Text.Trim();
        Position.Menge = LiveValue(_menge);
        Position.Einheit = _einheit.Text.Trim();
        Position.Einzelpreis = LiveValue(_einzelpreis);
        UpdateBetrag();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateBetrag() => _betrag.Text = UiText.Betrag(Rechnungsbetrag.Positionsbetrag(Position));

    /// <summary>
    /// The value as typed so far. <see cref="NumericUpDown.Value"/> only changes when the box is left, but the amount
    /// should follow every keystroke. Thousands separators ("1.234,50") are allowed.
    /// </summary>
    internal static decimal LiveValue(NumericUpDown box)
    {
        var text = box.Text.Replace(".", string.Empty, StringComparison.Ordinal);
        return decimal.TryParse(text, NumberStyles.Number, German, out var value)
            ? Math.Clamp(value, box.Minimum, box.Maximum)
            : box.Value;
    }
}
