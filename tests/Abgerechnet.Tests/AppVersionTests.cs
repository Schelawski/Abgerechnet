using Abgerechnet.Core;

namespace Abgerechnet.Tests;

public class AppVersionTests
{
    [Theory]
    [InlineData("1.2.3+abc123", "1.2.3")]
    [InlineData("1.3.0-beta.1+abc123", "1.3.0-beta.1")]
    [InlineData("1.2.3", "1.2.3")]
    public void StripBuildMetadata_RemovesCommitSuffix(string input, string expected)
    {
        Assert.Equal(expected, AppVersion.StripBuildMetadata(input));
    }

    [Fact]
    public void Current_IsTheProjectVersion()
    {
        Assert.Matches(@"^\d+\.\d+\.\d+", AppVersion.Current);
        Assert.DoesNotContain("+", AppVersion.Current);
    }

    [Fact]
    public void Help_IsEmbeddedIntoTheExe()
    {
        var assembly = typeof(AppVersion).Assembly;
        Assert.Contains("Abgerechnet.Help.help.de.md", assembly.GetManifestResourceNames());
    }
}
