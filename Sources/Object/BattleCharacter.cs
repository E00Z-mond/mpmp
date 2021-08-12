using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class BattleCharacter : Character
{
    public static Action<BattleCharacter> CharacterDieEvent = null;

    private Transform _parent = null;
    private Character_Table _myCharacter;
    private Skill_Table _mySkill;
    private float _currentHp = 0.0f;
    private int _instanceID = 0;
    private int _distance = 0;
    private int _revivalCount = 0;
    private int _coolTime = 0;
    private bool _isDead = false;
    private float _buffAppliedValue = 0.0f;

    private readonly Dictionary<Buff_id, Buff_Table> _buffList = new Dictionary<Buff_id, Buff_Table>();
    private List<Buff_id> _buffChecker = new List<Buff_id>();
    public void Init(Character_id characterID, int distance, int level)
    {
        Init();

        _myCharacter = TableManager.Instance.GetLevelAppliedCharacterTable(characterID, level);
        _mySkill = (Skill_Table)TableManager.Instance.GetTableValue(Table_resources.Skill_Table, (int)_myCharacter.skill);
        _currentHp = GetData(Character_data.Character_data_hp);
        _distance = distance;
        _coolTime = _mySkill.cool_time;
        _instanceID = GetInstanceID();
        FXManager.Instance.PreLoadSingleFX(_mySkill.fx);
    }
    public void Release()
    {
        _buffList.Clear();
        _buffChecker.Clear();
    }
    public float GetData(Character_data data)
    {
        switch (data)
        {
            case Character_data.Character_data_current_hp:
                return _currentHp;
            case Character_data.Character_data_current_hp_rate:
                return _currentHp / _myCharacter.hp;
            case Character_data.Character_data_hp:
                return _myCharacter.hp;
            case Character_data.Character_data_str:
                return _myCharacter.str;
            case Character_data.Character_data_def:
                return _myCharacter.def;
            case Character_data.Character_data_distance:
                return _distance;
            default:
                return 0;
        }
    }
    public Skill_Table GetSkillData()
    {
        return _mySkill;
    }
    public void Skill(int popCount, List<BattleCharacter> targets)
    {
        int skillLevel = popCount - 1;

        if (_mySkill.buff != Buff_id.Buff_id_none)
        {
            if (_mySkill.buff_to_self)
            {
                AddBuff(_mySkill.buff, skillLevel);
            }
            else
            {
                var applyBuff = targets.GetEnumerator();
                while (applyBuff.MoveNext())
                {
                    applyBuff.Current.AddBuff(_mySkill.buff, skillLevel);
                }
            }
        }

        ChangeState(Character_state.Character_state_skill);
        CheckApplicableBuff(Apply_point.Apply_point_onAttack);
        float power = GetData(_mySkill.base_ability) * _mySkill.ability_rate * (1 + skillLevel * Const.PUZZLE_BUFF_RATE) + _buffAppliedValue;
        power *= Random.Range(Const.ATTACK_HIT_MIN, Const.ATTACK_HIT_MAX);

        var applySkill = targets.GetEnumerator();
        while (applySkill.MoveNext())
        {
            if (_mySkill.is_attack == false)
            {
                applySkill.Current.Healed(power);
            }
            else
            {
                applySkill.Current.Damaged(power);
            }
        }
    }

    public void ShowSkillEffect()
    {
        if (_mySkill.fx == FX_id.None) return;
        if (_parent == null) _parent = _transform.parent;
        FXManager.Instance.MakeWorldFX(_mySkill.fx, _transform.position, (int)_parent.localScale.x);
    }

    public void Damaged(float power)
    {
        CheckApplicableBuff(Apply_point.Apply_point_onDamaged);
        float result = power * ( 1 - (GetData(Character_data.Character_data_def) + _buffAppliedValue) * 0.01f);
        if (result < 0) result = 0;

        ChangeState(Character_state.Character_state_damaged);
        FXManager.Instance.MakeWorldFX(FX_id.HitRed, _transform.position);

        ChangeHP(-result);
    }

    public void Healed(float power)
    {
        ChangeHP(power);
        FXManager.Instance.MakeWorldFX(FX_id.Heal, _transform.position);
    }

    private void ChangeHP(float power)
    {
        float result = _currentHp + power;

        if (result > GetData(Character_data.Character_data_hp)) result = GetData(Character_data.Character_data_hp);
        else if (result < 0) result = 0;

        _currentHp = result;

        UIManager.Instance.UpdateUI(UIEvent.UIEvent_hp_update,
            new object[] { _instanceID, GetData(Character_data.Character_data_current_hp_rate) });
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_floating_damage, new object[] { power, _transform.position });

        if (_currentHp <= 0)
        {
            Dead();
        }
    }
    public void Dead()
    {
        ChangeState(Character_state.Character_state_die);
        _isDead = true;
        _revivalCount = 0;

        FXManager.Instance.MakeWorldFX(FX_id.DeathSkull, _transform.position);
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_dead, _instanceID);
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_rc, new object[] { _instanceID, Const.BLOCK_COUNT_FOR_REVIVAL });

        CharacterDieEvent?.Invoke(this);
    }

    public bool IsDead()
    {
        if (_isDead)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TryRevival(int blockCount)
    {
        _revivalCount += blockCount;

        if (_revivalCount >= Const.BLOCK_COUNT_FOR_REVIVAL)
        {
            Revival();
        }
        else
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_rc, new object[] { _instanceID, Const.BLOCK_COUNT_FOR_REVIVAL - _revivalCount });
        }
    }

    private void Revival()
    {
        _isDead = false;
        _revivalCount = 0;
        _coolTime = _mySkill.cool_time;

        OffAllBuffs();
        ChangeHP(GetData(Character_data.Character_data_hp) * 0.5f);
        ChangeState(Character_state.Character_state_idle);
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_revival, _instanceID);
        FXManager.Instance.MakeWorldFX(FX_id.GlitterExplosionYellow, _transform.position);
    }

    public int GetCoolTime()
    {
        return _coolTime;
    }
    public void UpdateCoolTime()
    {
        if (_coolTime == 0)
        {
            _coolTime = _mySkill.cool_time;
        }
        else
        {
            _coolTime--;
        }
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_ct_update, new object[] { _instanceID, _coolTime });
    }

    public void AddBuff(Buff_id id, int skillLevel)
    {
        Buff_Table buff = (Buff_Table)TableManager.Instance.GetTableValue(Table_resources.Buff_Table, (int)id);

        if(buff.type == Buff_type.Buff_type_active)
        {
            ApplyBuff(id);
            return;
        }

        if (_buffList.ContainsKey(id))
        {
            OffBuff(id);
        }

        buff.skill_level = skillLevel;
        _buffList.Add(id, buff);
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_add_buff, new object[] { _instanceID, buff.id});
    }

    private void ApplyBuff(Buff_id id)
    {
        Buff_Table table = (Buff_Table)TableManager.Instance.GetTableValue(Table_resources.Buff_Table, (int)id);

        if (table.fx != FX_id.None) FXManager.Instance.MakeWorldFX(table.fx, _transform.position);

        switch (table.attribute_type)
        {
            case Buff_attribute_type.Buff_attribute_offBuff:
                OffBuffsByType(Buff_type.Buff_type_buff);
                break;
            case Buff_attribute_type.Buff_attribute_offDebuff:
                OffBuffsByType(Buff_type.Buff_type_debuff);
                break;
            case Buff_attribute_type.Buff_attribute_none:
                //저장된 skill Level 사용을 위해
                Buff_Table buff = _buffList[id];
                var result = GetData(buff.target_ability) * (buff.base_rate + buff.increase_rate * buff.skill_level);
                if (buff.type == Buff_type.Buff_type_debuff) result *= -1;

                switch (buff.target_ability)
                {
                    case Character_data.Character_data_hp:
                        ChangeHP(result);
                        break;
                    case Character_data.Character_data_def:
                    case Character_data.Character_data_str:
                        _buffAppliedValue += result;
                        break;
                }
                break;
        }
    }

    public void UpdateBuff()
    {
        if (IsDead()) return;
        if (_buffList.Count == 0) return;

        _buffChecker = new List<Buff_id>(_buffList.Keys);

        for(int i = 0; i < _buffChecker.Count; i++)
        {
            Buff_Table buff = _buffList[_buffChecker[i]];
            buff.continuous_turns--;

            if (buff.continuous_turns <= 0)
            {
                OffBuff(buff.id);
            }
            else
            {
                // 잔여 턴 수 갱신
                _buffList[buff.id] = buff;
                UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_buff, new object[] { _instanceID, buff.id, buff.continuous_turns });
            }
        }

        _buffChecker.Clear();
    }

    public void CheckApplicableBuff(Apply_point point)
    {
        if (IsDead()) return;
        _buffAppliedValue = 0;

        if (_buffList.Count == 0) return;

        var enumer = _buffList.GetEnumerator();
        while (enumer.MoveNext())
        {
            Buff_Table buff = _buffList[enumer.Current.Key];
            if(buff.apply_point == point)
            {
                ApplyBuff(enumer.Current.Key);
            }
        }
    }

    private void OffBuffsByType(Buff_type type)
    {
        if (_buffList.Count == 0) return;

        var enumer = _buffList.GetEnumerator();
        while (enumer.MoveNext())
        {
            if(enumer.Current.Value.type == type)
            {
                _buffChecker.Add(enumer.Current.Key);
            }
        }

        for(int i = 0; i < _buffChecker.Count; i++)
        {
            OffBuff(_buffChecker[i]);
        }

        _buffChecker.Clear();
    }

    private void OffBuff(Buff_id id)
    {
        _buffList.Remove(id);
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_remove_buff, new object[] { _instanceID, id });
    }

    private void OffAllBuffs()
    {
        var off = _buffList.GetEnumerator();
        while (off.MoveNext())
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_remove_buff, new object[] { _instanceID, off.Current.Key });
        }

        _buffList.Clear();
    }

    public bool ExistsAttributeTypeBuff(Buff_attribute_type attribute)
    {
        if (_buffList.Count == 0) return false;

        Buff_Table buff;
        var enumer = _buffList.GetEnumerator();
        while (enumer.MoveNext())
        {
            buff = (Buff_Table)TableManager.Instance.GetTableValue(Table_resources.Buff_Table, (int)enumer.Current.Key);
            if (buff.attribute_type == attribute) return true;
        }
        return false;
    }

    public bool IsPlayable()
    {
        return _myCharacter.is_playable;
    }

    public int GetCharacterInstanceID()
    {
        return _instanceID;
    }
}
