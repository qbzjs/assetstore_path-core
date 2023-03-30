using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using HeathenEngineering.SteamworksIntegration;
using Mirror;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    // private Callback<LobbyCreated_t> lobbyCreatedCallback;
    // private Callback<GameLobbyJoinRequested_t> lobbyJoinRequestedCallback;
    // private Callback<LobbyEnter_t> lobbyEnteredCallback;
    //
    // [SerializeField] private int MaxPeopleInLobby = 8;

    private NetworkManager _networkManager;

    // [SerializeField] private CSteamID lobbyID;
    //
    // [SerializeField] private GameObject partCanvasObject;
    //
    //
    // [SerializeField] private List<TMP_Text> playerNamesText_TextObjects = new List<TMP_Text>(8);
    //
    // Start is called before the first frame update
    void Start()
    {
         _networkManager = FindObjectOfType<NetworkManager>();
        // Debug.Log(_networkManager.isNetworkActive + " : server active");
        // _networkManager.StartServer();
        // Debug.Log(_networkManager.isNetworkActive + " : server active");
        //
        // lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        // lobbyEnteredCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        // lobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
        //
        // CreateFriendsOnlyLobby();
        _networkManager.StartHost();
        
    }

//     private void OnLobbyCreated(LobbyCreated_t callback)
//     {
//         if (callback.m_eResult == EResult.k_EResultOK)
//         {
//             partCanvasObject.SetActive(true);
//             lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
//             SteamMatchmaking.SetLobbyData(lobbyID, "HostAddress", SteamUser.GetSteamID().ToString());
//             Debug.Log("Lobby created ");
//             // _networkManager.StartHost();
//             //setup lobby data here.
//             //this is where we would put other configs for this lobby.
//         }
//         else
//         {
//             Debug.LogError($"Error: Failed to create lobby - {callback.m_eResult}");
//         }
//     }
//
//     private void OnLobbyEntered(LobbyEnter_t callback)
//     {
//         string hostAddress = SteamMatchmaking.GetLobbyData(lobbyID, "HostAddress");
//         NetworkManager.singleton.networkAddress = hostAddress;
//         ClientHandleInfoUpdated();
//
//     }
//
//     private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
//     {
//         SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
//         Debug.Log("Joining the lobby.");
//     }
//
//     private void CreateFriendsOnlyLobby()
//     {
//         SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, MaxPeopleInLobby);
//     }
//
//     private void ClientHandleInfoUpdated()
//     {
//         for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(lobbyID); i++)
//         {
//             CSteamID currLobbPlayer = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
//
//             Debug.Log(SteamFriends.GetFriendPersonaName(currLobbPlayer));
//             playerNamesText_TextObjects[i].text = SteamFriends.GetFriendPersonaName(currLobbPlayer);
//         }
//
//         for (int i = SteamMatchmaking.GetNumLobbyMembers(lobbyID); i < playerNamesText_TextObjects.Count; i++)
//         {
//             playerNamesText_TextObjects[i].text = "Waiting For Player...";
//         }
//
// //        startGameButton.interactable = players.Count >= 2;
//     }
}