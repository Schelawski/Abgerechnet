using System.Reflection;
using System.Text;
using Abgerechnet.Core.Vorlagen;

namespace Abgerechnet.Core.Help;

/// <summary>Ids of the help topics, in the order of the help file. F1 in a window opens its topic.</summary>
public static class HelpTopics
{
    public const string About = "about";
    public const string FirstSteps = "start";
    public const string Folder = "ordner";
    public const string MeineDaten = "meinedaten";
    public const string Kunden = "kunden";
    public const string Rechnungen = "rechnungen";
    public const string Pdf = "pdf";
    public const string Vorlagen = "vorlagen";
    public const string VorlageAnpassen = "anpassen";
    public const string Ki = "ki";
    public const string Problems = "probleme";
    public const string Steuern = "steuern";
    public const string Licenses = "lizenzen";

    public static IReadOnlyList<string> All { get; } =
        [About, FirstSteps, Folder, MeineDaten, Kunden, Rechnungen, Pdf, Vorlagen, VorlageAnpassen, Ki, Problems, Steuern, Licenses];
}

/// <summary>Formatting of a piece of text.</summary>
public enum HelpSpanStyle
{
    Normal,

    /// <summary><c>**bold**</c></summary>
    Bold,

    /// <summary><c>`code`</c>, e.g. a folder path or a placeholder.</summary>
    Code,
}

public readonly record struct HelpSpan(string Text, HelpSpanStyle Style);

public abstract record HelpBlock(IReadOnlyList<HelpSpan> Spans);

/// <summary>A paragraph of running text.</summary>
public sealed record HelpParagraph(IReadOnlyList<HelpSpan> Spans) : HelpBlock(Spans);

/// <summary><c>## Subheading</c> inside a topic.</summary>
public sealed record HelpHeading(IReadOnlyList<HelpSpan> Spans) : HelpBlock(Spans);

/// <summary>A list item: <c>- item</c> (Marker "•") or <c>1. item</c> (Marker "1.").</summary>
public sealed record HelpListItem(string Marker, IReadOnlyList<HelpSpan> Spans) : HelpBlock(Spans);

/// <summary>
/// <c>&gt; text</c>: a highlighted box, e.g. the AI prompt. Its lines are kept as they are (no joining), so the text
/// can be copied exactly.
/// </summary>
public sealed record HelpQuote(IReadOnlyList<HelpSpan> Spans) : HelpBlock(Spans);

public sealed record HelpTopic(string Id, string Title, IReadOnlyList<HelpBlock> Blocks);

/// <summary>
/// The help texts. They are written in a small subset of Markdown and embedded into Abgerechnet.exe
/// (<c>Help/help.de.md</c>), so they can be read and edited like plain text:
/// <list type="bullet">
/// <item><c># id | Title</c> starts a topic (id from <see cref="HelpTopics"/>).</item>
/// <item><c>## Text</c> is a subheading; <c>- </c> and <c>1. </c> start list items; <c>&gt; </c> lines form a box.</item>
/// <item>Lines are joined into paragraphs until an empty line; <c>**bold**</c> and <c>`code`</c> inside the text.</item>
/// <item>A line <c>[[platzhalter]]</c> becomes the list of all template placeholders, <c>[[ki-prompt]]</c> the AI
/// prompt – both generated from the code, so the help always matches the program.</item>
/// </list>
/// </summary>
public sealed class HelpDocument
{
    public const string PlatzhalterMarker = "[[platzhalter]]";
    public const string KiPromptMarker = "[[ki-prompt]]";

    private HelpDocument(IReadOnlyList<HelpTopic> topics) => Topics = topics;

    public IReadOnlyList<HelpTopic> Topics { get; }

    public HelpTopic? Find(string? id) =>
        Topics.FirstOrDefault(topic => string.Equals(topic.Id, id, StringComparison.OrdinalIgnoreCase));

    /// <summary>Loads the embedded help with the generated parts filled in.</summary>
    /// <exception cref="InvalidOperationException">The resource is missing (a build error).</exception>
    public static HelpDocument Load() => Parse(ErsetzeMarker(LoadMarkdown()));

    /// <summary>The embedded help file as written.</summary>
    internal static string LoadMarkdown()
    {
        const string name = "Abgerechnet.Help.help.de.md";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Help resource {name} not found.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>Replaces the marker lines with the placeholder list and the AI prompt.</summary>
    internal static string ErsetzeMarker(string markdown)
    {
        var platzhalter = new StringBuilder();
        foreach (var gruppe in Platzhalter.Alle.GroupBy(p => p.Gruppe))
        {
            platzhalter.Append("## ").AppendLine(gruppe.Key);
            foreach (var info in gruppe)
                platzhalter.Append("- `").Append(info.Schreibweise).Append("` – ").AppendLine(info.Beschreibung);
            platzhalter.AppendLine();
        }

        var prompt = string.Join('\n', KiPrompt.Text(KiPrompt.VorlagePlatzhalter).ReplaceLineEndings("\n").Split('\n').Select(l => "> " + l));

        return markdown.ReplaceLineEndings("\n")
            .Replace(PlatzhalterMarker, platzhalter.ToString().TrimEnd(), StringComparison.Ordinal)
            .Replace(KiPromptMarker, prompt, StringComparison.Ordinal);
    }

    public static HelpDocument Parse(string markdown)
    {
        var topics = new List<HelpTopic>();
        string? id = null, title = null;
        var blocks = new List<HelpBlock>();
        var pending = new StringBuilder();
        string? pendingMarker = null; // null: paragraph, otherwise the list marker
        var quote = new List<string>();

        void FlushQuote()
        {
            if (quote.Count == 0)
                return;
            blocks.Add(new HelpQuote([new HelpSpan(string.Join('\n', quote), HelpSpanStyle.Normal)]));
            quote.Clear();
        }

        void FlushBlock()
        {
            FlushQuote();
            if (pending.Length == 0)
                return;

            var spans = ParseInline(pending.ToString());
            blocks.Add(pendingMarker is null ? new HelpParagraph(spans) : new HelpListItem(pendingMarker, spans));
            pending.Clear();
            pendingMarker = null;
        }

        void FlushTopic()
        {
            FlushBlock();
            if (id is not null)
                topics.Add(new HelpTopic(id, title!, blocks.ToList()));
            blocks.Clear();
        }

        foreach (var rawLine in markdown.ReplaceLineEndings("\n").Split('\n'))
        {
            var line = rawLine.TrimEnd();
            var trimmed = line.TrimStart();

            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                FlushTopic();
                var header = line[2..];
                var separator = header.IndexOf('|');
                id = separator < 0 ? header.Trim() : header[..separator].Trim();
                title = separator < 0 ? id : header[(separator + 1)..].Trim();
                continue;
            }

            if (id is null)
                continue; // text before the first topic (e.g. an editor's note)

            // Quote lines are taken literally, including empty ones written as ">".
            if (trimmed == ">" || trimmed.StartsWith("> ", StringComparison.Ordinal))
            {
                if (quote.Count == 0)
                    FlushBlock();
                quote.Add(trimmed.Length > 2 ? trimmed[2..] : string.Empty);
                continue;
            }
            FlushQuote();

            if (trimmed.Length == 0)
            {
                FlushBlock();
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                FlushBlock();
                blocks.Add(new HelpHeading(ParseInline(line[3..].Trim())));
                continue;
            }

            if (ListMarker(trimmed) is { } marker)
            {
                FlushBlock();
                pendingMarker = marker.Marker;
                pending.Append(trimmed[marker.Length..].Trim());
                continue;
            }

            // Continuation of the current paragraph or list item.
            if (pending.Length > 0)
                pending.Append(' ');
            pending.Append(trimmed);
        }

        FlushTopic();
        return new HelpDocument(topics);
    }

    /// <summary>"- text" → "•", "12. text" → "12.".</summary>
    private static (string Marker, int Length)? ListMarker(string line)
    {
        if (line.StartsWith("- ", StringComparison.Ordinal))
            return ("•", 2);

        var digits = 0;
        while (digits < line.Length && char.IsAsciiDigit(line[digits]))
            digits++;
        return digits > 0 && digits + 1 < line.Length && line[digits] == '.' && line[digits + 1] == ' '
            ? (line[..(digits + 1)], digits + 2)
            : null;
    }

    /// <summary>Splits <c>**bold**</c> and <c>`code`</c> from normal text. An unclosed marker is kept as text.</summary>
    public static IReadOnlyList<HelpSpan> ParseInline(string text)
    {
        var spans = new List<HelpSpan>();
        var normal = new StringBuilder();
        var i = 0;

        void FlushNormal()
        {
            if (normal.Length > 0)
                spans.Add(new HelpSpan(normal.ToString(), HelpSpanStyle.Normal));
            normal.Clear();
        }

        while (i < text.Length)
        {
            if (text.AsSpan(i).StartsWith("**") && text.IndexOf("**", i + 2, StringComparison.Ordinal) is var end and > 0)
            {
                FlushNormal();
                spans.Add(new HelpSpan(text[(i + 2)..end], HelpSpanStyle.Bold));
                i = end + 2;
            }
            else if (text[i] == '`' && text.IndexOf('`', i + 1) is var close and > 0)
            {
                FlushNormal();
                spans.Add(new HelpSpan(text[(i + 1)..close], HelpSpanStyle.Code));
                i = close + 1;
            }
            else
            {
                normal.Append(text[i]);
                i++;
            }
        }

        FlushNormal();
        return spans;
    }
}
