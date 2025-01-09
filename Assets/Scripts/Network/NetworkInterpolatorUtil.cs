using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보간용으로 생성된 Visual 오브젝트의 보간을 수행하는 Class
/// </summary>
public class NetworkInterpolatorUtil : MonoBehaviour
{
    private const float LOCAL_LERP_SPEED = 16f;
    private const float OTHER_LERP_SPEED = 8f;

    private Transform _target;

    private float _lerpSpeed = LOCAL_LERP_SPEED;

    private bool _isParenting = false;
    private float _parentingCooldown = 0f;

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

    public void StartParenting(float cooldown)
    {
        _isParenting = true;
        _parentingCooldown = cooldown;
    }
}
