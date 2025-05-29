using System.Collections;
using UnityEngine;

namespace TubeStage.TubeStage2
{
    /// <summary>
    /// 플레이어 입력을 확인하여 정답 여부를 확인합니다.
    /// 성공했을 때 모두 완료하면 Cleared, 남아있다면 Asking
    /// 실패했을 때 기회가 남았다면 Asking, 없다면 Waiting으로 돌아갑니다.
    /// </summary>
    internal class CheckingState : TubeStageState
    {
        private TubeStage2Problem _problem;
        
        public override void Enter()
        {
            TubeStage2Manager.Instance.TubeStageController.StopStage();
            
            if (TubeStage2Manager.Instance.Problem is null)
            {
                Logger.LogError("Problem does not exist");
                TubeStage2Manager.Instance.ChangeState(TubeStage2Manager.Instance.Waiting);
                return;
            }
            
            _problem = TubeStage2Manager.Instance.Problem.Value;
            CheckingAnswer();
        }
        
        public override void Exit()
        {
            TubeStage2Manager.Instance.Problem = _problem;
        }

        public override void OnButtonClicked(ColorType colorType)
        {
            TubeStage2Manager.Instance.ChangeState(TubeStage2Manager.Instance.Asking);
        }

        private void CheckingAnswer()
        {
            if (_problem.OnPlayerAnswered == false || _problem.PlayerAnswer != (ColorType)_problem.Answer[_problem.CurrentCount])
            {
                TubeStage2Manager.Instance.TubeStageController.PlayIncorrectSFXClientRpc();
                _problem.LastAttemptFailed = true;
                
                if (++_problem.FailCount == _problem.MaxFailCount)
                {
                    TubeStage2Manager.Instance.TubeStageController.SetFailCountInMonitor(_problem.FailCount);
                    TubeStage2Manager.Instance.StartFailedCoroutine(5f);
                }
                else
                {
                    TubeStage2Manager.Instance.TubeStageController.SetFailCountInMonitor(_problem.FailCount);
                }
            }
            else
            {
                // 정답이므로 Tube 반영!
                TubeStage2Manager.Instance.TubeStageController.ApplyTubeCondition(_problem.PlayerAnswer);
                TubeStage2Manager.Instance.TubeStageController.PlayCorrectSFXClientRpc();
                _problem.LastAttemptFailed = false;
                
                if (++_problem.CurrentCount == _problem.StageCount)
                {
                    TubeStage2Manager.Instance.TubeStageController.SetSuccessInMonitor(true);
                    TubeStage2Manager.Instance.ChangeState(TubeStage2Manager.Instance.Cleared);
                }
                else
                {
                    TubeStage2Manager.Instance.TubeStageController.SetSuccessInMonitor(false);
                }
            }
        }
    }
}