using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkInterpolator : MonoBehaviour
{
    private Transform _target;

    void Update()
    {
        if (_target != null)
        {
            transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * 10f);
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
