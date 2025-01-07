using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using Unity.Multiplayer.Samples.Utilities;
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
            
            UIManager.Instance.CloseAllOpenUI();
            
            // Offline 상태에서는 Home Scene에 있어야 합니다.
            if (SceneManager.GetActiveScene().name != SceneType.Home.ToString())
            {
                SceneLoaderWrapper.Instance.LoadScene(SceneType.Home.ToString(), false);
            }
        }

        public override void Exit() { }

        public override void StartServer()
    {   
            // Server를 시작하면 StartingHost State로 전환
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.StartingHost);
        }

        public override async Task<Result> StartClient(string lobbyId)
        {
            Logger.Log($"input lobbyId : {lobbyId}");
            Lobby? lobby = null;
            Result result = Result.None;
            (lobby, result) =  await FindLobby(lobbyId);
            
            if (result == Result.OK)
            {
                ConnectionManager.Instance.CurrentLobby = lobby;
                ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.ClientConnecting);
            }

            return result;
        }

        private async Task<(Lobby? lobby, Result result)> FindLobby(string lobbyId)
        {
            if (!ulong.TryParse(lobbyId, out ulong id))
            {
                return (null, Result.InvalidParam);
            }

            LobbyQuery lobbyQuery = new LobbyQuery();
            Lobby[] lobbies = await lobbyQuery.WithSlotsAvailable(1).RequestAsync();

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
        
        public override void GameLobbyJoinRequested(Lobby lobby, SteamId _)
        {
            ConnectionManager.Instance.CurrentLobby = lobby;
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.ClientConnecting);
        }
        
    }
}