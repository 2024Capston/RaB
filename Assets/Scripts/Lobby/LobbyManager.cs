using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkSingletonBehaviour<LobbyManager>
{
    private readonly string LOBBY_PATH = "Prefabs/Lobby/";
    
    public LobbyUIController LobbyUIController {  get; private set; }

    //  TODO
    // 퇴장할 때 처리가 필요합니다.
    
    protected override void Init()
    {
        _isDestroyOnLoad = true;
        base.Init();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        LobbyUIController = FindObjectOfType<LobbyUIController>();

        if (!LobbyUIController)
        {
            Logger.LogError("LobbyUIManager does not exist");
            return;
        }

        LobbyUIController.SetPlayerColorData(IsHost);

        // 1. 맵을 불러온다.
        GameObject map = Resources.Load<GameObject>(LOBBY_PATH + "Map");
        if (map == null)
        {
            Logger.LogError(LOBBY_PATH + " Map has does not exist");
            return;
        }
        Instantiate(map);

        // 2. 자신의 Player를 Spawn한다.
        PlayerManager.Instance.SpawnPlayerServerRpc();
        
        UIManager.Instance.CloseAllOpenUI();
    }

    /// <summary>
    /// InGame으로 이동하는 메소드 
    /// </summary>
    /// <param name="stageName"></param>
    [ServerRpc]
    public void LoadInGameServerRpc(StageName stageName)
    {
        // InGame Scene으로 이동하면 ConnectionManager에 있는 SelectStage로 Loader를 불러온다.
        SessionManager.Instance.SelectedStage = stageName;
        NetworkManager.SceneManager.LoadScene(SceneType.InGame.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
