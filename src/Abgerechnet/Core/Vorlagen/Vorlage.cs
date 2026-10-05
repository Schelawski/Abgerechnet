using System.Text;
using System.Text.RegularExpressions;

namespace Abgerechnet.Core.Vorlagen;

/// <summary>The filled-in template and the placeholders it contained that are not known.</summary>
/// <param name="UnbekanntePlatzhalter">Unknown placeholders as written, e.g. "{{kundenname}}"; they stay in the HTML.</param>
public sealed record VorlagenErgebnis(string Html, IReadOnlyList<string> UnbekanntePlatzhalter);

/// <summary>
/// Fills an HTML invoice template (issue #7): every <c>{{name}}</c> is replaced by its value (see
/// <see cref="Platzhalter"/>). Spaces inside the braces and upper case are allowed. HTML comments are left as they
/// are, so a template can document its placeholders in comments.
/// </summary>
public static partial class Vorlage
{
    [GeneratedRegex(@"\{\{\s*([A-Za-z0-9_]+)\s*\}\}")]
    private static partial Regex PlaceholderRegex();

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex CommentRegex();

    [GeneratedRegex(@"<head(\s[^>]*)?>", RegexOptions.IgnoreCase)]
    private static partial Regex HeadRegex();

    [GeneratedRegex(@"<base\s", RegexOptions.IgnoreCase)]
    private static partial Regex BaseRegex();

    /// <param name="vorlage">The HTML of the template.</param>
    /// <param name="daten">Settings, invoice and recipient.</param>
    /// <param name="vorlagenOrdner">
    /// Folder of the template. Relative paths in the template (logo, own CSS file) are resolved against it with a
    /// <c>&lt;base&gt;</c> element, unless the template sets its own.
    /// </param>
    public static VorlagenErgebnis Ausfuellen(string vorlage, RechnungsDaten daten, string? vorlagenOrdner = null)
    {
        ArgumentNullException.ThrowIfNull(vorlage);
        var werte = Platzhalter.Werte(daten);
        var unbekannt = new List<string>();

        // Replace outside of HTML comments only.
        var result = new StringBuilder(vorlage.Length + 4096);
        var position = 0;
        foreach (Match comment in CommentRegex().Matches(vorlage))
        {
            result.Append(Ersetzen(vorlage[position..comment.Index], werte, unbekannt));
            result.Append(comment.Value);
            position = comment.Index + comment.Length;
        }
        result.Append(Ersetzen(vorlage[position..], werte, unbekannt));

        var html = result.ToString();
        if (vorlagenOrdner is not null)
            html = BasisSetzen(html, vorlagenOrdner);
        return new VorlagenErgebnis(html, unbekannt.Distinct(StringComparer.OrdinalIgnoreCase).ToList());
    }

    private static string Ersetzen(string text, IReadOnlyDictionary<string, string> werte, List<string> unbekannt) =>
        PlaceholderRegex().Replace(text, match =>
        {
            if (werte.TryGetValue(match.Groups[1].Value, out var wert))
                return wert;
            unbekannt.Add(match.Value);
            return match.Value;
        });

    /// <summary>Inserts <c>&lt;base href="file:///…/Vorlagen/"&gt;</c> at the start of the head.</summary>
    internal static string BasisSetzen(string html, string ordner)
    {
        if (BaseRegex().IsMatch(html))
            return html;

        var pfad = Path.GetFullPath(ordner);
        if (!pfad.EndsWith(Path.DirectorySeparatorChar))
            pfad += Path.DirectorySeparatorChar;
        var basis = $"<base href=\"{new Uri(pfad).AbsoluteUri}\">";

        var head = HeadRegex().Match(html);
        return head.Success
            ? html.Insert(head.Index + head.Length, basis)
            : basis + html;
    }
}
