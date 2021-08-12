using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIBattleHUD : UIBase
{
    //팝업 
    public GameObject _winPopup;
    public GameObject _losePopup;
    public GameObject _pausePopup;
    public GameObject _screenDim;

    // - 승리 팝업
    public GameObject[] _winPopupStars;
    public Text _winPopupGoldText;
    public Text _winPopupCloverText;
    public GameObject _winPopupNextBtn;

    // - 패매 팝업
    public Text _losePopupGoldText;

    // 전투 정보
    public Text _stageInfoText;
    public Text _turnLimitCountText;
    public GameObject _floatingDamageOrigin;
    public Transform _floatingDamageGroup;

    // 팝업 관련 이벤트
    public static Action<bool> _PauseEvent = null;
    public static Action _goLobbyEvent = null;
    public static Action<int> _changeStageEvent = null;

    // 애니메이터
    public Animator _anim;

    private readonly Queue<TextObjectWithRect> _floatingDamagePool = new Queue<TextObjectWithRect>();
    private int _turnLimit = 0;
    private StringBuilder _stringBuilder = new StringBuilder();
    public override void Init()
    {
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_start_stage, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_update_battle_turn, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_battle_win, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_battle_lose, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_show_floating_damage, this);
    }

    public override void Release()
    {
        StopAllCoroutines();

        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_start_stage, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_update_battle_turn, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_battle_win, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_battle_lose, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_show_floating_damage, this);

        for (int i = 0; i < _winPopupStars.Length; i++)
        {
            if (_winPopupStars[i].activeSelf == true)
            {
                _winPopupStars[i].SetActive(false);
            }
        }

        _winPopupNextBtn.SetActive(false);
        _winPopup.SetActive(false);
        _losePopup.SetActive(false);
        _pausePopup.SetActive(false);
        _screenDim.SetActive(false);

        for (int j = 0; j < _floatingDamageGroup.childCount; j++)
        {
            Destroy(_floatingDamageGroup.GetChild(j).gameObject);
        }

        _floatingDamagePool.Clear();
        _stringBuilder.Clear();
    }

    public override void UpdateUI(UIEvent uiEvent, params object[] values)
    {
        switch (uiEvent)
        {
            case UIEvent.UIEvent_start_stage:
                // 챕터, 인덱스, 턴 제한 수
                _stringBuilder.Append(values[0].ToString()).Append("-").Append(values[1].ToString());
                _stageInfoText.text = _stringBuilder.ToString();
                _stringBuilder.Clear();
                _turnLimit = (int)values[2];
                break;

            case UIEvent.UIEvent_update_battle_turn:
                // 현재 턴 수
                _stringBuilder.Append(values[0].ToString()).Append(" / ").Append(_turnLimit.ToString());
                _turnLimitCountText.text = _stringBuilder.ToString();
                _stringBuilder.Clear();
                break;

            case UIEvent.UIEvent_battle_win:
                //별 개수, 획득 골드, 클로버, 다음 단계 존재 여부
                _screenDim.SetActive(true);
                _winPopup.SetActive(true);

                for (int i = 0; i < (int)values[0]; i++)
                {
                    _winPopupStars[i].SetActive(true);
                }

                _winPopupGoldText.text = values[1].ToString();
                _winPopupCloverText.text = values[2].ToString();
                _winPopupNextBtn.SetActive((bool)values[3]);
                _anim.SetTrigger("Win");
                break;

            case UIEvent.UIEvent_battle_lose:
                //골드
                _screenDim.SetActive(true);
                _losePopup.SetActive(true);
                _losePopupGoldText.text = values[0].ToString();
                _anim.SetTrigger("Lose");
                break;

            case UIEvent.UIEvent_show_floating_damage:
                //데미지, 위치
                if(_floatingDamagePool.Count == 0)
                {
                    GameObject obj = Instantiate(_floatingDamageOrigin, _floatingDamageGroup);

                    Text textCom = obj.GetComponent<Text>();
                    RectTransform rectCom = obj.GetComponent<RectTransform>();

                    _floatingDamagePool.Enqueue(new TextObjectWithRect(rectCom, textCom));
                }

                TextObjectWithRect fd = _floatingDamagePool.Dequeue();
                Color color;
                int damage = Mathf.FloorToInt((float)values[0]);
                if(damage < 0)
                {
                    damage *= -1;
                    color = Color.red;
                }
                else
                {
                    color = Color.green;
                }

                color.a = Const.FLOATING_DAMAGE_ALPHA;
                fd._text.color = color;
                fd._text.text = damage.ToString();
                fd._text.fontSize = Const.FLOATING_DAMAGE_DEFAULT_SIZE + Const.FLOATING_DAMAGE_ENHANCE_SIZE * damage / 10;

                Vector3 worldPos = (Vector3)values[1];
                worldPos.x += Random.Range(Const.FLOATING_DAMAGE_X_MIN, Const.FLOATING_DAMAGE_X_MAX);
                worldPos.y += Random.Range(Const.FLOATING_DAMAGE_Y_MIN, Const.FLOATING_DAMAGE_Y_MAX);
                fd._rect.anchoredPosition = UIManager.Instance.GetDynamicPositionByWorldPos(worldPos);

                StartCoroutine(ShowFloatingDamage(fd));
                break;
        }
    }
    public void ChangePauseState(bool isOn)
    {
        _PauseEvent?.Invoke(isOn);
        _pausePopup.SetActive(isOn);
        _screenDim.SetActive(isOn);
    }

    public void GoLobby()
    {
        _goLobbyEvent?.Invoke();
    }

    public void ChangeStage(int index)
    {
        _changeStageEvent?.Invoke(index);
    }

    private IEnumerator ShowFloatingDamage(TextObjectWithRect fd)
    {
        yield return new WaitForSeconds(0.5f);

        fd._rect.gameObject.SetActive(true);
        RectTransform rect = fd._rect;

        Vector2 destination = rect.anchoredPosition;
        destination.y += 100;

        float elapesd = 0.0f;
        while (true)
        {
            if (elapesd > 1) break;

            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, destination, Time.deltaTime);
            elapesd += Time.deltaTime;
            yield return null;
        }

        fd._rect.gameObject.SetActive(false);
        _floatingDamagePool.Enqueue(fd);
    }
}
