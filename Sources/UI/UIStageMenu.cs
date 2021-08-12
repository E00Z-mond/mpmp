using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStageMenu : UIBase
{
    //UI 정보 요소
    public Text _starTotalText;
    public Text _chapterText;
    public GameObject _stageBoxOrigin;
    public Transform _stageGroup;
    public GameObject _startButton;
    public GameObject _nextBtn;
    public GameObject _prevBtn;
    public GameObject _levelTextOrigin;
    public Transform _levelTextGroup;

    private readonly Dictionary<int, UIStageBox> _stageBoxes = new Dictionary<int, UIStageBox>();
    private readonly Text[] _levelTexts = new Text[6];
    private Dictionary<int, Stage_Data> _stageDatas;

    public static Action<int> _stageStartEvent = null;
    public static Action<int, bool> _stageSelectEvent = null;
    private int _currentChapter = 0;
    private int _currentStageID = 0;

    public override void Init()
    {
        _stageDatas = DataController.Instance.GetGameData().stage_Datas;
        UIStageBox._stageSelectCheckEvent += CheckStageSelect;

        //유저의 최상위 스테이지 조회
        var _stageData = _stageDatas.Values.GetEnumerator();
        int maxStage = 0;
        while (_stageData.MoveNext())
        {
            Stage_Data data = _stageData.Current;
            if (data.id > maxStage)
            {
                maxStage = data.id;
            }
        }

        Stage_Table focusedStage = (Stage_Table)TableManager.Instance.GetTableValue(Table_resources.Stage_Table, maxStage);
        _currentChapter = focusedStage.chapter;
        _currentStageID = -1;

        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_set_stage_menu, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_show_battle_btn, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_show_level, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_hide, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_register_level, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_update_level, this);

        //스테이지 박스 생성
        for (int i = 1; i <= Const.MAX_STAGE_COUNT_IN_CHAPTER; i++)
        {
            GameObject stageBox = Instantiate(_stageBoxOrigin, _stageGroup);
            stageBox.SetActive(true);

            UIStageBox stageBoxScript = stageBox.GetComponent<UIStageBox>();

            //Key = 스테이지 인덱스
            _stageBoxes.Add(i, stageBoxScript);
        }

        for (int j = 0; j < Const.FIELD_CHARACTER_COUNT; j++)
        {
            GameObject levelText = Instantiate(_levelTextOrigin, _levelTextGroup);
            levelText.SetActive(true);

            Text textCom = levelText.GetComponent<Text>();
            _levelTexts[j] = textCom;
        }

        gameObject.SetActive(false);
    }

    public override void Release()
    {
        UIStageBox._stageSelectCheckEvent -= CheckStageSelect;

        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_set_stage_menu, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_show_battle_btn, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_show_level, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_hide, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_register_level, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_update_level, this);

        _startButton.SetActive(false);

        var des = _stageBoxes.GetEnumerator();
        while (des.MoveNext())
        {
            Destroy(des.Current.Value.gameObject);
        }

        _stageBoxes.Clear();

        for(int i = 0; i < _levelTexts.Length; i++)
        {
            Destroy(_levelTexts[i].gameObject);
        }

        _levelTexts.Initialize();
    }

    public override void UpdateUI(UIEvent uiEvent, params object[] values)
    {
        switch (uiEvent)
        {
            case UIEvent.UIEvent_set_stage_menu:
                SetStageBox();
                gameObject.SetActive(true);
                break;

            case UIEvent.UIEvent_show_battle_btn:
                _startButton.SetActive((bool)values[0]);
                break;

            case UIEvent.UIEvent_show_level:
                _levelTextGroup.gameObject.SetActive((bool)values[0]);
                break;

            case UIEvent.UIEvent_hide:
                gameObject.SetActive(false);
                _currentStageID = -1;
                _startButton.SetActive(false);
                _levelTextGroup.gameObject.SetActive(false);
                break;

            case UIEvent.UIEvent_register_level:
                //인덱스, 위치, 레벨
                int index = (int)values[0];

                Vector3 worldPos = (Vector3)values[1];
                worldPos.y += 1.9f;
                RectTransform rect = _levelTexts[index].gameObject.GetComponent<RectTransform>();
                rect.anchoredPosition = UIManager.Instance.GetDynamicPositionByWorldPos(worldPos);
                _levelTexts[index].text = "Lv. " + values[2].ToString();
                break;

            case UIEvent.UIEvent_update_level:
                _levelTexts[(int)values[0]].text = "Lv. " + values[1].ToString();
                break;
        }
    }

    private void SetStageBox()
    {
        int _starTotal = 0;
        var stages = TableManager.Instance.GetStageIDListByChapter(_currentChapter).GetEnumerator();

        Stage_Table table;
        while (stages.MoveNext())
        {
            table = (Stage_Table)TableManager.Instance.GetTableValue(Table_resources.Stage_Table, stages.Current);
            if (_stageDatas.ContainsKey(table.id))
            {
                Stage_Data data = _stageDatas[table.id];
                _stageBoxes[table.index].SetData(table.id, table.index, data.star_count);
                _starTotal += data.star_count;
            }
            else
            {
                _stageBoxes[table.index].SetLock();
            }

            if(table.id == _currentStageID)
            {
                _stageBoxes[table.index].Select();
            }
        }

        _starTotalText.text = _starTotal.ToString() + " / " + Const.MAX_STAR_COUNT_IN_CHAPTER;
        _chapterText.text = "Chapter  " + _currentChapter.ToString();

        if (TableManager.Instance.ChapterExist(_currentChapter - 1)) _prevBtn.SetActive(true); else
        {
            _prevBtn.SetActive(false);
        }
        if (TableManager.Instance.ChapterExist(_currentChapter + 1)) _nextBtn.SetActive(true); else
        {
            _nextBtn.SetActive(false);
        }
    }

    public void ChangeChapterBtn(int value)
    {
        _currentChapter += value;

        SetStageBox();
    }

    public void OnClickStartBtn()
    {
        _stageStartEvent?.Invoke(_currentStageID);
    }

    private void CheckStageSelect(int stageID)
    {
        if (_currentStageID == stageID) return;

        bool needToRelease = false;

        if(_currentStageID > 0)
        {
            needToRelease = true;

            Stage_Table table = (Stage_Table)TableManager.Instance.GetTableValue(Table_resources.Stage_Table, _currentStageID);
            if(table.id == _stageBoxes[table.index].GetStageID())
            {
                _stageBoxes[table.index].Deselect();
            }            
        }

        _currentStageID = stageID;
        Stage_Table newTable = (Stage_Table)TableManager.Instance.GetTableValue(Table_resources.Stage_Table, _currentStageID);
        _stageBoxes[newTable.index].Select();

        _stageSelectEvent?.Invoke(_currentStageID, needToRelease);
    }
}
