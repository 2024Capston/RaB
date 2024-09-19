using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene의 종류
/// </summary>
public enum SceneType
{
    Title,
    Home,
    Lobby,
    InGame,
}

/// <summary>
/// Scene을 불러오기 위한 Singleton Class
/// </summary>
public class SceneLoader : SingletonBehavior<SceneLoader>
{

    /// <summary>
    /// SceneType Enum에 해당하는 Scene을 불러온다.
    /// </summary>
    /// <param name="sceneType"></param>
    public void LoadScene(SceneType sceneType)
    {
        Logger.Log($"{sceneType} scene loading");
        SceneManager.LoadScene(sceneType.ToString());
    }

    /// <summary>
    /// SceneType Enum에 해당하는 Scene을 비동기적으로 불러온다.
    /// </summary>
    /// <param name="sceneType"></param>
    /// <returns></returns>
    public AsyncOperation LoadSceneAsync(SceneType sceneType)
    {
        Logger.Log($"{sceneType} scene async loading");
        return SceneManager.LoadSceneAsync(sceneType.ToString());
    }
}
