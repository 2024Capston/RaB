using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class K1_StageManager : StageManager
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
