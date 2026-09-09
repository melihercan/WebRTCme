using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class MediaRecorderParameters
    {
        public string FileName { get; set; }
        public int PeriodMs { get; set; }
        public IMediaStream MediaStream{get;set;}
        public MediaRecorderOptions MediaRecorderOptions { get; set; }
        public BlobStream BlobStream { get; set; }
        public IMediaRecorder MediaRecorder { get; set; }
    }
}
