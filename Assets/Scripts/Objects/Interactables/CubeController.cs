using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CubeController : PlayerDependantBehaviour, IInteractable
{
    [SerializeField] private ColorType _cubeColor;
    public ColorType CubeColor
    {
        get => _cubeColor;
        set => _cubeColor = value;
    }

    private Rigidbody _rigidbody;
    private NetworkSyncTransform _networkSyncTransform;

    private PlayerController _interactingPlayer;
    private Rigidbody _platform;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _networkSyncTransform = GetComponent<NetworkSyncTransform>();
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
