using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ConfirmType
{
    OK,
    OK_Cancel,
}

public class ConfirmUIData : BaseUIData
{
    public ConfirmType ConfirmType;
    public string TitleText;
    public string DescText;

    public string OKButtonText;
    public Action OnClickOKButton;

    public string CancelButtonText;
    public Action OnClickCancelButton;
}

public class ConfirmUI : BaseUI
{
    [SerializeField]
    private TMP_Text _titleText;

    [SerializeField]
    private TMP_Text _descText;

    [SerializeField]
    private Button _okButton;

    [SerializeField]
    private TMP_Text _okButtonText;

    [SerializeField]
    private Button _cancelButton;

    [SerializeField]
    private TMP_Text _cancelButtonText;

    private ConfirmUIData _confirmUIData;
    private Action _onClickOKButton;
    private Action _onClickCancelButton;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        _confirmUIData = uiData as ConfirmUIData;   

        _titleText.text = _confirmUIData.TitleText;
        _descText.text = _confirmUIData.DescText;
        _okButtonText.text = _confirmUIData.OKButtonText;
        _onClickOKButton = _confirmUIData.OnClickOKButton;
        _cancelButtonText.text = _confirmUIData.CancelButtonText;
        _onClickCancelButton = _confirmUIData.OnClickCancelButton;

        _okButton.gameObject.SetActive(true);
        _cancelButton.gameObject.SetActive(_confirmUIData.ConfirmType == ConfirmType.OK_Cancel);
    }

    public void OnClickOKButton()
    {
        _onClickOKButton?.Invoke();
        _onClickOKButton = null;
        CloseUI();
    }

    public void OnClickCancelButton()
    {
        _onClickCancelButton?.Invoke();
        _onClickCancelButton = null;
        CloseUI();
    }
}
