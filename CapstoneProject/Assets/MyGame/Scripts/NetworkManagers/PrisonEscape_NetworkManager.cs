using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PrisonEscape_NetworkManager : NetworkManager
{
    private static PrisonEscape_NetworkManager _instance;

    public AsyncOperation clientLoadOperation;

    [SerializeField] private SceneLoader sl;

    public static PrisonEscape_NetworkManager PrisonEscapeNetworkManager_Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PrisonEscape_NetworkManager();
            }

            return _instance;
        }
    }

    public override void OnServerChangeScene(string newSceneName)
    {
        StartCoroutine(sl.ServerChangeSceneAsync(newSceneName));
    }

    public void OnClientChangeSceneBaseCall(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        StartCoroutine(sl.ClientSceneLoadAsync(clientLoadOperation, newSceneName, sceneOperation, customHandling));
    }
    
    public void UnloadSceneFromServer(string sceneName)
    {
        if (NetworkServer.active)
        {
            StartCoroutine(sl.ServerUnloadSceneAsync(sceneName));
        }
    }

    //change the internals of mirror to use the index as well but i guess the string works for now.
    public void ChangeScene(string SceneName)
    {
        if (NetworkServer.active)
        {
            ServerChangeScene(SceneName);
        }
    }

    public bool AreAllClientsReadyToLoad()
    {
        // Implement a check to ensure all connected clients are ready to load the new scene.
        // This could be done by maintaining a list of connected clients and their ready status.
        return true;
    }
}