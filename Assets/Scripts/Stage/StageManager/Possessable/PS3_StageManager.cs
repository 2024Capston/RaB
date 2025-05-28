using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Possessable
{
    public class PS3_StageManager : StageManager
    {
        // Plate 1, Open Door A: EventA -> EventB (O) -> EventC (C), EventD (M)
        // Plate 2: EventE -> EventG (M) -> EventI (O) -> EventJ (C)
        // Plate 3: EventF -> EventH (M)

        // Button 1: EventK
        // Button 2: EventL * 2 -> EventM (O)

        // Left Clear Plate: EventN -> EventP (O) -> EventQ (C)
        // Rgiht Clear Plate: EventO


        [SerializeField] private AudioSource _clearAudiouSource;

        private bool _isFirstPlatePressed, _isSecondPlatePressed, _isThirdPlatePressed;
        private bool _isFirstDoorOpen, _isSecondDoorOpen;
        private int _buttonPressCount = 0;
        private bool _isLeftPlatePressed, _isRightPlatePressed, _isClearDoorOpen;

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
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventA, OnFirstPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventE, OnSecondPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventF, OnThirdPlatePressed);

            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventK, PlayClearSoundClientRpc);
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventL, OnButtonPairPressed);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventN, OnLeftPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventO, OnRightPlatePressed);
        }

        [ClientRpc(RequireOwnership = false)]
        private void PlayClearSoundClientRpc()
        {
            if (!_clearAudiouSource.isPlaying)
            {
                _clearAudiouSource.Play();
            }
        }

        private bool CheckSecondDoorCondition()
        {
            if (_isFirstPlatePressed && _isSecondPlatePressed && _isThirdPlatePressed)
            {
                if (!_isSecondDoorOpen)
                {
                    _isSecondDoorOpen = true;
                    PlayClearSoundClientRpc();

                    EventBus.Instance.InvokeEvent(EventType.EventI);

                    return true;
                }
            }
            else
            {
                _isSecondDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventJ);
            }

            return false;
        }

        private void CheckClearCondition()
        {
            if (_isLeftPlatePressed && _isRightPlatePressed)
            {
                if (!_isClearDoorOpen)
                {
                    _isClearDoorOpen = true;
                    PlayClearSoundClientRpc();

                    EventBus.Instance.InvokeEvent(EventType.EventP);
                }
            }
            else
            {
                _isClearDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventQ);
            }
        }

        public void OnFirstPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isFirstPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            bool playClearSound = CheckSecondDoorCondition();

            if (_isFirstPlatePressed)
            {
                if (!_isFirstDoorOpen)
                {
                    playClearSound |= true;
                    _isFirstDoorOpen = true;
                }

                EventBus.Instance.InvokeEvent(EventType.EventB);
                EventBus.Instance.InvokeEvent(EventType.EventD, MonitorType.CheckMark);
            }
            else
            {
                _isFirstDoorOpen = false;

                EventBus.Instance.InvokeEvent(EventType.EventC);
                EventBus.Instance.InvokeEvent(EventType.EventD, MonitorType.XMark);
            }

            if (playClearSound)
            {
                PlayClearSoundClientRpc();
            }
        }

        public void OnSecondPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isSecondPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isSecondPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventG, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventG, MonitorType.XMark);
            }

            if (CheckSecondDoorCondition())
            {
                PlayClearSoundClientRpc();
            }
        }

        public void OnThirdPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isThirdPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isThirdPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventH, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventH, MonitorType.XMark);
            }

            if (CheckSecondDoorCondition())
            {
                PlayClearSoundClientRpc();
            }
        }

        public void OnButtonPairPressed()
        {
            if (++_buttonPressCount == 2)
            {
                PlayClearSoundClientRpc();
                EventBus.Instance.InvokeEvent(EventType.EventM);
            }
        }

        public void OnLeftPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isLeftPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            CheckClearCondition();
        }

        public void OnRightPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isRightPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            CheckClearCondition();
        }
    }
}