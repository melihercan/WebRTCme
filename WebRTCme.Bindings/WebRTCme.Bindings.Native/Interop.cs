using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Bindings.Native
{
    public class Interop
    {
        [DllImport(Registry.InteropLib)]
        public static extern IntPtr CreateBuiltinAudioEncoderFactory();

        [DllImport(Registry.InteropLib)]
        public static extern IntPtr CreateBuiltinVideoEncoderFactory();

        [DllImport(Registry.InteropLib)]
        public static extern IntPtr CreateBuiltinVideoDecoderFactory();

        //[DllImport(Registry.InteropLib)]
        //public static extern IntPtr ;


    }
}
