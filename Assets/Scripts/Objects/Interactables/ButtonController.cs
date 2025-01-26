using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using System;
using UnityEngine.Events;
using System.Runtime.CompilerServices;

/// <summary>
/// 버튼을 조작하는 Class
/// </summary>
public class ButtonController : NetworkBehaviour, IInteractable
{
    /// <summary>
    /// 버튼 색깔
    /// </summary>
    [SerializeField] private ColorType _buttonColor;

    /// <summary>
    /// 버튼 작동 방식
    /// </summary>
    [SerializeField] private ButtonType _buttonType;

    /// <summary>
    /// 작동 방식이 Temporary일 때 비활성화까지 걸리는 시간
    /// </summary>
    [SerializeField] private float _temporaryCooldown;

    /// <summary>
    /// 두 플레이어가 모두 근처에 위치해야 하는지 여부
    /// </summary>
    [SerializeField] private bool _requiresBoth;

    /// <summary>
    /// Requires Both가 true일 때 인식 반경
    /// </summary>
    [SerializeField] private float _detectionRadius;

    /// <summary>
    /// 버튼이 활성화됐을 때 Activate될 IInteractable 오브젝트
    /// </summary>
    [SerializeField] private GameObject[] _activatables;

    /// <summary>
    /// 버튼이 활성화됐을 때 실행될 함수
    /// </summary>
    [SerializeField] private UnityEvent _events;

    /// <summary>
    /// 렌더링에 쓰일 매터리얼. (파랑, 빨강 순)
    /// </summary>
    [SerializeField] private Material[] _materials;

    private bool _isPressed = false;    // 버튼이 눌려서 활성화됐는지 여부
    private bool _isEnabled = true;     // 버튼을 사용 가능한지 여부
    private float _temporaryTime = 0f;  // Temporary용 타이머

    private Outline _outline;
    public Outline Outline {
        get => _outline;
        set => _outline = value;
    }

    public override void OnNetworkSpawn()
    {
        GetComponent<MeshRenderer>().material = _materials[(int)_buttonColor - 1];
        _outline = GetComponent<Outline>();
    }

    private void Update()
    {
        if (!IsServer || !_isEnabled || _buttonType != ButtonType.Temporary)
        {
            return;
        }

        // Temporary 처리
        if (_isPressed)
        {
            _temporaryTime -= Time.deltaTime;

            if (_temporaryTime < 0f)
            {
                SetButtonPressStateClientRpc(false);
                DeactivateObjects();
            }
        }
    }

    private void ActivateObjects()
    {
        // IActivatable 활성화
        foreach (GameObject gameObject in _activatables)
        {
            if (gameObject.TryGetComponent<IActivatable>(out IActivatable activatable))
            {
                activatable.Activate(gameObject);
            }
        }

        // 이벤트 호출
        _events.Invoke();
    }

    private void DeactivateObjects()
    {
        // IActivatable 비활성화
        foreach (GameObject gameObject in _activatables)
        {
            if (gameObject.TryGetComponent<IActivatable>(out IActivatable activatable))
            {
                activatable.Deactivate(gameObject);
            }
        }
    }

    public bool IsInteractable(PlayerController player)
    {
        // 버튼이 비활성화 상태인 경우
        if (!_isEnabled)
        {
            return false;
        }
        // 플레이어 둘이 주변에 있어야 하는 경우
        else if (_requiresBoth)
        {
            PlayerController[] playerControllers = FindObjectsOfType<PlayerController>();

            foreach (PlayerController playerController in playerControllers)
            {
                if (Vector3.Distance(transform.position, playerController.transform.position) > _detectionRadius) {
                    return false;
                }
            }
        }
        
        return (_buttonColor == ColorType.Purple || _buttonColor == player.Color) && (_buttonType == ButtonType.Toggle || !_isPressed);
    }

    public bool StartInteraction(PlayerController player)
    {
        PressButtonServerRpc();

        return false;
    }

    public bool StopInteraction(PlayerController player)
    {
        return false;
    }

    /// <summary>
    /// 서버 측에서 버튼 입력을 처리한다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void PressButtonServerRpc()
    {
        // 버튼 타입: Persistent
        if (_buttonType == ButtonType.Persistent && !_isPressed)
        {
            SetButtonPressStateClientRpc(true);

            ActivateObjects();
        }
        // 버튼 타입: Toggle
        else if (_buttonType == ButtonType.Toggle)
        {
            SetButtonPressStateClientRpc(!_isPressed);

            if (_isPressed)
            {
                ActivateObjects();
            }
            else
            {
                DeactivateObjects();
            }
        }
        // 버튼 타입: Temporary
        else if (_buttonType == ButtonType.Temporary)
        {
            if (!_isPressed)
            {
                SetButtonPressStateClientRpc(true);
                _temporaryTime = _temporaryCooldown;

                ActivateObjects();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetButtonStateServerRpc(bool isEnabled)
    {
        SetButtonStateClientRpc(isEnabled);
    }

    [ClientRpc]
    private void SetButtonStateClientRpc(bool isEnabled)
    {
        _isEnabled = isEnabled;
    }

    [ClientRpc]
    private void SetButtonPressStateClientRpc(bool isPressed)
    {
        _isPressed = isPressed;
    }

    /// <summary>
    /// 버튼을 활성화한다.
    /// </summary>
    public void EnableButton()
    {
        SetButtonStateServerRpc(true);
    }

    /// <summary>
    /// 버튼을 비활성화한다.
    /// </summary>
    public void DisableButton()
    {
        SetButtonStateServerRpc(false);
    }
}
