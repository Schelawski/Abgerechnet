using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Abgerechnet.UI;

/// <summary>
/// Turns a filled HTML template into a PDF with WebView2 and shows PDFs. Used by the PDF preview (issue #8) and the
/// template choice in the welcome wizard (issue #11). Everything happens locally: the template folder is mapped to a
/// virtual host, links open in the normal browser.
/// </summary>
internal sealed class PdfDrucker
{
    /// <summary>Virtual host that maps the template folder, so the template can load "logo.png" and own CSS files.</summary>
    private const string VorlagenHost = "vorlage.abgerechnet.example";

    /// <summary><c>%LOCALAPPDATA%\Abgerechnet</c>: WebView2's data and the temporary preview PDFs.</summary>
    public static readonly string AppDatenOrdner =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Abgerechnet");

    /// <summary>Base address for relative paths in the template (<c>logo.png</c>).</summary>
    public static readonly Uri Basis = new($"https://{VorlagenHost}/");

    public PdfDrucker(WebView2 webView) => WebView = webView;

    public WebView2 WebView { get; }

    /// <summary>A new file name for a temporary preview PDF.</summary>
    public static string NeueVorschauDatei() => Path.Combine(AppDatenOrdner, "Vorschau", $"{Guid.NewGuid():N}.pdf");

    /// <summary>Creates the WebView2 with its own data folder and maps <paramref name="vorlagenOrdner"/>.</summary>
    /// <exception cref="WebView2RuntimeNotFoundException">The WebView2 runtime is not installed.</exception>
    public async Task StartenAsync(string vorlagenOrdner)
    {
        var umgebung = await CoreWebView2Environment.CreateAsync(browserExecutableFolder: null, userDataFolder: Path.Combine(AppDatenOrdner, "WebView2"));
        await WebView.EnsureCoreWebView2Async(umgebung);

        var core = WebView.CoreWebView2;
        Directory.CreateDirectory(vorlagenOrdner);
        core.SetVirtualHostNameToFolderMapping(VorlagenHost, vorlagenOrdner, CoreWebView2HostResourceAccessKind.Allow);
        core.Settings.IsStatusBarEnabled = false;
        // A link in the invoice opens in the normal browser instead of replacing the preview.
        core.NavigationStarting += (_, args) =>
        {
            if (args.Uri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                args.Cancel = true;
                Oeffnen(args.Uri);
            }
        };
        core.NewWindowRequested += (_, args) =>
        {
            args.Handled = true;
            Oeffnen(args.Uri);
        };
    }

    /// <summary>Renders <paramref name="html"/> and prints it to <paramref name="pdf"/> (DIN A4).</summary>
    /// <exception cref="IOException">The PDF could not be written.</exception>
    public async Task DruckenAsync(string html, string pdf)
    {
        await NavigierenAsync(() => WebView.CoreWebView2.NavigateToString(html));
        Directory.CreateDirectory(Path.GetDirectoryName(pdf)!);
        if (!await WebView.CoreWebView2.PrintToPdfAsync(pdf, DruckEinstellungen()))
            throw new IOException(pdf);
    }

    /// <summary>Shows a PDF in WebView2's viewer; <paramref name="ansicht"/> is appended, e.g. "#toolbar=0".</summary>
    public Task PdfZeigenAsync(string pdf, string ansicht = "") =>
        NavigierenAsync(() => WebView.CoreWebView2.Navigate(new Uri(pdf).AbsoluteUri + ansicht));

    /// <summary>Starts a navigation and waits until the page has loaded.</summary>
    private async Task NavigierenAsync(Action navigation)
    {
        var geladen = new TaskCompletionSource();
        void OnCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs args) => geladen.TrySetResult();
        WebView.CoreWebView2.NavigationCompleted += OnCompleted;
        try
        {
            navigation();
            await geladen.Task;
        }
        finally
        {
            WebView.CoreWebView2.NavigationCompleted -= OnCompleted;
        }
    }

    /// <summary>DIN A4 portrait; the margins come from the template's <c>@page</c> rule.</summary>
    private CoreWebView2PrintSettings DruckEinstellungen()
    {
        var settings = WebView.CoreWebView2.Environment.CreatePrintSettings();
        settings.PageWidth = 8.27;   // DIN A4 in inches
        settings.PageHeight = 11.69;
        settings.MarginTop = settings.MarginBottom = settings.MarginLeft = settings.MarginRight = 0;
        settings.ShouldPrintBackgrounds = true;
        settings.ShouldPrintHeaderAndFooter = false;
        settings.Orientation = CoreWebView2PrintOrientation.Portrait;
        return settings;
    }

    /// <summary>Deletes a temporary PDF; one left behind in <c>%LOCALAPPDATA%\Abgerechnet\Vorschau</c> is harmless.</summary>
    public static void Loeschen(string pdf)
    {
        try
        {
            File.Delete(pdf);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    public static void Oeffnen(string? ziel)
    {
        if (!string.IsNullOrEmpty(ziel))
            Process.Start(new ProcessStartInfo { FileName = ziel, UseShellExecute = true })?.Dispose();
    }
}
