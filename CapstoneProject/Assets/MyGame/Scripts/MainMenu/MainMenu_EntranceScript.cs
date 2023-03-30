using System;
using Mirror;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


namespace MyGame.Scripts.MainMenu
{
    public class MainMenu_EntranceScript : NetworkBehaviour
    {
        public GameObject playerPrefab;

        // Get a list of all connections
        private void Start()
        {
            List<NetworkConnectionToClient> connections = NetworkServer.connections.Values.ToList();

// Iterate over the connections and check if each connection has a player object spawned
            foreach (NetworkConnectionToClient conn in connections)
            {
                // Check if the connection is not null and it doesn't already have a player object spawned
                if (conn != null && conn.identity == null)
                {
                    // Spawn the player object for this connection
                    GameObject playerObject = Instantiate(playerPrefab);
                    NetworkServer.AddPlayerForConnection(conn, playerObject);
                }
            }
        }
    }
}