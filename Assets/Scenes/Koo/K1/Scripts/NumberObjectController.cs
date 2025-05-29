using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NumberObjectController : NetworkBehaviour, IInteractable
{
    private const float DISTANCE_FROM_PLAYER = 64f;         // 플레이어와 큐브 사이의 거리
    private const float MAXIMUM_DISTANCE_FROM_PLAYER = 70f; // 플레이어와 큐브가 멀어질 수 있는 최대 거리
    private const float CUBE_SPEED = 64f;                   // 큐브의 이동 속력
    
    private Rigidbody _rigidbody;

    private bool _isTaken;
    
    private PlayerController _interactingPlayer;
    
    private NetworkInterpolator _networkInterpolator;
    
    private ColorType _color;
    
    public ColorType Color
    {
        get => _color;
        set => _color = value;
    }

    private ColorType _playerColor;
    
    private int _viewMode;

    public int ViewMode
    {
        get => _viewMode;
        set => _viewMode = value;
    }

    public void Initialize()
    {
        Debug.Log("Initialize-Start");

        InitializeClientRpc(transform.position, transform.rotation, transform.localScale);
        
        Debug.Log("Initialize-end");
    }
    
    public bool IsTaken
    {
        get => _isTaken;
    }
    
    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }
    
    public override void OnNetworkSpawn()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();
        
        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _outline = _networkInterpolator.VisualReference.GetComponent<Outline>();
            _outline.enabled = false;
        });
    }
    
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (_interactingPlayer)
        {
            Vector3 target = Camera.main.transform.position + Camera.main.transform.forward * DISTANCE_FROM_PLAYER;

            Vector3 direction = (target - transform.position).normalized;
            float magnitude = Mathf.Clamp(Mathf.Pow((target - transform.position).magnitude * CUBE_SPEED, 2), 0, 128f);


            if (_rigidbody.SweepTest(direction, out RaycastHit hit, magnitude * Time.deltaTime))
            {
                magnitude /= 4f;
            }

            _rigidbody.velocity = direction * magnitude;

            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, Quaternion.LookRotation(transform.position - Camera.main.transform.position), Time.deltaTime * 16f));

            if (Vector3.Distance(transform.position, _interactingPlayer.transform.position) > MAXIMUM_DISTANCE_FROM_PLAYER)
            {
                //ForceStopInteraction();
            }
        }
    }

    public bool IsInteractable(PlayerController player)
    {
        return !IsTaken;
    }

    public bool StartInteraction(PlayerController player)
    {
        _interactingPlayer = player;
        _rigidbody.useGravity = false;

        SetTakenServerRpc(true);
        
        if(!IsOwner)
        {
            RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        return true;
    }

    public bool StopInteraction(PlayerController player)
    {
        _interactingPlayer = null;
        _rigidbody.useGravity = true;

        SetTakenServerRpc(false);

        return true;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ulong clientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetTakenServerRpc(bool isTaken)
    {
        SetTakenClientRpc(isTaken);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetTakenClientRpc(bool isTaken)
    {
        _isTaken = isTaken;
    }
    
    [ClientRpc(RequireOwnership = false)]
    private void InitializeClientRpc(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Debug.Log("InitializeClientRpc-Start");
        
        _rigidbody.MovePosition(position);
        _rigidbody.MoveRotation(rotation);
        transform.localScale = scale;
        
        _playerColor = NetworkManager.Singleton.IsHost ? ColorType.Blue : ColorType.Red;
        
        Material[] materials = GetComponent<Renderer>().materials;
        materials[1].SetMaterial(ColorType.Red, _playerColor, 1);
        GetComponent<Renderer>().materials = materials;
        
        Debug.Log("InitializeClientRpc-End");
    }
}
