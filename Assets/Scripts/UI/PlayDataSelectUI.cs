using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayDataSelectUIData : BaseUIData
{
    public UserGameData UserGameData;
}

/// <summary>
/// 저장된 데이터를 선택하는 UI
/// </summary>
public class PlayDataSelectUI : BaseUI
{
    [FormerlySerializedAs("_saveDatasText")] [SerializeField]
    private TMP_Text[] _playDatasText = new TMP_Text[3];

    private PlayDataSelectUIData _playDataSelectUIData;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        _playDataSelectUIData = uiData as PlayDataSelectUIData;

        for (int i = 0; i < 3; i++)
        {
            if (!_playDataSelectUIData.UserGameData.PlayDatas[i].HasData)
            {
                _playDatasText[i].text = "새 게임";
            }
            else
            {
                _playDatasText[i].text = $"챕터 진행도 : {_playDataSelectUIData.UserGameData.PlayDatas[i].StageClearCount} / {_playDataSelectUIData.UserGameData.PlayDatas[i].StageCount}";
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
        if (!_playDataSelectUIData.UserGameData.PlayDatas[index].HasData)
        {
            descText = "새 게임";
        }
        else
        {
            descText = $"챕터 진행도 : {_playDataSelectUIData.UserGameData.PlayDatas[index].StageClearCount} / {_playDataSelectUIData.UserGameData.PlayDatas[index].StageCount}";
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
                if (!_playDataSelectUIData.UserGameData.PlayDatas[index].HasData)
                {
                    _playDataSelectUIData.UserGameData.SetNewData(index);
                    _playDataSelectUIData.UserGameData.SaveData();
                }

                ConnectionManager.Instance.SelectPlayData = index;
                RaB.Connection.ConnectionManager.Instance.StartServer();
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
