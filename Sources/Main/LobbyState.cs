using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyState : GameStateBase
{
    private Game_Data _gameData = null;
    private FieldPostion _lobbyField = null;
    private CameraController _cameraController = null;
    private CoroutineHandler _coroutineHandler = null;

    private readonly Character[] _players = new Character[3];
    private readonly Character[] _enemys = new Character[3];
    private LobbyMenu _curMenu = LobbyMenu.None;
    private Coroutine _curFlow = null;
    //스테이지 선택 연출 진행 중
    private bool _inProgress = false;
    public LobbyState()
    {
        _gameStateType = GameState.LobbyState;
    }
    public override void Init(object value = null)
    {
        _coroutineHandler = Main.GameMain._coroutineHandler;
        _cameraController = Main.GameMain._cameraManager;
        _gameData = DataController.Instance.GetGameData();

#if UNITY_EDITOR
        _coroutineHandler.StartCoroutineCustom(ConsoleCommand());
#endif

        UIManager.Instance.ActiveUI(UIPrefab.UIGoods);
        UIManager.Instance.ActiveUI(UIPrefab.UILobbyTab);
        UIManager.Instance.ActiveUI(UIPrefab.UIStageMenu);
        UIManager.Instance.ActiveUI(UIPrefab.UICharacterMenu);
        UIManager.Instance.ActiveUI(UIPrefab.UIPopup);
        UIManager.Instance.ActiveUI(UIPrefab.UIGuide);

        UIStageMenu._stageSelectEvent += SelectStage;
        UIStageMenu._stageStartEvent += StartStage;
        UILobbyTab._changeLobbyMenuEvent += ActiveMenu;
        UICharacterMenu._characterJoinEvent += ChangeCharacter;

        SetLobbyField();

        UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_goods);
        ActiveMenu(LobbyMenu.Stage);
        //FXManager.Instance.FadeIn();
        SoundManager.Instance.PlayBGM(Sound.LobbyBGM);

        if (_gameData.checked_guides.Contains(Guide.Guide_Team) == false)
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_guide, Guide.Guide_Team);
            _gameData.checked_guides.Add(Guide.Guide_Team);
            DataController.Instance.SaveData();
        }
    }
    private IEnumerator ConsoleCommand()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                DataController.Instance.ResetData();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                _gameData.horn += 100;
                _gameData.gold += 1000;
                _gameData.clover += 100;
                DataController.Instance.SaveData();
                UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_goods);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                for (int i = 0; i < Const.MAX_CHAPTER; i++)
                {
                    HashSet<int> tables = TableManager.Instance.GetStageIDListByChapter(i + 1);

                    var enumer = tables.GetEnumerator();
                    while (enumer.MoveNext())
                    {
                        if (_gameData.stage_Datas.ContainsKey(enumer.Current) == false)
                        {
                            _gameData.stage_Datas.Add(enumer.Current, new Stage_Data(enumer.Current, 2));
                        }
                    }
                }
                DataController.Instance.SaveData();
            } else if (Input.GetKeyDown(KeyCode.H))
            {
                _gameData.horn += 10;
                DataController.Instance.SaveData();
                UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_goods);
            } 
            yield return null;
        }
    }
    public override void Release()
    {
        DeactiveMenu();

        UIManager.Instance.DeactiveUI(UIPrefab.UIGoods);
        UIManager.Instance.DeactiveUI(UIPrefab.UILobbyTab);
        UIManager.Instance.DeactiveUI(UIPrefab.UIStageMenu);
        UIManager.Instance.DeactiveUI(UIPrefab.UICharacterMenu);

        UIStageMenu._stageSelectEvent -= SelectStage;
        UIStageMenu._stageStartEvent -= StartStage;
        UILobbyTab._changeLobbyMenuEvent -= ActiveMenu;
        UICharacterMenu._characterJoinEvent -= ChangeCharacter;

        _curMenu = LobbyMenu.None;
        ResetCharacters(_players);
        Object.Destroy(_lobbyField.gameObject);

        _coroutineHandler = null;
        _gameData = null;
        _cameraController = null;
    }

    #region common
    private void ActiveMenu(LobbyMenu targetMenu)
    {
        if (_curMenu == targetMenu) return;

        DeactiveMenu();
        FXManager.Instance.FadeIn();
        _curMenu = targetMenu;
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_Lobby_Menu_select, _curMenu);

        switch (targetMenu)
        {
            case LobbyMenu.Stage:
                UIManager.Instance.UpdateUI(UIEvent.UIEvent_set_stage_menu);
                break;
            case LobbyMenu.Character:
                UIManager.Instance.UpdateUI(UIEvent.UIEvent_set_character_menu);
                SetPlayersCenter();
                break;
        }
    }

    private void DeactiveMenu()
    {
        if (_curMenu == LobbyMenu.None) return;

        UIManager.Instance.UpdateUI(UIEvent.UIEvent_hide);
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_Lobby_Menu_deselect, _curMenu);

        switch (_curMenu)
        {
            case LobbyMenu.Stage:
                ResetCharacters(_enemys);
                _cameraController.ResetCameraPos();
                if(_curFlow != null) _coroutineHandler.StopCoroutineCustom(_curFlow);
                break;
        }

        for(int i = 0; i < _players.Length; i++)
        {
            _players[i].ResetPos();
        }
    }

    private void SetLobbyField()
    {
        GameObject _lobbyFieldObj = Object.Instantiate((GameObject)Resources.Load(Const.PREFABS_PATH + Prefab.LobbyField.ToString()));
        _lobbyField = _lobbyFieldObj.GetComponent<FieldPostion>();

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
    }

    private void CreateCharacter(Character_id id, bool isPlr, int distance, int level)
    {
        GameObject characterObj = Object.Instantiate(Resources.Load<GameObject>(Const.CHARACTER_PATH + id.ToString()));

        Transform characterPosition = isPlr ? _lobbyField._playerPositions[distance] : _lobbyField._enemyPositions[distance];
        BattleCharacter character = characterObj.GetComponent<BattleCharacter>();
        character.Init();
        character._transform.SetParent(characterPosition);
        character._transform.localPosition = Vector3.zero;
        character._transform.localRotation = Quaternion.Euler(Vector3.zero);
        character._transform.localScale = Vector3.one;

        if (isPlr)
        {
            _players[distance] = character;
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_register_level, new object[] { distance, character._transform.position, level });
        }
        else
        {
            _enemys[distance] = character;
            Vector3 interval = new Vector3(Const.STAGE_TRAVEL_DISTANCE, 0, 0);
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_register_level, new object[] { distance + Const.FIELD_CHARACTER_COUNT / 2, character._transform.position  - interval, level });
        }
    }
    private void ResetCharacters(Character[] characters)
    {
        for(int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null) Object.Destroy(characters[i].gameObject);
        }
    }

    #endregion

    #region stage
    private void SelectStage(int stageID, bool needToRelease)
    {
        if (_inProgress)
        {
            if(_curFlow != null) _coroutineHandler.StopCoroutineCustom(_curFlow);
            FXManager.Instance.ResetFade();
        }

        _curFlow = _coroutineHandler.StartCoroutineCustom(ChangeStageFlow(stageID, needToRelease));
    }

    private IEnumerator ChangeStageFlow(int stageID, bool needToRelease)
    {
        _inProgress = true;

        if (needToRelease)
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_battle_btn, false);
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_level, false);

            Vector2 pos_ = Vector2.zero;
            for (int i = 0; i < _players.Length; i++)
            {
                _players[i].Move(pos_);
            }
            _cameraController.Move(pos_);

            Character actor_ = _players[0];
            yield return new WaitWhile(() => actor_.IsMoving());

            ResetCharacters(_enemys);
        }

        if(stageID > 0)
        {
            FXManager.Instance.FadeIn();
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_battle_btn, true);
            SetStageData(stageID);

            float distance = Vector2.Distance(_players[0].transform.parent.position, _enemys[2].transform.position) - Const.STAGE_FIELD_INTERVAL;
            Vector2 pos = new Vector2(-distance, 0);
            for (int i = 0; i < _players.Length; i++)
            {
                _players[i].Move(pos);
            }
            _cameraController.Move(-pos);

            Character actor = _players[0];
            yield return new WaitWhile(() => actor.IsMoving());

            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_level, true);
        }

        _inProgress = false;
    }

    private void SetStageData(int stageID)
    {
        Stage_Table curStage = (Stage_Table)TableManager.Instance.GetTableValue(Table_resources.Stage_Table, stageID);
        CreateCharacter(curStage.first_enemy_id, false, 0, curStage.first_enemy_level);
        CreateCharacter(curStage.second_enemy_id, false, 1, curStage.second_enemy_level);
        CreateCharacter(curStage.third_enemy_id, false, 2, curStage.third_enemy_level);

        _cameraController.ChangeSkyColor(curStage.sky_color);
    }

    private void StartStage(int stageID)
    {
        if (_gameData.horn > 0)
        {
            if (_gameData.horn == Const.MAX_HORN) Main.GameMain._timeController.SaveHornTime();
            _gameData.horn--;
            DataController.Instance.SaveData();

            Main.GameMain.ChangeState(GameState.BattleState, stageID);
        }
        else
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Not enough horn!");
        }
    }

    #endregion

    #region character
    private void SetPlayersCenter()
    {
        int point = Const.CHARACTER_FIELD_POINT;
        for (int i = 0; i < _players.Length; i++)
        {
            Vector2 pos = new Vector2(point, 0);
            _players[i].Move(pos);
            point++;
        }
    }
    private void ChangeCharacter(Character_id newCharacter, Character_type type)
    {
        int distance = (int)type;
        Character old = _players[distance];
        Vector2 pos = old._transform.localPosition;
        Object.Destroy(old.gameObject);

        CreateCharacter(newCharacter, true, distance, _gameData.character_datas[newCharacter].level);
        _players[distance].Move(pos);
    }

    #endregion
}
