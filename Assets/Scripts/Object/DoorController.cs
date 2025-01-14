using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DoorController : NetworkBehaviour
{
    private Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void OpenDoorServerRpc()
    {
        _animator.SetBool("Open", true);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void CloseDoorServerRpc()
    {
        _animator.SetBool("Open", false);
    }

    private void OnTriggerEnter(Collider other)
    {
        OpenDoorServerRpc();
    }

    private void OnTriggerExit(Collider other)
    {
        CloseDoorServerRpc();
    }
}
