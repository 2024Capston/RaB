using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DoorController : NetworkBehaviour
{
    private Animator _animator;
    
    /// <summary>
    /// 문이 열리기 위해선 Host에서 IsOpened가 true 상태이어야 함.
    /// </summary>
    public bool IsOpened { get; set; } = false;
    
    // TODO IsOpened의 값에 따라 DoorLight의 매터리얼 값을 바꾸어야 한다.
    
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
