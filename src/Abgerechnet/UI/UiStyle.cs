namespace Abgerechnet.UI;

/// <summary>
/// Colors, fonts and small helpers shared by the windows (same palette as Wortlaut).
/// </summary>
internal static class UiStyle
{
    public static readonly Color Accent = Color.FromArgb(37, 99, 235);
    public static readonly Color AccentDisabled = Color.FromArgb(191, 207, 240);
    public static readonly Color MutedText = Color.FromArgb(75, 85, 99);
    public static readonly Color GridLine = Color.FromArgb(229, 231, 235);
    public static readonly Color Success = Color.FromArgb(22, 101, 52);
    public static readonly Color Danger = Color.FromArgb(153, 27, 27);
    public static readonly Color HintBack = Color.FromArgb(254, 243, 199);

    /// <summary>Text colors of the invoice status in the list and the tiles.</summary>
    public static readonly Color StatusEntwurf = Color.FromArgb(107, 114, 128);
    public static readonly Color StatusOffen = Color.FromArgb(180, 83, 9);
    public static readonly Color StatusBezahlt = Color.FromArgb(21, 128, 61);
    public static readonly Color StatusStorniert = Color.FromArgb(156, 163, 175);

    public static Color StatusColor(Core.Model.RechnungsStatus status) => status switch
    {
        Core.Model.RechnungsStatus.Offen => StatusOffen,
        Core.Model.RechnungsStatus.Bezahlt => StatusBezahlt,
        Core.Model.RechnungsStatus.Storniert => StatusStorniert,
        _ => StatusEntwurf,
    };

    /// <summary>
    /// Selects the whole number when the box is entered, so typing replaces it. Without this the cursor stands
    /// before "0,00" after Tab, and typing "12,5" gives "12,50,00".
    /// </summary>
    public static T SelectAllOnEnter<T>(T box)
        where T : NumericUpDown
    {
        box.Enter += (_, _) => box.BeginInvoke(() => box.Select(0, box.Text.Length));
        return box;
    }

    /// <summary>
    /// Shrinks a window that is larger than the screen's working area (e.g. at 150 % scaling on a laptop) and moves it
    /// fully onto the screen, so its buttons at the bottom stay reachable. Call it from OnLoad.
    /// </summary>
    public static void FitToScreen(Form form)
    {
        var area = Screen.FromControl(form).WorkingArea;
        var size = new Size(Math.Min(form.Width, area.Width), Math.Min(form.Height, area.Height));
        if (size != form.Size)
            form.Size = size;
        var x = Math.Clamp(form.Left, area.Left, area.Right - form.Width);
        var y = Math.Clamp(form.Top, area.Top, area.Bottom - form.Height);
        form.Location = new Point(x, y);
    }

    /// <summary>Filled blue button for the main action.</summary>
    public static void MakePrimary(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.ForeColor = Color.White;
        button.Font = new Font(button.Font, FontStyle.Bold);
        button.UseVisualStyleBackColor = false;

        void Apply() => button.BackColor = button.Enabled ? Accent : AccentDisabled;
        button.EnabledChanged += (_, _) => Apply();
        Apply();
    }

    /// <summary>Common settings for buttons: sized to their text with some breathing room.</summary>
    public static Button CreateButton(string text)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(10, 3, 10, 3),
            MinimumSize = new Size(0, 30),
            UseVisualStyleBackColor = true,
        };
    }

    public static Label CreateCaption(string text) => new()
    {
        Text = text,
        AutoSize = true,
        UseMnemonic = false, // show "&" literally
        ForeColor = MutedText,
        Margin = new Padding(3, 6, 3, 0),
    };
}
