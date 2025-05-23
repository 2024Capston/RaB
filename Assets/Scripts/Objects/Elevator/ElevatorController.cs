using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ElevatorController : NetworkBehaviour
{
    private readonly int Max_Floor = 9;

    [SerializeField] private List<ElevatorArrowButtonController> _elevatorArrowButtonControllers = new List<ElevatorArrowButtonController>();
    [SerializeField] private ElevatorMoveButtonController _elevatorMoveButtonController;
    [SerializeField] private DoorController _elevatorDoor;
    [SerializeField] private List<ElevatorSegmentController> _elevatorSegments;
    [SerializeField] private ElevatorArrowController _elevatorArrow;
    
    private int _selectFloor;
    /// <summary>
    /// 현재 선택한 층
    /// </summary>
    public int SelectFloor
    {
        get => _selectFloor;
        set
        {
            int temp = Math.Clamp(value, 1, Max_Floor);

            foreach (var segment in _elevatorSegments)
            {
                segment.SegmentValue = temp;
            }

            if (temp == 1)
            {
                // 아래키 비활성화
                _elevatorArrowButtonControllers[0].IsActive = false;
                _elevatorArrowButtonControllers[1].IsActive = true;
            }
            else if (temp == Max_Floor)
            {
                // 윗키 비활성화
                _elevatorArrowButtonControllers[0].IsActive = true;
                _elevatorArrowButtonControllers[1].IsActive = false;
            }
            else
            {
                _elevatorArrowButtonControllers[0].IsActive = _elevatorArrowButtonControllers[1].IsActive = true;
            }

            _elevatorMoveButtonController.IsActive = temp != SessionManager.Instance.CurrentFloor;
            Logger.Log($"{temp}, {SessionManager.Instance.CurrentFloor}");
            _selectFloor = temp;
        }
    }
    
    private int _playerCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            if (++_playerCount == 1)
            {
                _elevatorMoveButtonController.IsActive = SessionManager.Instance.CurrentFloor != _selectFloor;
            }
            Logger.Log($"{other.gameObject.name} Enter {_playerCount}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            --_playerCount;
            _elevatorMoveButtonController.IsActive = false;
            
            Logger.Log($"{other.gameObject.name} Exit {_playerCount}");
        }
    }
    
    public void InitElevator()
    {
        // 강제로 세그먼트 방향버튼 갱신
        SelectFloor = _selectFloor;
    }
}
