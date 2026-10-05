using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Abgerechnet.Core.Model;

/// <summary>
/// Builds the PDF file name from the pattern in the settings, e.g. <c>{nummer}_{kunde_kurzname}_{datum}.pdf</c>
/// → <c>111412_Nordlicht_2026-10-01.pdf</c>.
/// </summary>
public static partial class PdfDateiname
{
    public const string DefaultMuster = "{nummer}_{kunde_kurzname}_{datum}.pdf";

    /// <summary>The placeholders a pattern may contain.</summary>
    public static readonly IReadOnlyList<string> Platzhalter =
        ["{nummer}", "{kunde_kurzname}", "{kunde}", "{datum}", "{jahr}", "{monat}"];

    [GeneratedRegex(@"\{[^{}]*\}")]
    private static partial Regex PlaceholderRegex();

    /// <summary>Placeholders in the pattern that are not known, e.g. "{kundenname}".</summary>
    public static IReadOnlyList<string> UnbekanntePlatzhalter(string muster) =>
        PlaceholderRegex().Matches(muster ?? string.Empty)
            .Select(m => m.Value)
            .Where(p => !Platzhalter.Contains(p, StringComparer.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

    /// <summary>
    /// Fills the pattern. Characters that are not allowed in file names are replaced by "_", and ".pdf" is
    /// appended when missing. A customer without a short name uses the company name.
    /// </summary>
    public static string Erzeugen(string muster, string nummer, DateOnly datum, Kunde? kunde)
    {
        if (string.IsNullOrWhiteSpace(muster))
            muster = DefaultMuster;

        var firma = kunde?.Firma.Trim() ?? string.Empty;
        var kurzname = kunde is null ? string.Empty : (string.IsNullOrWhiteSpace(kunde.Kurzname) ? firma : kunde.Kurzname.Trim());

        var name = PlaceholderRegex().Replace(muster, match => match.Value.ToLowerInvariant() switch
        {
            "{nummer}" => nummer.Trim(),
            "{kunde_kurzname}" => kurzname,
            "{kunde}" => firma,
            "{datum}" => datum.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            "{jahr}" => datum.ToString("yyyy", CultureInfo.InvariantCulture),
            "{monat}" => datum.ToString("MM", CultureInfo.InvariantCulture),
            _ => match.Value,
        });

        name = MakeValidFileName(name);
        if (!name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            name += ".pdf";
        return name;
    }

    private static string MakeValidFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(name.Length);
        foreach (var c in name)
            builder.Append(invalid.Contains(c) || c is '{' or '}' ? '_' : c);

        // Empty parts leave "__" or a leading "_" (e.g. no customer yet); tidy that up.
        var result = Regex.Replace(builder.ToString(), "_{2,}", "_").Trim(' ', '.', '_');
        return result.Length == 0 ? "Rechnung" : result;
    }
}
