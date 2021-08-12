using System;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : Singleton<TableManager>
{
    private readonly Dictionary<Skill_id, Skill_Table> _skillTable = new Dictionary<Skill_id, Skill_Table>();
    private readonly Dictionary<Character_id, Character_Table> _characterTable = new Dictionary<Character_id, Character_Table>();
    private readonly Dictionary<int, Stage_Table> _stageTable = new Dictionary<int, Stage_Table>();
    private readonly Dictionary<int, Training_Table> _trainingTable = new Dictionary<int, Training_Table>();
    private readonly Dictionary<Buff_id, Buff_Table> _buffTable = new Dictionary<Buff_id, Buff_Table>();
    private readonly Dictionary<FX_id, FX_Table> _fxTable = new Dictionary<FX_id, FX_Table>();

    // 챕터 별 스테이지 데이터 분류
    private readonly Dictionary<int, HashSet<int>> _chapterDatas = new Dictionary<int, HashSet<int>>();
    // 플레이어블 캐릭터 분류
    private readonly HashSet<Character_id> _playableCharacters = new HashSet<Character_id>();

    private const string _path = "CSV/";
    public void Init()
    {
        ReadCSV();
        SortStageByChapter();
        FilterPlayableCharacter();
    }
    private void ReadCSV()
    {
        Array tables = Enum.GetValues(typeof(Table_resources));
        var enumerator = tables.GetEnumerator();
        while (enumerator.MoveNext())
        {
            Table_resources table = (Table_resources)enumerator.Current;

            TextAsset data = Resources.Load<TextAsset>(_path + table.ToString());
            string[] lines = data.ToString().Split('\n');

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                if (values.Length == 0 || values[0] == "") continue;

                DeserializeTableData(table, values);
            }

            Resources.UnloadAsset(data);
        }
    }
    private void DeserializeTableData(Table_resources table, string[] values)
    {
        switch (table)
        {
            case Table_resources.Character_Table:
                Character_Table character_Table = new Character_Table();

                character_Table.id = (Character_id)Enum.Parse(typeof(Character_id), values[0]);
                character_Table.type = (Character_type)Enum.Parse(typeof(Character_type), values[1]);
                character_Table.hp = float.Parse(values[2]);
                character_Table.str = float.Parse(values[3]);
                character_Table.def = float.Parse(values[4]);
                character_Table.skill = (Skill_id)Enum.Parse(typeof(Skill_id), values[5]);
                if (bool.TryParse(values[6], out character_Table.is_playable) == false) character_Table.is_playable = false;
                character_Table.name_text = values[7];
                int.TryParse(values[8], out character_Table.price);

                _characterTable.Add(character_Table.id, character_Table);
                break;

            case Table_resources.Skill_Table:
                Skill_Table skill_Table = new Skill_Table();

                skill_Table.id = (Skill_id)Enum.Parse(typeof(Skill_id), values[0]);
                skill_Table.is_attack = bool.Parse(values[1]);
                skill_Table.base_ability = (Character_data)Enum.Parse(typeof(Character_data), values[2]);
                skill_Table.ability_rate = float.Parse(values[3]);
                Enum.TryParse(values[4], out skill_Table.buff);
                skill_Table.target_standard = (Character_data)Enum.Parse(typeof(Character_data), values[5]);
                skill_Table.target_standard_order = int.Parse(values[6]);
                skill_Table.target_count = int.Parse(values[7]);
                if (int.TryParse(values[8], out skill_Table.cool_time) == false) skill_Table.cool_time = 1;
                Enum.TryParse(values[9], out skill_Table.fx);
                skill_Table.des_text = values[10];
                if (bool.TryParse(values[11], out skill_Table.buff_to_self) == false) skill_Table.buff_to_self = false;

                _skillTable.Add(skill_Table.id, skill_Table);
                break;

            case Table_resources.Stage_Table:
                Stage_Table stage_Table = new Stage_Table();

                stage_Table.id = int.Parse(values[0]);
                stage_Table.chapter = int.Parse(values[1]);
                stage_Table.index = int.Parse(values[2]);
                stage_Table.first_enemy_id = (Character_id)Enum.Parse(typeof(Character_id), values[3]);
                stage_Table.first_enemy_level = int.Parse(values[4]);
                stage_Table.second_enemy_id = (Character_id)Enum.Parse(typeof(Character_id), values[5]);
                stage_Table.second_enemy_level = int.Parse(values[6]);
                stage_Table.third_enemy_id = (Character_id)Enum.Parse(typeof(Character_id), values[7]);
                stage_Table.third_enemy_level = int.Parse(values[8]);
                ColorUtility.TryParseHtmlString("#" + values[9], out stage_Table.sky_color);
                stage_Table.reward_gold = int.Parse(values[10]);
                stage_Table.reward_clover = float.Parse(values[11]);
                stage_Table.limit_turn_count = int.Parse(values[12]);

                _stageTable.Add(stage_Table.id, stage_Table);
                break;
            case Table_resources.Training_Table:
                Training_Table training_Table = new Training_Table();

                training_Table.id = int.Parse(values[0]);
                training_Table.required_clover = int.Parse(values[1]);
                training_Table.chance_of_success = int.Parse(values[2]);

                _trainingTable.Add(training_Table.id, training_Table);
                break;
            case Table_resources.Buff_Table:
                Buff_Table buff_Table = new Buff_Table();

                buff_Table.id = (Buff_id)Enum.Parse(typeof(Buff_id), values[0]);
                buff_Table.type = (Buff_type)Enum.Parse(typeof(Buff_type), values[1]);
                Enum.TryParse(values[2], out buff_Table.target_ability);                
                int.TryParse(values[3], out buff_Table.continuous_turns);
                float.TryParse(values[4], out buff_Table.base_rate);
                float.TryParse(values[5], out buff_Table.increase_rate);
                Enum.TryParse(values[6], out buff_Table.apply_point);
                Enum.TryParse(values[7], out buff_Table.attribute_type);
                ColorUtility.TryParseHtmlString("#" + values[8], out buff_Table.icon_color);
                Enum.TryParse(values[9], out buff_Table.fx);
                buff_Table.des_text = values[10];

                _buffTable.Add(buff_Table.id, buff_Table);
                break;
            case Table_resources.FX_Table:
                FX_Table fx_Table = new FX_Table();

                fx_Table.id = (FX_id)Enum.Parse(typeof(FX_id), values[0]);
                fx_Table.use_type = (FX_use_type)Enum.Parse(typeof(FX_use_type), values[1]);
                fx_Table.dir_type = (FX_dir_type)Enum.Parse(typeof(FX_dir_type), values[2]);

                _fxTable.Add(fx_Table.id, fx_Table);
                break;
        }
    }
    public object GetTableValue(Table_resources table, int id)
    {
        switch (table)
        {
            case Table_resources.Character_Table:
                if (_characterTable.ContainsKey((Character_id)id)) return _characterTable[(Character_id)id]; break;
            case Table_resources.Skill_Table: 
                if (_skillTable.ContainsKey((Skill_id)id)) return _skillTable[(Skill_id)id]; break;
            case Table_resources.Stage_Table:
                if (_stageTable.ContainsKey(id)) return _stageTable[id]; break;
            case Table_resources.Training_Table: 
                if (_trainingTable.ContainsKey(id)) return _trainingTable[id]; break;
            case Table_resources.Buff_Table: 
                if (_buffTable.ContainsKey((Buff_id)id)) return _buffTable[(Buff_id)id]; break;
            case Table_resources.FX_Table:
                if (_fxTable.ContainsKey((FX_id)id)) return _fxTable[(FX_id)id]; break;
        }
        return null;
    } 

    public object GetTable(Table_resources table)
    {
        switch (table)
        {
            case Table_resources.Character_Table: return _characterTable; 
            case Table_resources.Skill_Table: return _skillTable; 
            case Table_resources.Stage_Table: return _stageTable; 
            case Table_resources.Training_Table: return _trainingTable; 
            case Table_resources.Buff_Table: return _buffTable;
            case Table_resources.FX_Table: return _fxTable; 
        }
        return null;
    }

    #region Helper
    private void SortStageByChapter()
    {
        var stages = _stageTable.Values.GetEnumerator();

        while (stages.MoveNext())
        {
            Stage_Table cur = stages.Current;
            if (_chapterDatas.ContainsKey(cur.chapter))
            {
                _chapterDatas[cur.chapter].Add(cur.id);
            }
            else
            {
                HashSet<int> stageList = new HashSet<int>();
                stageList.Add(cur.id);

                _chapterDatas.Add(cur.chapter, stageList);
            }
        }
    }

    private void FilterPlayableCharacter()
    {
        var characters = _characterTable.Values.GetEnumerator();

        while (characters.MoveNext())
        {
            Character_Table cur = characters.Current;
            if (cur.is_playable)
            {
                _playableCharacters.Add(cur.id);
            }
        }        
    }

    public HashSet<int> GetStageIDListByChapter(int chapter)
    {
        return _chapterDatas[chapter];
    }

    public HashSet<Character_id> GetPlayableCharacter()
    {
        return _playableCharacters;
    }

    public bool ChapterExist(int chapter)
    {
        return _chapterDatas.ContainsKey(chapter);
    }

    public Character_Table GetLevelAppliedCharacterTable(Character_id id, int level)
    {
        Character_Table table = _characterTable[id];

        int enhance = level - 1;
        table.hp += enhance * Const.HP_ENHANCE_BY_LEVEL;
        table.def += enhance * Const.DEF_ENHANCE_BY_LEVEL;
        table.str += enhance * Const.STR_ENHANCE_BY_LEVEL;

        return table;
    }

    #endregion
}
