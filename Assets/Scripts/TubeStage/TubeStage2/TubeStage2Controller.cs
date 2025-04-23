namespace TubeStage
{
    public class TubeStage2Controller : TubeStageController
    {
        public void InitStage()
        {
            for (int i = 0; i < _sourceTubeGroup.Count; i++)
            {
                _sourceTubeGroup[i].UpdateValue(-1f);
            }
            
            _destTube.UpdateValue(-1f);

            for (int i = 0; i < _sourceButtonGroup.Count; i++)
            {
                SetSourceButtonEnable(i, false);
                SetSourceButtonPress(i, true);
                _sourceButtonGroup[i].SetButtonColor(ColorType.None);
            }
            
            _descButton.SetButtonColor(ColorType.Purple);
            _descButton.UnpressButton();
            _descButton.PlayPressAnimation(false);
            _descButton.EnableButton();
            
            // 모니터 설정하기
        }
    }
}