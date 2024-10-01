using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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

    /// <summary>
    /// Lobby를 생성하는 메소드
    /// </summary>
    public async void CreateLobby()
    {
        await SteamMatchmaking.CreateLobbyAsync(2);
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
        GameManager.Instance.CurrentLobby = lobby;
        // LobbyID.text = lobby.Id.ToString();
        Logger.Log("We entered Lobby");


        if (NetworkManager.Singleton.IsHost)
        {
            LoadLobbyScene();
            return;

        }

        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();

        LoadLobbyScene();
    }

    private async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        await lobby.Join();
    }

    private void LoadLobbyScene()
    {
        UIManager.Instance.CloseAllOpenUI();
        Logger.Log($"Lobby ID : {GameManager.Instance.CurrentLobby.Value.Id}");

        NetworkManager.Singleton.SceneManager.LoadScene(SceneType.Lobby.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
