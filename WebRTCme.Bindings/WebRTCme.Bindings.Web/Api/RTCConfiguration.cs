using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRtcBindingsWeb.Extensions;
using WebRTCme;
using System.Linq;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCConfiguration : ApiBase, IRTCConfiguration
    {
        public static IRTCConfiguration Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefConfiguration) =>
            new RTCConfiguration(jsRuntime, jsObjectRefConfiguration);

        private RTCConfiguration(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public RTCBundlePolicy? BundlePolicy 
        {
            get => GetNativeProperty<RTCBundlePolicy>("bundlePolicy");
            set => SetNativeProperty("bundlePolicy", value);
        }
        
        public IRTCCertificate[] Certificates 
        { 
            get
            {
                var jsObjectRefCertificates = JsRuntime.GetJsPropertyObjectRef(NativeObject, "certificates");
                var jsObjectRefCertificateArray = JsRuntime.GetJsPropertyArray(jsObjectRefCertificates);
                var certificates = jsObjectRefCertificateArray
                    .Select(jsObjectRef => RTCCertificate.Create(JsRuntime, jsObjectRef))
                    .ToArray();
                JsRuntime.DeleteJsObjectRef(jsObjectRefCertificates.JsObjectRefId);
                return certificates;

            }
            set => SetNativeProperty("certificates", value.Select(certificate => certificate.NativeObject).ToArray());
        }
        
        public byte? IceCandidatePoolSize 
        {
            get => GetNativeProperty<byte?>("iceCandidatePoolSize");
            set => SetNativeProperty("iceCandidatePoolSize", value);
        }
        
        public RTCIceServer[] IceServers 
        { 
            get
            {
                var iceServers = new List<RTCIceServer>();
                var jsObjectRefIceServers = JsRuntime.GetJsPropertyObjectRef(NativeObject, "iceServers");
                var jsObjectRefIceServerArray = JsRuntime.GetJsPropertyArray(jsObjectRefIceServers);
                foreach (var jsObjectRefIceServer in jsObjectRefIceServerArray)
                {
                    iceServers.Add(JsRuntime.GetJsPropertyValue<RTCIceServer>(jsObjectRefIceServer, null));
                    JsRuntime.DeleteJsObjectRef(jsObjectRefIceServer.JsObjectRefId);
                }
                JsRuntime.DeleteJsObjectRef(jsObjectRefIceServers.JsObjectRefId);
                return iceServers.ToArray();
            }
            set => SetNativeProperty("iceServers", value);
        }
        
        public RTCIceTransportPolicy? IceTransportPolicy 
        {
            get => GetNativeProperty<RTCIceTransportPolicy?>("iceTransportPolicy");
            set => SetNativeProperty("iceTransportPolicy", value);
        }
        
        public string PeerIdentity 
        {
            get => GetNativeProperty<string>("peerIdentity");
            set => SetNativeProperty("peerIdentity", value);
        }
        
        public RTCRtcpMuxPolicy? RtcpMuxPolicy 
        {
            get => GetNativeProperty<RTCRtcpMuxPolicy?>("rtcpMuxPolicy");
            set => SetNativeProperty("rtcpMuxPolicy", value);
        }
    }
}
