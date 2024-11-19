using Steamworks.Data;
using TMPro;
using Unity.Netcode;

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

public class GameManager : NetworkSingletonBehaviour<GameManager>
{

}
