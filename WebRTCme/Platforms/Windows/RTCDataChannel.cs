using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static WebRTCme.Bindings.Maui.Windows.Interop;

namespace WebRTCme.Windows;

/// <summary>
/// An SCTP data channel, either created locally or arriving from the peer.
/// </summary>
/// <remarks>
/// The ABI carries a channel's configuration in one direction only -- it is set when the channel
/// is created and never read back -- so a channel that arrives through <c>OnDataChannel</c>
/// reports the W3C defaults for <see cref="Ordered"/>, <see cref="Protocol"/> and the retransmit
/// limits. Label, id, state and buffered amount are read from the channel itself and are always
/// accurate.
/// </remarks>
internal sealed class RTCDataChannel : IRTCDataChannel
{
    private readonly EventDispatcher _events = new();
    private readonly GCHandle _self;

    private IntPtr _handle;
    private int _lastState = -1;

    internal RTCDataChannel(IntPtr handle, RTCDataChannelInit init = null)
    {
        _handle = handle;
        _self = GCHandle.Alloc(this);

        Ordered = init?.Ordered ?? true;
        MaxPacketLifeTime = init?.MaxPacketLifeTime;
        MaxRetransmits = init?.MaxRetransmits;
        Protocol = init?.Protocol ?? string.Empty;
        Negotiated = init?.Negotiated ?? false;

        WebRtcRuntime.Check(DataChannelGetLabel(handle, out var label),
                            "read the data channel label");
        Label = WebRtcRuntime.TakeString(label);

        // Registered before anything can open the channel. For an incoming channel this
        // constructor runs inside the on_data_channel callback, which is the only place the
        // open transition can still be caught.
        unsafe
        {
            var observer = new DataChannelObserver
            {
                OnState = &RaiseState,
                OnMessage = &RaiseMessage,
                OnBufferedAmountChange = &RaiseBufferedAmountChange
            };

            var status = DataChannelSetObserver(handle, observer, GCHandle.ToIntPtr(_self));
            if (status != Ok)
            {
                _self.Free();
                WebRtcRuntime.Check(status, "observe the data channel");
            }
        }
    }

    public string Label { get; }

    public bool Ordered { get; }

    public ushort? MaxPacketLifeTime { get; }

    public ushort? MaxRetransmits { get; }

    public string Protocol { get; }

    public bool Negotiated { get; }

    /// <summary>The transport assigns this when the channel opens; -1 until then, which the
    /// W3C type cannot express, so it reads as 0.</summary>
    public ushort Id
    {
        get
        {
            if (_handle == IntPtr.Zero || DataChannelGetId(_handle, out var id) != Ok || id < 0)
                return 0;
            return (ushort)id;
        }
    }

    public RTCDataChannelState ReadyState
    {
        get
        {
            if (_handle == IntPtr.Zero || DataChannelGetState(_handle, out var state) != Ok)
                return RTCDataChannelState.Closed;
            return ToState(state);
        }
    }

    public uint BufferedAmount
    {
        get
        {
            if (_handle == IntPtr.Zero ||
                DataChannelGetBufferedAmount(_handle, out var amount) != Ok)
                return 0;
            return amount > uint.MaxValue ? uint.MaxValue : (uint)amount;
        }
    }

    /// <summary>
    /// Remembered rather than applied: the ABI reports each message's own binary flag, so what
    /// arrives is already typed and this cannot change how it is delivered.
    /// </summary>
    public BinaryType BinaryType { get; set; } = BinaryType.ArrayBuffer;

    /// <summary>Remembered; the shim raises no low-buffer signal to compare it against.</summary>
    public uint BufferedAmountLowThreshold { get; set; }

    public event EventHandler OnOpen;
    public event EventHandler OnClosing;
    public event EventHandler OnClose;
    public event EventHandler<IMessageEvent> OnMessage;

    // Neither has a signal behind it: the ABI reports buffered_amount changes but no threshold
    // crossing, and a send failure is asynchronous and only logged natively.
#pragma warning disable CS0067
    public event EventHandler OnBufferedAmountLow;
    public event EventHandler<IErrorEvent> OnError;
#pragma warning restore CS0067

    /// <summary>
    /// Sends a <see cref="byte"/> array as binary or a <see cref="string"/> as UTF-8 text, the
    /// same two types the Android and Blazor bindings accept.
    /// </summary>
    public void Send(object data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

        byte[] payload;
        bool binary;

        switch (data)
        {
            case byte[] bytes:
                payload = bytes;
                binary = true;
                break;
            case string text:
                payload = Encoding.UTF8.GetBytes(text);
                binary = false;
                break;
            default:
                throw new ArgumentException(
                    $"A data channel takes byte[] or string, not {data.GetType().FullName}.",
                    nameof(data));
        }

        unsafe
        {
            fixed (byte* pinned = payload)
            {
                WebRtcRuntime.Check(
                    DataChannelSend(_handle, pinned, payload.Length, binary ? 1 : 0),
                    "send on the data channel");
            }
        }
    }

    public void Close()
    {
        if (_handle == IntPtr.Zero)
            return;

        WebRtcRuntime.Check(DataChannelClose(_handle), "close the data channel");
    }

    public void Dispose()
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle == IntPtr.Zero)
            return;

        DataChannelClose(handle);
        _events.Dispose();

        // Releasing unregisters the observer, so no callback can arrive afterwards and the
        // GCHandle behind user_data is safe to free.
        DataChannelRelease(handle);
        _self.Free();
    }

    // ---- callbacks ----------------------------------------------------------
    // On the signalling thread: copy, queue, return.

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void RaiseState(IntPtr userData, int state)
    {
        var self = FromUserData(userData);
        if (self is null || Interlocked.Exchange(ref self._lastState, state) == state)
            return;

        self._events.Post(() =>
        {
            switch (state)
            {
                case DataChannelStateOpen:
                    self.OnOpen?.Invoke(self, EventArgs.Empty);
                    break;
                case DataChannelStateClosing:
                    self.OnClosing?.Invoke(self, EventArgs.Empty);
                    break;
                case DataChannelStateClosed:
                    self.OnClose?.Invoke(self, EventArgs.Empty);
                    break;
            }
        });
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void RaiseMessage(IntPtr userData, byte* data, int size, int isBinary)
    {
        var self = FromUserData(userData);
        if (self is null || size < 0)
            return;

        // The buffer is borrowed for this call only, so it has to be copied before the event
        // reaches the dispatcher.
        var bytes = new ReadOnlySpan<byte>(data, size).ToArray();
        object payload = isBinary != 0 ? bytes : Encoding.UTF8.GetString(bytes);

        self._events.Post(() => self.OnMessage?.Invoke(self, new MessageEvent(payload)));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void RaiseBufferedAmountChange(IntPtr userData, ulong buffered) { }

    private static RTCDataChannel FromUserData(IntPtr userData) =>
        userData == IntPtr.Zero ? null : GCHandle.FromIntPtr(userData).Target as RTCDataChannel;

    private static RTCDataChannelState ToState(int state) => state switch
    {
        DataChannelStateConnecting => RTCDataChannelState.Connecting,
        DataChannelStateOpen => RTCDataChannelState.Open,
        DataChannelStateClosing => RTCDataChannelState.Closing,
        _ => RTCDataChannelState.Closed
    };
}
