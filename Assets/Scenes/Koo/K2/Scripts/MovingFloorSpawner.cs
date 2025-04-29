using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovingFloorSpawner : NetworkObjectSpawner
{
    [SerializeField] private Vector3 _endPoint;
    [SerializeField] private float _moveDurationIn;
    [SerializeField] private float _moveDurationOut;
    [SerializeField] private EventType[] _buttonClicked;
    [SerializeField] private NetworkObject[] _otherFloors;
    
    // Start is called before the first frame update
    void Start()
    {if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        var spawnedFloor = Instantiate(_prefab);

        spawnedFloor.transform.position = transform.position;
        spawnedFloor.transform.rotation = transform.rotation;
        spawnedFloor.transform.localScale = transform.lossyScale;
        
        spawnedFloor.GetComponent<NetworkObject>().Spawn();
        spawnedFloor.GetComponent<MovingFloorController>().Initialize(transform.position, _endPoint, _moveDurationIn, _moveDurationOut, _buttonClicked);

    }
}
