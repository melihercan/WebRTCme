using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.Models
{
    public class Room
    {

        public string RoomName { get; set; }

        // Unique group name per hub. Set from "turnServer".roomName parameter.
        public string GroupName { get; set; }

        public bool IsReserved { get; set; }

        public string AdminUserName { get; set; }

        public List<Client> Clients { get; set; }
    }
}
