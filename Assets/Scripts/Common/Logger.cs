using System.Diagnostics;

/* 
Project Settings -> Player -> Scripting Define Symbols에
DEV_VER 심볼이 있으면 Console에 Log가 남는다.
배포 버전에선 DEV_VER 심볼을 없애면 Log 메소드가 실행되지 않아 오버헤드를 줄일 수 있다.
 */

/// <summary>
/// Log를 남길 Class
/// </summary>
public static class Logger
{
    [Conditional("DEV_VER")]
    public static void Log(string msg)
    {
        UnityEngine.Debug.LogFormat($"[{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {msg}");
    }

    [Conditional("DEV_VER")]
    public static void LogWarning(string msg)
    {
        UnityEngine.Debug.LogWarningFormat($"[{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {msg}");
    }

    public static void LogError(string msg)
    {
        UnityEngine.Debug.LogErrorFormat($"[{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {msg}");
    }
}
