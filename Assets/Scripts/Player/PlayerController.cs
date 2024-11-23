using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private float _walkSpeed = 10.0f;

    [SerializeField]
    private float _rotateSpeed = 2f;

    private GameObject _mainCamera;

    private float _pitchAngle;

    private Rigidbody _rigidbody;

    public override void OnNetworkSpawn()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (IsOwner)
        {
            _mainCamera = new GameObject("Main Camera");
            _mainCamera.transform.parent = transform;
            _mainCamera.transform.localPosition = new Vector3(0, 20, 0);
            _mainCamera.AddComponent<Camera>().cullingMask ^= 1 << LayerMask.NameToLayer("UI");
            _mainCamera.AddComponent<AudioListener>();
            _mainCamera.tag = "MainCamera";

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        CameraHandler();
        MoveHandler();
    }

    private void CameraHandler()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        _pitchAngle = Mathf.Clamp(_pitchAngle + mouseY * _rotateSpeed, -90, 90);

        transform.Rotate(new Vector3(0, mouseX * _rotateSpeed, 0));
        Vector3 cameraRot = _mainCamera.transform.rotation.eulerAngles;
        _mainCamera.transform.rotation = Quaternion.Euler(_pitchAngle, cameraRot.y, cameraRot.z);
    }

    private void MoveHandler()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = (v * transform.forward + h * transform.right).normalized * _walkSpeed;
        _rigidbody.velocity = new Vector3(moveDir.x, _rigidbody.velocity.y, moveDir.z);
    }
}
