using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class AirlockController : NetworkBehaviour
{
    [SerializeField] private DoorController _doorIn;
    [SerializeField] private DoorController _doorOut;

    /// <summary>
    /// 0 : Lock Material, 1 : Open Material
    /// </summary>
    [SerializeField] private Material[] _doorLightMaterials = new Material[2];

    /// <summary>
    /// 0 : _doorIn Mesh, 1 : _doorOut Mesh
    /// </summary>
    [SerializeField] private MeshRenderer[] _doorLightMeshRenderers = new MeshRenderer[2];
    
    /// <summary>
    /// 0 : off Material, 1 : Blue Material, 2 : Red Material
    /// </summary>
    [SerializeField] private Material[] _inoutMaterials = new Material[3];
    
    /// <summary>
    /// 0 : blue In Mesh, 1 : blue Out Mesh
    /// </summary>
    [SerializeField] private MeshRenderer[] _blueInOutMeshRenderers = new MeshRenderer[2];
    
    /// <summary>
    /// 0 : red In Mesh, 1 : red Out Mesh
    /// </summary>
    [SerializeField] private MeshRenderer[] _redInOutMeshRenderers = new MeshRenderer[2];
    
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
            _doorIn.IsOpened = _isInOpened && IsAirlockOpened;
            _doorOut.IsOpened = !_isInOpened && IsAirlockOpened;
            _doorLightMeshRenderers[0].material = _doorLightMaterials[_doorIn.IsOpened ? 1 : 0];
            _doorLightMeshRenderers[1].material = _doorLightMaterials[_doorOut.IsOpened ? 1 : 0];
        }
    }

    private bool _isAirlockOpened;
    /// <summary>
    /// true일 때 스테이지가 해금 되어 해당 문에 접근할 수 있다.
    /// </summary>
    public bool IsAirlockOpened
    {
        get => _isAirlockOpened;
        set
        {
            _isAirlockOpened = value;
            _doorIn.IsOpened = _isAirlockOpened && _isInOpened;
            _doorOut.IsOpened = _isAirlockOpened && !_isInOpened;
            _doorLightMeshRenderers[0].material = _doorLightMaterials[_doorIn.IsOpened ? 1 : 0];
            _doorLightMeshRenderers[1].material = _doorLightMaterials[_doorOut.IsOpened ? 1 : 0];
        }
    }
    public StageName StageName { get; set; }
    
    public override void OnNetworkSpawn()
    {
        StageName = StageName.Size;
        IsAirlockOpened = true;
        IsInOpened = true;

        // Init
        OnClickAirlockButtonServerRpc(ColorType.Blue, true);
        OnClickAirlockButtonServerRpc(ColorType.Red, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnClickAirlockButtonServerRpc(ColorType colorType, bool isInButton)
    {
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
            
            // TODO 둘 다 true가 되었을 때 Stage 이동이 필요
            /*if (!_isBlueOpened)
            {
                LobbyManager.Instance.RequestPlayStageServerRpc(StageName);
            }*/
        }
    }

    [ClientRpc]
    private void SetButtonMaterialClientRpc(ColorType colorType, bool isInButton)
    {
        if (colorType == ColorType.Blue)
        {
            _blueInOutMeshRenderers[isInButton ? 0 : 1].material = _inoutMaterials[1];
            _blueInOutMeshRenderers[isInButton ? 1 : 0].material = _inoutMaterials[0];
        }
        else
        {
            _redInOutMeshRenderers[isInButton ? 0 : 1].material = _inoutMaterials[2];
            _redInOutMeshRenderers[isInButton ? 1 : 0].material = _inoutMaterials[0];
        }
    }
}
