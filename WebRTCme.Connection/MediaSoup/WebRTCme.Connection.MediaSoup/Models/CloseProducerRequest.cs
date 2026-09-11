using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    /// <summary>
    /// Tells the server to stop carrying a producer entirely.
    /// </summary>
    /// <remarks>
    /// Named "Request" for consistency with its neighbours, but it travels as a notification -
    /// see <see cref="MethodName"/>.
    ///
    /// Distinct from pausing: a paused producer keeps its consumers and can resume, while a closed
    /// one takes its consumers with it, which is what makes a shared screen's tile disappear from
    /// every other peer rather than freeze.
    /// </remarks>
    public class CloseProducerRequest
    {
        public string ProducerId { get; init; }
    }
}
