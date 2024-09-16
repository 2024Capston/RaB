using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 사용할 Scene의 종류
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

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneType.ToString());
    }

    public void ReloadScene()
    {
        Logger.Log($"{SceneManager.GetActiveScene().name} scene loading");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
