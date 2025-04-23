using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace TubeStage
{
    public class TubeStageController : NetworkBehaviour
    {
        [Header("Buttons")]
        [SerializeField] protected List<TubeButtonController> _sourceButtonGroup;
        [SerializeField] protected TubeButtonController _destinationButton;
        
        [Header("Tubes")]
        [SerializeField] protected List<TubeController> _sourceTubeGroup;
        [SerializeField] protected TubeController _destinationTube;

        [Header("Monitors")] 
        [SerializeField] protected List<TextRenderMonitorController> _monitorGroup;

        [Header("Doors")] 
        [SerializeField] protected DoorController _startDoorController;
        [SerializeField] protected DoorController _endDoorController;
        
        #region Button Control

        protected void SetSourceButtonPress(int index, bool isPress)
        {
            if (isPress)
            {
                _sourceButtonGroup[index].PressButton();
                _sourceButtonGroup[index].PlayPressAnimation(true);
            }
            else
            {
                _sourceButtonGroup[index].UnpressButton();
                _sourceButtonGroup[index].PlayPressAnimation(false);
            }
        }

        protected void SetSourceButtonEnable(int index, bool isEnable)
        {
            if (isEnable)
            {
                _sourceButtonGroup[index].EnableButton();
            }
            else
            {
                _sourceButtonGroup[index].DisableButton();
            }
        }

        #endregion

        #region Tube Control

        protected void SetTubeLight(int index, bool isLightOn)
        {
            _sourceTubeGroup[index].SetTubeLightClientRpc(isLightOn);
        }

        protected void SetSourceTubeValue(int index, float deltaValue)
        {
            _sourceTubeGroup[index].UpdateValue(deltaValue);
        }

        protected void SetDestinationTubeValue(float deltaValue)
        {
            _destinationTube.UpdateValue(deltaValue);
        }

        #endregion

        #region Door Control

        public void StartDoorOpen()
        {
            _startDoorController.IsOpened = true;
            _startDoorController.OpenDoorServerRpc();
        }

        public void EndDoorOpen()
        {
            _endDoorController.IsOpened = true;
            _endDoorController.OpenDoorServerRpc();
        }

        #endregion
    }
}