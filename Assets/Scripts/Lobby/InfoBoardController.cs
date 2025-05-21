using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class InfoBoardController : MonoBehaviour
{
    private ColorType _color;

    private ColorType _playerColor;
    
    private int _viewMode;

    public void Start()
    {
        _playerColor = NetworkManager.Singleton.IsHost ? ColorType.Blue : ColorType.Red;

        Material[] materials = GetComponent<Renderer>().materials;
        materials[1].SetMaterial(ColorType.Blue, _playerColor, 1);
        materials[2].SetMaterial(ColorType.Purple, _playerColor, 1);
        materials[3].SetMaterial(ColorType.Red, _playerColor, 1);
        GetComponent<Renderer>().materials = materials;
        
        GetComponentInChildren<TMP_Text>().text = "1F";
    }

    public void UpdateColorInfo(int viewType, int floor)
    {
        _playerColor = NetworkManager.Singleton.IsHost ? ColorType.Blue : ColorType.Red;

        Material[] materials = GetComponent<Renderer>().materials;
        materials[0].SetMaterial(ColorType.Blue, _playerColor, viewType);
        materials[4].SetMaterial(ColorType.Purple, _playerColor, viewType);
        materials[5].SetMaterial(ColorType.Red, _playerColor, viewType);
        GetComponent<Renderer>().materials = materials;
        
        GetComponentInChildren<TMP_Text>().text = floor + "F";
    }
}
