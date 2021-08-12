using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuffBox : MonoBehaviour
{
    public Image _buffIcon;
    public Text _buffTurnsText;
    public RectTransform _transform;
    private Buff_id _buffId = Buff_id.Buff_id_none;

    public static Action<Buff_id, Vector2> _clickBuffBoxEvent;
    public void Register(Buff_id id, int turns, Color color, Buff_type type)
    {
        _buffId = id;
        _buffTurnsText.text = turns.ToString();
        _buffIcon.color = color;
        if(type == Buff_type.Buff_type_buff)
        {
            _buffIcon.sprite = UIManager.Instance.GetSpriteResource(Texture.buff_type_buff.ToString());
        } else if(type == Buff_type.Buff_type_debuff)
        {
            _buffIcon.sprite = UIManager.Instance.GetSpriteResource(Texture.buff_type_debuff.ToString());
        }
    }

    public void ChangePos(Vector2 pos)
    {
        _transform.localPosition = pos;
    }

    public void Changeturns(int turns)
    {
        _buffTurnsText.text = turns.ToString();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void OnClickBuffBox()
    {
        _clickBuffBoxEvent?.Invoke(_buffId, _transform.position);
    }
}
