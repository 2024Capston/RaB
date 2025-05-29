using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class K1_StageManager : StageManager
{
    private int _lastPressedButton;
    private string _buttonCheck;
    public override void StartGame()
    {
        _buttonCheck = "";
        EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventE, OnButtonEPressed);
        EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventF, OnButtonFPressed);
        EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventG, OnButtonGPressed);
        EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventH, OnButtonHPressed);
    }

    private void OnButtonEPressed()
    {
        ButtonCheck("1");
    }

    private void OnButtonFPressed()
    {
        ButtonCheck("2");
    }
    private void OnButtonGPressed()
    {
        ButtonCheck("3");
    }
    private void OnButtonHPressed()
    {
        ButtonCheck("4");
    }

    private void ButtonCheck(string num)
    {
        _buttonCheck += num;
        if (_buttonCheck.Length == 4) {
            if (_buttonCheck == "1234")
            {
                EventBus.Instance.InvokeEvent(EventType.EventI);
            }
            StartCoroutine(WaitForButtonCheck());
        }
    }

    private IEnumerator WaitForButtonCheck()
    {
        yield return new WaitForSeconds(0.5f);
        EventBus.Instance.InvokeEvent(EventType.EventJ);
        _buttonCheck = "";
    }

    public override void RestartGame()
    {
        
    }

    public override void EndGame()
    {
        InGameManager.Instance.EndGameServerRpc();  
    }
}
