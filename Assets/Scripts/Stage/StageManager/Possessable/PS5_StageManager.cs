using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Possessable
{
    public class PS5_StageManager : StageManager
    {
        // Left Door: EventA
        // Right Door: EventB

        // Left Door: EventC
        // Right Door: EventD

        // Left Plate: EventE -> EventF (O) -> EventG (C)
        // Right Plate: EventH -> EventI (O) -> EventJ (C)

        // Clear Plate: EventK, EventL, EventM, EventN -> EventO (O) -> EventP (C)
        //              EventQ, EventR, EventS, EventT (M)

        [SerializeField] private AudioSource _clearAudiouSource;

        private bool _isLeftDoorOpen, _isRightDoorOpen;
        private bool _isFirstPlatePressed, _isSecondPlatePressed, _isThirdPlatePressed, _isFourthPlatePressed, _isClearDoorOpen;

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
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventA, PlayClearSoundClientRpc);
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventB, PlayClearSoundClientRpc);
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventC, PlayClearSoundClientRpc);
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventD, PlayClearSoundClientRpc);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventE, OnLeftPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventH, OnRightPlatePressed);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventK, OnFirstPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventL, OnSecondPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventM, OnThirdPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventN, OnFourthPlatePressed);
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
            if (_isFirstPlatePressed && _isSecondPlatePressed && _isThirdPlatePressed && _isFourthPlatePressed)
            {
                if (!_isClearDoorOpen)
                {
                    _isClearDoorOpen = true;
                    PlayClearSoundClientRpc();

                    EventBus.Instance.InvokeEvent(EventType.EventO);
                }
            }
            else
            {
                _isClearDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventP);
            }
        }

        public void OnLeftPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (!_isLeftDoorOpen && plateController.ObjectsOnPlate.Count > 0)
            {
                PlayClearSoundClientRpc();

                _isLeftDoorOpen = true;
                EventBus.Instance.InvokeEvent(EventType.EventF);
            }
            else if (_isLeftDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isLeftDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventG);
            }
        }

        public void OnRightPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (!_isLeftDoorOpen && plateController.ObjectsOnPlate.Count > 0)
            {
                PlayClearSoundClientRpc();

                _isLeftDoorOpen = true;
                EventBus.Instance.InvokeEvent(EventType.EventI);
            }
            else if (_isLeftDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isLeftDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventJ);
            }
        }

        public void OnFirstPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isFirstPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isFirstPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventQ, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventQ, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnSecondPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isSecondPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isSecondPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventR, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventR, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnThirdPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isThirdPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isThirdPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventS, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventS, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnFourthPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isFourthPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isFourthPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventT, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventT, MonitorType.XMark);
            }

            CheckClearCondition();
        }
    }
}