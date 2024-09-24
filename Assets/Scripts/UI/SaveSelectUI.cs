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

    public void OnClickSelectButton(int index)
    {
        HomeManager.Instance.SelectedIndex = index;
    }
    
    public void OnClickUndoButton()
    {
        HomeManager.Instance.SelectedIndex = -1;
        CloseUI();
    }
}
