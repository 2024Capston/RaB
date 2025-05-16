using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NumberObjectFixerSpawner : NetworkObjectSpawner
{
    // 고정시킬 좌표
    [SerializeField] private Vector3 _fixedPosition;
    
    // 고정시킬 각도
    [SerializeField] private Quaternion _fixedRotation;
    
    // 고정시킬 물체 이름
    [SerializeField] private string _objectName;
    
    // Start is called before the first frame update
    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        _spawnedObject = Instantiate(_prefab);

        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.lossyScale;

        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponent<NumberObjectFixer>().Initialize(_fixedPosition, _fixedRotation, _objectName);
    }
}
