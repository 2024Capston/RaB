using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkSingletonBehaviour<T> : NetworkBehaviour where T : NetworkSingletonBehaviour<T>
{
    /// <summary>
    /// Scene이 바꿀 때 객체 파괴여부를 설정한다.
    /// </summary>
    protected bool _isDestroyOnLoad = false;

    protected static T _instance;

    public static T Instance
    {
        get => _instance;
    }

    private void Awake()
    {
        Init();
    }

    protected virtual void OnDestory()
    {
        Dispose();
    }

    /// <summary>
    /// 객체를 처음 초기화하는 메소드
    /// </summary>
    protected virtual void Init()
    {
        if (_instance == null)
        {
            _instance = (T)this;

            if (!_isDestroyOnLoad)
            {
                DontDestroyOnLoad(this);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 객체가 파괴될 때 호출되는 메소드
    /// </summary>
    protected virtual void Dispose()
    {
        _instance = null;
    }
}
