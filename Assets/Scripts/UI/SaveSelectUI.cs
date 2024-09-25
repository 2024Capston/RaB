using System.Collections;
using System.Collections.Generic;
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
            if (_saveSelectUIData.UserGameData.ProgessChapter[i] == -1)
            {
                _saveDatasText[i].text = "새 게임";
            }
            else
            {
                _saveDatasText[i].text = $"현재 챕터 : {_saveSelectUIData.UserGameData.ProgessChapter[i]}";
            }
        }
    }

    /// <summary>
    /// 저장된 게임 슬롯을 선택하면 호출된다.
    /// </summary>
    /// <param name="index"></param>
    public void OnClickSelectButton(int index)
    {
        HomeManager.Instance.SelectedIndex = index;

        string descText;
        if (_saveSelectUIData.UserGameData.ProgessChapter[index] == -1)
        {
            descText = "새 게임";
        }
        else
        {
            descText = $"현재 챕터 : {_saveSelectUIData.UserGameData.ProgessChapter[index]}";
        }


        ConfirmUIData confirmUIData = new ConfirmUIData()
        {
            ConfirmType = ConfirmType.OK_Cancel,
            TitleText = "확인",
            DescText = $"{descText}을\n시작하겠습니까?",
            OKButtonText = "확인",
            CancelButtonText = "취소",

            // TODO OK 버튼을 눌렀을 때 
            // 1. Lobby를 생성하고 참가한다.
            // 2. Lobby Scene으로 이동한다.
        };
        UIManager.Instance.OpenUI<ConfirmUI>(confirmUIData);

    }
    
    public void OnClickUndoButton()
    {
        HomeManager.Instance.SelectedIndex = -1;
        CloseUI();
    }
}
