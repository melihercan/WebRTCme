using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WebRTCme.Middleware
{
    public class DataParameters
    {
        [Key]
        public uint Id { get; set; }

        public uint SequenceNo { get; set; }

        public DataFromType From { get; set; }

        public string PeerUserName { get; set; }

        public string PeerUserNameTextColor { get; set; }

        public string Time { get; set; }

        public object Object { get; set; }

        ///// TODO: HOW TO ADVANCE PROGRESS BAR DURING FILE??? UPLOAD/DOWNLOAD
    }
}
