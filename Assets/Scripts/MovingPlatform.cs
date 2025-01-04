using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovingPlatform : NetworkBehaviour
{
    private float _timer;

    public override void OnNetworkSpawn()
    {
        GameObject renderer = new GameObject("Ghost");

        renderer.transform.localScale = transform.localScale;

        renderer.AddComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
        renderer.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<MeshRenderer>());

        renderer.AddComponent<NetworkInterpolator>().SetTarget(transform);
    }

    void FixedUpdate()
    {
        _timer += Time.fixedDeltaTime;

        if (IsServer && _timer > Mathf.PI * 2f)
        {
            _timer -= Mathf.PI * 2f;
            UpdatePositionClientRpc(_timer, GetComponent<Rigidbody>().position);
        }

        Vector3 newPosition = transform.position;
        newPosition.y = (Mathf.Sin(_timer) * 5f) + 3f;

        GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(GetComponent<Rigidbody>().position, newPosition, Time.fixedDeltaTime * 20f));
    }

    [ClientRpc]
    void UpdatePositionClientRpc(float timer, Vector3 position)
    {
        _timer = timer;
        GetComponent<Rigidbody>().MovePosition(position);
    }
}
