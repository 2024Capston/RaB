using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRenderer : NetworkBehaviour
{
    [SerializeField] private GameObject[] _playerRenderPrefab;

    private MeshFilter _visualMeshFilter;
    private MeshRenderer _visualMeshRenderer;

    private PlayerController _playerController;
    private NetworkInterpolator _networkInterpolator;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();
    }

    public void Initialize()
    {
        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            if (!_visualMeshFilter)
            {
                _visualMeshFilter = _networkInterpolator.VisualReference.AddComponent<MeshFilter>();
                _visualMeshRenderer = _networkInterpolator.VisualReference.AddComponent<MeshRenderer>();
            }

            int colorIndex = (int)_playerController.PlayerColor - 1;

            _visualMeshFilter.sharedMesh = _playerRenderPrefab[colorIndex].GetComponent<MeshFilter>().sharedMesh;
            _visualMeshRenderer.material = _playerRenderPrefab[colorIndex].GetComponent<MeshRenderer>().sharedMaterial;
        });
    }

    [ServerRpc]
    public void ShowPlayerRenderServerRpc()
    {
        ShowPlayerRenderClientRpc();
    }

    [ClientRpc]
    public void ShowPlayerRenderClientRpc()
    {
        _visualMeshRenderer.enabled = true;
    }

    [ServerRpc]
    public void HidePlayerRenderServerRpc()
    {
        HidePlayerRenderClientRpc();
    }

    [ClientRpc]
    public void HidePlayerRenderClientRpc()
    {
        _visualMeshRenderer.enabled = false;
    }
}
