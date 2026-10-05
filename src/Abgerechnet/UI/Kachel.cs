using System.Drawing.Drawing2D;

namespace Abgerechnet.UI;

/// <summary>
/// A tile of the main window: caption, amount and number of invoices on a white card with a colored stripe.
/// </summary>
internal sealed class Kachel : Control
{
    private readonly Color _farbe;
    private string _betrag = string.Empty;
    private string _anzahl = string.Empty;

    public Kachel(string titel, Color farbe)
    {
        Text = titel;
        _farbe = farbe;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        Height = 78;
        Margin = new Padding(0, 0, 12, 0);
        Dock = DockStyle.Fill;
    }

    public void Anzeigen(string betrag, string anzahl)
    {
        _betrag = betrag;
        _anzahl = anzahl;
        AccessibleName = $"{Text}: {betrag}, {anzahl}";
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? SystemColors.Control);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var card = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var path = RoundedRectangle(card, Scale(8)))
        {
            using var fill = new SolidBrush(Color.White);
            using var border = new Pen(UiStyle.GridLine);
            g.FillPath(fill, path);
            g.DrawPath(border, path);
        }
        using (var stripe = new SolidBrush(_farbe))
            g.FillRectangle(stripe, new Rectangle(Scale(1), Scale(12), Scale(4), Height - Scale(24)));

        var left = Scale(16);
        using var captionFont = new Font(Font.FontFamily, 9f);
        using var amountFont = new Font(Font.FontFamily, 15f, FontStyle.Bold);
        var flags = TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;
        TextRenderer.DrawText(g, Text, captionFont, new Rectangle(left, Scale(8), Width / 2, Scale(20)), UiStyle.MutedText, flags);
        TextRenderer.DrawText(g, _anzahl, captionFont, new Rectangle(Width / 2, Scale(8), Width / 2 - Scale(12), Scale(20)), UiStyle.MutedText, flags & ~TextFormatFlags.Left | TextFormatFlags.Right);
        TextRenderer.DrawText(g, _betrag, amountFont, new Rectangle(left, Scale(30), Width - left - Scale(12), Scale(36)), Color.FromArgb(17, 24, 39), flags);
    }

    private int Scale(int value) => (int)Math.Round(value * DeviceDpi / 96f);

    private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
