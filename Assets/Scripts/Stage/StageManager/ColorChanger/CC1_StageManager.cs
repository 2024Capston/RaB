using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ColorChanger
{
    public class CC1_StageManager : StageManager
    {
        // LeftPlate: EventA -> EventB (M) -> EventE (O)
        // RightPlate: EventC -> EventD (M)

        // FirstPlate: EventF, EventG, EventH, EventI -> EventN (O) -> EventO (C)
        //             EventJ, EventK, EventL, EventM (M)

        [SerializeField] private AudioSource _clearAudiouSource;

        private bool _isLeftPlatePressed, _isRightPlatePressed;
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
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventA, OnLeftPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventC, OnRightPlatePressed);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventF, OnFirstPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventG, OnSecondPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventH, OnThirdPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventI, OnFourthPlatePressed);
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

                    EventBus.Instance.InvokeEvent(EventType.EventN);
                }
            }
            else
            {
                _isClearDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventO);
            }
        }

        public void OnLeftPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isLeftPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isLeftPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventD, MonitorType.CheckMark);

                if (_isRightPlatePressed)
                {
                    PlayClearSoundClientRpc();
                    EventBus.Instance.InvokeEvent(EventType.EventE);
                }
            }
        }

        public void OnRightPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isRightPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isRightPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventB, MonitorType.CheckMark);

                if (_isLeftPlatePressed)
                {
                    PlayClearSoundClientRpc();
                    EventBus.Instance.InvokeEvent(EventType.EventE);
                }
            }
        }

        public void OnFirstPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isFirstPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isFirstPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventJ, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventJ, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnSecondPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isSecondPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isSecondPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventK, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventK, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnThirdPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isThirdPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isThirdPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventL, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventL, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnFourthPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isFourthPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isFourthPlatePressed)
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