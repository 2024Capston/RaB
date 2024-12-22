using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Stage에 입장할 때 StageLoader를 생성하는 Manager
/// </summary>
public class StageLoadManager : NetworkSingletonBehaviour<StageLoadManager>
{
    private readonly string STAGELOADER_PATH = "Prefabs/Stage/StageLoader";
    
    private Dictionary<StageName, string> _stageLoaderData;
    
    protected override void Init()
    {
        base.Init();

        _stageLoaderData = new Dictionary<StageName, string>();
        for (int i = 0; i < (int)StageName.Size; i++)
        {
            string stageLoaderName = ((StageName)i).ToString() + "Loader";
            Logger.Log(stageLoaderName);
            _stageLoaderData.Add((StageName)i, stageLoaderName);
        }
    }

    /// <summary>
    /// StageName에 해당하는 StageLoader를 Load
    /// </summary>
    /// <param name="stageName"></param>
    [ServerRpc]
    public void LoadStageServerRpc(StageName stageName)
    {
        if (!_stageLoaderData.TryGetValue(stageName, out string stageLoderName))
        {
            
            return;
        }

        GameObject gameObject = Instantiate(Resources.Load<GameObject>(STAGELOADER_PATH + stageLoderName));
        if (!gameObject)
        {
            return;
        }
        gameObject.GetComponent<NetworkObject>().Spawn();
        
        StageLoader stageLoader = gameObject.GetComponent<StageLoader>();
        if (!stageLoader)
        {
            return;
        }

        stageLoader.LoadStage();
    } 
}
