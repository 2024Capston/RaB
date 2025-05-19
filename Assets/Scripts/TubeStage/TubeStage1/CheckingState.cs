using System.Collections;
using UnityEngine;

namespace TubeStage.TubeStage1
{
    /// <summary>
    /// 플레이어 입력을 확인하여 정답 여부를 확인합니다.
    /// 성공했을 때 모두 완료하면 Cleared, 남아있다면 Asking
    /// 실패했을 때 기회가 남았다면 Asking, 없다면 Waiting으로 돌아갑니다.
    /// </summary>
    internal class CheckingState : TubeStageState
    {
        private TubeStage1Problem _problem;
        
        public override void Enter()
        {
            TubeStage1Manager.Instance.TubeStageController.StopStage();
            
            if (TubeStage1Manager.Instance.Problem is null)
            {
                Logger.LogError("Problem does not exist");
                TubeStage1Manager.Instance.ChangeState(TubeStage1Manager.Instance.Waiting);
                return;
            }
            
            _problem = TubeStage1Manager.Instance.Problem.Value;
            CheckingAnswer();
        }
        
        public override void Exit()
        {
            TubeStage1Manager.Instance.Problem = _problem;
        }

        public override void OnButtonClicked(ColorType colorType)
        {
            TubeStage1Manager.Instance.ChangeState(TubeStage1Manager.Instance.Asking);
        }

        private void CheckingAnswer()
        {
            if (_problem.OnPlayerAnswered == false || _problem.PlayerAnswer != (ColorType)_problem.Answer[_problem.CurrentCount])
            {
                TubeStage1Manager.Instance.TubeStageController.PlayIncorrectSFXClientRpc();
                if (++_problem.FailCount == _problem.MaxFailCount)
                {
                    TubeStage1Manager.Instance.TubeStageController.SetFailCountInMonitor(_problem.FailCount);
                    TubeStage1Manager.Instance.StartFailedCoroutine(5f);
                }
                else
                {
                    TubeStage1Manager.Instance.TubeStageController.SetFailCountInMonitor(_problem.FailCount);
                }
            }
            else
            {
                // 정답이므로 Tube 반영!
                TubeStage1Manager.Instance.TubeStageController.ApplyTubeCondition(_problem.PlayerAnswer);
                TubeStage1Manager.Instance.TubeStageController.PlayCorrectSFXClientRpc();
                if (++_problem.CurrentCount == _problem.StageCount)
                {
                    TubeStage1Manager.Instance.TubeStageController.SetSuccessInMonitor(true);
                    TubeStage1Manager.Instance.ChangeState(TubeStage1Manager.Instance.Cleared);
                }
                else
                {
                    TubeStage1Manager.Instance.TubeStageController.SetSuccessInMonitor(false);
                }
            }
        }
    }
}