using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PingController : NetworkBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _pings;
    [SerializeField] private float _pingSpawnDuration = .5f;
    [SerializeField] bool _availSpawnPing = true;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        InputHandler.Instance.OnPing += GetPingPositionAndRotation;
    }
    public override void OnNetworkDespawn()
    {
        InputHandler.Instance.OnPing -= GetPingPositionAndRotation;
        base.OnNetworkDespawn();
    }

    void GetPingPositionAndRotation()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 offset = new Vector3(hit.point.x, hit.point.y, hit.point.z) + hit.normal * 0.1f;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            Debug.Log($"핑충돌! 각도{hit.normal}");
            StartCoroutine(TimedSpawnPing(offset, rotation, IsHost, _pingSpawnDuration));
            //RequestSpawnPingServerRpc(offset, rotation, IsHost);
        }

    }
    IEnumerator TimedSpawnPing(Vector3 position, Quaternion rotation, bool isHost, float duration)
    {
        if (_availSpawnPing)
        {
            _availSpawnPing = false;
            RequestSpawnPingServerRpc(position, rotation, isHost);
            yield return new WaitForSeconds(duration);
            _availSpawnPing = true;
        }
    }
    
    [ServerRpc(RequireOwnership =false)]
    void RequestSpawnPingServerRpc(Vector3 position, Quaternion rotation, bool isHost)
    {
        SpawnPingClientRpc(position, rotation, isHost);
    }

    [ClientRpc]
    void SpawnPingClientRpc(Vector3 position, Quaternion rotation, bool isHost)
    {
        GameObject pingObject = Instantiate(_pings, position, rotation);
        pingObject.GetComponent<Ping>().SpawnPing(isHost);
    }
    
}
