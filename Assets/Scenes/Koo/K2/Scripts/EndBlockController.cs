using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class EndBlockController : NetworkBehaviour
{
    [SerializeField] private Vector3 _startPoint;
    [SerializeField] private Vector3 _endPoint;
    [SerializeField] private float _moveDuration;
    
    [SerializeField] private EventType[] _buttonClicked;
    
    private bool _interrupted = false;

    private int _count;
    
    // Start is called before the first frame update
    public void Start()
    {
        _count = 0;
        
        foreach (EventType eventType in _buttonClicked)
        {
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(eventType, OnPlatesClicked);
        }
    }


    private void OnPlatesClicked(PlateController plateController, GameObject b)
    {
        _count++;

        if (_count == 1)
        {
            StartCoroutine(Move());
        }
    }
    
    private IEnumerator Move()
    {
        _interrupted = false;
        
        float elapsedTime = 0f;

        while (elapsedTime < _moveDuration)
        {
            if (_interrupted) break;
            transform.position = Vector3.Lerp(_startPoint, _endPoint, elapsedTime / _moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void Reset()
    {
        _interrupted = true;
        _count = 0;
        transform.position = _startPoint;
        
    }
}
