using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class MessageEvent : NativeBase<object>, IMessageEvent
    {
        public MessageEvent(object data) : base(data) { }

        public object Data => NativeObject;

        public string Origin => throw new NotImplementedException();

        public string LastEventId => throw new NotImplementedException();

        public object Source => throw new NotImplementedException();

        public object[] Ports => throw new NotImplementedException();
    }
}
