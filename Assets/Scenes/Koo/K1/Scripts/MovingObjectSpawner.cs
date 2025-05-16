using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovingObjectSpawner : NetworkObjectSpawner
{
    [SerializeField] private Vector3 _endPoint;
    [SerializeField] private float _moveDuration;
    [SerializeField] private float _waitAtEndTime;
    [SerializeField] private EventType[] _buttonClicked;
    [SerializeField] private EventType[] _plateClicked;
    [SerializeField] private int _requirement;
    [SerializeField] private EventType[] _plateOut;

 
    
    // Start is called before the first frame update
    public override void SpawnObject()
    {
        base.SpawnObject();
        
        _spawnedObject = Instantiate(_prefab);

        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.lossyScale;
        
        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponent<MovingObjectController>().Initialize(transform.position, _endPoint, _moveDuration,  _waitAtEndTime, _requirement, _buttonClicked, _plateClicked, _plateOut);

    }
}
