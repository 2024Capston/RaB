using System;
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

    private void Awake()
    {
        _playerColor = NetworkManager.Singleton.IsHost ? ColorType.Blue : ColorType.Red;
    }
    
    public void UpdateColorInfo(int viewType, int floor)
    {
        Material[] materials = GetComponent<Renderer>().materials;
        materials[0].SetMaterial(ColorType.None, _playerColor, viewType);
        materials[1].SetMaterial(ColorType.Blue, _playerColor, viewType);
        materials[2].SetMaterial(ColorType.Purple, _playerColor, viewType);
        materials[3].SetMaterial(ColorType.Red, _playerColor, viewType);
        GetComponent<Renderer>().materials = materials;
        
        GetComponentInChildren<TMP_Text>().text = floor + "F";
    }
}
