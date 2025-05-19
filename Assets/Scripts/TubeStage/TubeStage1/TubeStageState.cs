namespace TubeStage.TubeStage1
{
    internal abstract class TubeStageState
    {
        public abstract void Enter();

        public abstract void Exit();

        public virtual void OnButtonClicked(ColorType colorType) { }
    }
}