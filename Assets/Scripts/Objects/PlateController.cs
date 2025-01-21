using UnityEngine;
using Unity.Netcode;

using UnityEngine.Events;

public class PlateController : NetworkBehaviour
{
    [SerializeField] UnityEvent<PlateController, Collision> _eventsOnEnter;
    [SerializeField] UnityEvent<PlateController, Collision> _eventsOnExit;

    private void OnCollisionEnter(Collision collision)
    {
        _eventsOnEnter.Invoke(this, collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        _eventsOnExit.Invoke(this, collision);
    }
}
