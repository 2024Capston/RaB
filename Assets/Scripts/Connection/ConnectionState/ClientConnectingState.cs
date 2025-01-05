using System;
using Netcode.Transports.Facepunch;
using Steamworks.Data;
using Unity.Netcode;

namespace RaB.Connection
{
    internal class ClientConnectingState : OnlineState
    {
        public override void Enter()
        {
            EnterLobby();
        }

        public override void Exit()
        {
            
        }

        public override void OnClientConnected(ulong _)
        {
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.ClientConnected);
        }

        public override void OnClientDisconnect(ulong _)
        {
            StartingClientFailed();
        }

        public override void OnLobbyEntered(Lobby lobby)
        {
            Logger.Log($"Lobby ID : {lobby.Id.ToString()}");
            
            NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = ConnectionManager.Instance.CurrentLobby.Value.Owner.Id;
            
            if (!NetworkManager.Singleton.StartClient())
            {
                Logger.LogError("Start Client Failed!");
                StartingClientFailed();
            }
        }

        private async void EnterLobby()
        {
            try
            {
                await ConnectionManager.Instance.CurrentLobby?.Join();
            }
            catch (Exception)
            {
                StartingClientFailed();
                throw;
            }
        }

        private void StartingClientFailed()
        {
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Offline);    
        }       
        
        
    }
}