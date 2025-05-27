using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ElevatorMoveButtonController : NetworkBehaviour, IInteractable
{
    private NetworkVariable<bool> _isActive = new NetworkVariable<bool>();
    [SerializeField] private List<Material> _materials;

    private MeshRenderer _meshRenderer;
    private AudioSource _audioSource;
    
    public Outline Outline { get; set; }

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        Outline = GetComponent<Outline>();
        _audioSource = GetComponent<AudioSource>();
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
        _audioSource.pitch = Random.Range(1.0f, 2.0f);
        _audioSource.PlayOneShot(_audioSource.clip, _audioSource.volume);
        
        PlayPressSound();

        if (IsServer)
        {
            PlayPressSoundClientRpc();
        }
        else
        {
            PlayPressSoundServerRpc();
        }
        
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
        Outline.enabled = false;
        Material[] materials = _meshRenderer.materials;
        materials[1] = _materials[0];
        _meshRenderer.materials = materials;
    }
    
    private void PlayPressSound()
    {
        _audioSource.pitch = Random.Range(1.0f, 2.0f);
        _audioSource.PlayOneShot(_audioSource.clip, _audioSource.volume);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayPressSoundServerRpc()
    {
        PlayPressSound();
    }

    [ClientRpc(RequireOwnership = false)]
    private void PlayPressSoundClientRpc()
    {
        if (IsServer)
        {
            return;
        }

        PlayPressSound();
    }
}
