using Org.Webrtc;
using System;
using WebRTCme;
using WebRTCme.Platforms.Android.Custom;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
{
    internal class RTCDTMFSender : NativeBase<Webrtc.DtmfSender>, IRTCDTMFSender
    {
        public static IRTCDTMFSender Create(Webrtc.DtmfSender nativeDtmfSender) =>
            new RTCDTMFSender(nativeDtmfSender);

        public RTCDTMFSender(DtmfSender nativeDtmfSender) : base(nativeDtmfSender)
        {
        }

        public string ToneBuffer => throw new NotImplementedException();

        public event EventHandler OnToneChange;

        public void InsertDTMF(string tones, ulong duration = 100, ulong interToneGap = 70)
        {
            throw new NotImplementedException();
        }

    }
}