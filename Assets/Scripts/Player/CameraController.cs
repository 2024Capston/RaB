using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera _firstPersonCamera;
    [SerializeField] CinemachineFreeLook _thirdPersonCamera;

    private Vector2 _rotateInput;

    private CinemachinePOV _cinemachinePOV;

    private bool _isFirstPerson;
    public bool IsFirstPerson
    {
        get => _isFirstPerson;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _cinemachinePOV = _firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
            _isFirstPerson = _firstPersonCamera.m_Priority > _thirdPersonCamera.m_Priority;

            NetworkInterpolator networkInterpolator = GetComponent<NetworkInterpolator>();

            networkInterpolator.AddVisualReferenceDependantFunction(() =>
            {
                _firstPersonCamera.Follow = networkInterpolator.VisualReference.transform;

                _thirdPersonCamera.Follow = networkInterpolator.VisualReference.transform;
                _thirdPersonCamera.LookAt = networkInterpolator.VisualReference.transform;
            });

            _firstPersonCamera.transform.parent = null;

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Destroy(_firstPersonCamera.gameObject);
            Destroy(_thirdPersonCamera.gameObject);
            Destroy(GetComponentInChildren<Camera>().gameObject);
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (_isFirstPerson)
        {
            _cinemachinePOV.m_VerticalAxis.m_InputAxisValue = _rotateInput.y;
            _cinemachinePOV.m_HorizontalAxis.m_InputAxisValue = _rotateInput.x;
        }
        else
        {
            _thirdPersonCamera.m_YAxis.m_InputAxisValue = _rotateInput.y;
            _thirdPersonCamera.m_XAxis.m_InputAxisValue = _rotateInput.x;
        }
    }

    public void ChangeCameraMode(bool toFirstPerson)
    {
        if (toFirstPerson && !_isFirstPerson)
        {
            _isFirstPerson = true;

            _firstPersonCamera.m_Priority = 10;
            _thirdPersonCamera.m_Priority = 0;

            _cinemachinePOV.m_HorizontalAxis.Value = Camera.main.transform.rotation.eulerAngles.y;
            _cinemachinePOV.m_VerticalAxis.Value = 0f;
        }
        else if (!toFirstPerson && _isFirstPerson)
        {
            _isFirstPerson = false;

            _firstPersonCamera.m_Priority = 0;
            _thirdPersonCamera.m_Priority = 10;
        }
    }

    public void OnLookAroundInput(InputValue value)
    {
        _rotateInput = value.Get<Vector2>() / 64f;
    }
}
