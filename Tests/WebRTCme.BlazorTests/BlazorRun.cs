using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace WebRTCme.BlazorTests;

/// <summary>One scenario's result, as the page reported it.</summary>
public record ScenarioLine(string Name, string Outcome, string Message);

/// <summary>
/// Loads the test host in headless Chromium, waits for the run to finish, and hands back what the
/// page said.
/// </summary>
public sealed class BlazorRun : IAsyncDisposable
{
    /// <summary>
    /// Long enough for the WebAssembly runtime to download and start on a cold cache and for a
    /// negotiation to complete, short enough that a hung run fails rather than hangs the suite.
    /// </summary>
    const int TimeoutMs = 120_000;

    IPlaywright? _playwright;
    IBrowser? _browser;

    public IReadOnlyList<string> Lines { get; private set; } = [];
    public IReadOnlyDictionary<string, ScenarioLine> Scenarios { get; private set; } =
        new Dictionary<string, ScenarioLine>();
    public string? Summary { get; private set; }
    public IReadOnlyList<string> ConsoleErrors { get; private set; } = [];

    /// <summary>
    /// Where the host is being served. Set by Tests/Test-Blazor.ps1, which starts it.
    /// </summary>
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("WEBRTCME_BLAZOR_URL")
        ?? throw new InvalidOperationException(
            "WEBRTCME_BLAZOR_URL is not set. This tier needs the Blazor host served first - run it "
            + "through Tests/Test-Blazor.ps1, which builds the host against the package under test, "
            + "starts it and passes the URL.");

    /// <summary>Loads the page and runs the scenarios, optionally just one of them.</summary>
    public static async Task<BlazorRun> StartAsync(string? filter = null)
    {
        var run = new BlazorRun();
        await run.ExecuteAsync(filter);
        return run;
    }

    async Task ExecuteAsync(string? filter)
    {
        _playwright = await Playwright.CreateAsync();

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args =
            [
                // A synthetic camera and microphone, and permission granted without a prompt. This
                // is why Blazor is the one platform in the suite that needs no hardware - and why
                // it can run while the Windows app is holding the machine's only real camera.
                "--use-fake-device-for-media-stream",
                "--use-fake-ui-for-media-stream",
            ]
        });

        var page = await _browser.NewPageAsync();

        var consoleErrors = new List<string>();
        page.Console += (_, message) =>
        {
            if (message.Type == "error") consoleErrors.Add(message.Text);
        };
        page.PageError += (_, error) => consoleErrors.Add(error);

        var url = string.IsNullOrEmpty(filter)
            ? BaseUrl
            : $"{BaseUrl.TrimEnd('/')}/?filter={Uri.EscapeDataString(filter)}";

        await page.GotoAsync(url, new PageGotoOptions { Timeout = TimeoutMs });

        // Waits for the run to end rather than for a fixed time. A page still saying "running" when
        // this expires did not finish - the app died, or never started - and that has to fail rather
        // than read as zero failures.
        await page.WaitForFunctionAsync(
            "() => document.getElementById('webrtcme-results')?.dataset.state !== 'running'",
            null,
            new PageWaitForFunctionOptions { Timeout = TimeoutMs });

        var text = await page.InnerTextAsync("#webrtcme-results");

        ConsoleErrors = consoleErrors;
        Lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Summary = Lines.LastOrDefault(l => l.Contains("WEBRTCME-SUMMARY"));
        Scenarios = Lines.Select(Parse).OfType<ScenarioLine>().ToDictionary(s => s.Name);
    }

    /// <summary>
    /// Pulls a scenario line apart. The shape is fixed by ScenarioResult.ToLine:
    /// <c>WEBRTCME-SCENARIO | PASSED  |     412ms | Name</c>, with an optional <c>| message</c>.
    /// </summary>
    static ScenarioLine? Parse(string line)
    {
        var match = Regex.Match(
            line,
            @"WEBRTCME-SCENARIO \| (?<outcome>\w+)\s*\|\s*(?<ms>[\d.]+)ms \| (?<name>\w+)(?: \| (?<message>.*))?$");

        return match.Success
            ? new ScenarioLine(match.Groups["name"].Value,
                               match.Groups["outcome"].Value,
                               match.Groups["message"].Value)
            : null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }
}
