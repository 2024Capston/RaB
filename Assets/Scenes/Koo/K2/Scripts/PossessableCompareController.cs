using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Possessable;
using Unity.Netcode;
using UnityEngine;

public class PossessableCompareController : NetworkBehaviour
{
    private Rigidbody _rigidbody;
    private ColorType _color;
    
    private void OnTriggerEnter(Collider other)
    {
        //if (other.TryGetComponent(out PlayerController playerController))
        //{
        //    if (playerController.InteractableInHand == { 정답 Possessable의 IInteractable 레퍼런스 }) {
        //        return;
        //    }
        //}
        
        EventBus.Instance.InvokeEvent(EventType.EventB);
        StartCoroutine(CoWait());
    }

    private IEnumerator CoWait()
    {
        yield return new WaitForSeconds(2f);
        EventBus.Instance.InvokeEvent(EventType.EventB);
    }
}
