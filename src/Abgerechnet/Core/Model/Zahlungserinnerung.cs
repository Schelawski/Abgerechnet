namespace Abgerechnet.Core.Model;

/// <summary>What the reminder bar above the invoice list reports; empty lists mean no bar.</summary>
/// <param name="Ueberfaellig">Open invoices older than the payment term, oldest first.</param>
/// <param name="VergesseneEntwuerfe">Drafts older than <see cref="Zahlungserinnerung.EntwurfTage"/> days, oldest first.</param>
public sealed record Erinnerung(IReadOnlyList<Rechnung> Ueberfaellig, IReadOnlyList<Rechnung> VergesseneEntwuerfe)
{
    public bool IstLeer => Ueberfaellig.Count == 0 && VergesseneEntwuerfe.Count == 0;
}

/// <summary>
/// Reminds of open invoices that are probably paid by now but still marked "Offen", and of drafts whose PDF was
/// never created (issue #17). Deliberately simple: no dunning, only a hint.
/// </summary>
public static class Zahlungserinnerung
{
    /// <summary>A draft is "forgotten" when it is older than this many days.</summary>
    public const int EntwurfTage = 7;

    /// <summary>
    /// Days after the invoice date at which an open invoice counts as overdue: the payment term, or 30 days when no
    /// payment term is set (the invoice should still not be forgotten).
    /// </summary>
    public static int Tage(Einstellungen einstellungen) =>
        einstellungen.Rechnung.ZahlungszielTage > 0
            ? einstellungen.Rechnung.ZahlungszielTage
            : RechnungsEinstellungen.DefaultZahlungszielTage;

    /// <summary>
    /// True for an open invoice whose due day has passed. On the due day itself the invoice is not yet overdue.
    /// </summary>
    public static bool IstUeberfaellig(Rechnung rechnung, Einstellungen einstellungen, DateOnly heute) =>
        rechnung.Status == RechnungsStatus.Offen && heute > rechnung.Datum.AddDays(Tage(einstellungen));

    /// <summary>True for a draft older than <see cref="EntwurfTage"/> days (the PDF was probably forgotten).</summary>
    public static bool IstVergessenerEntwurf(Rechnung rechnung, DateOnly heute) =>
        rechnung.Status == RechnungsStatus.Entwurf && heute > rechnung.Datum.AddDays(EntwurfTage);

    /// <summary>Days since the invoice date, e.g. "seit 42 Tagen offen".</summary>
    public static int TageSeit(Rechnung rechnung, DateOnly heute) => heute.DayNumber - rechnung.Datum.DayNumber;

    public static Erinnerung Pruefen(IEnumerable<Rechnung> rechnungen, Einstellungen einstellungen, DateOnly heute)
    {
        var alle = rechnungen.OrderBy(r => r.Datum).ThenBy(r => r.Nummer, Rechnungsnummer.Vergleich).ToList();
        return new Erinnerung(
            alle.Where(r => IstUeberfaellig(r, einstellungen, heute)).ToList(),
            alle.Where(r => IstVergessenerEntwurf(r, heute)).ToList());
    }
}
