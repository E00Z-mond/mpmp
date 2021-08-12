using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuide : UIBase
{
    public Image _guideImage;
    public static Action _closeEvent = null;
    public override void Init()
    {
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_show_guide, this);
        gameObject.SetActive(false);
    }

    public override void Release()
    {
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_show_guide, this);
    }

    public override void UpdateUI(UIEvent uiEvent, params object[] values)
    {
        switch (uiEvent)
        {
            case UIEvent.UIEvent_show_guide:
                transform.SetAsLastSibling();
                gameObject.SetActive(true);
                _guideImage.sprite = UIManager.Instance.GetSpriteResource(values[0].ToString());
                break;
        }
    }

    public void OnCloseEvent()
    {
        SoundManager.Instance.PlaySFX(Sound.Close);

        if (_closeEvent != null)
        {
            _closeEvent.Invoke();
            _closeEvent = null;
        }

        gameObject.SetActive(false);
    }
}
