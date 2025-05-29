using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class K2_StageManager : StageManager
{
    public override void StartGame()
    {
    }

    public override void RestartGame()
    {
        
    }

    public override void EndGame()
    {
        InGameManager.Instance.EndGameServerRpc();  
    }
}
