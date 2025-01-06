using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Server에서 모든 Player를 관리하고 있는 객체
/// </summary>
public class PlayerManager : NetworkSingletonBehaviour<PlayerManager>
{
    // PlayerManager는 멀티 플레이 상황에서만 유지하고 HomeScene으로 이동할 때 파괴해주어야 한다. 
    // 이는 전체 Scene을 관통하는 매니저에서 추후 해줘야할 일일 듯
    
    private readonly string PLAYER_PATH = "Prefabs/Object/Player/Player";    

    private List<PlayerController> _players = new List<PlayerController>();
    
    private ClientRpcParams _clientRpcParams = new ClientRpcParams();
    
    private GameObject _playerPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Logger.Log("NetworkSpawn");
        // PlayerPrefab을 캐싱한다.
        _playerPrefab = Resources.Load<GameObject>(PLAYER_PATH);
        if (_playerPrefab == null)
        {
            Logger.LogError("PlayerPrefabs does not exist");
            return;
        }
        
        // ClientRpcParams를 캐싱한다.
        _clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[1] 
            }
        };
    }
    

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        GameObject player = Instantiate(Resources.Load<GameObject>(PLAYER_PATH));
        player.transform.position = Vector3.zero;
        player.transform.rotation = Quaternion.identity;

        player.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        _players.Add(player.GetComponent<PlayerController>());  
    }

    public void DespawnClientPlayer()
    {
        if (_players.Count == 2)
        {
            _players.RemoveAt(1);
        }
    }

}
