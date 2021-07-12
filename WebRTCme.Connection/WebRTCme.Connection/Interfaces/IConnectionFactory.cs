using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection
{
    public interface IConnectionFactory
    {
        IConnection SelectConnection(ConnectionType connectionType);
    }
}
