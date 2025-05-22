using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ElevatorMoveButtonController : NetworkBehaviour, IInteractable
{
    private NetworkVariable<bool> _isActive;
    [SerializeField] private List<Material> _materials;

    private MeshRenderer _meshRenderer;
    
    public Outline Outline { get; set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _meshRenderer = GetComponent<MeshRenderer>();
        _isActive.OnValueChanged += OnValueChanged;
    }

    private void OnValueChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            ActivateButton();    
        }

        else
        {
            DeactivateButton();
        }
    }

    public bool IsActive
    {
        get => _isActive.Value;
        set => _isActive.Value = value;
    }
    
    public bool IsInteractable(PlayerController player)
    {
        return IsActive;
    }

    public bool StartInteraction(PlayerController player)
    {
        RequestInteractionServerRpc();
        return false;
    }

    public bool StopInteraction(PlayerController player)
    {
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractionServerRpc()
    {
        LobbyManager.Instance.RequestMoveFloorServerRpc(LobbyManager.Instance.Elevator.SelectFloor);
    }

    private void ActivateButton()
    {
        _meshRenderer.material = _materials[1];
    }

    private void DeactivateButton()
    {
        _meshRenderer.material = _materials[0];
    }
}
