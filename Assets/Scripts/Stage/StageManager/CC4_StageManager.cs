using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ColorChanger
{ 
    public class CC4_StageManager : StageManager
    {
        [SerializeField] AudioSource _audioSource;

        public override void EndGame()
        {
            InGameManager.Instance.EndGameServerRpc();
        }

        public override void RestartGame()
        {
        }

        public override void StartGame()
        {
            EventBus.Instance.SubscribeEvent<UnityAction<GameObject>>(EventType.EventA, OnRoomEntered);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventC, OnPlatePressed);
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventI, PlayClearSound);

        }

        public void OnRoomEntered(GameObject other)
        {
            if (other.GetComponent<CubeController>())
            {
                EventBus.Instance.InvokeEvent(EventType.EventB);
                EventBus.Instance.InvokeEvent(EventType.EventH, MonitorType.CheckMark);

                PlayClearSoundClientRpc();
            }
        }

        public void OnPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (objectOnPlate.GetComponent<CubeController>())
            {
                EventBus.Instance.InvokeEvent(EventType.EventD);
                PlayClearSoundClientRpc();
            }
        }

        public void PlayClearSound()
        {
            PlayClearSoundClientRpc();
        }

        [ClientRpc(RequireOwnership = false)]
        private void PlayClearSoundClientRpc()
        {
            _audioSource.PlayOneShot(_audioSource.clip, _audioSource.volume);
        }
    }
}