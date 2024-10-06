using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// User Data를 저장하거나 가져오는 Singleton Class
/// </summary>
public class UserDataManager : SingletonBehavior<UserDataManager>
{
    private List<IUserData> _userDataList = new List<IUserData>();

    /// <summary>
    /// 저장된 User Data가 있는지 확인하는 Bool Property
    /// </summary>
    public bool HasUserData { get; private set; }



    protected override void Init()
    {
        base.Init();

        // TODO : IUserData를 상속받은 UserDataClass를 _userDataList에 추가해야 한다.
        // ex)
        _userDataList.Add(new UserGameData());
    }

    /// <summary>
    /// UserData를 기본값으로 설정한다.
    /// </summary>
    public void SetDefaultUserData()
    {
        foreach (IUserData userData in _userDataList)
        {
            userData.SetDefaultData();
        }
    }

    /// <summary>
    /// 저장되어있는 UserData를 불러온다.
    /// </summary>
    public void LoadUserData()
    {
        // 저장된 UserData가 있는지 불러온다. 
        // TODO : SteamCloud 사용시 해당 경로로 바꾸어야 한다.
        HasUserData = PlayerPrefs.GetInt("HasUserData") == 1 ? true : false;

        if (HasUserData)
        {
            foreach (IUserData userData in _userDataList)
            {
                userData.LoadData();
            }
        }
    }

    public void SaveUserData()
    {
        // 저장 과정에서 문제가 생기지 않는지 확인하는 로컬변수
        bool hasSaveError = false;

        foreach (IUserData userData in _userDataList)
        {
            bool isSaveSuccess = userData.SaveData();

            // 저장과정에서 문제가 생겼다면 hasSaveError를 true로 바꾸어준다.
            if (!isSaveSuccess)
            {
                hasSaveError = true;
            }
        }

        // 저장과정에서 문제가 없었다면 저장된 UserData가 있다는 사실을 저장한다.
        if (!hasSaveError)
        {
            HasUserData = true;
            PlayerPrefs.SetInt("HasUserData", 1);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// IUserData를 상속받은 T UserData를 반환한다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetUserData<T>() where T : class, IUserData
    {
        return _userDataList.OfType<T>().FirstOrDefault();
    }
}
