using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.UI;
using MyGame.Scripts.NetworkManagers;
using Steamworks;
using UnityEngine;
using clientAPI = HeathenEngineering.SteamworksIntegration.API;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuLobbyController : MonoBehaviour
{
    public LobbyManager lobbyManager;
    public LobbyMemberSlot[] slots;

    //Local user things
    [Header("Local User Features")] public GameObject userOwnerPip;
    public Button readyButton;
    public Button notReadyButton;
    public Button leaveButton;

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
        //   Debug.Log("I have been called because someone is requesting to join. - \nthe user is : " + user.Name);

        Debug.Log("Lobby data requested successfully.");
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
        lobbyManager.SetLobbyData("HostAddress", SteamUser.GetSteamID().ToString());
        if (lobby["HostAddress"] != null)
        {
            Debug.Log($"This is the host address for the lobby now : {lobby["HostAddress"]}");
        }

        PrisonEscape_NetworkManager.singleton.StartHost();


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
        }

        //this is the last thing to call at start game.
        lobbyManager.Lobby.SetGameServer();

        PrisonEscape_NetworkManager.singleton.ServerChangeScene("Lockwood_Prison_1");
    }


    //occurs when the server info has been set on this lobby
    public void OnGameCreated(LobbyGameServer server)
    {
        if (lobbyManager.Lobby.IsOwner)
        {
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
        else
        {
            Debug.Log("i am something completely different.");
        }
    }

    public void LoadSinglePlayerGame()
    {
        PrisonEscape_NetworkManager.singleton.StartHost();
        SceneManager.LoadSceneAsync("Lockwood_Prison_1");
    }

    public void HandleLeaveRequest()
    {
        PrisonEscape_NetworkManager.singleton.StopHost();
        lobbyManager.Lobby.Leave();
        lobbyManager.Lobby = default;
    }

    public void OnSuccessfulJoinRequest(LobbyData lobbyData)
    {
        Debug.Log("someone joined me and i am now here.");
        string ServerLobbyAddress = lobbyData["HostAddress"];
        Debug.Log($"the lobby address theyre connecting to is : {ServerLobbyAddress}.");
        PrisonEscape_NetworkManager.singleton.networkAddress = ServerLobbyAddress;
        PrisonEscape_NetworkManager.singleton.StartClient();
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

            if (readyButton != null)
                readyButton.gameObject.SetActive(false);

            if (notReadyButton != null)
                notReadyButton.gameObject.SetActive(false);

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

            leaveButton.gameObject.SetActive(true);
            userOwnerPip.SetActive(lobbyManager.IsPlayerOwner);

            if (readyButton != null)
                readyButton.gameObject.SetActive(!lobbyManager.IsPlayerReady);

            if (notReadyButton != null)
                notReadyButton.gameObject.SetActive(lobbyManager.IsPlayerReady);

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