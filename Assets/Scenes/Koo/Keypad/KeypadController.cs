using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class KeypadController : NetworkBehaviour
{
    private string _password;
    [SerializeField] TextMeshPro _passwordText;
    [SerializeField] TextMeshPro _placeholderText;
    private EventType[] _publishOnAnswer;

    private AudioSource _audioSource;

    public void Initialize(string password, EventType[] publishOnAnswer)
    {
        InitializeClientRpc(password);
        _publishOnAnswer = publishOnAnswer;
        _audioSource = GetComponent<AudioSource>();

    }

    // 서버, 클라이언트 초기 상태 동기화
    [ClientRpc]
    private void InitializeClientRpc(string password)
    {
        _password = password;
    }
    
    public void OnButtonPressed(int i)
    {
       GetButtonInputServerRpc(i);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetButtonInputServerRpc(int i)
    {
        if (!_passwordText.enabled)
        {
            return;
        }

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

        if (_passwordText.text.Length == 4)
        {
            if (_password == _passwordText.text)
            {
                ClearKeypadClientRpc();

                foreach (EventType evt in _publishOnAnswer)
                {
                    EventBus.Instance.InvokeEvent(evt);
                }
            }
            else
            {
                ErrorKeypadClientRpc();
            }
        }
    }
    
    [ClientRpc]
    private void UpdateKeypadClientRpc(string passwordInput)
    {
        _passwordText.text = passwordInput;

        _audioSource.pitch = Random.Range(1.0f, 2.0f);
        _audioSource.PlayOneShot(_audioSource.clip, _audioSource.volume);
    }

    [ClientRpc]
    private void ClearKeypadClientRpc()
    {
        _passwordText.enabled = false;

        _placeholderText.GetComponent<RectTransform>().anchoredPosition = new Vector3(-0.06f, 1.84f, 0.125f);
        _placeholderText.fontSize = 18;

        _placeholderText.text = "CORRECT";
    }

    [ClientRpc]
    private void ErrorKeypadClientRpc()
    {
        _passwordText.enabled = false;

        _placeholderText.GetComponent<RectTransform>().anchoredPosition = new Vector3(-0.06f, 1.84f, 0.125f);
        _placeholderText.fontSize = 18;

        _placeholderText.text = "ERROR";

        StartCoroutine(CoDisplayError());
    }

    private IEnumerator CoDisplayError()
    {
        yield return new WaitForSeconds(2.0f);

        _passwordText.text = "";
        _passwordText.enabled = true;

        _placeholderText.GetComponent<RectTransform>().anchoredPosition = new Vector3(-0.06f, 1.803f, 0.125f);
        _placeholderText.fontSize = 14;
        _placeholderText.text = "_ _ _ _";
    }
}
