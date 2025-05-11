using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ping : MonoBehaviour
{
    [SerializeField] private float timer = 5f;

    private Transform[] _piecesTransforms;
    [SerializeField] private MeshRenderer[] _childMeshRenderers;

    // Start is called before the first frame update
    void Start()
    {
        //timer = 3.5f;
        StartCoroutine(DestroySelf(timer));
        Initialize();
    }
    IEnumerator DestroySelf(float timer)
    {
        yield return new WaitForSeconds(timer);
        GetComponent<NetworkObject>().Despawn(); // 네트워크에서 제거
        Destroy(this); // 로컬에서 제거
        yield break;
    }
    public void Initialize()
    {
        ColorType _playerColor = NetworkManager.Singleton.IsHost ? ColorType.Blue : ColorType.Red;

        int childCount = _childMeshRenderers.Length;

        for (int i = 0; i < childCount; i++)
        {
            _childMeshRenderers[i].material.SetMaterial(_playerColor, _playerColor, 0);     //viewMode 0:서로색보임 1:상대흰색 2:상대투명
        }
    }
}
