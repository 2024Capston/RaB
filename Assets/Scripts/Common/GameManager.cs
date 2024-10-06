using Steamworks.Data;


/// <summary>
/// Lobby를 관리하는 Singleton
/// </summary>
public class GameManager : SingletonBehavior<GameManager>
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
