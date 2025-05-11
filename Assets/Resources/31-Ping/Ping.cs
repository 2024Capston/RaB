using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Ping : NetworkBehaviour
{
    [SerializeField] AudioClip _pingAudio;
    [SerializeField] private float timer = 5f;

    private Transform[] _piecesTransforms;
    [SerializeField] private MeshRenderer[] _childMeshRenderers;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        AudioSource.PlayClipAtPoint(_pingAudio, transform.position);

        Initialize();
        if(IsHost)
            StartCoroutine(DestroyMe(timer));
    }

    IEnumerator DestroyMe(float timer)
    {
        yield return new WaitForSeconds(timer);
        OnDestroySelf();
        yield break;
    }
    private void OnDestroySelf()
    {
        GetComponent<NetworkObject>().Despawn(); // 네트워크에서 제거
    }

    private void Initialize()
    {
        ColorType _playerColor = NetworkManager.Singleton.IsHost ? ColorType.Blue : ColorType.Red;

        int childCount = _childMeshRenderers.Length;

        for (int i = 0; i < childCount; i++)
        {
            _childMeshRenderers[i].material.SetMaterial(_playerColor, _playerColor, 0);     //viewMode 0:서로색보임 1:상대흰색 2:상대투명
        }
    }
}
