using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 위치 보간용으로 별도의 Visual 오브젝트를 생성하는 Class
/// </summary>
public class NetworkInterpolator : NetworkBehaviour
{
    /// <summary>
    /// 항상 Local 수준으로 보간할 지 여부
    /// </summary>
    [SerializeField] private bool _alwaysLocal = false;

    private NetworkInterpolatorUtil _networkInterpolatorUtil;

    /// <summary>
    /// 보간용 오브젝트
    /// </summary>
    private GameObject _visualReference;
    public GameObject VisualReference
    {
        get => _visualReference;
    }

    /// <summary>
    /// 보간용 오브젝트가 생성되었을 때 호출되는 delegate
    /// </summary>
    private Action _visualReferenceCreated;
    public Action VisualReferenceCreated
    {
        get => _visualReferenceCreated;
        set => _visualReferenceCreated = value;
    }

    public override void OnNetworkSpawn()
    {
        _visualReference = new GameObject(gameObject.name + " (Visual)");

        _visualReference.transform.position = transform.position;
        _visualReference.transform.rotation = transform.rotation;
        _visualReference.transform.localScale = transform.localScale;

        // 시각 정보를 복사한다.
        if (TryGetComponent<MeshFilter>(out MeshFilter meshFilter) && TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
        {
            _visualReference.AddComponent<MeshFilter>().mesh = meshFilter.mesh;
            _visualReference.AddComponent<MeshRenderer>().material = meshRenderer.material;
            Destroy(meshFilter);
            Destroy(meshRenderer);
        }

        // Outline 컴포넌트가 있다면 복사한다.
        if (TryGetComponent<Outline>(out Outline outline))
        {
            Outline newOutline = _visualReference.AddComponent<Outline>();

            newOutline.OutlineMode = outline.OutlineMode;
            newOutline.OutlineColor = outline.OutlineColor;
            newOutline.OutlineWidth = outline.OutlineWidth;

            Destroy(outline);
        }

        // Visual 오브젝트의 타겟을 지정한다.
        _networkInterpolatorUtil = _visualReference.AddComponent<NetworkInterpolatorUtil>();
        _networkInterpolatorUtil.SetTarget(transform, _alwaysLocal | IsOwner);

        _visualReferenceCreated?.Invoke();
    }

    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        _networkInterpolatorUtil.ChangeLerpSpeed(_alwaysLocal | IsOwner);
    }

    public void StartParenting(float parentingCooldown)
    {
        _networkInterpolatorUtil.StartParenting(parentingCooldown);
    }

    public void AddVisualReferenceDependantFunction(Action action)
    {
        if (_visualReference)
        {
            action.Invoke();
        }
        else
        {
            _visualReferenceCreated += action;
        }
    }
}
