using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TubeStage
{
    internal struct TubeStage2Problem
    {
        public List<int> Answer;
        public int StageCount => Answer.Count;
        public int CurrentCount;
        public int FailCount; 
    }
    
    public class TubeStage2Manager : StageManager
    {
        private TubeStageState _currentState;

        internal readonly WaitingState Waiting = new WaitingState();
        internal readonly AskingState Asking = new AskingState();
        internal readonly CheckingState Checking = new CheckingState();
        internal readonly ClearedState Cleared = new ClearedState();

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
        }

        public override void RestartGame()
        {
            ChangeState(Waiting);
        }

        public override void EndGame()
        {
            
        }

        private void OnButtonClicked(ColorType colorType)
        {
            _currentState.OnButtonClicked(colorType);
        }
    }
}


