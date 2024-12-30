using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;

    private CharacterController _characterController;
    private float _colliderHeight;
    private NetworkObject _platform;

    private Vector3 _moveInput;
    private bool _jumpInput;
    private float _verticalSpeed;

    public override void OnNetworkSpawn()
    {
        GameObject renderer = new GameObject("Ghost");

        renderer.AddComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
        renderer.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<MeshRenderer>());

        renderer.AddComponent<NetworkInterpolator>().SetTarget(transform);

        if (IsOwner)
        {
            _characterController = GetComponent<CharacterController>();
            _colliderHeight = GetComponent<CapsuleCollider>().height / 2f;

            FindObjectOfType<CinemachineFreeLook>().Follow = renderer.transform;
            FindObjectOfType<CinemachineFreeLook>().LookAt = renderer.transform;

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

    bool OnGround()
    {
        RaycastHit[] hit = Physics.RaycastAll(transform.position, Vector3.down, _colliderHeight + 0.1f);

        return hit.Length > 0;
    }

    void FindPlatform()
    {
        RaycastHit[] hit = Physics.RaycastAll(transform.position, Vector3.down, _colliderHeight + 3.2f);

        if (hit.Length > 0)
        {
            if (hit[0].collider.gameObject.TryGetComponent<NetworkObject>(out _platform))
            {
                Debug.Log("OKAY");
            }
            else
            {
                Debug.DrawLine(transform.position, transform.position + Vector3.down * (_colliderHeight + 3.2f), Color.red, 2f);
                Debug.Log("NO");
            }
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.down * (_colliderHeight + 3.2f), Color.red, 2f);
            Debug.Log("NO");
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
        if (OnGround())
        {
            _jumpInput = true;
        }
    }

    void OnInteractionInput()
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
            GUILayout.Label($"{_verticalSpeed * Time.deltaTime}\t");
            GUILayout.EndArea();
        }
    }
}
