using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class MethodName
    {
        public const string GetRouterRtpCapabilities = "getRouterRtpCapabilities";
        public const string CreateWebRtcTransport = "createWebRtcTransport";
        public const string Join = "join";
        public const string NewConsumer = "newConsumer";
        public const string NewDataConsumer = "newDataConsumer";
        public const string ConnectWebRtcTransport = "connectWebRtcTransport";
        public const string Produce = "produce";
        public const string PauseProducer = "pauseProducer";
        public const string ResumeProducer = "resumeProducer";
        public const string ProduceData = "produceData";
        public const string PauseConsumer = "pauseConsumer";
        public const string ResumeConsumer = "resumeConsumer";
        public const string NewPeer = "newPeer";

        // Server notifications. Everything the room can send is listed so an unrecognised
        // method really means unrecognised, rather than merely unhandled.
        public const string PeerClosed = "peerClosed";
        public const string PeerDisplayNameChanged = "peerDisplayNameChanged";
        public const string ConsumerClosed = "consumerClosed";
        public const string ConsumerPaused = "consumerPaused";
        public const string ConsumerResumed = "consumerResumed";
        public const string ConsumerScore = "consumerScore";
        public const string ConsumerLayersChanged = "consumerLayersChanged";
        public const string DataConsumerClosed = "dataConsumerClosed";
        public const string ActiveSpeaker = "activeSpeaker";
        public const string DownlinkBwe = "downlinkBwe";
        public const string MediasoupVersion = "mediasoupVersion";
        public const string SpeakingPeers = "speakingPeers";
        public const string ProducerScore = "producerScore";
        public const string GetTransportStats = "getTransportStats";
        public const string GetProducerStats = "getProducerStats";
        public const string GetConsumerStats = "getConsumerStats";

    }
}
