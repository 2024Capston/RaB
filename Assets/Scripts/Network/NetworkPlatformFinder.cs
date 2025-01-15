using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 물체 아래에 있는 플랫폼을 탐색하는 Class
/// </summary>
public class NetworkPlatformFinder : NetworkBehaviour
{
    [SerializeField] private float _detectionThreshold = 10f;   // 플랫폼 탐색 범위

    private CharacterController _characterController;
    private NetworkSyncTransform _networkSyncTransform;
    private Vector3 _lastPosition;                              // 속도 계산용 마지막 위치

    private Rigidbody _platform;    // 현재 찾은 플랫폼. 없는 경우 Null
    public Rigidbody Platform
    {
        get => _platform;
    }

    private Vector3 _velocity;      // 현재 찾은 플랫폼의 속도
    public Vector3 Velocity
    {
        get => _velocity;
    }
   

    public override void OnNetworkSpawn()
    {
        _networkSyncTransform = GetComponent<NetworkSyncTransform>();
        _characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        Rigidbody newPlatform = null;
        RaycastHit[] hits = new RaycastHit[0];

        if (_characterController && false)
        {
            Vector3 offset = Vector3.up * (_characterController.height / 2f - _characterController.radius);
            hits = Physics.CapsuleCastAll(transform.position + offset, transform.position - offset, _characterController.radius, Vector3.down, _detectionThreshold);
        }
        else
        {
            hits = Physics.RaycastAll(transform.position, Vector3.down, _detectionThreshold);
        }

        if (hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject) &&
                    hit.collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
                {
                    newPlatform = rigidbody;
                }
            }
        }

        // 새로운 플랫폼을 발견한 경우
        if (_platform != newPlatform)
        {
            Debug.Log("Changed Platform");
            _platform = newPlatform;
            _networkSyncTransform.SetParent(newPlatform?.gameObject);

            if (_platform)
            {
                _lastPosition = _platform.position;
            }
        }

        // 플랫폼이 있는 경우 속도 갱신
        if (_platform)
        {
            _velocity = (_platform.transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = _platform.transform.position;
        }
    }
}