using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class KeypadButtonController : MonoBehaviour, IInteractable
{
    [SerializeField] int _keyNumber;
    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        _outline = GetComponent<Outline>();
    }
    
    public bool IsInteractable(PlayerController player)
    {
        return true;
    }

    public bool StartInteraction(PlayerController player)
    {
        GetComponentInParent<KeypadController>().OnButtonPressed(_keyNumber);
        return false;
    }

    public bool StopInteraction(PlayerController player)
    {
        return false;
    }
}
