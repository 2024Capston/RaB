using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;

namespace TubeStage
{
    internal struct TubeStage2Problem
    {
        public List<int> Answer;
        public int StageCount => Answer.Count;
        public int CurrentCount;
        public int FailCount;
        public int MaxFailCount;
        public ColorType PlayerAnswer;
        public bool OnPlayerAnswered;
    }

    public class TubeStage2Manager : StageManager
    {
        public new static TubeStage2Manager Instance => StageManager.Instance as TubeStage2Manager;

        private TubeStageState _currentState;

        internal readonly WaitingState Waiting = new WaitingState();
        internal readonly AskingState Asking = new AskingState();
        internal readonly CheckingState Checking = new CheckingState();
        internal readonly ClearedState Cleared = new ClearedState();

        public TubeStage2Controller TubeStageController;
        internal TubeStage2Problem? Problem = null;

        private Coroutine _timerCoroutine;
        
        protected override void Init()
        {
            _isDestroyOnLoad = true;
            base.Init();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            EventBus.Instance.SubscribeEvent<UnityAction<ColorType>>(EventType.EventA, OnButtonClicked);
        }
        

        internal void ChangeState(TubeStageState nextState)
        {
            if (_currentState != null)
            {
                _currentState.Exit();
            }

            _currentState = nextState;
            _currentState.Enter();
        }

        public override void StartGame()
        {
            ChangeState(Waiting);
            TubeStageController.StartDoorOpen();
        }

        public override void RestartGame()
        {
            ChangeState(Waiting);
        }

        public override void EndGame()
        {
            InGameManager.Instance.EndGameServerRpc();
        }

        public void StartAskingCoroutine(float duration)
        {
            double startTime = NetworkManager.Singleton.ServerTime.Time;
            StartLocalTimerClientRpc(startTime, duration);
            _timerCoroutine = StartCoroutine(CoServerTimer(startTime, duration));
            TubeStageController.StartTicTacSFXClientRpc();
        }

        public void StartFailedCoroutine(float duration)
        {
            StartCoroutine(CoWaitTimer(duration));
        }

        public void StopCoroutine()
        {
            StopLocalTimerClientRpc();
            if (_timerCoroutine is not null)
            {
                StopCoroutine(_timerCoroutine);
            }
        }

        private IEnumerator CoServerTimer(double startTime, float duration)
        {
            double endTime = startTime + duration;

            while (NetworkManager.Singleton.ServerTime.Time < endTime)
            {
                yield return null;
            }
            
            _timerCoroutine = null;
            ChangeState(Checking);  
        }

        private IEnumerator CoWaitTimer(float duration)
        {
            yield return new WaitForSeconds(duration);
            ChangeState(Waiting);   
        }
        
        [ClientRpc]
        private void StartLocalTimerClientRpc(double startTime, float duration)
        {
            TubeStageController.StartLocalTimer(startTime + duration);
        }

        [ClientRpc]
        private void StopLocalTimerClientRpc()
        {
            TubeStageController.StopLocalTimer();
        }

        private void OnButtonClicked(ColorType colorType)
        {
            _currentState.OnButtonClicked(colorType);
        }
    }
}


