using UnityEngine;

namespace TubeStage.TubeStage1
{
    /// <summary>
    /// 문제를 맵에 출력하고 Player의 입력을 기다리고 있는 State
    /// Player가 입력을 하거나, 제한시간이 지나면 CheckingState로 넘어간다.
    /// </summary>
    internal class AskingState : TubeStageState
    {
        private TubeStage1Problem _problem;
        
        /// <summary>
        /// Asking State에 진입 시 새로운 문제를 냅니다.
        /// </summary>
        public override void Enter()
        {
            if (TubeStage1Manager.Instance.Problem is null)
            {
                Logger.LogError("Problem does not exist");
                TubeStage1Manager.Instance.ChangeState(TubeStage1Manager.Instance.Waiting);
                return;
            }

            _problem = TubeStage1Manager.Instance.Problem.Value;
            _problem.OnPlayerAnswered = false;
            
            // 현재 Answer를 Stage에 적용한다.
            TubeStage1Manager.Instance.TubeStageController.SetProblemInMonitor(_problem.CurrentCount, (ColorType)_problem.Answer[_problem.CurrentCount]);
            
            TubeStage1Manager.Instance.TubeStageController.ResumeStage();
            
            // 타이머를 시작한다.
            TubeStage1Manager.Instance.StartAskingCoroutine(MakingAnswerTime());
        }

        public override void Exit()
        {
            TubeStage1Manager.Instance.StopCoroutine();
            TubeStage1Manager.Instance.Problem = _problem;
            TubeStage1Manager.Instance.TubeStageController.StopTicTacSFXClientRpc();
        }

        
        public override void OnButtonClicked(ColorType colorType)
        {
            _problem.PlayerAnswer = colorType;
            _problem.OnPlayerAnswered = true;
            
            TubeStage1Manager.Instance.ChangeState(TubeStage1Manager.Instance.Checking);
        }

        private float MakingAnswerTime()
        {
            return 15 - _problem.CurrentCount;
        }
    }
}