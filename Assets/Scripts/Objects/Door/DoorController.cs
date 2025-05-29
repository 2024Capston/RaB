using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SphereCollider))]
public class DoorController : NetworkBehaviour, IActivatable
{
    [Tooltip("Light Material")] [SerializeField]
    private Renderer _lightRenderer;
    
    /// <summary>
    /// 근처에 가면 열릴지 여부
    /// </summary>
    [SerializeField] private bool _isTriggerable = true;

    /// <summary>
    /// 문 조명을 IsOpen과 동기화할지 여부
    /// </summary>
    [SerializeField] private bool _automateLight = true;

    /// <summary>
    /// 호출되면 문을 열 이벤트
    /// </summary>
    [SerializeField] private EventType[] _subscribeForActivation;

    /// <summary>
    /// 호출되면 문을 닫을 이벤트
    /// </summary>
    [SerializeField] private EventType[] _subscribeForDeactivation;

    /// <summary>
    /// 호출되면 IsOpen을 true로 설정할 이벤트
    /// </summary>
    [SerializeField] private EventType[] _subscribeForSetOpen;

    /// <summary>
    /// 호출되면 IsOpen을을 false로 설정할 이벤트
    /// </summary>
    [SerializeField] private EventType[] _subscribeForSetClose;

    /// <summary>
    /// 호출되면 문 조명 색깔을 바꿀 이벤트
    /// </summary>
    [SerializeField] private EventType[] _subscribeForLightChange;

    private Animator _animator;
    private float _playerCount = 0;

    [SerializeField] private bool _isOpened = false;

    /// <summary>
    /// 문이 열리기 위해선 Host에서 IsOpened가 true 상태이어야 함.
    /// </summary>
    public bool IsOpened
    {
        get => _isOpened;
        set
        {
            if (_automateLight)
            {
                SetDoorLightClientRpc(value, ColorType.None);
            }
            
            _isOpened = value;
        }
    } 

    public override void OnNetworkSpawn()
    {
        _animator = GetComponent<Animator>();

        if (_lightRenderer != null)
        {
            if (PlayerController.LocalPlayer != null)
            {
                _lightRenderer.material.SetPlayerColor(PlayerController.LocalPlayer.Color);
            }
            else
            {
                PlayerController.LocalPlayerCreated += () =>
                {
                    _lightRenderer.material.SetPlayerColor(PlayerController.LocalPlayer.Color);
                };
            }
        }
    }

    /// <summary>
    /// IsOpened = true라면 문을 엽니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void OpenDoorServerRpc()
    {
        if (IsOpened)
        {
            _animator.SetBool("Open", true);
        }
    }
    
    /// <summary>
    /// 문을 닫습니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void CloseDoorServerRpc()
    {
        _animator.SetBool("Open", false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        if (other.GetComponent<PlayerController>() != null)
        {
            _playerCount++;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        if (_isTriggerable && _playerCount > 0)
        {
            OpenDoorServerRpc();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer)
        {
            return;
        }
        if (other.GetComponent<PlayerController>() != null)
        {
            _playerCount--;

            if (_isTriggerable && _playerCount == 0)
            {
                CloseDoorServerRpc();
            }
        }
    }

    public bool IsActivatable(GameObject activator = null)
    {
        return true;
    }

    public bool Activate(GameObject activator = null)
    {
        OpenDoorServerRpc();

        return true;
    }

    public bool Deactivate(GameObject activator = null)
    {
        CloseDoorServerRpc();

        return true;
    }

    public void SetOpen()
    {
        IsOpened = true;
    }

    public void SetClose()
    {
        IsOpened = false;
        CloseDoorServerRpc();
    }

    /// <summary>
    /// 서버와 클라이언트의 초기 상태를 동기화한다. 이 함수는 서버와 클라이언트 모두에서 호출된다.
    /// </summary>
    /// <param name="isTriggerable">주변에 가면 켜질지 여부</param>
    /// <param name="isOpen">열린 상태 여부</param>
    [ClientRpc]
    private void InitializeClientRpc(bool isTriggerable, bool automateLight, bool isOpen)
    {
        _isTriggerable = isTriggerable;
        _automateLight = automateLight;

        IsOpened = isOpen;
    }

    /// <summary>
    /// 문 상태를 초기화하고 클라이언트와 동기화한다. 이 함수는 서버에서만 호출한다.
    /// </summary>
    /// <param name="isTriggerable">주변에 가면 켜질지 여부</param>
    /// <param name="isOpen">열린 상태 여부</param>
    public void Initialize(bool isTriggerable, bool automateLight, bool isOpen, EventType[] subscribeForActivation, EventType[] subscribeForDeactivation, EventType[] subscribeForSetOpen, EventType[] subscribeForSetClose, EventType[] subscribeForLightChange)
    {
        InitializeClientRpc(isTriggerable, automateLight, isOpen);

        _subscribeForActivation = subscribeForActivation;
        _subscribeForDeactivation = subscribeForDeactivation;
        _subscribeForSetOpen = subscribeForSetOpen;
        _subscribeForSetClose = subscribeForSetClose;
        _subscribeForLightChange = subscribeForLightChange;

        foreach (EventType eventType in _subscribeForActivation)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, OpenDoorServerRpc);
        }

        foreach (EventType eventType in _subscribeForDeactivation)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, CloseDoorServerRpc);
        }

        foreach (EventType eventType in _subscribeForSetOpen)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, SetOpen);
        }

        foreach (EventType eventType in _subscribeForSetClose)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, SetClose);
        }

        foreach (EventType eventType in _subscribeForLightChange)
        {
            EventBus.Instance.SubscribeEvent<UnityAction<bool, ColorType>>(eventType, SetDoorLight);
        }
    }
    
    [ClientRpc]
    private void SetDoorLightClientRpc(bool isOn, ColorType color)
    {
        if (_lightRenderer != null)
        {
            if (isOn)
            {
                _lightRenderer?.material.SetInt("_IsOn", 1);
                _lightRenderer?.material.SetObjectColor(color);
            }
            else
            {
                _lightRenderer?.material.SetInt("_IsOn", 0);
                _lightRenderer?.material.SetObjectColor(ColorType.None);
            }
        }
    }

    public void SetDoorLight(bool isOn, ColorType colorType)
    {
        SetDoorLightClientRpc(isOn, colorType);
    }
}
