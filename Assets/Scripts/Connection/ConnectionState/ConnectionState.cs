using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace RaB.Connection
{
    internal abstract class ConnectionState
    {
        public abstract void Enter();

        public abstract void Exit();
        
        public virtual void OnClientConnected(ulong clientId) { }
        
        public virtual void OnClientDisconnect(ulong clientId) { }
        
        public virtual void OnServerStarted() { }
        
        public virtual void OnServerStopped() { }
        
        public virtual void StartServer() { }
        
        public virtual void StartClient(string lobbyId, out Result result) { result = Result.None; }
        
        public virtual void OnUserRequestedShutdown() { }
        
        public virtual void OnTransportFailure() { }
        
        public virtual void OnLobbyCreated(Result result, Lobby lobby) { }
        
        public virtual void OnLobbyEntered(Lobby lobby) { }
        
        public virtual void GameLobbyJoinRequested(Lobby lobby, SteamId steamId) { }
    } 
}


