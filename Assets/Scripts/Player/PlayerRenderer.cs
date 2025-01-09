using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] private GameObject[] _playerRenderPrefab;

    private GameObject _renderObject;

    private PlayerController _playerController;
    private NetworkInterpolator _networkInterpolator;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();
    }

    public void Initialize()
    {
        if (_networkInterpolator.VisualReference)
        {
            ShowPlayerRender();
        }
        else
        {
            _networkInterpolator.VisualReferenceCreated += () =>
            {
                ShowPlayerRender();
            };
        }
    }

    private void ShowPlayerRender()
    {
        if (_renderObject)
        {
            Destroy(_renderObject);
        }
        int colorIndex = (int)_playerController.PlayerColor - 1;

        _renderObject = Instantiate(_playerRenderPrefab[colorIndex], _networkInterpolator.VisualReference.transform);
        _renderObject.transform.localPosition = Vector3.zero;
        _renderObject.transform.localRotation = Quaternion.identity;
    }
}
