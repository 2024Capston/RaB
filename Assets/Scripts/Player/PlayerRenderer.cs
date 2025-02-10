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

    private PlayerController _playerController;
    private NetworkInterpolator _networkInterpolator;

    private GameObject _playerRender;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();
    }

    public void Initialize()
    {
        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            int colorIndex = (int)_playerController.PlayerColor - 1;

            _playerRender = Instantiate(_playerRenderPrefab[colorIndex]);
            _playerRender.transform.SetParent(_networkInterpolator.VisualReference.transform);

            _playerRender.transform.localPosition = Vector3.zero;
            _playerRender.transform.localRotation = Quaternion.identity;
            _playerRender.transform.localScale = Vector3.one;
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
        _playerRender.SetActive(true);
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
        _playerRender.SetActive(false);
    }
}
