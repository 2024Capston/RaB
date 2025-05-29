using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LaserSpawner : NetworkObjectSpawner
{
    [SerializeField] private Vector3 _endPoint;
    [SerializeField] private float _moveDuration;
    [SerializeField] private float _waitAtStartTime;
    [SerializeField] private float _waitAtEndTime;
    [SerializeField] private ColorType _laserColor;
    [SerializeField] private bool _laserVisible;

    private bool _movingToEnd = true;
    
    // Start is called before the first frame update
    public override void SpawnObject()
    {
        base.SpawnObject();

        _spawnedObject = Instantiate(_prefab);
        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.lossyScale;
        
        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponent<LaserController>().Initialize(transform.position, _endPoint, _moveDuration, _waitAtStartTime, _waitAtEndTime, _movingToEnd, _laserColor, _laserVisible);
    }
}
