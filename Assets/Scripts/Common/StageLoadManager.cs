using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Stage에 입장할 때 StageLoader를 생성하는 Manager
/// </summary>
public class StageLoadManager : NetworkSingletonBehaviour<StageLoadManager>
{
    private readonly string STAGELOADER_PATH = "Prefabs/Stage/StageLoader/";
    
    private Dictionary<StageName, string> _stageLoaderData;
    
    protected override void Init()
    {
        base.Init();

        _stageLoaderData = new Dictionary<StageName, string>();
        for (int i = 0; i < (int)StageName.Size; i++)
        {
            string stageLoaderName = ((StageName)i).ToString() + "Loader";
            _stageLoaderData.Add((StageName)i, stageLoaderName);
        }
    }

    /// <summary>
    /// StageName에 해당하는 StageLoader를 생성하여 반환합니다.
    /// </summary>
    /// <param name="stageName">생성할 StageName</param>
    /// <returns>생성된 Loader</returns>
    public StageLoader LoadLoader(StageName stageName)
    {
        // 해당 Stage의 Loader의 이름이 해싱되었는지 확인
        if (!_stageLoaderData.TryGetValue(stageName, out string stageLoaderName))
        {
            throw new Exception($"{stageName} does not exist in the dictionary");
        }
        
        GameObject loaderPrefab = Resources.Load<GameObject>(STAGELOADER_PATH + stageLoaderName);
        
        // 올바른 경로에 Loader가 존재하는지 확인
        if (loaderPrefab is null)
        {
            throw new Exception($"Cannot find the path {STAGELOADER_PATH + stageLoaderName}");
        }

        StageLoader stageLoader = Instantiate(loaderPrefab).GetComponent<StageLoader>();
        
        // 생성된 Loader에 StageLoader 컴포넌트가 있는지 확인
        if (stageLoader is null)
        {
            throw new Exception("StageLoader Component does not exist");
        }
        
        // Loader 컴포넌트가 존재하면 NetworkObject 컴포넌트는 존재하므로 예외처리 필요 없음
        stageLoader.GetComponent<NetworkObject>().Spawn();

        return stageLoader;
    } 
}
