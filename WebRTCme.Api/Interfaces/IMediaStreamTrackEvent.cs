﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaStreamTrackEvent
    {
        Task<IMediaStreamTrack> Track { get; }
    }
}