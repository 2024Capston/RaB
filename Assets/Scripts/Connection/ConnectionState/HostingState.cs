using Steamworks;
using Steamworks.Data;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RaB.Connection
{
    internal class HostingState : OnlineState
    {
        public override void Enter()
        {
            // TODO 
            // Scene을 이동한다.
            SceneLoaderWrapper.Instance.LoadScene(SceneType.Lobby.ToString(), useNetworkSceneManager: true);
            UIManager.Instance.CloseAllOpenUI();
        }

        public override void Exit()
        {
            
        }

        public override void OnClientConnected(ulong _)
        {
            // TODO Client가 들어왔을 때 처리할 일
            // Player를 Spawn할 수 있게 처리한다. -> 현재 LobbyManager에서 처리
            
        }

        public override void OnClientDisconnect(ulong _)
        {
            // TODO 만약 현재 Scene이 InGame이라면 Lobby Scene으로 돌아가는게 필요
            if (SceneManager.GetActiveScene().name == SceneType.InGame.ToString())
            {
                SceneLoaderWrapper.Instance.LoadScene(SceneType.Lobby.ToString(), useNetworkSceneManager: true);
            }
        }

        public override void OnUserRequestedShutdown()
        {
            base.OnUserRequestedShutdown();

            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
            {
                ulong id = NetworkManager.Singleton.ConnectedClientsIds[i];
                if (id != NetworkManager.Singleton.LocalClientId)
                {
                    NetworkManager.Singleton.DisconnectClient(id, "HostEndedSession");
                }
            }
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Offline);
        }

        public override void OnServerStopped()
        {
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Offline);
        }

        public override async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
        {
            await lobby.Join();
        }
    }
}