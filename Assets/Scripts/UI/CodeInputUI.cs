using Steamworks;
using Steamworks.Data;
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

    private Lobby? _currentLobby;
    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        _descText.text = "";
        _okButton.interactable = false;
        _currentLobby = null;
    }

    /// <summary>
    ///  버튼을 눌렀을 때 일치하는 Lobby가 있는지 확인한다.
    /// </summary>
    public async void OnClickSearchButton()
    {
        _currentLobby = null;
        _okButton.interactable = false;

        ulong ID;
        if (!ulong.TryParse(_inputField.text, out ID))
        {
            _descText.text = $"The format is incorrect.";
            _descText.color = UnityEngine.Color.red;
            return;
        }

        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

        foreach (Lobby lobby in lobbies)
        {
            if (lobby.Id == ID)
            {
                _descText.text = "Find the lobby!";
                _descText.color = UnityEngine.Color.black;
                _currentLobby = lobby;
                _okButton.interactable = true;

                return;
            }
        }

        // Lobby를 찾지 못했을 때
        _descText.text = $"Couldn't find the lobby";
        _descText.color = UnityEngine.Color.red;
    }

    public async void OnClickOkButton()
    {
        _okButton.interactable = false;
        await _currentLobby?.Join();
        _currentLobby = null;
    }

    public void OnClickUndoButton()
    {
        CloseUI();
    }
}
