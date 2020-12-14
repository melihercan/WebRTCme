using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal static class ModelExtensions
    {
        public static Webrtc.MediaConstraints ToNative(this MediaTrackConstraints constraints)
        {
            var mandatoryDictionary = new Dictionary<string, string>();
            var optionalDictionary = new Dictionary<string, string>();

            mandatoryDictionary.Add("googEchoCancellation", constraints.EchoCancellation.ToBool() ? "true" : "false");
            mandatoryDictionary.Add("googAutoGainControl", constraints.AutoGainControl.ToBool() ? "true" : "false");
            mandatoryDictionary.Add("googHighpassFilter", "false");
            mandatoryDictionary.Add("googNoiseSuppression", constraints.NoiseSuppression.ToBool() ? "true" : "false");

            var mandatory = mandatoryDictionary.Select(
                pair => new Webrtc.MediaConstraints.KeyValuePair(pair.Key, pair.Value)).ToList();
            var optional = optionalDictionary.Select
                (pair => new Webrtc.MediaConstraints.KeyValuePair(pair.Key, pair.Value)).ToList();

            var nativeConstrains = new Webrtc.MediaConstraints
            {
                Mandatory = mandatory,
                Optional = optional
            };

            return nativeConstrains;
        }
    }
}
