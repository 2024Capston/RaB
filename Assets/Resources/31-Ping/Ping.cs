using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Ping : MonoBehaviour
{
    //[SerializeField] AudioClip _pingAudio;
    [SerializeField] private float _timer = 5f;

    private Transform[] _piecesTransforms;
    [SerializeField] private MeshRenderer[] _childMeshRenderers;
    
    public void SpawnPing(bool isHost)
    {
        //AudioSource.PlayClipAtPoint(_pingAudio, transform.position);
        ColorType playerColor = isHost ? ColorType.Blue : ColorType.Red;
        int childCount = _childMeshRenderers.Length;

        for (int i = 0; i < childCount; i++)
        {
            _childMeshRenderers[i].material.SetMaterial(playerColor, playerColor, 0);     //viewMode 0:서로색보임 1:상대흰색 2:상대투명
        }
        
        StartCoroutine(DestroyMe(_timer));
    }

    IEnumerator DestroyMe(float timer)
    {
        yield return new WaitForSeconds(timer);
        Destroy(gameObject);
    }

}
