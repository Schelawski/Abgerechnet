using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.Core.Vorlagen;

/// <summary>A template ready to fill: its HTML and where it comes from.</summary>
/// <param name="Name">Name without ".html", e.g. "klassisch".</param>
/// <param name="Datei">Full path of the template file, or <c>null</c> for the copy built into the exe.</param>
/// <param name="Ersatz">True when the requested template was not found and the standard one is used instead.</param>
public sealed record VorlagenQuelle(string Name, string Html, string? Datei, bool Ersatz = false);

/// <summary>
/// The invoice templates (issue #9). Three are built into Abgerechnet.exe and copied into the folder
/// <c>Vorlagen</c> of the invoice folder, where the user adapts them; every other <c>.html</c> file there is a
/// template too. A file in the folder takes precedence over the built-in template of the same name.
/// </summary>
public static class MitgelieferteVorlagen
{
    public const string Standard = "klassisch";

    /// <summary>The built-in templates, in the order they are offered.</summary>
    public static readonly IReadOnlyList<string> Namen = ["klassisch", "modern", "schlicht"];

    /// <summary>The HTML of a built-in template, e.g. "schlicht".</summary>
    /// <exception cref="ArgumentException">There is no built-in template with that name.</exception>
    public static string Lesen(string name)
    {
        var resource = $"Abgerechnet.Vorlagen.{name.ToLowerInvariant()}.html";
        using var stream = typeof(MitgelieferteVorlagen).Assembly.GetManifestResourceStream(resource)
            ?? throw new ArgumentException($"No built-in template '{name}'.", nameof(name));
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static bool IstMitgeliefert(string name) => Namen.Contains(name, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Copies the built-in templates into the folder <c>Vorlagen</c> when it has no template yet (a new invoice
    /// folder). Existing files are never overwritten. Returns the names that were copied.
    /// </summary>
    /// <exception cref="IOException">The files cannot be written.</exception>
    public static IReadOnlyList<string> InOrdnerKopieren(DataFolder folder)
    {
        Directory.CreateDirectory(folder.VorlagenPath);
        if (Directory.EnumerateFiles(folder.VorlagenPath, "*.html").Any())
            return [];

        var kopiert = new List<string>();
        foreach (var name in Namen)
        {
            var datei = Pfad(folder, name);
            if (File.Exists(datei))
                continue;
            File.WriteAllText(datei, Lesen(name));
            kopiert.Add(name);
        }
        return kopiert;
    }

    /// <summary>
    /// Puts the original of a built-in template back into the folder. A changed copy is kept as
    /// <c>name.bak.html</c> (replacing an older backup). Returns the path of the backup, or <c>null</c> when there was
    /// nothing to keep.
    /// </summary>
    /// <exception cref="IOException">The files cannot be written.</exception>
    public static string? Wiederherstellen(DataFolder folder, string name)
    {
        var original = Lesen(name);
        Directory.CreateDirectory(folder.VorlagenPath);
        var datei = Pfad(folder, name);
        string? sicherung = null;
        if (File.Exists(datei) && File.ReadAllText(datei) != original)
        {
            sicherung = Path.Combine(folder.VorlagenPath, name + ".bak.html");
            File.Copy(datei, sicherung, overwrite: true);
        }
        File.WriteAllText(datei, original);
        return sicherung;
    }

    /// <summary>
    /// All templates to choose from: the built-in ones and every other <c>.html</c> file in the folder (backups
    /// "*.bak.html" excluded), built-in first, then the own ones alphabetically.
    /// </summary>
    public static IReadOnlyList<string> Verfuegbare(DataFolder folder)
    {
        var eigene = Directory.Exists(folder.VorlagenPath)
            ? Directory.EnumerateFiles(folder.VorlagenPath, "*.html")
                .Select(Path.GetFileNameWithoutExtension)
                .OfType<string>()
                .Where(n => !n.EndsWith(".bak", StringComparison.OrdinalIgnoreCase) && !IstMitgeliefert(n))
                .Order(StringComparer.CurrentCultureIgnoreCase)
            : Enumerable.Empty<string>();
        return Namen.Concat(eigene).ToList();
    }

    /// <summary>The template an invoice is printed with: its own choice, otherwise the standard from the settings.</summary>
    public static string NameFuer(Rechnung rechnung, Einstellungen einstellungen) =>
        !string.IsNullOrWhiteSpace(rechnung.Vorlage) ? rechnung.Vorlage.Trim() : einstellungen.Rechnung.Vorlage;

    /// <summary>
    /// Loads a template: the file in the folder <c>Vorlagen</c>, otherwise the built-in template of that name,
    /// otherwise (deleted own template) the built-in standard template, marked as <see cref="VorlagenQuelle.Ersatz"/>.
    /// </summary>
    /// <exception cref="IOException">The template file exists but cannot be read.</exception>
    public static VorlagenQuelle Laden(DataFolder folder, string name)
    {
        var datei = Pfad(folder, name);
        if (File.Exists(datei))
            return new VorlagenQuelle(name, File.ReadAllText(datei), datei);
        if (IstMitgeliefert(name))
            return new VorlagenQuelle(name, Lesen(name), null);
        return new VorlagenQuelle(Standard, Lesen(Standard), null, Ersatz: true);
    }

    private static string Pfad(DataFolder folder, string name) => Path.Combine(folder.VorlagenPath, name + ".html");
}
