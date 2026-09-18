namespace WebRTCme.DeviceTests.Runner;

internal static partial class NativeLogging
{
    // Held for the process lifetime. RTCCallbackLogger stops logging when it is deallocated, and a
    // logger that is collected mid-run takes the evidence with it - silently, which is the worst
    // way for a diagnostic to fail.
    static Webrtc.RTCCallbackLogger? _logger;

    static partial void StartCore(string severity, Action<string> report)
    {
        _logger = new Webrtc.RTCCallbackLogger
        {
            Severity = severity.ToLowerInvariant() switch
            {
                "verbose" => Webrtc.RTCLoggingSeverity.Verbose,
                "warning" => Webrtc.RTCLoggingSeverity.Warning,
                "error" => Webrtc.RTCLoggingSeverity.Error,
                _ => Webrtc.RTCLoggingSeverity.Info
            }
        };

        // The message-and-severity overload rather than the plain one: knowing whether a line is
        // an error or a trace is most of its value when reading a tail backwards from a crash.
        //
        // This block runs on whichever libwebrtc thread logged, so it must do nothing but format
        // and write. Touching anything else from here is how a diagnostic becomes a second bug.
        _logger.StartWithMessageAndSeverityHandler((message, level) =>
            report($"{level} | {message.TrimEnd('\n')}"));
    }
}
