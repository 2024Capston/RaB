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
    [SerializeField] private List<Material> _segments;
    
    private MeshRenderer _meshRenderer;

    private int _segmentValue;

    public int SegmentValue
    {
        get => _segmentValue;
    }
    
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _meshRenderer = GetComponent<MeshRenderer>();
    }
    
    
}
