using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IRTCIdentityAssertion : INativeObject
    {
        string Idp { get; set; }
        
        string Name { get; set; }
    }
}
