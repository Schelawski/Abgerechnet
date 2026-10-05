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

    public const string EmptyState =
        "Willkommen bei Abgerechnet.\n\nHier erscheint bald die Liste Ihrer Rechnungen.";

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
