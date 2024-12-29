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
    
    [SerializeField] private List<GameObject> _networkObjects;

    [SerializeField] private List<GameObject> _localObjects;

    [SerializeField] private List<NetworkObject> _instantiatedNetworkObjects;

    [SerializeField] private List<GameObject> _instantiatedLocalObjects;
    
    public void LoadStage()
    {
        Init();
        
        // NetworkObject는 모두 서버에서 Spawn
        LoadAllNetworkObjects();
        
        // Map은 Client에 개별적으로 Load 한다.
        LoadAllLocalObject(); 
        
        // InGameManager에도 자신을 등록한다.
        InGameManager.Instance.StageLoader = this;
    }

    public void DestoryStage()
    {   
        Init();
        
        DestroyAllNetworkObject();
        DestoryAllLocalObjectClientRpc();
    }
    
    private void Init()
    {
        _clientLoadCount = 0;
    }

    /// <summary>
    /// _networkObject List에 저장된 모든 NetworkObject를 서버 상에서 Spawn 하는 메소드
    /// </summary>
    private void LoadAllNetworkObjects()
    {
        foreach (GameObject networkObject in _networkObjects)
        {
            NetworkObject instantiatedNetworkObject = Instantiate(networkObject).GetComponent<NetworkObject>();

            if (!instantiatedNetworkObject)
            {
                Logger.LogError($"{networkObject.name} dose not have a NetworkObject Component.");
                
                // TODO
                // 예외 처리 로직이 필요합니다.
                // ex) 이전 맵으로 돌아가던가 하는?
                continue;
            }
            
            _instantiatedNetworkObjects.Add(instantiatedNetworkObject);
            instantiatedNetworkObject.Spawn();
        }
    }
    
    /// <summary>
    /// _localObject List에 저장된 모든 Object를 각 클라이언트에서 Spawn 하는 메소드
    /// </summary>
    [ClientRpc]
    private void LoadAllLocalObject()
    {
        
        foreach (GameObject localObject in _localObjects)
        {
            GameObject instantiateLocalObject = Instantiate(localObject);
            _instantiatedLocalObjects.Add(instantiateLocalObject);
        }
        
        CompleteLoadAllLocalObjectServerRpc();
    }

    [ServerRpc]
    private void CompleteLoadAllLocalObjectServerRpc()
    {
        if (++_clientLoadCount == 2)
        {
            // Client의 Loading이 모두 완료되었으므로 게임 시작
            InGameManager.Instance.StartGame();
        }
        // TODO 
        // 둘 중 한 플레이어의 Load가 TimeOut되면 연결을 초기화 한다.
    }

    private void DestroyAllNetworkObject()
    {
        foreach (NetworkObject networkObject in _instantiatedNetworkObjects)
        {
            networkObject.Despawn();
            Destroy(networkObject);
        }
        _instantiatedNetworkObjects.Clear();
    }

    [ClientRpc]
    private void DestoryAllLocalObjectClientRpc()
    {
        foreach (GameObject localObject in _instantiatedLocalObjects)
        {
            Destroy(localObject);
        }
        _instantiatedLocalObjects.Clear();
        
        CompleteDestoryAllLocalObjectServerRpc();
    }

    [ServerRpc]
    private void CompleteDestoryAllLocalObjectServerRpc()
    {
        if (++_clientLoadCount == 2)
        {
            // 모든 Object의 파괴가 완료되었으니 다시 생성
            LoadStage();
        }
    }
}
 