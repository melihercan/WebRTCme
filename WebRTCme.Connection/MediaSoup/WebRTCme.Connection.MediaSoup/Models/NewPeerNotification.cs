using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    /// <summary>
    /// The body of a <c>newPeer</c> notification.
    /// </summary>
    /// <remarks>
    /// The peer is nested under its own member rather than being the notification body, exactly as
    /// the router capabilities are in their response. Deserialising the body straight into a
    /// <see cref="Peer"/> yields one with every property null, and since a peer with no id is
    /// discarded, the arrival of a peer went unnoticed entirely - so a client only ever learned a
    /// display name for peers that were already in the room when it joined.
    /// </remarks>
    public class NewPeerNotification
    {
        public Peer Peer { get; init; }
    }
}
