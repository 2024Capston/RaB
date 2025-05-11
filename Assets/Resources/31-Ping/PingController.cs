using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PingController : NetworkBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _pings;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        InputHandler.Instance.OnPing += GetPingPositionAndRotation;
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
            //if(hit.collider.gameObject.layer == 8)
            //{
            Vector3 offset = new Vector3(hit.point.x, hit.point.y, hit.point.z) + hit.normal * 0.1f;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            //}

            Debug.Log($"핑충돌! 각도{hit.normal}");
            RequestSpawnPingServerRpc(offset, rotation);
        }

    }
    [ServerRpc(RequireOwnership =false)]
    void RequestSpawnPingServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject pingObject = Instantiate(_pings, position, rotation);
        pingObject.GetComponent<Ping>().RequestPlayer = IsHost;
        pingObject.GetComponent<NetworkObject>().Spawn();
    }
    
}
