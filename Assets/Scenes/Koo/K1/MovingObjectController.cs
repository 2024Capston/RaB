using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class MovingObjectController : NetworkBehaviour
{
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private float _moveDuration;
    private float _waitAtEndTime;
    private int _requirement;
    
    private bool _movingToEnd = true;

    private int _count;

    private EventType[] _buttonClicked;
    private EventType[] _plateClicked;
    private EventType[] _plateOut;
    
    // Start is called before the first frame update
    public void Initialize(Vector3 startPoint, Vector3 endPoint, float moveDuration, float waitAtEndTime, int requirement, EventType[] buttonClicked,  EventType[] plateClicked, EventType[] _plateOut)
    {
        _buttonClicked = buttonClicked;
        _plateClicked = plateClicked;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _moveDuration = moveDuration;
        _waitAtEndTime = waitAtEndTime;
        _requirement = requirement;
        
        _count = 0;

        foreach (EventType eventType in _buttonClicked)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, OnButtonsClicked);
        }
        
        foreach (EventType eventType in _plateClicked)
        {
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(eventType, OnPlatesClicked);
        }
        
        foreach (EventType eventType in _plateOut)
        {
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(eventType, OnPlateOut);
        }
    }

    private void OnButtonsClicked()
    {
        print("OnButtonsClicked");
        if (_count == _requirement-1)
        {
            StartCoroutine(MoveLoof());
        }
        else
        {
            _count++;
        }
    }

    private void OnPlateOut(PlateController plateController, GameObject b)
    {
        if (_count != 0) _count--;
    }
    
    private void OnPlatesClicked(PlateController plateController, GameObject b)
    {
        if (_count == _requirement-1)
        {
            StartCoroutine(MoveLoof());
            _count = 0;
        }
        else
        {
            _count++;
        }
    }


    private IEnumerator MoveLoof()
    {
        {
            for (int i = 0; i < 2; i++)
            {
                _count = 0;

                Vector3 endPosition = _movingToEnd ? _endPoint : _startPoint;
                Vector3 startPosition = _movingToEnd ? _startPoint : _endPoint;

                float elapsedTime = 0f;

                while (elapsedTime < _moveDuration)
                {
                    transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / _moveDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                transform.position = endPosition;
                _movingToEnd = !_movingToEnd;

                yield return new WaitForSeconds(_movingToEnd ? 0 : _waitAtEndTime);
            }
        }
    }
}
