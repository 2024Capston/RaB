using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirlockButtonController : MonoBehaviour, IInteractable
{
    [SerializeField] private AirlockController _airlockController;
    [SerializeField] private bool _isInButton;
    [SerializeField] private ColorType _buttonColor;
    
    public Outline Outline { get; set; }

    public bool IsInteractable(PlayerController player)
    {
        return player.PlayerColor == _buttonColor;
    }

    public bool StartInteraction(PlayerController player)
    {
        if (IsInteractable(player))
        {
            _airlockController.OnClickAirlockButtonServerRpc(player.PlayerColor, _isInButton);
            return true;
        }
        return false;
    }

    public bool StopInteraction(PlayerController playerController)
    {
        return true;
    }
}