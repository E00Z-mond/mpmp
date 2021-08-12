using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIStageBox : MonoBehaviour
{
    public Image _frame;
    public Image[] _stars;
    public Text _stageIndexText;
    public GameObject _lock;
    public Button _btn;

    private int _stageID;

    public static Action<int> _stageSelectCheckEvent = null;

    private void Init()
    {
        for (int j = 0; j < _stars.Length; j++)
        {
            if (_stars[j].gameObject.activeSelf) _stars[j].gameObject.SetActive(false);
        }

        if (_stageIndexText.gameObject.activeSelf) _stageIndexText.gameObject.SetActive(false);
        if (_lock.activeSelf) _lock.SetActive(false);
    }

    public void SetLock()
    {
        Init();

        _frame.sprite = UIManager.Instance.GetSpriteResource(Texture.stage_frame_lock.ToString());
        _lock.SetActive(true);
        _btn.enabled = false;
        _stageID = -1;
    }

    public void SetData(int id, int index, int starCount)
    {
        Init();
        _stageID = id;

        _frame.sprite = UIManager.Instance.GetSpriteResource(Texture.stage_frame_default.ToString());

        for (int i = 0; i< _stars.Length; i++)
        {
            _stars[i].gameObject.SetActive(true);
            if (i < starCount)
            {
                _stars[i].sprite = UIManager.Instance.GetSpriteResource(Texture.stage_star_active.ToString());
            }
            else
            {
                _stars[i].sprite = UIManager.Instance.GetSpriteResource(Texture.stage_star_deactive.ToString());
            }

        }

        _stageIndexText.gameObject.SetActive(true);
        _stageIndexText.text = index.ToString();

        _btn.enabled = true;
    }

    public int GetStageID()
    {
        return _stageID;
    }

    public void OnSelectEvent()
    {
        _stageSelectCheckEvent?.Invoke(_stageID);
    }

    public void Deselect()
    {
        _frame.sprite = UIManager.Instance.GetSpriteResource(Texture.stage_frame_default.ToString());
    }

    public void Select()
    {
        _frame.sprite = UIManager.Instance.GetSpriteResource(Texture.stage_frame_select.ToString());
    }
}
