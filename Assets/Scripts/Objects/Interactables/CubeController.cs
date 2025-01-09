using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class CubeController : PlayerDependantBehaviour, IInteractable
{
    [SerializeField] private ColorType _cubeColor;
    public ColorType CubeColor
    {
        get => _cubeColor;
        set => _cubeColor = value;
    }

    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    private Rigidbody _rigidbody;
    private NetworkSyncTransform _networkSyncTransform;
    private NetworkInterpolator _networkInterpolator;

    private PlayerController _interactingPlayer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _networkSyncTransform = GetComponent<NetworkSyncTransform>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();

        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _outline = _networkInterpolator.VisualReference.GetComponent<Outline>();
            _outline.enabled = false;
        });
    }

    public override void OnPlayerInitialized()
    {
        if (_cubeColor == PlayerController.LocalPlayer.PlayerColor)
        {
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
        }
    }

    private void Update()
    {
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

    public void StartInteraction(PlayerController player)
    {
        _interactingPlayer = player;

        _rigidbody.useGravity = false;
        _networkSyncTransform.SetParent(player.gameObject);
    }

    public void StopInteraction(PlayerController player)
    {
        _interactingPlayer = null;

        _rigidbody.useGravity = true;
        _networkSyncTransform.SetParent(null);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestOwnershipServerRpc(ulong clientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }
}
