using System.Collections;
using System.Collections.Generic;
using Mirror;
using MyGame.Scripts.NetworkManagers;
using UnityEngine;

public class PrisonSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;


    // Start is called before the first frame update
    void Start()
    {
        NetworkStartPosition[] spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        for (int i = 0; i < NetworkServer.connections.Count; i++)
        {
            if (NetworkServer.connections[i] != null && NetworkServer.connections[i].identity == null)
            {
                GameObject player = Instantiate(playerPrefab, spawnPoints[i].gameObject.transform);
                NetworkServer.AddPlayerForConnection(NetworkServer.connections[i], player);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}