using TMPro;
using UnityEngine;

public class SaveSelectUIData : BaseUIData
{
    public UserGameData UserGameData;
}

/// <summary>
/// 저장된 데이터를 선택하는 UI
/// </summary>
public class SaveSelectUI : BaseUI
{
    [SerializeField]
    private TMP_Text[] _saveDatasText = new TMP_Text[3];

    private SaveSelectUIData _saveSelectUIData;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        _saveSelectUIData = uiData as SaveSelectUIData;

        for (int i = 0; i < 3; i++)
        {
            if (!_saveSelectUIData.UserGameData.SaveDatas[i].HasData)
            {
                _saveDatasText[i].text = "새 게임";
            }
            else
            {
                _saveDatasText[i].text = $"챕터 진행도 : {_saveSelectUIData.UserGameData.SaveDatas[i].StageClearCount} / {_saveSelectUIData.UserGameData.SaveDatas[i].StageCount}";
            }
        }
    }

    /// <summary>
    /// 저장된 게임 슬롯을 선택하면 호출된다.
    /// </summary>
    /// <param name="index"></param>
    public void OnClickSelectButton(int index)
    {
        // TODO : 다른 객체가 현재 선택한 것을 들고 있을 것!
        //GameManager.Instance.SelectedIndex = index;

        string descText;
        if (!_saveSelectUIData.UserGameData.SaveDatas[index].HasData)
        {
            descText = "새 게임";
        }
        else
        {
            descText = $"챕터 진행도 : {_saveSelectUIData.UserGameData.SaveDatas[index].StageClearCount} / {_saveSelectUIData.UserGameData.SaveDatas[index].StageCount}";
        }


        ConfirmUIData confirmUIData = new ConfirmUIData()
        {
            ConfirmType = ConfirmType.OK_Cancel,
            TitleText = "확인",
            DescText = $"{descText}을\n시작하겠습니까?",
            OKButtonText = "확인",
            CancelButtonText = "취소",
            OnClickOKButton = () =>
            {
                if (!_saveSelectUIData.UserGameData.SaveDatas[index].HasData)
                {
                    _saveSelectUIData.UserGameData.SetNewData(index);
                    _saveSelectUIData.UserGameData.SaveData();
                }
                HomeManager.Instance.CreateLobby();
            },
        };
        UIManager.Instance.OpenUI<ConfirmUI>(confirmUIData);

    }

    public void OnClickUndoButton()
    {
        // TODO : 다른 객체가 현재 선택한 것을 들고 있을 것!
        //GameManager.Instance.SelectedIndex = -1;
        CloseUI();
    }
}
