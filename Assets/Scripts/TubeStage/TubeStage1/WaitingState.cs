using System;
using System.Collections.Generic;
using System.Linq;

namespace TubeStage.TubeStage1
{
    /// <summary>
    /// 게임 시작을 기다리고 있는 State
    /// Player가 DestinationButton을 누르면 AskingState로 넘어갑니다.
    /// </summary>
    internal class WaitingState : TubeStageState
    {
        /// <summary>
        /// Waiting State에 진입하면 모든 Object가 기본값으로 초기화 
        /// </summary>
        public override void Enter()
        {
            TubeStage1Manager.Instance.TubeStageController.InitStage();
        }

        /// <summary>
        /// Asking State로 넘어갈 때엔 문제를 새로 생성하고 색 배치를 새로 만듭니다.
        /// </summary>
        public override void Exit()
        {
            TubeStage1Manager.Instance.Problem = MakingProblem();
            
            List<ColorType> colorList = new List<ColorType>
                { ColorType.None, ColorType.Blue, ColorType.Red, ColorType.Purple };
            Random random = new Random();
            
            TubeStage1Manager.Instance.TubeStageController.StartStage(colorList.OrderBy(x => random.Next()).ToList());
        }

        /// <summary>
        /// 버튼이 눌리면 Asking State로 전환
        /// </summary>
        /// <param name="colorType"></param>
        public override void OnButtonClicked(ColorType colorType)
        {
            TubeStage1Manager.Instance.ChangeState(TubeStage1Manager.Instance.Asking);
        }

        private TubeStage1Problem MakingProblem()
        {
            List<int> answer = new List<int> { 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3 };
            Random random = new Random();
            
            TubeStage1Problem problem = new TubeStage1Problem()
            {
                Answer = answer.OrderBy(x => random.Next()).Take(8).ToList(),
                CurrentCount = 0,
                FailCount = 0,
                MaxFailCount = 3
            };

            return problem;
        }   
    }
}