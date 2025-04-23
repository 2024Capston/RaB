using System.Collections.Generic;
using UnityEngine;

namespace TubeStage
{
    public class TubeStage2Controller : TubeStageController
    {
        /// <summary>
        /// Stage를 초기 상태로 만든다.
        /// </summary>
        public void InitStage()
        {
            // source tube의 모든 value를 0f로 만든다.
            foreach (var tube in _sourceTubeGroup)
            {
                tube.UpdateValue(-1f);
            }
            
            // destination tube의 모든 value를 0f로 만든다.
            _destinationTube.UpdateValue(-1f);

            // source button을 모두 비활성화 한다.
            for (int i = 0; i < _sourceButtonGroup.Count; i++)
            {
                SetSourceButtonEnable(i, false);
                SetSourceButtonPress(i, true);
                _sourceButtonGroup[i].SetButtonColor(ColorType.None);
            }
            
            // destination button을 활성화한다.
            _destinationButton.SetButtonColor(ColorType.Purple);
            _destinationButton.UnpressButton();
            _destinationButton.PlayPressAnimation(false);
            _destinationButton.EnableButton();
            
            // TODO 모니터 설정하기
        }

        /// <summary>
        /// 게임이 시작되었을 때 Tube와 Button에 색 배정을 한다.
        /// </summary>
        /// <param name="colorList"></param>
        public void StartStage(List<ColorType> colorList)
        {
            for (int i = 0; i < 4; i++)
            {
                _sourceTubeGroup[i].SetTubeColorClientRpc(colorList[i]);
                _sourceTubeGroup[i].UpdateValue(1f);
                _sourceButtonGroup[i].SetButtonColor(colorList[i]);
                
                Logger.Log($"colorList {i} : {colorList[i]}");
            }
        }

        /// <summary>
        /// Stage의 정답 입력을 다시 재개
        /// </summary>
        public void ResumeStage()
        {
            // source button을 모두 활성화 한다.
            for (int i = 0; i < _sourceButtonGroup.Count; i++)
            {
                SetSourceButtonEnable(i, true);
                SetSourceButtonPress(i, false);
            }
            
            // destination button을 비활성화한다.
            _destinationButton.SetButtonColor(ColorType.None);
            _destinationButton.PressButton();
            _destinationButton.PlayPressAnimation(true);
            _destinationButton.DisableButton();
        }
        
        /// <summary>
        /// Stage에서 정답 입력을 중지
        /// </summary>
        public void StopStage() 
        {
            // source button을 모두 활성화 한다.
            for (int i = 0; i < _sourceButtonGroup.Count; i++)
            {
                SetSourceButtonEnable(i, false);
                SetSourceButtonPress(i, true);
            }
            
            // destination button을 활성화한다.
            _destinationButton.SetButtonColor(ColorType.Purple);
            _destinationButton.UnpressButton();
            _destinationButton.PlayPressAnimation(false);
            _destinationButton.EnableButton();
        }

        public void ApplyProblem(ColorType colorType)
        {
            // 모니터에 띄우기
            Logger.Log($"Problem {colorType}");
        }

        public void ApplyTubeCondition(ColorType colorType)
        {
            TubeController tube = _sourceTubeGroup.Find(x => x.Color == colorType);
            tube.UpdateValue(-0.33333f);
            _destinationTube.UpdateValue(0.125f);
        }
    }
}