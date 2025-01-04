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
    private NetworkObject _platform;

    private Vector3 _moveInput;
    private bool _jumpInput;
    private float _jumpRemember;
    private float _verticalSpeed;

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
            Vector3 rotation = Camera.main.transform.rotation.eulerAngles;
            rotation.x = 0;
            rotation.z = 0;

            _characterController.Move((Quaternion.Euler(rotation) * _moveInput) * Time.deltaTime * _moveSpeed);

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

            if (_platform && _platform.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                _characterController.Move(rigidbody.velocity * Time.deltaTime);
            }

            FindPlatform();

            if (IsServer)
            {
                if (_platform)
                {
                    UpdateTransformClientRpc(_platform, transform.position - _platform.transform.position, transform.rotation);
                }
                else
                {
                    UpdateTransformClientRpc(transform.position, transform.rotation);
                }
            }
            else
            {
                if (_platform)
                {
                    UpdateTransformServerRpc(_platform, transform.position - _platform.transform.position, transform.rotation);
                }
                else
                {
                    UpdateTransformServerRpc(transform.position, transform.rotation);
                }
            }
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
            hit.collider.gameObject.TryGetComponent<NetworkObject>(out _platform);
        }
        else
        {
            _platform = null;
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
        _jumpRemember = 0.1f;
    }

    void OnInteractionInput()
    {

    }

    void OnEscapeInput()
    {

    }

    public void UpdateTransform(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    [ServerRpc]
    public void UpdateTransformServerRpc(NetworkObjectReference platform, Vector3 position, Quaternion rotation)
    {
        if (platform.TryGet(out NetworkObject networkObject))
        {
            position += networkObject.transform.position;
        }

        UpdateTransform(position, rotation);
    }

    [ClientRpc]
    public void UpdateTransformClientRpc(NetworkObjectReference platform, Vector3 position, Quaternion rotation)
    {
        if (IsServer)
        {
            return;
        }

        if (platform.TryGet(out NetworkObject networkObject))
        {
            position += networkObject.transform.position;
        }

        UpdateTransform(position, rotation);
    }

    [ServerRpc]
    public void UpdateTransformServerRpc(Vector3 position, Quaternion rotation)
    {
        UpdateTransform(position, rotation);
    }

    [ClientRpc]
    public void UpdateTransformClientRpc(Vector3 position, Quaternion rotation)
    {
        UpdateTransform(position, rotation);
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
