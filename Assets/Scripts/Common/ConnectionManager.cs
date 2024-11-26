using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ConnectionManager : NetworkSingletonBehaviour<ConnectionManager>
{   
    private bool _isDisconnectRequested = false;
    
    /// <summary>
    /// Steam을 통해서 현재 접속해 있는 Lobby를 저장한다.
    /// </summary>
    public Lobby? CurrentLobby { get; set; }
    
    // TODO
    /*
     * 1. 내가 Client일 때 - 모든 연결을 끊고 Home으로 이동한다.
     * -> DisconnectHandler
     *
     * 2. 내가 Server일 때 -
     *  2.1 현재 Scene이 Lobby다. -> Client가 Owner인 NetworkObject는 모두 Destory되기 때문에 크게 처리할 것이 없다.
     *  2.2 현재 Scene이 InGame이다 -> Lobby로 Scene로 이동한다.
     */

    /// <summary>
    /// OnConnectionEvent에 Event를 등록하는 메소드
    /// </summary>
    public void RegisterNetworkEvents()
    {
        NetworkManager.Singleton.OnConnectionEvent += DisconnectHandler;
        _isDisconnectRequested = false;
    }

    public void UnRegisterNetworkEvents()
    {
        NetworkManager.Singleton.OnConnectionEvent -= DisconnectHandler;
    }

    public void RequestDisconnect()
    {
        _isDisconnectRequested = true;  
    }
    
    /// <summary>
    /// 연결이 끊어졌을 때 처리를 담당하는 핸들러
    /// 여기 로직이 굉장히 이쁘지 않기 때문에 추후 수정 필요합니다.
    /// </summary>
    private void DisconnectHandler(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        // ConnectionEvent.ClientDisconnected만 처리한다.
        if (connectionEventData.EventType != ConnectionEvent.ClientDisconnected)
        {
            return;
        }
        
        // Client가 Host와 연결이 끊어진 경우에만 처리합니다.
        if (!NetworkManager.Singleton.IsHost)
        {
            ClearConnection();
            UnRegisterNetworkEvents();
            
            if (!_isDisconnectRequested)
            {
                ConfirmUIData confirmUIData = new ConfirmUIData()
                {
                    ConfirmType = ConfirmType.OK,
                    TitleText = "네트워크 연결 실패",
                    DescText = $"서버와의 연결이 끊어졌습니다.",
                    OKButtonText = "확인",
                };
        
                UIManager.Instance.OpenUI<ConfirmUI>(confirmUIData);
            }
            
            SceneManager.LoadScene("Home");
        }
       
        else
        {
            // Host의 연결 종료가 아니라면 Client의 기록을 지운다
            if (!_isDisconnectRequested)
            {
                PlayerManager.Instance.DespawnClientPlayer();
            }
            // Host의 연결종료인 경우
            else
            {
                ClearConnection();
                UnRegisterNetworkEvents();
                SceneManager.LoadScene("Home");
            }
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
    }
}
