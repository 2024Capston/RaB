using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ElevatorSegmentController : NetworkBehaviour
{
    private readonly int[][] Segment_Data =
    {
        new int[]{1, 0, 1, 1, 0, 1, 1, 1, 1, 1},
        new int[]{1, 0, 0, 0, 1, 1, 1, 0, 1, 1},
        new int[]{1, 1, 1, 1, 1, 0, 0, 1, 1, 1},
        new int[]{0, 0, 1, 1, 1, 1, 1, 0, 1, 1},
        new int[]{1, 0, 1, 0, 0, 0, 1, 0, 1, 0},
        new int[]{1, 1, 0, 1, 1, 1, 1, 1, 1, 1},
        new int[]{1, 0, 1, 1, 0, 1, 1, 0, 1, 1}
    };
    
    [SerializeField] private List<Material> _materials;
    [SerializeField] private List<MeshRenderer> _segments;

    private NetworkVariable<int> _segmentValue = new NetworkVariable<int>();

    public int SegmentValue
    {
        get => _segmentValue.Value;
        set => _segmentValue.Value = value;
    }

    private void Awake()
    {
        _segmentValue.OnValueChanged += OnValueChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
    }

    private void OnValueChanged(int previousValue, int newValue)
    {
        SetSegment(newValue);
    }

    private void SetSegment(int value)
    {
        for (int i = 0; i < 7; i++)
        {
            _segments[i].material = _materials[Segment_Data[i][value]];
        }
    }
}
