using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ElevatorArrowController : NetworkBehaviour
{
    [SerializeField] private List<Material> _materials;
    [SerializeField] private List<MeshRenderer> _meshRenderers;

    private NetworkVariable<int> _arrowValue = new NetworkVariable<int>();

    public int ArrowValue
    {
        get => _arrowValue.Value;
        set => _arrowValue.Value = value;
    }

    private void Awake()
    {
        _arrowValue.OnValueChanged += OnValueChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
    }

    private void OnValueChanged(int previousValue, int newValue)
    {
        if (newValue == -1)
        {
            _meshRenderers[0].material = _materials[1];
            _meshRenderers[1].material = _materials[0];
        }
        else if (newValue == 1)
        {
            _meshRenderers[0].material = _materials[0];
            _meshRenderers[1].material = _materials[1];
        }
        else
        {
            _meshRenderers[0].material = _materials[0];
            _meshRenderers[1].material = _materials[0];
        }
    }
}
