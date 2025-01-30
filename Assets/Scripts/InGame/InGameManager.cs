using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class InGameManager : NetworkSingletonBehaviour<InGameManager>
{
    private Transform _blueSpawnPoint;
    private Transform _redSpawnPoint;
    
    [SerializeField] private GameObject[] _playerPrefabs = new GameObject[2];
    
    public StageLoader StageLoader { get; set; }
    
    protected override void Init()
    {
        _isDestroyOnLoad = true;
        base.Init();

        StageLoader = null;
    }

    private void Start()
    {
        if (!IsHost)
        {
            return;
        }

        _redSpawnPoint = null;
        _blueSpawnPoint = null;

        try
        {
            // 시작한 스테이지를 가져온다.
            StageName stageName = SessionManager.Instance.SelectedStage;
            if (stageName == StageName.Size)    
            {
                throw new Exception("StageName MissMatch");
            }   

            // 해당 스테이지의 Loader를 생성한다.
            StageLoadManager.Instance.LoadStage(stageName);
        }
        catch (Exception e)
        {
            StageLoadFailed(e);
        }
        
    }

    public void StartGame()
    {
        // SpawnPoint Tag를 찾느다.
        try
        {
            FindSpawnPoint();

            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                SpawnPlayer(clientId);
            }
        }
        catch (Exception e)
        {
            DestoryAllObjects();
            StageLoadFailed(e);
        }
        UIManager.Instance.CloseAllOpenUI();
        // StageManager.Instance.StartGame();

        StartCoroutine(CoTest());
    }

    private void FindSpawnPoint()
    {
        _blueSpawnPoint = GameObject.FindGameObjectWithTag("BlueSpawnPoint").transform;
        _redSpawnPoint = GameObject.FindGameObjectWithTag("RedSpawnPoint").transform;
        if (!_blueSpawnPoint)
        {
            throw new Exception("BlueSpawnPoint has not exist");
        }
        if (!_redSpawnPoint)
        {
            throw new Exception("RedSpawnPoint has not exist");
        }
    }
    
    private void SpawnPlayer(ulong clinetId)
    {
        PlayerConfig playerConfig = NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(clinetId).GetComponent<PlayerConfig>();
        int isBlue = playerConfig.IsBlue ? 1 : 0;
        GameObject player = Instantiate(_playerPrefabs[isBlue]);
        
        player.GetComponent<NetworkObject>().SpawnWithOwnership(clinetId);
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerConfig.MyPlayer = playerController;
        //playerController.PlayerColor = playerConfig.IsBlue ? ColorType.Blue : ColorType.Red;
    }

    private IEnumerator CoTest()
    {
        yield return new WaitForSeconds(5f);
        EndGameServerRpc();
    }
    
    /// <summary>
    /// 게임이 종료되었으면 LobbyScene으로 되돌아간다.
    /// </summary>
    [ServerRpc]
    public void EndGameServerRpc()
    {
        // TODO Clear 했을 때 GameData를 업데이트 해야 한다.
        
        DestoryAllObjects();
        SceneLoaderWrapper.Instance.LoadScene(SceneType.Lobby.ToString(), true);
    }

    private void DestoryAllObjects()
    {
        foreach (ulong playerId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            PlayerConfig playerConfig = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<PlayerConfig>();
            playerConfig.MyPlayer.GetComponent<NetworkObject>().Despawn();
        }
        
        StageLoader.DestoryStage();
        StageLoader.GetComponent<NetworkObject>().Despawn();
    }

    /// <summary>
    /// StageLoad를 실패했을 경우 Lobby Scene으로 되돌아갑니다.
    /// </summary>
    private void StageLoadFailed(Exception e)
    {
        Logger.LogError($"StageLoad Failed\n{e}");
        Logger.Log($"Scene Transition {SceneManager.GetActiveScene().name} to {SceneType.Lobby}");
        SceneLoaderWrapper.Instance.LoadScene(SceneType.Lobby.ToString(), true);
    }
}
