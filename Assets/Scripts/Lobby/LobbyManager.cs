using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkSingletonBehaviour<LobbyManager>
{
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
        
        /* 
         TODO 
        1. 맵을 불러온다.
        2. 현재 저장 상태에 맞게 맵 시스템을 조정
        3. 자신 Player를 불러온다.
        4. 상대 Player를 불러온다.
         
         */
        
        UIManager.Instance.CloseAllOpenUI();
    }

    


}
