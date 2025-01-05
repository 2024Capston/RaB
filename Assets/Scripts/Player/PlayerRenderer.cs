using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRenderer : LocalDependantBehaviour
{
    [SerializeField] GameObject[] _playerRenderPrefab;

    public override void OnLocalInitialized()
    {
        // 플레이어 색상 가져오기
        // 플레이어 색상에 따라 처리
    }
}
