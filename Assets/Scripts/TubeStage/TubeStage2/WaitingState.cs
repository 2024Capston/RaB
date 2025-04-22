namespace TubeStage
{
    /// <summary>
    /// 게임 시작을 기다리고 있는 State
    /// Player가 DescButton을 누르면 AskingState로 넘어갑니다.
    /// </summary>
    public class WaitingState : TubeStageState
    {
        public override void Enter()
        {
            /*
             * TODO
             * 1. SourceTube.Fill을 0으로 만든다.
             * 2. DescTube.Fill을 0으로 만든다.
             * 3. SourceButton을 모두 비활성화한다.
             * 4. DescButton을 활성화한다.
             * 5. Monitor에 PressStart를 띄운다.
             */
        }

        public override void Exit()
        {
            // TODO
            // 문제를 만든다.
            
            
        }

        public override void OnButtonClicked(ColorType colorType)
        {
            // TODO
            // 현재 작동 가능한 버튼은 DescButton 밖에 없다.
            // Progress State로 넘어가면 된다.
        }
    }
}