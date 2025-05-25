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
    
    private NetworkVariable<int> _selectFloor = new NetworkVariable<int>(1);
    /// <summary>
    /// 현재 선택한 층
    /// </summary>
    public int SelectFloor
    {
        get => _selectFloor.Value;
        set => _selectFloor.Value = Math.Clamp(value, 1, Max_Floor);
    }
    
    private int _playerCount = 0;

    private void Awake()
    {
        _selectFloor.OnValueChanged += OnValueChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
    }

    private void OnValueChanged(int previousValue, int newValue)
    {
        if (!IsServer)
        {
            return;
        }
        
        foreach (var segment in _elevatorSegments)
        {
            // 강제 갱신을 위한 코드
            segment.SegmentValue = 0;
            
            segment.SegmentValue = newValue;
        }

        // 강제 갱신을 위한 코드
        _elevatorArrowButtonControllers[0].IsActive = _elevatorArrowButtonControllers[1].IsActive = false;
        
        if (newValue == 1)
        {
            // 아래키 비활성화
            _elevatorArrowButtonControllers[0].IsActive = false;
            _elevatorArrowButtonControllers[1].IsActive = true;
        }
        else if (newValue == Max_Floor)
        {
            // 윗키 비활성화
            _elevatorArrowButtonControllers[0].IsActive = true;
            _elevatorArrowButtonControllers[1].IsActive = false;
        }
        else
        {
            _elevatorArrowButtonControllers[0].IsActive = _elevatorArrowButtonControllers[1].IsActive = true;
        }
    
        // 강제 갱신을 위한 코드
        _elevatorMoveButtonController.IsActive = false;
        
        _elevatorMoveButtonController.IsActive = newValue != SessionManager.Instance.CurrentFloor;
    }

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
                _elevatorMoveButtonController.IsActive = SessionManager.Instance.CurrentFloor != SelectFloor;
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
    
    [ServerRpc(RequireOwnership = false)]
    public void InitElevatorServerRpc()
    {
        // 강제로 세그먼트 방향버튼 갱신
        OnValueChanged(SelectFloor, SelectFloor);
    }
}
