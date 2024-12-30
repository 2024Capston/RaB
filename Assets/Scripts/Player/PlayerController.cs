using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;

    private CharacterController _characterController;
    private float _colliderHeight;

    private Vector3 _moveInput;
    private bool _jumpInput;
    private float _verticalSpeed;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _characterController = GetComponent<CharacterController>();
            _colliderHeight = GetComponent<CapsuleCollider>().height / 2f;

            FindObjectOfType<CinemachineFreeLook>().Follow = transform;
            FindObjectOfType<CinemachineFreeLook>().LookAt = transform;

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            GetComponent<PlayerInput>().enabled = false;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            Vector3 rotation = Camera.main.transform.rotation.eulerAngles;
            rotation.x = 0;
            rotation.z = 0;

            _characterController.Move((Quaternion.Euler(rotation) * _moveInput) * Time.deltaTime * _moveSpeed);

            if (OnGround() && _verticalSpeed < 0f)
            {
                 _verticalSpeed = 0f;
            }

            if (_jumpInput)
            {
                _verticalSpeed = 5f;
                _jumpInput = false;
            }

            _verticalSpeed += Physics.gravity.y * Time.deltaTime;
            _characterController.Move(new Vector3(0, _verticalSpeed * Time.deltaTime, 0));
        }
    }

    bool OnGround()
    {
        RaycastHit[] hit = Physics.RaycastAll(transform.position, Vector3.down, _colliderHeight + 0.1f);

        return hit.Length > 0;
    }

    void OnMoveInput(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        _moveInput = new Vector3(input.x, 0, input.y).normalized;
    }

    void OnJumpInput()
    {
        if (OnGround())
        {
            _jumpInput = true;
        }
    }

    void OnInteractionInput()
    {

    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 30, 30));
        GUILayout.Label($"{_characterController?.isGrounded}");
        GUILayout.EndArea();
    }
}
