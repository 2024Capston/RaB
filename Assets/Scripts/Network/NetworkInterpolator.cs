using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkInterpolator : NetworkBehaviour
{
    [SerializeField] private bool _alwaysLocal = false;

    private GameObject _visualReference;
    public GameObject VisualReference
    {
        get => _visualReference;
    }

    private Action _VisualReferenceCreated;
    public Action VisualReferenceCreated
    {
        get => _VisualReferenceCreated;
        set => _VisualReferenceCreated = value;
    }

    public override void OnNetworkSpawn()
    {
        _visualReference = new GameObject(gameObject.name + " (Visual)");

        _visualReference.transform.position = transform.position;
        _visualReference.transform.rotation = transform.rotation;
        _visualReference.transform.localScale = transform.localScale;

        _visualReference.AddComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
        _visualReference.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<MeshRenderer>());

        if (TryGetComponent<Outline>(out Outline outline))
        {
            Outline newOutline = _visualReference.AddComponent<Outline>();

            newOutline.OutlineMode = outline.OutlineMode;
            newOutline.OutlineColor = outline.OutlineColor;
            newOutline.OutlineWidth = outline.OutlineWidth;

            Destroy(outline);
        }

        _visualReference.AddComponent<NetworkInterpolatorUtil>().SetTarget(transform, _alwaysLocal | IsOwner);

        _VisualReferenceCreated?.Invoke();
    }
}
