namespace Abgerechnet.Core;

/// <summary>
/// Everything Abgerechnet remembers about this computer between sessions. Stored as
/// <c>Abgerechnet.settings.json</c>. The invoice data itself lives in the invoice folder.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Full path of the invoice folder opened last. Empty until the user chose one.</summary>
    public string DataFolder { get; set; } = string.Empty;

    /// <summary>Position and size of the main window, <c>null</c> until it was closed once.</summary>
    public WindowPlacement? MainWindow { get; set; }

    /// <summary>Replaces missing values (e.g. from a hand-edited file) with defaults.</summary>
    internal void Normalize()
    {
        DataFolder ??= string.Empty;
        if (MainWindow is { Width: <= 0 } or { Height: <= 0 })
            MainWindow = null;
    }
}

/// <summary>Window bounds in screen pixels (when not maximized) and whether it was maximized.</summary>
public sealed record WindowPlacement(int X, int Y, int Width, int Height, bool Maximized);
