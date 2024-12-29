using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class InGameManager : NetworkSingletonBehaviour<InGameManager>
{
    public StageLoader StageLoader { get; set; }
    
    protected override void Init()
    {
        _isDestroyOnLoad = true;
        base.Init();

        StageLoader = null;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
        {
            return;
        }
        
        // 시작한 스테이지를 가져온다.
        StageName stageName = ConnectionManager.Instance.SelectStage;
        if (stageName == StageName.Size)
        {
            Logger.LogError("StageName MissMatch");
            return;
        }
        
        // 해당 스테이지의 Loader를 생성한다.
        StageLoadManager.Instance.LoadStage(stageName);
    }

    public void StartGame()
    {
        StageManager.Instance.StartGame();
    }
    
    /// <summary>
    /// 게임이 종료되었으면 LobbyScene으로 되돌아간다.
    /// </summary>
    [ServerRpc]
    public void EndGameServerRpc()
    {
        UserGameData userGameData = UserDataManager.Instance.GetUserData<UserGameData>();
        userGameData.UpdateData(ConnectionManager.Instance.SelectPlayData, ConnectionManager.Instance.SelectStage, 1);
    }
}
