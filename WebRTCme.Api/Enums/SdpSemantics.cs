using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]

    public enum SdpSemantics
    {
        [EnumMember(Value = "plan-b")]
        PlanB,

        [EnumMember(Value = "unified-plan")]
        UnifiedPlan
    }
}
