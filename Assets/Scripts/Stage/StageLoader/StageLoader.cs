using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Stage들을 Load하는 Class
/// </summary>
public abstract class StageLoader : NetworkBehaviour
{
    /*
     * StageLoader에서 할 일
     * 1. 등록되어 있는 StageManager를 생성한다.
     * 2. 맵들을 불러온다.
     */
    
    [SerializeField] protected StageManager StageManager;
    
    [SerializeField] protected GameObject Map;
    
    public void LoadStage()
    {
        Instantiate(StageManager);
        StageManager.StageLoader = this;
        
        Instantiate(Map);
    }

    public void ResetStage()
    {
        
    }
}
 