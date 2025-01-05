using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

namespace RaB.Connection
{
    public class ConnectionManager : SingletonBehavior<ConnectionManager>
    {
        private ConnectionState _currentState;

        internal readonly OfflineState Offline = new OfflineState();
        internal readonly StartingHostState StartingHost = new StartingHostState();
        internal readonly HostingState Hosting = new HostingState();
        internal readonly ClientConnectingState ClientConnecting = new ClientConnectingState();
        internal readonly ClientConnectedState ClientConnected = new ClientConnectedState();
        
        public Lobby? CurrentLobby { get; internal set; }

        private void Start()
        {
            CurrentLobby = null;

            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
            NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
        }

        private void OnDestroy()
        {
            CurrentLobby = null;
            
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
            NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
            NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
        }

        public void StartServer()
        {
            _currentState.StartServer();
        }

        public void StartClient(string lobbyId, out Result result)
        {
            _currentState.StartClient(lobbyId, out result);
        }

        public void RequestShutdown()
        {
            _currentState.OnUserRequestedShutdown();
        }

        /// <summary>
        /// 현재 State를 나가고 nextState에 들어간다. 
        /// </summary>
        /// <param name="nextState"></param>
        internal void ChangeState(ConnectionState nextState)
        {
            Debug.Log(
                $"{name}: Changed connection state from {_currentState.GetType().Name} to {nextState.GetType().Name}.");

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

        private void OnTransportFailure()
        {
            _currentState.OnTransportFailure();
        }

        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            _currentState.OnLobbyCreated(result, lobby);
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            _currentState.OnLobbyEntered(lobby);
        }

        private void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
        {
            _currentState.GameLobbyJoinRequested(lobby, steamId);
        }
    }
}

