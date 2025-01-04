using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _jumpSpeed = 5f;

    private GameObject _ghost;
    private CharacterController _characterController;
    private float _colliderHeight;
    private Rigidbody _platform;

    private Vector3 _moveInput;
    private bool _jumpInput;
    private float _jumpRemember;
    private float _verticalSpeed;

    private Vector3 _lastFetchedPosition;
    private Quaternion _lastFetchedRotation;

    public override void OnNetworkSpawn()
    {
        _ghost = new GameObject("Ghost");

        _ghost.AddComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
        _ghost.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<MeshRenderer>());

        _ghost.AddComponent<NetworkInterpolator>().SetTarget(transform, IsOwner);

        if (IsOwner)
        {
            _characterController = GetComponent<CharacterController>();
            _colliderHeight = GetComponent<CapsuleCollider>().height / 2f;

            FindObjectOfType<CinemachineFreeLook>().Follow = _ghost.transform;
            FindObjectOfType<CinemachineFreeLook>().LookAt = _ghost.transform;

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
            HandleMovement();
            HandleJump();
            HandlePlatform();

            SendTransform();
        }
        else if (_lastFetchedPosition != null && _lastFetchedRotation != null)
        {
            UpdateFetchedTransform();
        }
    }

    private void HandleMovement()
    {
        Vector3 rotation = Camera.main.transform.rotation.eulerAngles;
        rotation.x = 0;
        rotation.z = 0;

        _characterController.Move((Quaternion.Euler(rotation) * _moveInput) * Time.deltaTime * _moveSpeed);
    }

    private void HandleJump()
    {
        _jumpRemember -= Time.deltaTime;

        if (IsGrounded() && _verticalSpeed < 0f)
        {
            _verticalSpeed = 0f;
        }

        if (IsGrounded() && _jumpInput)
        {
            if (_jumpRemember > 0f)
            {
                _verticalSpeed = _jumpSpeed;
            }

            _jumpInput = false;
        }

        if (!IsGrounded())
        {
            _verticalSpeed += Physics.gravity.y * Time.deltaTime;
        }

        _characterController.Move(new Vector3(0, _verticalSpeed * Time.deltaTime, 0));
    }

    private void HandlePlatform()
    {
        FindPlatform();

        if (_platform && _platform.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
        {
            _characterController.Move(rigidbody.velocity * Time.deltaTime);
        }
    }

    private void SendTransform()
    {
        Vector3 positionToSend = transform.position;

        if (_platform)
        {
            positionToSend -= _platform.position;
        }

        if (IsServer)
        {
            SendTransformClientRpc(positionToSend, transform.rotation);
        }
        else
        {
            SendTransformServerRpc(positionToSend, transform.rotation);
        }
    }

    private void UpdateFetchedTransform()
    {
        if (_platform)
        {
            transform.position = _lastFetchedPosition + _platform.position;
            transform.rotation = _lastFetchedRotation;
        }
        else
        {
            transform.position = _lastFetchedPosition;
            transform.rotation = _lastFetchedRotation;
        }
    }

    bool IsGrounded()
    {
        RaycastHit[] hit = Physics.RaycastAll(transform.position, Vector3.down, _colliderHeight + 0.16f);

        return hit.Length > 0;
    }

    void FindPlatform()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _colliderHeight + 2f))
        {
            if (hit.collider.gameObject.TryGetComponent<Rigidbody>(out _platform) &&
                hit.collider.gameObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                if (IsServer)
                {
                    SetPlatformClientRpc(networkObject);
                }
                else
                {
                    SetPlatformServerRpc(networkObject);
                }
            }
            else
            {
                ResetPlatform();
            }
        }
        else if (_platform != null)
        {
            ResetPlatform();
        }
    }

    private void ResetPlatform()
    {
        _platform = null;

        if (IsServer)
        {
            ResetPlatformClientRpc();
        }
        else
        {
            ResetPlatformServerRpc();
        }
    }

    void OnMoveInput(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        _moveInput = new Vector3(input.x, 0, input.y).normalized;
    }

    void OnJumpInput()
    {
        _jumpInput = true;
        _jumpRemember = 0.16f;
    }

    void OnInteractionInput()
    {

    }

    void OnEscapeInput()
    {

    }

    [ServerRpc]
    public void SetPlatformServerRpc(NetworkObjectReference platform)
    {
        if (platform.TryGet(out NetworkObject networkObject))
        {
            networkObject.TryGetComponent<Rigidbody>(out _platform);
        }
    }

    [ClientRpc]
    public void SetPlatformClientRpc(NetworkObjectReference platform)
    {
        if (platform.TryGet(out NetworkObject networkObject))
        {
            networkObject.TryGetComponent<Rigidbody>(out _platform);
        }
    }

    [ServerRpc]
    public void ResetPlatformServerRpc()
    {
        _platform = null;
    }

    [ClientRpc]
    public void ResetPlatformClientRpc()
    {
        if (IsServer)
        {
            return;
        }

        _platform = null;
    }

    [ServerRpc]
    public void SendTransformServerRpc(Vector3 position, Quaternion rotation)
    {
        _lastFetchedPosition = position;
        _lastFetchedRotation = rotation;
    }

    [ClientRpc]
    public void SendTransformClientRpc(Vector3 position, Quaternion rotation)
    {
        if (IsServer)
        {
            return;
        }

        _lastFetchedPosition = position;
        _lastFetchedRotation = rotation;
    }

    private void OnGUI()
    {
        if (IsOwner)
        {
            GUILayout.BeginArea(new Rect(10, 10, 100, 100));
            if (_platform)
            {
                Vector3 diff = transform.position - _platform.transform.position;
                GUILayout.Label($"On Platform: {diff.x} {diff.y} {diff.z}");
            }
            else
            {
                GUILayout.Label($"{transform.position.x} {transform.position.y} {transform.position.z}");
            }
            GUILayout.EndArea();
        }
    }
}
