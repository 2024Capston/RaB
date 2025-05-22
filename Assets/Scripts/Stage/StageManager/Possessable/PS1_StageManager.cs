using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Possessable
{
    public class PS1_StageManager : StageManager
    {
        // Open the door for tall: EventA -> EventB (O) -> EventN (C)
        // Open the door for wide: EventC -> EventD (O) -> EventO (C)

        // Open the return door: EventE -> EventG (M) -> EventJ, EventK (M)
        //                       EventH -> EventI (M) /^
        // Open the clear door: (EventL, EventP) -> EventM (O) -> EventQ (C)

        [SerializeField] private AudioSource _clearAudiouSource;

        private bool _isTallDoorOpen, _isWideDoorOpen;
        private bool _isBlueButtonPressed, _isRedButtonPressed;
        private bool _leftPlatePressed, _rightPlatePressed, _isClearDoorOpen;

        public override void EndGame()
        {
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
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventA, OnPlatePressedForTallPossessable);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventC, OnPlatePressedForWidePossessable);

            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventE, OnBlueButtonPressed);
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventH, OnRedButtonPressed);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventL, OnLeftClearPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventP, OnRightClearPlatePressed);
        }

        [ClientRpc(RequireOwnership = false)]
        private void PlayClearSoundClientRpc()
        {
            if (!_clearAudiouSource.isPlaying)
            {
                _clearAudiouSource.Play();
            }
        }

        private void CheckClearCondition()
        {
            if (_leftPlatePressed && _rightPlatePressed)
            {
                if (!_isClearDoorOpen)
                {
                    _isClearDoorOpen = true;
                    PlayClearSoundClientRpc();

                    EventBus.Instance.InvokeEvent(EventType.EventM);
                }
            }
            else
            {
                _isClearDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventQ);
            }
        }

        public void OnPlatePressedForTallPossessable(PlateController plateController, GameObject objectOnPlate)
        {
            if (plateController.ObjectsOnPlate.Count > 0)
            {
                if (!_isTallDoorOpen)
                {
                    _isTallDoorOpen = true;
                    PlayClearSoundClientRpc();

                    EventBus.Instance.InvokeEvent(EventType.EventB);
                }
            }
            else
            {
                _isTallDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventN);
            }
        }

        public void OnPlatePressedForWidePossessable(PlateController plateController, GameObject objectOnPlate)
        {
            if (plateController.ObjectsOnPlate.Count > 0)
            {
                if (!_isWideDoorOpen)
                {
                    _isWideDoorOpen = true;
                    PlayClearSoundClientRpc();

                    EventBus.Instance.InvokeEvent(EventType.EventD);
                }
            }
            else
            {
                _isWideDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventO);
            }
        }

        public void OnBlueButtonPressed()
        {
            _isBlueButtonPressed = true;
            EventBus.Instance.InvokeEvent(EventType.EventG, MonitorType.CheckMark);

            if (_isRedButtonPressed)
            {
                PlayClearSoundClientRpc();

                EventBus.Instance.InvokeEvent(EventType.EventJ);
                EventBus.Instance.InvokeEvent(EventType.EventK, MonitorType.CheckMark);
            }
        }

        public void OnRedButtonPressed()
        {
            _isRedButtonPressed = true;
            EventBus.Instance.InvokeEvent(EventType.EventI, MonitorType.CheckMark);

            if (_isBlueButtonPressed)
            {
                PlayClearSoundClientRpc();

                EventBus.Instance.InvokeEvent(EventType.EventJ);
                EventBus.Instance.InvokeEvent(EventType.EventK, MonitorType.CheckMark);
            }
        }

        public void OnLeftClearPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _leftPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            CheckClearCondition();
        }

        public void OnRightClearPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _rightPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            CheckClearCondition();
        }
    }
}