using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterHUD
{
    HPBar,
    BuffPrinter,
    RevivalCounter,
    CoolTimeCounter
}

public struct TextObjectWithRect
{
    public RectTransform _rect;
    public Text _text;
    public TextObjectWithRect(RectTransform rect, Text text)
    {
        _rect = rect;
        _text = text;
    }
}

public struct TextObject
{
    public GameObject _obj;
    public Text _text;
    public TextObject(GameObject obj, Text text)
    {
        _obj = obj;
        _text = text;
    }
}
public class UIBattleCharacter : UIBase
{
    // 캐릭터 별 정보 요소 원본
    public GameObject _hpBar = null;
    public GameObject _ctCounter = null;
    public GameObject _buffPrinter = null;
    public GameObject _buffDescObj = null;
    public GameObject _revivalCounter = null;

    private Transform _hpGroup = null;
    private Transform _ctGroup = null;
    private Transform _bpGroup = null;
    private Transform _rcGroup = null;

    // 정보 요소 목록
    private readonly Dictionary<int, Slider> _activeHPBars = new Dictionary<int, Slider>();
    private readonly Dictionary<int, TextObject> _activeCTCounters = new Dictionary<int, TextObject>();
    private readonly Dictionary<int, TextObject> _activeRevivalCounters = new Dictionary<int, TextObject>();
    private readonly Dictionary<int, UIBuffPrinter> _activeBuffPrinters = new Dictionary<int, UIBuffPrinter>();

    private Coroutine _buffCoroutine = null;
    private TextObjectWithRect _buffDesc;

    public override void Init()
    {
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_register, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_hp_update, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_ct_update, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_add_buff, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_remove_buff, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_update_buff, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_dead, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_revival, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_update_rc, this);

        UIBuffBox._clickBuffBoxEvent += ClickBuffDesc;
        _hpGroup = _hpBar.transform.parent;
        _ctGroup = _ctCounter.transform.parent;
        _bpGroup = _buffPrinter.transform.parent;
        _rcGroup = _revivalCounter.transform.parent;
        _buffDesc = new TextObjectWithRect(_buffDescObj.GetComponent<RectTransform>(), _buffDescObj.GetComponentInChildren<Text>());
    }

    public override void Release()
    {
        StopAllCoroutines();

        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_register, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_hp_update, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_ct_update, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_add_buff, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_remove_buff, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_update_buff, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_dead, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_revival, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_update_rc, this);

        UIBuffBox._clickBuffBoxEvent -= ClickBuffDesc;

        var enumerator = _activeHPBars.GetEnumerator();
        while (enumerator.MoveNext())
        {
            int id = enumerator.Current.Key;

            Destroy(_activeHPBars[id].gameObject);
            Destroy(_activeBuffPrinters[id].gameObject);
            if (_activeCTCounters.ContainsKey(id)) Destroy(_activeCTCounters[id]._obj);
            if(_activeRevivalCounters.ContainsKey(id)) Destroy(_activeRevivalCounters[id]._obj);
        }

        _activeCTCounters.Clear();
        _activeHPBars.Clear();
        _activeBuffPrinters.Clear();
        _activeRevivalCounters.Clear();

        _buffDescObj.SetActive(false);
    }

    public override void UpdateUI(UIEvent uiEvent, params object[] values)
    {
        int instanceID = (int)values[0];

        switch (uiEvent)
        {
            case UIEvent.UIEvent_register:
                //인스턴스 ID, 위치, isPlr, 쿨타임값(생략 가능)
                Vector3 pos = (Vector3)values[1];
                RegisterUI(CharacterHUD.HPBar, instanceID, pos);
                RegisterUI(CharacterHUD.BuffPrinter, instanceID, pos);
                if ((bool)values[2]) RegisterUI(CharacterHUD.RevivalCounter, instanceID, pos);
                else RegisterUI(CharacterHUD.CoolTimeCounter, instanceID, pos, (int)values[3]);
                break;

            case UIEvent.UIEvent_hp_update:
                //인스턴스 ID, 슬라이더 비율값
                Slider slider_ = _activeHPBars[instanceID];
                slider_.value = (float)values[1];
                break;

            case UIEvent.UIEvent_ct_update:
                //인스턴스 ID, 쿨타임 값
                Text text = _activeCTCounters[instanceID]._text;
                text.text = values[1].ToString();
                break;

            case UIEvent.UIEvent_add_buff:
                //인스턴스 ID, 버프 ID
                _activeBuffPrinters[instanceID].AddBuffBox((Buff_id)values[1]);
                break;

            case UIEvent.UIEvent_remove_buff:
                //인스턴스 ID, 버프 ID
                _activeBuffPrinters[instanceID].RemoveBuffBox((Buff_id)values[1]);
                break;

            case UIEvent.UIEvent_update_buff:
                //인스턴스 ID, 버프 ID, 턴 수 
                _activeBuffPrinters[instanceID].UpdateBuffTurns((Buff_id)values[1], (int)values[2]);
                break;

            case UIEvent.UIEvent_dead:
                //인스턴스 ID
                SetActiveUIGroup(instanceID, false);
                if(_activeRevivalCounters.ContainsKey(instanceID)) _activeRevivalCounters[instanceID]._obj.SetActive(true);
                break;

            case UIEvent.UIEvent_revival:
                //인스턴스 ID
                SetActiveUIGroup(instanceID, true);
                _activeRevivalCounters[instanceID]._obj.SetActive(false);
                break;

            case UIEvent.UIEvent_update_rc:
                //인스턴스 ID, rc
                if (_activeRevivalCounters.ContainsKey(instanceID)) _activeRevivalCounters[instanceID]._text.text = values[1].ToString();
                break;
        }
    }

    private void RegisterUI(CharacterHUD hud, int instanceID, Vector2 worldPos, int value = 0)
    {
        GameObject obj = null;
        switch (hud)
        {
            case CharacterHUD.HPBar:
                worldPos.y += 2;
                obj = Instantiate(_hpBar, _hpGroup);

                Slider slider = obj.GetComponent<Slider>();
                _activeHPBars.Add(instanceID, slider);
                break;
            case CharacterHUD.CoolTimeCounter:
                worldPos.y += 2;
                worldPos.x--;
                obj = Instantiate(_ctCounter, _ctGroup);

                Text text = obj.GetComponentInChildren<Text>();
                _activeCTCounters.Add(instanceID, new TextObject(obj, text));
                text.text = value.ToString();
                break;
            case CharacterHUD.BuffPrinter:
                worldPos.y -= 0.2f;
                worldPos.x -= 0.2f;
                obj = Instantiate(_buffPrinter, _bpGroup);

                UIBuffPrinter uIBuffPrinter = obj.GetComponent<UIBuffPrinter>();
                _activeBuffPrinters.Add(instanceID, uIBuffPrinter);
                break;
            case CharacterHUD.RevivalCounter:
                worldPos.y += 0.5f;
                obj = Instantiate(_revivalCounter, _rcGroup);

                Text text_ = obj.GetComponentInChildren<Text>();
                _activeRevivalCounters.Add(instanceID, new TextObject(obj, text_));
                break;
        }

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = UIManager.Instance.GetDynamicPositionByWorldPos(worldPos);

        if (hud != CharacterHUD.RevivalCounter) obj.SetActive(true);
    }

    private void SetActiveUIGroup(int id, bool active)
    {
        _activeHPBars[id].gameObject.SetActive(active);
        _activeBuffPrinters[id].gameObject.SetActive(active);
        if (_activeCTCounters.ContainsKey((id))) _activeCTCounters[id]._obj.SetActive(active);
    }

    private void ClickBuffDesc(Buff_id id, Vector2 pos)
    {
        if (_buffDescObj.activeSelf)
        {
            if (_buffCoroutine != null) StopCoroutine(_buffCoroutine);
            _buffDescObj.SetActive(false);
        }

        Buff_Table buff = (Buff_Table)TableManager.Instance.GetTableValue(Table_resources.Buff_Table, (int)id);

        _buffDesc._text.text = buff.des_text;
        Vector2 target = UIManager.Instance.GetDynamicPositionByWorldPos(pos);
        target.y += 55;
        _buffDesc._rect.anchoredPosition = target;

        _buffCoroutine = StartCoroutine(ShowBuffDesc());
    }

    private IEnumerator ShowBuffDesc()
    {
        _buffDescObj.SetActive(true);
        yield return new WaitForSeconds(2f);
        _buffDescObj.SetActive(false);
    }
}
