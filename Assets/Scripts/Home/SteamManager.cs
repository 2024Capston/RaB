using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SteamManager : SingletonBehavior<SteamManager>
{
    protected override void Init()
    {
        _isDestroyOnLoad = true;
        base.Init();
    }

    private void OnEnable()
    {
        // 로비가 생성되면 해당 델리게이트를 호출한다.
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
    }

    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);

            NetworkManager.Singleton.StartHost();
        }
    }

    private void LobbyEntered(Lobby lobby)
    {
        LobbyManager.Instance.CurrentLobby = lobby;
        // LobbyID.text = lobby.Id.ToString();
        Logger.Log("We entered Lobby");


        if (NetworkManager.Singleton.IsHost) return;

        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
    }

    private async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        await lobby.Join();
    }
}
