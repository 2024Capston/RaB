using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CubeBlockerSpawner : NetworkObjectSpawner
{
    [SerializeField] ColorType _color;
    [SerializeField] EventType[] _subscribeForBlock;

    public override void SpawnObject()
    {
        base.SpawnObject();

        _spawnedObject = Instantiate(_prefab);

        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.localScale;

        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponent<CubeBlockerController>().Initialize(_color, _subscribeForBlock);
    }
}
