using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NumberObjectSpawner : NetworkObjectSpawner
{
    [SerializeField] private ColorType _color;
    
    [SerializeField] private int _viewMode;
    
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
        
        NumberObjectController numberObjectController = _spawnedObject.GetComponent<NumberObjectController>();
        numberObjectController.Color = _color;
        numberObjectController.ViewMode = _viewMode;
        
        numberObjectController.Initialize();
    }
}
