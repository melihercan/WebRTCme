using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware.Enums;

namespace WebRTCme.Middleware.Models
{
    internal class BaseDto
    {
        public ulong Cookie { get; init; }

        public DataObjectType DtoObjectType { get; init; }
    }
}
