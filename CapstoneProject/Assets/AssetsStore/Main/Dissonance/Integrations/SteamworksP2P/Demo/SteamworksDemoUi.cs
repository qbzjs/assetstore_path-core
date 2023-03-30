using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dissonance.Integrations.SteamworksP2P.Demo
{
    public class SteamworksDemoUi
        : MonoBehaviour
    {
        private const string DissonanceLobbyKey = "1A67E8FC-BCD9-4F53-845D-4F69531F0B29";
        private const string DissonanceLobbyVal = "536366D9-A522-456B-BEC5-51986EE2E515";

        private BaseState _state;
        private BaseState _nextState;

        private bool _skipDestroy;

        public void Awake()
        {
            //If there's another instance destroy this instance
            if (FindObjectsOfType<SteamworksDemoUi>().Length > 1)
            {
                _skipDestroy = true;
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(this);

            //Sanity check that we can use steamworks. If not early exit and never do anything else.
            if (!SteamAPI.Init())
                _state = new FailedToInitSteamworks();
            else
                _state = new LoadingLobbyList();
            _state.Activate();
        }

        public void OnDestroy()
        {
            if (_skipDestroy)
                return;

            //Special case to leave the network session
            var game = _state as BaseInGame;
            if (game != null && game.CommsNetwork != null)
                LeaveSession(game.Lobby, game.CommsNetwork);

            //Destroy the state
            var ds = _state as IDisposable;
            if (ds != null)
                ds.Dispose();
            _state = null;

            //Destroy the next state too
            var dns = _state as IDisposable;
            if (dns != null)
                dns.Dispose();
            _nextState = null;

            SteamAPI.Shutdown();
        }

        [UsedImplicitly]
        private void Update()
        {
            _state.Update();

            if (_nextState != null)
            {
                //Dispose current state
                var state = _state as IDisposable;
                if (state != null)
                    state.Dispose();

                //Move to next state
                _state = _nextState;
                _nextState = null;

                //Activate the new state
                _state.Activate();
            }
        }

        [UsedImplicitly] private void OnGUI()
        {
            using (new GUILayout.AreaScope(new Rect(10, 10, 250, Screen.height - 20)))
            {
                var next = _state.OnGUI();
                if (next == null)
                    return;

                //Sanity check that we're not about to dispose ourself
                if (ReferenceEquals(next, _state))
                    throw new InvalidOperationException("Cannot set next state to current state");

                //Early exit if we've already applied this state
                if (ReferenceEquals(next, _nextState))
                    return;

                //Dispose next state if there was one already waiting
                var ns = _nextState as IDisposable;
                if (ns != null)
                    ns.Dispose();

                //save next state
                _nextState = next;
            }
        }

        private static void LeaveSession(CSteamID lobby, [NotNull] SteamworksP2PCommsNetwork network)
        {
            //Kill Dissonance
            network.Stop();

            //Close network connections
            var count = SteamMatchmaking.GetNumLobbyMembers(lobby);
            for (var i = 0; i < count; i++)
                SteamNetworking.CloseP2PSessionWithUser(SteamMatchmaking.GetLobbyMemberByIndex(lobby, i));

            //Leave the lobby
            SteamMatchmaking.LeaveLobby(lobby);
        }

        private class LobbyEntry
        {
            public CSteamID Id { get; private set; }
            public string Name { get; private set; }

            public LobbyEntry(CSteamID id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        private abstract class BaseState
        {
            public abstract void Activate();

            public virtual void Update()
            {

            }

            [CanBeNull] public abstract BaseState OnGUI();
        }

        private sealed class FailedToInitSteamworks
            : BaseState
        {
            public override void Activate()
            {
            }

            public override BaseState OnGUI()
            {
                GUILayout.Label("Steamworks Failed To Initialize!");
                return null;
            }
        }

        #region lobby list states
        private sealed class LoadingLobbyList
            : BaseState, IDisposable
        {
            private Callback<LobbyMatchList_t> _lobbyListCallback;
            private List<LobbyEntry> _result;

            public override void Activate()
            {
                //Subscribe to results callback
                _lobbyListCallback = Callback<LobbyMatchList_t>.Create(LobbyListCallback);

                //Add a filter to only load games with this specific key/value pair attached. Filtering the lobby list to Dissonance only
                SteamMatchmaking.AddRequestLobbyListStringFilter(DissonanceLobbyKey, DissonanceLobbyVal, ELobbyComparison.k_ELobbyComparisonEqual);
                SteamMatchmaking.RequestLobbyList();
            }

            private void LobbyListCallback(LobbyMatchList_t lobbies)
            {
                var list = new List<LobbyEntry>();

                for (var index = 0; index < lobbies.m_nLobbiesMatching; index++)
                {
                    var id = SteamMatchmaking.GetLobbyByIndex(index);
                    var nm = SteamMatchmaking.GetLobbyData(id, "name");
                    if (!string.IsNullOrEmpty(nm))
                        list.Add(new LobbyEntry(id, nm));
                }

                _result = list;

                _lobbyListCallback.Dispose();
                _lobbyListCallback = null;
            }

            public override BaseState OnGUI()
            {
                GUILayout.Label("Loading Steam Game List.");

                return _result == null
                     ? null
                     : new LobbyListState(_result);
            }

            public void Dispose()
            {
                if (_lobbyListCallback != null)
                    _lobbyListCallback.Dispose();
                _lobbyListCallback = null;
            }
        }

        private sealed class LobbyListState
            : BaseState
        {
            private readonly LobbyEntry[] _lobbyList;
            private Vector2 _lobbyListScrollState;

            public LobbyListState([NotNull] List<LobbyEntry> lobbyList)
            {
                _lobbyList = lobbyList.ToArray();
            }

            public override void Activate()
            {
            }

            public override BaseState OnGUI()
            {
                if (GUILayout.Button("Refresh List"))
                    return new LoadingLobbyList();

                if (GUILayout.Button("Create Game (Public)"))
                    return new CreatingGame(false);

                //if (GUILayout.Button("Create Game (Friends Only)"))
                //    return new CreatingGame(true);

                GUILayout.Label(string.Format("Found {0} Games:", _lobbyList.Length));

                using (var scrollViewScope = new GUILayout.ScrollViewScope(_lobbyListScrollState, false, false, GUILayout.ExpandWidth(true)))
                {
                    for (var i = 0; i < _lobbyList.Length; i++)
                        using (new GUILayout.HorizontalScope())
                            if (GUILayout.Button(_lobbyList[i].Name))
                                return new JoiningGame(_lobbyList[i]);

                    _lobbyListScrollState = scrollViewScope.scrollPosition;
                }

                return null;
            }
        }
        #endregion

        #region session management
        private sealed class JoiningGame
            : BaseState
        {
            private readonly LobbyEntry _game;

            public JoiningGame(LobbyEntry game)
            {
                _game = game;
            }

            public override void Activate()
            {
                //Session setup
                // 1. Enter the lobby
                // 2. Set the "password" on self
                // 3. Send p2p packet to host (trying to trigger a session request)
                //   - Keep doing this until success, fail or timeout
                //   4. If we succeeded: move to InGameAsClient
                //   5. If we failed:
                //     5a. Leave lobby
                //     6a. Close session with host
                //     5b. move to LoadingLobbyList
            }

            [NotNull] public override BaseState OnGUI()
            {
                return new JoiningLobby(_game.Id);
            }

            private class JoiningLobby
                : BaseState, IDisposable
            {
                private readonly CSteamID _lobbyId;

                private CallResult<LobbyEnter_t> _lobbyEnterResult;
                private bool _failed;

                private bool _joined;
                private DateTime _joinedTimer;

                public JoiningLobby(CSteamID lobbyId)
                {
                    _lobbyId = lobbyId;
                }

                public override void Activate()
                {
                    var result = SteamMatchmaking.JoinLobby(_lobbyId);
                    _lobbyEnterResult = CallResult<LobbyEnter_t>.Create(LobbyEnterCallback);
                    _lobbyEnterResult.Set(result);
                }

                private void LobbyEnterCallback(LobbyEnter_t lobby, bool bIOFailure)
                {
                    if (bIOFailure)
                    {
                        Debug.LogError("Failed to join lobby");
                        return;
                    }

                    var response = (EChatRoomEnterResponse)lobby.m_EChatRoomEnterResponse;
                    if (response != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                    {
                        Debug.LogError(string.Format("Failed to join lobby ({0})", response));
                        _failed = true;
                    }
                    else
                    {
                        SteamMatchmaking.SetLobbyMemberData(_lobbyId, DissonanceLobbyKey, DissonanceLobbyVal);

                        _joinedTimer = DateTime.UtcNow + TimeSpan.FromSeconds(1);
                        _joined = true;
                    }
                }

                public override BaseState OnGUI()
                {
                    GUILayout.Label("Entering Steam Lobby...");

                    if (_failed)
                        return new LoadingLobbyList();
                    else if (_joined && DateTime.UtcNow > _joinedTimer)
                        return new SendingSessionRequests(_lobbyId);
                    else
                        return null;
                }

                public void Dispose()
                {
                    if (_lobbyEnterResult != null)
                        _lobbyEnterResult.Dispose();
                    _lobbyEnterResult = null;
                }
            }

            private class SendingSessionRequests
                : BaseState, IDisposable
            {
                private readonly CSteamID _lobbyId;

                private CSteamID _lobbyOwner;

                private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
                private readonly DateTime _firstSendRequest;
                private DateTime _lastSendRequest = DateTime.MinValue;

                private string _status = "Establishing Connection";
                private bool _failed;
                private bool _succeeded;

                private Callback<P2PSessionConnectFail_t> _sessionConnectFailCallback;

                public SendingSessionRequests(CSteamID lobbyId)
                {
                    _lobbyId = lobbyId;
                    _firstSendRequest = DateTime.UtcNow;
                }

                public override void Activate()
                {
                    _lobbyOwner = SteamMatchmaking.GetLobbyOwner(_lobbyId);

                    _sessionConnectFailCallback = Callback<P2PSessionConnectFail_t>.Create(SessionConnectFailCallback);
                }

                private void SessionConnectFailCallback(P2PSessionConnectFail_t param)
                {
                    var err = (EP2PSessionError)param.m_eP2PSessionError;
                    Debug.LogError("Failed to connect to session: " + err);
                    _failed = true;
                }

                public override void Update()
                {
                    base.Update();

                    //Early exit if we've already failed or succeeded
                    if (_failed || _succeeded)
                        return;

                    //Check if we have timed out
                    var elapsed = DateTime.UtcNow - _firstSendRequest;
                    if (elapsed > TimeSpan.FromSeconds(10))
                    {
                        Debug.LogError("Host failed to respond");
                        _failed = true;
                    }

                    //Check if anything has failed, and if so leave the lobby
                    if (_failed)
                    {
                        SteamMatchmaking.LeaveLobby(_lobbyId);
                        return;
                    }

                    //Periodically send another hello packet
                    if (DateTime.UtcNow - _lastSendRequest > TimeSpan.FromMilliseconds(75))
                    {
                        //Send a reliable packet to the lobby owner. Contents aren't really important, what's important is that this will trigger a session request.
                        //When the session request is accepted, we'll transition to the in-game state
                        SteamNetworking.SendP2PPacket(_lobbyOwner, Encoding.ASCII.GetBytes("Dissonance Join Request"), 10, EP2PSend.k_EP2PSendReliable);
                        _lastSendRequest = DateTime.UtcNow;
                    }

                    //Check for successful connection
                    P2PSessionState_t state;
                    if (!SteamNetworking.GetP2PSessionState(_lobbyOwner, out state))
                        _status = "Establishing Connection...";
                    else if (state.m_bConnectionActive == 0)
                        _status = "Awaiting Connection Activation...";
                    else
                        _succeeded = true;
                }

                public override BaseState OnGUI()
                {
                    if (_failed)
                        return new LoadingLobbyList();

                    if (_succeeded)
                        return new LoadingGame(false, _lobbyId);

                    var timeRemaining = (_timeout - (DateTime.UtcNow - _firstSendRequest)).TotalSeconds;
                    GUILayout.Label("Connecting To Game:");
                    GUILayout.Label(" - " + _status);
                    GUILayout.Label(" - " + timeRemaining);

                    return null;
                }

                public void Dispose()
                {
                    if (_sessionConnectFailCallback != null)
                        _sessionConnectFailCallback.Dispose();
                    _sessionConnectFailCallback = null;
                }
            }
        }

        private sealed class CreatingGame
            : BaseState, IDisposable
        {
            private readonly bool _friendsOnly;

            private Callback<LobbyCreated_t> _lobbyCreatedCallback;
            private BaseState _next;

            public CreatingGame(bool friendsOnly)
            {
                _friendsOnly = friendsOnly;
            }

            public override void Activate()
            {
                //Setup a callback so we know when we've created a lobby
                _lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(LobbyCreatedCallback);

                //Create a lobby
                SteamMatchmaking.CreateLobby(_friendsOnly ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePublic, 8);
            }

            private void LobbyCreatedCallback(LobbyCreated_t result)
            {
                if (result.m_eResult == EResult.k_EResultOK)
                {
                    var lobby = new CSteamID(result.m_ulSteamIDLobby);

                    SteamMatchmaking.SetLobbyData(lobby, DissonanceLobbyKey, DissonanceLobbyVal);
                    SteamMatchmaking.SetLobbyData(lobby, "name", SteamFriends.GetPersonaName() + " - Dissonance Demo");
                    _next = new LoadingGame(true, lobby);
                }
                else
                {
                    Debug.LogError("Failed to create lobby, steam error: " + result.m_eResult);
                    _next = new LoadingLobbyList();
                }

                _lobbyCreatedCallback.Dispose();
                _lobbyCreatedCallback = null;
            }

            public override BaseState OnGUI()
            {
                GUILayout.Label("Creating Game...");

                return _next;
            }

            public void Dispose()
            {
                if (_lobbyCreatedCallback != null)
                    _lobbyCreatedCallback.Dispose();
                _lobbyCreatedCallback = null;
            }
        }

        private sealed class LoadingGame
            : BaseState, IDisposable
        {
            private readonly bool _host;
            private readonly CSteamID _lobby;

            private bool _loaded;

            public LoadingGame(bool host, CSteamID lobby)
            {
                _host = host;
                _lobby = lobby;
            }

            public override void Activate()
            {
                SceneManager.sceneLoaded += SceneLoaded;
                SceneManager.LoadScene("SteamworksDotnetGameWorld");
            }

            private void SceneLoaded(Scene scene, LoadSceneMode mode)
            {
                //We've loaded the game scene, so we need to tell Dissonance to startup in the right mode
                if (scene.name == "SteamworksDotnetGameWorld")
                {
                    var comms = FindObjectOfType<SteamworksP2PCommsNetwork>();
                    if (_host)
                    {
                        comms.InitializeAsServer();
                    }
                    else
                    {
                        comms.InitializeAsClient(SteamMatchmaking.GetLobbyOwner(_lobby));
                    }

                    _loaded = true;
                }
            }

            public override BaseState OnGUI()
            {
                GUILayout.Label("Loading Game...");

                if (_loaded)
                {
                    if (_host)
                        return new InGameAsHost(_lobby);
                    else
                        return new InGameAsClient(_lobby);
                }
                else
                    return null;
            }

            public void Dispose()
            {
                SceneManager.sceneLoaded -= SceneLoaded;
            }
        }

        private sealed class LeavingGame
            : BaseState
        {
            private bool _exited;

            private readonly SteamworksP2PCommsNetwork _commsNetwork;
            private readonly CSteamID _lobby;

            private bool _complete;
            private AsyncOperation _async;

            public LeavingGame(SteamworksP2PCommsNetwork commsNetwork, CSteamID lobby)
            {
                _commsNetwork = commsNetwork;
                _lobby = lobby;
            }

            public override void Activate()
            {
            }

            public override void Update()
            {
                base.Update();

                if (_async == null)
                    _async = SceneManager.LoadSceneAsync("SteamworksDotnetDemo", LoadSceneMode.Single);
                if (_async.isDone)
                    _complete = true;

                if (!_exited)
                {
                    LeaveSession(_lobby, _commsNetwork);

                    _exited = true;
                }
            }

            public override BaseState OnGUI()
            {
                if (_exited && _complete)
                    return new LoadingLobbyList();
                else
                    return null;
            }
        }
        #endregion

        #region in game states
        private abstract class BaseInGame
            : BaseState, IDisposable
        {
            #region fields and properties
            private Callback<P2PSessionRequest_t> _requestSessionCallback;
            private Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;

            public readonly CSteamID Lobby;
            private readonly CSteamID _lobbyHost;
            private readonly string _lobbyName;

            private SteamworksDemoPlayerManager _playerManager;

            private SteamworksP2PCommsNetwork _commsNetwork;
            public SteamworksP2PCommsNetwork CommsNetwork { get { return _commsNetwork; } }

            private bool _exit;
            #endregion

            protected BaseInGame(CSteamID lobby)
            {
                Lobby = lobby;
                _lobbyName = SteamMatchmaking.GetLobbyData(Lobby, "name");
                _lobbyHost = SteamMatchmaking.GetLobbyOwner(Lobby);
            }

            public override void Activate()
            {
                _commsNetwork = FindObjectOfType<SteamworksP2PCommsNetwork>();

                _playerManager = FindObjectOfType<SteamworksDemoPlayerManager>();
                _playerManager.LobbyId = Lobby;

                _requestSessionCallback = Callback<P2PSessionRequest_t>.Create(RequestSessionCallback);
                _lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(LobbyChatUpdateCallback);

                //Inform dissonance about all the peers already in the session
                var count = SteamMatchmaking.GetNumLobbyMembers(Lobby);
                for (var i = 0; i < count; i++)
                    _commsNetwork.PeerConnected(SteamMatchmaking.GetLobbyMemberByIndex(Lobby, i));
            }

            #region session management
            private void LobbyChatUpdateCallback(LobbyChatUpdate_t param)
            {
                // If we received an event for some other lobby ignore it
                if (new CSteamID(param.m_ulSteamIDLobby) != Lobby)
                    return;

                switch ((EChatMemberStateChange)param.m_rgfChatMemberStateChange)
                {

                    //If someone left inform Dissonance (or if it was the host, exit the game altogether)
                    case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                    case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                    case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                    case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                        var player = new CSteamID(param.m_ulSteamIDUserChanged);
                        if (player == _lobbyHost)
                            _exit = true;
                        else
                            PlayerDisconnected(new CSteamID(param.m_ulSteamIDUserChanged));
                        break;

                    case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private void RequestSessionCallback(P2PSessionRequest_t param)
            {
                //If this player is not in the lobby, reject them
                var isPlayerInLobby = Enumerable.Range(0, SteamMatchmaking.GetNumLobbyMembers(Lobby))
                    .Select(i => SteamMatchmaking.GetLobbyMemberByIndex(Lobby, i))
                    .Any(m => m.m_SteamID == param.m_steamIDRemote.m_SteamID);

                if (!isPlayerInLobby)
                {
                    Debug.LogError("Rejecting session request: Not In Lobby");
                    Reject(param.m_steamIDRemote);
                    return;
                }

                //If this player does not have the correct key set, reject them
                var password = SteamMatchmaking.GetLobbyMemberData(Lobby, param.m_steamIDRemote, DissonanceLobbyKey);
                if (password != DissonanceLobbyVal)
                {
                    Debug.LogError("Rejecting session request: Wrong User Data Key ('" + password + "')");
                    Reject(param.m_steamIDRemote);
                    return;
                }

                //They're in the lobby, accept them
                Debug.Log("Accepted session request");
                Accept(param.m_steamIDRemote);
            }

            private static void Reject(CSteamID user)
            {
                SteamNetworking.CloseP2PSessionWithUser(user);
            }

            private void Accept(CSteamID user)
            {
                SteamNetworking.AcceptP2PSessionWithUser(user);
                SteamNetworking.SendP2PPacket(user, Encoding.ASCII.GetBytes("Dissonance Join Response"), 10, EP2PSend.k_EP2PSendReliable);

                _commsNetwork.PeerConnected(user);
            }

            private void PlayerDisconnected(CSteamID user)
            {
                var name = SteamFriends.GetFriendPersonaName(user);
                Debug.Log("Player Left Steam Session: " + name);

                _commsNetwork.PeerDisconnected(user);
                Reject(user);
            }
            #endregion

            public override BaseState OnGUI()
            {
                if (GUILayout.Button(new GUIContent("Exit Room")))
                    _exit = true;

                GUILayout.Label("Lobby ID: " + Lobby);
                GUILayout.Label("Lobby Name: " + _lobbyName);

                if (_exit)
                    return new LeavingGame(_commsNetwork, Lobby);
                else
                    return null;
            }

            public void Dispose()
            {
                //Dispose callbacks
                if (_requestSessionCallback != null)
                    _requestSessionCallback.Dispose();
                _requestSessionCallback = null;

                if (_lobbyChatUpdateCallback != null)
                    _lobbyChatUpdateCallback.Dispose();
                _lobbyChatUpdateCallback = null;
            }
        }

        private sealed class InGameAsClient
            : BaseInGame
        {
            public InGameAsClient(CSteamID lobby)
                : base(lobby)
            {
            }
        }

        private sealed class InGameAsHost
            : BaseInGame
        {
            public InGameAsHost(CSteamID lobby)
                : base(lobby)
            {
            }
        }
        #endregion
    }
}
