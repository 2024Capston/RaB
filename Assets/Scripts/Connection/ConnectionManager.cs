using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace RaB.Connection
{
    public class ConnectionManager : SingletonBehavior<ConnectionManager>
    {
        private ConnectionState _currentState;
        private void Start()
        {
        }

        private void OnDestroy()
        {
            
        }
        
        /// <summary>
        /// 현재 State를 나가고 nextState에 들어간다. 
        /// </summary>
        /// <param name="nextState"></param>
        internal void ChangeState(ConnectionState nextState)
        {
            Debug.Log($"{name}: Changed connection state from {_currentState.GetType().Name} to {nextState.GetType().Name}.");

            if (_currentState != null)
            {
                _currentState.Exit();
            }

            _currentState = nextState;
            _currentState.Enter();
        }

        private void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
        {
            if (connectionEventData.EventType == ConnectionEvent.ClientConnected)
            {
                OnClientConnectedCallback(connectionEventData.ClientId);
            }
            else if (connectionEventData.EventType == ConnectionEvent.ClientDisconnected)
            {
                OnClientDisconnetCallback(connectionEventData.ClientId);
            }
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            _currentState.OnClientConnected(clientId);
        }

        private void OnClientDisconnetCallback(ulong clientId)
        {
            _currentState.OnClientDisconnect(clientId);
        }  

        private void OnServerStarted()
        {
            _currentState.OnServerStarted();
        }

        private void OnServerStopped(bool _)
        {
            _currentState.OnServerStopped();
        }

        private void ApprovalCheck()
        {
            _currentState.ApprovalCheck();
        }

        private void OnTransportFailure()
        {
            _currentState.OnTransportFailure();
        }

        public void StartServer()
        {
            _currentState.StartServer();
        }

        public void StartClient()
        {
            _currentState.StartCleint();
        }

        public void RequestShutdown()
        {
            _currentState.OnUserRequestedShutdown();
        }
    }
}

