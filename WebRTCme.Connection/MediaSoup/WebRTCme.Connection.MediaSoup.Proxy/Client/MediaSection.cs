using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    abstract class MediaSection
    {
        protected readonly MediaObject _mediaObject = new();
        protected readonly bool _planB;

        readonly IceParameters _iceParameters;
        readonly IceCandidate[] _iceCandidates;
        readonly DtlsParameters _dtlsParameters;

        protected MediaSection(IceParameters iceParameters, IceCandidate[] iceCandidates, 
            DtlsParameters dtlsParameters, bool planB)
        {
            _iceParameters = iceParameters;
            _iceCandidates = iceCandidates;
            _dtlsParameters = dtlsParameters;
            _planB = planB;

            if (iceParameters is not null)
            {
                SetIceParameters(iceParameters);
            }
        }

        public void SetIceParameters(IceParameters iceParameters)
        {
            _mediaObject.IceUfrag = new IceUfrag { Ufrag = iceParameters.UsernameFragment };
            _mediaObject.IcePwd = new IcePwd { Password = iceParameters.Password };
        }
    }
}
