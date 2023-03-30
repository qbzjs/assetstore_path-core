using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Steamworks;
using UnityEngine;

namespace Dissonance.Integrations.SteamworksP2P.Demo
{
    public class SteamworksDemoPlayerManager
        : MonoBehaviour
    {
        private const ulong Magic = 9162487192474286414;

        private DissonanceComms _comms;

        private GameObject _localPlayer;
        private readonly Dictionary<string, SteamworksPlayerController> _players = new Dictionary<string, SteamworksPlayerController>();

        private Vector3 _lastSendPosition = Vector3.zero;
        private Quaternion _lastSendRotation = Quaternion.identity;
        private DateTime _lastPacketUpdate = DateTime.MinValue;

        [UsedImplicitly, SerializeField] public GameObject PlayerPrefab;

        public CSteamID LobbyId;

        private readonly byte[] _receiveBuffer = new byte[4096];
        private readonly byte[] _sendBuffer = new byte[4096];

        public void OnEnable()
        {
            _comms = FindObjectOfType<DissonanceComms>();
            _comms.OnPlayerJoinedSession += OnPlayerJoined;
            _comms.OnPlayerLeftSession += OnPlayerLeft;
        }

        public void Update()
        {
            if (_localPlayer == null && _comms.LocalPlayerName != null)
            {
                //Create player instance for the local player
                _localPlayer = Instantiate(PlayerPrefab);
                var component = _localPlayer.GetComponent<SteamworksDemoPlayer>();
                component.Setup(true, _comms.LocalPlayerName);
                _localPlayer.SetActive(true);
            }

            SendUpdate();
            ReceiveUpdates();
        }

        private void SendUpdate()
        {
            var trans = _localPlayer.transform;

            var moved = (trans.position - _lastSendPosition).sqrMagnitude > 0.1f || Quaternion.Angle(_lastSendRotation, trans.rotation) > 1;
            var minTime = DateTime.UtcNow - _lastPacketUpdate > TimeSpan.FromMilliseconds(20);
            var maxTime = DateTime.UtcNow - _lastPacketUpdate > TimeSpan.FromMilliseconds(100);
            var lobby = LobbyId != default(CSteamID);

            if (lobby && ((moved && minTime) || maxTime))
            {
                _lastPacketUpdate = DateTime.UtcNow;
                _lastSendRotation = trans.rotation;
                _lastSendPosition = trans.position;

                if (_localPlayer != null)
                {
                    uint length;
                    using (var packet = new MemoryStream(_sendBuffer))
                    {
                        using (var writer = new BinaryWriter(packet))
                        {
                            writer.Write(Magic);
                            writer.Write(_comms.LocalPlayerName);

                            writer.Write(trans.position.x);
                            writer.Write(trans.position.y);
                            writer.Write(trans.position.z);

                            writer.Write(trans.rotation.x);
                            writer.Write(trans.rotation.y);
                            writer.Write(trans.rotation.z);
                            writer.Write(trans.rotation.w);

                            length = (uint)packet.Position;
                        }
                    }

                    var count = SteamMatchmaking.GetNumLobbyMembers(LobbyId);
                    for (var i = 0; i < count; i++)
                    {
                        var remotePlayer = SteamMatchmaking.GetLobbyMemberByIndex(LobbyId, i);
                        if (remotePlayer != SteamUser.GetSteamID())
                            SteamNetworking.SendP2PPacket(remotePlayer, _sendBuffer, length, EP2PSend.k_EP2PSendUnreliable, 1);
                    }
                }
            }
        }

        private void ReceiveUpdates()
        {
            uint length;
            CSteamID remote;
            while (SteamNetworking.ReadP2PPacket(_receiveBuffer, (uint)_receiveBuffer.Length, out length, out remote, 1))
            {
                using (var mem = new MemoryStream(_receiveBuffer, 0, (int)length, false))
                using (var reader = new BinaryReader(mem))
                {
                    //If the start of the packet is incorrect skip over it
                    if (reader.ReadUInt64() != Magic)
                        continue;

                    //If we don't know about this player skip this packet
                    var id = reader.ReadString();
                    SteamworksPlayerController player;
                    if (!_players.TryGetValue(id, out player))
                        continue;

                    //Update position (no interpolation means this will be jittery but it's only a demo!)
                    player.SetTarget(
                        new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                    );
                }
            }
        }

        public void OnDisable()
        {
            _comms.OnPlayerJoinedSession -= OnPlayerJoined;
            _comms.OnPlayerLeftSession -= OnPlayerLeft;
        }

        private void OnPlayerJoined([NotNull] VoicePlayerState player)
        {
            var playerObj = Instantiate(PlayerPrefab);
            var component = playerObj.GetComponent<SteamworksDemoPlayer>();
            component.Setup(false, player.Name);
            playerObj.SetActive(true);

            _players.Add(player.Name, playerObj.GetComponent<SteamworksPlayerController>());
        }

        private void OnPlayerLeft([NotNull] VoicePlayerState player)
        {
            SteamworksPlayerController obj;
            if (_players.TryGetValue(player.Name, out obj))
            {
                Destroy(obj.gameObject);
                _players.Remove(player.Name);
            }
        }
    }
}
