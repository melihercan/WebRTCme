using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Maui.MacCatalyst.Custom;

namespace WebRTCme.Bindings.Maui.MacCatalyst.Api
{
    internal class ErrorEvent : NativeBase<object>, IErrorEvent
    {
        public ErrorEvent(object message) : base(message) { }

        public string Message => NativeObject.ToString();

        public string FileName => throw new NotImplementedException();

        public int LineNo => throw new NotImplementedException();

        public int ColNo => throw new NotImplementedException();

        public void Dispose()
        {
        }
    }
}
