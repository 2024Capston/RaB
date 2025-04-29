using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class KeypadSpawner : NetworkObjectSpawner
{
    [SerializeField] string _password;
    [SerializeField] EventType[] _publishOnAnswer;

    
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
        _spawnedObject.GetComponent<KeypadController>().Initialize(_password, _publishOnAnswer);
    }

}
