using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware.Models
{
    [Serializable]
    internal class MessageDto : BaseDto
    {
        public string Text { get; init; }
    }
}
