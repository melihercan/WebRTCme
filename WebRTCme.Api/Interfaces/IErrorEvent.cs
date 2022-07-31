using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IErrorEvent : IDisposable // INativeObject
    {
        string Message { get; }

        string FileName { get; }

        int LineNo { get; }

        int ColNo { get; }

        ////object Error { get; }
    }
}
