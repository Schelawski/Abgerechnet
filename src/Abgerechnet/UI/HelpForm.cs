using System.Runtime.InteropServices;
using Abgerechnet.Core.Help;
using Abgerechnet.Core.Storage;
using Abgerechnet.Core.Vorlagen;

namespace Abgerechnet.UI;

/// <summary>
/// The help window (as in Wortlaut): topic list on the left, formatted text on the right. The texts come from the
/// embedded Markdown file and are shown in a RichTextBox (no browser component needed). The topic "Vorlage mit KI
/// anpassen" has a button that copies the AI prompt together with the user's standard template.
/// </summary>
internal sealed class HelpForm : Form
{
    private static HelpForm? _open;

    private readonly HelpDocument _document = HelpDocument.Load();
    private readonly ListBox _topicList = new()
    {
        Dock = DockStyle.Fill,
        BorderStyle = BorderStyle.None,
        IntegralHeight = false,
        DrawMode = DrawMode.OwnerDrawFixed,
    };
    private readonly RichTextBox _text = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        BorderStyle = BorderStyle.None,
        BackColor = SystemColors.Window,
        DetectUrls = false,
        ScrollBars = RichTextBoxScrollBars.Vertical,
    };
    private readonly Panel _promptBar = new() { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(24, 8, 12, 10), Visible = false };
    private readonly Button _promptButton = UiStyle.CreateButton(UiText.PromptKopieren);

    public HelpForm()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = UiText.HelpTitle;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1000, 680);
        MinimumSize = new Size(600, 400);
        ShowInTaskbar = false;
        MinimizeBox = false;
        KeyPreview = true;
        BuildLayout();
        ResumeLayout(false);
        PerformLayout();

        foreach (var topic in _document.Topics)
            _topicList.Items.Add(topic);
        _topicList.DrawItem += DrawTopic;
        _topicList.SelectedIndexChanged += (_, _) => ShowSelectedTopic();
        _promptButton.Click += (_, _) => PromptKopieren();
    }

    /// <summary>The open invoice folder; its standard template goes into the AI prompt.</summary>
    public static DataFolder? Folder { get; set; }

    private void BuildLayout()
    {
        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = SystemColors.Window };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // The topic list on a light grey band, the text on white with some room around it.
        var listHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 244, 246), Padding = new Padding(8, 12, 8, 12), Margin = new Padding(0) };
        _topicList.BackColor = listHost.BackColor;
        _topicList.ItemHeight = 30;
        listHost.Controls.Add(_topicList);
        split.Controls.Add(listHost, 0, 0);

        UiStyle.MakePrimary(_promptButton);
        _promptButton.Dock = DockStyle.Left;
        _promptBar.Controls.Add(_promptButton);

        var textHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 12, 12), Margin = new Padding(0) };
        textHost.Controls.Add(_text);
        var right = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
        right.Controls.Add(textHost);
        right.Controls.Add(_promptBar);
        split.Controls.Add(right, 1, 0);

        Controls.Add(split);
    }

    /// <summary>
    /// Opens the help at <paramref name="topicId"/> next to <paramref name="owner"/>. An open help window is reused,
    /// so F1 never piles up windows; it stays open while working in the main window.
    /// </summary>
    public static void Open(Form owner, string? topicId)
    {
        if (_open is not { IsDisposed: false })
        {
            _open = new HelpForm();
            _open.FormClosed += (_, _) => _open = null;
            _open.Show(owner);
        }

        _open.ShowTopic(topicId);
        if (_open.WindowState == FormWindowState.Minimized)
            _open.WindowState = FormWindowState.Normal;
        _open.Activate();
    }

    /// <summary>Shows a topic; an unknown id shows the first one.</summary>
    public void ShowTopic(string? id)
    {
        var topic = _document.Find(id) ?? _document.Topics.FirstOrDefault();
        if (topic is not null)
            _topicList.SelectedItem = topic;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UiStyle.FitToScreen(this);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
            Close();
        base.OnKeyDown(e);
    }

    /// <summary>Copies the AI prompt with the user's standard template (or the built-in one without a folder).</summary>
    private void PromptKopieren()
    {
        try
        {
            var name = Folder?.Einstellungen.Rechnung.Vorlage ?? MitgelieferteVorlagen.Standard;
            var vorlage = Folder is null
                ? new VorlagenQuelle(name, MitgelieferteVorlagen.Lesen(name), null)
                : MitgelieferteVorlagen.Laden(Folder, name);
            Clipboard.SetText(KiPrompt.Text(vorlage.Html));
            MessageBox.Show(this, UiText.PromptKopiert(vorlage.Name), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ExternalException)
        {
            MessageBox.Show(this, UiText.VorlageFehler(ex.Message), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DrawTopic(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || _topicList.Items[e.Index] is not HelpTopic topic)
            return;

        var selected = (e.State & DrawItemState.Selected) != 0;
        using (var background = new SolidBrush(_topicList.BackColor))
            e.Graphics.FillRectangle(background, e.Bounds);
        if (selected)
        {
            var bounds = new RectangleF(e.Bounds.X, e.Bounds.Y + 1, e.Bounds.Width, e.Bounds.Height - 2);
            UiStyle.FillRoundedRectangle(e.Graphics, Color.White, bounds, LogicalToDeviceUnits(6));
        }

        using var font = new Font(_topicList.Font, selected ? FontStyle.Bold : FontStyle.Regular);
        var textBounds = new Rectangle(e.Bounds.X + LogicalToDeviceUnits(10), e.Bounds.Y, e.Bounds.Width - LogicalToDeviceUnits(12), e.Bounds.Height);
        TextRenderer.DrawText(e.Graphics, topic.Title, font, textBounds, selected ? UiStyle.Accent : SystemColors.ControlText,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
    }

    private void ShowSelectedTopic()
    {
        if (_topicList.SelectedItem is not HelpTopic topic)
            return;

        _promptBar.Visible = topic.Id == HelpTopics.Ki;

        _text.SuspendLayout();
        _text.Clear();
        var baseFont = _text.Font;
        using var titleFont = new Font(baseFont.FontFamily, 15f, FontStyle.Bold);
        using var headingFont = new Font(baseFont.FontFamily, 11f, FontStyle.Bold);
        using var bodyFont = new Font(baseFont.FontFamily, 10f);
        using var boldFont = new Font(baseFont.FontFamily, 10f, FontStyle.Bold);
        using var codeFont = new Font("Consolas", 9.5f);
        var quoteBack = Color.FromArgb(243, 244, 246);

        void Append(string text, Font font, Color? color = null, Color? back = null)
        {
            _text.SelectionStart = _text.TextLength;
            _text.SelectionLength = 0;
            _text.SelectionFont = font;
            _text.SelectionColor = color ?? SystemColors.ControlText;
            _text.SelectionBackColor = back ?? _text.BackColor;
            _text.AppendText(text);
        }

        void AppendSpans(IReadOnlyList<HelpSpan> spans, Font normal)
        {
            foreach (var span in spans)
            {
                var font = span.Style switch
                {
                    HelpSpanStyle.Bold => normal.Bold ? normal : boldFont,
                    HelpSpanStyle.Code => codeFont,
                    _ => normal,
                };
                Append(span.Text, font, span.Style == HelpSpanStyle.Code ? Color.FromArgb(17, 94, 89) : null);
            }
        }

        var listIndent = LogicalToDeviceUnits(6);
        var hanging = LogicalToDeviceUnits(22);
        SetParagraph(indent: 0, hanging: 0, spaceAfter: 10);
        Append(topic.Title + "\n", titleFont, UiStyle.Accent);

        foreach (var block in topic.Blocks)
        {
            switch (block)
            {
                case HelpHeading heading:
                    SetParagraph(indent: 0, hanging: 0, spaceAfter: 4, spaceBefore: 8);
                    AppendSpans(heading.Spans, headingFont);
                    break;

                case HelpListItem item:
                    // The marker hangs in front of the text, wrapped lines align with the text.
                    SetParagraph(indent: listIndent, hanging: hanging, spaceAfter: 4);
                    Append(item.Marker + "\t", bodyFont, item.Marker == "•" ? UiStyle.Accent : null);
                    AppendSpans(item.Spans, bodyFont);
                    break;

                case HelpQuote quote:
                    // A grey box, line by line as written, so the prompt can be read and copied exactly.
                    foreach (var line in string.Concat(quote.Spans.Select(s => s.Text)).Split('\n'))
                    {
                        SetParagraph(indent: LogicalToDeviceUnits(12), hanging: 0, spaceAfter: 0);
                        Append(line.Length == 0 ? " " : line, codeFont, null, quoteBack);
                        Append("\n", bodyFont);
                    }
                    SetParagraph(indent: 0, hanging: 0, spaceAfter: 8);
                    continue;

                default:
                    SetParagraph(indent: 0, hanging: 0, spaceAfter: 8);
                    AppendSpans(block.Spans, bodyFont);
                    break;
            }

            Append("\n", bodyFont);
        }

        _text.SelectionStart = 0;
        _text.SelectionLength = 0;
        _text.ScrollToCaret();
        _text.ResumeLayout();

        // indent: first line (the marker); hanging: how much further the wrapped lines start, where a tab
        // after the marker also ends, so the text of all lines is aligned.
        void SetParagraph(int indent, int hanging, int spaceAfter, int spaceBefore = 0)
        {
            _text.SelectionStart = _text.TextLength;
            _text.SelectionIndent = indent;
            _text.SelectionHangingIndent = hanging;
            _text.SelectionTabs = hanging > 0 ? [hanging] : [];
            SetParagraphSpacing(_text, LogicalToDeviceUnits(spaceBefore), LogicalToDeviceUnits(spaceAfter));
        }
    }

    /// <summary>Space before and after the current paragraph (not exposed by RichTextBox, so via EM_SETPARAFORMAT).</summary>
    private static void SetParagraphSpacing(RichTextBox box, int before, int after)
    {
        // Twips: 1/1440 inch; the values are device pixels at the box's DPI.
        var twipsPerPixel = 1440f / box.DeviceDpi;
        var format = new ParaFormat2
        {
            cbSize = Marshal.SizeOf<ParaFormat2>(),
            dwMask = PFM_SPACEBEFORE | PFM_SPACEAFTER,
            rgxTabs = new int[32],
            dySpaceBefore = (int)(before * twipsPerPixel),
            dySpaceAfter = (int)(after * twipsPerPixel),
        };
        SendMessage(box.Handle, EM_SETPARAFORMAT, IntPtr.Zero, ref format);
    }

    // ----- RichEdit interop -----

    private const int EM_SETPARAFORMAT = 0x0400 + 71;
    private const int PFM_SPACEBEFORE = 0x40;
    private const int PFM_SPACEAFTER = 0x80;

    [StructLayout(LayoutKind.Sequential)]
    private struct ParaFormat2
    {
        public int cbSize;
        public int dwMask;
        public short wNumbering;
        public short wEffects;
        public int dxStartIndent;
        public int dxRightIndent;
        public int dxOffset;
        public short wAlignment;
        public short cTabCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] rgxTabs;
        public int dySpaceBefore;
        public int dySpaceAfter;
        public int dyLineSpacing;
        public short sStyle;
        public byte bLineSpacingRule;
        public byte bOutlineLevel;
        public short wShadingWeight;
        public short wShadingStyle;
        public short wNumberingStart;
        public short wNumberingStyle;
        public short wNumberingTab;
        public short wBorderSpace;
        public short wBorderWidth;
        public short wBorders;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref ParaFormat2 lParam);
}
