using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TubeStage
{
    public class TubeStage2Controller : TubeStageController
    {
        [SerializeField] private TubeStageSoundController _soundController;
        private double _endTime;
        private bool _isTimeUpdate;
        
        private void Update()
        {
            if (!_isTimeUpdate)
            {
                return;
            }
            int remainTime = Math.Max(0, (int)(_endTime - NetworkManager.Singleton.ServerTime.Time));
            
            _monitorGroup[2].UpdateMonitorText(remainTime.ToString());
        }
        
        public void ApplyTubeCondition(ColorType colorType)
        {
            TubeController tube = _sourceTubeGroup.Find(x => x.Color == colorType);
            tube.UpdateValue(-0.33334f);
            _destinationTube.UpdateValue(0.125f);
        }

        public void StartLocalTimer(double endTime)
        {
            _endTime = endTime;
            _isTimeUpdate = true;
        }

        public void StopLocalTimer()
        {
            _isTimeUpdate = false;
        }

        #region Stage Control

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
            
            SetReadyInMonitor();
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

        public void ClearStage()
        {
            // destination button을 비활성화한다.
            _destinationButton.SetButtonColor(ColorType.None);
            _destinationButton.PressButton();
            _destinationButton.PlayPressAnimation(true);
            _destinationButton.DisableButton();

            foreach (TubeController tube in _sourceTubeGroup)
            {
                tube.UpdateValue(-1f);
            }
            _destinationTube.UpdateValue(1f);
        }

        #endregion

        #region Monitor Control

        /// <summary>
        /// 모니터에 현재 문제를 띄운다.
        /// </summary>
        /// <param name="stageNum"></param>
        /// <param name="colorType"></param>
        public void SetProblemInMonitor(int stageNum, ColorType colorType)
        {
            // 모니터에 띄우기
            Logger.Log($"Problem {colorType}");
            
            _monitorGroup[0].UpdateMonitorType(MonitorType.Text);
            _monitorGroup[0].UpdateMonitorTextServerRpc($"{stageNum + 1} / 8");
            _monitorGroup[1].UpdateMonitorType(MonitorType.Color);
            _monitorGroup[1].UpdateMonitorColorServerRpc(colorType);
            _monitorGroup[2].UpdateMonitorType(MonitorType.Text);
            _monitorGroup[2].UpdateMonitorTextServerRpc("");
        }
        
        /// <summary>
        /// 모니터가 첫 입력을 받을 준비를 한다. 
        /// </summary>
        public void SetReadyInMonitor()
        {
            _monitorGroup[0].UpdateMonitorType(MonitorType.Text);
            _monitorGroup[0].UpdateMonitorTextServerRpc("");
            _monitorGroup[1].UpdateMonitorType(MonitorType.Text);
            _monitorGroup[1].UpdateMonitorTextServerRpc("Press\nButton");
            _monitorGroup[2].UpdateMonitorType(MonitorType.Text);
            _monitorGroup[2].UpdateMonitorTextServerRpc("");
        }

        /// <summary>
        /// 문제를 틀렸을 때 모니터에 틀린 횟수를 쓴다.
        /// </summary>
        /// <param name="failCount"></param>
        public void SetFailCountInMonitor(int failCount)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i < failCount)
                {
                    _monitorGroup[i].UpdateMonitorType(MonitorType.XMark);
                }
                else
                {
                    _monitorGroup[i].UpdateMonitorType(MonitorType.Text);
                    _monitorGroup[i].UpdateMonitorTextServerRpc("");
                }
            }
        }

        /// <summary>
        /// 문제를 맞췄을 때 맞췄다고 적는다.
        /// </summary>
        /// <param name="isClear"></param>
        public void SetSuccessInMonitor(bool isClear)
        {
            if (isClear)
            {
                for (int i = 0; i < 3; i++)
                {
                    _monitorGroup[i].UpdateMonitorType(MonitorType.CheckMark);
                }
            }
            else
            {
                _monitorGroup[0].UpdateMonitorType(MonitorType.Text);
                _monitorGroup[0].UpdateMonitorTextServerRpc("");
                _monitorGroup[1].UpdateMonitorType(MonitorType.CheckMark);
                _monitorGroup[2].UpdateMonitorType(MonitorType.Text);
                _monitorGroup[2].UpdateMonitorTextServerRpc("");
            }
        }
        
        

        #endregion

        #region SFX

        [ClientRpc]
        public void PlayCorrectSFXClientRpc()
        {
            _soundController.PlayCorrectSFX();
        }

        [ClientRpc]
        public void PlayIncorrectSFXClientRpc()
        {
            _soundController.PlayInCorrectSFX();
        }

        [ClientRpc]
        public void StartTicTacSFXClientRpc()
        {
            _soundController.PlayTicTacSFX();
        }

        [ClientRpc]
        public void StopTicTacSFXClientRpc()
        {
            _soundController.StopTicTacSFX();
        }

        #endregion
    }
}