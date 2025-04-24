namespace TubeStage
{
    /// <summary>
    /// 모든 단계를 성공했을 때 들어오는 State
    /// EndDoor가 열리게 되고 Tube의 값들이 끝났을 때 값으로 바뀌게 됩니다.
    /// </summary>
    internal class ClearedState : TubeStageState
    {
        public override void Enter()
        {
            TubeStage2Manager.Instance.TubeStageController.ClearStage();
            TubeStage2Manager.Instance.TubeStageController.EndDoorOpen();
        }

        public override void Exit()
        {
            
        }
    }
}