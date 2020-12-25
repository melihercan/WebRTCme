using System;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCDTMFSender : ApiBase, IRTCDTMFSender
    {
        public static IRTCDTMFSender Create(Webrtc.RTCDtmfSender nativeDtmfSender) =>
            new RTCDTMFSender(nativeDtmfSender);

        public RTCDTMFSender(Webrtc.RTCDtmfSender nativeDtmfSender) : base(nativeDtmfSender)
        {
        }

        public string ToneBuffer => throw new NotImplementedException();

        public event EventHandler OnToneChange;

        public void InsertDTMF(string tones, ulong duration = 100, ulong interToneGap = 70) =>
            ((Webrtc.RTCDtmfSender)NativeObject).InsertDtmf(tones, (double)duration, (double)interToneGap);
    }
}