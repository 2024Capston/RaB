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
        if (other.TryGetComponent(out PossessableController possessableController) && IsServer && possessableController.Color == _color)
        {
            if (possessableController.name!= "Possessable2_5(Clone)")
            {
                EventBus.Instance.InvokeEvent(EventType.EventB);
                StartCoroutine(Wait());
                EventBus.Instance.InvokeEvent(EventType.EventB);
            }
        }
        else
        {
            EventBus.Instance.InvokeEvent(EventType.EventB);
            StartCoroutine(Wait());
            EventBus.Instance.InvokeEvent(EventType.EventB);
        }
    }

    public IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);
    }
}
