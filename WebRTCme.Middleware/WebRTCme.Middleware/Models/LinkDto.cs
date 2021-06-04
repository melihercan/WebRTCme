using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware.Models
{
    [Serializable]
    internal class LinkDto
    {
        public string Url { get; init; }
        public string TextToDisplay { get; init; }
    }
}
