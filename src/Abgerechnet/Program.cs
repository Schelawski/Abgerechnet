using Abgerechnet.Core;
using Abgerechnet.Core.Einrichtung;
using Abgerechnet.UI;
using Abgerechnet.UI.Einrichtung;

namespace Abgerechnet;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args"><c>--neue-rechnung</c>: open a new invoice right away (set by the welcome wizard).</param>
    [STAThread]
    private static void Main(string[] args)
    {
        // Applies the settings from the project file (PerMonitorV2 high DPI, visual styles, default font).
        ApplicationConfiguration.Initialize();

        Application.AddMessageFilter(new KontextmenueTaste());
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
            MessageBox.Show(UiText.UnexpectedError(e.Exception.Message), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);

        var settingsStore = new SettingsStore();
        var settings = settingsStore.Load();
        var neueRechnung = args.Contains(LocalInstall.NeueRechnungArgument, StringComparer.OrdinalIgnoreCase);

        Core.Storage.DataFolder? folder;
        if (string.IsNullOrWhiteSpace(settings.DataFolder))
        {
            // First start: the welcome wizard chooses the folder. Closed before that, Abgerechnet ends.
            using var wizard = new EinrichtungsAssistent(settingsStore, settings, aktuellerOrdner: null);
            wizard.ShowDialog();
            if (wizard.Ergebnis == EinrichtungsErgebnis.KopieGestartet)
                return;
            folder = wizard.Folder;
            neueRechnung |= wizard.ErsteRechnung;
        }
        else
        {
            folder = DataFolderDialogs.OpenAtStartup(settings);
        }

        if (folder is null)
            return;

        Application.Run(new MainForm(settingsStore, settings, folder, neueRechnung));
    }
}
