using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UICharacterMenu : UIBase
{
    public GameObject _characterBoxOrigin;
    public Transform _characterGroup;
    public Animator _anim;

    public GameObject _popup;
    public GameObject _popupPurchaseGroup;
    public GameObject _popupJoinBtn;
    public GameObject _popupTrainingGroup;
    public GameObject _maxLevel;
    public GameObject _trainResult;

    public Image _popupCharacterIcon;
    public Image _popupTypeIcon;
    public Text _popupNameText;
    public Text _popupStrText;
    public Text _popupDefText;
    public Text _popupHpText;
    public Text _popupSkillText;
    public Text _popupGoldText;
    public Text _popupLevelText;
    public Text _popupTrainLevelText;
    public Text _popupTrainCloverText;
    public Text _popupTrainChanceText;

    private Game_Data _gameData = null;
    private HashSet<Character_id> _playableCharacter;
    private Character_id _currentCharacter = Character_id.Character_id_none;

    private readonly Dictionary<Character_id, UICharacterBox> _characterBoxes = new Dictionary<Character_id, UICharacterBox>();
    public static Action<Character_id, Character_type> _characterJoinEvent;
    private StringBuilder _stringBuilder = new StringBuilder();
    public override void Init()
    {
        _gameData = DataController.Instance.GetGameData();

        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_set_character_menu, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_hide, this);

        UICharacterBox._characterClickEvent += ShowInfoPopup;

        _playableCharacter = TableManager.Instance.GetPlayableCharacter();

        using var enumer = _playableCharacter.GetEnumerator();
        while (enumer.MoveNext())
        {
            GameObject characterBox = Instantiate(_characterBoxOrigin, _characterGroup);
            characterBox.SetActive(true);

            UICharacterBox characterBoxScript = characterBox.GetComponent<UICharacterBox>();

            if (_gameData.character_datas.ContainsKey(enumer.Current))
            {
                Character_Data data = _gameData.character_datas[enumer.Current];
                Character_Table table = (Character_Table)TableManager.Instance.GetTableValue(Table_resources.Character_Table, (int)enumer.Current);
                characterBoxScript.Register(data.character_Id, data.level, data.is_joined, table.type);
            }
            else
            {
                characterBoxScript.SetLock(enumer.Current);
            }

            _characterBoxes.Add(enumer.Current, characterBoxScript);
        }

        gameObject.SetActive(false);
    }

    public override void Release()
    {
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_set_character_menu, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_hide, this);

        UICharacterBox._characterClickEvent -= ShowInfoPopup;

        for (int i = 0; i < _characterGroup.childCount; i++)
        {
            Destroy(_characterGroup.GetChild(i).gameObject);
        }

        _characterBoxes.Clear();
        _stringBuilder.Clear();

        _gameData = null;
    }

    public override void UpdateUI(UIEvent uiEvent, params object[] values)
    {
        switch (uiEvent)
        {
            case UIEvent.UIEvent_set_character_menu:
                gameObject.SetActive(true);
                break;

            case UIEvent.UIEvent_hide:
                gameObject.SetActive(false);
                break;
        }
    }

    private void ShowInfoPopup(Character_id id, bool isLock)
    {
        transform.SetAsLastSibling();
        _popup.SetActive(true);

        var character = (Character_Table)TableManager.Instance.GetTableValue(Table_resources.Character_Table, (int)id);
        var skill = (Skill_Table)TableManager.Instance.GetTableValue(Table_resources.Skill_Table, (int)character.skill);

        _popupCharacterIcon.sprite = UIManager.Instance.GetSpriteResource(id.ToString());
        _popupTypeIcon.sprite = UIManager.Instance.GetSpriteResource(character.type.ToString());

        if (isLock)
        {
            _popupTrainingGroup.SetActive(false);
            _maxLevel.SetActive(false);
            _popupLevelText.gameObject.SetActive(false);
            _popupJoinBtn.SetActive(false);

            _popupPurchaseGroup.SetActive(true);
            _popupGoldText.text = character.price.ToString();
        }
        else
        {
            _popupPurchaseGroup.SetActive(false);

            _popupLevelText.gameObject.SetActive(true);
            Character_Data data = _gameData.character_datas[id];
            _popupLevelText.text = "Lv. " + data.level.ToString();
            if (data.is_joined == false)
            {
                _popupJoinBtn.SetActive(true);
            }
            else
            {
                _popupJoinBtn.SetActive(false);
            }

            int nextLevel = data.level + 1;
            if(nextLevel > Const.MAX_LEVEL)
            {
                _maxLevel.SetActive(true);
                _popupTrainingGroup.SetActive(false);
            }
            else
            {
                _popupTrainingGroup.SetActive(true);
                _maxLevel.SetActive(false);

                _stringBuilder.Append("Lv. ").Append(data.level.ToString()).Append(" -> Lv. ").Append(nextLevel.ToString());
                _popupTrainLevelText.text = _stringBuilder.ToString();
                _stringBuilder.Clear();

                Training_Table table = (Training_Table)TableManager.Instance.GetTableValue(Table_resources.Training_Table, data.level);
                _popupTrainCloverText.text = table.required_clover.ToString();
                _stringBuilder.Append("Chance ").Append(Mathf.FloorToInt((float)(table.chance_of_success * 0.01)).ToString()).Append("%");
                _popupTrainChanceText.text = _stringBuilder.ToString();
                _stringBuilder.Clear();
            }

            character = TableManager.Instance.GetLevelAppliedCharacterTable(id, data.level);
        }

        _popupNameText.text = character.name_text;
        _popupStrText.text = character.str.ToString();
        _popupDefText.text = character.def.ToString();
        _popupHpText.text = character.hp.ToString();
        _popupSkillText.text = skill.des_text;

        _currentCharacter = id;
    }
    public void PurchaseCharacter()
    {
        var character = (Character_Table)TableManager.Instance.GetTableValue(Table_resources.Character_Table, (int)_currentCharacter);

        if(_gameData.gold < character.price)
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Not enough gold!");
            return;
        }

        _gameData.gold -= character.price;
        _gameData.character_datas.Add(_currentCharacter, new Character_Data(_currentCharacter, 1, false));
        DataController.Instance.SaveData();

        UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_goods);
        SoundManager.Instance.PlaySFX(Sound.Purchase);
        _characterBoxes[_currentCharacter].Register(_currentCharacter, 1, false, character.type);

        ShowInfoPopup(_currentCharacter, false);
    }

    public void JoinCharacter()
    {
        var character = (Character_Table)TableManager.Instance.GetTableValue(Table_resources.Character_Table, (int)_currentCharacter);

        using var enumer = _gameData.character_datas.GetEnumerator();
        while (enumer.MoveNext())
        {
            if (enumer.Current.Value.is_joined)
            {
                var target = (Character_Table)TableManager.Instance.GetTableValue(Table_resources.Character_Table, (int)enumer.Current.Key);

                if (target.type == character.type)
                {
                    _gameData.character_datas[target.id].is_joined = false;
                    _gameData.character_datas[character.id].is_joined = true;

                    DataController.Instance.SaveData();

                    _characterBoxes[target.id].Join(false);
                    _characterBoxes[character.id].Join(true);

                    _characterJoinEvent?.Invoke(character.id, character.type);

                    SoundManager.Instance.PlaySFX(Sound.Join);
                    _popup.SetActive(false);
                    return;
                }
            }
        }
    }

    public void TrainCharacter()
    {
        var characterData = _gameData.character_datas[_currentCharacter];
        Training_Table table = (Training_Table)TableManager.Instance.GetTableValue(Table_resources.Training_Table, characterData.level);

        if(_gameData.clover < table.required_clover)
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Not enough clover!");
            return;
        }

        _trainResult.SetActive(true);
        int random = Random.Range(0, Const.TRANING_CHANCE_MAX);
        if (random < table.chance_of_success)
        {
            _anim.SetTrigger("LevelUP");
            characterData.level++;
            if (characterData.is_joined)
            {
                Character_Table character = (Character_Table)TableManager.Instance.GetTableValue(Table_resources.Character_Table, (int)_currentCharacter);
                UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_level, new object[] { character.type, characterData.level });
            }
        }
        else
        {
            _anim.SetTrigger("Lose");
        }

        _gameData.clover -= table.required_clover;
        DataController.Instance.SaveData();
        _characterBoxes[_currentCharacter].ChangeLevel(characterData.level);
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_goods);
        ShowInfoPopup(_currentCharacter, false);
    }

    public void CloseTrainResult()
    {
        _anim.SetTrigger("CloseTrainResult");
        _trainResult.SetActive(false);
    }
}
