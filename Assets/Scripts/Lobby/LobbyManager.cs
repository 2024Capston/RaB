using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LobbyManager : NetworkSingletonBehaviour<LobbyManager>
{
    [SerializeField] private GameObject[] _playerPrefabs = new GameObject[2];
    [SerializeField] private Transform[] _spawnPoints = new Transform[2];
    [SerializeField] private AirlockController[] _airlockControllers = new AirlockController[6];
    public LobbyUIController LobbyUIController {  get; private set; }
    
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
    }

    private void Start()
    {
        Logger.Log($"{GetType()}::Start");
        SetMapData();
        SpawnPlayerServerRpc();
        UIManager.Instance.CloseAllOpenUI();
    }

    /// <summary>
    /// 엘리베이터에서 층을 이동할 때 호출됩니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveFloorServerRpc(int floor)
    {
        
    }
    
    /// <summary>
    /// 현재 선택한 Stage를 저장하고 InGame Scene으로 이동합니다.
    /// </summary>
    /// <param name="stageName"></param>
    public void RequestTrasitionInGameScene(StageName stageName)
    {
        // InGame Scene으로 이동하면 ConnectionManager에 있는 SelectStage로 Loader를 불러온다.
        SessionManager.Instance.SelectedStage = stageName;


        foreach (ulong playerId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            PlayerConfig playerConfig = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<PlayerConfig>();
            playerConfig.MyPlayer.GetComponent<NetworkObject>().Despawn();
        }
        
        SceneLoaderWrapper.Instance.LoadScene(SceneType.InGame.ToString(), true);
    }
    
    /// <summary>
    /// PlayerConfig를 참고하여 Player를 스폰합니다.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        PlayerConfig playerConfig = NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(serverRpcParams.Receive.SenderClientId).GetComponent<PlayerConfig>();
        int isBlue = playerConfig.IsBlue ? 1 : 0;
        GameObject player = Instantiate(_playerPrefabs[isBlue]);
    /*    player.transform.position = _spawnPoints[isBlue].position;
        player.transform.rotation = _spawnPoints[isBlue].rotation;*/
        
        player.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerConfig.MyPlayer = playerController;
        //playerController.PlayerColor = playerConfig.IsBlue ? ColorType.Blue : ColorType.Red;
    }

    /// <summary>
    /// 각 Airlock에 Stage를 할당하고, 해당 Stage Data에 맞게 Airlock 문을 개방합니다.
    /// </summary>
    private void SetMapData()
    {
        PlayData data = SessionManager.Instance.SelectedData;
        for (int i = 0; i < 6; i++)
        {
            // 해당 스테이지를 클리어 했다면 문을 연다.
            _airlockControllers[(SessionManager.Instance.CurrentFloor - 1) * 6 + i].IsAirlockOpened =
                data.MapInfoList[(SessionManager.Instance.CurrentFloor - 1) * 6 + i].ClearFlag != 0;
            
            _airlockControllers[(SessionManager.Instance.CurrentFloor - 1) * 6 + i].StageName = (StageName)((SessionManager.Instance.CurrentFloor - 1) * 6 + i);
        }
    }
}

