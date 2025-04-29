using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FloorController : MonoBehaviour
{
    private ColorType _playerColor;
    
    // Start is called before the first frame update
    void Start()
    {
        _playerColor = NetworkManager.Singleton.IsHost ? ColorType.Blue : ColorType.Red;
        
        Material[] materials = GetComponent<Renderer>().materials;
        
        materials[1].SetMaterial(ColorType.Red, _playerColor, 1);
        materials[2].SetMaterial(ColorType.Blue, _playerColor, 1);
        materials[3].SetMaterial(ColorType.Purple, _playerColor, 1);

        GetComponent<Renderer>().materials = materials;
    }
}
