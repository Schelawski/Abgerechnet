using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.Tests;

public class DataFolderTests
{
    [Fact]
    public void Open_EmptyFolder_CreatesFilesAndSubfolders()
    {
        using var temp = new TempFolder();

        var folder = DataFolder.Open(temp.Path);

        Assert.True(File.Exists(temp.File("abgerechnet.json")));
        Assert.True(File.Exists(temp.File("kunden.json")));
        Assert.True(File.Exists(temp.File("rechnungen.json")));
        Assert.True(Directory.Exists(temp.File("Vorlagen")));
        Assert.True(Directory.Exists(temp.File("PDF")));
        Assert.Empty(folder.Rechnungen.Rechnungen);
        Assert.Empty(folder.Kunden.Kunden);
        Assert.True(DataFolder.ContainsData(temp.Path));
    }

    [Fact]
    public void SaveAndOpen_KeepsTheData()
    {
        using var temp = new TempFolder();
        var folder = DataFolder.Open(temp.Path);
        var kunde = new Kunde { Firma = "Beispiel AG" };
        folder.Kunden.Kunden.Add(kunde);
        folder.Rechnungen.Rechnungen.Add(new Rechnung { Nummer = "111401", KundeId = kunde.Id });
        folder.SaveKunden();
        folder.SaveRechnungen();

        var reopened = DataFolder.Open(temp.Path);

        Assert.Equal("Beispiel AG", Assert.Single(reopened.Kunden.Kunden).Firma);
        Assert.Equal(kunde.Id, Assert.Single(reopened.Rechnungen.Rechnungen).KundeId);
    }

    [Fact]
    public void Open_CorruptFile_ChangesNothingInTheFolder()
    {
        using var temp = new TempFolder();
        temp.CreateFile("rechnungen.json", "{ kaputt");

        var ex = Assert.Throws<DataFileException>(() => DataFolder.Open(temp.Path));

        Assert.Equal("rechnungen.json", ex.FileName);
        Assert.Equal("{ kaputt", File.ReadAllText(temp.File("rechnungen.json")));
        // The other files and folders are not created either.
        Assert.Equal(["rechnungen.json"], Directory.GetFileSystemEntries(temp.Path).Select(Path.GetFileName));
    }

    [Fact]
    public void Open_MissingFolder_Throws()
    {
        using var temp = new TempFolder();

        Assert.Throws<DirectoryNotFoundException>(() => DataFolder.Open(temp.File("gibt-es-nicht")));
    }

    [Fact]
    public void ContainsData_IsFalseForAnEmptyFolder()
    {
        using var temp = new TempFolder();

        Assert.False(DataFolder.ContainsData(temp.Path));
    }
}
