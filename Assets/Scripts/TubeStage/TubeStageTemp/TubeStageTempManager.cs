using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TubeStage.TubeStageTemp
{
    public class TubeStageTempManager : StageManager
    {
        private Coroutine _currentGame;

        private TubeStageTempController _tubeStageTempController;
        
        protected override void Init()
        {
            _isDestroyOnLoad = true;
            base.Init();
            _tubeStageTempController = GetComponent<TubeStageTempController>();
        }
        
        public override void StartGame()
        {
            _tubeStageTempController.SetStartButton(true);
            TubeStageTempMapper.Instance.ResetAll();
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventA, OnClickStartButton);
            _tubeStageTempController.StartDoorOpen();
        }

        public override void RestartGame()
        {
            if (_currentGame != null)
            {
                StopCoroutine(_currentGame);
            }
            _currentGame = null;
            TubeStageTempMapper.Instance.SetSourceTubeLight(0);
            _tubeStageTempController.SetStartButton(true);
            TubeStageTempMapper.Instance.ResetAll();
        }

        public override void EndGame()
        {
            EventBus.Instance.UnsubscribeEvent<UnityAction>(EventType.EventA, OnClickStartButton);
            InGameManager.Instance.EndGameServerRpc();
        }

        private void OnClickStartButton()
        {
            _tubeStageTempController.SetStartButton(false);
            _currentGame = StartCoroutine(CoProgressGame());
        }

        private IEnumerator CoProgressGame()
        {
            TubeStageTempMapper.Instance.InitTube(); 
            TubeStageTempMapper.Instance.ClearState();

            bool isSuccess = true;
            
            for (int i = 0; i < 8; i++)
            {
                int lightMap = Random.Range(0, 15);
                TubeStageTempMapper.Instance.SetButtonGroupColor();
                TubeStageTempMapper.Instance.SetSourceTubeLight(lightMap);
                
                
                yield return new WaitForSeconds(15f);
                TubeStageTempMapper.Instance.ApplyButtonState();

                isSuccess &= lightMap == TubeStageTempMapper.Instance.ButtonState;
                Logger.Log($"{lightMap} {TubeStageTempMapper.Instance.ButtonState}");
                
                TubeStageTempMapper.Instance.ClearState();
                
            }

            if (isSuccess)
            {
                _tubeStageTempController.EndDoorOpen();
                TubeStageTempMapper.Instance.ClearAll();
            }
            else
            {
                RestartGame();
            }
        }
    }
}

