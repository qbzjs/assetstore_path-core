using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

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

    private void Update()
    {
        Debug.Log("Is the network active: " + isNetworkActive);
        if (networkSceneName != null)
        {
            Debug.Log("Current network scene location: " + networkSceneName);
        }

        Debug.Log("Current scenemanager active scene : " + SceneManager.GetActiveScene().name);
    }

    public override void OnServerChangeScene(string newSceneName)
    {
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        Debug.Log("The clients server is changing from: " + SceneManager.GetActiveScene().name);
        Debug.Log("The clients server is now changing too : " + newSceneName);
        Debug.Log("The clients server operation for loading is : " + sceneOperation.ToString());
    }
    

    public void UnloadSceneFromServer(string sceneName)
    {
        if (NetworkServer.active)
        {
            sl.UnloadLevelFromServer(sceneName);
        }
    }

    public void UnloadSceneFromClient(string sceneName)
    {
        sl.UnloadLevelFromClient(sceneName);
    }

    public override void ServerChangeScene(string newSceneToLoad)
    {
        if (NetworkServer.active)
        {
            Debug.Log("Currently active scene for the server :" + NetworkManager.networkSceneName);

            if (string.IsNullOrWhiteSpace(newSceneToLoad))
            {
                Debug.LogError("ServerChangeScene empty scene name");
                return;
            }

            if (NetworkServer.isLoadingScene && newSceneToLoad == networkSceneName)
            {
                Debug.LogError($"Scene change is already in progress for {newSceneToLoad}");
                return;
            }

            NetworkServer.SetAllClientsNotReady();
            networkSceneName = newSceneToLoad;

            // Let server prepare for scene change
            OnServerChangeScene(newSceneToLoad);

            StartCoroutine(sl.ServerChangeSceneAsync(newSceneToLoad));

        }

        Debug.Log("Currently active scene for the server after everything:" + NetworkManager.networkSceneName);
    }

    public bool AreAllClientsReadyToLoad()
    {
        // Implement a check to ensure all connected clients are ready to load the new scene.
        // This could be done by maintaining a list of connected clients and their ready status.
        return true;
    }
}