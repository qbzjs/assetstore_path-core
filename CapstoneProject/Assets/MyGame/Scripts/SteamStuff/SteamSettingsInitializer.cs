using HeathenEngineering.SteamworksIntegration;
using UnityEngine;

namespace MyGame.Scripts.SteamStuff
{
    public class SteamSettingsInitializer : MonoBehaviour
    {
        [SerializeField] private SteamSettings _steamSettings;

        // Start is called before the first frame update
        void Start()
        {
            //really simple, this can be changed in the future to be different - this goes a little in depth as to how it can be different
            //so the documentation that goes over this is located here : 
            //https://kb.heathen.group/assets/steamworks/unity-engine/quick-start-guide/scriptableobject-initialization -- way we do it
            //or
            //https://kb.heathen.group/assets/steamworks/unity-engine/quick-start-guide/api-initialization -- way we can do it eventually if need be.
            _steamSettings.Initialize();
        }
    }
}