using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ColorWall
{ 
    public class CC17_StageManager : StageManager
    {
        int playerCount = 0;

        public void OnGoalEnter(GameObject other)
        {
            if (other.GetComponent<PlayerController>()) {
                playerCount++;
            };
            if (playerCount == 2)
            {
                EventBus.Instance.InvokeEvent(EventType.EventC);
            }
        }

        public void OnGoalExit(GameObject other)
        {
            if (other.GetComponent<PlayerController>()) {
                playerCount--;
            };
            if (playerCount == 0)
            {
                EventBus.Instance.InvokeEvent(EventType.EventD);
            }
        }
        public override void EndGame()
        {
            EventBus.Instance.ClearEventBus();
            InGameManager.Instance.EndGameServerRpc();
        }

        public override void RestartGame()
        {
            EventBus.Instance.ClearEventBus();

            foreach (PlayerController playerController in FindObjectsOfType<PlayerController>())
            {
                playerController.RespawnPlayer();
                playerController.ForceStopInteraction();
            }

            foreach (NetworkObjectSpawner networkObjectSpawner in FindObjectsOfType<NetworkObjectSpawner>())
            {
                networkObjectSpawner.SpawnObject();
            }

            StartGame();
        }

        public override void StartGame()
        {
            EventBus.Instance.ClearEventBus();
            EventBus.Instance.SubscribeEvent<UnityAction<GameObject>>(EventType.EventA, OnGoalEnter);
            EventBus.Instance.SubscribeEvent<UnityAction<GameObject>>(EventType.EventB, OnGoalExit);
        }

    }
}