namespace TubeStage
{
    public abstract class TubeStageState
    {
        public abstract void Enter();

        public abstract void Exit();

        public virtual void OnButtonClicked(ColorType colorType) { }
    }
}