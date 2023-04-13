using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VersionNumberCheck : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TextToDisplayVersion;

    // Start is called before the first frame update
    void Start()
    {
        TextToDisplayVersion.text = "V." + PlayerPrefs.GetString("GameVersion");
        Debug.Log($"The current version of the game is: {PlayerPrefs.GetString("GameVersion")}");
    }
}