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
     * 
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

    /// <summary>
    /// OnConnectionEvent에 Event를 등록하는 메소드
    /// </summary>
    public void RegisterNetworkEvents()
    {
        NetworkManager.Singleton.OnConnectionEvent += DisconnectHandler;
    }

    public void UnRegisterNetworkEvents()
    {
        NetworkManager.Singleton.OnConnectionEvent -= DisconnectHandler;
    }
    
    /// <summary>
    /// 연결이 끊어졌을 때 처리를 담당하는 핸들러
    /// </summary>
    private void DisconnectHandler(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        // ConnectionEvent.ClientDisconnected만 처리한다.
        if (connectionEventData.EventType != ConnectionEvent.ClientDisconnected)
        {
            return;
        }
        
        // Client가 Server와 연결이 끊어진 경우에만 처리합니다.
        if (!NetworkManager.Singleton.IsHost)
        {
            ClearConnection();
            UnRegisterNetworkEvents();
            SceneManager.LoadScene("Home");
        }
    }

    /// <summary>
    /// 연결상태를 모두 초기화한다.
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
