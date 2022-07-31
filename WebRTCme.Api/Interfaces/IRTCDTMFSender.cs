using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IRTCDTMFSender : IDisposable // INativeObject
    {
        string ToneBuffer { get; }

        event EventHandler OnToneChange;

        void InsertDTMF(string tones, ulong duration = 100, ulong interToneGap = 70);
    }
}
