using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    /// <summary>
    /// The envelope every stats response arrives in.
    /// </summary>
    public class StatsResponse<T>
    {
        public T[] Stats { get; init; }
    }
}
