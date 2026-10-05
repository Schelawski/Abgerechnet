namespace Abgerechnet.UI;

/// <summary>
/// Builds simple two-column forms: caption on the left, input on the right, one row per field.
/// </summary>
internal sealed class FormLayout
{
    public FormLayout()
    {
        // The table grows with its rows; the host panel scrolls when the window is too small. (An AutoScroll
        // table measures wrapping labels against a stale column width and leaves large gaps.)
        Table = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(12, 10, 12, 10),
        };
        Table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        Table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        Host = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        Host.Controls.Add(Table);
    }

    public TableLayoutPanel Table { get; }

    /// <summary>The scrolling panel that contains <see cref="Table"/>; add this one to the window.</summary>
    public Panel Host { get; }

    /// <summary>Adds a row with a caption and a control that fills the width.</summary>
    public T Add<T>(string caption, T control)
        where T : Control
    {
        var label = new Label
        {
            Text = caption,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3, 6, 12, 3),
        };
        if (control is TextBox { Multiline: true })
            label.Anchor = AnchorStyles.Left | AnchorStyles.Top;

        if (control.Dock == DockStyle.None && control.Anchor == (AnchorStyles.Top | AnchorStyles.Left) && control is not CheckBox)
            control.Anchor = AnchorStyles.Left | AnchorStyles.Right;

        AddRow(label, control);
        return control;
    }

    /// <summary>Adds a control that spans both columns, e.g. a check box or a hint.</summary>
    public T AddWide<T>(T control)
        where T : Control
    {
        Table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Table.Controls.Add(control, 0, Table.RowCount);
        Table.SetColumnSpan(control, 2);
        Table.RowCount++;
        return control;
    }

    /// <summary>Adds a section heading.</summary>
    public void AddHeading(string text)
    {
        var label = new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(Control.DefaultFont, FontStyle.Bold),
            Margin = new Padding(3, Table.RowCount == 0 ? 3 : 14, 3, 4),
        };
        AddWide(label);
    }

    /// <summary>Adds a small grey hint under the previous row (in the input column).</summary>
    public Label AddHint(string text) => AddHint(UiStyle.CreateCaption(text));

    /// <summary>Adds the given label as a hint, e.g. one whose text changes.</summary>
    public Label AddHint(Label label)
    {
        label.Margin = new Padding(3, 0, 3, 6);
        // Stretched across the column, so the label wraps at the column width.
        label.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        AddRow(new Label { AutoSize = true, Margin = Padding.Empty }, label);
        return label;
    }

    private void AddRow(Control left, Control right)
    {
        Table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Table.Controls.Add(left, 0, Table.RowCount);
        Table.Controls.Add(right, 1, Table.RowCount);
        Table.RowCount++;
    }

    /// <summary>Puts several controls into one row, e.g. postcode and city; the last one fills the rest.</summary>
    public static TableLayoutPanel Row(params Control[] controls)
    {
        var row = new TableLayoutPanel
        {
            ColumnCount = controls.Length,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
        };
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        for (var i = 0; i < controls.Length; i++)
        {
            var last = i == controls.Length - 1;
            row.ColumnStyles.Add(last ? new ColumnStyle(SizeType.Percent, 100) : new ColumnStyle(SizeType.AutoSize));
            if (last && controls[i] is TextBox)
                controls[i].Anchor = AnchorStyles.Left | AnchorStyles.Right;
            else if (controls[i] is Label)
                controls[i].Anchor = AnchorStyles.Left;
            row.Controls.Add(controls[i], i, 0);
        }
        return row;
    }
}
