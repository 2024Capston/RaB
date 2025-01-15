using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 물체 아래에 있는 플랫폼을 탐색하는 Class
/// </summary>
public class NetworkPlatformFinder : NetworkBehaviour
{
    /// <summary>
    /// 탐색 범위
    /// </summary>
    [SerializeField] private float _detectionThreshold = 10f;

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

        Rigidbody newPlatform = null;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, _detectionThreshold);

        if (hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                // !! 플랫폼을 따로 구별할 수 있도록 Layer나 Tag로 수정할 것 !!
                if (hit.collider.gameObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject) &&
                    hit.collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody) &&
                    hit.collider.gameObject.name == "Elevator")
                {
                    newPlatform = rigidbody;
                }
            }
        }

        // 새로운 플랫폼을 발견한 경우
        if (_platform?.gameObject != newPlatform)
        {
            _platform = newPlatform;
            _networkSyncTransform.SetParent(newPlatform?.gameObject);
        }
    }
}