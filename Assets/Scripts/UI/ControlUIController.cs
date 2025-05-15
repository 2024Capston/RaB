using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlUIController
{
    private VisualElement _root;
    
    private Action OnCloseControl;
    
    private Button _closeControlUIButton;

    public ControlUIController(VisualElement root, Action OnClickCloseControlUIButtonClick)
    {
        _root = root;
        
        _root.RegisterButtonClickSound();

        OnCloseControl = OnClickCloseControlUIButtonClick;
        
        _closeControlUIButton = _root.Q<Button>("CloseControlUIButton");
        
        _closeControlUIButton.RegisterCallback<ClickEvent>(OnClickCloseControlUIButton);
    }
    
    private void OnClickCloseControlUIButton(ClickEvent evt)
    {
        OnCloseControl?.Invoke();
    }
}
