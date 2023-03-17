﻿using System;
using Dissonance.Networking;
using JetBrains.Annotations;
using Steamworks;
using UnityEngine;

namespace Dissonance.Integrations.SteamworksP2P
{
    public class SteamworksP2PCommsNetwork
        : BaseCommsNetwork<SteamworksP2PServer, SteamworksP2PClient, CSteamID, CSteamID, Unit>
    {
        [SerializeField, UsedImplicitly] private int _channelToServer = 144984105;
        public int P2PPacketChannelToServer
        {
            get { return _channelToServer; }
        }

        [SerializeField, UsedImplicitly] private int _channelToClient = 144984106;
        public int P2PPacketChannelToClient
        {
            get { return _channelToClient; }
        }

        public void InitializeAsDedicatedServer()
        {
            RunAsDedicatedServer(Unit.None);
        }

        public void InitializeAsServer()
        {
            var local = SteamUser.GetSteamID();
            RunAsHost(Unit.None, local);
        }

        public void InitializeAsClient(CSteamID server)
        {
            RunAsClient(server);
        }

        public void PeerDisconnected(CSteamID client)
        {
            var s = Server;
            if (s != null)
                s.PeerDisconnected(client);

            var c = Client;
            if (c != null)
                c.PeerDisconnected(client);
        }

        public void PeerConnected(CSteamID client)
        {
            var c = Client;
            if (c != null)
                c.PeerConnected(client);
        }

        protected override SteamworksP2PServer CreateServer(Unit connectionParameters)
        {
            return new SteamworksP2PServer(this);
        }

        protected override SteamworksP2PClient CreateClient(CSteamID serverId)
        {
            return new SteamworksP2PClient(this, serverId);
        }

        internal bool Send(CSteamID dest, ArraySegment<byte> packet, EP2PSend sendType, bool toServer)
        {
            if (packet.Offset != 0)
                throw Log.CreatePossibleBugException("Non-zero packet offset", "F12DADBF-3688-4758-ADFB-8D30AAABAD96");

            return SteamNetworking.SendP2PPacket(dest, packet.Array, (uint)packet.Count, sendType, toServer ? P2PPacketChannelToServer : P2PPacketChannelToClient);
        }
    }
}
