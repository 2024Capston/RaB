using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 플레이어를 렌더링하는 Class
/// </summary>
public class PlayerRenderer : NetworkBehaviour
{
    /// <summary>
    /// 렌더링에 사용할 플레이어 프리팹. (파랑, 빨강 순)
    /// </summary>
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
            _visualMeshFilter = _networkInterpolator.VisualReference.AddComponent<MeshFilter>();
            _visualMeshRenderer = _networkInterpolator.VisualReference.AddComponent<MeshRenderer>();

            int colorIndex = (int)_playerController.PlayerColor - 1;

            _visualMeshFilter.sharedMesh = _playerRenderPrefab[colorIndex].GetComponent<MeshFilter>().sharedMesh;
            _visualMeshRenderer.material = _playerRenderPrefab[colorIndex].GetComponent<MeshRenderer>().sharedMaterial;
        });
    }

    /// <summary>
    /// 플레이어 모습을 표시한다.
    /// </summary>
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

    /// <summary>
    /// 플레이어 모습을 숨긴다.
    /// </summary>
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
