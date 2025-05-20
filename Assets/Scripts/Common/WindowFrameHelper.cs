using UnityEditor;
using UnityEngine;

public class WindowFrameHelper : MonoBehaviour
{
    [SerializeField] float _defaultScale = 1f;
    [SerializeField] bool _exchangeYZ;

    [SerializeField] private Transform[] _center;
    [SerializeField] private Transform _top;
    [SerializeField] private Transform _bottom;
    [SerializeField] private Transform _left;
    [SerializeField] private Transform _right;

    public void UpdateTransform()
    {
        _top.localScale = new Vector3(_defaultScale * 2f, 1f / transform.localScale.y, 2f / transform.localScale.z);
        _top.position = transform.position + transform.rotation * Vector3.up * (_defaultScale * transform.localScale.y - 0.5f);

        _bottom.localScale = new Vector3(_defaultScale * 2f, 1f / transform.localScale.y, 2f / transform.localScale.z);
        _bottom.position = transform.position - transform.rotation * Vector3.up * (_defaultScale * transform.localScale.y - 0.5f);

        _left.localScale = new Vector3(1f / transform.localScale.x, _defaultScale * 2f, 2f / transform.localScale.z);
        _left.position = transform.position - transform.rotation * Vector3.right * (_defaultScale * transform.localScale.x - 0.5f);

        _right.localScale = new Vector3(1f / transform.localScale.x, _defaultScale * 2f, 2f / transform.localScale.z);
        _right.position = transform.position + transform.rotation * Vector3.right * (_defaultScale * transform.localScale.x - 0.5f);
    }
}
