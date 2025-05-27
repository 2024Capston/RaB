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
        _selectFloor.Value = SessionManager.Instance.CurrentFloor;
    }

    private void OnValueChanged(int previousValue, int newValue)
    {
        if (!IsServer)
        {
            return;
        }

        // 강제 갱신을 위한 코드
        _elevatorSegments[0].SegmentValue = 0;
        _elevatorSegments[0].SegmentValue = newValue;

        // 강제 갱신을 위한 코드
        _elevatorSegments[1].SegmentValue = _elevatorSegments[2].SegmentValue = 0;
        _elevatorSegments[1].SegmentValue = _elevatorSegments[2].SegmentValue = SessionManager.Instance.CurrentFloor;

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
        
        _elevatorMoveButtonController.IsActive = newValue != SessionManager.Instance.CurrentFloor && _playerCount == 2;
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
        }
    }
    
    
    
    [ServerRpc(RequireOwnership = false)]
    public void InitElevatorServerRpc()
    {
        print(SelectFloor);
        // 강제로 세그먼트 방향버튼 갱신
        OnValueChanged(SelectFloor, SelectFloor);
    }

    public void OpenElevatorDoor()
    {
        _elevatorDoor.IsOpened = true;
        _elevatorDoor.Activate();
    }
    public void CloseElevatorDoor()
    {
        _elevatorDoor.IsOpened = false;
        _elevatorDoor.Deactivate();
    }

    [ClientRpc]
    public void StartElevatorAnimationClientRpc(int preFloor, int nxtFloor)
    {
        int delta = preFloor - nxtFloor;
        
        if (IsServer)
        {
            if (delta < 0)
            {
                _elevatorArrow.ArrowValue = 1;
            }
            else
            {
                _elevatorArrow.ArrowValue = -1;
            }
        }

        StartCoroutine(CoElevatorAnimation(preFloor, nxtFloor));
    }

    private IEnumerator CoElevatorAnimation(int preFloor, int nxtFloor)
    {
        float duration = 5f; 
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float value = elapsed / duration;
            CameraController.LocalCamera.ChangeShakeAmplitude(value);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_elevatorArrow.ArrowValue == 1)
        {
            for (int i = preFloor; i <= nxtFloor; i++)
            {
                Logger.Log(i.ToString());
                yield return new WaitForSeconds(5f);
                if (IsServer)
                {
                    foreach (var segment in _elevatorSegments)
                    {
                        segment.SegmentValue = i;
                    }
                }
            }
        }
        else
        {
            for (int i = preFloor; i >= nxtFloor; i--)
            {
                Logger.Log(i.ToString());
                yield return new WaitForSeconds(5f);
                if (IsServer)
                {
                    foreach (var segment in _elevatorSegments)
                    {
                        segment.SegmentValue = i;
                    }
                }
            }
        }
        
        duration = 5f; 
        elapsed = 0f;
        
        while (elapsed < duration)
        {
            float value = elapsed / duration;
            CameraController.LocalCamera.ChangeShakeAmplitude(1f - value);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (IsServer)
        {
            _elevatorArrow.ArrowValue = 0;
        }
        
        LobbyManager.Instance.OnMoveFloorEndServerRpc(nxtFloor);
    }
}
