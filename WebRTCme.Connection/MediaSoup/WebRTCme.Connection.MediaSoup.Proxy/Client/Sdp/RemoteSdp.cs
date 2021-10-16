using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilme.SdpTransform;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp
{
    class RemoteSdp
    {
        IceParameters _iceParameters;
        IceCandidate[] _iceCandidates;
        DtlsParameters _dtlsParameters;
        SctpParameters _sctpParameters;
        PlainRtpParameters _plainRtpParameters;
        bool? _planB;
        List<MediaSection> _mediaSections = new();
        Dictionary<Mid, int> _midToIndex = new();
        Mid _firstMid;

        Utilme.SdpTransform.Sdp _sdp;


        public RemoteSdp(IceParameters iceParameters, IceCandidate[] iceCandidates, DtlsParameters dtlsParameters, 
            SctpParameters sctpParameters, PlainRtpParameters plainRtpParameters, bool? planB)
        {
            _iceParameters = iceParameters;
            _iceCandidates = iceCandidates;
            _dtlsParameters = dtlsParameters;
            _sctpParameters = sctpParameters;
            _plainRtpParameters = plainRtpParameters;
            _planB = planB;

            _sdp = new Utilme.SdpTransform.Sdp
            {
                ProtocolVersion = 0,
                Origin = new Origin
                {
                    UserName = "mediasoup-client",
                    SessionId = 10000,
                    SessionVersion = 0,
                    NetType = NetType.Internet,
                    AddrType = AddrType.Ip4,
                    UnicastAddress = "0.0.0.0"
                },
                SessionName = "-",
                Timings = new List<Timing>() { new Timing 
                { 
                    StartTime = new DateTime(1900, 1, 1), 
                    StopTime = new DateTime(1900, 1, 1) } 
                },
                Attributes = new(),
                MediaDescriptions = new List<MediaDescription>()
            };

            if (iceParameters is not null && iceParameters.IceLite.HasValue)
                _sdp.Attributes.IceLite = true;

            if (dtlsParameters is not null)
            {
                _sdp.Attributes.MsidSemantic = new MsidSemantic 
                { 
                    Token = MsidSemantic.WebRtcMediaStreamToken, 
                    IdList = new string[] { MsidSemantic.AllIds } 
                };

                var numFingerprints = dtlsParameters.Fingerprints.Length;
                var fingerprintStr = dtlsParameters.Fingerprints[numFingerprints - 1].Algorithm + " " +
                    dtlsParameters.Fingerprints[numFingerprints - 1].Value;
                _sdp.Attributes.Fingerprint = fingerprintStr.ToFingerprint();

                _sdp.Attributes.Group = new Group 
                {
                    Semantics = GroupSemantics.Bundle,
                    SemanticsExtensions = new string[] { }
                };
            }

            if (plainRtpParameters is not null)
            {
                _sdp.Origin.UnicastAddress = plainRtpParameters.Ip;
                _sdp.Origin.AddrType = plainRtpParameters.IpVersion;
            }

        }

        public void UpdateIceParameters(IceParameters iceParameters)
        {
            _iceParameters = iceParameters;
            _sdp.Attributes.IceLite = iceParameters.IceLite.HasValue ? true : null;
            foreach (var mediaSection in _mediaSections)
                mediaSection.SetIceParameters(iceParameters);
        }

        public void UpdateDtlsRole(DtlsRole role)
        {
            _dtlsParameters.Role = role;
            foreach (var mediaSection in _mediaSections)
                mediaSection.SetDtlsRole(role);
        }

        public string GetSdp()
        {
            _sdp.Origin.SessionVersion++;
            return _sdp.ToText();
        }


        public MediaSectionIdx GetNextMediaSectionIdx()
        {
            // If a closed media section is found, return its index.
            for (var idx = 0; idx < _mediaSections.Count; ++idx)
            {
                var mediaSection = _mediaSections[idx];
                if (mediaSection.Closed)
                    return new MediaSectionIdx { Idx = idx, ReuseMid = mediaSection.Mid };
            }

            // If no closed media section is found, return next one.
            return new MediaSectionIdx { Idx = _mediaSections.Count };
        }

        public void Send(MediaObject offerMediaObject, Mid reuseMid, RtpParameters offerRtpParameters, 
            RtpParameters answerRtpParameters, ProducerCodecOptions codecOptions, bool extmapAllowMixed)
        {
            var mediaSection = new AnswerMediaSection(
                _iceParameters,
				_iceCandidates,
				_dtlsParameters,
                null,
				_plainRtpParameters,
				_planB ?? false,
				offerMediaObject,
				offerRtpParameters,
				answerRtpParameters,
				codecOptions,
				extmapAllowMixed);

		    // Unified-Plan with closed media section replacement.
		    if (reuseMid is not null)
		    {
			    ReplaceMediaSection(mediaSection, reuseMid);
            }
		    // Unified-Plan or Plan-B with different media kind.
		    else if (!_midToIndex.ContainsKey(mediaSection.Mid))
            {
                AddMediaSection(mediaSection);
            }
		    // Plan-B with same media kind.
		    else
            {
                ReplaceMediaSection(mediaSection);
            }
        }

        public void CloseMediaSection(string midId)
        {
            Mid mid = new Mid { Id = midId };
            if (!_midToIndex.ContainsKey(mid))
                throw new Exception($"no media section found for mid: {mid}");

            var idx = _midToIndex[mid];
            var mediaSection = _mediaSections[idx];

            // NOTE: Closing the first m section is a pain since it invalidates the
            // bundled transport, so let's avoid it.
            if (mid == _firstMid)
            {
                Console.WriteLine($"closeMediaSection() | cannot close first media section, disabling it instead {mid.Id}");
                DisableMediaSection(mid);
                return;
            }

            mediaSection.Close();

            // Regenerate BUNDLE mids.
            RegenerateBundleMids();

        }

        public void SendSctpAssociation(MediaObject offerMediaObject)
        {
            var mediaSection = new AnswerMediaSection(
                _iceParameters,
                _iceCandidates,
                _dtlsParameters,
                _sctpParameters,
                _plainRtpParameters,
                false,
                offerMediaObject,
                null,
                null,
                null,
                false);

		    AddMediaSection(mediaSection);
        }


        void AddMediaSection(MediaSection newMediaSection)
        {
            if (_firstMid is null)
                _firstMid = newMediaSection.Mid;

            // Add to the vector.
            _mediaSections.Add(newMediaSection);

            // Add to the map.
            _midToIndex.Add(newMediaSection.Mid, _mediaSections.Count - 1);

            // Add to the SDP object.
            _sdp.MediaDescriptions.Add(newMediaSection.MediaObject.MediaDescription);

            // Regenerate BUNDLE mids.
            RegenerateBundleMids();
        }

        void ReplaceMediaSection(MediaSection newMediaSection, Mid reuseMid = null)
        {
            // Store it in the map.
            if (reuseMid is not null)
            {
                if (!_midToIndex.ContainsKey(reuseMid))
                    throw new Exception($"no media section found for reuseMid: {reuseMid}");

                var idx = _midToIndex[reuseMid];
                var oldMediaSection = _mediaSections[idx];

                // Replace the index in the vector with the new media section.
                _mediaSections[idx] = newMediaSection;

                // Update the map.
                _midToIndex.Remove(oldMediaSection.Mid);
                _midToIndex.Add(newMediaSection.Mid, idx);

                // Update the SDP object.
                _sdp.MediaDescriptions[idx] = newMediaSection.MediaObject.MediaDescription;

                // Regenerate BUNDLE mids.
                RegenerateBundleMids();
            }
            else
            {
                if (!_midToIndex.ContainsKey(newMediaSection.Mid))
                    throw new Exception($"no media section found for mid: {newMediaSection.Mid}");

                var idx = _midToIndex[newMediaSection.Mid];

                // Replace the index in the vector with the new media section.
                _mediaSections[idx] = newMediaSection;

                // Update the SDP object.
                _sdp.MediaDescriptions[idx] = newMediaSection.MediaObject.MediaDescription;
            }

        }

        void DisableMediaSection(Mid mid)
        {
            if (!_midToIndex.ContainsKey(mid))
                throw new Exception($"no media section found for mid: {mid}");
            
            var idx = _midToIndex[mid];
            var mediaSection = _mediaSections[idx];
            mediaSection.Disable();
        }

        void RegenerateBundleMids()
        {
            if (_dtlsParameters is null)
                return;

            _sdp.Attributes.Group.SemanticsExtensions = _mediaSections
                .Where(mediaSection => !mediaSection.Closed)
                .Select(mediaSection => mediaSection.Mid.Id).ToArray();
        }

        public void Receive(string midId, MediaKind kind, RtpParameters offerRtpParameters, 
            string streamId, string trackId)
        {
            Mid mid = new Mid { Id = midId };
            OfferMediaSection mediaSection = null;

            if (_midToIndex.ContainsKey(mid))
            {
                var idx = _midToIndex[mid];
                mediaSection = _mediaSections[idx] as OfferMediaSection;
            }

            if (mediaSection is null)
            {
                mediaSection = new OfferMediaSection(
                    _iceParameters,
                    _iceCandidates,
                    _dtlsParameters,
                    null,
                    _plainRtpParameters,
                    _planB ?? false,
                    mid,
                    kind,
                    offerRtpParameters,
                    streamId,
                    trackId,
                    false);
                // Let's try to recycle a closed media section (if any).
                // NOTE: Yes, we can recycle a closed m=audio section with a new m=video.
               var oldMediaSection = _mediaSections.FirstOrDefault(m => m.Closed);
                if (oldMediaSection is not null)
                {
                    ReplaceMediaSection(mediaSection, oldMediaSection.Mid);
                }
                else
                {
                    AddMediaSection(mediaSection);
                }
            }
            // Plan-B.
            else
            {
                mediaSection.PlanBReceive(offerRtpParameters, streamId, trackId);
                ReplaceMediaSection(mediaSection);
            }
        }

        public void ReceiveSctpAssociation(bool oldDataChannelSpec = false)
        {
            var mediaSection = new OfferMediaSection(
                _iceParameters,
                _iceCandidates,
                _dtlsParameters,
                _sctpParameters,
                _plainRtpParameters,
                _planB ?? false,
                new Mid() { Id = "datachannel" },
                MediaKind.Application,
                null,
                null,
                null,
                oldDataChannelSpec);
            AddMediaSection(mediaSection);
        }
    }
}
