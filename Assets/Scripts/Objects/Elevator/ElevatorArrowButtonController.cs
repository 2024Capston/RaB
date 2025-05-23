using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ElevatorArrowButtonController : NetworkBehaviour, IInteractable
{
    [SerializeField] private int _deltaValue;
    [SerializeField] private List<Material> _materials;

    // 제경씨 덕분에 코드가 복잡해졌어요!
    [SerializeField] private int _materialNum;

    private MeshRenderer _meshRenderer;
    
    public Outline Outline { get; set; }

    private NetworkVariable<bool> _isActive = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _meshRenderer = GetComponent<MeshRenderer>();
        Outline = GetComponent<Outline>();
        _isActive.OnValueChanged += OnValueChanged;
    }

    private void OnValueChanged(bool previousValue, bool newValue)
    {
        Logger.Log($"{previousValue} -> {newValue}");
        
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
        RequestInteractionServerRpc(_deltaValue);
        return false;
    }

    public bool StopInteraction(PlayerController player)
    {
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractionServerRpc(int deltaValue)
    {
        Logger.Log("Test");
        LobbyManager.Instance.Elevator.SelectFloor += deltaValue;
    }

    private void ActivateButton()
    {
        Material[] materials = _meshRenderer.materials;
        materials[_materialNum] = _materials[1];
        _meshRenderer.materials = materials;
    }

    private void DeactivateButton()
    {
        Material[] materials = _meshRenderer.materials;
        materials[_materialNum] = _materials[0];
        _meshRenderer.materials = materials;
    }
}
