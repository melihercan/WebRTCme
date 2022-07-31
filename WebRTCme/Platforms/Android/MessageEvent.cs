using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class MessageEvent : NativeBase<object>, IMessageEvent
    {
        public static IMessageEvent Create(object data) =>
            new MessageEvent(data);

        public MessageEvent(object data) : base(data) { }

        public object Data => NativeObject;

        public string Origin => throw new NotImplementedException();

        public string LastEventId => throw new NotImplementedException();

        public object Source => throw new NotImplementedException();

        public object[] Ports => throw new NotImplementedException();
    }
}
