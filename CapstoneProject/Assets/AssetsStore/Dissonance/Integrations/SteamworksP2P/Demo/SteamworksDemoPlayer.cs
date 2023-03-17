using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Dissonance.Integrations.SteamworksP2P.Demo
{
    public class SteamworksDemoPlayer
        : MonoBehaviour, IDissonancePlayer
    {
        private bool _isTracking;
        private bool _isLocal;
        private string _playerId;

        private DissonanceComms _comms;

        public string PlayerId
        {
            get { return _playerId; }
        }

        public Vector3 Position
        {
            get { return transform.position; }
        }

        public Quaternion Rotation
        {
            get { return transform.rotation; }
        }

        public NetworkPlayerType Type
        {
            get { return _isLocal ? NetworkPlayerType.Local : NetworkPlayerType.Remote; }
        }

        public bool IsTracking
        {
            get { return _isTracking; }
        }

        public void OnEnable()
        {
            _comms = FindObjectOfType<DissonanceComms>();
            _comms.TrackPlayerPosition(this);
            _isTracking = true;
        }

        public void OnDisable()
        {
            if (_comms != null)
                _comms.StopTracking(this);
            _isTracking = false;
        }

        public void Setup(bool isLocal, [NotNull] string playerId)
        {
            if (playerId == null)
                throw new ArgumentNullException("playerId");

            _isLocal = isLocal;
            _playerId = playerId;
        }
    }
}
