using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IBlob
    {
        int Size { get; }

        string Type { get; }

        Task<byte[]> ArrayBuffer();

        IBlob Slice(int start = 0, int end = 0, string contentType = "");

        ////IReadableStream Stream();

        Task<string> Text();

        object GetNativeObject();
    }
}
