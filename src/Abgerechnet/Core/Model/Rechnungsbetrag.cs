namespace Abgerechnet.Core.Model;

/// <summary>
/// The amounts of an invoice. Each line item and the VAT are rounded commercially to cents (0.005 rounds up), as
/// they are printed on the invoice; the totals are sums of the printed values.
/// </summary>
public readonly record struct Rechnungsbetrag(decimal Netto, decimal Umsatzsteuer, decimal Brutto)
{
    public static Rechnungsbetrag Berechnen(Rechnung rechnung)
    {
        ArgumentNullException.ThrowIfNull(rechnung);
        var netto = rechnung.Positionen.Sum(Positionsbetrag);
        var steuer = rechnung.Kleinunternehmer ? 0m : Runden(netto * rechnung.Umsatzsteuersatz / 100m);
        return new Rechnungsbetrag(netto, steuer, netto + steuer);
    }

    /// <summary>Quantity × unit price, rounded to cents.</summary>
    public static decimal Positionsbetrag(Position position) => Runden(position.Menge * position.Einzelpreis);

    /// <summary>Commercial rounding to two decimals.</summary>
    public static decimal Runden(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
