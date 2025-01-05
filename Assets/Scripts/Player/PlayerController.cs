using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어를 조작하는 Class
/// </summary>
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;    // 이동 속도
    [SerializeField] private float _jumpSpeed = 5f;     // 점프 속도

    private const float GROUND_DETECTION_THRESHOLD = 0.1f;  // 접지 판정 범위
    private const float PLATFORM_DETECTION_THRESHOLD = 2f;  // 플랫폼 탐색 범위
    private const float SEND_TRANSFORM_THRESHOLD = 0.01f;   // transform 전송 기준값

    private CharacterController _characterController;
    private NetworkInterpolator _networkInterpolator;

    private float _colliderHeight;              // 플레이어 콜라이더 높이의 절반 값 (h/2)
    private Rigidbody _platform;                // 플레이어가 따라갈 플랫폼
    private GameObject _interactableOnPointer;  // 플레이어가 바라보고 있는 Interactable
    private GameObject _interactableInHand;     // 플레이어가 들고 있는 Interactable

    // 입력 관련
    private Vector3 _moveInput;     // 방향 입력 값 (수직, 수평)
    private bool _jumpInput;        // 점프 입력 여부
    private float _jumpRemember;    // 입력된 점프를 처리할 수 있는 쿨타임
    private float _verticalSpeed;   // 현재 수직 속력

    // transform 싱크 관련
    private Vector3 _lastSyncedPosition;    // 마지막으로 주고 받은 위치
    private Quaternion _lastSyncedRotation; // 마지막으로 주고 받은 회전

    public override void OnNetworkSpawn()
    {
        // TEST
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
            else    // 보간용 플레이어 오브젝트가 아직 생성되지 않은 경우
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

    /// <summary>
    /// X, Z 축 입력을 처리한다.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 rotation = Camera.main.transform.rotation.eulerAngles;
        rotation.x = 0;
        rotation.z = 0;

        _characterController.Move((Quaternion.Euler(rotation) * _moveInput) * Time.deltaTime * _moveSpeed);
    }

    /// <summary>
    /// Y 축 이동 및 점프 입력을 처리한다.
    /// </summary>
    private void HandleJump()
    {
        _jumpRemember -= Time.deltaTime;

        if (IsGrounded())
        {
            // 떨어지는 중에 접지한 경우
            if (_verticalSpeed < 0f)
            {
                _verticalSpeed = 0f;
            }

            // 점프 입력이 들어온 상태에서 접지한 경우
            if (_jumpInput)
            {
                // 아직 점프를 처리할 수 있는 쿨타임이 남은 경우
                if (_jumpRemember > 0f)
                {
                    _verticalSpeed = _jumpSpeed;
                }

                _jumpInput = false;
            }
        }
        else
        {
            // 공중에서 떨어지고 있는 경우
            _verticalSpeed += Physics.gravity.y * Time.deltaTime;
        }

        _characterController.Move(new Vector3(0, _verticalSpeed * Time.deltaTime, 0));
    }

    /// <summary>
    /// 플레이어와 플랫폼의 관계를 처리한다.
    /// </summary>
    private void HandlePlatform()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _colliderHeight + PLATFORM_DETECTION_THRESHOLD))
        {
            if (_platform != hit.collider.gameObject)
            {
                // 새로운 플랫폼을 발견한 경우
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

        // 플랫폼에 올라가 있다면 플랫폼의 이동을 플레이어에게도 적용
        if (_platform)
        {
            _characterController.Move(_platform.velocity * Time.deltaTime);
        }
    }

    /// <summary>
    /// 플레이어와 플랫폼 사이의 연결을 제거한다.
    /// </summary>
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

    /// <summary>
    /// 플레이어가 보고 있는 Interactable을 탐색한다.
    /// </summary>
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

    /// <summary>
    /// 상대에게 자신의 위치를 전송한다.
    /// </summary>
    private void SendTransform()
    {
        Vector3 positionToSend = transform.position;

        // 플랫폼에 올라가 있는 경우, 상대(relative) 위치를 전송.
        if (_platform)
        {
            positionToSend -= _platform.transform.position;
        }

        float positionDiff = Vector3.Distance(transform.position, _lastSyncedPosition);
        float rotationDiff = Quaternion.Angle(transform.rotation, _lastSyncedRotation) / 10f;

        // transform 변화값이 기준을 넘어서는 경우에만 전송
        if (positionDiff < SEND_TRANSFORM_THRESHOLD && rotationDiff < SEND_TRANSFORM_THRESHOLD)
        {
            return;
        }

        // 마지막으로 전송한 transform 값 저장
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

    /// <summary>
    /// 전송 받은 transform에 따라 상대의 위치를 갱신한다.
    /// </summary>
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

    /// <summary>
    /// 접지 여부를 판단한다.
    /// </summary>
    /// <returns>접지 여부</returns>
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, _colliderHeight + GROUND_DETECTION_THRESHOLD);
    }

    /// <summary>
    /// X, Z 축 입력을 받는 Callback.
    /// </summary>
    /// <param name="value">입력 값</param>
    void OnMoveInput(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        _moveInput = new Vector3(input.x, 0, input.y).normalized;
    }

    /// <summary>
    /// 점프 입력을 받는 Callback.
    /// </summary>
    void OnJumpInput()
    {
        _jumpInput = true;
        _jumpRemember = 0.16f;
    }

    /// <summary>
    /// 상호작용 입력을 받는 Callback.
    /// </summary>
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

    /// <summary>
    /// ESC 입력을 받는 Callback.
    /// </summary>
    void OnEscapeInput()
    {

    }

    /// <summary>
    /// 클라이언트에서 서버로 새 플랫폼 연결을 전달한다.
    /// </summary>
    /// <param name="platform">새 플랫폼</param>
    [ServerRpc]
    public void SetPlatformServerRpc(NetworkObjectReference platform)
    {
        if (platform.TryGet(out NetworkObject networkObject))
        {
            networkObject.TryGetComponent<Rigidbody>(out _platform);
        }
    }

    /// <summary>
    /// 서버에서 클라이언트로 새 플랫폼 연결을 전달한다.
    /// </summary>
    /// <param name="platform">새 플랫폼</param>
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

    /// <summary>
    /// 클라이언트에서 서버로 플랫폼 연결 초기화를 요청한다.
    /// </summary>
    [ServerRpc]
    public void ResetPlatformServerRpc()
    {
        _platform = null;
    }

    /// <summary>
    /// 서버에서 클라이언트로 플랫폼 연결 초기화를 요청한다.
    /// </summary>
    [ClientRpc]
    public void ResetPlatformClientRpc()
    {
        if (IsServer)
        {
            return;
        }

        _platform = null;
    }

    /// <summary>
    /// 클라이언트에서 서버로 transform을 전달한다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <param name="rotation">회전</param>
    [ServerRpc]
    public void SendTransformServerRpc(Vector3 position, Quaternion rotation)
    {
        _lastSyncedPosition = position;
        _lastSyncedRotation = rotation;
    }

    /// <summary>
    /// 서버에서 클라이언트로 transform을 전달한다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <param name="rotation">회전</param>
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
