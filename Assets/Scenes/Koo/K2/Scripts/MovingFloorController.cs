using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class MovingFloorController : NetworkBehaviour
{
    [SerializeField] private Vector3 _startPoint;
    [SerializeField] private Vector3 _endPoint;
    [SerializeField] private float _moveDurationIn;
    [SerializeField] private float _moveDurationOut;
    
    [SerializeField] private MovingFloorController[] _linkedFloors; 
    
    public bool _isIn;

    [SerializeField] private EventType[] _buttonClicked;

    public void Initialize(Vector3 startPoint, Vector3 endPoint, float moveDurationIn, float moveDurationOut, EventType[] buttonClicked)
    {
        _buttonClicked = buttonClicked;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _moveDurationIn = moveDurationIn;
        _moveDurationOut = moveDurationOut;
        
        _isIn = false;

        foreach (EventType eventType in _buttonClicked)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, OnButtonsClicked);
        }
    }
    
    private void Start()
    {
        _isIn = false;

        foreach (EventType eventType in _buttonClicked)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, OnButtonsClicked);
        }
    }
    
    private void OnButtonsClicked()
    {
        StartCoroutine(MoveFloor(_isIn));
        
        foreach (MovingFloorController floor in _linkedFloors)
        {
            if (floor._isIn) floor.FloorOff();
        }
    }

    public void FloorOff()
    {
        StartCoroutine(MoveFloor(true));
    }
    
    private IEnumerator MoveFloor(bool tmp)
    {
        _isIn = !tmp;

        var moveDuration = tmp ? _moveDurationOut : _moveDurationIn;
        var startPoint = tmp ? _endPoint : _startPoint;
        var endPoint = tmp ? _startPoint : _endPoint;
        
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPoint, endPoint, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPoint;
    }
}
