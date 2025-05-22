using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ElevatorController : NetworkBehaviour
{
    private readonly int Max_Floor = 4;

    [SerializeField] private List<ElevatorArrowButtonController> _elevatorArrowButtonControllers = new List<ElevatorArrowButtonController>();
    [SerializeField] private ElevatorMoveButtonController _elevatorMoveButtonController;
    [SerializeField] private DoorController _elevatorDoor;
    
    
    private int _selectFloor;
    /// <summary>
    /// 현재 선택한 층
    /// </summary>
    public int SelectFloor
    {
        get => _selectFloor;
        set
        {
            int temp = Math.Clamp(value, 0, Max_Floor);

            if (temp == 0)
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
            if (++_playerCount == 2)
            {
                //_elevatorMoveButtonController.IsActive = true;
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
            //_elevatorMoveButtonController.IsActive = false;
            
            Logger.Log($"{other.gameObject.name} Exit {_playerCount}");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            _elevatorDoor.IsOpened = false;
            _elevatorDoor.Deactivate();
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            
            _elevatorDoor.Activate();
        }
    }
}
