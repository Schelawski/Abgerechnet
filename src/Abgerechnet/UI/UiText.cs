namespace Abgerechnet.UI;

/// <summary>
/// All user-visible texts. The user interface is German; keeping every text here makes it easy to review the
/// wording and to add more languages later (issue #15).
/// </summary>
internal static class UiText
{
    public const string AppTitle = "Abgerechnet";

    // ----- Main window -----

    public const string MenuFile = "&Datei";
    public const string MenuChangeFolder = "Rechnungsordner &wechseln…";
    public const string MenuOpenFolder = "Rechnungsordner im &Explorer öffnen";
    public const string MenuExit = "&Beenden";
    public const string MenuHelp = "&Hilfe";
    public const string MenuAbout = "Ü&ber Abgerechnet…";


    public static string FolderStatus(string path) => $"Rechnungsordner: {path}";

    public const string FolderStatusTooltip = "Hier liegen Ihre Rechnungen, Kunden, Vorlagen und PDFs. Klicken öffnet den Ordner.";

    // ----- Invoice folder -----

    public const string ChooseFolderFirstStart =
        "Abgerechnet speichert alles in einem Ordner Ihrer Wahl: Rechnungen, Kunden, Vorlagen und PDFs.\n\n" +
        "Wählen Sie im nächsten Schritt einen Ordner – zum Beispiel einen neuen Ordner „Rechnungen“ in Ihren " +
        "Dokumenten oder in OneDrive, dann sind Ihre Daten automatisch gesichert.";

    public const string ChooseFolderDescription = "Rechnungsordner wählen (ein neuer, leerer Ordner oder ein vorhandener Rechnungsordner)";

    public const string ChooseOtherFolderQuestion = "Möchten Sie einen anderen Rechnungsordner wählen?";

    public static string FolderNotFound(string path) =>
        $"Der Rechnungsordner wurde nicht gefunden:\n{path}\n\n" +
        "Liegt er auf einem USB-Stick, einem Netzlaufwerk oder in einem Cloud-Ordner, der gerade nicht verfügbar ist?";

    public static string DataFileCorrupt(string fileName, int? line, string? backupFileName) =>
        $"Die Datei „{fileName}“ im Rechnungsordner ist beschädigt und kann nicht gelesen werden" +
        (line is { } number ? $" (Fehler in Zeile {number})." : ".") +
        "\n\nAbgerechnet hat die Datei nicht verändert." +
        (backupFileName is null
            ? " Sie können sie mit einem Texteditor reparieren."
            : $" Die vorige Fassung liegt als „{backupFileName}“ im selben Ordner. Sie können die Datei mit einem " +
              $"Texteditor reparieren oder die Sicherung in „{fileName}“ umbenennen.");

    public static string DataFileTooNew(string fileName) =>
        $"Die Datei „{fileName}“ wurde mit einer neueren Version von Abgerechnet gespeichert und kann mit dieser " +
        "Version nicht gelesen werden. Bitte laden Sie die neueste Version von github.com/Schelawski/Abgerechnet " +
        "herunter.\n\nDie Datei wurde nicht verändert.";

    public static string DataFileUnreadable(string fileName, string reason) =>
        $"Die Datei „{fileName}“ kann nicht geöffnet werden. Ist sie gerade in einem anderen Programm geöffnet?\n\n{reason}";

    public static string DataFileNotWritable(string path, string reason) =>
        $"„{path}“ kann nicht gespeichert werden. Ist der Ordner schreibgeschützt oder die Datei in einem anderen " +
        $"Programm geöffnet?\n\n{reason}";

    // ----- Meine Daten -----

    public const string MenuMeineDaten = "&Meine Daten…";
    public const string MeineDatenTitle = "Meine Daten";
    public const string MeineDatenMissing = "Tragen Sie zuerst Ihre Daten ein – Name, Anschrift, Steuernummer und Bankverbindung erscheinen auf jeder Rechnung.";
    public const string MeineDatenButton = "Meine Daten eintragen…";

    public const string TabAbsender = "Absender";
    public const string TabSteuerBank = "Steuer und Bank";
    public const string TabRechnungen = "Rechnungen";

    public const string Firma = "Firma";
    public const string Unterzeile = "Unterzeile";
    public const string UnterzeileHint = "Erscheint unter der Firma, z. B. „Softwareentwicklung im .NET-Umfeld“.";
    public const string Name = "Name";
    public const string Strasse = "Straße und Nr.";
    public const string PlzOrt = "PLZ und Ort";
    public const string Land = "Land";
    public const string Telefon = "Telefon";
    public const string Email = "E-Mail";
    public const string Website = "Website";

    public const string HeadingSteuer = "Steuer";
    public const string Steuernummer = "Steuernummer";
    public const string UstIdNr = "USt-IdNr.";
    public const string SteuernummerHint = "Eine von beiden muss auf jeder Rechnung stehen (§ 14 UStG).";
    public const string Kleinunternehmer = "Ich bin Kleinunternehmer nach § 19 UStG (keine Umsatzsteuer)";
    public const string KleinunternehmerHinweis = "Hinweis auf der Rechnung";
    public const string Umsatzsteuersatz = "Umsatzsteuersatz";
    public const string Prozent = "%";

    public const string HeadingBank = "Bankverbindung";
    public const string Kontoinhaber = "Kontoinhaber";
    public const string Bank = "Bank";
    public const string Iban = "IBAN";
    public const string Bic = "BIC";
    public const string IbanValid = "✓ gültig";
    public const string IbanInvalid = "✗ ungültig";

    public const string HeadingZahlung = "Zahlung und Nummern";
    public const string Zahlungsziel = "Zahlungsziel";
    public const string Tage = "Tage";
    public const string ZahlungszielHint = "0 = kein Zahlungsziel. Nach dieser Zeit erinnert Abgerechnet an offene Rechnungen.";
    public const string NaechsteNummer = "Nächste Rechnungsnummer";
    public const string NaechsteNummerHint = "Leer lassen: höchste vorhandene Nummer + 1. Eintragen, um den Nummernkreis zu beginnen, z. B. „111401“ oder „RE-2026-001“.";
    public const string PdfDateiname = "PDF-Dateiname";
    public static string PdfDateinameHint(string placeholders) => $"Platzhalter: {placeholders}";
    public static string PdfDateinameExample(string example) => $"Beispiel: {example}";

    public const string HeadingNeuePosition = "Vorgaben für neue Positionen";
    public const string Beschreibung = "Beschreibung";
    public const string Einheit = "Einheit";
    public const string Einzelpreis = "Einzelpreis (netto)";
    public const string Euro = "€";
    public static readonly string[] Einheiten = ["Std.", "Tage", "Stück", "Monate", "pauschal"];

    public const string Save = "Speichern";
    public const string Cancel = "Abbrechen";

    public const string NameRequired = "Bitte tragen Sie Ihre Firma oder Ihren Namen ein.";
    public const string AddressRequired = "Bitte tragen Sie Ihre vollständige Anschrift ein (Straße, PLZ und Ort).";
    public const string IbanInvalidMessage = "Die IBAN ist nicht gültig. Bitte prüfen Sie sie oder lassen Sie das Feld leer.";
    public static string UnknownPlaceholders(string placeholders) =>
        $"Der PDF-Dateiname enthält unbekannte Platzhalter: {placeholders}";
    public const string SteuernummerMissingQuestion =
        "Sie haben weder eine Steuernummer noch eine USt-IdNr. eingetragen. Eine davon muss auf jeder Rechnung " +
        "stehen (§ 14 UStG).\n\nTrotzdem speichern?";

    // ----- Kunden -----

    public const string MenuKunden = "&Kunden…";
    public const string KundenTitle = "Kunden";
    public const string KundeNeu = "&Neu";
    public const string KundeLoeschen = "&Löschen";
    public const string KundeOhneNamen = "(ohne Firmennamen)";
    public const string KundenLeer = "Noch keine Kunden. Mit „Neu“ legen Sie den ersten an.";
    public const string Kurzname = "Kurzname";
    public const string KurznameHint = "Für den PDF-Dateinamen, z. B. „Nordlicht“. Leer: die Firma.";
    public const string Ansprechpartner = "Ansprechpartner";
    public const string KundeUstIdNr = "USt-IdNr. des Kunden";
    public const string KundeUstIdNrHint = "Optional; wird später für die E-Rechnung gebraucht.";
    public const string AnschriftVorschau = "So steht die Anschrift auf der Rechnung:";

    public const string KundeFirmaRequired = "Bitte tragen Sie für jeden Kunden die Firma bzw. den Namen ein.";

    public static string KundeLoeschenFrage(string firma) => $"Den Kunden „{firma}“ löschen?";

    public static string KundeWirdVerwendet(string firma) =>
        $"Der Kunde „{firma}“ kann nicht gelöscht werden, weil es Rechnungen an ihn gibt.\n\n" +
        "Gestellte Rechnungen müssen erhalten bleiben. Sie können den Kunden aber umbenennen oder seine Anschrift ändern – " +
        "bereits erzeugte Rechnungen behalten die Anschrift, mit der sie gestellt wurden.";

    public const string AenderungenVerwerfen = "Ihre Änderungen an den Kunden wurden nicht gespeichert. Verwerfen?";

    // ----- Rechnungsliste -----

    public const string FilterJahr = "Jahr";
    public const string FilterStatus = "Status";
    public const string AlleJahre = "Alle Jahre";
    public const string AlleStatus = "Alle";

    public const string KachelOffen = "Offen";
    public const string KachelBezahlt = "Bezahlt";
    public const string KachelGesamt = "Gesamt";
    public const string KachelGesamtTooltip = "Offene und bezahlte Rechnungen. Entwürfe und stornierte Rechnungen zählen nicht mit.";
    public static string Anzahl(int count) => count == 1 ? "1 Rechnung" : $"{count} Rechnungen";
    public static string Entwuerfe(int count) => count switch
    {
        0 => string.Empty,
        1 => "1 Entwurf",
        _ => $"{count} Entwürfe",
    };

    public const string SpalteNummer = "Nr.";
    public const string SpalteKunde = "Kunde";
    public const string SpalteZeitraum = "Zeitraum";
    public const string SpalteDatum = "Datum";
    public const string SpalteStatus = "Status";
    public const string SpalteBetrag = "Betrag (brutto)";

    public static string StatusName(Core.Model.RechnungsStatus status) => status switch
    {
        Core.Model.RechnungsStatus.Entwurf => "Entwurf",
        Core.Model.RechnungsStatus.Offen => "Offen",
        Core.Model.RechnungsStatus.Bezahlt => "Bezahlt",
        Core.Model.RechnungsStatus.Storniert => "Storniert",
        _ => status.ToString(),
    };

    public const string KeineRechnungen = "Noch keine Rechnungen in diesem Ordner.";
    public const string KeineRechnungenFilter = "Keine Rechnungen für diese Auswahl.";
    public const string KundeUnbekannt = "–";

    public const string MenuPdfOeffnen = "&PDF öffnen";
    public const string MenuImOrdnerZeigen = "Im &Ordner zeigen";
    public const string MenuStatusAendern = "&Status ändern";
    public const string MenuLoeschen = "&Löschen";

    public static string PdfFehlt(string path) => $"Die PDF-Datei wurde nicht gefunden:\n{path}";

    public static string StornierenFrage(IReadOnlyList<string> nummern) =>
        (nummern.Count == 1 ? $"Rechnung {nummern[0]} stornieren?" : $"{nummern.Count} Rechnungen stornieren ({string.Join(", ", nummern)})?") +
        "\n\nStornierte Rechnungen bleiben in der Liste und behalten ihre Nummer, zählen aber nicht mehr zu " +
        "„Offen“, „Bezahlt“ und „Gesamt“.";

    public static string LoeschenFrage(IReadOnlyList<string> nummern) =>
        nummern.Count == 1 ? $"Den Entwurf {nummern[0]} löschen?" : $"{nummern.Count} Entwürfe löschen ({string.Join(", ", nummern)})?";

    public const string NurEntwuerfeLoeschen =
        "Nur Entwürfe können gelöscht werden.\n\n" +
        "Eine gestellte Rechnung muss erhalten bleiben, sonst entsteht eine Lücke im Nummernkreis. " +
        "Wählen Sie stattdessen „Status ändern → Storniert“.";

    public static string Betrag(decimal betrag) => betrag.ToString("#,##0.00", German) + " €";

    public static string Datum(DateOnly datum) => datum.ToString("dd.MM.yyyy", German);

    private static readonly System.Globalization.CultureInfo German = System.Globalization.CultureInfo.GetCultureInfo("de-DE");

    // ----- Rechnungsformular -----

    public const string MenuNeueRechnung = "&Neue Rechnung";
    public const string NeueRechnungButton = "+ Neue Rechnung";
    public const string MenuOeffnen = "Ö&ffnen";
    public const string MenuKopieren = "Als neue Rechnung &kopieren";

    public static string RechnungTitle(string nummer) => nummer.Trim().Length > 0 ? $"Rechnung {nummer.Trim()}" : "Neue Rechnung";

    public const string Nummer = "Nr.";
    public const string Rechnungsdatum = "Datum";
    public const string Leistungszeitraum = "Leistungszeitraum";
    public const string Projekt = "Projekt / Betreff";
    public const string Status = "Status";
    public const string Kunde = "Kunde";
    public const string KundenVerwalten = "Kunden verwalten…";
    public const string KeinKunde = "Bitte wählen Sie einen Kunden – oder legen Sie mit „Kunden verwalten…“ einen an.";

    public const string Positionen = "Positionen";
    public const string PositionZeitraum = "Zeitraum";
    public const string PositionBeschreibung = "Beschreibung";
    public const string PositionDetail = "Zusatz (optional)";
    public const string PositionMenge = "Menge";
    public const string PositionEinheit = "Einheit";
    public const string PositionEinzelpreis = "Einzelpreis";
    public const string PositionBetrag = "Betrag";
    public const string PositionHinzufuegen = "+ Position hinzufügen";
    public const string PositionNachOben = "Nach oben";
    public const string PositionNachUnten = "Nach unten";
    public const string PositionEntfernen = "Position entfernen";
    public const string LetztePosition = "Die letzte Position kann nicht entfernt werden.";
    public static string ZeitraumPlatzhalter(string zeitraum) => zeitraum.Trim().Length > 0 ? zeitraum.Trim() : "wie Rechnung";

    public const string SummeNetto = "Nettobetrag";
    public static string SummeUmsatzsteuer(decimal satz) => $"Umsatzsteuer {satz.ToString("0.##", German)} %";
    public const string SummeBrutto = "Rechnungsbetrag";
    public const string KleinunternehmerSumme = "Keine Umsatzsteuer (Kleinunternehmer nach § 19 UStG)";

    public const string SaveAndClose = "Speichern";
    public const string Close = "Schließen";

    public const string NummerFehlt = "Bitte geben Sie eine Rechnungsnummer ein.";
    public static string NummerVergeben(string nummer) =>
        $"Die Rechnungsnummer „{nummer}“ ist bereits vergeben. Jede Nummer darf nur einmal vorkommen.";

    public const string UngespeichertFrage = "Die Rechnung hat ungespeicherte Änderungen. Speichern?";

    public const string GestelltHinweis = "Diese Rechnung wurde bereits gestellt und ist schreibgeschützt.";
    public const string Bearbeiten = "Bearbeiten…";
    public static string BearbeitenFrage(string nummer) =>
        $"Rechnung {nummer} wurde bereits gestellt.\n\n" +
        "Wenn Sie sie ändern, müssen Sie das PDF neu erzeugen und dem Kunden die korrigierte Rechnung erneut " +
        "schicken. Trotzdem bearbeiten?";

    // ----- PDF und Vorschau -----

    public const string PdfErzeugenButton = "&PDF erzeugen…";
    public const string MenuPdfErzeugen = "PDF &erzeugen…";
    public static string VorschauTitle(string nummer) => $"Vorschau – Rechnung {nummer}";
    public const string PdfSpeichern = "PDF &speichern";
    public const string PdfOeffnenButton = "PDF öffnen";
    public const string ImOrdnerZeigenButton = "Im Ordner zeigen";
    public static string VorlageInfo(string name, bool mitgeliefert) =>
        mitgeliefert ? $"Vorlage: {name} (mitgeliefert)" : $"Vorlage: {name}.html aus dem Vorlagen-Ordner";
    public static string PdfGespeichert(string datei) => $"Gespeichert: {datei}";

    public const string HinweiseTitel = "Bitte prüfen Sie vor dem Versand:";
    public static string Pflichtangabe(Core.Model.Pflichtangabe angabe) => angabe switch
    {
        Core.Model.Pflichtangabe.AbsenderName => "Ihre Firma oder Ihr Name fehlt (Meine Daten).",
        Core.Model.Pflichtangabe.AbsenderAnschrift => "Ihre Anschrift ist unvollständig (Meine Daten).",
        Core.Model.Pflichtangabe.Steuernummer => "Steuernummer oder USt-IdNr. fehlt (Meine Daten).",
        Core.Model.Pflichtangabe.EmpfaengerName => "Es ist kein Kunde gewählt.",
        Core.Model.Pflichtangabe.EmpfaengerAnschrift => "Die Anschrift des Kunden ist unvollständig.",
        Core.Model.Pflichtangabe.Rechnungsnummer => "Die Rechnungsnummer fehlt.",
        Core.Model.Pflichtangabe.Leistungszeitraum => "Der Leistungszeitraum fehlt.",
        Core.Model.Pflichtangabe.Leistungsbeschreibung => "Mindestens eine Position hat keine Beschreibung.",
        Core.Model.Pflichtangabe.KleinunternehmerHinweis => "Der Hinweis für Kleinunternehmer ist leer (Meine Daten).",
        _ => angabe.ToString(),
    };
    public static string UnbekanntePlatzhalter(IEnumerable<string> platzhalter) =>
        $"Die Vorlage enthält unbekannte Platzhalter: {string.Join(", ", platzhalter)}";

    public static string PdfExistiert(string datei) => $"Die Datei „{datei}“ gibt es bereits im PDF-Ordner. Überschreiben?";
    public static string PdfFehler(string grund) =>
        $"Das PDF konnte nicht gespeichert werden. Ist die Datei gerade in einem anderen Programm geöffnet?\n\n{grund}";
    public static string VorlageFehler(string grund) => $"Die Vorlage konnte nicht gelesen werden:\n\n{grund}";

    public const string WebView2Fehlt =
        "Für die Vorschau und das PDF braucht Abgerechnet die „Microsoft Edge WebView2 Runtime“. Sie ist auf " +
        "Windows 10 und 11 normalerweise vorinstalliert, fehlt auf diesem Computer aber.\n\n" +
        "Möchten Sie die Download-Seite von Microsoft öffnen? Nach der Installation starten Sie Abgerechnet neu.";
    public const string WebView2DownloadUrl = "https://developer.microsoft.com/microsoft-edge/webview2/";
    public static string WebView2Fehler(string grund) => $"Die Vorschau konnte nicht gestartet werden:\n\n{grund}";

    // ----- About -----

    public static string AboutText(string version) =>
        $"Abgerechnet {version}\n\n" +
        "Rechnungen für Freiberufler – einfach erfassen, als PDF erzeugen, fertig.\n\n" +
        "Abgerechnet ist kostenlos und freie Software (GPL-3.0).\n" +
        "Einzige offizielle Quelle: github.com/Schelawski/Abgerechnet";

    // ----- Errors -----

    public static string SettingsNotSaved(string reason) =>
        $"Die Einstellungen von Abgerechnet konnten nicht gespeichert werden:\n\n{reason}";

    public static string UnexpectedError(string message) =>
        $"Ein unerwarteter Fehler ist aufgetreten:\n\n{message}";
}
