using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AirlockButtonController : NetworkBehaviour, IInteractable
{
    [SerializeField] private AirlockController _airlockController;
    [SerializeField] private bool _isInButton;
    [SerializeField] private ColorType _buttonColor;

    public Outline Outline { get; set; }

    public bool IsInteractable(PlayerController player)
    {
        return true;
        return player.PlayerColor == _buttonColor;
    }

    public bool StartInteraction(PlayerController player)
    {
        if (IsInteractable(player))
        {
            _airlockController.OnClickAirlockButtonServerRpc(_buttonColor, _isInButton);
            return true;
        }
        return false;
    }

    public bool StopInteraction(PlayerController playerController)
    {
        return true;
    }
}