using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ElevatorArrowButtonController : NetworkBehaviour, IInteractable
{
    [SerializeField] private int _deltaValue;
    [SerializeField] private List<Material> _materials;

    // 제경씨 덕분에 코드가 복잡해졌어요!
    [SerializeField] private int _materialNum;

    private MeshRenderer _meshRenderer;
    private AudioSource _audioSource;
    
    public Outline Outline { get; set; }

    private NetworkVariable<bool> _isActive = new NetworkVariable<bool>();

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
        
        return false;
    }

    public bool StopInteraction(PlayerController player)
    {
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractionServerRpc(int deltaValue)
    {
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
        Outline.enabled = false;
        Material[] materials = _meshRenderer.materials;
        materials[_materialNum] = _materials[0];
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
