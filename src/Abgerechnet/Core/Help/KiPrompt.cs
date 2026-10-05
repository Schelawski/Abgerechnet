namespace Abgerechnet.Core.Help;

/// <summary>
/// The prompt for adapting an invoice template with an AI chat (help topic "Vorlage mit KI anpassen"). It tells the
/// AI to change only the look and to keep everything Abgerechnet relies on.
/// </summary>
public static class KiPrompt
{
    /// <summary>Stands in for the template in the help text; the copy button inserts the real template instead.</summary>
    public const string VorlagePlatzhalter = "[HIER DEN INHALT DER VORLAGE EINFÜGEN]";

    public const string WunschPlatzhalter = "[HIER IHREN WUNSCH EINTRAGEN]";

    /// <summary>The prompt with the template (or <see cref="VorlagePlatzhalter"/>) at the end.</summary>
    public static string Text(string vorlage) =>
        $"""
        Hier ist eine Rechnungsvorlage für das Programm „Abgerechnet“ (eine HTML-Datei, aus der ein A4-PDF erzeugt wird).
        Bitte ändere nur das Aussehen. Mein Wunsch: {WunschPlatzhalter}
        (Beispiele: „Akzentfarbe Dunkelgrün“, „Logo links oben statt rechts“, „größere Schrift“, „ohne Zebrastreifen“.)

        Halte dich dabei an diese Regeln:
        1. Lass alle Platzhalter in doppelten geschweiften Klammern, z. B. {"{{rechnung_nummer}}"}, genau so stehen – nicht übersetzen, nicht umbenennen, nicht löschen.
        2. Lass die CSS-Klassen der fertigen Tabellen unverändert (positionen, pos-…, summen, netto, ust, brutto) und ändere ihr Aussehen nur per CSS.
        3. Behalte das A4-Format (@page), die Tabelle „seitenrahmen“ mit dem Platz für die Fußzeile und die Klasse „wenn-gefuellt“.
        4. Lade nichts aus dem Internet: keine Webschriften, keine Bilder oder Skripte von anderen Seiten.
        5. Gib mir die vollständige HTML-Datei zurück, damit ich sie direkt speichern kann.

        Hier ist die Vorlage:

        {vorlage}
        """;
}
