using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.UI;
using Michsky.UI.Dark;
using Mirror;
using MyGame.Scripts.NetworkManagers;
using Steamworks;
using UnityEngine;
using clientAPI = HeathenEngineering.SteamworksIntegration.API;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuLobbyController : MonoBehaviour
{
    public LobbyManager lobbyManager;
    public LobbyMemberSlot[] slots;
    [SerializeField] private MainPanelManager _mainPanelManager;


    //Local user things
    [Header("Local User Features")] public GameObject userOwnerPip;
    public Button leaveButton;

    public Color ReadyColour;
    public Color ReadyColourText;
    public Color UnreadyColourText;
    public Color StandardColourText;
    public Color UnreadyColour;
    public Color StandardColour;

    public bool firstTime = false;

    public Image ReadyUnreadyButtonImage;
    public TextMeshProUGUI readyUnreadyButtonTextNormal;
    public TextMeshProUGUI readyUnreadyButtonTextHighlighted;

    public Button SinglePlayerButton;

    public GameObject ShowLobbyMainMenuButton;
    public GameObject CreateNewLobbyMainMenuButton;

    public Image StartGameBackGroundImage;
    public TextMeshProUGUI StartGameNormalText;
    public TextMeshProUGUI StartGameHighlightedText;

    [Header("Configuration")] public RectTransform invitePanel;
    public FriendInviteDropDown inviteDropdown;
    public bool updateRichPresenceGroupData = true;

    [Header("Chat")] public int maxMessages = 200;
    public GameObject chatPanel;
    public TMP_InputField inputField;
    public ScrollRect scrollView;
    public Transform messageRoot;
    public GameObject myChatTemplate;
    public GameObject theirChatTemplate;

    private readonly List<IChatMessage> chatMessages = new List<IChatMessage>();
    private LobbyData loadingLobbyData;

    private void Start()
    {
        inviteDropdown.Invited.AddListener(HandleInvitedUser);
        clientAPI.Matchmaking.Client.EventLobbyChatMsg.AddListener(HandleChatMessage);
        clientAPI.Overlay.Client.EventGameLobbyJoinRequested.AddListener(HandleLobbyJoinRequest);
    }

    private void Update()
    {
        if (invitePanel.gameObject.activeSelf
            && !inviteDropdown.IsExpanded
            && ((Mouse.current.leftButton.wasPressedThisFrame
                 && !RectTransformUtility.RectangleContainsScreenPoint(invitePanel, Mouse.current.position.ReadValue()))
                || Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            //And if so then we hide the panel and clear the to invite text field
            inviteDropdown.gameObject.SetActive(false);
            inviteDropdown.inputField.text = string.Empty;
        }

        if (EventSystem.current.currentSelectedGameObject == inputField.gameObject &&
            (Keyboard.current.enterKey.wasPressedThisFrame
             || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
        {
            OnSendChatMessage();
        }
    }


    public void HandleExitGameOnQuitButton()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        clientAPI.Matchmaking.Client.EventLobbyChatMsg.RemoveListener(HandleChatMessage);
    }

    public void HandleChatUpdate(LobbyChatUpdate_t arg0)
    {
        if (arg0.m_ulSteamIDLobby == lobbyManager.Lobby)
        {
            RefreshUI();
        }
    }

    public void HandleLobbyJoinRequest(LobbyData lobby, UserData userData)
    {
        clientAPI.Matchmaking.Client.RequestLobbyData(lobby);
        //leave your server and client if you have them before you connect to the new player.
        if (NetworkManager.singleton.isNetworkActive)
        {
            string offlineScene = NetworkManager.singleton.offlineScene;
            NetworkManager.singleton.offlineScene = "";
            NetworkManager.singleton.StopHost();
            NetworkManager.singleton.offlineScene = offlineScene;
        }
        
        //leave your lobby before you connect to the new player
        if (lobbyManager.HasLobby)
        {
            lobbyManager.Lobby.Leave();
        }

        Debug.Log("I have been called because someone is requesting to join. - \nthe user is : " + userData.Name);
        Debug.Log("Lobby data requested successfully.");
        Debug.Log($"This is the person i am joining steam cid : {userData.id}");

        // Join the lobby here
        lobbyManager.Join(lobby);
        RefreshUI();
    }

    public void HandleChatMessage(LobbyChatMsg message)
    {
        if (message.lobby == lobbyManager.Lobby)
        {
            if (chatMessages.Count == maxMessages)
            {
                Destroy(chatMessages[0].GameObject);
                chatMessages.RemoveAt(0);
            }

            if (message.sender == UserData.Me)
            {
                var go = Instantiate(myChatTemplate, messageRoot);
                go.transform.SetAsLastSibling();
                var cmsg = go.GetComponent<IChatMessage>();
                if (cmsg != null)
                {
                    cmsg.Initialize(message);
                    if (chatMessages.Count > 0
                        && chatMessages[chatMessages.Count - 1].User == cmsg.User)
                        cmsg.IsExpanded = false;

                    chatMessages.Add(cmsg);
                }
            }
            else
            {
                var go = Instantiate(theirChatTemplate, messageRoot);
                go.transform.SetAsLastSibling();
                var cmsg = go.GetComponent<IChatMessage>();
                if (cmsg != null)
                {
                    cmsg.Initialize(message);
                    if (chatMessages[chatMessages.Count - 1].User == cmsg.User)
                        cmsg.IsExpanded = false;

                    chatMessages.Add(cmsg);
                }
            }

            StartCoroutine(ForceScrollDown());
        }
    }

    IEnumerator SelectInputField()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        inputField.Select();
    }

    /// <summary>
    /// Called when a new chat message is added to the UI to force the system to scroll to the bottom
    /// </summary>
    /// <returns></returns>
    IEnumerator ForceScrollDown()
    {
        // Wait for end of frame AND force update all canvases before setting to bottom.
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        scrollView.verticalNormalizedPosition = 0f;
    }

    public void OnSendChatMessage()
    {
        if (lobbyManager.HasLobby
            && !string.IsNullOrEmpty(inputField.text))
        {
            lobbyManager.Lobby.SendChatMessage(inputField.text);
            inputField.text = string.Empty;
            StartCoroutine(SelectInputField());
        }
    }

    public void HandleInvitedUser(UserData userData)
    {
        lobbyManager.Lobby.InviteUserToLobby(userData);
        RefreshUI();
    }

    public void OnJoinedALobby()
    {
        RefreshUI();

        //check if the server has already been set for this lobby.
        if (lobbyManager.HasServer)
            //If so update the user about that
            OnGameCreated(lobbyManager.GameServer);
    }

    public void OnLobbyCreated(LobbyData lobby)
    {
        //   lobbyManager.SetLobbyData("HostAddress", SteamUser.GetSteamID().ToString());
        // if (lobbyManager["HostAddress"] != null)
        //{
        //     Debug.Log($"This is the host address for the lobby now : {lobbyManager["HostAddress"]}");
        //   Debug.Log($"This is my own csteamid when i started the lobby: {SteamUser.GetSteamID().ToString()}");
        // }
        CreateNewLobbyMainMenuButton.SetActive(false);
        ShowLobbyMainMenuButton.SetActive(true);
        SinglePlayerButton.gameObject.SetActive(false);

        firstTime = false;
        readyUnreadyButtonTextHighlighted.text = "READY";
        readyUnreadyButtonTextNormal.text = "READY";
        ReadyUnreadyButtonImage.color = StandardColour;
        readyUnreadyButtonTextHighlighted.faceColor  = StandardColourText;
        readyUnreadyButtonTextNormal.faceColor  = StandardColourText;
        
        lobbyManager.Lobby.SetGameServer(SteamUser.GetSteamID());
        NetworkManager.singleton.StartHost();

        //here also show in the main menu the "Show lobby button now instead"
        RefreshUI();
    }

    public void HandleOnMemberLeft(UserLobbyLeaveData lobbyLeaveData)
    {
        Debug.Log($"A user named {lobbyLeaveData.user.Nickname} left the lobby");
        RefreshUI();
    }

    //Occurs when the player clicks the ready or wait button
    public void HandleReady()
    {
        if (!firstTime)
        {
            firstTime = true;
        }

        lobbyManager.IsPlayerReady = !lobbyManager.IsPlayerReady;
        RefreshUI();
    }

    public void UserJoined(UserData data)
    {
        Debug.Log($"A user named {data.Nickname} joined the lobby");
        RefreshUI();
    }

    public void StartGame()
    {
        if (!lobbyManager.Lobby.AllPlayersReady)
        {
            Debug.Log($"You chose to start the session before all players where marked ready");
            return;
        }

        //Make sure we are the player and not someone else in the lobby.
        if (lobbyManager.IsPlayerOwner)
        {
            NetworkManager.singleton.ServerChangeScene("Lockwood_Prison_1");
        }
        else
        {
            Debug.Log(
                "Unfortunately only the host of the server and lobby can start the game when all players are ready.");
        }
    }


    //occurs when the server info has been set on this lobby
    public void OnGameCreated(LobbyGameServer server)
    {
        if (lobbyManager.Lobby.IsOwner)
        {
            Debug.Log("Currently i have set the server game info after creating the server.");
            return;
        }
    }

    public void OnLobbyLeave()
    {
        Debug.Log("On Lobby leave invoked.");
        RefreshUI();
    }

    public void LobbyDataUpdated(LobbyDataUpdateEventData arg0)
    {
        if (arg0.lobby == lobbyManager.Lobby)
        {
            RefreshUI();
            Debug.Log("i am doing something in the lobby data updated but the lobby and my arg is the same thing");
        }

        else if (arg0.lobby == loadingLobbyData)
        {
            Debug.Log("lord have mercy");
            if (loadingLobbyData.IsGroup)
            {
                if (lobbyManager.HasLobby && lobbyManager.Lobby != loadingLobbyData)
                {
                    lobbyManager.Lobby.Leave();
                }

                lobbyManager.Lobby = loadingLobbyData;
                loadingLobbyData.Join((result, error) =>
                {
                    if (result.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                        RefreshUI();
                    else
                    {
                        loadingLobbyData.Leave();
                        lobbyManager.Lobby = default;
                    }
                });
            }
            else if (loadingLobbyData.IsSession)
            {
                if (LobbyData.SessionLobby(out var session))
                {
                    if (session != loadingLobbyData)
                    {
                        session.Leave();

                        loadingLobbyData.Join((result, error) =>
                        {
                            if (result.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                            {
                                RefreshUI();
                            }
                            else
                            {
                                loadingLobbyData.Leave();
                            }
                        });
                    }
                }
                else
                {
                    loadingLobbyData.Join((result, error) =>
                    {
                        if (result.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                        {
                            RefreshUI();
                        }
                        else
                        {
                            loadingLobbyData.Leave();
                        }
                    });
                }
            }

            loadingLobbyData = default;
        }
    }

    public void LoadSinglePlayerGame()
    {
        NetworkManager.singleton.StartHost();
        SceneManager.LoadSceneAsync("Lockwood_Prison_1");
    }

    public void HandleLeaveRequest()
    {
        CreateNewLobbyMainMenuButton.SetActive(true);
        ShowLobbyMainMenuButton.SetActive(false);
        SinglePlayerButton.gameObject.SetActive(true);
        _mainPanelManager.PanelAnim(0);


        if (NetworkManager.singleton.offlineScene != null || NetworkManager.singleton.offlineScene.Length > 0)
        {
            String offlineSceneTemp = NetworkManager.singleton.offlineScene;
            NetworkManager.singleton.offlineScene = "";
            NetworkManager.singleton.StopHost();
            NetworkManager.singleton.offlineScene = offlineSceneTemp;
        }

        firstTime = false;
        readyUnreadyButtonTextHighlighted.text = "READY";
        readyUnreadyButtonTextNormal.text = "READY";
        ReadyUnreadyButtonImage.color = StandardColour;
        readyUnreadyButtonTextHighlighted.faceColor  = StandardColourText;
        readyUnreadyButtonTextNormal.faceColor  = StandardColourText;

        lobbyManager.Lobby.Leave();
        lobbyManager.Lobby = default;
        RefreshUI();
    }

    public void OnSuccessfulJoinRequest(LobbyData lobbyData)
    {
        NetworkManager.singleton.networkAddress = lobbyManager.Lobby.GameServer.id.ToString();
        NetworkManager.singleton.StartClient();

        _mainPanelManager.PanelAnim(2);
        CreateNewLobbyMainMenuButton.SetActive(false);
        ShowLobbyMainMenuButton.SetActive(true);
        SinglePlayerButton.gameObject.SetActive(false);

        firstTime = false;
        
        readyUnreadyButtonTextHighlighted.text = "READY";
        readyUnreadyButtonTextNormal.text = "READY";
        
        ReadyUnreadyButtonImage.color = StandardColour;
        readyUnreadyButtonTextHighlighted.faceColor  = StandardColourText;
        readyUnreadyButtonTextNormal.faceColor  = StandardColourText;
        //here also show in the main menu the "Show lobby button now instead".
        RefreshUI();
        Debug.Log(
            $"the current prison networkaddress after the start client and networkaddress : {NetworkManager.singleton.networkAddress}");
    }


//called anytime lobby or server changes happen
    private void RefreshUI()
    {
        if (!lobbyManager.HasLobby)
        {
            if (updateRichPresenceGroupData)
            {
                UserData.SetRichPresence("steam_player_group", string.Empty);
                UserData.SetRichPresence("steam_player_group_size", string.Empty);
            }

            foreach (var slot in slots)
            {
                slot.ClearUser();
                slot.Interactable = true;
            }

            userOwnerPip.SetActive(false);

            leaveButton.gameObject.SetActive(false);
            chatPanel.SetActive(false);
        }
        else
        {
            if (updateRichPresenceGroupData)
            {
                UserData.SetRichPresence("steam_player_group", lobbyManager.Lobby.ToString());
                UserData.SetRichPresence("steam_player_group_size", lobbyManager.Members.Length.ToString());
            }

            if (lobbyManager.IsPlayerReady)
            {
                readyUnreadyButtonTextNormal.text = "NOT READY";
                readyUnreadyButtonTextHighlighted.text = "NOT READY";
                ReadyUnreadyButtonImage.color = UnreadyColour;
                readyUnreadyButtonTextNormal.faceColor  = UnreadyColourText;
                readyUnreadyButtonTextHighlighted.faceColor  = UnreadyColourText;
            }
            else if (!lobbyManager.IsPlayerReady && !firstTime)
            {
                readyUnreadyButtonTextNormal.text = "READY";
                readyUnreadyButtonTextHighlighted.text = "READY";
                ReadyUnreadyButtonImage.color = ReadyColour;
                readyUnreadyButtonTextNormal.faceColor  = ReadyColourText;
                readyUnreadyButtonTextHighlighted.faceColor  = ReadyColourText;
            }


            if (lobbyManager.IsPlayerOwner)
            {
                //Set the colour of the background image to bright since we are the host.
                var color = StartGameBackGroundImage.color;
                color.a = 1;
                StartGameBackGroundImage.color = color;

                var color1 = StartGameNormalText.color;
                color1.a = 1;
                StartGameNormalText.color = color1;

                var color2 = StartGameHighlightedText.color;
                color2.a = 1;
                StartGameHighlightedText.color = color2;
            }
            else
            {
                //Set the colour of the background image to half transparent as we are the client..
                var color = StartGameBackGroundImage.color;
                color.a = 0.55f;
                StartGameBackGroundImage.color = color;

                var color1 = StartGameNormalText.color;
                color1.a = 0.55f;
                StartGameNormalText.color = color1;

                var color2 = StartGameHighlightedText.color;
                color2.a = 0.55f;
                StartGameHighlightedText.color = color2;
            }

            leaveButton.gameObject.SetActive(true);
            userOwnerPip.SetActive(lobbyManager.IsPlayerOwner);

            var members = lobbyManager.Lobby.Members;
            if (members.Length > 1)
            {
                members = members.Where(p => p.user != UserData.Me).ToArray();

                for (int i = 0; i < slots.Length; i++)
                {
                    var slot = slots[i];
                    slot.Interactable = lobbyManager.Lobby.IsOwner;
                    if (members.Length > i)
                        slot.SetUser(members[i]);
                    else
                        slot.ClearUser();
                }

                chatPanel.SetActive(true);
            }
            else
                chatPanel.SetActive(false);
        }
    }
}