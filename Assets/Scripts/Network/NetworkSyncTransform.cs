using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 오브젝트 Transform의 동기화를 담당하는 Class.
/// 오브젝트의 Owner가 상대방에게 Transform 정보를 전달한다.
/// </summary>
public class NetworkSyncTransform : NetworkBehaviour
{
    [SerializeField] private float _sendThreshold = 0.01f;      // transform 값을 전송하는 기준 값
    [SerializeField] private float _parentingCooldown = 0.5f;   // parenting 보간에 걸리는 시간

    private NetworkInterpolator _networkInterpolator;   // 현재 오브젝트의 interpolator

    private GameObject _parent;             // 현재 오브젝트가 parent로 간주하는 오브젝트
    private bool _isParenting = false;      // parenting 보간이 진행 중인지 여부
    private float _currentCooldown = 0f;    // parenting 보간에 사용하는 시간 변수

    // transform 정보 관련
    private Vector3 _lastSyncedPosition;    // 마지막으로 주고 받은 위치
    private Quaternion _lastSyncedRotation; // 마지막으로 주고 받은 회전

    public override void OnNetworkSpawn()
    {
        _networkInterpolator = GetComponent<NetworkInterpolator>();
    }

    void Update()
    {
        if (IsOwner)
        {
            SendTransform();
        }
        else if (_lastSyncedPosition != null && _lastSyncedRotation != null)
        {
            // interpolator가 존재하는 경우 parenting 보간 진행
            if (_networkInterpolator && _isParenting)
            {
                _currentCooldown -= Time.deltaTime;
                
                if (_currentCooldown <= 0f)
                {
                    _currentCooldown = 0f;
                    _isParenting = false;

                    _networkInterpolator.SetMoveTowards(false);
                }
            }

            UpdateFetchedTransform();
        }
    }

    /// <summary>
    /// 현재 transform을 상대방에게 전달한다.
    /// </summary>
    private void SendTransform()
    {
        Vector3 positionToSend = transform.position;

        // parent가 있는 있는 경우, 상대(relative) 위치를 전송.
        if (_parent)
        {
            positionToSend -= _parent.transform.position;
        }

        float positionDiff = Vector3.Distance(transform.position, _lastSyncedPosition);
        float rotationDiff = Quaternion.Angle(transform.rotation, _lastSyncedRotation) / 10f;

        // transform 변화값이 기준을 넘어서는 경우에만 전송
        if (positionDiff < _sendThreshold && rotationDiff < _sendThreshold)
        {
            return;
        }

        // 마지막으로 전송한 transform 값 저장
        _lastSyncedPosition = positionToSend;
        _lastSyncedRotation = transform.rotation;

        if (IsServer)
        {
            SendTransformClientRpc(positionToSend, transform.rotation);
        }
        else
        {
            SendTransformServerRpc(positionToSend, transform.rotation);
        }
    }

    /// <summary>
    /// 상대방으로부터 받아온 transform을 적용한다.
    /// </summary>
    private void UpdateFetchedTransform()
    {
        if (_parent)
        {
            transform.position = _lastSyncedPosition + _parent.transform.position;
            transform.rotation = _lastSyncedRotation;
        }
        else
        {
            transform.position = _lastSyncedPosition;
            transform.rotation = _lastSyncedRotation;
        }
    }

    /// <summary>
    /// 오브젝트의 parent를 갱신한다. NULL로 설정하면 parent를 제거한다.
    /// </summary>
    /// <param name="parent">parent</param>
    public void SetParent(GameObject parent)
    {
        if (parent)
        {
            if (parent.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                _parent = parent;

                if (IsServer)
                {
                    SetParentClientRpc(networkObject);
                }
                else
                {
                    SetParentServerRpc(networkObject);
                }
            }
            else
            {
                Logger.Log("Synced Parent must have Network Object as its component.");
            }
        }
        else
        {
            _parent = null;

            if (IsServer)
            {
                ResetParentClientRpc();
            }
            else
            {
                ResetParentServerRpc();
            }
        }
    }

    /// <summary>
    /// 클라이언트에서 서버로 transform을 전달한다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <param name="rotation">회전</param>
    [ServerRpc]
    public void SendTransformServerRpc(Vector3 position, Quaternion rotation)
    {
        _lastSyncedPosition = position;
        _lastSyncedRotation = rotation;
    }

    /// <summary>
    /// 서버에서 클라이언트로 transform을 전달한다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <param name="rotation">회전</param>
    [ClientRpc]
    public void SendTransformClientRpc(Vector3 position, Quaternion rotation)
    {
        if (IsServer)
        {
            return;
        }

        _lastSyncedPosition = position;
        _lastSyncedRotation = rotation;
    }

    /// <summary>
    /// RPC를 통해 전달받은 parent를 설정한다. 필요하면 parenting 보간을 시작한다.
    /// </summary>
    /// <param name="parent"></param>
    private void ApplyParent(GameObject parent)
    {
        _parent = parent;

        if (_networkInterpolator)
        {
            _networkInterpolator.SetMoveTowards(true);

            _isParenting = true;
            _currentCooldown = _parentingCooldown;
        }
    }

    /// <summary>
    /// 클라이언트에서 서버로 새 parent를 전달한다.
    /// </summary>
    /// <param name="parent">parent</param>
    [ServerRpc]
    public void SetParentServerRpc(NetworkObjectReference parent)
    {
        if (parent.TryGet(out NetworkObject networkObject))
        {
            ApplyParent(parent);
        }
    }

    /// <summary>
    /// 서버에서 클라이언트로 새 parent를 전달한다.
    /// </summary>
    /// <param name="parent">parent</param>
    [ClientRpc]
    public void SetParentClientRpc(NetworkObjectReference parent)
    {
        if (IsServer)
        {
            return;
        }

        if (parent.TryGet(out NetworkObject networkObject))
        {
            ApplyParent(parent);
        }
    }

    /// <summary>
    /// 클라이언트에서 서버로 parent 초기화를 요청한다.
    /// </summary>
    [ServerRpc]
    public void ResetParentServerRpc()
    {
        ApplyParent(null);
    }

    /// <summary>
    /// 서버에서 클라이언트로 parent 초기화를 요청한다.
    /// </summary>
    [ClientRpc]
    public void ResetParentClientRpc()
    {
        if (IsServer)
        {
            return;
        }

        ApplyParent(null);
    }
}
