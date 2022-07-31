﻿using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCRtpTransceiver : NativeBase<Webrtc.RtpTransceiver>, IRTCRtpTransceiver
    {

        public static IRTCRtpTransceiver Create(Webrtc.RtpTransceiver nativeTransceiver) =>
            new RTCRtpTransceiver(nativeTransceiver);

        private RTCRtpTransceiver(RtpTransceiver nativeTransceiver) : base(nativeTransceiver)
        {
        }

        public RTCRtpTransceiverDirection CurrentDirection =>
            NativeObject.CurrentDirection.FromNative();

        public RTCRtpTransceiverDirection Direction
        {
            get => NativeObject.Direction.FromNative();
            set => NativeObject.SetDirection(Direction.ToNative());
        }

        public string Mid => NativeObject.Mid;

        public IRTCRtpReceiver Receiver => RTCRtpReceiver.Create(NativeObject.Receiver);

        public IRTCRtpSender Sender => RTCRtpSender.Create(NativeObject.Sender);

        public bool Stopped => NativeObject.IsStopped;


        public void SetCodecPreferences(RTCRtpCodecCapability[] codecs)
        {
            throw new NotImplementedException();
        }

        public void Stop() => NativeObject.Stop();
    }
}