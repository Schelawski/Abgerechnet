using Abgerechnet.Core.Storage;

namespace Abgerechnet.Core.Vorlagen;

/// <summary>A template ready to fill: its HTML and where it comes from.</summary>
/// <param name="Datei">Full path of the template file, or <c>null</c> for the copy built into the exe.</param>
public sealed record VorlagenQuelle(string Name, string Html, string? Datei);

/// <summary>
/// The templates built into Abgerechnet.exe. The user's copy in the folder <c>Vorlagen</c> takes precedence; until it
/// exists the built-in one is used. (Copying them into the folder, more templates and choosing one follow in #9.)
/// </summary>
public static class MitgelieferteVorlagen
{
    public const string Standard = "schlicht";

    /// <summary>The HTML of a built-in template, e.g. "schlicht".</summary>
    /// <exception cref="ArgumentException">There is no built-in template with that name.</exception>
    public static string Lesen(string name)
    {
        var resource = $"Abgerechnet.Vorlagen.{name}.html";
        using var stream = typeof(MitgelieferteVorlagen).Assembly.GetManifestResourceStream(resource)
            ?? throw new ArgumentException($"No built-in template '{name}'.", nameof(name));
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// The template for an invoice: <c>Vorlagen/schlicht.html</c> in the invoice folder if it exists, otherwise the
    /// built-in one.
    /// </summary>
    /// <exception cref="IOException">The template file exists but cannot be read.</exception>
    public static VorlagenQuelle Laden(DataFolder folder)
    {
        var datei = Path.Combine(folder.VorlagenPath, Standard + ".html");
        return File.Exists(datei)
            ? new VorlagenQuelle(Standard, File.ReadAllText(datei), datei)
            : new VorlagenQuelle(Standard, Lesen(Standard), null);
    }
}
