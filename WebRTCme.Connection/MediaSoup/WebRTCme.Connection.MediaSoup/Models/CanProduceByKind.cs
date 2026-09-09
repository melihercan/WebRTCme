using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class CanProduceByKind
    {
        public bool Audio { get; set; }
        public bool Video { get; set; }

        public bool this[MediaKind kind]
        {
            get
            {
                var ret = kind switch
                {
                    MediaKind.Audio => Audio,
                    MediaKind.Video => Video,
                    _ => false,
                };
                return ret;
            }
            set
            {
                switch(kind)
                {
                    case MediaKind.Audio:
                        Audio = value;
                        break;
                    case MediaKind.Video:
                        Video = value;
                        break;
                }
            }
        }
    }
}
