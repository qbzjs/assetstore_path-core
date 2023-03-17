using UnityEngine;

namespace Dissonance.Integrations.SteamworksP2P.Demo
{
    public class SteamworksPlayerController
        : MonoBehaviour
    {
        private IDissonancePlayer _player;
        private CharacterController _controller;

        private const float InterpolationTime = 0.15f;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private float _interpolationTime;
        private bool _haveTarget;

        public void OnEnable()
        {
            _player = GetComponent<IDissonancePlayer>();
            _controller = GetComponent<CharacterController>();
        }

        public void Update()
        {
            if (_player.Type == NetworkPlayerType.Local)
            {
                var rotation = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
                var speed = Input.GetAxis("Vertical") * 3.0f;

                transform.Rotate(0, rotation, 0);
                var forward = transform.TransformDirection(Vector3.forward);
                _controller.SimpleMove(forward * speed);

                if (transform.position.y < -3)
                {
                    transform.position = Vector3.zero;
                    transform.rotation = Quaternion.identity;
                }
            }
            else if (_player.Type == NetworkPlayerType.Remote)
            {
                if (_haveTarget)
                {
                    _interpolationTime += Time.deltaTime / InterpolationTime;
                    transform.position = Vector3.Lerp(_startPosition, _targetPosition, _interpolationTime);
                    transform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, _interpolationTime);
                }
            }
        }

        public void SetTarget(Vector3 position, Quaternion rotation)
        {
            _targetPosition = position;
            _targetRotation = rotation;

            _startPosition = transform.position;
            _startRotation = transform.rotation;

            _haveTarget = true;
            _interpolationTime = 0;
        }
    }
}
