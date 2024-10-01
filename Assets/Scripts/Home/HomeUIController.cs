using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeUIController : MonoBehaviour
{
    /// <summary>
    /// 방 만들기 버튼을 누르면 호출된다.
    /// </summary>
    public void OnClickCreateButton()
    {
        SaveSelectUIData saveSelectUIData = new SaveSelectUIData()
        {
            UserGameData = HomeManager.Instance.UserGameData
        };
        UIManager.Instance.OpenUI<SaveSelectUI>(saveSelectUIData);

    }

    /// <summary>
    /// 방 참가 버튼을 누르면 호출된다.
    /// </summary>
    public void OnClickEnterButton()
    {
        BaseUIData uidata = new BaseUIData();
        UIManager.Instance.OpenUI<CodeInputUI>(uidata); 
    }

    /// <summary>
    /// 설정 버튼을 누르면 호출된다.
    /// </summary>
    public void OnClickSettingButton()
    {

    }

    /// <summary>
    /// 종료 버튼을 누르면 호출된다.
    /// </summary>
    public void OnClickExitButton()
    {

    }
}
