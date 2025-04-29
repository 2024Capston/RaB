using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePointController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    
    [SerializeField] private int _num;
    
    [SerializeField] private RespawnAreaController _respawnAreaController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController playerController))
        {
            _rigidbody = playerController.GetComponent<Rigidbody>();

            if (playerController.Color == ColorType.Red)
            {
                _respawnAreaController.RedSaved = _num;
            }
            else
            {
                _respawnAreaController.BlueSaved = _num;
            }
        }
    }
}
