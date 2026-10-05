using Abgerechnet.Core;

namespace Abgerechnet.Tests;

public class SettingsStoreTests
{
    [Fact]
    public void SaveAndLoad_KeepsFolderAndWindow()
    {
        using var temp = new TempFolder();
        var store = new SettingsStore(temp.File("exe"), temp.File("appdata"));
        store.Save(new AppSettings
        {
            DataFolder = @"C:\Users\Jörg\OneDrive\Rechnungen",
            MainWindow = new WindowPlacement(10, 20, 1000, 700, Maximized: true),
        });

        var loaded = new SettingsStore(temp.File("exe"), temp.File("appdata")).Load();

        Assert.Equal(@"C:\Users\Jörg\OneDrive\Rechnungen", loaded.DataFolder);
        Assert.Equal(new WindowPlacement(10, 20, 1000, 700, true), loaded.MainWindow);
        Assert.Contains("Jörg", File.ReadAllText(temp.File(Path.Combine("exe", SettingsStore.FileName))));
    }

    [Fact]
    public void Load_WithoutFile_ReturnsDefaults()
    {
        using var temp = new TempFolder();

        var settings = new SettingsStore(temp.File("exe"), temp.File("appdata")).Load();

        Assert.Equal(string.Empty, settings.DataFolder);
        Assert.Null(settings.MainWindow);
    }

    [Fact]
    public void Load_CorruptFile_ReturnsDefaults()
    {
        using var temp = new TempFolder();
        temp.CreateFile(Path.Combine("exe", SettingsStore.FileName), "{ kaputt");

        var settings = new SettingsStore(temp.File("exe"), temp.File("appdata")).Load();

        Assert.Equal(string.Empty, settings.DataFolder);
    }

    [Fact]
    public void Load_InvalidWindowSize_IsIgnored()
    {
        using var temp = new TempFolder();
        temp.CreateFile(Path.Combine("exe", SettingsStore.FileName),
            """{ "DataFolder": null, "MainWindow": { "X": 0, "Y": 0, "Width": 0, "Height": 500, "Maximized": false } }""");

        var settings = new SettingsStore(temp.File("exe"), temp.File("appdata")).Load();

        Assert.Equal(string.Empty, settings.DataFolder);
        Assert.Null(settings.MainWindow);
    }
}
