using Abgerechnet.Core;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.UI;

/// <summary>
/// Choosing and opening the invoice folder, with plain-language messages when that fails.
/// </summary>
internal static class DataFolderDialogs
{
    /// <summary>
    /// Opens the folder remembered in the settings; if it cannot be opened, lets the user choose another one.
    /// Returns <c>null</c> when the user gives up (the app then ends).
    /// </summary>
    public static DataFolder? OpenAtStartup(AppSettings settings)
    {
        try
        {
            return DataFolder.Open(settings.DataFolder);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or DataFileException)
        {
            var answer = MessageBox.Show(
                Describe(ex, settings.DataFolder) + "\n\n" + UiText.ChooseOtherFolderQuestion,
                UiText.AppTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            return answer == DialogResult.Yes ? ChooseAndOpen(owner: null, settings.DataFolder) : null;
        }
    }

    /// <summary>
    /// Shows the folder dialog until a folder could be opened or the user cancels (<c>null</c>).
    /// </summary>
    public static DataFolder? ChooseAndOpen(IWin32Window? owner, string? initialPath)
    {
        while (true)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = UiText.ChooseFolderDescription,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
            };
            if (!string.IsNullOrWhiteSpace(initialPath) && Directory.Exists(initialPath))
                dialog.InitialDirectory = initialPath;

            if (dialog.ShowDialog(owner) != DialogResult.OK)
                return null;

            try
            {
                return DataFolder.Open(dialog.SelectedPath);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException or DataFileException)
            {
                MessageBox.Show(owner, Describe(ex, dialog.SelectedPath), UiText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                initialPath = dialog.SelectedPath;
            }
        }
    }

    /// <summary>A plain-language message for a folder that cannot be opened.</summary>
    public static string Describe(Exception ex, string? folderPath) => ex switch
    {
        DataFileException { Problem: DataFileProblem.Corrupt } data =>
            UiText.DataFileCorrupt(data.FileName, data.Line, File.Exists(JsonDataFile.BackupPath(data.FilePath))
                ? Path.GetFileName(JsonDataFile.BackupPath(data.FilePath))
                : null),
        DataFileException { Problem: DataFileProblem.TooNew } data => UiText.DataFileTooNew(data.FileName),
        DataFileException { Problem: DataFileProblem.Unreadable } data => UiText.DataFileUnreadable(data.FileName, data.Message),
        DataFileException data => UiText.DataFileNotWritable(data.FilePath, data.Message),
        DirectoryNotFoundException => UiText.FolderNotFound(folderPath ?? ex.Message),
        _ => ex.Message,
    };
}
