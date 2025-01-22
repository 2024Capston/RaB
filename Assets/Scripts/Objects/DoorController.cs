using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NetworkAnimator))]
[RequireComponent(typeof(SphereCollider))]
public class DoorController : NetworkBehaviour
{
    private Animator _animator;
    
    /// <summary>
    /// 문이 열리기 위해선 Host에서 IsOpened가 true 상태이어야 함.
    /// </summary>
    [field: SerializeField]
    public bool IsOpened { get; set; } = false;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void OpenDoorServerRpc()
    {
        if (IsOpened)
        {
            _animator.SetBool("Open", true);    
        }
        
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
