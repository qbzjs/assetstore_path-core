using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
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
    [SerializeField] private NetworkedSceneLoader _networkedSceneLoader;

    public float loadingSceneProgressAmount
    {
        get => LoadingSceneProgressAmount;
        private set => LoadingSceneProgressAmount = value;
    }

    [SerializeField] public float MinimumLevelLoadBeforeSceneTransitionAllowed = 0.90f;

    #region Client Scene Loading/Unloading

    public IEnumerator ClientSceneLoadAsync(AsyncOperation ao, string sceneToLoad, SceneOperation loadSceneMode,
        bool customHandling)
    {
        ao = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        ao.allowSceneActivation = false;

        // Notify the server that this client is ready
        _networkedSceneLoader.CmdClientReady();

        while (!ao.allowSceneActivation)
        {
            yield return null;
        }

        // Wait for the client to finish loading the new scene
        while (!ao.isDone)
        {
            yield return null;
        }


        // Call the base method for handling scene changes
        if (!customHandling)
        {
            _networkManager.OnClientChangeSceneBaseCall(sceneToLoad, loadSceneMode, customHandling);
        }
    }
    //right now mirror only implements the scenebyloadname not by index so i will add that later.
    // public IEnumerator ClientSceneLoadAsync(int sceneIndexToLoad, LoadSceneMode loadSceneMode,bool customHandling)
    // {
    //     AsyncOperation ao = SceneManager.LoadSceneAsync(sceneIndexToLoad, LoadSceneMode.Additive);
    //     ao.allowSceneActivation = false;
    //
    //     // Notify the server that this client is ready
    //     _networkManager.CmdClientReady();
    //     
    //     while (!ao.allowSceneActivation)
    //     {
    //         yield return null;
    //     }
    //     
    //     // Wait for the client to finish loading the new scene
    //     while (!ao.isDone)
    //     {
    //         yield return null;
    //     }
    //     
    //     
    //     // Call the base method for handling scene changes
    //     if (!customHandling)
    //     {
    //         _networkManager.OnClientChangeSceneBaseCall(sceneIndexToLoad, loadSceneMode, customHandling);
    //     }
    // }


    // public IEnumerator DeloadClientSceneAsynchronous(int sceneIndexToLoad, UnloadSceneOptions options)
    //   {
    //       AsyncOperation ao = SceneManager.UnloadSceneAsync(sceneIndexToLoad, UnloadSceneOptions.None);
    //       ao.allowSceneActivation = false;
    //
    //       while (!ao.isDone)
    //       {
    //           LoadingSceneProgressAmount = ao.progress;
    //           float progress = Mathf.Clamp01(ao.progress / 0.9f);
    //           //       LoadingText.text = "Loading: " + (int)(progress * 100) + "%";
    //           Debug.Log("Unloading: " + Mathf.Clamp01(ao.progress / 0.9f));
    //           Debug.Log("Unloading: " + (int)(progress * 100) + "%");
    //
    //           if (ao.progress >= MinimumLevelLoadBeforeSceneTransitionAllowed)
    //               ao.allowSceneActivation = true;
    //
    //           yield return new WaitForEndOfFrame();
    //       }
    //
    //       if (loadingSceneProgressAmount >= 1f)
    //       {
    //           loadingSceneProgressAmount = 0;
    //       }
    //
    //
    //       yield return new WaitForEndOfFrame();
    //   }


    // public IEnumerator DeloadClientSceneAsynchronous(string sceneName, UnloadSceneOptions options)
    // {
    //     AsyncOperation ao = SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.None);
    //     ao.allowSceneActivation = false;
    //
    //     while (!ao.isDone)
    //     {
    //         LoadingSceneProgressAmount = ao.progress;
    //         float progress = Mathf.Clamp01(ao.progress / 0.9f);
    //         //       LoadingText.text = "Loading: " + (int)(progress * 100) + "%";
    //         Debug.Log("Unloading: " + Mathf.Clamp01(ao.progress / 0.9f));
    //         Debug.Log("Unloading: " + (int)(progress * 100) + "%");
    //
    //         if (ao.progress >= MinimumLevelLoadBeforeSceneTransitionAllowed)
    //             ao.allowSceneActivation = true;
    //
    //         yield return new WaitForEndOfFrame();
    //     }
    //
    //     if (loadingSceneProgressAmount >= 1f)
    //     {
    //         loadingSceneProgressAmount = 0;
    //     }
    //
    //
    //     yield return new WaitForEndOfFrame();
    // }

    public IEnumerator ClientUnloadSceneAsync(string sceneName)
    {
        // Unload the scene asynchronously on the client
        AsyncOperation clientUnloadOperation = SceneManager.UnloadSceneAsync(sceneName);

        // Wait for the client to finish unloading the scene
        while (!clientUnloadOperation.isDone)
        {
            yield return null;
        }
    }

    #endregion


    #region Server Scene Loading/Unloading

    public IEnumerator ServerChangeSceneAsync(string newSceneName)
    {
        // Notify clients to start loading the new scene
        _networkManager.ServerChangeScene(newSceneName);

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

        // Notify clients to activate the new scene
        _networkedSceneLoader.RpcClientActivateNewScene();
    }

    public IEnumerator ServerUnloadSceneAsync(string sceneName)
    {
        // Notify clients to start unloading the scene
        _networkedSceneLoader.RpcClientUnloadScene(sceneName);

        // Unload the scene asynchronously on the server
        AsyncOperation serverUnloadOperation = SceneManager.UnloadSceneAsync(sceneName);

        // Wait for the server to finish unloading the scene
        while (!serverUnloadOperation.isDone)
        {
            yield return null;
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

    public void ResetProgressAmountToZero()
    {
        loadingSceneProgressAmount = 0;
    }
}