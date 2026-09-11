using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    /// <summary>
    /// Asks the server which simulcast layers to forward for one consumer.
    /// </summary>
    /// <remarks>
    /// Named "Request" for consistency with its neighbours, but it travels as a notification -
    /// see <see cref="MethodName"/>.
    ///
    /// Layers are indices into what the producer publishes, counting from 0, and the server clamps
    /// anything higher to the top layer available. Asking for a layer therefore sets a ceiling
    /// rather than a fixed choice: the server still drops below it when it has to.
    /// </remarks>
    public class SetConsumerPreferredLayersRequest
    {
        public string ConsumerId { get; init; }

        /// <summary>Resolution layer. 0 is the smallest.</summary>
        public int SpatialLayer { get; init; }

        /// <summary>Frame rate layer within the spatial one. 0 is the lowest.</summary>
        public int TemporalLayer { get; init; }
    }
}
