using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Bindings.Native
{
    public class Dummy
    {
        //[DllImport(Registry.NativeLib)]
        //public static extern void DummyXxx();



        [DllImport(Registry.InteropLib)]
        public static extern IntPtr CreateBuiltinAudioEncoderFactory();

    }
}
