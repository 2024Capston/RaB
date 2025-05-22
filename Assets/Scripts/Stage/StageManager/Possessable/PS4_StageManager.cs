using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Possessable
{
    public class PS4_StageManager : StageManager
    {
        // Left Plate: EventA -> EventB (O) -> EventC (C)
        // Right Plate: EventD -> EventE (O) -> EventF (C)

        // Center Plate: EventG, EventH, EventI, EventJ -> EventK (O) -> EventL (O)
        //               EventM, EventN, EventO, EventP (M)

        // Platform: EventQ

        // Left Clear: EventR -> EventS (O) -> EventT (C)
        // Right Clear: EventU

        [SerializeField] private AudioSource _clearAudiouSource;

        private bool _isLeftDoorOpen, _isRightDoorOpen;
        private bool _isFirstPlatePressed, _isSecondPlatePressed, _isThirdPlatePressed, _isFourthPlatePressed, _isDoorOpen;
        private bool _isClearLeftPlatePressed, _isClearRightPlatePressed, _isClearDoorOpen;

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
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventA, OnLeftPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventD, OnRightPlatePressed);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventG, OnFirstPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventH, OnSecondPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventI, OnThirdPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventJ, OnFourthPlatePressed);

            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventQ, PlayClearSoundClientRpc);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventR, OnClearLeftPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventU, OnClearRightPlatePressed);
        }

        [ClientRpc(RequireOwnership = false)]
        private void PlayClearSoundClientRpc()
        {
            if (!_clearAudiouSource.isPlaying)
            {
                _clearAudiouSource.Play();
            }
        }

        private void CheckDoorCondition()
        {
            if (_isFirstPlatePressed && _isSecondPlatePressed && _isThirdPlatePressed && _isFourthPlatePressed)
            {
                if (!_isDoorOpen)
                {
                    _isDoorOpen = true;
                    PlayClearSoundClientRpc();

                    EventBus.Instance.InvokeEvent(EventType.EventK);
                }
            }
            else
            {
                _isDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventL);
            }
        }

        private void CheckClearCondition()
        {
            if (_isClearLeftPlatePressed && _isClearRightPlatePressed)
            {
                if (!_isClearDoorOpen)
                {
                    _isClearDoorOpen = true;
                    PlayClearSoundClientRpc();

                    EventBus.Instance.InvokeEvent(EventType.EventS);
                }
            }
            else
            {
                _isClearDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventT);
            }
        }

        public void OnLeftPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (!_isLeftDoorOpen && plateController.ObjectsOnPlate.Count > 0)
            {
                PlayClearSoundClientRpc();

                _isLeftDoorOpen = true;
                EventBus.Instance.InvokeEvent(EventType.EventB);
            }
            else if (_isLeftDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isLeftDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventC);
            }
        }

        public void OnRightPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (!_isRightDoorOpen && plateController.ObjectsOnPlate.Count > 0)
            {
                PlayClearSoundClientRpc();

                _isRightDoorOpen = true;
                EventBus.Instance.InvokeEvent(EventType.EventE);
            }
            else if (_isRightDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isRightDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventF);
            }
        }

        public void OnFirstPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isFirstPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isFirstPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventM, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventM, MonitorType.XMark);
            }

            CheckDoorCondition();
        }

        public void OnSecondPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isSecondPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isSecondPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventN, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventN, MonitorType.XMark);
            }

            CheckDoorCondition();
        }

        public void OnThirdPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isThirdPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isThirdPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventO, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventO, MonitorType.XMark);
            }

            CheckDoorCondition();
        }

        public void OnFourthPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isFourthPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isFourthPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventP, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventP, MonitorType.XMark);
            }

            CheckDoorCondition();
        }

        public void OnClearLeftPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isClearLeftPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            CheckClearCondition();
        }

        public void OnClearRightPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isClearRightPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            CheckClearCondition();
        }
    }
}