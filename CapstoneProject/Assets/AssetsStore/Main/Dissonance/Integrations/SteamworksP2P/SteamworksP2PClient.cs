﻿using System;
using System.Collections.Generic;
using Dissonance.Networking;
using Dissonance.Networking.Client;
using JetBrains.Annotations;
using Steamworks;

namespace Dissonance.Integrations.SteamworksP2P
{
    public class SteamworksP2PClient
        : BaseClient<SteamworksP2PServer, SteamworksP2PClient, CSteamID>
    {
        private readonly SteamworksP2PCommsNetwork _network;
        private readonly byte[] _receiveBuffer = new byte[4096];

        private readonly CSteamID _server;

        private readonly TimeSpan _handshakePeriod = TimeSpan.FromSeconds(23);
        private DateTime _previousHandshakeTime = DateTime.UtcNow;
        private byte[] _handshake;
        private readonly List<CSteamID> _peers = new List<CSteamID>();

        public SteamworksP2PClient([NotNull] SteamworksP2PCommsNetwork network, CSteamID serverId)
            : base(network)
        {
            _network = network;
            _server = serverId;
        }

        public override void Connect()
        {
            //We don't need to do any prep work to connect, and we assume we're already in a session as soon as this is called
            Connected();
        }

        public override void Disconnect()
        {
            //Close the Dissonance network channel to the server
            SteamNetworking.CloseP2PChannelWithUser(_server, _network.P2PPacketChannelToServer);

            base.Disconnect();
        }

        protected override void ReadMessages()
        {
            uint packetSize;
            CSteamID sender;
            while (SteamNetworking.ReadP2PPacket(_receiveBuffer, (uint)_receiveBuffer.Length, out packetSize, out sender, _network.P2PPacketChannelToClient))
            {
                var id = NetworkReceivedPacket(new ArraySegment<byte>(_receiveBuffer, 0, (int)packetSize));
                if (id.HasValue)
                    ReceiveHandshakeP2P(id.Value, sender);
            }
        }

        protected override void SendReliable(ArraySegment<byte> packet)
        {
            if (!_network.Send(_server, packet, EP2PSend.k_EP2PSendReliable, true))
                FatalError("Steam failed to send P2P Packet");
        }

        protected override void SendUnreliable(ArraySegment<byte> packet)
        {
            _network.Send(_server, packet, EP2PSend.k_EP2PSendUnreliableNoDelay, true);
        }

        public override ClientStatus Update()
        {
            var status = base.Update();

            //Periodically resend handshake messages
            if (status == ClientStatus.Ok && IsConnected && (DateTime.UtcNow - _previousHandshakeTime) > _handshakePeriod)
                SendHandshakes();

            return status;
        }

        #region p2p
        protected override void SendReliableP2P(List<ClientInfo<CSteamID?>> destinations, ArraySegment<byte> packet)
        {
            //Send all the P2P packets that we know how to contact directly
            for (var i = destinations.Count - 1; i >= 0; i--)
            {
                var conn = destinations[i].Connection;
                if (!conn.HasValue)
                    break;

                destinations.RemoveAt(i);
                _network.Send(conn.Value, packet, EP2PSend.k_EP2PSendReliable, false);
            }

            //Send to the rest of the destinations using server relay
            base.SendReliableP2P(destinations, packet);
        }

        protected override void SendUnreliableP2P(List<ClientInfo<CSteamID?>> destinations, ArraySegment<byte> packet)
        {
            //Send all the P2P packets that we know how to contact directly
            for (var i = destinations.Count - 1; i >= 0; i--)
            {
                var conn = destinations[i].Connection;
                if (!conn.HasValue)
                    break;

                destinations.RemoveAt(i);
                _network.Send(conn.Value, packet, EP2PSend.k_EP2PSendUnreliableNoDelay, false);
            }

            //Send to the rest of the destinations using server relay
            base.SendUnreliableP2P(destinations, packet);
        }

        protected override void OnServerAssignedSessionId(uint session, ushort id)
        {
            base.OnServerAssignedSessionId(session, id);

            //The server has assigned us an ID. Now we can introduce ourselves to other peers
            _handshake = WriteHandshakeP2P(session, id);
            SendHandshakes();
        }

        private void SendHandshakes()
        {
            if (_handshake != null)
            {
                _previousHandshakeTime = DateTime.UtcNow;

                Log.Debug("Sending P2P handshakes to {0} peers", _peers.Count);
                for (var i = 0; i < _peers.Count; i++)
                    _network.Send(_peers[i], new ArraySegment<byte>(_handshake), EP2PSend.k_EP2PSendReliableWithBuffering, false);
            }
            else
            {
                Log.Debug("Not sending P2P handshakes (not yet assigned ID)");
            }
        }

        internal void PeerDisconnected(CSteamID conn)
        {
            _peers.Remove(conn);
        }

        internal void PeerConnected(CSteamID client)
        {
            if (!_peers.Contains(client))
                _peers.Add(client);

            // When a new peer joins the session send a handshake to everyone to prompt P2P negotiation as necessary
            SendHandshakes();
        }
        #endregion
    }
}
