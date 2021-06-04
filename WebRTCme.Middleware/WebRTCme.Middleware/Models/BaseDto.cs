using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware.Enums;

namespace WebRTCme.Middleware.Models
{
    internal class BaseDto
    {
        public ulong Cookies { get; init; }

        public DtoObjectType DtoObjectType { get; init; }
    }
}
