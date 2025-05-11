using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PingManager : MonoBehaviour
{
    public Camera _mainCamera;
    public GameObject[] _pings;
    public AudioClip[] _pingAudios;

    private int _selectedPing = 0;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("v"))   //인풋매니저에 delegate로 추가
        {
            GetPingPositionAndRotation();
        }         
    }

    void GetPingPositionAndRotation()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //if(hit.collider.gameObject.layer == 8)
            //{
            Vector3 offset = new Vector3(hit.point.x, hit.point.y, hit.point.z) + hit.normal * 0.1f;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            //}

            Debug.Log($"핑충돌! 각도{hit.normal}");
            RequestSpawnPingServerRpc(offset, rotation);
        }

    }
    [ServerRpc(RequireOwnership =false)]
    void RequestSpawnPingServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject pingObject = Instantiate(_pings[_selectedPing], position, rotation);
        pingObject.GetComponent<NetworkObject>().Spawn();
    }
    
}
