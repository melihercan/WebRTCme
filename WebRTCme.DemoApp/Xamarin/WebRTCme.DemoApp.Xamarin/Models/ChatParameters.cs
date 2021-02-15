using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.DemoApp.Xamarin.Enums;
using Xamarin.Forms;

namespace WebRTCme.DemoApp.Xamarin.Models
{
    public class ChatParameters
    {
        public ChatMessageType ChatMessageType { get; set; }

        public string PeerUserName { get; set; }

        public Color PeerUserNameTextColor { get; set; } 

        public string Time { get; set; }

        public string Message { get; set; }
    }
}
