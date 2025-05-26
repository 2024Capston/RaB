using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Possessable
{
    public class PS2_StageManager : StageManager
    {
        // Door for left room: EventA (O)
        // Door for right room: EventB (O)
        // Clear Door: EventC, EventD, EventE, EventK -> EventF (O) -> EventG (C)
        //             EventH, EventI, EventJ, EventL (M)

        [SerializeField] private AudioSource _clearAudiouSource;

        private bool _isLeftPlatePressed, _isCenterPlatePressed, _isRightPlatePressed, _isAdditionalPlatePressed, _isClearDoorOpen;

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

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventC, OnLeftPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventD, OnCenterPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventE, OnRightPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventK, OnAdditionalPlatePressed);
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
            if (_isLeftPlatePressed && _isCenterPlatePressed && _isRightPlatePressed && _isAdditionalPlatePressed)
            {
                if (!_isClearDoorOpen)
                {
                    _isClearDoorOpen = true;
                    PlayClearSoundClientRpc();

                    EventBus.Instance.InvokeEvent(EventType.EventF);
                }
            }
            else
            {
                _isClearDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventG);
            }
        }

        public void OnLeftPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isLeftPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isLeftPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventH, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventH, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnCenterPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isCenterPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isCenterPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventI, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventI, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnRightPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isRightPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isRightPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventJ, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventJ, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnAdditionalPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isAdditionalPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isAdditionalPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventL, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventL, MonitorType.XMark);
            }

            CheckClearCondition();
        }
    }
}