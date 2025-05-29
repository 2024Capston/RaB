using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TubeStage
{
    public class ButtonGroupManager : MonoBehaviour
    {
        [SerializeField] private List<TubeButtonController> _sourceButtonGroup;
        [SerializeField] private TubeButtonController _descButton;

        public void SetButtonColor(int index, ColorType colorType)
        {
            _sourceButtonGroup[index].SetButtonColor(colorType);
        }

        public void SetButtonPress(int index, bool isPress)
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

        public void SetButtonEnable(int index, bool isEnable)
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
    }   
}