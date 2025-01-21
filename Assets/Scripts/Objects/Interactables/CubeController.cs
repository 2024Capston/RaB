using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 큐브를 조작하는 Class.
/// </summary>
public class CubeController : PlayerDependantBehaviour, IInteractable
{
    /// <summary>
    /// 큐브 색깔
    /// </summary>
    [SerializeField] private ColorType _cubeColor;
    public ColorType CubeColor
    {
        get => _cubeColor;
        set => _cubeColor = value;
    }

    private const float DISTANCE_FROM_PLAYER = 20f;
    private const float CUBE_SPEED = 32f;

    private Rigidbody _rigidbody;
    private CubeRenderer _cubeRenderer;
    private NetworkInterpolator _networkInterpolator;

    private PlayerController _interactingPlayer;    // 큐브를 들고 있는 플레이어
    private bool _isActive = true;

    private bool _isTaken;
    public bool IsTaken
    {
        get => _isTaken;
    }

    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _cubeRenderer = GetComponent<CubeRenderer>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();

        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _outline = _networkInterpolator.VisualReference.GetComponent<Outline>();
            _outline.enabled = false;
        });
    }

    public override void OnPlayerInitialized()
    {
        if (_cubeColor == PlayerController.LocalPlayer.PlayerColor && IsClient)
        {
            // 플레이어와 색깔이 같으면 Ownership 요청
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
        }
        else
        {
            _rigidbody.isKinematic = true;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (_interactingPlayer)
        {
            Vector3 target = _interactingPlayer.transform.position + _interactingPlayer.transform.forward * DISTANCE_FROM_PLAYER;
            _rigidbody.velocity = (target - transform.position) * CUBE_SPEED;
        }
    }

    public bool IsInteractable(PlayerController player)
    {
        return _cubeColor == player.PlayerColor && _isActive;
    }

    public bool StartInteraction(PlayerController player)
    {
        _interactingPlayer = player;
        _rigidbody.useGravity = false;

        SetTakenServerRpc(true);

        return true;
    }

    public bool StopInteraction(PlayerController player)
    {
        _interactingPlayer = null;
        _rigidbody.useGravity = true;

        SetTakenServerRpc(false);

        return true;
    }

    /// <summary>
    /// 색깔이 같은 큐브에 대해 서버에 Ownership을 요청한다.
    /// </summary>
    /// <param name="clientId">요청하는 플레이어 ID</param>
    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ulong clientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetTakenServerRpc(bool isTaken)
    {
        SetTakenClientRpc(isTaken);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetTakenClientRpc(bool isTaken)
    {
        _isTaken = isTaken;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeColorServerRpc(ColorType color, bool updateRender)
    {
        ChangeColorClientRpc(color, updateRender);
    }

    [ClientRpc(RequireOwnership = false)]
    private void ChangeColorClientRpc(ColorType color, bool updateRender)
    {
        _cubeColor = color;

        if (_cubeColor == PlayerController.LocalPlayer.PlayerColor)
        {
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
            _rigidbody.isKinematic = false;
        }
        else
        {
            _rigidbody.isKinematic = true;
        }

        if (updateRender)
        {
            _cubeRenderer.UpdateColor();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActiveServerRpc(bool isActive)
    {
        SetActiveClientRpc(isActive);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetActiveClientRpc(bool isActive)
    {
        _isActive = isActive;
    }

    [ClientRpc(RequireOwnership = false)]
    private void ForceStopInteractionClientRpc()
    {
        if (!IsOwner || !_interactingPlayer)
        {
            return;
        }

        _interactingPlayer.ForceStopInteraction();
    }

    /// <summary>
    /// 큐브의 색깔 정보를 서버와 클라이언트 
    /// </summary>
    /// <param name="color">새 색깔</param>
    public void ChangeColor(ColorType color, bool updateRender = true)
    {
        ChangeColorServerRpc(color, updateRender);
    }

    public void SetActive(bool isActive)
    {
        SetActiveServerRpc(isActive);
    }

    public void ForceStopInteraction()
    {
        ForceStopInteractionClientRpc();
    }
}
