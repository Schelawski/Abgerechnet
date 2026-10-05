using Abgerechnet.Core.Einrichtung;

namespace Abgerechnet.UI.Einrichtung;

/// <summary>
/// One page of the welcome wizard. The wizard owns the header and the buttons; the page describes what they do.
/// </summary>
internal abstract class EinrichtungsSeite : UserControl
{
    /// <summary>Width at which page texts wrap (logical pixels).</summary>
    protected const int TextWidth = 780;

    protected EinrichtungsSeite(EinrichtungsAssistent assistent)
    {
        Assistent = assistent;
        AutoScaleMode = AutoScaleMode.Inherit;
        Dock = DockStyle.Fill;
    }

    protected EinrichtungsAssistent Assistent { get; }

    public abstract EinrichtungsSchritt Schritt { get; }

    public abstract string Titel { get; }

    /// <summary>Text of the main (blue) button.</summary>
    public virtual string PrimaerText => UiText.EinrichtungWeiter;

    /// <summary>Text of the optional second button, <c>null</c> to hide it.</summary>
    public virtual string? SekundaerText => null;

    /// <summary>Called once the page is visible, e.g. to set the focus or start the preview.</summary>
    public virtual void Angezeigt()
    {
    }

    /// <summary>The main button: save what the page set up and go on.</summary>
    public virtual void Primaer() => Assistent.Weiter();

    public virtual void Sekundaer()
    {
    }

    /// <summary>A label that wraps at the page width.</summary>
    protected static Label Absatz(string text, Padding? margin = null) => new()
    {
        Text = text,
        AutoSize = true,
        UseMnemonic = false,
        MaximumSize = new Size(TextWidth, 0),
        Margin = margin ?? new Padding(3, 0, 3, 12),
    };

    /// <summary>A vertical stack of auto-sized rows, filling the page.</summary>
    protected static TableLayoutPanel Stapel()
    {
        var stapel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Margin = new Padding(0),
        };
        stapel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return stapel;
    }

    /// <summary>Appends an auto-sized row to a <see cref="Stapel"/>.</summary>
    protected static void Hinzufuegen(TableLayoutPanel stapel, Control control)
    {
        stapel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stapel.Controls.Add(control, 0, stapel.RowCount++);
    }

    /// <summary>Appends a row that takes the remaining height.</summary>
    protected static void HinzufuegenFuellend(TableLayoutPanel stapel, Control control)
    {
        control.Dock = DockStyle.Fill;
        stapel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        stapel.Controls.Add(control, 0, stapel.RowCount++);
    }

    /// <summary>The most important promise, set apart in green.</summary>
    protected static Control Hervorgehoben(string text)
    {
        var label = new Label
        {
            Text = text,
            AutoSize = true,
            UseMnemonic = false,
            MaximumSize = new Size(TextWidth - 24, 0),
            ForeColor = UiStyle.Success,
            Margin = new Padding(0),
        };
        label.Font = new Font(label.Font, FontStyle.Bold);

        var box = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = UiStyle.SuccessBack,
            Padding = new Padding(10, 8, 10, 8),
            Margin = new Padding(3, 0, 3, 14),
        };
        box.Controls.Add(label);
        return box;
    }
}

/// <summary>What Abgerechnet does and that the data stays on this computer.</summary>
internal sealed class WillkommenSeite : EinrichtungsSeite
{
    public WillkommenSeite(EinrichtungsAssistent assistent)
        : base(assistent)
    {
        var stapel = Stapel();
        Hinzufuegen(stapel, Absatz(UiText.WillkommenText));
        Hinzufuegen(stapel, Hervorgehoben(UiText.WillkommenDatenschutz));
        Hinzufuegen(stapel, Absatz(UiText.WillkommenSchritte, new Padding(3, 4, 3, 12)));
        Controls.Add(stapel);
    }

    public override EinrichtungsSchritt Schritt => EinrichtungsSchritt.Willkommen;

    public override string Titel => UiText.WillkommenTitel;
}
