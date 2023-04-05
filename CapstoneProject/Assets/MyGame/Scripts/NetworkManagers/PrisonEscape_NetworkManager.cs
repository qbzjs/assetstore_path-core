using Mirror;
using UnityEngine;

namespace MyGame.Scripts.NetworkManagers
{
    public class PrisonEscape_NetworkManager : NetworkManager
    {
        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Awake()
        {
            base.Awake();
        }

        private void Update()
        {
            if (NetworkServer.active)
            {
                int clientCount = 0;

                foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
                {
                    if (conn != null)
                    {
                        clientCount++;
                    }
                }

                Debug.Log($"Total number of clients connected: {clientCount}");
            }
            else
            {
                Debug.Log("Server is not active.");
            }
        }

        #region Scene Management

        #endregion


        #region Server System Callbacks

        #endregion
    }
}