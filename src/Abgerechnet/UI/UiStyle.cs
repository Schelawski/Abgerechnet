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
