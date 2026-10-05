using System.Globalization;

namespace Abgerechnet.Core.Model;

/// <summary>
/// Invoice numbers: the next free number and the check for duplicates. Numbers are text, so schemes like
/// "111412" or "RE-2026-007" both work; the last group of digits is counted up and keeps its width.
/// </summary>
public static class Nummernkreis
{
    /// <summary>Number of the very first invoice when nothing else is set.</summary>
    public static string Startnummer(int jahr) => $"{jahr}-001";

    /// <summary>
    /// The next number: the highest existing number plus one, or the number from the settings when that is
    /// higher (e.g. to start a new scheme). Never a number that is already used.
    /// </summary>
    public static string Naechste(Einstellungen einstellungen, IReadOnlyCollection<Rechnung> rechnungen, int jahr)
    {
        var kandidaten = new List<string>();
        var hoechste = rechnungen
            .Select(r => r.Nummer.Trim())
            .Where(n => n.Length > 0)
            .Max(Rechnungsnummer.Vergleich);
        if (hoechste is not null)
            kandidaten.Add(Erhoehen(hoechste));

        var vorgabe = einstellungen.Rechnung.NaechsteNummer.Trim();
        if (vorgabe.Length > 0)
            kandidaten.Add(vorgabe);

        var nummer = kandidaten.Count == 0 ? Startnummer(jahr) : kandidaten.Max(Rechnungsnummer.Vergleich)!;
        while (IstVergeben(nummer, rechnungen, ausser: null))
            nummer = Erhoehen(nummer);
        return nummer;
    }

    /// <summary>"111412" → "111413", "RE-2026-009" → "RE-2026-010", "A99" → "A100", "RE" → "RE1".</summary>
    public static string Erhoehen(string nummer)
    {
        nummer = nummer.Trim();
        var ende = nummer.Length;
        while (ende > 0 && !char.IsAsciiDigit(nummer[ende - 1]))
            ende--;
        if (ende == 0)
            return nummer + "1";

        var start = ende;
        while (start > 0 && char.IsAsciiDigit(nummer[start - 1]))
            start--;

        var ziffern = nummer[start..ende];
        var erhoeht = (System.Numerics.BigInteger.Parse(ziffern, CultureInfo.InvariantCulture) + 1)
            .ToString(CultureInfo.InvariantCulture)
            .PadLeft(ziffern.Length, '0');
        return nummer[..start] + erhoeht + nummer[ende..];
    }

    /// <summary>True when another invoice already has this number (ignoring case and surrounding spaces).</summary>
    /// <param name="ausser">The invoice being edited, which may keep its own number.</param>
    public static bool IstVergeben(string nummer, IEnumerable<Rechnung> rechnungen, Guid? ausser) =>
        rechnungen.Any(r => r.Id != ausser && string.Equals(r.Nummer.Trim(), nummer.Trim(), StringComparison.OrdinalIgnoreCase));
}

/// <summary>Service periods as text.</summary>
public static class Leistungszeitraum
{
    private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>The month before <paramref name="heute"/>, e.g. "September 2026" in October 2026.</summary>
    public static string Vormonat(DateOnly heute) => Monat(heute.AddMonths(-1));

    /// <summary>"September 2026".</summary>
    public static string Monat(DateOnly datum) => datum.ToString("MMMM yyyy", German);
}
