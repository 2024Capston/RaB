using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ColorFloorController : MonoBehaviour
{
    private ColorType _playerColor;
    
    [SerializeField]
    private int _viewMode;

    [SerializeField]
    private ColorType _color;
    // Start is called before the first frame update
    void Start()
    {
        _playerColor = NetworkManager.Singleton.IsHost ? ColorType.Blue : ColorType.Red;
        
        Material[] materials = GetComponent<Renderer>().materials;
        materials[0].SetMaterial(_color, _playerColor, 1);
        GetComponent<Renderer>().materials = materials;
        
        if (PlayerController.LocalPlayer == null)
        {
            PlayerController.LocalPlayerCreated += () => {
                if (PlayerController.LocalPlayer.Color != _color)
                {
                    GetComponent<MeshCollider>().enabled = false;
                }
            };  
        }
        else
        {
            if (PlayerController.LocalPlayer.Color != _color)
            {
                GetComponent<MeshCollider>().enabled = false;
            }
        }
        
    }
}
