using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class CubeRenderer : NetworkBehaviour
{
    [SerializeField] Material[] _materials;

    private CubeController _cubeController;
    private NetworkInterpolator _networkInterpolator;

    private Transform[] _piecesTransforms;
    private MeshRenderer[] _piecesMeshRenderers;

    private void Start()
    {
        _cubeController = GetComponent<CubeController>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();

        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            int childCount = _networkInterpolator.VisualReference.transform.childCount;
            _piecesTransforms = new Transform[childCount];
            _piecesMeshRenderers = new MeshRenderer[childCount];

            for (int i = 0; i < childCount; i++)
            {
                Transform child = _networkInterpolator.VisualReference.transform.GetChild(i);

                _piecesTransforms[i] = child.transform;
                _piecesMeshRenderers[i] = child.GetComponent<MeshRenderer>();

                Material[] materials = _piecesMeshRenderers[i].materials;
                materials[1] = _materials[(int)_cubeController.CubeColor - 1];
                _piecesMeshRenderers[i].materials = materials;
            }
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateColorServerRpc()
    {
        UpdateColorClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    private void UpdateColorClientRpc()
    {
        int childCount = _piecesTransforms.Length;

        for (int i = 0; i < childCount; i++)
        {
            Material[] materials = _piecesMeshRenderers[i].materials;
            materials[1] = _materials[(int)_cubeController.CubeColor - 1];
            _piecesMeshRenderers[i].materials = materials;
        }
    }

    /// <summary>
    /// 큐브의 렌더링 색깔을 갱신한다.
    /// </summary>
    public void UpdateColor()
    {
        UpdateColorServerRpc();
    }
}
