using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        MakeJsInteropOmitNulls(js);

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

    /// <summary>
    /// Makes JSInterop leave unset properties out of the JSON it sends to the browser.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This is a requirement the package places on every Blazor consumer, not a test fixture.</b>
    /// WebRTCme's models use nullable properties for optional dictionary members - RTCConfiguration
    /// leaves BundlePolicy, IceTransportPolicy and RtcpMuxPolicy null unless you set them - and
    /// JSInterop serialises a null property as an explicit null rather than omitting it. The browser
    /// rejects that outright, because null is not a member of an enum:
    /// </para>
    /// <code>
    /// TypeError: Failed to construct 'RTCPeerConnection': Failed to read the 'bundlePolicy'
    /// property from 'RTCConfiguration': The provided value 'null' is not a valid enum value
    /// of type RTCBundlePolicy.
    /// </code>
    /// <para>
    /// So a Blazor app that does not do this cannot create a peer connection at all. WebRTCme.DemoApp
    /// .Blazor does it in Program.ConfigureProviders, by the same reflection, and this host does it
    /// because a test host has to be an honest consumer - not because the test needs it.
    /// </para>
    /// <para>
    /// The reflection is the ugly part and there is no supported alternative: the options object
    /// JSInterop serialises with is a non-public property of JSRuntime, and WebAssemblyHostBuilder
    /// exposes no way to configure it. It is also write-once - System.Text.Json seals an options
    /// instance the first time it is used - so this has to happen before any interop call, which is
    /// why it is the first thing after Build().
    /// </para>
    /// <para>
    /// Recorded in doc/KnownGaps.md. The library could stop requiring it by serialising its own
    /// arguments with JsonHelper.WebRtcJsonSerializerOptions, which already says exactly this.
    /// </para>
    /// </remarks>
    static void MakeJsInteropOmitNulls(IJSRuntime js)
    {
        var property = typeof(JSRuntime).GetProperty(
            "JsonSerializerOptions", BindingFlags.NonPublic | BindingFlags.Instance);

        if (property?.GetValue(js) is not JsonSerializerOptions options)
        {
            // Deliberately fatal, unlike the demo app, which catches and writes to the console. A
            // test host that silently skipped this would report a page full of failures that say
            // nothing about the package under test.
            throw new InvalidOperationException(
                "Could not reach JSRuntime.JsonSerializerOptions. WebRTCme needs JSInterop to omit "
                + "nulls; see the remarks on this method.");
        }

        // DefaultIgnoreCondition, not the IgnoreNullValues the demo app sets - that property has been
        // obsolete since .NET 5 and this is the setting it forwards to.
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
}
