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
    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        var spawnedRazer = Instantiate(_prefab);
        spawnedRazer.transform.position = transform.position;
        spawnedRazer.transform.rotation = transform.rotation;
        spawnedRazer.transform.localScale = transform.lossyScale;
        
        spawnedRazer.GetComponent<NetworkObject>().Spawn();
        spawnedRazer.GetComponent<LaserController>().Initialize(transform.position, _endPoint, _moveDuration, _waitAtStartTime, _waitAtEndTime, _movingToEnd, _laserColor, _laserVisible);
    }
}
