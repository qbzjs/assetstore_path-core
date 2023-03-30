using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using Animancer;
using HeathenEngineering.SteamworksIntegration;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using appclient = HeathenEngineering.SteamworksIntegration.API.App.Client;

public class BootStrapLoadingLogic : MonoBehaviour
{
    private static BootStrapLoadingLogic _instance;

    public static BootStrapLoadingLogic BootStrapLoadingLogic_Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BootStrapLoadingLogic();
            }

            return _instance;
        }
    }

    [SerializeField] private GameObject BootloaderCameraObject;
    [SerializeField] private SceneLoader _sceneLoader;
    [SerializeField] private GameObject BootloaderIntroTextObject;
    [SerializeField] private GameObject BootloaderPanelObject;
    [SerializeField] private GameObject BootloaderCanvasObject;
    [SerializeField] private float SplashScreenWaitBeforeDestroy = 2f;

    [SerializeField] private PrisonEscape_NetworkManager _networkManager;
    
    [SerializeField] private AnimancerComponent _Animancer;
    [SerializeField] private AnimationClip _PanelFadeOut;
    [SerializeField] private AnimationClip _PanelFadeIn;
    [SerializeField] private bool SceneAlreadyLoading = false;

    public void Start()
    {
         _Animancer.Play(_PanelFadeIn);
    }

    public void Update()
    {
        if (Input.anyKeyDown && !SceneAlreadyLoading)
        {
            _networkManager.ServerChangeScene("MainMenu");
            SceneAlreadyLoading = true;

            Debug.Log(
                "Since i am done loading and the animation is finished, i will clear bootstrap items canvas and camera.");
            //It destroys the camera because mainmenu has a camera.
            Destroy(BootloaderCameraObject);
            Destroy(BootloaderPanelObject);
            Destroy(BootloaderCanvasObject);
            //Destroy the text since it is done animating.
            Destroy(BootloaderIntroTextObject);

            _Animancer.Play(_PanelFadeOut);
 
            _Animancer = null;
            //      _sceneLoader.ResetProgressAmountToZero();
        }
    }

    //Validates and loads the steam api, but also normally the network manager gets made and added here. Now we need to just load the scene whenever we want as additive and it should all work.
    private IEnumerator ValidateBootStrapItems()
    {
        yield return new WaitUntil(() => SteamSettings.Initialized);
        Debug.Log("Steam api is initalized as a app " + appclient.Id.ToString() + " now starting scene load.");
    }

    public void OpeningCutSceneEventEnabled()
    {
        StartCoroutine(ValidateBootStrapItems());

        Debug.Log("Cutscene started. Level also loading.");
    }

    public void OpeningCutSceneEventDisabled()
    {
        Debug.Log("The cutscene has finished playing.");

        if (!SceneAlreadyLoading)
        {
            _networkManager.ServerChangeScene("MainMenu");
            SceneAlreadyLoading = true;
        }
    }
}