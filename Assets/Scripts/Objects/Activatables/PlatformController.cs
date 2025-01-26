using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 움직이는 플랫폼을 조작하는 Class
/// </summary>
public class PlatformController : NetworkBehaviour, IActivatable
{
    /// <summary>
    /// 목표 위치 (오름차순으로 순서대로 이동)
    /// </summary>
    [SerializeField] private Transform[] _targets;

    /// <summary>
    /// 이동 속력
    /// </summary>
    [SerializeField] private float _moveSpeed;

    private Rigidbody _rigidbody;

    private bool _isActive;
    private float _timer;

    private int _previousTarget = 0;    // 이전 목표 위치
    private int _currentTarget = 1;     // 현재 목표 위치
    private float _targetMoveTime;      // 이동에 걸릴 시간

    private void OnEnable()
    {
        if (_targets.Length < 2)
        {
            Logger.Log("There must be at least two targets for a platform.");
        }
    }

    public override void OnNetworkSpawn()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _targetMoveTime = (_targets[_currentTarget].position - _targets[_previousTarget].position).magnitude / _moveSpeed;
    }

    void FixedUpdate()
    {
        if (!_isActive)
        {
            return;
        }

        _timer += Time.fixedDeltaTime;
        float lerpCoefficient = EaseInOut(_timer / _targetMoveTime);

        _rigidbody.MovePosition(Vector3.Lerp(_targets[_previousTarget].position, _targets[_currentTarget].position, lerpCoefficient));
        _rigidbody.MoveRotation(Quaternion.Slerp(_targets[_previousTarget].rotation, _targets[_currentTarget].rotation, lerpCoefficient));

        // 목표에 도달했으면 목표 위치 갱신
        if (Vector3.Distance(transform.position, _targets[_currentTarget].position) < 0.1f)
        {
            _timer = 0f;

            _previousTarget = _currentTarget;
            _currentTarget = (_currentTarget + 1) % _targets.Length;

            _targetMoveTime = (_targets[_currentTarget].position - _targets[_previousTarget].position).magnitude / _moveSpeed;

            // 서버 측에서 클라이언트 측으로 갱신 알림
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

    private float EaseInOut(float timer) {
        float sine = Mathf.Sin(Mathf.PI * timer / 2f);
        return sine * sine;
    }

    /// <summary>
    /// 활성화 여부를 설정한다.
    /// </summary>
    /// <param name="isActive">활성화 여부</param>
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

    /// <summary>
    /// 서버 측에서 클라이언트 측으로 갱신 알림을 보낸다.
    /// </summary>
    /// <param name="position">갱신 위치</param>
    /// <param name="rotation">갱신 회전</param>
    /// <param name="currentTarget">갱신 목표 위치</param>
    [ClientRpc]
    private void UpdatePlatformPositionClientRpc(Vector3 position, Quaternion rotation, int currentTarget)
    {
        if (IsServer)
        {
            return;
        }

        _timer = 0f;

        _rigidbody.MovePosition(position);
        _rigidbody.MoveRotation(rotation);

        _currentTarget = currentTarget;

        if (_currentTarget == 0)
        {
            _previousTarget = _targets.Length - 1;
        }
        else
        {
            _previousTarget = _currentTarget - 1;
        }

        _targetMoveTime = (_targets[_currentTarget].position - _targets[_previousTarget].position).magnitude / _moveSpeed;
    }
}
