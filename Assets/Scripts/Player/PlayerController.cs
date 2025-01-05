using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _jumpSpeed = 5f;

    private const float GROUND_DETECTION_THRESHOLD = 0.1f;
    private const float PLATFORM_DETECTION_THRESHOLD = 2f;
    private const float SEND_TRANSFORM_THRESHOLD = 0.01f;

    private CharacterController _characterController;
    private NetworkInterpolator _networkInterpolator;

    private float _colliderHeight;
    private Rigidbody _platform;
    private GameObject _interactableOnPointer;
    private GameObject _interactableInHand;

    private Vector3 _moveInput;
    private bool _jumpInput;
    private float _jumpRemember;
    private float _verticalSpeed;

    private Vector3 _lastSyncedPosition;
    private Quaternion _lastSyncedRotation;

    public override void OnNetworkSpawn()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        if (IsOwner)
        {
            _characterController = GetComponent<CharacterController>();
            _networkInterpolator = GetComponent<NetworkInterpolator>();

            _colliderHeight = GetComponent<CapsuleCollider>().height / 2f;

            CinemachineFreeLook camera = GetComponentInChildren<CinemachineFreeLook>();

            if (_networkInterpolator.VisualReference)
            {
                camera.Follow = _networkInterpolator.VisualReference.transform;
                camera.LookAt = _networkInterpolator.VisualReference.transform;
            }
            else
            {
                _networkInterpolator.VisualReferenceCreated += () =>
                {
                    camera.Follow = _networkInterpolator.VisualReference.transform;
                    camera.LookAt = _networkInterpolator.VisualReference.transform;
                };
            }

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            GetComponent<PlayerInput>().enabled = false;

            Destroy(GetComponentInChildren<CinemachineFreeLook>().gameObject);
            Destroy(GetComponentInChildren<Camera>().gameObject);
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            HandleMovement();
            HandleJump();
            HandlePlatform();
            SearchInteractables();

            SendTransform();
        }
        else if (_lastSyncedPosition != null && _lastSyncedRotation != null)
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

        if (IsGrounded())
        {
            if (_verticalSpeed < 0f)
            {
                _verticalSpeed = 0f;
            }

            if (_jumpInput)
            {
                if (_jumpRemember > 0f)
                {
                    _verticalSpeed = _jumpSpeed;
                }

                _jumpInput = false;
            }
        }
        else
        {
            _verticalSpeed += Physics.gravity.y * Time.deltaTime;
        }

        _characterController.Move(new Vector3(0, _verticalSpeed * Time.deltaTime, 0));
    }

    private void HandlePlatform()
    {
        FindPlatform();

        if (_platform)
        {
            transform.position += _platform.velocity * Time.deltaTime;
        }
    }

    private void SearchInteractables()
    {
        if (_interactableInHand)
        {
            return;
        }

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 20f))
        {
            if (_interactableOnPointer != hit.collider.gameObject)
            {
                if (_interactableOnPointer)
                {
                    _interactableOnPointer = null;
                }

                if (hit.collider.gameObject.TryGetComponent<IInteractable>(out IInteractable interactable) && interactable.IsInteractable(this))
                {
                    _interactableOnPointer = hit.collider.gameObject;
                }
            }
        }
    }

    private void SendTransform()
    {
        Vector3 positionToSend = transform.position;

        if (_platform)
        {
            positionToSend -= _platform.transform.position;
        }

        float positionDiff = Vector3.Distance(transform.position, _lastSyncedPosition);
        float rotationDiff = Quaternion.Angle(transform.rotation, _lastSyncedRotation) / 10f;

        if (positionDiff < SEND_TRANSFORM_THRESHOLD && rotationDiff < SEND_TRANSFORM_THRESHOLD)
        {
            return;
        }

        _lastSyncedPosition = positionToSend;
        _lastSyncedRotation = transform.rotation;

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
            transform.position = _lastSyncedPosition + _platform.transform.position;
            transform.rotation = _lastSyncedRotation;
        }
        else
        {
            transform.position = _lastSyncedPosition;
            transform.rotation = _lastSyncedRotation;
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, _colliderHeight + GROUND_DETECTION_THRESHOLD);
    }

    void FindPlatform()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _colliderHeight + PLATFORM_DETECTION_THRESHOLD))
        {
            if (_platform != hit.collider.gameObject)
            {
                if (hit.collider.gameObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject) && 
                    hit.collider.gameObject.TryGetComponent<Rigidbody>(out _platform))
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
        if (_interactableInHand)
        {
            _interactableOnPointer.GetComponent<IInteractable>().StopInteraction(this);
            _interactableInHand = null;
        }
        else if (_interactableOnPointer)
        {
            _interactableOnPointer.GetComponent<IInteractable>().StartInteraction(this);
            _interactableOnPointer = null;
        }
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
        if (IsServer)
        {
            return;
        }

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
        _lastSyncedPosition = position;
        _lastSyncedRotation = rotation;
    }

    [ClientRpc]
    public void SendTransformClientRpc(Vector3 position, Quaternion rotation)
    {
        if (IsServer)
        {
            return;
        }

        _lastSyncedPosition = position;
        _lastSyncedRotation = rotation;
    }

    private void OnGUI()
    {
        if (IsOwner)
        {
            GUILayout.BeginArea(new Rect(10, 10, 100, 100));
            if (_platform) GUILayout.Label($"");
            GUILayout.EndArea();
        }
    }
}
