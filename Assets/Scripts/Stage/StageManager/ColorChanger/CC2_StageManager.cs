using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ColorChanger
{
    public class CC2_StageManager : StageManager
    {
        // Button: EventA (O) -> EventB (C)
        //         EventC (LC), EventD (M), EventE (E), EventP (D)

        // FirstPlate: EventF, EventG, EventH, EventI -> EventN (O) -> EventO (C)
        //             EventJ, EventK, EventL, EventM (M)
        //             EventQ, EventR (B)

        [SerializeField] private AudioSource _clearAudiouSource;

        private bool _isRoomEntered;
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
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventA, OnDoorOpen);
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventB, OnDoorClose);

            EventBus.Instance.SubscribeEvent<UnityAction<GameObject>>(EventType.EventE, OnRoomEntered);

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

        public void OnDoorOpen()
        {
            EventBus.Instance.InvokeEvent(EventType.EventC, true, ColorType.None);
        }

        public void OnDoorClose()
        {
            EventBus.Instance.InvokeEvent(EventType.EventC, false, ColorType.None);
        }

        public void OnRoomEntered(GameObject other)
        {
            if (!_isRoomEntered && other.TryGetComponent(out CubeController cubeController))
            {
                _isRoomEntered = true;

                PlayClearSoundClientRpc();

                EventBus.Instance.InvokeEvent(EventType.EventP);
                EventBus.Instance.InvokeEvent(EventType.EventC, true, ColorType.None);
                EventBus.Instance.InvokeEvent(EventType.EventD, MonitorType.CheckMark);
            }
        }

        private void CheckClearCondition()
        {
            if (_isFirstPlatePressed && _isSecondPlatePressed && _isThirdPlatePressed && _isFourthPlatePressed)
            {
                if (!_isClearDoorOpen)
                {
                    Debug.Log("CLEARDS");
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
                EventBus.Instance.InvokeEvent(EventType.EventQ);
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
                EventBus.Instance.InvokeEvent(EventType.EventR);
            }
            else
            {
                EventBus.Instance.InvokeEvent(EventType.EventM, MonitorType.XMark);
            }

            CheckClearCondition();
        }
    }
}