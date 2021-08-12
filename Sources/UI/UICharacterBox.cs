using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterBox : MonoBehaviour
{
    public Image _characterImage;
    public Image _typeImage;
    public Text _levelText;
    public GameObject _lock;
    public GameObject _join;
    public Text _goldText;

    private Character_id _id = Character_id.Character_id_none;
    private bool _isLock = false;

    public static Action<Character_id, bool> _characterClickEvent; 
    public void SetLock(Character_id id)
    {
        _isLock = true;
        _id = id;
        _characterImage.sprite = UIManager.Instance.GetSpriteResource(id.ToString());
        _characterImage.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        _lock.SetActive(true);
        Character_Table character = (Character_Table)TableManager.Instance.GetTableValue(Table_resources.Character_Table, (int)id);
        _goldText.text = character.price.ToString();
    }
    public void Register(Character_id id, int level, bool isJoined, Character_type type)
    {
        if (_lock.activeSelf) _lock.SetActive(false);
        
        _id = id;
        _isLock = false;
        _characterImage.sprite = UIManager.Instance.GetSpriteResource(id.ToString());
        _characterImage.color = Color.white;

        _levelText.gameObject.SetActive(true);
        _typeImage.gameObject.SetActive(true);

        _levelText.text = "Lv. " + level.ToString(); 
        _typeImage.sprite = UIManager.Instance.GetSpriteResource(type.ToString());

        if (isJoined)
        {
            _join.SetActive(true);
        }
    }

    public void Join(bool isJoined)
    {
        if (isJoined)
        {
            _join.SetActive(true);
        }
        else
        {
            _join.SetActive(false);
        }
    }

    public void ChangeLevel(int level)
    {
        _levelText.text = "Lv. " + level.ToString();
    }

    public void CharacterClickEvent()
    {
        _characterClickEvent?.Invoke(_id, _isLock);
    }
}
