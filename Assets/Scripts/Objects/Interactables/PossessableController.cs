using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PossessableController : PlayerDependantBehaviour, IInteractable
{
    [SerializeField] private ColorType _possessableColor;
    public ColorType PossessableColor
    {
        get => _possessableColor;
        set => _possessableColor = value;
    }

    [SerializeField] private Material[] _materials;

    private Rigidbody _rigidbody;
    private BoxCollider _boxCollider;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private NetworkSyncTransform _networkSyncTransform;
    private NetworkInterpolator _networkInterpolator;

    private PlayerController _interactingPlayer;
    private MeshFilter _interactingMeshFilter;
    private MeshRenderer _interactingMeshRenderer;
    private CharacterController _characterController;

    private Rigidbody _platform;
    private Mesh _originalMesh;
    private Material _originalMaterial;

    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();

        _networkSyncTransform = GetComponent<NetworkSyncTransform>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();

        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _meshFilter = _networkInterpolator.VisualReference.GetComponent<MeshFilter>();
            _meshRenderer = _networkInterpolator.VisualReference.GetComponent<MeshRenderer>();

            _outline = _networkInterpolator.VisualReference.GetComponent<Outline>();
            _outline.enabled = false;

            _meshRenderer.material = _materials[(int)_possessableColor - 1];
        });
    }

    public override void OnPlayerInitialized()
    {
        if (_possessableColor == PlayerController.LocalPlayer.PlayerColor && IsClient)
        {
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
        }
        else
        {
            _rigidbody.isKinematic = true;
        }
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (_interactingPlayer)
        {
            transform.position = _interactingPlayer.transform.position;
            transform.rotation = _interactingPlayer.transform.rotation;
        }
        else
        {
            HandlePlatform();
        }
    }

    private void HandlePlatform()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 4f))
        {
            if (_platform?.gameObject != hit.collider.gameObject)
            {
                // 새로운 플랫폼을 발견한 경우
                if (hit.collider.gameObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject) &&
                    hit.collider.gameObject.TryGetComponent<Rigidbody>(out _platform))
                {
                    _networkSyncTransform.SetParent(networkObject.gameObject);
                }
                else if (_platform != null)
                {
                    _networkSyncTransform.SetParent(null);
                    _platform = null;
                }
            }
        }
        else if (_platform != null)
        {
            _networkSyncTransform.SetParent(null);
            _platform = null;
        }
    }

    private bool CheckDispossessionPosition()
    {
        Vector3 origin = _characterController.transform.position;
        origin.y -= _boxCollider.size.y;
        origin.y += PlayerController.INITIAL_CAPSULE_HEIGHT;

        Vector3 verticalPad = Vector3.up * PlayerController.INITIAL_CAPSULE_HEIGHT / 2f;
        float radius = (_boxCollider.size.x / 2f + PlayerController.INITIAL_CAPSULE_RADIUS) * 1.2f;

        for (int i = 0; i < 9; i++)
        {
            Vector3 newPoint;

            newPoint = origin + Quaternion.Euler(0, i * 20, 0) * transform.forward * radius;

            if (Physics.OverlapCapsule(newPoint + verticalPad, newPoint - verticalPad, PlayerController.INITIAL_CAPSULE_RADIUS).Length == 0)
            {
                _characterController.enabled = false;
                _characterController.transform.position = newPoint;
                _characterController.enabled = true;

                return true;
            }

            newPoint = origin + Quaternion.Euler(0, -i * 20, 0) * transform.forward * radius;

            if (Physics.OverlapCapsule(newPoint + verticalPad, newPoint - verticalPad, PlayerController.INITIAL_CAPSULE_RADIUS).Length == 0)
            {
                _characterController.enabled = false;
                _characterController.transform.position = newPoint;
                _characterController.enabled = true;

                return true;
            }
        }

        return false;
    }

    public bool IsInteractable(PlayerController player)
    {
        return _possessableColor == player.PlayerColor;
    }

    public bool StartInteraction(PlayerController player)
    {
        _interactingPlayer = player;
        _characterController = player.GetComponent<CharacterController>();

        StartPossession(player);

        if (IsServer)
        {
            StartPossesionClientRpc(player.gameObject);
        }
        else
        {
            StartPossessionServerRpc(player.gameObject);
        }

        _characterController.enabled = false;
        _characterController.transform.position = transform.position;
        _characterController.transform.rotation = transform.rotation;
        _characterController.enabled = true;

        return true;
    }

    public bool StopInteraction(PlayerController player)
    {
        if (CheckDispossessionPosition())
        {
            StopPossession(player);

            if (IsServer)
            {
                StopPossesionClientRpc(player.gameObject);
            }
            else
            {
                StopPossessionServerRpc(player.gameObject);
            }

            _interactingPlayer = null;
            _characterController = null;

            return true;
        }
        else
        {
            return false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestOwnershipServerRpc(ulong clientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }

    private void StartPossession(PlayerController player)
    {
        NetworkInterpolator playerInterpolator = player.GetComponent<NetworkInterpolator>();

        _interactingMeshFilter = playerInterpolator.VisualReference.GetComponent<MeshFilter>();
        _interactingMeshRenderer = playerInterpolator.VisualReference.GetComponent<MeshRenderer>();

        Debug.Log($"{_interactingMeshFilter == null}");

        _originalMesh = _interactingMeshFilter.sharedMesh;
        _originalMaterial = _interactingMeshRenderer.material;

        _interactingMeshFilter.mesh = _meshFilter.sharedMesh;
        _interactingMeshRenderer.material = _meshRenderer.material;

        _rigidbody.isKinematic = true;
        _boxCollider.enabled = false;
        _meshRenderer.enabled = false;

        player.UpdateCollider(_boxCollider);
    }

    private void StopPossession(PlayerController player)
    {
        _interactingMeshFilter.mesh = _originalMesh;
        _interactingMeshRenderer.material = _originalMaterial;

        _interactingMeshFilter = null;
        _interactingMeshRenderer = null;

        _originalMesh = null;
        _originalMaterial = null;

        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
        _boxCollider.enabled = true;
        _meshRenderer.enabled = true;

        player.UpdateCollider(null);
    }

    [ServerRpc]
    public void StartPossessionServerRpc(NetworkObjectReference player)
    {
        if (player.TryGet(out NetworkObject networkObject))
        {
            StartPossession(networkObject.GetComponent<PlayerController>());
        }
    }

    [ClientRpc]
    public void StartPossesionClientRpc(NetworkObjectReference player)
    {
        if (IsServer)
        {
            return;
        }

        if (player.TryGet(out NetworkObject networkObject))
        {
            StartPossession(networkObject.GetComponent<PlayerController>());
        }
    }

    [ServerRpc]
    public void StopPossessionServerRpc(NetworkObjectReference player)
    {
        if (player.TryGet(out NetworkObject networkObject))
        {
            StopPossession(networkObject.GetComponent<PlayerController>());
        }
    }

    [ClientRpc]
    public void StopPossesionClientRpc(NetworkObjectReference player)
    {
        if (IsServer)
        {
            return;
        }

        if (player.TryGet(out NetworkObject networkObject))
        {
            StopPossession(networkObject.GetComponent<PlayerController>());
        }
    }
}
