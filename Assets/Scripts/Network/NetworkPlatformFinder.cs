using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlatformFinder : NetworkBehaviour
{
    [SerializeField] private float _detectionThreshold = 3f;

    private NetworkSyncTransform _networkSyncTransform;

    private Rigidbody _platform;
    public Rigidbody Platform
    {
        get => _platform;
    }

    public override void OnNetworkSpawn()
    {
        _networkSyncTransform = GetComponent<NetworkSyncTransform>();
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, _detectionThreshold);

        if (hits.Length > 0)
        {
            bool platformFound = false;

            foreach (RaycastHit hit in hits)
            {
                // !! 플랫폼을 따로 구별할 수 있도록 Layer나 Tag로 수정할 것 !!
                if (hit.collider.gameObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject) &&
                    hit.collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody) &&
                    hit.collider.gameObject.name == "Elevator")
                {
                    // 새로운 플랫폼을 발견한 경우
                    if (_platform?.gameObject != hit.collider.gameObject)
                    {
                        _platform = rigidbody;
                        _networkSyncTransform.SetParent(networkObject.gameObject);
                    }

                    platformFound = true;
                    break;
                }
            }

            if (!platformFound && !_platform)
            {
                _networkSyncTransform.SetParent(null);
                _platform = null;
            }
        }
        else if (_platform != null)
        {
            _networkSyncTransform.SetParent(null);
            _platform = null;
        }
    }

    private void OnGUI()
    {
        if (IsOwner && gameObject.GetComponent<PlayerController>() != null)
        {
            GUILayout.BeginArea(new Rect(10, 10, 100, 100));
            GUILayout.Label($"{_platform?.name}");
            GUILayout.EndArea();
        }
    }
}
