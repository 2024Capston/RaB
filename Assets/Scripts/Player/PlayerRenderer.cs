using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRenderer : PlayerDependantBehaviour
{
    [SerializeField] private GameObject[] _playerRenderPrefab;

    private PlayerController _playerController;
    private NetworkInterpolator _networkInterpolator;

    public override void OnLocalInitialized()
    {
        _playerController = GetComponent<PlayerController>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();
    }
}
