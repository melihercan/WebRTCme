using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp
{
    class AnswerMediaSection : MediaSection
    {
        public AnswerMediaSection(
            IceParameters iceParameters,
            IceCandidate[] iceCandidates,
            DtlsParameters dtlsParameters,
            SctpParameters sctpParameters,
            PlainRtpParameters plainRtpParameters,
            bool planB,
            MediaObject offerMediaObject,
            RtpParameters offerRtpParameters,
            RtpParameters answerRtpParameters,
            ProducerCodecOptions codecOptions,
            bool extmapAllowMixed) : base(iceParameters, iceCandidates, dtlsParameters, planB)
        {
            _mediaObject.Mid = offerMediaObject.Mid;
            _mediaObject.Kind = offerMediaObject.Kind;
        }

        protected override void SetDtlsRole(DtlsRole? dtlsRole)
        {
            throw new NotImplementedException();
        }
    }
}
