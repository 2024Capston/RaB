using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class K1_RespawnAreaController : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController playerController))
        {
            _rigidbody = playerController.GetComponent<Rigidbody>();
            
            _rigidbody.MovePosition(GameObject.FindWithTag("Blue Spawn Point").transform.position);
        }
        else
        {
            other.TryGetComponent(out NumberObjectController numberObjectController);
            
            numberObjectController.transform.position = GameObject.FindWithTag("Blue Spawn Point").transform.position;
        }
    }
}
