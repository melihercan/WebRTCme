using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.Models
{
    public class Room
    {
        public string RoomId { get; set; }

        public Client InitiatiorClient { get; set; }

        public IEnumerable<Client> ParticipantClients { get; set; }
    }
}
