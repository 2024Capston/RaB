namespace TubeStage
{
    /// <summary>
    /// 플레이어 입력을 확인하여 정답 여부를 확인합니다.
    /// 성공했을 때 모두 완료하면 Cleared, 남아있다면 Asking
    /// 실패했을 때 기회가 남았다면 Asking, 없다면 Waiting으로 돌아갑니다.
    /// </summary>
    public class CheckingState : TubeStageState
    {
        public override void Enter()
        {
            // TODO
            // 모든 버튼의 입력을 비활성화합니다.
        }
        
        // 여기에서 처리?

        public override void Exit()
        {
            throw new System.NotImplementedException();
        }
    }
}