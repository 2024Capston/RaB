using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnAreaController : MonoBehaviour
{
    private Rigidbody _rigidbody;

    [SerializeField] private string[] _redSavePoints;
    [SerializeField] private string[] _blueSavePoints;

    private int _redSaved, _blueSaved;
    
    [SerializeField] private EndBlockController[] _endBlockController;

    public int RedSaved
    {
        get => _redSaved;
        set => _redSaved = value;
    }
    
    public int BlueSaved
    {
        get => _blueSaved;
        set => _blueSaved = value;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController playerController))
        {
            _rigidbody = playerController.GetComponent<Rigidbody>();

            /*if (_redSaved == 3 && _blueSaved == 3)
            {
                if (playerController.Color == ColorType.Red)
                {
                    ResetServerRpc();
                    _rigidbody.MovePosition(GameObject.FindWithTag(_redSavePoints[_redSaved]).transform.position);
                }
                else
                {
                    ResetClientRpc();
                    _rigidbody.MovePosition(GameObject.FindWithTag(_blueSavePoints[_blueSaved]).transform.position);
                }
            }
            else
            {
                if (playerController.Color == ColorType.Red)
                {
                    _rigidbody.MovePosition(GameObject.FindWithTag(_redSavePoints[_redSaved]).transform.position);
                }
                else
                {
                    _rigidbody.MovePosition(GameObject.FindWithTag(_blueSavePoints[_blueSaved]).transform.position);
                }
            }*/
            if (playerController.Color == ColorType.Red)
            {
                _rigidbody.MovePosition(GameObject.FindWithTag(_redSavePoints[_redSaved]).transform.position);
            }
            else
            {
                _rigidbody.MovePosition(GameObject.FindWithTag(_blueSavePoints[_blueSaved]).transform.position);
            }
        }
        
        _endBlockController[0].Reset();
        _endBlockController[1].Reset();
    }
    
    /*[ServerRpc(RequireOwnership = false)]
    private void ResetServerRpc()
    {
        Rigidbody rigidbody = PlayerController.LocalPlayer.GetComponent<Rigidbody>();
        rigidbody.MovePosition(GameObject.FindWithTag(_blueSavePoints[_blueSaved]).transform.position);
    }

    [ClientRpc(RequireOwnership = false)]
    private void ResetClientRpc()
    {
        if (IsServer) return;
        Rigidbody rigidbody = PlayerController.LocalPlayer.GetComponent<Rigidbody>();
        rigidbody.MovePosition(GameObject.FindWithTag(_redSavePoints[_redSaved]).transform.position);
    }*/
}
