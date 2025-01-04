using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovingPlatform : NetworkBehaviour
{
    private Rigidbody _rigidbody;

    private float _timer;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (IsServer && _timer > Mathf.PI * 2f)
        {
            _timer -= Mathf.PI * 2f;
            UpdatePositionClientRpc(_timer, _rigidbody.position);
        }

        Vector3 newPosition = transform.position;
        newPosition.y = (Mathf.Sin(_timer) * 5f) + 3f;

        _rigidbody.MovePosition(Vector3.Lerp(_rigidbody.position, newPosition, Time.deltaTime * 20f));
    }

    [ClientRpc]
    void UpdatePositionClientRpc(float timer, Vector3 position)
    {
        _timer = timer;
        GetComponent<Rigidbody>().MovePosition(position);
    }
}
