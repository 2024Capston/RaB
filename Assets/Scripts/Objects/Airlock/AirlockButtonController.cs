using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AirlockButtonController : NetworkBehaviour, IInteractable
{
    [SerializeField] private AirlockController _airlockController;
    [SerializeField] private bool _isInButton;
    [SerializeField] private ColorType _buttonColor;

    private AudioSource _audioSource;

    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }

    public bool IsInteractable(PlayerController player)
    {
        return true;
        return player.Color == _buttonColor;
    }

    public bool StartInteraction(PlayerController player)
    {
        _airlockController.OnClickAirlockButtonServerRpc(_buttonColor, _isInButton);

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

    public bool StopInteraction(PlayerController playerController)
    {
        return true;
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