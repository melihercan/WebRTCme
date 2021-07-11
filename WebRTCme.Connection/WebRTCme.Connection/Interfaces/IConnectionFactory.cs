using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.Interfaces
{
    public interface IConnectionFactory
    {
        IConnection SelectConnection(ConnectionType connectionType);
    }
}
