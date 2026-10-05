using System.Text.Json.Serialization;

namespace Abgerechnet.Core.Model;

/// <summary>
/// Lifecycle of an invoice. Stored in lower case ("entwurf", "offen", …).
/// </summary>
public enum RechnungsStatus
{
    /// <summary>Being written; may still be deleted.</summary>
    Entwurf,

    /// <summary>PDF created and sent; waiting for payment.</summary>
    Offen,

    Bezahlt,

    /// <summary>Cancelled. Issued invoices are cancelled instead of deleted, so the number sequence has no gaps.</summary>
    Storniert,
}

/// <summary>
/// One invoice in <c>rechnungen.json</c>. The domain terms are German because they appear as they are in the
/// JSON files and in the template placeholders.
/// </summary>
public sealed class Rechnung
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Invoice number, e.g. "111412". Text, so leading zeros and prefixes are possible.</summary>
    public string Nummer { get; set; } = string.Empty;

    /// <summary>Invoice date.</summary>
    public DateOnly Datum { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>Service period as text, e.g. "September 2026".</summary>
    public string Zeitraum { get; set; } = string.Empty;

    /// <summary>Project or subject line.</summary>
    public string Projekt { get; set; } = string.Empty;

    public Guid? KundeId { get; set; }

    /// <summary>
    /// Copy of the customer's address taken when the PDF is created, so later changes to the customer do not
    /// change issued invoices. <c>null</c> for drafts.
    /// </summary>
    public Kunde? Empfaenger { get; set; }

    public RechnungsStatus Status { get; set; } = RechnungsStatus.Entwurf;

    /// <summary>Date the payment arrived; set together with <see cref="RechnungsStatus.Bezahlt"/>.</summary>
    public DateOnly? BezahltAm { get; set; }

    /// <summary>File name of the created PDF, relative to the <c>PDF</c> folder.</summary>
    public string? PdfDatei { get; set; }

    /// <summary>
    /// VAT rate in percent for this invoice. Taken from the settings when the invoice is created, so a later change
    /// of the settings does not change existing invoices.
    /// </summary>
    public decimal Umsatzsteuersatz { get; set; } = RechnungsEinstellungen.DefaultUmsatzsteuersatz;

    /// <summary>Invoice of a small business (§ 19 UStG): no VAT. Taken from the settings like the rate.</summary>
    public bool Kleinunternehmer { get; set; }

    public List<Position> Positionen { get; set; } = [];

    /// <summary>
    /// A new draft with the next number, today's date, last month as period, the tax rule from the settings and one
    /// line item with the default values.
    /// </summary>
    public static Rechnung Neu(Einstellungen einstellungen, IReadOnlyCollection<Rechnung> vorhandene, DateOnly heute, Guid? kundeId = null) => new()
    {
        Nummer = Nummernkreis.Naechste(einstellungen, vorhandene, heute.Year),
        Datum = heute,
        Zeitraum = Leistungszeitraum.Vormonat(heute),
        KundeId = kundeId,
        Umsatzsteuersatz = einstellungen.Rechnung.Umsatzsteuersatz,
        Kleinunternehmer = einstellungen.Rechnung.Kleinunternehmer,
        Positionen = [Position.Neu(einstellungen.NeuePosition)],
    };

    /// <summary>
    /// "Als neue Rechnung kopieren" – the usual month-end: same customer, project and line items, but the next
    /// number, today's date, last month as period and status draft. Line items lose their own period (it belonged
    /// to the old month); the tax rule comes from the current settings.
    /// </summary>
    public Rechnung AlsNeueRechnung(Einstellungen einstellungen, IReadOnlyCollection<Rechnung> vorhandene, DateOnly heute)
    {
        var kopie = Neu(einstellungen, vorhandene, heute, KundeId);
        kopie.Projekt = Projekt;
        kopie.Positionen = Positionen.Select(p => new Position
        {
            Beschreibung = p.Beschreibung,
            Detail = p.Detail,
            Menge = p.Menge,
            Einheit = p.Einheit,
            Einzelpreis = p.Einzelpreis,
        }).ToList();
        if (kopie.Positionen.Count == 0)
            kopie.Positionen.Add(Position.Neu(einstellungen.NeuePosition));
        return kopie;
    }

    /// <summary>Only drafts may be deleted; issued invoices are cancelled, so the number sequence has no gaps.</summary>
    [JsonIgnore]
    public bool KannGeloeschtWerden => Status == RechnungsStatus.Entwurf;

    /// <summary>
    /// Status the user may choose by hand. A draft becomes open only by creating its PDF (issue #8); an issued
    /// invoice can be paid, cancelled, or set back to open.
    /// </summary>
    public IReadOnlyList<RechnungsStatus> ErlaubteStatuswechsel() => Status switch
    {
        RechnungsStatus.Entwurf => [],
        RechnungsStatus.Offen => [RechnungsStatus.Bezahlt, RechnungsStatus.Storniert],
        RechnungsStatus.Bezahlt => [RechnungsStatus.Offen, RechnungsStatus.Storniert],
        RechnungsStatus.Storniert => [RechnungsStatus.Offen],
        _ => [],
    };

    /// <summary>
    /// Changes the status by hand. "Bezahlt" records the payment date; leaving "Bezahlt" clears it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The change is not allowed (see <see cref="ErlaubteStatuswechsel"/>).</exception>
    public void StatusAendern(RechnungsStatus neu, DateOnly bezahltAm)
    {
        if (!ErlaubteStatuswechsel().Contains(neu))
            throw new InvalidOperationException($"Status change {Status} → {neu} is not allowed.");

        Status = neu;
        BezahltAm = neu == RechnungsStatus.Bezahlt ? bezahltAm : null;
    }

    /// <summary>
    /// The recipient to show or print: the copy taken when the PDF was created, otherwise the current customer.
    /// </summary>
    public Kunde? EmpfaengerAus(KundenDatei kunden) => Empfaenger ?? kunden.Finden(KundeId);

    /// <summary>
    /// Freezes the recipient's address in the invoice (when the PDF is created), so later changes to the customer
    /// do not change this invoice.
    /// </summary>
    public void EmpfaengerFestschreiben(Kunde kunde)
    {
        ArgumentNullException.ThrowIfNull(kunde);
        KundeId = kunde.Id;
        Empfaenger = kunde.Kopie();
    }

    /// <summary>Replaces missing values (e.g. from an older or hand-edited file) with defaults.</summary>
    internal void Normalize()
    {
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
        Nummer ??= string.Empty;
        Zeitraum ??= string.Empty;
        Projekt ??= string.Empty;
        if (!Enum.IsDefined(Status))
            Status = RechnungsStatus.Entwurf;
        if (Status != RechnungsStatus.Bezahlt)
            BezahltAm = null;
        if (Umsatzsteuersatz is < 0 or > 100)
            Umsatzsteuersatz = RechnungsEinstellungen.DefaultUmsatzsteuersatz;
        Empfaenger?.Normalize();
        Positionen = (Positionen ?? []).Where(p => p is not null).ToList();
        foreach (var position in Positionen)
            position.Normalize();
    }
}

/// <summary>One line item of an invoice.</summary>
public sealed class Position
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Own service period; empty means the period of the invoice.</summary>
    public string Zeitraum { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    /// <summary>Optional second line with details.</summary>
    public string Detail { get; set; } = string.Empty;

    public decimal Menge { get; set; }

    /// <summary>Unit such as "Std.", "Tage" or "pauschal".</summary>
    public string Einheit { get; set; } = string.Empty;

    /// <summary>Net price per unit.</summary>
    public decimal Einzelpreis { get; set; }

    /// <summary>A new line item with the default values from the settings (quantity 0).</summary>
    public static Position Neu(PositionsVorgaben vorgaben) => new()
    {
        Beschreibung = vorgaben.Beschreibung,
        Einheit = vorgaben.Einheit,
        Einzelpreis = vorgaben.Einzelpreis,
    };

    internal void Normalize()
    {
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
        Zeitraum ??= string.Empty;
        Beschreibung ??= string.Empty;
        Detail ??= string.Empty;
        Einheit ??= string.Empty;
    }
}
