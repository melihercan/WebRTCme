using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Utilme;

namespace WebRTCme.Connection.MediaSoup
{
    public interface IMediaSoupServerApi : IAsyncDisposable
    {
        Task<Result<Unit>> ConnectAsync(Guid id, string name, string room);
        Task<Result<Unit>> DisconnectAsync(Guid id);

        /// <summary>
        /// Sends a protoo request and waits for its response.
        /// </summary>
        /// <remarks>
        /// Only for the methods the server dispatches as requests. Anything else is answered with
        /// "unknown request method", because requests and notifications are separate switches over
        /// separate sets of names - see <see cref="NotifyAsync"/>.
        /// </remarks>
        Task<Result<object>> ApiAsync(string method, object data = null);

        /// <summary>
        /// Sends a protoo notification, which has no response.
        /// </summary>
        /// <remarks>
        /// The demo server splits its protocol in two, and the split is not about importance:
        /// pausing a producer is a notification, while asking for transport statistics is a
        /// request. Sending one as the other fails both ways - as a request it is rejected with
        /// "unknown request method", and as a notification it is silently ignored.
        ///
        /// The notifications are closeProducer, pauseProducer, resumeProducer, pauseConsumer,
        /// resumeConsumer, setConsumerPreferredLayers, setConsumerPriority,
        /// requestConsumerKeyFrame and changeDisplayName. Everything else this client sends is a
        /// request.
        ///
        /// The result reports the send, not the outcome: there is no acknowledgement to wait for,
        /// so a notification the server dislikes fails silently by design.
        /// </remarks>
        Task<Result<Unit>> NotifyAsync(string method, object data = null);

        event IMediaSoupServerNotify.NotifyDelegateAsync NotifyEventAsync;
        event IMediaSoupServerNotify.RequestDelegateAsync RequestEventAsync;

    }
}
