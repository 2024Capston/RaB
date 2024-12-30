using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovingPlatform : NetworkBehaviour
{
    private float _timer;

    void Update()
    {
        if (IsServer)
        {
            _timer += Time.deltaTime;


            Vector3 newPosition = transform.position;
            newPosition.y = (Mathf.Sin(_timer) * 5f) + 3.5f;

            GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 20f));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("WOW");
        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            collision.gameObject.GetComponent<NetworkObject>().TrySetParent(gameObject, true);
        }
    }
}
