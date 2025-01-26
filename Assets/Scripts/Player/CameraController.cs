using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera _firstPersonCamera;
    [SerializeField] CinemachineFreeLook _thirdPersonCamera;
    
    private bool _isFirstPerson;
    public bool IsFirstPerson
    {
        get => _isFirstPerson;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            NetworkInterpolator networkInterpolator = GetComponent<NetworkInterpolator>();

            networkInterpolator.AddVisualReferenceDependantFunction(() =>
            {
                _firstPersonCamera.Follow = networkInterpolator.VisualReference.transform;
                _firstPersonCamera.LookAt = networkInterpolator.VisualReference.transform;

                _thirdPersonCamera.Follow = networkInterpolator.VisualReference.transform;
                _thirdPersonCamera.LookAt = networkInterpolator.VisualReference.transform;
            });

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Destroy(_firstPersonCamera.gameObject);
            Destroy(_thirdPersonCamera.gameObject);
            Destroy(GetComponentInChildren<Camera>().gameObject);
        }
    }

    public void ChangeCameraMode(bool toFirstPerson)
    {
        if (toFirstPerson && !_isFirstPerson)
        {
            _isFirstPerson = true;

            _firstPersonCamera.m_Priority = 10;
            _thirdPersonCamera.m_Priority = 0;
        }
        else if (!toFirstPerson && _isFirstPerson) {
            _isFirstPerson = false;

            _firstPersonCamera.m_Priority = 0;
            _thirdPersonCamera.m_Priority = 10;
        }
    }
}
