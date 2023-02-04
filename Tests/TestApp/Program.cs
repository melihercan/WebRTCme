using System;
using Utilme.SdpTransform;

/****
e=j.doe@example.com d.ercan@gmail.com m.ercan@example.com k.ercan@yahoo.com jd.doe@example.com s.ercan@gmail.com x.y@xxx.com
e=j.doe@example.com (Jane Doe) d.ercan@gmail.com m.ercan@example.com (Melik) k.ercan@yahoo.com Jane Doe <j.doe@example.com> s.ercan@gmail.com x.y@xxx.com (a b)
p=+1 617 555-6011 +90 551 205 85 10 (Melik Ercan) +90 534 396 67 67

****/
namespace TestApp
{
    class Program
    {
        static string SdpText =
@"
v=0
o=- 5723307564613334900 2 IN IP4 127.0.0.1
s=-
i=A class on computer networking
u=http://www.cs.princeton.edu/
e=j.doe@example.com (Jane Doe) d.ercan@gmail.com m.ercan@example.com (Melik) k.ercan@yahoo.com Jane Doe <j.doe@example.com> s.ercan@gmail.com x.y@xxx.com (a b)
p=+1 617 555-6011 +90 551 205 85 10 (Melik Ercan)  +34 56-5456 +90 534 396 67 67 Derya Ercan <+90 123-456-679>
c=IN IP4 0.0.0.0
b=AS:64
b=RS:800
b=RR:2400
t=0 0
t=3837838042 3837838342
r=7d 1h 0 25h
z=2882844526 -1h 2898848070 0
k=base64:gAefQzo4jeI97kV
a=group:BUNDLE 0 1
a=extmap-allow-mixed
a=msid-semantic: WMS
m=audio 9 UDP/TLS/RTP/SAVPF 111 103 104 9 0 8 106 105 13 110 112 113 126
a=rtcp:9 IN IP4 0.0.0.0
a=ice-ufrag:or3m
a=ice-pwd:vdjbCwR0KpR1FK2WVIVigCJM
a=ice-options:trickle
a=fingerprint:sha-256 02:36:77:82:CA:C8:CA:4B:55:2E:AC:C3:7D:33:97:F0:48:E6:83:ED:D4:BD:8B:CC:C0:56:46:8A:84:D7:4D:AD
a=setup:actpass
a=mid:0
a=extmap:1 urn:ietf:params:rtp-hdrext:ssrc-audio-level
a=extmap:2 http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time
a=extmap:3 http://www.ietf.org/id/draft-holmer-rmcat-transport-wide-cc-extensions-01
a=extmap:4 urn:ietf:params:rtp-hdrext:sdes:mid
a=extmap:5 urn:ietf:params:rtp-hdrext:sdes:rtp-stream-id
a=extmap:6 urn:ietf:params:rtp-hdrext:sdes:repaired-rtp-stream-id
a=sendrecv
a=msid:- 29def635-b438-4750-b856-c1db421d1e53
a=rtcp-mux
a=rtpmap:111 opus/48000/2
a=rtcp-fb:111 transport-cc
a=fmtp:111 minptime=10;useinbandfec=1
a=rtpmap:103 ISAC/16000
a=rtpmap:104 ISAC/32000
a=rtpmap:9 G722/8000
a=rtpmap:0 PCMU/8000
a=rtpmap:8 PCMA/8000
a=rtpmap:106 CN/32000
a=rtpmap:105 CN/16000
a=rtpmap:13 CN/8000
a=rtpmap:110 telephone-event/48000
a=rtpmap:112 telephone-event/32000
a=rtpmap:113 telephone-event/16000
a=rtpmap:126 telephone-event/8000
a=ssrc:53605734 cname:4hecBhTMXW6zRMlw
a=ssrc:53605734 msid:- 29def635-b438-4750-b856-c1db421d1e53
a=ssrc:53605734 mslabel:-
a=ssrc:53605734 label:29def635-b438-4750-b856-c1db421d1e53
m=video 9 UDP/TLS/RTP/SAVPF 96 97 98 99 100 101 102 121 127 120 125 107 108 109 35 36 124 119 123 118 114 115 116
c=IN IP4 0.0.0.0
a=rtcp:9 IN IP4 0.0.0.0
a=ice-ufrag:or3m
a=ice-pwd:vdjbCwR0KpR1FK2WVIVigCJM
a=ice-options:trickle
a=fingerprint:sha-256 02:36:77:82:CA:C8:CA:4B:55:2E:AC:C3:7D:33:97:F0:48:E6:83:ED:D4:BD:8B:CC:C0:56:46:8A:84:D7:4D:AD
a=setup:actpass
a=mid:1
a=extmap:14 urn:ietf:params:rtp-hdrext:toffset
a=extmap:2 http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time
a=extmap:13 urn:3gpp:video-orientation
a=extmap:3 http://www.ietf.org/id/draft-holmer-rmcat-transport-wide-cc-extensions-01
a=extmap:12 http://www.webrtc.org/experiments/rtp-hdrext/playout-delay
a=extmap:11 http://www.webrtc.org/experiments/rtp-hdrext/video-content-type
a=extmap:7 http://www.webrtc.org/experiments/rtp-hdrext/video-timing
a=extmap:8 http://www.webrtc.org/experiments/rtp-hdrext/color-space
a=extmap:4 urn:ietf:params:rtp-hdrext:sdes:mid
a=extmap:5 urn:ietf:params:rtp-hdrext:sdes:rtp-stream-id
a=extmap:6 urn:ietf:params:rtp-hdrext:sdes:repaired-rtp-stream-id
a=sendrecv
a=msid:- 1082e3a2-c7ae-40da-a43d-35f52283c889
a=rtcp-mux
a=rtcp-rsize
a=rtpmap:96 VP8/90000
a=rtcp-fb:96 goog-remb
a=rtcp-fb:96 transport-cc
a=rtcp-fb:96 ccm fir
a=rtcp-fb:96 nack
a=rtcp-fb:96 nack pli
a=rtpmap:97 rtx/90000
a=fmtp:97 apt=96
a=rtpmap:98 VP9/90000
a=rtcp-fb:98 goog-remb
a=rtcp-fb:98 transport-cc
a=rtcp-fb:98 ccm fir
a=rtcp-fb:98 nack
a=rtcp-fb:98 nack pli
a=fmtp:98 profile-id=0
a=rtpmap:99 rtx/90000
a=fmtp:99 apt=98
a=rtpmap:100 VP9/90000
a=rtcp-fb:100 goog-remb
a=rtcp-fb:100 transport-cc
a=rtcp-fb:100 ccm fir
a=rtcp-fb:100 nack
a=rtcp-fb:100 nack pli
a=fmtp:100 profile-id=2
a=rtpmap:101 rtx/90000
a=fmtp:101 apt=100
a=rtpmap:102 H264/90000
a=rtcp-fb:102 goog-remb
a=rtcp-fb:102 transport-cc
a=rtcp-fb:102 ccm fir
a=rtcp-fb:102 nack
a=rtcp-fb:102 nack pli
a=fmtp:102 level-asymmetry-allowed=1;packetization-mode=1;profile-level-id=42001f
a=rtpmap:121 rtx/90000
a=fmtp:121 apt=102
a=rtpmap:127 H264/90000
a=rtcp-fb:127 goog-remb
a=rtcp-fb:127 transport-cc
a=rtcp-fb:127 ccm fir
a=rtcp-fb:127 nack
a=rtcp-fb:127 nack pli
a=fmtp:127 level-asymmetry-allowed=1;packetization-mode=0;profile-level-id=42001f
a=rtpmap:120 rtx/90000
a=fmtp:120 apt=127
a=rtpmap:125 H264/90000
a=rtcp-fb:125 goog-remb
a=rtcp-fb:125 transport-cc
a=rtcp-fb:125 ccm fir
a=rtcp-fb:125 nack
a=rtcp-fb:125 nack pli
a=fmtp:125 level-asymmetry-allowed=1;packetization-mode=1;profile-level-id=42e01f
a=rtpmap:107 rtx/90000
a=fmtp:107 apt=125
a=rtpmap:108 H264/90000
a=rtcp-fb:108 goog-remb
a=rtcp-fb:108 transport-cc
a=rtcp-fb:108 ccm fir
a=rtcp-fb:108 nack
a=rtcp-fb:108 nack pli
a=fmtp:108 level-asymmetry-allowed=1;packetization-mode=0;profile-level-id=42e01f
a=rtpmap:109 rtx/90000
a=fmtp:109 apt=108
a=rtpmap:35 AV1X/90000
a=rtcp-fb:35 goog-remb
a=rtcp-fb:35 transport-cc
a=rtcp-fb:35 ccm fir
a=rtcp-fb:35 nack
a=rtcp-fb:35 nack pli
a=rtpmap:36 rtx/90000
a=fmtp:36 apt=35
a=rtpmap:124 H264/90000
a=rtcp-fb:124 goog-remb
a=rtcp-fb:124 transport-cc
a=rtcp-fb:124 ccm fir
a=rtcp-fb:124 nack
a=rtcp-fb:124 nack pli
a=fmtp:124 level-asymmetry-allowed=1;packetization-mode=1;profile-level-id=4d001f
a=rtpmap:119 rtx/90000
a=fmtp:119 apt=124
a=rtpmap:123 H264/90000
a=rtcp-fb:123 goog-remb
a=rtcp-fb:123 transport-cc
a=rtcp-fb:123 ccm fir
a=rtcp-fb:123 nack
a=rtcp-fb:123 nack pli
a=fmtp:123 level-asymmetry-allowed=1;packetization-mode=1;profile-level-id=64001f
a=rtpmap:118 rtx/90000
a=fmtp:118 apt=123
a=rtpmap:114 red/90000
a=rtpmap:115 rtx/90000
a=fmtp:115 apt=114
a=rtpmap:116 ulpfec/90000
a=ssrc-group:FID 1555266730 3235472342
a=ssrc:1555266730 cname:4hecBhTMXW6zRMlw
a=ssrc:1555266730 msid:- 1082e3a2-c7ae-40da-a43d-35f52283c889
a=ssrc:1555266730 mslabel:-
a=ssrc:1555266730 label:1082e3a2-c7ae-40da-a43d-35f52283c889
a=ssrc:3235472342 cname:4hecBhTMXW6zRMlw
a=ssrc:3235472342 msid:- 1082e3a2-c7ae-40da-a43d-35f52283c889
a=ssrc:3235472342 mslabel:-
a=ssrc:3235472342 label:1082e3a2-c7ae-40da-a43d-35f52283c889
";

        static void Main(string[] args)
        {

            Sdp sdp = SdpText.ToSdp();

            var text = sdp.ToText();
        }
    }
}
