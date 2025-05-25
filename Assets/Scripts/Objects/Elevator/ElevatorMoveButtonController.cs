using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ElevatorMoveButtonController : NetworkBehaviour, IInteractable
{
    private NetworkVariable<bool> _isActive = new NetworkVariable<bool>();
    [SerializeField] private List<Material> _materials;

    private MeshRenderer _meshRenderer;
    
    public Outline Outline { get; set; }

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        Outline = GetComponent<Outline>();
        _isActive.OnValueChanged += OnValueChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
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
        LobbyManager.Instance.RequestMoveFloor(LobbyManager.Instance.Elevator.SelectFloor);
    }

    private void ActivateButton()
    {
        Material[] materials = _meshRenderer.materials;
        materials[1] = _materials[1];
        _meshRenderer.materials = materials;
    }

    private void DeactivateButton()
    {
        Material[] materials = _meshRenderer.materials;
        materials[1] = _materials[0];
        _meshRenderer.materials = materials;

        Outline.enabled = false;
    }
}
