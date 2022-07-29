using Java.Security.Spec;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Android
{
    internal abstract class ApiBase : Java.Lang.Object, INativeObject
    {
        protected ApiBase() { }

        protected ApiBase(object nativeObject)
        {
            NativeObject = nativeObject;
        }

        public object NativeObject { get; set; }
    }
}
