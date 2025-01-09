using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보간용으로 생성된 Visual 오브젝트의 보간을 수행하는 Class
/// </summary>
public class NetworkInterpolatorUtil : MonoBehaviour
{
    // Local 수준으로 빠르게 보간하거나, Remote 수준으로 느리게 보간할 수 있다
    private const float LOCAL_LERP_SPEED = 16f;
    private const float OTHER_LERP_SPEED = 8f;

    private Transform _target;

    private float _lerpSpeed = LOCAL_LERP_SPEED;    // 현재 보간 속력

    private bool _isParenting = false;              // Parenting 보간 중인지 여부
    private float _parentingCooldown = 0f;          // Parenting 보간에 쓰일 시간

    void Update()
    {
        if (_target)
        {
            if (_isParenting)
            {
                transform.position = Vector3.MoveTowards(transform.position, _target.position, Time.deltaTime * _lerpSpeed);

                _parentingCooldown -= Time.deltaTime;
                if (_parentingCooldown < 0f)
                {
                    _parentingCooldown = 0f;
                    _isParenting = false;
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * _lerpSpeed);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, _target.rotation, Time.deltaTime * _lerpSpeed);
        }
    }

    /// <summary>
    /// 보간 목표를 설정한다.
    /// </summary>
    /// <param name="target">목표</param>
    /// <param name="isLocal">보간 속력</param>
    public void SetTarget(Transform target, bool isLocal)
    {
        _target = target;

        if (isLocal)
        {
            _lerpSpeed = LOCAL_LERP_SPEED;
        }
        else
        {
            _lerpSpeed = OTHER_LERP_SPEED;
        }
    }

    /// <summary>
    /// 보간 속력을 갱신한다.
    /// </summary>
    /// <param name="isLocal">보간 속력</param>
    public void ChangeLerpSpeed(bool isLocal)
    {
        if (isLocal)
        {
            _lerpSpeed = LOCAL_LERP_SPEED;
        }
        else
        {
            _lerpSpeed = OTHER_LERP_SPEED;
        }
    }

    /// <summary>
    /// 일정 시간 동안 Parenting 보간을 시작한다.
    /// </summary>
    /// <param name="cooldown">보간 시간</param>
    public void StartParenting(float cooldown)
    {
        _isParenting = true;
        _parentingCooldown = cooldown;
    }
}
