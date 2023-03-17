using System;
using Dissonance.Networking;
using Steamworks;

namespace Dissonance.Integrations.SteamworksP2P
{
    public class SteamworksP2PServer
        : BaseServer<SteamworksP2PServer, SteamworksP2PClient, CSteamID>
    {
        private readonly SteamworksP2PCommsNetwork _network;

        private readonly byte[] _receiveBuffer = new byte[4096];

        public SteamworksP2PServer(SteamworksP2PCommsNetwork network)
        {
            _network = network;
        }

        protected override void ReadMessages()
        {
            uint packetSize;
            CSteamID sender;
            while (SteamNetworking.ReadP2PPacket(_receiveBuffer, (uint)_receiveBuffer.Length, out packetSize, out sender, _network.P2PPacketChannelToServer))
                NetworkReceivedPacket(sender, new ArraySegment<byte>(_receiveBuffer, 0, (int)packetSize));
        }

        internal void PeerDisconnected(CSteamID conn)
        {
            ClientDisconnected(conn);
        }

        protected override void SendReliable(CSteamID connection, ArraySegment<byte> packet)
        {
            if (!_network.Send(connection, packet, EP2PSend.k_EP2PSendReliable, false))
                FatalError("Steam failed to send P2P Packet");
        }

        protected override void SendUnreliable(CSteamID connection, ArraySegment<byte> packet)
        {
            _network.Send(connection, packet, EP2PSend.k_EP2PSendUnreliableNoDelay, false);
        }
    }
}
