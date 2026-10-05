using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.Core.Vorlagen;

/// <summary>
/// An invented invoice for the template preview in the welcome wizard: the user's own sender data (as far as it was
/// entered), an invented customer and two line items. Nothing of it is saved.
/// </summary>
public static class Beispielrechnung
{
    public static RechnungsDaten Erzeugen(Einstellungen einstellungen, DateOnly heute)
    {
        var e = JsonDataFile.Clone(einstellungen);

        // Skipped "Meine Daten": an obviously invented sender, so the preview is still complete.
        if (!e.Absender.IstVollstaendig)
        {
            e.Absender = new Absender
            {
                Firma = "Ihre Firma",
                Name = "Ihr Name",
                Strasse = "Musterstraße 1",
                Plz = "12345",
                Ort = "Musterstadt",
                Email = "info@example.com",
                Steuernummer = "12/345/67890",
            };
        }
        if (string.IsNullOrWhiteSpace(e.Bank.Iban))
            e.Bank = new Bankverbindung { Kontoinhaber = e.Absender.Anzeigename, Bank = "Musterbank", Iban = "DE00 1234 5678 9012 3456 78" };

        var kunde = new Kunde
        {
            Firma = "Nordlicht GmbH",
            Kurzname = "Nordlicht",
            Ansprechpartner = "Lena Sommer",
            Strasse = "Hafenweg 12",
            Plz = "24103",
            Ort = "Kiel",
        };

        var rechnung = Rechnung.Neu(e, [], heute, kunde.Id);
        rechnung.Projekt = "Website-Relaunch";
        var vorgabe = e.NeuePosition;
        rechnung.Positionen =
        [
            new Position
            {
                Zeitraum = rechnung.Zeitraum,
                Beschreibung = string.IsNullOrWhiteSpace(vorgabe.Beschreibung) ? "Konzeption und Umsetzung" : vorgabe.Beschreibung,
                Detail = "Neue Startseite und Kontaktformular",
                Menge = 32,
                Einheit = string.IsNullOrWhiteSpace(vorgabe.Einheit) ? "Std." : vorgabe.Einheit,
                Einzelpreis = vorgabe.Einzelpreis > 0 ? vorgabe.Einzelpreis : 85m,
            },
            new Position
            {
                Beschreibung = "Abstimmungstermin vor Ort",
                Menge = 1,
                Einheit = "pauschal",
                Einzelpreis = 150m,
            },
        ];

        return new RechnungsDaten(e, rechnung, kunde);
    }
}
