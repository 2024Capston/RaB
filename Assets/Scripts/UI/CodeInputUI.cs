using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeInputUI : BaseUI
{
    [SerializeField]
    private TMP_InputField _inputField;

    [SerializeField]
    private Button _okButton;

    [SerializeField]
    private TMP_Text _descText;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        _descText.text = "";
    }

    /// <summary>
    ///  버튼을 눌렀을 때 일치하는 Lobby가 있는지 확인하고, 해당하는 Lobby가 있으면 입장한다.
    /// </summary>
    public async void OnClickOkButton()
    {
        // 코드를 입력받고 그에 따른 처리를 합니다.
        Result result = await RaB.Connection.ConnectionManager.Instance.StartClient(_inputField.text);

        switch (result)
        {
            // 유효한 코드의 경우 바로 해당 Lobby로 입장
            case Result.OK:
            {
                _descText.text = "Find the lobby!";
                _descText.color = UnityEngine.Color.black;
                CloseUI();
                break;
            }
            // 코드 형식이 올바르지 않은 경우
            case Result.InvalidParam:
            {
                _descText.text = "The format is incorrect.";
                _descText.color = UnityEngine.Color.red;
                break;
            }
            // 로비가 꽉 찾을 때
            case Result.Busy:
            {
                _descText.text = "Lobby is full";
                _descText.color = UnityEngine.Color.red;
                break;
            }
            // 유효하지 않은 코드
            case Result.InvalidName:
            {
                _descText.text = "Couldn't find the lobby";
                _descText.color = UnityEngine.Color.red;
                break;
            }
        }
    }

    public void OnClickUndoButton()
    {
        CloseUI();
    }
}
