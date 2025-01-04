using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkInterpolator : MonoBehaviour
{
    private const float OWNER_LERP_SPEED = 32f;
    private const float OTHER_LERP_SPEED = 16f;

    private Transform _target;
    private float _lerpSpeed = OWNER_LERP_SPEED;

    void Update()
    {
        if (_target != null)
        {
            transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * _lerpSpeed);
        }
    }

    public void SetTarget(Transform target, bool isOwner)
    {
        _target = target;

        transform.position = _target.position;
        transform.rotation = _target.rotation;

        if (isOwner)
        {
            _lerpSpeed = OWNER_LERP_SPEED;
        }
        else
        {
            _lerpSpeed = OTHER_LERP_SPEED;
        }
    }
}
