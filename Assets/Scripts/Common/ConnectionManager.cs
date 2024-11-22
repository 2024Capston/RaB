using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ConnectionManager : NetworkSingletonBehaviour<ConnectionManager>
{
    /// <summary>
    /// Steam을 통해서 현재 접속해 있는 Lobby를 저장한다.
    /// </summary>
    public Lobby? CurrentLobby { get; set; }
    
    // TODO
    /*
     * 1. 자신의 연결이 끊겼을 때의 처리
     *    -> 연결을 모두 초기화하고 HomeScene으로 돌아간다.
     * 
     * 2. 상대방이 연결이 끊겼을 때의 처리
     *
     *  2.1 현재 Scene이 Lobby일 때
     *      2.1.1 Server인 상대가 끊어졌을 때
     *          -> 연결을 모두 초기화하고 HomeScene으로 돌아간다.
     *      2.1.2 Client인 상대가 끊어졌을 때
     *          -> 어떤 일을 해주어야할지 아직 잘 모르겠다.
     * 
     *  2.2 현재 Scene이 InGame일 때
     *      2.2.1 Server인 상대가 끊어졌을 때
     *          -> 연결을 모두 초기화 하고 HomeScene으로 돌아간다.
     *      2.2.2 Client인 상대가 끊어졌을 때
     *          -> LobbyScene으로 돌아간다.
     */

    public void RegisterNetworkEvents()
    {
        NetworkManager.Singleton.OnConnectionEvent += OnClientDisconnect;
    }

    private void OnClientDisconnect(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        if (connectionEventData.EventType == ConnectionEvent.ClientConnected || connectionEventData.EventType == ConnectionEvent.PeerConnected)
        {
            // TODO 
            // 아마 여기는 처리 할게 없긴 한데 혹시 몰라서 적어 놓습니다.
            return;
        }
        
    }

    /// <summary>
    /// 연결을 모두 초기화하고 HomeScene으로 돌아간다.
    /// </summary>
    private void ClearConnection()
    {
        // 1. NetworkManager의 연결을 초기화 한다.
        NetworkManager.Singleton.Shutdown();
        
        // 2. Lobby의 연결을 초기화 한다.
        CurrentLobby?.Leave();
        CurrentLobby = null;
        
        ConfirmUIData confirmUIData = new ConfirmUIData()
        {
            ConfirmType = ConfirmType.OK,
            TitleText = "네트워크 연결 실패",
            DescText = $"서버와의 연결이 끊어졌습니다.",
            OKButtonText = "확인",
        };
        
        UIManager.Instance.OpenUI<ConfirmUI>(confirmUIData);
    }
}
