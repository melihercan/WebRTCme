using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IRTCIdentityAssertion
    {
        string Idp { get; set; }
        
        string Name { get; set; }
    }
}
