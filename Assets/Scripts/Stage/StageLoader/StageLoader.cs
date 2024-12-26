using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Stage들을 Load하는 Class
/// </summary>
public class StageLoader : NetworkBehaviour
{
    private int _clientLoadCount;
    
    /*
     * StageLoader에서 할 일
     * 1. 등록되어 있는 StageManager를 생성한다.
     * 2. 맵들을 불러온다.
     */
    [SerializeField] private GameObject _stageManager;
    
    [SerializeField] private GameObject _map;

    /// <summary>
    /// 실제 생성된 Map Object
    /// </summary>
    private GameObject _currentMap;

    protected void Init()
    {
        _clientLoadCount = 0;
        _currentMap = null;
    }
    
    public void LoadStage()
    {
        Init();

        if (!_stageManager)
        {
            Logger.LogError("stageManager dose not exist");
            return;
        }
        
        // StageManager는 Server에서 생성하여 공유한다.
        GameObject stageManager = Instantiate(_stageManager);
        stageManager.GetComponent<NetworkObject>().Spawn();
        
        // Map은 Client에 개별적으로 Load 한다.
        LoadMapClientRpc(); 
        
        // InGameManager에도 자신을 등록한다.
        InGameManager.Instance.StageLoader = this;
    }

    public void ResetStage()
    {
        
    }

    [ClientRpc]
    private void LoadMapClientRpc()
    {
        _currentMap = Instantiate(_map);
        CompleteLoadMapServerRpc();
    }

    [ServerRpc]
    private void CompleteLoadMapServerRpc()
    {
        if (++_clientLoadCount == 2)
        {
            // Client의 Loading이 모두 완료되었으므로 게임 시작
            InGameManager.Instance.StartGame();
        }
        // TODO 
        // 둘 중 한 플레이어의 Load가 TimeOut되면 연결을 초기화 한다.
    }
    
}
 