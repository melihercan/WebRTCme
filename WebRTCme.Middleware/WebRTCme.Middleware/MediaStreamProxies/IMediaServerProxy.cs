﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware.MediaStreamProxies
{
    interface IMediaServerProxy
    {
        Task JoinAsync(ConnectionRequestParameters connectionRequestParameters);
    }
}
