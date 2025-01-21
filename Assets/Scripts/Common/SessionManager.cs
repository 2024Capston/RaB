using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 선택한 GameData와 Stage 등 게임정보들을 관리
/// </summary>
public class SessionManager : NetworkSingletonBehaviour<SessionManager>
{
    private UserGameData _userGameData;
    private int _selectedDataIndex;
    
    public PlayData SelectedData { get; private set; }
    public StageName SelectedStage { get; set; }
    public int CurrentFloor { get; set; }

    /// <summary>
    /// 방에 입장할 때 Host에서 새 Session을 만든다.
    /// </summary>
    public void CreateSession(int selectedDataIndex)
    {
        _userGameData = UserDataManager.Instance.GetUserData<UserGameData>();
        _selectedDataIndex = selectedDataIndex;
        SelectedData = _userGameData.PlayDatas[_selectedDataIndex];
        CurrentFloor = 1;
    }
    
    /// <summary>
    /// 현재 Play한 Stage의 Clear Data를 저장 
    /// </summary>
    public void SaveGameData()
    {
        _userGameData.UpdateData(_selectedDataIndex, Instance.SelectedStage, 1);
    }
}
