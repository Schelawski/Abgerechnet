using System.Text;
using System.Text.RegularExpressions;

namespace Abgerechnet.Core.Model;

/// <summary>
/// A customer in <c>kunden.json</c>: just enough for the invoice address (issue #4).
/// </summary>
public sealed partial class Kunde
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Firma { get; set; } = string.Empty;

    /// <summary>Short name used in the PDF file name, e.g. "Mueller".</summary>
    public string Kurzname { get; set; } = string.Empty;

    public string Strasse { get; set; } = string.Empty;

    public string Plz { get; set; } = string.Empty;

    public string Ort { get; set; } = string.Empty;

    public string Land { get; set; } = string.Empty;

    public string Ansprechpartner { get; set; } = string.Empty;

    /// <summary>VAT ID of the customer (needed later for e-invoices, issue #12).</summary>
    public string UstIdNr { get; set; } = string.Empty;

    /// <summary>
    /// The address as lines, as it appears on the invoice: company, contact, street, postcode and city, country.
    /// Empty parts are left out.
    /// </summary>
    public IReadOnlyList<string> Anschrift()
    {
        var plzOrt = $"{Plz.Trim()} {Ort.Trim()}".Trim();
        return new[] { Firma, Ansprechpartner, Strasse, plzOrt, Land }
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();
    }

    /// <summary>
    /// An independent copy. An invoice keeps such a copy of its recipient when the PDF is created, so later
    /// changes to the customer do not change issued invoices.
    /// </summary>
    public Kunde Kopie() => (Kunde)MemberwiseClone();

    // Lookarounds instead of \b, because "e. K." ends with a dot where \b does not match.
    [GeneratedRegex(@"(?<!\w)(GmbH|mbH|AG|UG|KG|OHG|GbR|eG|e\.\s?K|e\.\s?V|SE|Ltd|Inc|Co|haftungsbeschränkt)(?!\w)\.?", RegexOptions.IgnoreCase)]
    private static partial Regex LegalFormRegex();

    /// <summary>
    /// Suggests a short name for the file name from the company: legal forms are dropped, umlauts written out,
    /// at most two words. "Müller &amp; Söhne GmbH" → "Mueller-Soehne".
    /// </summary>
    public static string KurznameVorschlag(string? firma)
    {
        var withoutLegalForm = LegalFormRegex().Replace(firma ?? string.Empty, " ");
        var words = withoutLegalForm
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(ToAsciiWord)
            .Where(word => word.Length > 0)
            .Take(2);
        var name = string.Join('-', words);
        return name.Length <= 24 ? name : name[..24];
    }

    private static string ToAsciiWord(string word)
    {
        var builder = new StringBuilder(word.Length);
        foreach (var c in word)
        {
            switch (c)
            {
                case 'ä': builder.Append("ae"); break;
                case 'ö': builder.Append("oe"); break;
                case 'ü': builder.Append("ue"); break;
                case 'Ä': builder.Append("Ae"); break;
                case 'Ö': builder.Append("Oe"); break;
                case 'Ü': builder.Append("Ue"); break;
                case 'ß': builder.Append("ss"); break;
                default:
                    if (char.IsAsciiLetterOrDigit(c))
                        builder.Append(c);
                    break;
            }
        }
        return builder.ToString();
    }

    internal void Normalize()
    {
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
        Firma ??= string.Empty;
        Kurzname ??= string.Empty;
        Strasse ??= string.Empty;
        Plz ??= string.Empty;
        Ort ??= string.Empty;
        Land ??= string.Empty;
        Ansprechpartner ??= string.Empty;
        UstIdNr ??= string.Empty;
    }
}
