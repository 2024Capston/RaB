using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace RaB.Connection
{
    internal abstract class ConnectionState
    {
        public abstract void Enter();

        public abstract void Exit();
        
        public virtual void OnClientConnected() { }
        
        public virtual void OnClientDisconnect() { }
        
        public virtual void OnServerStarted() { }
        
        public virtual void OnServerStopped() { }
        
        public virtual void StartServer() { }

        public virtual Task<Result> StartClient(string lobbyId) { return Task.FromResult(Result.None); } 
        
        public virtual void OnUserRequestedShutdown() { }
        
        public virtual void OnTransportFailure() { }
        
        public virtual void OnLobbyCreated(Result result, Lobby lobby) { }
        
        public virtual void OnLobbyEntered(Lobby lobby) { }
        
        public virtual void GameLobbyJoinRequested(Lobby lobby, SteamId steamId) { }
    } 
}


