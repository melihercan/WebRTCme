using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IErrorEventAsync
    {
        public Task<string> Message { get; }

        public Task<string> FileName { get; }

        public Task<int?> LineNo { get; }

        public Task<int?> ColNo { get; }

        ////public object Error { get; }
    }
}
