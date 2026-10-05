using System.Drawing;
using System.Drawing.Imaging;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.Core.Vorlagen;

/// <summary>
/// The logo on the invoice: <c>Vorlagen\logo.png</c>, which all built-in templates show when it exists.
/// </summary>
public static class Logo
{
    public const string DateiName = "logo.png";

    /// <summary>Image files the wizard accepts; anything but PNG is converted.</summary>
    public static readonly IReadOnlyList<string> Endungen = [".png", ".jpg", ".jpeg", ".gif", ".bmp"];

    public static string Pfad(DataFolder folder) => Path.Combine(folder.VorlagenPath, DateiName);

    public static bool Vorhanden(DataFolder folder) => File.Exists(Pfad(folder));

    /// <summary>
    /// Makes <paramref name="bild"/> the logo: a PNG is copied, other images are converted to PNG. An existing logo
    /// is replaced.
    /// </summary>
    /// <exception cref="IOException">The image cannot be read or the logo cannot be written.</exception>
    public static void Uebernehmen(DataFolder folder, string bild)
    {
        Directory.CreateDirectory(folder.VorlagenPath);
        var ziel = Pfad(folder);
        if (string.Equals(Path.GetFullPath(bild), Path.GetFullPath(ziel), StringComparison.OrdinalIgnoreCase))
            return;

        var temp = ziel + ".tmp";
        try
        {
            if (string.Equals(Path.GetExtension(bild), ".png", StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(bild, temp, overwrite: true);
            }
            else
            {
                using var image = Image.FromFile(bild);
                image.Save(temp, ImageFormat.Png);
            }
            File.Move(temp, ziel, overwrite: true);
        }
        catch (Exception ex) when (ex is OutOfMemoryException or ArgumentException or System.Runtime.InteropServices.ExternalException)
        {
            // GDI+ reports an unreadable image as "out of memory".
            throw new IOException(ex.Message, ex);
        }
        finally
        {
            if (File.Exists(temp))
                File.Delete(temp);
        }
    }

    /// <summary>Removes the logo; the invoices are shown without one.</summary>
    /// <exception cref="IOException">The file cannot be deleted.</exception>
    public static void Entfernen(DataFolder folder)
    {
        if (File.Exists(Pfad(folder)))
            File.Delete(Pfad(folder));
    }
}
