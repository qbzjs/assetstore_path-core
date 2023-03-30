using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : NetworkBehaviour
{
    private static SceneLoader _instance;

    public static SceneLoader SceneLoader_Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SceneLoader();
            }

            return _instance;
        }
    }

    [SerializeField] private float LoadingSceneProgressAmount;
    [SerializeField] private PrisonEscape_NetworkManager _networkManager;

    public float loadingSceneProgressAmount
    {
        get => LoadingSceneProgressAmount;
        private set => LoadingSceneProgressAmount = value;
    }

    [SerializeField] public float MinimumLevelLoadBeforeSceneTransitionAllowed = 0.90f;

    #region Server/Client Scene Loading/Unloading

    [Server]
    //This is basically handling both client and server - there is alot going on to explain this.
    public IEnumerator ServerChangeSceneAsync(string newSceneName)
    {
        // Load the new scene asynchronously on the server
        AsyncOperation serverLoadOperation = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        serverLoadOperation.allowSceneActivation = false;

        // Wait for all clients to report they are ready
        while (!_networkManager.AreAllClientsReadyToLoad())
        {
            yield return null;
        }

        // Activate the new scene on the server
        serverLoadOperation.allowSceneActivation = true;

        // Wait for the server to finish loading the new scene
        while (!serverLoadOperation.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(newSceneName));
        
        // Notify clients that the scene has changed
        NetworkServer.SendToAll(new SceneMessage
            { sceneName = newSceneName, sceneOperation = SceneOperation.LoadAdditive });

        NetworkServer.isLoadingScene = false;
        _networkManager.OnServerSceneChanged(newSceneName);

        // Set all clients as ready to start gameplay
        foreach (var conn in NetworkServer.connections)
        {
            if (conn.Value != null && conn.Value.isReady)
                continue;

            NetworkServer.SetClientReady(conn.Value);
        }
    }

    [Command]
    public void UnloadLevelFromServer(string sceneToUnload)
    {
        if (isServer)
        {
            // Check if the scene is loaded additively
            if (SceneManager.GetSceneByName(sceneToUnload).isLoaded)
            {
                // Unload the scene additively on the server only
                SceneManager.UnloadSceneAsync(sceneToUnload,
                        UnityEngine.SceneManagement.UnloadSceneOptions.UnloadAllEmbeddedSceneObjects).completed +=
                    asyncOperation => { Debug.Log($"Scene {sceneToUnload} has been unloaded on the server."); };
            }
        }
    }

    public void UnloadLevelFromClient(string sceneToUnload)
    {
        if (isClient)
        {
            // Check if the scene is loaded additively
            if (SceneManager.GetSceneByName(sceneToUnload).isLoaded &&
                SceneManager.GetSceneByName(sceneToUnload).isLoaded &&
                SceneManager.GetSceneByName(sceneToUnload).isLoaded)
            {
                // Unload the scene additively on the server only
                SceneManager.UnloadSceneAsync(sceneToUnload,
                        UnityEngine.SceneManagement.UnloadSceneOptions.UnloadAllEmbeddedSceneObjects).completed +=
                    asyncOperation => { Debug.Log($"Scene {sceneToUnload} has been unloaded on the server."); };
            }
        }
    }

    #endregion

    #region SinglePlayer Stuff

    public IEnumerator LoadAsyncSinglePlayer(string nameOfScene, LoadSceneMode loadSceneMode)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(nameOfScene, LoadSceneMode.Additive);
        ao.allowSceneActivation = false;

        while (!ao.isDone)
        {
            LoadingSceneProgressAmount = ao.progress;
            Debug.Log("Loading: " + Mathf.Clamp01((ao.progress / MinimumLevelLoadBeforeSceneTransitionAllowed) * 100f) +
                      "%");
            if (ao.progress >= MinimumLevelLoadBeforeSceneTransitionAllowed)
            {
                ao.allowSceneActivation = true;
            }

            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(nameOfScene));
    }

    public IEnumerator LoadAsyncSinglePlayer(int indexOfScene, LoadSceneMode loadSceneMode)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(indexOfScene, LoadSceneMode.Additive);
        ao.allowSceneActivation = false;

        while (!ao.isDone)
        {
            Debug.Log(ao.allowSceneActivation + " : allow scene activation");
            LoadingSceneProgressAmount = ao.progress;

            Debug.Log("Loading: " + Mathf.Clamp01((ao.progress / MinimumLevelLoadBeforeSceneTransitionAllowed) * 100f) +
                      "%");

            if (ao.progress >= MinimumLevelLoadBeforeSceneTransitionAllowed)
            {
                ao.allowSceneActivation = true;
            }

            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(indexOfScene));
    }

    public IEnumerator DeloadSceneAsynchronousSinglePlayer(int sceneIndexToLoad, UnloadSceneOptions options)
    {
        AsyncOperation ao = SceneManager.UnloadSceneAsync(sceneIndexToLoad, UnloadSceneOptions.None);
        ao.allowSceneActivation = false;

        while (!ao.isDone)
        {
            LoadingSceneProgressAmount = ao.progress;
            Debug.Log("Unloading: " +
                      Mathf.Clamp01((ao.progress / MinimumLevelLoadBeforeSceneTransitionAllowed) * 100f) + "%");
            if (ao.progress >= MinimumLevelLoadBeforeSceneTransitionAllowed)
            {
                ao.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    public IEnumerator DeloadSceneAsynchronousSinglePlayer(string sceneName, UnloadSceneOptions options)
    {
        AsyncOperation ao = SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.None);
        ao.allowSceneActivation = false;

        while (!ao.isDone)
        {
            LoadingSceneProgressAmount = ao.progress;
            Debug.Log("Unloading: " +
                      Mathf.Clamp01((ao.progress / MinimumLevelLoadBeforeSceneTransitionAllowed) * 100f) + "%");

            if (ao.progress >= MinimumLevelLoadBeforeSceneTransitionAllowed)
            {
                ao.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadingSceneProgressAmount >= 1f)
        {
            loadingSceneProgressAmount = 0;
        }
    }

    #endregion
}