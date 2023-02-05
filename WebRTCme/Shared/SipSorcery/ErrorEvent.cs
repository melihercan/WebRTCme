using WebRTCme.Shared.SipSorcery.Custom;

namespace WebRTCme.Shared.SipSorcery
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
