using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public abstract class BlobStream : Stream
    {
        public virtual Task WriteAsync(IBlob blob , CancellationToken cancellationToken = default(CancellationToken))
        {
            throw null;
        }

        public virtual Task<int> ReadAsync(IBlob blob, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw null;
        }

    }
}
