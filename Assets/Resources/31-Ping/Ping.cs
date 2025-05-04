using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ping : MonoBehaviour
{
    [SerializeField] private float timer = 5f;
    // Start is called before the first frame update
    void Start()
    {
        timer = 5f;
        StartCoroutine(DestroySelf(timer));
    }
    IEnumerator DestroySelf(float timer)
    {
        yield return new WaitForSeconds(timer);
        GetComponent<NetworkObject>().Despawn(); // 네트워크에서 제거
        Destroy(this); // 로컬에서 제거
        yield break;
    }
}
