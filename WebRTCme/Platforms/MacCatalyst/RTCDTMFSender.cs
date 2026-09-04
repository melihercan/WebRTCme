using System;
using WebRTCme;
using WebRTCme.Platforms.MacCatalyst.Custom;

namespace WebRTCme.MacCatalyst
{
    internal class RTCDTMFSender : NativeBase<Webrtc.IRTCDtmfSender>, IRTCDTMFSender
    {
        public RTCDTMFSender(Webrtc.IRTCDtmfSender nativeDtmfSender) : base(nativeDtmfSender)
        { }

        public string ToneBuffer => throw new NotImplementedException();

        public event EventHandler OnToneChange;

        public void InsertDTMF(string tones, ulong duration = 100, ulong interToneGap = 70) =>
            NativeObject.InsertDtmf(tones, (double)duration, (double)interToneGap);
    }
}