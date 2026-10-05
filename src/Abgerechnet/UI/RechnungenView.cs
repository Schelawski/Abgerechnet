using System.Diagnostics;
using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.UI;

/// <summary>
/// The invoice list of the main window: year and status filter, tiles "Offen", "Bezahlt", "Gesamt", the table and
/// its context menu (issue #5).
/// </summary>
internal sealed class RechnungenView : UserControl
{
    private const int AlleJahre = 0;

    private readonly ComboBox _jahrBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
    private readonly ComboBox _statusBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
    private readonly Kachel _offen = new(UiText.KachelOffen, UiStyle.StatusOffen);
    private readonly Kachel _bezahlt = new(UiText.KachelBezahlt, UiStyle.StatusBezahlt);
    private readonly Kachel _gesamt = new(UiText.KachelGesamt, UiStyle.Accent);
    private readonly Label _entwuerfe = new() { AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = UiStyle.StatusEntwurf };
    private readonly ListView _list = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = true,
        HideSelection = false,
        GridLines = false,
        BorderStyle = BorderStyle.FixedSingle,
    };
    private readonly Label _leer = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = UiStyle.MutedText, Visible = false, BackColor = Color.White };
    private readonly ContextMenuStrip _menu = new();
    private readonly ToolStripMenuItem _pdfItem = new(UiText.MenuPdfOeffnen);
    private readonly ToolStripMenuItem _ordnerItem = new(UiText.MenuImOrdnerZeigen);
    private readonly ToolStripMenuItem _statusItem = new(UiText.MenuStatusAendern);
    private readonly ToolStripMenuItem _loeschenItem = new(UiText.MenuLoeschen) { ShortcutKeyDisplayString = "Entf" };
    private readonly ToolTip _toolTip = new();

    private readonly RechnungsSortierung _sortierung = new();
    private DataFolder? _folder;
    private bool _updatingFilters;

    public RechnungenView()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(12, 10, 12, 8);
        BuildLayout();
        BuildMenu();

        _list.Columns.Add(UiText.SpalteNummer);
        _list.Columns.Add(UiText.SpalteKunde);
        _list.Columns.Add(UiText.SpalteZeitraum);
        _list.Columns.Add(UiText.SpalteDatum);
        _list.Columns.Add(UiText.SpalteStatus);
        _list.Columns.Add(UiText.SpalteBetrag, 0, HorizontalAlignment.Right);
        _list.HandleCreated += (_, _) => SetColumnWidths();
        _list.DpiChangedAfterParent += (_, _) => SetColumnWidths();
        _list.ListViewItemSorter = new ListSorter(_sortierung);
        _list.ColumnClick += (_, e) => SortBy(e.Column);
        _list.Resize += (_, _) => FitKundeColumn();
        _list.KeyDown += OnListKeyDown;
        _list.ItemActivate += (_, _) => OpenSelected(); // double click or Enter
        _list.ContextMenuStrip = _menu;

        _statusBox.Items.Add(UiText.AlleStatus);
        foreach (var status in Enum.GetValues<RechnungsStatus>())
            _statusBox.Items.Add(UiText.StatusName(status));
        _statusBox.SelectedIndex = 0;
        _jahrBox.Format += (_, e) => e.Value = e.ListItem is AlleJahre ? UiText.AlleJahre : e.ListItem?.ToString();
        _jahrBox.SelectedIndexChanged += (_, _) => { if (!_updatingFilters) RefreshList(); };
        _statusBox.SelectedIndexChanged += (_, _) => { if (!_updatingFilters) RefreshList(); };
        _toolTip.SetToolTip(_gesamt, UiText.KachelGesamtTooltip);
    }

    /// <summary>Shows the invoices of <paramref name="folder"/>; the year filter starts at the current year.</summary>
    public void SetFolder(DataFolder folder)
    {
        _folder = folder;
        UpdateYears(selectYear: DateTime.Today.Year);
        RefreshList();
    }

    /// <summary>Shows the list again after the data changed elsewhere (e.g. customers renamed); keeps the filters.</summary>
    public void Aktualisieren()
    {
        UpdateYears(selectYear: null);
        RefreshList();
    }

    private void BuildLayout()
    {
        var filters = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = false, Margin = Padding.Empty, Padding = new Padding(0, 0, 0, 8) };
        filters.Controls.Add(new Label { Text = UiText.FilterJahr, AutoSize = true, Margin = new Padding(0, 7, 6, 0) });
        filters.Controls.Add(_jahrBox);
        filters.Controls.Add(new Label { Text = UiText.FilterStatus, AutoSize = true, Margin = new Padding(18, 7, 6, 0) });
        filters.Controls.Add(_statusBox);

        // "Neue Rechnung" at the right end of the filter row, as in the web tool.
        var neu = UiStyle.CreateButton(UiText.NeueRechnungButton);
        UiStyle.MakePrimary(neu);
        neu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        neu.Margin = new Padding(3, 0, 0, 8);
        neu.Click += (_, _) => NeueRechnung();
        _toolTip.SetToolTip(neu, "Strg+N");
        var filterRow = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        filterRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filterRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.Dock = DockStyle.None;
        filters.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        filterRow.Controls.Add(filters, 0, 0);
        filterRow.Controls.Add(neu, 1, 0);

        var tiles = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, RowCount = 1, Height = 78, Margin = Padding.Empty };
        tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        tiles.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tiles.Controls.Add(_offen, 0, 0);
        tiles.Controls.Add(_bezahlt, 1, 0);
        tiles.Controls.Add(_gesamt, 2, 0);
        tiles.Controls.Add(_entwuerfe, 3, 0);

        var spacer = new Panel { Dock = DockStyle.Top, Height = 12 };
        var listHost = new Panel { Dock = DockStyle.Fill };
        listHost.Controls.Add(_leer);
        listHost.Controls.Add(_list);

        Controls.Add(listHost);
        Controls.Add(spacer);
        Controls.Add(tiles);
        Controls.Add(filterRow);
    }

    private void BuildMenu()
    {
        var oeffnenItem = new ToolStripMenuItem(UiText.MenuOeffnen) { Font = new Font(_menu.Font, FontStyle.Bold), ShortcutKeyDisplayString = "Enter" };
        var kopierenItem = new ToolStripMenuItem(UiText.MenuKopieren);
        var pdfErzeugenItem = new ToolStripMenuItem(UiText.MenuPdfErzeugen);
        oeffnenItem.Click += (_, _) => OpenSelected();
        kopierenItem.Click += (_, _) => KopiereSelected();
        pdfErzeugenItem.Click += (_, _) => PdfErzeugenSelected();
        _menu.Items.AddRange([oeffnenItem, kopierenItem, pdfErzeugenItem, new ToolStripSeparator()]);
        _menu.Opening += (_, _) => oeffnenItem.Enabled = kopierenItem.Enabled = pdfErzeugenItem.Enabled = _list.SelectedItems.Count == 1;

        _pdfItem.Click += (_, _) => OpenPdf();
        _ordnerItem.Click += (_, _) => ShowInFolder();
        foreach (var status in new[] { RechnungsStatus.Offen, RechnungsStatus.Bezahlt, RechnungsStatus.Storniert })
        {
            var item = new ToolStripMenuItem(UiText.StatusName(status)) { Tag = status };
            item.Click += (_, _) => ChangeStatus(status);
            _statusItem.DropDownItems.Add(item);
        }
        _loeschenItem.Click += (_, _) => DeleteSelected();
        _menu.Items.AddRange([_pdfItem, _ordnerItem, new ToolStripSeparator(), _statusItem, new ToolStripSeparator(), _loeschenItem]);
        _menu.Opening += (_, e) =>
        {
            var selected = SelectedRechnungen();
            if (selected.Count == 0)
            {
                e.Cancel = true;
                return;
            }
            var single = selected.Count == 1 ? selected[0] : null;
            var pdf = single is null ? null : _folder?.PdfPfad(single);
            _pdfItem.Enabled = pdf is not null && File.Exists(pdf);
            _ordnerItem.Enabled = single is not null;
            foreach (ToolStripMenuItem item in _statusItem.DropDownItems)
                item.Enabled = selected.Any(r => r.ErlaubteStatuswechsel().Contains((RechnungsStatus)item.Tag!));
            _statusItem.Enabled = _statusItem.DropDownItems.Cast<ToolStripMenuItem>().Any(i => i.Enabled);
        };
    }

    // ----- Filling the list -----

    private void UpdateYears(int? selectYear)
    {
        if (_folder is null)
            return;

        var current = selectYear ?? (_jahrBox.SelectedItem as int? ?? AlleJahre);
        _updatingFilters = true;
        _jahrBox.Items.Clear();
        foreach (var jahr in Uebersicht.Jahre(_folder.Rechnungen.Rechnungen, DateTime.Today.Year))
            _jahrBox.Items.Add(jahr);
        _jahrBox.Items.Add(AlleJahre);
        _jahrBox.SelectedItem = _jahrBox.Items.Contains(current) ? current : DateTime.Today.Year;
        _updatingFilters = false;
    }

    private IEnumerable<Rechnung> RechnungenImJahr()
    {
        var jahr = _jahrBox.SelectedItem as int? ?? AlleJahre;
        var rechnungen = _folder?.Rechnungen.Rechnungen ?? [];
        return jahr == AlleJahre ? rechnungen : rechnungen.Where(r => r.Datum.Year == jahr);
    }

    private void RefreshList(IReadOnlyCollection<Guid>? select = null)
    {
        if (_folder is null)
            return;

        _sortierung.Kunden = _folder.Kunden;
        select ??= SelectedRechnungen().Select(r => r.Id).ToHashSet();
        var imJahr = RechnungenImJahr().ToList();
        var status = _statusBox.SelectedIndex > 0 ? (RechnungsStatus?)(_statusBox.SelectedIndex - 1) : null;
        var sichtbar = status is null ? imJahr : imJahr.Where(r => r.Status == status).ToList();

        _list.BeginUpdate();
        _list.Items.Clear();
        foreach (var rechnung in sichtbar)
        {
            var item = CreateItem(rechnung);
            item.Selected = select.Contains(rechnung.Id);
            _list.Items.Add(item);
        }
        _list.Sort();
        _list.EndUpdate();
        FitKundeColumn();

        var uebersicht = Uebersicht.Berechnen(imJahr);
        _offen.Anzeigen(UiText.Betrag(uebersicht.Offen.Betrag), UiText.Anzahl(uebersicht.Offen.Anzahl));
        _bezahlt.Anzeigen(UiText.Betrag(uebersicht.Bezahlt.Betrag), UiText.Anzahl(uebersicht.Bezahlt.Anzahl));
        _gesamt.Anzeigen(UiText.Betrag(uebersicht.Gesamt.Betrag), UiText.Anzahl(uebersicht.Gesamt.Anzahl));
        _entwuerfe.Text = UiText.Entwuerfe(uebersicht.Entwuerfe);

        _leer.Text = _folder.Rechnungen.Rechnungen.Count == 0 ? UiText.KeineRechnungen : UiText.KeineRechnungenFilter;
        _leer.Visible = sichtbar.Count == 0;
        if (_leer.Visible)
            _leer.BringToFront();
    }

    private ListViewItem CreateItem(Rechnung rechnung)
    {
        var kunde = rechnung.EmpfaengerAus(_folder!.Kunden);
        var betrag = Rechnungsbetrag.Berechnen(rechnung).Brutto;
        var item = new ListViewItem(rechnung.Nummer) { Tag = rechnung, UseItemStyleForSubItems = false };
        item.SubItems.Add(kunde is null || kunde.Firma.Length == 0 ? UiText.KundeUnbekannt : kunde.Firma);
        item.SubItems.Add(rechnung.Zeitraum);
        item.SubItems.Add(UiText.Datum(rechnung.Datum));
        item.SubItems.Add(UiText.StatusName(rechnung.Status)).ForeColor = UiStyle.StatusColor(rechnung.Status);
        item.SubItems.Add(UiText.Betrag(betrag));

        if (rechnung.Status == RechnungsStatus.Storniert)
        {
            // Cancelled invoices stay visible but faded.
            foreach (ListViewItem.ListViewSubItem sub in item.SubItems)
                sub.ForeColor = UiStyle.StatusStorniert;
        }
        return item;
    }

    /// <summary>Column widths in pixels of the current screen (list view columns are not scaled by Windows Forms).</summary>
    private void SetColumnWidths()
    {
        int[] logical = [90, 240, 150, 105, 95, 135];
        for (var i = 0; i < logical.Length; i++)
            _list.Columns[i].Width = _list.LogicalToDeviceUnits(logical[i]);
        FitKundeColumn();
    }

    private void FitKundeColumn()
    {
        if (_list.Columns.Count < 6)
            return;
        var others = _list.Columns.Cast<ColumnHeader>().Where(c => c.Index != 1).Sum(c => c.Width);
        var available = _list.ClientSize.Width - others;
        _list.Columns[1].Width = Math.Max(LogicalToDeviceUnits(140), available);
    }

    private void SortBy(int column)
    {
        _sortierung.Umschalten((RechnungsSpalte)column);
        _list.Sort();
    }

    private List<Rechnung> SelectedRechnungen() =>
        _list.SelectedItems.Cast<ListViewItem>().Select(i => (Rechnung)i.Tag!).ToList();

    // ----- Actions -----

    private void OnListKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
        {
            DeleteSelected();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.A && e.Control)
        {
            foreach (ListViewItem item in _list.Items)
                item.Selected = true;
            e.Handled = true;
        }
    }

    /// <summary>Opens the invoice form for a new draft (button, Ctrl+N).</summary>
    public void NeueRechnung()
    {
        if (_folder is null)
            return;
        var rechnung = Rechnung.Neu(_folder.Einstellungen, _folder.Rechnungen.Rechnungen, DateOnly.FromDateTime(DateTime.Today));
        Bearbeiten(rechnung);
    }

    /// <summary>Opens the selected invoice; with several selected, the one with the keyboard focus.</summary>
    private void OpenSelected()
    {
        if (SelectedRechnungen() is [var rechnung])
            Bearbeiten(rechnung);
        else if (_list.FocusedItem is { Selected: true, Tag: Rechnung focused })
            Bearbeiten(focused);
    }

    /// <summary>"Als neue Rechnung kopieren" – the usual month-end.</summary>
    private void KopiereSelected()
    {
        if (_folder is null || SelectedRechnungen() is not [var vorlage])
            return;
        var kopie = vorlage.AlsNeueRechnung(_folder.Einstellungen, _folder.Rechnungen.Rechnungen, DateOnly.FromDateTime(DateTime.Today));
        Bearbeiten(kopie);
    }

    private void PdfErzeugenSelected()
    {
        if (_folder is null || SelectedRechnungen() is not [var rechnung])
            return;
        using var vorschau = new VorschauForm(_folder, rechnung.Id);
        vorschau.ShowDialog(FindForm());
        if (vorschau.PdfErzeugt)
            ZeigeRechnung(rechnung.Id);
    }

    private void Bearbeiten(Rechnung rechnung)
    {
        if (_folder is null)
            return;
        using var form = new RechnungForm(_folder, rechnung);
        form.ShowDialog(FindForm());
        // Customers may have been added in the form as well.
        UpdateYears(selectYear: null);
        if (form.GespeicherteRechnung is { } id)
            ZeigeRechnung(id);
        else
            RefreshList();
    }

    /// <summary>Selects the invoice in the list, switching the year and status filter if it would be hidden.</summary>
    private void ZeigeRechnung(Guid id)
    {
        if (_folder?.Rechnungen.Rechnungen.Find(r => r.Id == id) is not { } rechnung)
            return;

        _updatingFilters = true;
        if (_jahrBox.SelectedItem is int jahr && jahr != AlleJahre && jahr != rechnung.Datum.Year)
            _jahrBox.SelectedItem = rechnung.Datum.Year;
        if (_statusBox.SelectedIndex > 0 && _statusBox.SelectedIndex - 1 != (int)rechnung.Status)
            _statusBox.SelectedIndex = 0;
        _updatingFilters = false;

        RefreshList([id]);
        if (_list.SelectedItems.Count > 0)
        {
            _list.SelectedItems[0].Focused = true;
            _list.SelectedItems[0].EnsureVisible();
        }
        _list.Focus();
    }

    private void OpenPdf()
    {
        if (SelectedRechnungen() is not [var rechnung] || _folder?.PdfPfad(rechnung) is not { } path)
            return;
        if (!File.Exists(path))
        {
            MessageBox.Show(this, UiText.PdfFehlt(path), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true })?.Dispose();
    }

    private void ShowInFolder()
    {
        if (SelectedRechnungen() is not [var rechnung] || _folder is null)
            return;
        var pdf = _folder.PdfPfad(rechnung);
        if (pdf is not null && File.Exists(pdf))
            Process.Start("explorer.exe", $"/select,\"{pdf}\"")?.Dispose();
        else
            Process.Start(new ProcessStartInfo { FileName = _folder.PdfPath, UseShellExecute = true })?.Dispose();
    }

    private void ChangeStatus(RechnungsStatus status)
    {
        var betroffen = SelectedRechnungen().Where(r => r.ErlaubteStatuswechsel().Contains(status)).ToList();
        if (betroffen.Count == 0)
            return;

        if (status == RechnungsStatus.Storniert
            && MessageBox.Show(this, UiText.StornierenFrage(betroffen.Select(r => r.Nummer).ToList()), UiText.AppTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var ids = betroffen.Select(r => r.Id).ToHashSet();
        var today = DateOnly.FromDateTime(DateTime.Today);
        Save(changed =>
        {
            foreach (var rechnung in changed.Rechnungen.Where(r => ids.Contains(r.Id)))
                rechnung.StatusAendern(status, today);
        }, ids);
    }

    private void DeleteSelected()
    {
        var selected = SelectedRechnungen();
        if (selected.Count == 0)
            return;
        if (selected.Any(r => !r.KannGeloeschtWerden))
        {
            MessageBox.Show(this, UiText.NurEntwuerfeLoeschen, UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (MessageBox.Show(this, UiText.LoeschenFrage(selected.Select(r => r.Nummer).ToList()), UiText.AppTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var ids = selected.Select(r => r.Id).ToHashSet();
        Save(changed => changed.Rechnungen.RemoveAll(r => ids.Contains(r.Id)), []);
    }

    /// <summary>Applies a change to a copy of the invoices and saves it; on failure nothing changes.</summary>
    private void Save(Action<RechnungenDatei> change, IReadOnlyCollection<Guid> select)
    {
        if (_folder is null)
            return;

        var changed = JsonDataFile.Clone(_folder.Rechnungen);
        change(changed);
        try
        {
            _folder.SaveRechnungen(changed);
        }
        catch (DataFileException ex)
        {
            MessageBox.Show(this, UiText.DataFileNotWritable(ex.FilePath, ex.Message), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        UpdateYears(selectYear: null);
        RefreshList(select);
    }

    /// <summary>Lets the list view sort its items with <see cref="RechnungsSortierung"/>.</summary>
    private sealed class ListSorter(RechnungsSortierung sortierung) : System.Collections.IComparer
    {
        public int Compare(object? x, object? y) =>
            sortierung.Compare((Rechnung)((ListViewItem)x!).Tag!, (Rechnung)((ListViewItem)y!).Tag!);
    }
}
