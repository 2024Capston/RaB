using System;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;

namespace RaB.Connection
{
    internal class StartingHostState : OnlineState
    {
        public override void Enter()
        {
            StartLobby();
        }

        public override void Exit() { }

        public override void OnLobbyCreated(Result result, Lobby lobby)
        {
            if (result != Result.OK)
            {
                Logger.LogError($"Lobby Created Error!, Error Code : {result.ToString()}");
                StartHostFailed();
                return;
            }
            
            lobby.SetPublic();
            lobby.SetJoinable(true);
        }

        public override void OnLobbyEntered(Lobby lobby)
        {
            ConnectionManager.Instance.CurrentLobby = lobby;
            Logger.Log($"Lobby ID : {lobby.Id.ToString()}");

            if (!NetworkManager.Singleton.StartHost())
            {
                Logger.LogError("Start Host Failed!");
                StartHostFailed();
            }
        }

        public override void OnServerStarted()
        {   
             ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Hosting);
        }

        private async void StartLobby()
        {
            try
            {
                await SteamMatchmaking.CreateLobbyAsync(2);
            }
            catch (Exception)
            {
                StartHostFailed();
                throw;
            }
        }

        private void StartHostFailed()
        {
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Offline);
        }
    }
}