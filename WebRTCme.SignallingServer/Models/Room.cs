using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.Models
{
    public class Room
    {
        // Unique group name per hub. Set from roomName parameter.
        public string GroupName { get; set; }

        public RTCIceServer[] IceServers { get; set; }

        public string InitiatiorUserName { get; set; }

        //public IEnumerable<Client> ParticipantClients { get; set; }
        public IEnumerable<Client> Clients { get; set; }
    }
}
