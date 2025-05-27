using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ColorChanger
{
    // LeftPlate: EventA -> EventB (O) -> EventC (C);
    // Button: EventD
    // RightPlate: EventE -> EventF (O) -> EventG (C);

    // ClearPlate: EventH, EventI, EventJ -> EventN (O) -> EventO (P);
    //             EventK, EventL, EventM

    public class CC3_StageManager : StageManager
    {
        [SerializeField] private AudioSource _clearAudiouSource;

        private bool _isLeftDoorOpen, _isRightDoorOpen;
        private bool _isFirstPlatePressed, _isSecondPlatePressed, _isThirdPlatePressed, _isClearDoorOpen;

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
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventE, OnRightPlatePressed);

            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventD, OnButtonPressed);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventH, OnFirstPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventI, OnSecondPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventJ, OnThirdPlatePressed);
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
            if (_isFirstPlatePressed && _isSecondPlatePressed && _isThirdPlatePressed)
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
                EventBus.Instance.InvokeEvent(EventType.EventB);
            }
            else if (_isLeftDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isLeftDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventC);
            }
        }

        public void OnButtonPressed()
        {
            PlayClearSoundClientRpc();
        }

        public void OnRightPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (!_isRightDoorOpen && plateController.ObjectsOnPlate.Count > 0)
            {
                PlayClearSoundClientRpc();

                _isRightDoorOpen = true;
                EventBus.Instance.InvokeEvent(EventType.EventF);
            }
            else if (_isRightDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isRightDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventG);
            }
        }

        public void OnFirstPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isFirstPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isFirstPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventK, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventK, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnSecondPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isSecondPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isSecondPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventL, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventL, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnThirdPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isThirdPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isThirdPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventM, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventM, MonitorType.XMark);
            }

            CheckClearCondition();
        }
    }
}