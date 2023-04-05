using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.UI;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using clientAPI = HeathenEngineering.SteamworksIntegration.API;


namespace MyGame.Scripts.NetworkManagers
{
    public class PartyGroupController : MonoBehaviour
    {
        [Header("Local User Features")] public GameObject userOwnerPip;
        public Button readyButton;
        public Button notReadyButton;
        public Button leaveButton;

        [Header("Configuration")] public RectTransform invitePanel;
        public FriendInviteDropDown inviteDropdown;
        public LobbyMemberSlot[] slots;
        public bool updateRichPresenceGroupData = true;

        [Header("Chat")] public int maxMessages = 200;
        public GameObject chatPanel;
        public TMP_InputField inputField;
        public ScrollRect scrollView;
        public Transform messageRoot;
        public GameObject myChatTemplate;
        public GameObject theirChatTemplate;

        [Header("Events")] public LobbyDataEvent evtSessionLobbyInvite;

        public LobbyData Lobby { get; set; }
        public bool HasLobby => Lobby != CSteamID.Nil.m_SteamID && SteamMatchmaking.GetNumLobbyMembers(Lobby) > 0;
        public bool IsPlayerOwner => Lobby.IsOwner;
        public bool AllPlayersReady => Lobby.AllPlayersReady;

        public bool IsPlayerReady
        {
            get => clientAPI.Matchmaking.Client.GetLobbyMemberData(Lobby, clientAPI.User.Client.Id, LobbyData.DataReady) ==
                   "true";
            set => clientAPI.Matchmaking.Client.SetLobbyMemberData(Lobby, LobbyData.DataReady, value.ToString().ToLower());
        }

        private readonly List<IChatMessage> chatMessages = new List<IChatMessage>();
        private LobbyData loadingLobbyData;

        // Start is called before the first frame update
        void Awake()
        {
            PrisonEscape_NetworkManager.singleton.StartHost();
        }

        private void Start()
        {
            inviteDropdown.Invited.AddListener(HandleInvitedUser);

            if (readyButton != null)
                readyButton.onClick.AddListener(HandleReadyClicked);

            if (notReadyButton != null)
                notReadyButton.onClick.AddListener(HandleNotReadyClicked);

            leaveButton.onClick.AddListener(HandleLeaveClicked);

            var group = clientAPI.Matchmaking.Client.memberOfLobbies.FirstOrDefault(p => p.IsGroup);
            if (group.IsValid)
            {
                Lobby = group;
            }

            clientAPI.Overlay.Client.EventGameLobbyJoinRequested.AddListener(HandleLobbyJoinRequest);
            clientAPI.Matchmaking.Client.EventLobbyChatMsg.AddListener(HandleChatMessage);
            clientAPI.Matchmaking.Client.EventLobbyEnterSuccess.AddListener(HandleLobbyEnterSuccess);
            clientAPI.Matchmaking.Client.EventLobbyAskedToLeave.AddListener(HandleLobbyKickRequest);
            clientAPI.Matchmaking.Client.EventLobbyDataUpdate.AddListener(HandleLobbyDataUpdated);
            clientAPI.Matchmaking.Client.EventLobbyChatUpdate.AddListener(HandleChatUpdate);

            if (clientAPI.App.Initialized)
                RefreshUI();
            else
                clientAPI.App.evtSteamInitialized.AddListener(HandleSteamInitalization);
        }

        private void HandleSteamInitalization()
        {
            RefreshUI();
            clientAPI.App.evtSteamInitialized.RemoveListener(HandleSteamInitalization);
        }

        private void OnDestroy()
        {
            clientAPI.Overlay.Client.EventGameLobbyJoinRequested.RemoveListener(HandleLobbyJoinRequest);
            clientAPI.Matchmaking.Client.EventLobbyChatMsg.RemoveListener(HandleChatMessage);
            clientAPI.Matchmaking.Client.EventLobbyEnterSuccess.RemoveListener(HandleLobbyEnterSuccess);
            clientAPI.Matchmaking.Client.EventLobbyAskedToLeave.RemoveListener(HandleLobbyKickRequest);
            clientAPI.Matchmaking.Client.EventLobbyDataUpdate.RemoveListener(HandleLobbyDataUpdated);
            clientAPI.Matchmaking.Client.EventLobbyChatUpdate.RemoveListener(HandleChatUpdate);
        }

        private void Update()
        {
            if (invitePanel.gameObject.activeSelf
                && !inviteDropdown.IsExpanded
                && ((
#if ENABLE_INPUT_SYSTEM
                        Mouse.current.leftButton.wasPressedThisFrame
                        && !RectTransformUtility.RectangleContainsScreenPoint(invitePanel,
                            Mouse.current.position.ReadValue())
#else
                Input.GetMouseButtonDown(0)
                && !RectTransformUtility.RectangleContainsScreenPoint(invitePanel, Input.mousePosition)
#endif
                    )
                    ||
#if ENABLE_INPUT_SYSTEM
                    Keyboard.current.escapeKey.wasPressedThisFrame
#else
                Input.GetKeyDown(KeyCode.Escape)
#endif
                ))
            {
                //And if so then we hide the panel and clear the to invite text field
                inviteDropdown.gameObject.SetActive(false);
                inviteDropdown.inputField.text = string.Empty;
            }

            if (EventSystem.current.currentSelectedGameObject == inputField.gameObject
#if ENABLE_INPUT_SYSTEM
                && (Keyboard.current.enterKey.wasPressedThisFrame
                    || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
#else
                && (Input.GetKeyDown(KeyCode.Return)
                    || Input.GetKeyDown(KeyCode.KeypadEnter))
#endif
               )
            {
                OnSendChatMessage();
            }
        }


        private void HandleChatUpdate(LobbyChatUpdate_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == Lobby)
            {
                var state = (EChatMemberStateChange)arg0.m_rgfChatMemberStateChange;
                if (state == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
                    clientAPI.Friends.Client.SetPlayedWith(arg0.m_ulSteamIDUserChanged);

                RefreshUI();
            }
        }

        private void OnSendChatMessage()
        {
            if (HasLobby
                && !string.IsNullOrEmpty(inputField.text))
            {
                Lobby.SendChatMessage(inputField.text);
                inputField.text = string.Empty;
                StartCoroutine(SelectInputField());
            }
        }

        private void HandleLeaveClicked()
        {
            if (HasLobby)
            {
                Lobby.Leave();
                Lobby = default;
                RefreshUI();
            }
        }

        private void HandleNotReadyClicked()
        {
            IsPlayerReady = false;
            RefreshUI();
        }

        private void HandleReadyClicked()
        {
            IsPlayerReady = true;
            RefreshUI();
        }


        private void HandleLobbyDataUpdated(LobbyDataUpdateEventData arg0)
        {
            if (arg0.lobby == Lobby)
                RefreshUI();
            else if (arg0.lobby == loadingLobbyData)
            {
                if (loadingLobbyData.IsGroup)
                {
                    if (HasLobby && Lobby != loadingLobbyData)
                    {
                        Lobby.Leave();
                    }

                    Lobby = loadingLobbyData;
                    loadingLobbyData.Join((result, error) =>
                    {
                        if (result.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                            RefreshUI();
                        else
                        {
                            loadingLobbyData.Leave();
                            Lobby = default;
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
                                    evtSessionLobbyInvite.Invoke(loadingLobbyData);
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
                                evtSessionLobbyInvite.Invoke(loadingLobbyData);
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

        private void HandleInvitedUser(UserData arg0)
        {
            if (!HasLobby)
            {
                clientAPI.Matchmaking.Client.CreateLobby(ELobbyType.k_ELobbyTypePublic, slots.Length + 1,
                    (result, lobby, error) =>
                    {
                        if (result == EResult.k_EResultOK && !error)
                        {
                            lobby.IsGroup = true;
                            Lobby = lobby;
                            Lobby.InviteUserToLobby(arg0);
                        }
                    });
            }
            else
            {
                Lobby.InviteUserToLobby(arg0);
            }
        }

        private void HandleLobbyKickRequest(LobbyData arg0)
        {
            if (arg0 == Lobby)
            {
                Lobby.Leave();
                Lobby = default;
                RefreshUI();
            }
        }

        private void HandleLobbyJoinRequest(LobbyData lobby, UserData user)
        {
            clientAPI.Matchmaking.Client.RequestLobbyData(lobby);

            Debug.Log("Lobby data requested successfully.");
            // Join the lobby here

            // clientAPI.Matchmaking.Client.JoinLobby(lobby, (lobbyenter, success) =>
            // {
            //     if (success)
            //     {
            //         Debug.Log(lobby["connectionData"]);
            //         Debug.Log("Joined lobby successfully.");
            //         Uri newserver = new Uri(lobby["connectionData"]);
            //         PrisonEscape_NetworkManager.singleton.networkAddress = lobby;
            //         PrisonEscape_NetworkManager.singleton.StartClient(newserver);
            //         RefreshUI();
            //     }
            //     else
            //     {
            //         Debug.Log("Failed to join lobby.");
            //     }
            // });
        }

        private void HandleLobbyEnterSuccess(LobbyEnter_t arg0)
        {
            LobbyData nLobby = arg0.m_ulSteamIDLobby;

            if (nLobby.IsGroup)
            {
                Lobby = nLobby;
                
                clientAPI.Matchmaking.Client.JoinLobby(nLobby, (lobbyenter, success) =>
                {
                    if (success)
                    {
                        Debug.Log(nLobby["connectionData"]);
                        Debug.Log("Joined lobby successfully.");
                        Uri newserver = new Uri(nLobby["connectionData"]);
                        PrisonEscape_NetworkManager.singleton.StartClient(newserver);

                        RefreshUI();
                    }
                    else
                    {
                        Debug.Log("Failed to join lobby.");
                    }
                });
                
                
            }
        }

        private void HandleChatMessage(LobbyChatMsg message)
        {
            if (message.lobby == Lobby)
            {
                if (message.Message.StartsWith("[SessionId]"))
                {
                    if (ulong.TryParse(message.Message.Substring(11), out ulong steamID))
                    {
                        loadingLobbyData = steamID;
                        clientAPI.Matchmaking.Client.RequestLobbyData(loadingLobbyData);
                    }
                }
                else
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

        private void RefreshUI()
        {
            if (!HasLobby)
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
                    UserData.SetRichPresence("steam_player_group", Lobby.ToString());
                    UserData.SetRichPresence("steam_player_group_size", (slots.Length + 1).ToString());
                }

                leaveButton.gameObject.SetActive(true);
                userOwnerPip.SetActive(IsPlayerOwner);

                if (readyButton != null)
                    readyButton.gameObject.SetActive(!IsPlayerReady);

                if (notReadyButton != null)
                    notReadyButton.gameObject.SetActive(IsPlayerReady);

                var members = Lobby.Members;
                if (members.Length > 1)
                {
                    members = members.Where(p => p.user != UserData.Me).ToArray();

                    for (int i = 0; i < slots.Length; i++)
                    {
                        var slot = slots[i];
                        slot.Interactable = Lobby.IsOwner;
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
}