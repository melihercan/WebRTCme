using System.Threading.Channels;

namespace WebRTCme.Windows;

/// <summary>
/// Moves callbacks off WebRTC's signalling thread, in arrival order.
/// </summary>
/// <remarks>
/// Every callback the shim raises arrives on the signalling thread, which all peer connections in
/// the process share, and the ABI calls block on it. Raising .NET events straight from there puts
/// application code on that thread, where one synchronous call back into the binding -- adding a
/// candidate, sending on a channel -- deadlocks it against itself. A single reader keeps the
/// ordering callers expect while getting them off that thread.
/// </remarks>
internal sealed class EventDispatcher : IDisposable
{
    private readonly Channel<Action> _queue =
        Channel.CreateUnbounded<Action>(new UnboundedChannelOptions { SingleReader = true });

    internal EventDispatcher() => _ = Task.Run(RunAsync);

    internal void Post(Action action) => _queue.Writer.TryWrite(action);

    private async Task RunAsync()
    {
        await foreach (var action in _queue.Reader.ReadAllAsync())
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                // A handler that throws must not take the dispatcher down with it, or every
                // later event on this object is lost silently.
                System.Diagnostics.Debug.WriteLine($"######## Event handler failed: {ex}");
            }
        }
    }

    public void Dispose() => _queue.Writer.TryComplete();
}
