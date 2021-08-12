using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleState : GameStateBase
{
    private PuzzleBlockManager _puzzleBlockManager = null;
    private FieldPostion _battleField = null;
    private CoroutineHandler _coroutineHandler = null;

    private readonly BattleCharacter[] _players = new BattleCharacter[3];
    private readonly BattleCharacter[] _enemys = new BattleCharacter[3];

    private bool _isBattleEnd = false;
    private bool _pauseReturnValue = false;
    private Stage_Table _curStage;
    private int _battleTurns = 1;
    private Game_Data _gameData = null;

    private readonly List<BattleCharacter> _targetCharacter = new List<BattleCharacter>();
    public BattleState()
    {
        _gameStateType = GameState.BattleState;
    }

    public override void Init(object stageID)
    {
        _coroutineHandler = Main.GameMain._coroutineHandler;
        _gameData = DataController.Instance.GetGameData();
        _curStage = (Stage_Table)TableManager.Instance.GetTableValue(Table_resources.Stage_Table, (int)stageID);
        _isBattleEnd = false;
        _battleTurns = 1;

        Main.GameMain._cameraManager.ResetCameraPos();
        UIManager.Instance.ActiveUI(UIPrefab.UIBattleCharacter);
        UIManager.Instance.ActiveUI(UIPrefab.UIBattleHUD);
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_start_stage, new object[] {_curStage.chapter, _curStage.index, _curStage.limit_turn_count });
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_battle_turn, _battleTurns);
        FXManager.Instance.FadeIn();
        SoundManager.Instance.PlayBGM(Sound.BattleBGM);
        SetBattleField();

        PuzzleBlockManager.PuzzlePopEvent += DeliverPuzzle;
        BattleCharacter.CharacterDieEvent += GameResultCheck;
        UIBattleHUD._goLobbyEvent += GoLobby;
        UIBattleHUD._changeStageEvent += ChangeBattleState;
        UIBattleHUD._PauseEvent += ChangePauseState;

        if(_gameData.checked_guides.Contains(Guide.Guide_Battle) == false)
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_guide, Guide.Guide_Battle);
            UIGuide._closeEvent += () => _puzzleBlockManager.SetTurnFlag(true);

            _gameData.checked_guides.Add(Guide.Guide_Battle);
            DataController.Instance.SaveData();
        } else _puzzleBlockManager.SetTurnFlag(true);
    }

    public override void Release()
    {
        UIManager.Instance.DeactiveUI(UIPrefab.UIBattleHUD);
        UIManager.Instance.DeactiveUI(UIPrefab.UIBattleCharacter);
        _gameData = null;
        _coroutineHandler = null;
        Time.timeScale = 1;

        for (int i = 0; i < _players.Length; i++)
        {
            _players[i].Release();
            _enemys[i].Release();
        }
        _puzzleBlockManager.Release();
        Object.Destroy(_battleField.gameObject);
        Object.Destroy(_puzzleBlockManager.gameObject);
        _targetCharacter.Clear();
        _enemys.Initialize();
        _players.Initialize();

        PuzzleBlockManager.PuzzlePopEvent -= DeliverPuzzle;
        BattleCharacter.CharacterDieEvent -= GameResultCheck;
        UIBattleHUD._goLobbyEvent -= GoLobby;
        UIBattleHUD._changeStageEvent -= ChangeBattleState;
        UIBattleHUD._PauseEvent -= ChangePauseState;
    }

    private void SetBattleField()
    {
        GameObject _battleFieldObj = Object.Instantiate(Resources.Load<GameObject>(Const.PREFABS_PATH + Prefab.BattleField.ToString()));
        GameObject _puzzleBoxObj = Object.Instantiate(Resources.Load<GameObject>(Const.PREFABS_PATH + Prefab.PuzzleBox.ToString()));

        _battleField = _battleFieldObj.GetComponent<FieldPostion>();
        _puzzleBlockManager = _puzzleBoxObj.GetComponent<PuzzleBlockManager>();
        _puzzleBlockManager.Init();

        var enumer = _gameData.character_datas.GetEnumerator();
        while (enumer.MoveNext())
        {
            if (enumer.Current.Value.is_joined)
            {
                Character_Data data = enumer.Current.Value;
                Character_Table table = (Character_Table)TableManager.Instance.GetTableValue(Table_resources.Character_Table, (int)data.character_Id);
                CreateCharacter(data.character_Id, true, (int)table.type, data.level);
            }
        }

        CreateCharacter(_curStage.first_enemy_id, false, 0, _curStage.first_enemy_level);
        CreateCharacter(_curStage.second_enemy_id, false, 1, _curStage.second_enemy_level);
        CreateCharacter(_curStage.third_enemy_id, false, 2, _curStage.third_enemy_level);
        Main.GameMain._cameraManager.ChangeSkyColor(_curStage.sky_color);
    }

    private void CreateCharacter(Character_id id, bool isPlr, int distance, int level)
    {
        GameObject characterObj = Object.Instantiate(Resources.Load<GameObject>(Const.CHARACTER_PATH + id.ToString()));

        Transform characterPosition = isPlr ? _battleField._playerPositions[distance] : _battleField._enemyPositions[distance];
        BattleCharacter character = characterObj.GetComponent<BattleCharacter>();
        character.Init(id, distance, level);
        character._transform.SetParent(characterPosition);
        character._transform.localPosition = Vector3.zero;
        character._transform.localRotation = Quaternion.Euler(Vector3.zero);
        character._transform.localScale = Vector3.one;

        if (isPlr)
        {
            _players[distance] = character;
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_register, new object[] { character.GetCharacterInstanceID(), character._transform.position, isPlr });
        }
        else
        {
            _enemys[distance] = character;
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_register, new object[] { character.GetCharacterInstanceID(), character._transform.position, isPlr, character.GetCoolTime() });
        }
    }
    private void DeliverPuzzle(Character_type type, int popCount)
    {
        BattleCharacter actor = _players[(int)type];
        _coroutineHandler.StartCoroutineCustom(TurnFlow(popCount, actor));
    }
    private IEnumerator TurnFlow(int popCount, BattleCharacter actor)
    {
        // 턴 시작 시 발동 버프 체크
        for(int i = 0; i < _players.Length; i++)
        {
            _players[i].CheckApplicableBuff(Apply_point.Apply_point_onStartTurn);
            _enemys[i].CheckApplicableBuff(Apply_point.Apply_point_onStartTurn);
        }

        //플레이어 턴
        if (actor.IsDead())
        {
            actor.TryRevival(popCount);
            yield return null;
        }
        else
        {
            yield return _coroutineHandler.StartCoroutineCustom(SkillFlow(popCount, actor, true));
        }

        //적 턴
        for (int i = 0; i < _enemys.Length; i++)
        {
            BattleCharacter enemy = _enemys[i];
            if (enemy.IsDead()) continue;
            if (enemy.GetCoolTime() == 0)
            {
                yield return _coroutineHandler.StartCoroutineCustom(SkillFlow(1, enemy, false));
            }
            enemy.UpdateCoolTime();
        }

        // 턴 종료 시 종료 시점 버프 적용 & 버프 턴 업데이트 
        for (int i = 0; i < _players.Length; i++)
        {
            _players[i].CheckApplicableBuff(Apply_point.Apply_point_onEndTurn);
            _enemys[i].CheckApplicableBuff(Apply_point.Apply_point_onEndTurn);

            _players[i].UpdateBuff();
            _enemys[i].UpdateBuff();
        }

        if (_isBattleEnd == false)
        {
            _battleTurns++;
            if (_battleTurns > _curStage.limit_turn_count) _coroutineHandler.StartCoroutineCustom(EndBattleFlow(false));
            else
            {
                UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_battle_turn, _battleTurns);
                _puzzleBlockManager.SetTurnFlag(true);
            }
        }
    }

    private IEnumerator SkillFlow(int popCount, BattleCharacter actor, bool isPlayer)
    {
        Skill_Table skillData = actor.GetSkillData();
        GetTarget(isPlayer, skillData);

        if (_targetCharacter.Count == 0) yield break;

        BattleCharacter target = _targetCharacter[0];
        if (skillData.is_attack)
        {
            float dis = Vector2.Distance(actor._transform.position, target._transform.position) - 1f;
            actor.Move(new Vector2(-dis, 0));
            yield return new WaitWhile(() => actor.IsMoving());
        }

        actor.Skill(popCount, _targetCharacter);
        yield return new WaitWhile(() => actor.IsPlaying());

        if (skillData.is_attack)
        {
            actor.Move(Vector2.zero);
            yield return new WaitWhile(() => actor.IsMoving());
        }

        _targetCharacter.Clear();
    }

    private void GetTarget(bool isPlr, Skill_Table skillData)
    {
        BattleCharacter[] targetPool;
        if (isPlr == true)
        {
            if (skillData.is_attack)
            {
                targetPool = _enemys;
            }
            else
            {
                targetPool = _players;
            }
        }
        else
        {
            if (skillData.is_attack)
            {
                targetPool = _players;
            }
            else
            {
                targetPool = _enemys;
            }
        }

        for (int i = 0; i < targetPool.Length; i++)
        {
            if (targetPool[i].IsDead() == false)
            {
                if (skillData.is_attack && targetPool[i].ExistsAttributeTypeBuff(Buff_attribute_type.Buff_attribute_provoke))
                {
                    _targetCharacter.Clear();
                    _targetCharacter.Add(targetPool[i]);
                    return;
                }

                _targetCharacter.Add(targetPool[i]);
            }
        }
        
        while (_targetCharacter.Count > skillData.target_count)
        {
            int pass = 0;
            float temp = _targetCharacter[0].GetData(skillData.target_standard);

            for (int j = 1; j < _targetCharacter.Count; j++)
            {
                float value = _targetCharacter[j].GetData(skillData.target_standard);
                if (skillData.target_standard_order == 1)
                {
                    if (temp >= value)
                    {
                        pass = j;
                        temp = value;
                    }
                }
                else
                {
                    if (temp <= value)
                    {
                        pass = j;
                        temp = value;
                    }
                }
            }
            _targetCharacter.Remove(_targetCharacter[pass]);
        }
    }
    private void GameResultCheck(BattleCharacter actor)
    {
        if (_isBattleEnd) return;

        bool isPlayer = actor.IsPlayable();
        if (isPlayer)
        {
            if (TeamDieCheck(_players)) _coroutineHandler.StartCoroutineCustom(EndBattleFlow(false));
        }
        else
        {
            if (TeamDieCheck(_enemys)) _coroutineHandler.StartCoroutineCustom(EndBattleFlow(true));
        }
    }

    private bool TeamDieCheck(BattleCharacter[] team)
    {
        int dieCount = 0;
        for (int i = 0; i < team.Length; i++)
        {
            if (team[i].IsDead()) dieCount++;
        }

        if (dieCount == team.Length) return true;
        return false;
    }

    private int CheckResultStar()
    {
        int starCount = 0;
        for (int i = 0; i < _players.Length; i++)
        {
            if (_players[i].IsDead() == false) starCount++;
        }
        return starCount;
    }

    private IEnumerator EndBattleFlow(bool isWinned)
    {
        _isBattleEnd = true;
        _puzzleBlockManager.SetTurnFlag(false);

        yield return new WaitForSeconds(1f);
        if (isWinned)
        {
            Stage_Data stageData = _gameData.stage_Datas[_curStage.id];
            int starResult = CheckResultStar();
            if (starResult > stageData.star_count)
            {
                stageData.star_count = starResult;
            }

            int nextStageID = stageData.id + 1;
            bool existNextStage = false;
            if (TableManager.Instance.GetTableValue(Table_resources.Stage_Table, nextStageID) != null)
            {
                if (_gameData.stage_Datas.ContainsKey(nextStageID) == false)
                {
                    Stage_Data nextStage = new Stage_Data(nextStageID, 0);
                    _gameData.stage_Datas.Add(nextStageID, nextStage);
                }
                existNextStage = true;
            }

            int goldAcquired = _curStage.reward_gold * starResult;
            int cloverAcquired = Mathf.FloorToInt(_curStage.reward_clover * starResult);

            _gameData.gold += goldAcquired;
            _gameData.clover += cloverAcquired;
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_battle_win, new object[] { starResult, goldAcquired, cloverAcquired, existNextStage });
        }
        else
        {
            int goldAcquired = Mathf.FloorToInt(_curStage.reward_gold * 0.3f);
            _gameData.gold += goldAcquired;
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_battle_lose, goldAcquired);
        }

        DataController.Instance.SaveData();
        SoundManager.Instance.TurnOffBGM();
    }
    private void ChangeBattleState(int isNextStage)
    {
        if (_gameData.horn > 0)
        {
            if (_gameData.horn == Const.MAX_HORN) Main.GameMain._timeController.SaveHornTime();
            _gameData.horn--;
            DataController.Instance.SaveData();

            Release();
            Init(_curStage.id + isNextStage);
        }
        else
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Not enough horn!");
        }
    }
    private void GoLobby()
    {
        Main.GameMain.ChangeState(GameState.LobbyState);
    }
    private void ChangePauseState(bool isOn)
    {
        if (isOn)
        {
            Time.timeScale = 0;
            _pauseReturnValue = _puzzleBlockManager.GetTurnFlag();
            _puzzleBlockManager.SetTurnFlag(false);
        }
        else
        {
            Time.timeScale = 1;
            if(_pauseReturnValue) _puzzleBlockManager.SetTurnFlag(_pauseReturnValue);
            _pauseReturnValue = false;
        }
    }
}
