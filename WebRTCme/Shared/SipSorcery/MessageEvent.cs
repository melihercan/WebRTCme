using WebRTCme.Shared.SipSorcery.Custom;

namespace WebRTCme.Shared.SipSorcery
{
    internal class MessageEvent : NativeBase<object>, IMessageEvent
    {
        public MessageEvent(object data) : base(data) { }

        public object Data => NativeObject;

        public string Origin => throw new NotImplementedException();

        public string LastEventId => throw new NotImplementedException();

        public object Source => throw new NotImplementedException();

        public object[] Ports => throw new NotImplementedException();
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

}
