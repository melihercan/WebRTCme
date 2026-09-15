using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using WebRTCme.DeviceTests.Core;

namespace WebRTCme.BlazorTestHost;

/// <summary>
/// Runs the loopback scenarios in the browser and writes each result into the page.
/// </summary>
/// <remarks>
/// <para>
/// No root component and no Razor. The app renders nothing of its own - the results element is in
/// index.html and three small functions there put text into it - so a component would be a layer
/// between the scenarios and the DOM that has nothing to do.
/// </para>
/// <para>
/// The filter comes from the query string, <c>?filter=ACompletedCall</c>, which is the browser's
/// equivalent of the intent extra the Android runner takes. The harness needs it for the same
/// reason: the device-enumeration scenario only means something in a page where no call has
/// happened yet.
/// </para>
/// </remarks>
public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        var host = builder.Build();

        var js = host.Services.GetRequiredService<IJSRuntime>();

        // Nothing is done to JSInterop's JSON serialiser here, and that is the point of this host.
        // Until the binding shaped its own arguments, a Blazor app had to reach into JSRuntime's
        // non-public JsonSerializerOptions by reflection and set the ignore condition globally, or
        // the first RTCPeerConnection threw - "The provided value 'null' is not a valid enum value
        // of type RTCBundlePolicy". WebRTCme.DemoApp.Blazor still carries that workaround. If this
        // host ever needs it again, the fix in JsRuntimeExtensions has regressed.

        // The scenarios reach the browser's WebRTC API through this. Nothing else on any platform
        // needs it, and every other binding ignores it.
        LoopbackScenarios.JsRuntime = js;

        // In-process, so reporting is an ordinary synchronous call. ScenarioRunner takes an
        // Action<string>, and firing InvokeVoidAsync into the void for each line would let the
        // results arrive out of order - or, at the end of a run, not at all.
        var inProcess = (IJSInProcessRuntime)js;

        bool ok;
        try
        {
            var filter = inProcess.Invoke<string>("webrtcmeFilter");
            ok = await ScenarioRunner.RunAsync(line => inProcess.InvokeVoid("webrtcmeReport", line), filter);
        }
        catch (Exception exception)
        {
            // A silent page is the worst outcome for a harness waiting on a summary line, so the
            // runner failing is itself reported as a result.
            ok = false;
            inProcess.InvokeVoid("webrtcmeReport",
                $"{ScenarioRunner.SummaryPrefix} | total:0 passed:0 failed:1 skipped:0 | "
                + $"runner threw {exception.GetType().Name}: {exception.Message}");
        }

        // Flips the element's data-state, which is what Playwright waits on. Set last and always,
        // including after a failure: the harness distinguishes "finished, something failed" from
        // "never finished", and only this call can tell it which.
        inProcess.InvokeVoid("webrtcmeDone", ok);

        await host.RunAsync();
    }

}
