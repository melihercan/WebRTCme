using System.Diagnostics;
using System.Text;
using WebRTCme.DeviceTests.Core;

namespace WebRTCme.DeviceTests.Runner;

/// <summary>
/// Runs the loopback scenarios as soon as the app appears, and reports them three ways.
/// </summary>
/// <remarks>
/// <para>
/// Built in code rather than XAML: there is one label on one page, and a XAML file plus its
/// code-behind would be more to read than the thing it describes.
/// </para>
/// <para>
/// Three reporting paths, because no single one is reliable on all three platforms. The harness
/// reads whichever it can:
/// </para>
/// <list type="bullet">
///   <item><c>Console.WriteLine</c> - reaches adb logcat on Android and devicectl's console on iOS.</item>
///   <item><c>Debug.WriteLine</c> - reaches the system log on Apple platforms when stdout does not.</item>
///   <item>A file under the app's data directory - survives a log that the tooling truncates or
///         interleaves, and on Android is readable with <c>adb exec-out run-as</c>.</item>
/// </list>
/// <para>
/// The scenarios also go on screen, so a person holding the phone can see the outcome without a
/// cable.
/// </para>
/// </remarks>
public class App : Application
{
    /// <summary>
    /// Set to a scenario name to run one in isolation. Android takes it from the launch intent;
    /// elsewhere the harness runs the whole set and then re-runs the isolated one in a fresh
    /// process, which is what the device-enumeration scenario needs to mean anything.
    /// </summary>
    public static string? Filter { get; set; }

    /// <summary>Where the report was written, so the harness can be told rather than guess.</summary>
    public const string ReportFileName = "webrtcme-devicetests.log";

    readonly Label _output = new()
    {
        FontSize = 12,
        FontFamily = "monospace",
        LineBreakMode = LineBreakMode.WordWrap,
        Text = "starting..."
    };

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var page = new ContentPage
        {
            Title = "WebRTCme device tests",
            Content = new ScrollView { Content = new VerticalStackLayout { Padding = 16, Children = { _output } } }
        };

        // Started here, not from page.Appearing, and that distinction cost a day.
        //
        // Appearing fires when the page becomes *visible*. A headless runner must not depend on
        // that: on an emulator started with -no-window, or on any device where another app happens
        // to be in front, the page never appears, no scenario ever runs, and the harness reports
        // "the runner did not finish" - which reads exactly like a hang or a crash and is neither.
        //
        // Nothing here needs the UI. The scenarios are two peer connections talking to each other;
        // the label is a courtesy for whoever is holding the device, and updating it is already
        // wrapped so that a failure to draw cannot fail a run.
        _ = RunAsync();

        return new Window(page);
    }

    async Task RunAsync()
    {
        // Before anything creates a peer connection, or the setup of the first one goes unlogged -
        // which on Apple is exactly the part under suspicion.
        NativeLogging.StartIfRequested();

        var lines = new StringBuilder();

        void Report(string line)
        {
            // Both channels: on Apple platforms stdout and the system log are not the same pipe,
            // and which one a given launch mechanism captures varies.
            Console.WriteLine(line);
            Debug.WriteLine(line);
            lines.AppendLine(line);
        }

        bool ok;
        try
        {
            ok = await ScenarioRunner.RunAsync(Report, Filter);
        }
        catch (Exception exception)
        {
            // The runner itself failing is a result too, and a silent app is the worst outcome for
            // a harness that is waiting on a summary line.
            ok = false;
            Report($"{ScenarioRunner.SummaryPrefix} | total:0 passed:0 failed:1 skipped:0 | "
                   + $"runner threw {exception.GetType().Name}: {exception.Message}");
        }

        TryWriteReport(lines.ToString());

        // Explicitly onto the UI thread. The scenarios complete their TaskCompletionSources from
        // whatever thread the native WebRTC callback arrives on, so the continuation that gets here
        // is not necessarily the one that started - and touching a Label from a native callback
        // thread kills the process on Android. That crash lands *after* the summary line, so the
        // harness reads a clean pass and the device shows "this app has a bug".
        //
        // Wrapped, too: the screen is the least important of the three reporting paths, and a run
        // that reported correctly must not be recorded as a failure because a Label threw.
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _output.Text = lines.ToString();
                _output.TextColor = ok ? Colors.SeaGreen : Colors.IndianRed;
            });
        }
        catch (Exception exception)
        {
            Console.WriteLine($"WEBRTCME-REPORT | could not update the screen: {exception.GetType().Name}");
        }
    }

    static void TryWriteReport(string report)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, ReportFileName);
            File.WriteAllText(path, report);
            Console.WriteLine($"WEBRTCME-REPORT | {path}");
            Debug.WriteLine($"WEBRTCME-REPORT | {path}");
        }
        catch (Exception exception)
        {
            // Not fatal: the log lines above are the primary channel and the file is the backup.
            Console.WriteLine($"WEBRTCME-REPORT | could not write: {exception.GetType().Name}");
        }
    }
}
