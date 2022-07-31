using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCCertificate : IDisposable // INativeObject
    {
        ulong Expires { get; }
    }
}
