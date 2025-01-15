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

    private Rigidbody _rigidbody;
    private NetworkInterpolator _networkInterpolator;
    private NetworkPlatformFinder _networkPlatformFinder;

    private PlayerController _interactingPlayer;    // 큐브를 들고 있는 플레이어
    private Rigidbody _platform;                    // 큐브가 올라가 있는 플랫폼

    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _networkInterpolator = GetComponent<NetworkInterpolator>();
        _networkPlatformFinder = GetComponent<NetworkPlatformFinder>();

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
            // 그렇지 않으면 kinematic으로 지정해 움직일 수 없게 (...?)
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
            Vector3 target = _interactingPlayer.transform.position + _interactingPlayer.transform.forward * 2f;
            _rigidbody.velocity = (target - transform.position) * 32f;
        }
    }

    public bool IsInteractable(PlayerController player)
    {
        return _cubeColor == player.PlayerColor;
    }

    public bool StartInteraction(PlayerController player)
    {
        _interactingPlayer = player;

        _rigidbody.useGravity = false;

        return true;
    }

    public bool StopInteraction(PlayerController player)
    {
        _interactingPlayer = null;

        _rigidbody.useGravity = true;

        return true;
    }

    /// <summary>
    /// 색깔이 같은 큐브에 대해 서버에 Ownership을 요청한다.
    /// </summary>
    /// <param name="clientId">요청하는 플레이어 ID</param>
    [ServerRpc(RequireOwnership = false)]
    public void RequestOwnershipServerRpc(ulong clientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }
}
