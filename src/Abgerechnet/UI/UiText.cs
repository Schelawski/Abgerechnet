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
    public const string MenuExit = "&Beenden";
    public const string MenuHelp = "&Hilfe";
    public const string MenuAbout = "Ü&ber Abgerechnet…";

    public const string EmptyState =
        "Willkommen bei Abgerechnet.\n\nHier erscheint bald die Liste Ihrer Rechnungen.";

    // ----- About -----

    public static string AboutText(string version) =>
        $"Abgerechnet {version}\n\n" +
        "Rechnungen für Freiberufler – einfach erfassen, als PDF erzeugen, fertig.\n\n" +
        "Abgerechnet ist kostenlos und freie Software (GPL-3.0).\n" +
        "Einzige offizielle Quelle: github.com/Schelawski/Abgerechnet";

    // ----- Errors -----

    public static string UnexpectedError(string message) =>
        $"Ein unerwarteter Fehler ist aufgetreten:\n\n{message}";
}
