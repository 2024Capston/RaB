using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;


/// <summary>
/// Lobby를 관리하는 Singleton
/// </summary>
public class LobbyManager : SingletonBehavior<LobbyManager>
{
    /// <summary>
    /// 현재 Lobby를 갖고 있다.
    /// </summary>
    public Lobby? CurrentLobby { get; set; } = null;
}
