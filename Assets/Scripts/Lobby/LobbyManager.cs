using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkSingletonBehaviour<LobbyManager>
{
    private readonly string LOBBY_PATH = "Prefabs/Lobby/";


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

        if (LobbyUIController == null)
        {
            Logger.LogError("LobbyUIManager does not exist");
            return;
        }

        LobbyUIController.SetPlayerColorData(IsHost);

        GameObject gameObject = Instantiate(Resources.Load<GameObject>(LOBBY_PATH + "PlayerManager"));
        gameObject.GetComponent<NetworkObject>().Spawn();

        /* 
         TODO 
         
        2. 현재 저장 상태에 맞게 맵 시스템을 조정 -> 맵이 나중에 만들어질 때
        3. 자신 Player를 불러온다.
        4. 상대 Player를 불러온다.
         
         */

        // 1. 맵을 불러온다. -> 추후 LobbyMapManager 같은 곳에서 제어가 필요할 것으로 보입니다.
        GameObject map = Resources.Load<GameObject>(LOBBY_PATH + "Map");
        if (map == null)
        {
            Logger.LogError(LOBBY_PATH + "Map has does not exist");
            return;
        }
        Instantiate(map);

        // 게임 매니저에서 현재 클라이언트 정보를 가져와서 플레이어를 스폰 시킨다?
        PlayerManager.Instance.SpawnPlayerServerRpc();
        UIManager.Instance.CloseAllOpenUI();
    }
}
