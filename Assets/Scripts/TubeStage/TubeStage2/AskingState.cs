using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace TubeStage.TubeStage2
{
    /// <summary>
    /// 문제를 맵에 출력하고 Player의 입력을 기다리고 있는 State
    /// Player가 입력을 하거나, 제한시간이 지나면 CheckingState로 넘어간다.
    /// </summary>
    internal class AskingState : TubeStageState
    {
        private TubeStage2Problem _problem;
        
        /// <summary>
        /// Asking State에 진입 시 새로운 문제를 냅니다.
        /// </summary>
        public override void Enter()
        {
            if (TubeStage2Manager.Instance.Problem is null)
            {
                Logger.LogError("Problem does not exist");
                TubeStage2Manager.Instance.ChangeState(TubeStage2Manager.Instance.Waiting);
                return;
            }

            _problem = TubeStage2Manager.Instance.Problem.Value;
            _problem.OnPlayerAnswered = false;
            
            // 현재 Answer를 Stage에 적용한다.
            TubeStage2Manager.Instance.TubeStageController.SetProblemInMonitor(_problem.CurrentCount, (ColorType)_problem.Answer[_problem.CurrentCount]);

            if (!_problem.LastAttemptFailed)
            {
                // 직전에 맞췄다면 버튼의 배치를 변경한다.
                TubeStage2Manager.Instance.TubeStageController.SetButtonsColor(GetButtonColors());
            }
            
            TubeStage2Manager.Instance.TubeStageController.ResumeStage();
            
            // 타이머를 시작한다.
            TubeStage2Manager.Instance.StartAskingCoroutine(MakingAnswerTime());
        }

        public override void Exit()
        {
            TubeStage2Manager.Instance.StopCoroutine();
            TubeStage2Manager.Instance.Problem = _problem;
            TubeStage2Manager.Instance.TubeStageController.StopTicTacSFXClientRpc();
        }

        
        public override void OnButtonClicked(ColorType colorType)
        {
            _problem.PlayerAnswer = colorType;
            _problem.OnPlayerAnswered = true;
            
            TubeStage2Manager.Instance.ChangeState(TubeStage2Manager.Instance.Checking);
        }

        private float MakingAnswerTime()
        {
            return 15 - _problem.CurrentCount;
        }

        private List<ColorType> GetButtonColors()
        {
            List<ColorType> colorTypes = new List<ColorType>
                { ColorType.None, ColorType.Blue, ColorType.Red, ColorType.Purple };

            Random random = new Random();
            return colorTypes.OrderBy(x => random.Next()).ToList();
        } 
    }
}