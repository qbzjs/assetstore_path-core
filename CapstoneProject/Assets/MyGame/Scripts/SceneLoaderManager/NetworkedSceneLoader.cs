using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkedSceneLoader : NetworkBehaviour
{
    [SerializeField]
    private PrisonEscape_NetworkManager _networkManager;

    [SerializeField] private SceneLoader sl;
    
    [ClientRpc]
    public void RpcClientActivateNewScene()
    {
        // Activate the new scene on the client
        if (_networkManager.clientLoadOperation != null)
        {
            _networkManager.clientLoadOperation.allowSceneActivation = true;
        }
    }

    
    [ClientRpc]
    public void RpcClientUnloadScene(string sceneName)
    {
        StartCoroutine(sl.ClientUnloadSceneAsync(sceneName));
    }


    [Command]
    public void CmdClientReady()
    {
        // Implement a method to mark the client as ready to load the new scene.
        // This could be done by updating a list of connected clients and their ready status.
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
