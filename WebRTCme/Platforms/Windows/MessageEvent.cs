namespace WebRTCme.Windows;

/// <summary>
/// A data channel message. <see cref="Data"/> is a <see cref="byte"/> array for a binary message
/// and a <see cref="string"/> for a text one, matching the other bindings.
/// </summary>
internal sealed class MessageEvent(object data) : IMessageEvent
{
    public object Data { get; } = data;

    /// <summary>Web page concepts with no counterpart on a native data channel.</summary>
    public string Origin => null;

    public string LastEventId => null;

    public object Source => null;

    public object[] Ports => [];

    public void Dispose() { }
}
