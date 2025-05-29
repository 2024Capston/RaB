using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ColorChanger
{
    public class CC5_StageManager : StageManager
    {
        // FirstPlate: EventA -> EventB (ON) -> EventC (OFF)
        // SubPlate:   EventS
        // SecondPlate: EventD -> EventE (O) -> EventF (C)

        // ThirdPlate: EventG -> EventH (ON) -> EventI (OFF)
        // FourthPlate: EventJ -> EventK (ON) -> EventL (OFF)

        // ClearPlate: EventM, EventN -> EventO (O) -> EventP (C)
        //             EventQ, EventR (M)

        [SerializeField] private AudioSource _clearAudiouSource;

        private bool _isFirstPlatePressed, _isSubPlatePressed;
        private bool _isSecondDoorOpen;
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
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventS, OnSubPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventD, OnSecondPlatePressed);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventG, OnThirdPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventJ, OnFourthPlatePressed);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventM, OnLeftPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventN, OnRightPlatePressed);
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
            if (_isLeftPlatePressed && _isRightPlatePressed)
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

        public void OnFirstPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isFirstPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isFirstPlatePressed || _isSubPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventB);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventC);
            }
        }

        public void OnSubPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isSubPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isFirstPlatePressed || _isSubPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventB);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventC);
            }
        }

        public void OnSecondPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (!_isSecondDoorOpen && plateController.ObjectsOnPlate.Count > 0)
            {
                PlayClearSoundClientRpc();

                _isSecondDoorOpen = true;
                EventBus.Instance.InvokeEvent(EventType.EventE);
            }
            else if (_isSecondDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isSecondDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventF);
            }
        }

        public void OnThirdPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (plateController.ObjectsOnPlate.Count > 0)
            {
                EventBus.Instance.InvokeEvent(EventType.EventH);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventI);
            }
        }

        public void OnFourthPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (plateController.ObjectsOnPlate.Count > 0)
            {
                EventBus.Instance.InvokeEvent(EventType.EventK);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventL);
            }
        }

        public void OnLeftPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isLeftPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isLeftPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventQ, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventQ, MonitorType.XMark);
            }

            CheckClearCondition();
        }

        public void OnRightPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            _isRightPlatePressed = plateController.ObjectsOnPlate.Count > 0;

            if (_isRightPlatePressed)
            {
                EventBus.Instance.InvokeEvent(EventType.EventR, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventR, MonitorType.XMark);
            }

            CheckClearCondition();
        }
    }
}