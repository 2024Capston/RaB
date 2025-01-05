using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace RaB.Connection
{
    internal class OfflineState : ConnectionState
    {
        public override void Enter()
        {
            ConnectionManager.Instance.CurrentLobby?.Leave();
            ConnectionManager.Instance.CurrentLobby = null;
            NetworkManager.Singleton.Shutdown();
            
            // Offline 상태에서는 Home Scene에 있어야 합니다.
            if (SceneManager.GetActiveScene().name != SceneType.Home.ToString())
            {
                // TODO
                // SceneManager를 개편하고 HomeScene을 Load하는 코드를 추가한다.
            }
        }

        public override void Exit() { }

        public override void StartServer()
        {
            // Server를 시작하면 StartingHost State로 전환
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.StartingHost);
        }

        public override void StartClient(string lobbyId, out Result result)
        {
            Lobby? lobby = null;
            (lobby, result) = FindLobby(lobbyId).Result;
            
            if (result == Result.OK)
            {
                ConnectionManager.Instance.CurrentLobby = lobby;
                ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.ClientConnecting);
            }
        }

        private async Task<(Lobby? lobby, Result result)> FindLobby(string lobbyId)
        {
            if (!ulong.TryParse(lobbyId, out ulong id))
            {
                return (null, Result.InvalidParam);
            }

            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

            foreach (Lobby lobby in lobbies)
            {
                if (lobby.Id == id)
                {
                    if (lobby.MemberCount == 2)
                    {
                        return (null, Result.Busy);
                    }

                    return (lobby, Result.OK);
                }
            }

            return (null, Result.InvalidName);
        }
        
    }
}