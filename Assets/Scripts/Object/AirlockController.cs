using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AirlockController : NetworkBehaviour
{
    [SerializeField] private DoorController _doorIn;
    [SerializeField] private DoorController _doorOut;

    /// <summary>
    /// 0 : off , 1 : Blue, 2 : Red Material
    /// </summary>
    [SerializeField] Material[] _inoutMaterials = new Material[3];
    
    /// <summary>
    /// 0 : blue In Mesh, 1 : blue Out Mesh
    /// </summary>
    [SerializeField] MeshRenderer[] _blueInOutMaterials = new MeshRenderer[2];
    
    /// <summary>
    /// 0 : red In Mesh, 1 : red Out Mesh
    /// </summary>
    [SerializeField] MeshRenderer[] _redInOutMaterials = new MeshRenderer[2];
    
    // 두 값이 true일 땐 DoorIn이 개방, 두 값이 false일 땐 DoorOut이 개방
    private bool _isBlueOpened = true;
    private bool _isRedOpened = true;
    
    
    private bool _isInOpened;
    /// <summary>
    /// true일 때 Lobby쪽 문이 열려있다.
    /// </summary>
    private bool IsInOpened
    {
        get => _isInOpened;
        set
        {
            _isInOpened = value;
            _doorIn.IsOpened = _isInOpened && IsOpened;
            _doorOut.IsOpened = !_isInOpened && IsOpened;
        }
    }

    private bool _isOpened;
    /// <summary>
    /// true일 때 스테이지가 해금 되어 해당 문에 접근할 수 있다.
    /// </summary>
    public bool IsOpened
    {
        get => _isOpened;
        set
        {
            _isOpened = value;
            _doorIn.IsOpened = _isOpened && _isInOpened;
            _doorOut.IsOpened = _isOpened && !_isInOpened;
        }
    }
    public StageName StageName { get; set; }
    
    public override void OnNetworkSpawn()
    {
        StageName = StageName.Size;
        IsOpened = true;
        IsInOpened = true;

        // Init
        OnClickAirlockButtonServerRpc(ColorType.Blue, true);
        OnClickAirlockButtonServerRpc(ColorType.Red, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnClickAirlockButtonServerRpc(ColorType colorType, bool isInButton)
    {
        Logger.Log($"Colortype {colorType}, is in : {isInButton}");
        SetButtonMaterialClientRpc(colorType, isInButton);
        if (colorType == ColorType.Blue)
        {
            _isBlueOpened = isInButton;
        }
        else if (colorType == ColorType.Red)
        {
            _isRedOpened = isInButton;
        }

        // 일치하면 문 개방 방향이 변경된다.
        if (_isBlueOpened == _isRedOpened)
        {
            IsInOpened = _isBlueOpened;
        }
    }

    [ClientRpc]
    private void SetButtonMaterialClientRpc(ColorType colorType, bool isInButton)
    {
        if (colorType == ColorType.Blue)
        {
            _blueInOutMaterials[isInButton ? 0 : 1].material = _inoutMaterials[1];
            _blueInOutMaterials[isInButton ? 1 : 0].material = _inoutMaterials[0];
        }
        else
        {
            _redInOutMaterials[isInButton ? 0 : 1].material = _inoutMaterials[2];
            _redInOutMaterials[isInButton ? 1 : 0].material = _inoutMaterials[0];
        }
    }
}
