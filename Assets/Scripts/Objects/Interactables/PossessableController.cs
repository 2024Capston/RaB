using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 빙의 가능한 물체를 조작하는 Class
/// </summary>
public class PossessableController : PlayerDependantBehaviour, IInteractable
{
    /// <summary>
    /// 물체의 색깔
    /// </summary>
    [SerializeField] private ColorType _possessableColor;
    public ColorType PossessableColor
    {
        get => _possessableColor;
        set => _possessableColor = value;
    }

    /// <summary>
    /// 렌더링에 쓰일 매터리얼. (파랑, 빨강 순)
    /// </summary>
    [SerializeField] private Material[] _materials;

    private Rigidbody _rigidbody;
    private BoxCollider _boxCollider;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private NetworkSyncTransform _networkSyncTransform;
    private NetworkInterpolator _networkInterpolator;

    // 빙의한 플레이어에 대한 레퍼런스
    private PlayerController _interactingPlayer;
    private MeshFilter _interactingMeshFilter;
    private MeshRenderer _interactingMeshRenderer;
    private CharacterController _characterController;

    // 빙의한 플레이어가 원래 가지고 있던 Mesh, Material
    private Mesh _originalMesh;
    private Material _originalMaterial;

    private Rigidbody _platform;

    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();

        _networkSyncTransform = GetComponent<NetworkSyncTransform>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();

        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _meshFilter = _networkInterpolator.VisualReference.GetComponent<MeshFilter>();
            _meshRenderer = _networkInterpolator.VisualReference.GetComponent<MeshRenderer>();

            _outline = _networkInterpolator.VisualReference.GetComponent<Outline>();
            _outline.enabled = false;

            _meshRenderer.material = _materials[(int)_possessableColor - 1];
        });
    }

    public override void OnPlayerInitialized()
    {
        if (_possessableColor == PlayerController.LocalPlayer.PlayerColor && IsClient)
        {
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
        }
        else
        {
            _rigidbody.isKinematic = true;
        }
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        // 빙의 상태에선 플레이어의 위치로 계속 이동
        if (_interactingPlayer)
        {
            transform.position = _interactingPlayer.transform.position;
            transform.rotation = _interactingPlayer.transform.rotation;
        }
        else
        {
            HandlePlatform();
        }
    }

    /// <summary>
    /// 물체와 플랫폼의 관계를 처리한다.
    /// </summary>
    private void HandlePlatform()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 4f))
        {
            if (_platform?.gameObject != hit.collider.gameObject)
            {
                // 새로운 플랫폼을 발견한 경우
                if (hit.collider.gameObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject) &&
                    hit.collider.gameObject.TryGetComponent<Rigidbody>(out _platform))
                {
                    _networkSyncTransform.SetParent(networkObject.gameObject);
                }
                else if (_platform != null)
                {
                    _networkSyncTransform.SetParent(null);
                    _platform = null;
                }
            }
        }
        else if (_platform != null)
        {
            _networkSyncTransform.SetParent(null);
            _platform = null;
        }
    }

    /// <summary>
    /// 빙의를 해제할 때 플레이어가 들어갈 자리가 있는지 파악하고, 있다면 그곳으로 이동시킨다.
    /// </summary>
    /// <returns>빙의 해제 가능 여부</returns>
    private bool CheckDispossessionPosition()
    {
        Vector3 origin = _characterController.transform.position;
        origin.y -= _boxCollider.size.y;
        origin.y += PlayerController.INITIAL_CAPSULE_HEIGHT;

        Vector3 verticalPad = Vector3.up * PlayerController.INITIAL_CAPSULE_HEIGHT / 2f;
        float radius = (_boxCollider.size.x / 2f + PlayerController.INITIAL_CAPSULE_RADIUS) * 1.2f;

        // 물체를 중심으로, 주변을 원으로 탐색한다.
        for (int i = 0; i < 9; i++)
        {
            Vector3 newPoint;

            // 정면으로부터 0~180도 회전
            newPoint = origin + Quaternion.Euler(0, i * 20, 0) * transform.forward * radius;

            if (Physics.OverlapCapsule(newPoint + verticalPad, newPoint - verticalPad, PlayerController.INITIAL_CAPSULE_RADIUS).Length == 0)
            {
                _characterController.enabled = false;
                _characterController.transform.position = newPoint;
                _characterController.enabled = true;

                return true;
            }

            // 정면으로부터 -180~0도 회전
            newPoint = origin + Quaternion.Euler(0, -i * 20, 0) * transform.forward * radius;

            if (Physics.OverlapCapsule(newPoint + verticalPad, newPoint - verticalPad, PlayerController.INITIAL_CAPSULE_RADIUS).Length == 0)
            {
                _characterController.enabled = false;
                _characterController.transform.position = newPoint;
                _characterController.enabled = true;

                return true;
            }
        }

        return false;
    }

    public bool IsInteractable(PlayerController player)
    {
        return _possessableColor == player.PlayerColor;
    }

    public bool StartInteraction(PlayerController player)
    {
        _interactingPlayer = player;
        _characterController = player.GetComponent<CharacterController>();

        // Local 환경과 Remote 환경에서 상태를 갱신한다.
        StartPossession(player);

        if (IsServer)
        {
            StartPossesionClientRpc(player.gameObject);
        }
        else
        {
            StartPossessionServerRpc(player.gameObject);
        }

        _characterController.enabled = false;
        _characterController.transform.position = transform.position;
        _characterController.transform.rotation = transform.rotation;
        _characterController.enabled = true;

        return true;
    }

    public bool StopInteraction(PlayerController player)
    {
        // 빙의 해제 후 들어갈 여유 공간이 있다면
        if (CheckDispossessionPosition())
        {
            // Local 환경과 Remote 환경에서 상태를 갱신한다.
            StopPossession(player);

            if (IsServer)
            {
                StopPossesionClientRpc(player.gameObject);
            }
            else
            {
                StopPossessionServerRpc(player.gameObject);
            }

            _interactingPlayer = null;
            _characterController = null;

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 색깔이 같은 물체에 대해 서버에 Ownership을 요청한다.
    /// </summary>
    /// <param name="clientId">요청하는 플레이어 ID</param>
    [ServerRpc(RequireOwnership = false)]
    public void RequestOwnershipServerRpc(ulong clientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }

    /// <summary>
    /// 빙의를 시작한다.
    /// </summary>
    /// <param name="player">빙의할 플레이어</param>
    private void StartPossession(PlayerController player)
    {
        NetworkInterpolator playerInterpolator = player.GetComponent<NetworkInterpolator>();

        _interactingMeshFilter = playerInterpolator.VisualReference.GetComponent<MeshFilter>();
        _interactingMeshRenderer = playerInterpolator.VisualReference.GetComponent<MeshRenderer>();

        // 플레이어의 기존 Mesh, Material 저장
        _originalMesh = _interactingMeshFilter.sharedMesh;
        _originalMaterial = _interactingMeshRenderer.material;

        // 플레이어의 Mesh, Materil 갱신
        _interactingMeshFilter.mesh = _meshFilter.sharedMesh;
        _interactingMeshRenderer.material = _meshRenderer.material;

        // 물체는 일시적으로 비활성화
        _rigidbody.isKinematic = true;
        _boxCollider.enabled = false;
        _meshRenderer.enabled = false;

        // 플레이어의 Collider 정보 갱신
        player.UpdateCollider(_boxCollider);
    }

    /// <summary>
    /// 빙의를 중단한다.
    /// </summary>
    /// <param name="player">빙의 중단할 플레이어</param>
    private void StopPossession(PlayerController player)
    {
        // 플레이어의 Mesh, Material 복구
        _interactingMeshFilter.mesh = _originalMesh;
        _interactingMeshRenderer.material = _originalMaterial;

        _interactingMeshFilter = null;
        _interactingMeshRenderer = null;

        _originalMesh = null;
        _originalMaterial = null;

        // 물체 다시 활성화
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
        _boxCollider.enabled = true;
        _meshRenderer.enabled = true;

        // 플레이어의 Collider 정보 갱신
        player.UpdateCollider(null);
    }

    /// <summary>
    /// 클라이언트에서 서버로 빙의 시작을 전달한다.
    /// </summary>
    /// <param name="player">빙의할 플레이어</param>
    [ServerRpc]
    public void StartPossessionServerRpc(NetworkObjectReference player)
    {
        if (player.TryGet(out NetworkObject networkObject))
        {
            StartPossession(networkObject.GetComponent<PlayerController>());
        }
    }

    /// <summary>
    /// 서버에서 클라이언트로 빙의 시작을 전달한다.
    /// </summary>
    /// <param name="player">빙의할 플레이어</param>
    [ClientRpc]
    public void StartPossesionClientRpc(NetworkObjectReference player)
    {
        if (IsServer)
        {
            return;
        }

        if (player.TryGet(out NetworkObject networkObject))
        {
            StartPossession(networkObject.GetComponent<PlayerController>());
        }
    }

    /// <summary>
    /// 클라이언트에서 서버로 빙의 중단을 전달한다.
    /// </summary>
    /// <param name="player">빙의 중단할 플레이어</param>
    [ServerRpc]
    public void StopPossessionServerRpc(NetworkObjectReference player)
    {
        if (player.TryGet(out NetworkObject networkObject))
        {
            StopPossession(networkObject.GetComponent<PlayerController>());
        }
    }

    /// <summary>
    /// 서버에서 클라이언트로 빙의 중단을 전달한다.
    /// </summary>
    /// <param name="player">빙의 중단할 플레이어</param>
    [ClientRpc]
    public void StopPossesionClientRpc(NetworkObjectReference player)
    {
        if (IsServer)
        {
            return;
        }

        if (player.TryGet(out NetworkObject networkObject))
        {
            StopPossession(networkObject.GetComponent<PlayerController>());
        }
    }
}
