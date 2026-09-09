using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IDOMException
    {
        string Message { get; }

        string Name { get; }
    }
}
