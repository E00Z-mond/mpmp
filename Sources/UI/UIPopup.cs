using System;
using UnityEngine.UI;

public class UIPopup : UIBase
{
    public Text _descText;
    public static Action _closeEvent = null;  
    public override void Init()
    {
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_show_popup, this);
        gameObject.SetActive(false);
    }

    public override void Release()
    {
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_show_popup, this);
    }

    public override void UpdateUI(UIEvent uiEvent, params object[] values)
    {
        switch (uiEvent)
        {
            case UIEvent.UIEvent_show_popup:
                transform.SetAsLastSibling();
                gameObject.SetActive(true);
                _descText.text = values[0].ToString();
                break;
        }
    }

    public void OnCloseEvent()
    {
        SoundManager.Instance.PlaySFX(Sound.Close);

        if(_closeEvent != null)
        {
            _closeEvent.Invoke();
            _closeEvent = null;
        }

        gameObject.SetActive(false);
    }
}
