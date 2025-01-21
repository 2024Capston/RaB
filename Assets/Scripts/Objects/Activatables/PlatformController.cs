using Unity.Netcode;
using UnityEngine;

public class PlatformController : NetworkBehaviour, IActivatable
{
    [SerializeField] private Transform[] _targets;
    [SerializeField] private float _moveSpeed;

    private Rigidbody _rigidbody;

    private bool _isActive;
    private int _currentTarget = 0;

    public override void OnNetworkSpawn()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!_isActive)
        {
            return;
        }

        _rigidbody.MovePosition(Vector3.Lerp(transform.position, _targets[_currentTarget].position, Time.fixedDeltaTime * _moveSpeed));
        _rigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, _targets[_currentTarget].rotation, Time.fixedDeltaTime * _moveSpeed));

        if (Vector3.Distance(transform.position, _targets[_currentTarget].position) < 0.1f)
        {
            _currentTarget = (_currentTarget + 1) % _targets.Length;

            if (IsServer)
            {
                UpdatePlatformPositionClientRpc(transform.position, transform.rotation, _currentTarget);
            }
        }
    }

    public bool IsActivatable(GameObject activator = null)
    {
        return true;
    }

    public bool Activate(GameObject activator = null)
    {
        SetActiveServerRpc(true);

        return true;
    }

    public bool Deactivate(GameObject activator = null)
    {
        SetActiveServerRpc(false);

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActiveServerRpc(bool isActive)
    {
        SetActiveClientRpc(isActive);
    }

    [ClientRpc]
    private void SetActiveClientRpc(bool isActive)
    {
        _isActive = isActive;
    }

    [ClientRpc]
    private void UpdatePlatformPositionClientRpc(Vector3 position, Quaternion rotation, int currentTarget)
    {
        _rigidbody.MovePosition(position);
        _rigidbody.MoveRotation(rotation);

        _currentTarget = currentTarget;
    }
}
