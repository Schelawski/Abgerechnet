using System.Reflection;

namespace Abgerechnet.Core;

/// <summary>
/// The version of Abgerechnet. Release builds get it from the Git tag (<c>v1.2.3</c> → <c>-p:Version=1.2.3</c>,
/// see <c>.github/workflows/release.yml</c>); local builds use the <c>Version</c> of the project file.
/// </summary>
public static class AppVersion
{
    /// <summary>E.g. "1.2.3" or "1.3.0-beta.1" (without the "+commit" suffix the SDK appends).</summary>
    public static string Current { get; } = Read();

    private static string Read()
    {
        var informational = typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrWhiteSpace(informational))
            return typeof(AppVersion).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

        return StripBuildMetadata(informational);
    }

    /// <summary>Removes the "+commit" suffix: "1.2.3+abc123" → "1.2.3".</summary>
    internal static string StripBuildMetadata(string version)
    {
        var plus = version.IndexOf('+');
        return plus < 0 ? version : version[..plus];
    }
}
