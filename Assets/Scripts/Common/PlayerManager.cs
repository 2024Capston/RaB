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

    private GameObject _playerPrefab;

    protected override void Init()
    {
        base.Init();

        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _playerPrefab = Resources.Load<GameObject>(PLAYER_PATH);

        if (_playerPrefab == null)
        {
            Logger.LogError("PlayerPrefabs does not exist");
            return;
        }

        Logger.Log("PlayerManager does NetworkSpawn");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        GameObject gameObject = Instantiate(Resources.Load<GameObject>(PLAYER_PATH));
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;

        gameObject.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        _players.Add(gameObject.GetComponent<PlayerController>());  
    }

}
