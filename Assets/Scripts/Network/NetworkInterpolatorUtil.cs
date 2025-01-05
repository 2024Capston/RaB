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

    void Update()
    {
        if (_target != null)
        {
            transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * _lerpSpeed);
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
}
