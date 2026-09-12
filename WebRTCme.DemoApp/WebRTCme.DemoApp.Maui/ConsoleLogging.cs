using Microsoft.Extensions.Logging;

namespace WebRTCme.DemoApp.Maui;

/// <summary>
/// Writes <see cref="ILogger"/> output to the console, so that it goes somewhere.
/// </summary>
/// <remarks>
/// <para>This application configured no logging providers at all, which meant every
/// <c>ILogger</c> call in the middleware went nowhere on every MAUI platform. It is not a
/// cosmetic gap: the whole of <c>CallViewModel</c> reports through <c>ILogger</c> - a local
/// device dying, a recovery succeeding or failing, a peer's media changing - and on Android and
/// the Apple platforms none of it was visible. Debugging there meant reading a call's behaviour
/// from the far end and inferring the rest, which on 2026-09-12 cost several rebuilds to find a
/// fault that one log line would have named.</para>
/// <para>Hand-written rather than <c>AddConsole</c> because that lives in a package this project
/// does not reference, and a diagnostic convenience is not worth a new dependency. What it costs
/// instead is this file.</para>
/// <para><c>Console.WriteLine</c> reaches somewhere useful on each platform: logcat on Android,
/// stderr on Mac Catalyst and iOS (visible through <c>open --stderr</c> or
/// <c>devicectl ... --console</c>), the browser console on Blazor, and stdout on Windows.</para>
/// </remarks>
internal sealed class ConsoleLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new ConsoleLogger(categoryName);

    public void Dispose() { }

    private sealed class ConsoleLogger(string category) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

        // Everything, because the interesting lines on this project are Information and the
        // filtering that matters is done by appsettings.
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel) || formatter is null)
                return;

            // The same "########" marker the platform code uses, so one grep finds everything
            // this project says regardless of which layer said it.
            var message = formatter(state, exception);
            System.Console.WriteLine($"######## [{logLevel}] {Short(category)}: {message}");

            if (exception is not null)
                System.Console.WriteLine($"######## {exception}");
        }

        // The category is a full type name and the leading namespace is the same for all of them.
        static string Short(string category) =>
            category?.Substring(category.LastIndexOf('.') + 1) ?? string.Empty;
    }
}
