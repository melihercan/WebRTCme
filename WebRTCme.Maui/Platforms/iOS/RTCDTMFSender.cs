using System;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal class RTCDTMFSender : ApiBase, IRTCDTMFSender
    {
        public static IRTCDTMFSender Create(Webrtc.IRTCDtmfSender nativeDtmfSender) =>
            new RTCDTMFSender(nativeDtmfSender);

        public RTCDTMFSender(Webrtc.IRTCDtmfSender nativeDtmfSender) : base(nativeDtmfSender)
        {
        }

        public string ToneBuffer => throw new NotImplementedException();

        public event EventHandler OnToneChange;

        public void InsertDTMF(string tones, ulong duration = 100, ulong interToneGap = 70) =>
            ((Webrtc.RTCDtmfSender)NativeObject).InsertDtmf(tones, (double)duration, (double)interToneGap);
    }
}