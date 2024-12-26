using System;
using UnityEngine;
using System.Collections.Generic;

/*
 임시로 레지스트리에 현재 진행 중인 Chapter를 저장하도록 만들었습니다.
 추후 SteamCloud 사용 시 해당 부분을 이용하도록 변경할 예정이고
 저장할 데이터들도 변경 될 예정입니다.
 현재 데이터는 예시 용으로 만들었다고 생각하면 됩니다.
 */

[Serializable]
public class MapInfo
{
    public int Floor;
    public int Stage;
    public int ClearFlag;

    public MapInfo(int floor, int stage, int clearflag)
    {
        Floor = floor;
        Stage = stage;
        ClearFlag = clearflag;
    }
}

/// <summary>
/// MapInfo Json으로 사용하기 위한 WrapperClass
/// </summary>
[Serializable]
public class MapInfoListWrapper
{
    public List<MapInfo> MapInfoList;
}

public class SaveData
{
    public bool HasData { get; set; }
    public List<MapInfo> MapInfoList { get; set; }
    public int StageCount { get; set; }
    public int StageClearCount { get; set; }

    public SaveData()
    {
        HasData = false;
        MapInfoList = new List<MapInfo>();
        StageCount = 0;
        StageClearCount = 0;
    }
}

/// <summary>
/// Player의 game 진행 정보를 담고 있는 Class
/// </summary>
public class UserGameData : IUserData
{
    private readonly string MAP_DATA_PATH = "Json/";
    public List<SaveData> SaveDatas { get; private set; }
    
    public void SetDefaultData()
    {
        Logger.Log($"{GetType()}::SetDefaultData");

        SaveDatas = new List<SaveData>();
        
        for (int i = 0; i < 3; i++)
        {
            SaveDatas.Add(new SaveData());
        }
    }

    public bool LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");
        
        bool result = false;
        SaveDatas = new List<SaveData>();
        
        try
        {
            for (int i = 0; i < 3; i++)
            {
                SaveData saveData = new SaveData();
                string mapInfoListstring = PlayerPrefs.GetString($"MapInfoList{i}");

                if (mapInfoListstring[0] == '1')
                {
                    saveData.HasData = true;
                    string mapInfoListJson = mapInfoListstring.Substring(1);
                    if (!string.IsNullOrEmpty(mapInfoListJson))
                    {
                        MapInfoListWrapper mapInfoListWrapper = JsonUtility.FromJson<MapInfoListWrapper>(mapInfoListJson);
                        foreach (MapInfo mapInfo in mapInfoListWrapper.MapInfoList)
                        {
                            saveData.MapInfoList.Add(new MapInfo(mapInfo.Floor, mapInfo.Stage, mapInfo.ClearFlag));                   
                            saveData.StageCount++;
                            saveData.StageClearCount += mapInfo.ClearFlag;
                        }
                    }    
                }
                
                SaveDatas.Add(saveData);
            }
            result = true;
        }
        catch (Exception e)
        {
            Logger.Log($"Load failed. (" + e.Message + ")");
        }

        return result;
    }

    public bool SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");

        bool result = false;

        try
        {
            for (int i = 0; i < 3; i++)
            {
                string mapInfoListstring = SaveDatas[i].HasData ? "1" : "0";
                if (SaveDatas[i].HasData)
                {
                    MapInfoListWrapper mapInfoListWrapper = new MapInfoListWrapper();
                    mapInfoListWrapper.MapInfoList = SaveDatas[i].MapInfoList;
                    string mapInfoListJson = JsonUtility.ToJson(mapInfoListWrapper);
                    mapInfoListstring += mapInfoListJson;
                }
                PlayerPrefs.SetString($"MapInfoList{i}", mapInfoListstring);
            }

            result = true;
        }
        catch (Exception e)
        {
            Logger.Log($"Save failed. (" + e.Message + ")");
        }

        return result;
    }

    public void SetNewData(int index)
    {
        SaveData saveData = SaveDatas[index];

        saveData.HasData = true;
        saveData.StageCount = 0;
        saveData.StageClearCount = 0;
        saveData.MapInfoList = new List<MapInfo>();
        
        // defaultData는 Resources/Json/mapdefaultinfo.json을 읽어온다.
        TextAsset defalutjson = Resources.Load<TextAsset>(MAP_DATA_PATH + "defaultmapinfo");
        if (!defalutjson)
        {
            Logger.LogError($"{MAP_DATA_PATH + "defaultmapinfo"} does not exist.");
            return;
        }

        MapInfoListWrapper mapInfoListWrapper = JsonUtility.FromJson<MapInfoListWrapper>(defalutjson.ToString());
        
        foreach (MapInfo mapInfo in mapInfoListWrapper.MapInfoList)
        {
            saveData.MapInfoList.Add(new MapInfo(mapInfo.Floor, mapInfo.Stage, mapInfo.ClearFlag));                   
            saveData.StageCount++;
            saveData.StageClearCount += mapInfo.ClearFlag;
        }
    }
}
