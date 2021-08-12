using System;
using System.Collections.Generic;
using UnityEngine;
public struct Skill_Table
{
    public Skill_id id;
    public bool is_attack;
    public Character_data base_ability;
    public float ability_rate;
    public Buff_id buff;
    public Character_data target_standard;
    public int target_standard_order;
    public int target_count;
    public int cool_time;
    public FX_id fx;
    public string des_text;
    public bool buff_to_self;
}
public struct Character_Table
{
    public Character_id id;
    public Character_type type;
    public float hp;
    public float str;
    public float def;
    public Skill_id skill;
    public bool is_playable;
    public string name_text;
    public int price;
}
public struct Stage_Table
{
    public int id;
    public int chapter;
    public int index;
    public Character_id first_enemy_id;
    public int first_enemy_level;
    public Character_id second_enemy_id;
    public int second_enemy_level;
    public Character_id third_enemy_id;
    public int third_enemy_level;
    public Color sky_color;
    public int reward_gold;
    public float reward_clover;
    public int limit_turn_count;
}
public struct Training_Table
{
    public int id;
    public int required_clover;
    public int chance_of_success;
}
public struct Buff_Table
{
    public Buff_id id;
    public Buff_type type;
    public Character_data target_ability;
    public int continuous_turns;
    public float base_rate;
    public float increase_rate;
    public Apply_point apply_point;
    public Buff_attribute_type attribute_type;
    public Color icon_color;
    public FX_id fx;
    public int skill_level;
    public string des_text;
}
public struct FX_Table
{
    public FX_id id;
    public FX_use_type use_type;
    public FX_dir_type dir_type;
}

[Serializable]
public class Stage_Data
{
    public int id;
    public int star_count;
    public Stage_Data(int ID, int StarCount)
    {
        id = ID;
        star_count = StarCount;
    }
}

[Serializable]
public class Character_Data
{
    public Character_id character_Id;
    public int level;
    public bool is_joined;
    public Character_Data(Character_id CharacterID, int Level, bool IsJoined)
    {
        character_Id = CharacterID;
        level = Level;
        is_joined = IsJoined;
    }
}

[Serializable]
public class Game_Data 
{
    public int gold = 0;
    public int clover = 0;
    public int horn = 0;
    public Dictionary<Character_id, Character_Data> character_datas = null;
    public Dictionary<int , Stage_Data> stage_Datas = null;
    public DateTime horn_charging_time;
    public HashSet<Guide> checked_guides;
    public Game_Data(int Gold, int Clover, int Horn, Dictionary<Character_id, Character_Data> CharacterDatas, Dictionary<int, Stage_Data> StageDatas)
    {
        gold = Gold;
        clover = Clover;
        horn = Horn;
        character_datas = CharacterDatas;
        stage_Datas = StageDatas;
        checked_guides = new HashSet<Guide>();
    }
}
