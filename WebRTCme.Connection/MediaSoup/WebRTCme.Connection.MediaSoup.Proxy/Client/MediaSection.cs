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
            
            if (iceCandidates is not null)
            {
                List<Candidate> candidates = new();

                foreach (var iceCandidate in iceCandidates)
                {
                    Candidate candidate = new()
                    {
                        Foundation = iceCandidate.Foundation,
                        ComponentId = 1,    // RTP
                        Transport = iceCandidate.Protocol.ToSdp(), 
                        Priority = iceCandidate.Priority,
                        ConnectionAddress = iceCandidate.Ip,
                        Port = iceCandidate.Port,
                        Type = iceCandidate.Type.ToSdp()
                    };
                    candidates.Add(candidate);
                }
                _mediaObject.Candidates = candidates.ToArray();
            }

            _mediaObject.IceOptions = new IceOptions { Tags = new string[] { "renomination" } };

            if (dtlsParameters is not null)
            {
                SetDtlsRole(dtlsParameters.DtlsRole);
            }
        }

        protected abstract void SetDtlsRole(DtlsRole? dtlsRole);

        public void SetIceParameters(IceParameters iceParameters)
        {
            _mediaObject.IceUfrag = new IceUfrag { Ufrag = iceParameters.UsernameFragment };
            _mediaObject.IcePwd = new IcePwd { Password = iceParameters.Password };
        }
    }
}
