using Steamworks.Data;

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
/// Lobby를 관리하는 Singleton
/// </summary>
public class GameManager : NetworkSingletonBehaviour<GameManager>
{
    /// <summary>
    /// 현재 Lobby를 갖고 있다.
    /// </summary>
    public Lobby? CurrentLobby { get; set; } = null;

    /// <summary>
    /// 선택한 ChapterIndex
    /// </summary>
    public int SelectedIndex { get; set; } = -1;


}
