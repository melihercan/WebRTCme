using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal abstract class ApiBase : NSObject, INativeObject
    {
        protected ApiBase() { }

        protected ApiBase(object nativeObject) => NativeObject = nativeObject;

        public object NativeObject { get; set; }
    }
}
