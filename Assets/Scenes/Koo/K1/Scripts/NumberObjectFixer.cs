using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NumberObjectFixer : NetworkBehaviour
{
    // 고정시킬 좌표
    private Vector3 _fixedPosition;
    
    // 고정시킬 각도
    private Quaternion _fixedRotation;
    
    // 고정시킬 물체 이름
    private string _objectName;
    
    private Rigidbody _rigidbody;
    
    private NumberObjectController _objectOnFixer;

    public void Initialize(Vector3 fixedPosition, Quaternion fixedRotation, string objectName)
    {
        InitializeClientRpc(fixedPosition, fixedRotation, objectName);
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsServer && _objectOnFixer == null && other.gameObject.TryGetComponent<NumberObjectController>(out NumberObjectController noc) && other.gameObject.name == _objectName)
        {
            SetObjectOnFixerClientRpc(other.gameObject.GetComponent<NetworkObject>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsServer && other.gameObject.TryGetComponent<NumberObjectController>(out NumberObjectController noc) &&
            _objectOnFixer == noc)
        {
            ResetObjectOnFixerClientRpc();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_objectOnFixer != null && _objectOnFixer.IsOwner && PlayerController.LocalPlayer.Color == ColorType.Blue && !_objectOnFixer.IsTaken)
        {
            _rigidbody.MovePosition(_fixedPosition);
            _rigidbody.MoveRotation(_fixedRotation);
        } 
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetObjectOnFixerClientRpc(NetworkObjectReference networkObject)
    {
        if (networkObject.TryGet(out NetworkObject no))
        {
            _objectOnFixer = no.GetComponent<NumberObjectController>();
            _rigidbody = no.GetComponent<Rigidbody>();
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void ResetObjectOnFixerClientRpc()
    {
        _objectOnFixer = null;
        _rigidbody = null;
    }
    
    [ClientRpc(RequireOwnership = false)]
    private void InitializeClientRpc(Vector3 fixedPosition, Quaternion fixedRotation, string objectName)
    {
        _fixedPosition = fixedPosition;
        _fixedRotation = fixedRotation.normalized;
        _objectName = objectName;
    }
}
