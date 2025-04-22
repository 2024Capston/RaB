namespace TubeStage
{
    /// <summary>
    /// 문제를 맵에 출력하고 Player의 입력을 기다리고 있는 State
    /// Player가 입력을 하거나, 제한시간이 지나면 CheckingState로 넘어간다.
    /// </summary>
    public class AskingState : TubeStageState
    {
        public override void Enter()
        {
            // TODO 
            // 현재 문제를 읽어와서 문제를 Monitor에 적용한다.
            // 코루틴으로 타이머를 센다.
        }

        public override void Exit()
        {
            // 여기서 나갈 때 진행중이 코루틴이 있다면 중지하고 나가면 됩니다.
        }

        
        public override void OnButtonClicked(ColorType colorType)
        {
            // TODO
            // 현재 문제의 답과 비교해야 한다.
        }
    }
}