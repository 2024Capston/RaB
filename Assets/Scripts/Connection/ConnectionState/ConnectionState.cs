using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaB.Connection
{
    public abstract class ConnectionState
    {
        protected ConnectionManager _connectionManager;

        public abstract void Enter();

        public abstract void Exit();
        
        public virtual void OnClientConnected(ulong clientId) { }
        
        public virtual void OnClientDisconnect(ulong clientId) { }
        
        public virtual void OnServerStarted() { }
        
        public virtual void OnServerStopped() { }
        
        public virtual void StartServer() { }
        
        public virtual void StartCleint() { }
        
        public virtual void OnUserRequestedShutdown() { }
        
        public virtual void ApprovalCheck() { }
        
        public virtual void OnTransportFailure() { }
    } 
}


