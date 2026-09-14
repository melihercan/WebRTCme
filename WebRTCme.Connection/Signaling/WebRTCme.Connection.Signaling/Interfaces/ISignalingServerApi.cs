using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Utilme;
// Utilme's own Unit, which is what Result<T> is documented to pair with: "a type with exactly one
// value, used as the payload of a Result<T> for operations that succeed without producing
// anything". Result<System.Reactive.Unit> predates it. Aliased rather than left to `using Utilme`
// because System.Reactive is imported here too and both namespaces have a Unit.
using Unit = Utilme.Unit;

namespace WebRTCme.Connection.Signaling
{
    public interface ISignalingServerApi : IAsyncDisposable
    {
        /// <summary>
        /// Brings the transport up before a join, if it is not already up.
        /// </summary>
        /// <remarks>
        /// Only the client proxy has a transport to manage - the server-side implementation of this
        /// interface is the hub itself - so this and <see cref="DisconnectAsync"/> default to doing
        /// nothing.
        /// </remarks>
        Task EnsureConnectedAsync() => Task.CompletedTask;

        /// <summary>
        /// Closes the transport gracefully once a call is over.
        /// </summary>
        /// <remarks>
        /// Without it the connection lives until the process does, and the server logs the socket
        /// as having "closed prematurely" because no WebSocket closing handshake ever happened.
        /// Harmless in itself - the peer has already left - but it makes an orderly shutdown
        /// indistinguishable from a crashed client in the server's logs.
        /// </remarks>
        Task DisconnectAsync() => Task.CompletedTask;

        Task<Result<RTCIceServer[]>> GetIceServersAsync();

        Task<Result<Unit>> JoinAsync(Guid id, string name, string room);

        Task<Result<Unit>> LeaveAsync(Guid id);

        Task<Result<Unit>> SdpAsync(Guid peerId, string sdp);

        Task<Result<Unit>> IceAsync(Guid peerId, string ice);

        Task<Result<Unit>> MediaAsync(Guid id, bool videoMuted, bool audioMuted, bool speaking);

        event ISignalingServerNotify.PeerJoinedDelegateAsync PeerJoinedEventAsync;
        event ISignalingServerNotify.PeerLeftDelegateAsync PeerLeftEventAsync;
        event ISignalingServerNotify.PeerSdpAsyncDelegateAsync PeerSdpEventAsync;
        event ISignalingServerNotify.PeerIceAsyncDelegateAsync PeerIceEventAsync;
        event ISignalingServerNotify.PeerMediaAsyncDelegateAsync PeerMediaEventAsync;
    }
}
