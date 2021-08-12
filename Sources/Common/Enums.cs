public enum Character_type
{
    Character_type_none = -1,
    Character_type_tanker,
    Character_type_dealer,
    Character_type_healer
}

public enum Character_data
{
    Character_data_none,
    Character_data_current_hp,
    Character_data_current_hp_rate,
    Character_data_hp,
    Character_data_str,
    Character_data_def,
    Character_data_distance
}

public enum Character_state
{
    Character_state_none = -1,
    Character_state_idle,
    Character_state_skill,
    Character_state_move,
    Character_state_damaged,
    Character_state_die
}

public enum Block_state
{
    Block_state_idle,
    Block_state_focus,
    Block_state_pop,
    Block_state_refill
}

public enum Table_resources
{
    Character_Table,
    Skill_Table,
    Stage_Table,
    Training_Table,
    Buff_Table,
    FX_Table
}

public enum Character_id
{
    Character_id_none,
    Character_dan,
    Character_dillan,
    Character_august,
    Character_siroo,
    Character_norman,
    Character_liamond,
    Character_lorena,
    Character_sun,
    Character_bada,
    Character_badguy,
    Character_geek,
    Character_sadguy,
    Character_coldguy,
    Character_shyguy,
    Character_covertguy
}
public enum Skill_id
{
    Skill_id_none,
    Skill_dan,
    Skill_dillan,
    Skill_august,
    Skill_siroo,
    Skill_norman,
    Skill_liamond,
    Skill_lorena,
    Skill_sun,
    Skill_bada,
    Skill_sadguy,
    Skill_coldguy,
    Skill_badguy,
    Skill_geek,
    Skill_shyguy,
    Skill_covertguy

}
public enum Buff_id
{
    Buff_id_none,
    Buff_def_inc,
    Buff_provoke,
    Buff_str_dec,
    Buff_def_dec,
    Buff_dot_deal,
    Buff_str_inc,
    Buff_dot_heal,
    Buff_off_debuff
}

public enum UIPrefab
{
    UIBattleHUD,
    UIStageMenu,
    UIGoods,
    UILobbyTab,
    UICharacterMenu,
    UIPopup,
    UIBattleCharacter,
    UIGuide
}

public enum UIEvent
{
    //lobby
    UIEvent_set_stage_menu,
    UIEvent_show_battle_btn,
    UIEvent_show_level,
    UIEvent_update_goods,
    UIEvent_show_horn_countdown,
    UIEvent_update_horn,
    UIEvent_Lobby_Menu_select,
    UIEvent_Lobby_Menu_deselect,
    UIEvent_set_character_menu,
    UIEvent_hide,
    UIEvent_register_level,
    UIEvent_update_level,

    //battle
    UIEvent_register,
    UIEvent_hp_update,
    UIEvent_ct_update,
    UIEvent_battle_win,
    UIEvent_battle_lose,
    UIEvent_add_buff,
    UIEvent_remove_buff,
    UIEvent_update_buff,
    UIEvent_dead,
    UIEvent_revival,
    UIEvent_start_stage,
    UIEvent_update_battle_turn,
    UIEvent_show_floating_damage,
    UIEvent_update_rc,

    //common
    UIEvent_show_popup,
    UIEvent_show_guide
}

public enum FX_id
{
    None,
    DeathSkull,
    Heal,
    HitRed,
    SwordSlashWhite,
    SwordWaveBlue,
    Fade,
    GlitterExplosionYellow,
    MagicBuffBlue,
    MagicBuffRed,
    MagicBuffYellow,
    MagicDebuffRed,
    MagicDebuffBlue,
    MagicBuffGreen,
    TouchEffect,
    Poisoned,
    MagicOffDebuff,
    CartoonyPunchHeavy,
    LightningExplosionYellow,
    PoisonExplosion,
    StarExplosionGreen,
    MagicExplosionGreen,
    LaserExplosionGreen,
    PurpleBloodExplosionRoundAnimated,
    SoftPunchExtreme,
    MuzzleFireballFire,
    ExplosionFireball,
    RoundHitRed,
    TwinkleExplosion,
    ShadowExplosion
}

public enum Sound
{
    None,
    LobbyBGM,
    BattleBGM,
    Test,
    Close,
    Purchase,
    Join
}

public enum Prefab
{
    BattleBackground,
    BattleField,
    PuzzleBlock,
    PuzzleBox,
    BattleManager,
    LobbyField
}

public enum GameState
{
    LobbyState,
    BattleState
}

public enum LobbyMenu
{
    None = -1,
    Stage,
    Character,
}

public enum Texture
{
    stage_frame_lock,
    stage_frame_default,
    stage_frame_select,
    stage_star_active,
    stage_star_deactive,
    buff_type_buff,
    buff_type_debuff
} 

public enum Buff_type
{
    Buff_type_buff,
    Buff_type_debuff,
    Buff_type_active
}

public enum Apply_point
{
    Apply_point_none,
    Apply_point_onAttack,
    Apply_point_onDamaged,
    Apply_point_onStartTurn,
    Apply_point_onEndTurn
}

public enum Buff_attribute_type
{
    Buff_attribute_none,
    Buff_attribute_offBuff,
    Buff_attribute_offDebuff,
    Buff_attribute_provoke
}

public enum FX_dir_type
{
    FX_dir_type_directionless,
    FX_dir_type_invert_to_x,
    FX_dir_type_invert_to_z
}

public enum FX_use_type
{
    FX_use_type_single,
    FX_use_type_multi
}

public enum Guide
{
    Guide_Team,
    Guide_Battle
}
