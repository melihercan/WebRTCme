using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Utilme.SdpTransform;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp
{
    abstract class MediaSection
    {
        protected readonly MediaObject _mediaObject;
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

            _mediaObject = new()
            {
                MediaDescription = new()
                {
                    Attributes = new()
                }
            };

            if (iceParameters is not null)
            {
                SetIceParameters(iceParameters);
            }
            
            if (iceCandidates is not null)
            {
                _mediaObject.MediaDescription.Attributes.Candidates = new List<Candidate>();
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
                    _mediaObject.MediaDescription.Attributes.Candidates.Add(candidate);
                }
            }

            _mediaObject.MediaDescription.Attributes.EndOfCandidates = true;
            _mediaObject.MediaDescription.Attributes.IceOptions = 
                new IceOptions { Tags = new string[] { "renomination" } };

            if (dtlsParameters is not null)
            {
                SetDtlsRole(dtlsParameters.Role);
            }
        }

        public MediaObject MediaObject => _mediaObject;
        public Mid Mid => _mediaObject.MediaDescription.Attributes.Mid;
        public bool Closed => _mediaObject.MediaDescription.Port == 0;

        public virtual void SetDtlsRole(DtlsRole? dtlsRole) => throw new NotImplementedException();

        public void SetIceParameters(IceParameters iceParameters)
        {
            _mediaObject.MediaDescription.Attributes.IceUfrag = new IceUfrag { Ufrag = iceParameters.UsernameFragment };
            _mediaObject.MediaDescription.Attributes.IcePwd = new IcePwd { Password = iceParameters.Password };
        }



        public void Disable()
        {
            _mediaObject.MediaDescription.Attributes.SendRecv = null;
            _mediaObject.MediaDescription.Attributes.SendOnly = null;
            _mediaObject.MediaDescription.Attributes.RecvOnly = null;

            _mediaObject.MediaDescription.Attributes.Extmaps?.Clear();
            _mediaObject.MediaDescription.Attributes.Extmaps = null;
            _mediaObject.MediaDescription.Attributes.Ssrcs?.Clear();
            _mediaObject.MediaDescription.Attributes.Ssrcs = null;
            _mediaObject.MediaDescription.Attributes.SsrcGroups?.Clear();;
            _mediaObject.MediaDescription.Attributes.SsrcGroups = null;
            _mediaObject.MediaDescription.Attributes.Simulcast = null;
            _mediaObject.Simulcast03 = null;
            _mediaObject.MediaDescription.Attributes.Rids?.Clear();
            _mediaObject.MediaDescription.Attributes.Rids = null;
        }

        public void Close()
        {
            _mediaObject.MediaDescription.Attributes.SendRecv = null;
            _mediaObject.MediaDescription.Attributes.SendOnly = null;
            _mediaObject.MediaDescription.Attributes.RecvOnly = null;
            _mediaObject.MediaDescription.Port = 0;

            _mediaObject.MediaDescription.Attributes.Extmaps?.Clear();
            _mediaObject.MediaDescription.Attributes.Extmaps = null;
            _mediaObject.MediaDescription.Attributes.Ssrcs?.Clear();
            _mediaObject.MediaDescription.Attributes.Ssrcs = null;
            _mediaObject.MediaDescription.Attributes.SsrcGroups?.Clear(); ;
            _mediaObject.MediaDescription.Attributes.SsrcGroups = null;
            _mediaObject.MediaDescription.Attributes.Simulcast = null;
            _mediaObject.Simulcast03 = null;
            _mediaObject.MediaDescription.Attributes.Rids?.Clear();
            _mediaObject.MediaDescription.Attributes.Rids = null;
            _mediaObject.MediaDescription.Attributes.ExtmapAllowMixed = null;
        }

        protected string GetCodecName(RtpCodecParameters codec)
        {
            var match = Regex.Match(codec.MimeType, "^(audio|video)/(.+)");
            if (!match.Success || match.Groups.Count != 3)
                throw new Exception("invalid codec.mimeType");

	        return match.Groups[2].Value;
        }


        //protected string CodecParametersToFmtpValue(Dictionary<string, string> codecParameters, int payloadType)
        //{


        //    List<string> values = new();
            
        //    if (codecParameters.Stereo.HasValue)
        //        values.Add($"stereo={((bool)codecParameters.Stereo ? 1 : 0)}");
        //    if (codecParameters.UseInBandFec.HasValue)
        //        values.Add($"useinbandfec{((bool)codecParameters.UseInBandFec ? 1 : 0)}");
        //    if (codecParameters.UsedTx.HasValue)
        //        values.Add($"usedtx={((bool)codecParameters.UsedTx ? 1 : 0)}");
        //    if (codecParameters.MaxAverageBitrate.HasValue)
        //        values.Add($"maxplaybackrate={(int)codecParameters.MaxPlaybackRate}");
        //    if (codecParameters.MaxAverageBitrate.HasValue)
        //        values.Add($"maxaveragebitrate={(int)codecParameters.MaxAverageBitrate}");
        //    if (codecParameters.Ptime.HasValue)
        //        values.Add($"ptime={(int)codecParameters.Ptime}");
        //    if (codecParameters.XGoogleStartBitrate.HasValue)
        //        values.Add($"x-google-start-bitrate={(int)codecParameters.XGoogleStartBitrate}");
        //    if (codecParameters.XGoogleMaxBitrate.HasValue)
        //        values.Add($"x-google-max-bitrate={(int)codecParameters.XGoogleMaxBitrate}");
        //    if (codecParameters.XGoogleMinBitrate.HasValue)
        //        values.Add($"x-google-min-bitrate={(int)codecParameters.XGoogleMinBitrate}");

        //    return string.Join(";", values.ToArray());
        //}
    }
}
