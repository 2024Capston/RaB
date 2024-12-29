using UnityEngine;

public class HomeUIController : MonoBehaviour
{
    /// <summary>
    /// 방 만들기 버튼을 누르면 호출된다.
    /// </summary>
    public void OnClickCreateButton()
    {
        PlayDataSelectUIData playDataSelectUIData = new PlayDataSelectUIData()
        {
            UserGameData = HomeManager.Instance.UserGameData
        };
        UIManager.Instance.OpenUI<PlayDataSelectUI>(playDataSelectUIData);

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
        // TODO : 설정 팝업을 만들면 해당 팝업을 띄운다.
    }

    /// <summary>
    /// 종료 버튼을 누르면 호출된다.
    /// </summary>
    public void OnClickExitButton()
    {
        Application.Quit();
    }
}
