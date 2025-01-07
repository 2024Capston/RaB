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
        Result result = await RaB.Connection.ConnectionManager.Instance.StartClient(_inputField.text);

        switch (result)
        {
            case Result.OK:
            {
                _descText.text = "Find the lobby!";
                _descText.color = UnityEngine.Color.black;
                CloseUI();
                break;
            }
            case Result.InvalidParam:
            {
                _descText.text = "The format is incorrect.";
                _descText.color = UnityEngine.Color.red;
                break;
            }
            case Result.Busy:
            {
                _descText.text = "Lobby is full";
                _descText.color = UnityEngine.Color.red;
                break;
            }
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
