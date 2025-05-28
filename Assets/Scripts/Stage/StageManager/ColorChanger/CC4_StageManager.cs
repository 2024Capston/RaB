using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ColorChanger
{
    public class CC4_StageManager : StageManager
    {
        // Trigger1: EventA -> EventB (O)
        // Trigger2: EventC -> EventD (O)
        // Trigger3: EventE -> EventF (O)
        // EventS, EventT, EventU (M)

        // LeftDoor: EventG -> EventH (O) -> EventI (C)
        // RightDoor: EventJ -> EventK (O) -> EventL (C)

        // InnerDoor: EventM -> EventN (O) -> EventO (C)

        // ClearDoor: EventP -> EventQ (O) -> EventR (C)

        [SerializeField] private AudioSource _clearAudiouSource;

        private bool _isFirstTriggerEntered, _isSecondTriggerEntered, _isThirdTriggerEntered;
        private bool _isLeftDoorOpen, _isRightDoorOpen, _isInnerDoorOpen, _isClearDoorOpen;

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
            EventBus.Instance.SubscribeEvent<UnityAction<GameObject>>(EventType.EventA, OnFirstTriggerEntered);
            EventBus.Instance.SubscribeEvent<UnityAction<GameObject>>(EventType.EventC, OnSecondTriggerEntered);
            EventBus.Instance.SubscribeEvent<UnityAction<GameObject>>(EventType.EventE, OnThirdTriggerEntered);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventG, OnLeftPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventJ, OnRightPlatePressed);

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventM, OnInnerPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventP, OnClearPlatePressed);
        }

        [ClientRpc(RequireOwnership = false)]
        private void PlayClearSoundClientRpc()
        {
            if (!_clearAudiouSource.isPlaying)
            {
                _clearAudiouSource.Play();
            }
        }

        public void OnFirstTriggerEntered(GameObject gameObject)
        {
            if (!_isFirstTriggerEntered && gameObject.GetComponent<CubeController>())
            {
                _isFirstTriggerEntered = true;

                PlayClearSoundClientRpc();

                EventBus.Instance.InvokeEvent(EventType.EventB);
                EventBus.Instance.InvokeEvent(EventType.EventS, MonitorType.CheckMark);
            }
        }

        public void OnSecondTriggerEntered(GameObject gameObject)
        {
            if (!_isSecondTriggerEntered && gameObject.GetComponent<CubeController>())
            {
                _isSecondTriggerEntered = true;

                PlayClearSoundClientRpc();

                EventBus.Instance.InvokeEvent(EventType.EventD);
                EventBus.Instance.InvokeEvent(EventType.EventU, MonitorType.CheckMark);
            }
        }

        public void OnThirdTriggerEntered(GameObject gameObject)
        {
            if (!_isThirdTriggerEntered && gameObject.GetComponent<CubeController>())
            {
                _isThirdTriggerEntered = true;

                PlayClearSoundClientRpc();

                EventBus.Instance.InvokeEvent(EventType.EventF);
                EventBus.Instance.InvokeEvent(EventType.EventV, MonitorType.CheckMark);
            }
        }

        public void OnLeftPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (!_isLeftDoorOpen && plateController.ObjectsOnPlate.Count > 0)
            {
                PlayClearSoundClientRpc();

                _isLeftDoorOpen = true;
                EventBus.Instance.InvokeEvent(EventType.EventH);
            }
            else if (_isLeftDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isLeftDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventI);
            }
        }

        public void OnRightPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (!_isRightDoorOpen && plateController.ObjectsOnPlate.Count > 0)
            {
                PlayClearSoundClientRpc();

                _isRightDoorOpen = true;
                EventBus.Instance.InvokeEvent(EventType.EventK);
            }
            else if (_isRightDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isRightDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventL);
            }
        }

        public void OnInnerPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (!_isInnerDoorOpen && plateController.ObjectsOnPlate.Count > 0)
            {
                PlayClearSoundClientRpc();

                _isInnerDoorOpen = true;
                EventBus.Instance.InvokeEvent(EventType.EventN);
            }
            else if (_isInnerDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isInnerDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventO);
            }
        }

        public void OnClearPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (!_isClearDoorOpen && plateController.ObjectsOnPlate.Count > 0)
            {
                PlayClearSoundClientRpc();

                _isClearDoorOpen = true;
                EventBus.Instance.InvokeEvent(EventType.EventQ);
            }
            else if (_isClearDoorOpen && plateController.ObjectsOnPlate.Count == 0)
            {
                _isClearDoorOpen = false;
                EventBus.Instance.InvokeEvent(EventType.EventR);
            }
        }
    }
}