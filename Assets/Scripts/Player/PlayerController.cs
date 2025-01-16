using Cinemachine;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어를 조작하는 Class
/// </summary>
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;    // 이동 속력
    [SerializeField] private float _jumpSpeed = 5f;     // 점프 속력

    private const float GROUND_DETECTION_THRESHOLD = 0.1f;      // 접지 판정 범위
    private const float JUMP_REMEMBER_TIME = 0.32f;             // 점프 키 입력 기억 시간

    public static float INITIAL_CAPSULE_HEIGHT = 2f;             // 최초 Capsule Collider 높이
    public static float INITIAL_CAPSULE_RADIUS = 0.5f;           // 최초 Capsule Collider 반경 

    private CharacterController _characterController;
    private PlayerRenderer _playerRenderer;
    private NetworkInterpolator _networkInterpolator;
    private NetworkPlatformFinder _networkPlatformFinder;

    private float _colliderHeight;              // 플레이어 콜라이더 높이의 절반 값 (h/2)
    private IInteractable _interactableOnPointer;  // 플레이어가 바라보고 있는 Interactable
    private IInteractable _interactableInHand;     // 플레이어가 들고 있는 Interactable

    // 서버에서 플레이어 색깔이 지정되었는지 확인하는 delegate
    private Action<ColorType> _playerColorAssigned;

    // 입력 관련
    private Vector3 _moveInput;     // 방향 입력 값 (수직, 수평)
    private bool _jumpInput;        // 점프 입력 여부
    private float _jumpRemember;    // 입력된 점프를 처리할 수 있는 쿨타임
    private float _verticalSpeed;   // 현재 수직 속력

    // 로컬 플레이어를 나타내는 static 변수
    private static PlayerController _localPlayer;
    public static PlayerController LocalPlayer
    {
        get => _localPlayer;
    }

    // 로컬 플레이어가 생성됐을 때 호출되는 delegate
    private static Action _localPlayerCreated;
    public static Action LocalPlayerCreated
    {
        get => _localPlayerCreated;
        set => _localPlayerCreated = value;
    }
    
    // 플레이어 색깔
    private ColorType _playerColor;
    public ColorType PlayerColor
    {
        get => _playerColor;
    }

    public override void OnNetworkSpawn()
    {
        // TEST
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        _playerRenderer = GetComponent<PlayerRenderer>();
        _characterController = GetComponent<CharacterController>();

        INITIAL_CAPSULE_HEIGHT = _characterController.height;
        INITIAL_CAPSULE_RADIUS = _characterController.radius;

        if (!IsOwner)
        {
            _characterController.height *= 0.8f;
        }

        _colliderHeight = _characterController.height / 2f;

        // 임시: 플레이어 색깔 지정
        if (IsServer)
        {
            if (IsOwner)
            {
                _playerColor = ColorType.Blue;

                _localPlayer = this;
                _localPlayerCreated?.Invoke();
            }
            else
            {
                _playerColor = ColorType.Red;
            }

            _playerColorAssigned?.Invoke(_playerColor);
            _playerRenderer.Initialize();
        }
        else
        {
            RequestPlayerColorServerRpc();
        }

        if (IsOwner)
        {
            _networkInterpolator = GetComponent<NetworkInterpolator>();
            _networkPlatformFinder = GetComponent<NetworkPlatformFinder>();

            CinemachineFreeLook camera = GetComponentInChildren<CinemachineFreeLook>();

            _networkInterpolator.AddVisualReferenceDependantFunction(() =>
            {
                camera.Follow = _networkInterpolator.VisualReference.transform;
                camera.LookAt = _networkInterpolator.VisualReference.transform;
            });

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
        transform.rotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// Y 축 이동 및 점프 입력을 처리한다.
    /// </summary>
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
                // 아직 점프를 처리할 수 있는 쿨타임이 남은 경우
                if (_jumpRemember > 0f)
                {
                    _verticalSpeed = _jumpSpeed;
                }

                _jumpInput = false;
            }
        }
        else if (!_networkPlatformFinder.Platform || !IsGrounded())
        {
            _verticalSpeed += Physics.gravity.y * Time.deltaTime;
        }

        _characterController.Move(new Vector3(0, _verticalSpeed * Time.deltaTime, 0));
    }

    /// <summary>
    /// 플레이어와 플랫폼의 관계를 처리한다.
    /// </summary>
    private void HandlePlatform()
    {
        // 플랫폼에 올라가 있다면 플랫폼의 이동을 플레이어에게도 적용
        if (_networkPlatformFinder.Platform)
        {
            transform.position += _networkPlatformFinder.Velocity * Time.deltaTime;
        }
    }

    /// <summary>
    /// 플레이어가 보고 있는 Interactable을 탐색한다.
    /// </summary>
    private void SearchInteractables()
    {
        if (_interactableInHand != null)
        {
            return;
        }

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 20f) &&
            hit.collider.gameObject.TryGetComponent<IInteractable>(out IInteractable interactable) &&
            interactable.IsInteractable(this))
        {
            if (_interactableOnPointer != interactable)
            {
                if (_interactableOnPointer != null)
                {
                    _interactableOnPointer.Outline.enabled = false;
                }

                _interactableOnPointer = interactable;
                _interactableOnPointer.Outline.enabled = true;
            }
        }
        else if (_interactableOnPointer != null)
        {
            _interactableOnPointer.Outline.enabled = false;
            _interactableOnPointer = null;
        }

        gameObject.layer = 0;
    }

    /// <summary>
    /// 접지 여부를 판단한다.
    /// </summary>
    /// <returns>접지 여부</returns>
    bool IsGrounded()
    {
        Vector3 offset = Vector3.up * (_colliderHeight - _characterController.radius);
        return Physics.CapsuleCast(transform.position + offset, transform.position - offset, _characterController.radius, Vector3.down, GROUND_DETECTION_THRESHOLD);
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
        _jumpRemember = JUMP_REMEMBER_TIME;
    }

    /// <summary>
    /// 상호작용 입력을 받는 Callback.
    /// </summary>
    void OnInteractionInput()
    {
        if (_interactableInHand != null)
        {
            if (_interactableOnPointer.StopInteraction(this))
            {
                _interactableOnPointer = null;
                _interactableInHand = null;
            }
        }
        else if (_interactableOnPointer != null)
        {
            if (_interactableOnPointer.StartInteraction(this))
            {
                _interactableOnPointer.Outline.enabled = false;
                _interactableInHand = _interactableOnPointer;
            }
        }
    }

    /// <summary>
    /// ESC 입력을 받는 Callback.
    /// </summary>
    void OnEscapeInput()
    {

    }

    /// <summary>
    /// 클라이언트 측에서 서버에게 플레이어 색깔을 묻는다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestPlayerColorServerRpc()
    {
        if (_playerColor == 0)
        {
            _playerColorAssigned += SendPlayerColorClientRpc;
        }
        else
        {
            SendPlayerColorClientRpc(_playerColor);
        }
    }

    /// <summary>
    /// 서버 측에서 클라이언트에게 플레이어 색깔을 전달한다.
    /// </summary>
    /// <param name="color">플레이어 색깔</param>
    [ClientRpc(RequireOwnership = false)]
    private void SendPlayerColorClientRpc(ColorType color)
    {
        if (IsServer)
        {
            return;
        }

        _playerColor = color;
        _playerRenderer.Initialize();

        if (IsOwner)
        {
            _localPlayer = this;
            _localPlayerCreated?.Invoke();
        }
    }

    /// <summary>
    /// 플레이어의 Capsule Collider 정보를 갱신한다
    /// </summary>
    /// <param name="collider">반영할 Collider 정보</param>
    /// <param name="height">Mesh Collider에서 사용할 높이</param>
    /// <param name="radius">Mesh Collider에서 사용할 반경</param>
    public void UpdateCollider(Collider collider = null, float height = 0f, float radius = 0f)
    {
        // 새 Collider가 Null이면 최초 상태로 초기화한다.
        if (collider == null)
        {
            _characterController.radius = INITIAL_CAPSULE_RADIUS;
            _characterController.height = INITIAL_CAPSULE_HEIGHT;

            if (!IsOwner)
            {
                _characterController.height *= 0.9f;
            }

            _colliderHeight = INITIAL_CAPSULE_HEIGHT / 2f;
        }
        else if (collider is BoxCollider)
        {
            _characterController.radius = ((BoxCollider)collider).size.x / 2f;
            _characterController.height = ((BoxCollider)collider).size.y;

            if (!IsOwner)
            {
                _characterController.height *= 0.9f;
            }

            _colliderHeight = ((BoxCollider)collider).size.y / 2f;
        }
    }

    private void OnGUI()
    {
        if (IsOwner)
        {
            GUILayout.BeginArea(new Rect(10, 10, 100, 100));
            if (_networkPlatformFinder?.Platform) GUILayout.Label($"{_networkPlatformFinder.Velocity.y}");
            GUILayout.EndArea();
        }
    }
}
