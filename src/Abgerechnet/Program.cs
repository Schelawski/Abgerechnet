using Abgerechnet.Core;
using Abgerechnet.UI;

namespace Abgerechnet;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // Applies the settings from the project file (PerMonitorV2 high DPI, visual styles, default font).
        ApplicationConfiguration.Initialize();

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
            MessageBox.Show(UiText.UnexpectedError(e.Exception.Message), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);

        var settingsStore = new SettingsStore();
        var settings = settingsStore.Load();

        var folder = DataFolderDialogs.OpenAtStartup(settings);
        if (folder is null)
            return;

        Application.Run(new MainForm(settingsStore, settings, folder));
    }
}
