using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class KeypadController : NetworkBehaviour
{
    [SerializeField] private string _password;
    [SerializeField] TextMeshPro _passwordText;
    [SerializeField] private EventType[] _publishOnAnswer;

    public void Initialize(string password, EventType[] publishOnAnswer)
    {
        InitializeClientRpc(password, transform.position, transform.rotation, transform.lossyScale);
        _publishOnAnswer = publishOnAnswer;
    }

    // 서버, 클라이언트 초기 상태 동기화
    [ClientRpc]
    private void InitializeClientRpc(string password, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        _password = password;
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
    }
    
    public void OnButtonPressed(int i)
    {
       GetButtonInputServerRpc(i);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetButtonInputServerRpc(int i)
    {
        // Cancel Button
        if (i == 11 && _passwordText.text.Length > 0)
        {
            UpdateKeypadClientRpc(_passwordText.text.Remove(_passwordText.text.Length - 1));
        }

        // Execute Button
        else if (i == 12)
        {
            UpdateKeypadClientRpc("");
        }
        
        else
        {
            if (_passwordText.text.Length < 4) UpdateKeypadClientRpc(_passwordText.text + i.ToString());
        }

        if (_password == _passwordText.text)
        {
            UpdateKeypadClientRpc("GOOD");

            foreach (EventType evt in _publishOnAnswer)
            {
                EventBus.Instance.InvokeEvent(evt);
            }
        }
    }
    
    [ClientRpc]
    private void UpdateKeypadClientRpc(string passwordInput)
    {
        _passwordText.text = passwordInput;
    }
}
