using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 律主动：受击时按实际结算伤害（onGetDamage）累加；手动释放；
/// 释放时将累计值写入 SkillContext 后交给 <see cref="SkillEffect_TainakaRitsuDrumSlam"/>，并清零累计。
/// 可用 <see cref="SkillConfig_Defence.defenceTime"/> 作为「至少累计多少伤害才可释放」的整数阈值。
/// </summary>
public class DefenceSkill_TainakaRitsu : SkillBase<SkillConfig_Defence>, ISkillCooldownProgress
{
    [LabelText("释放伤害下限"), Tooltip("大于 0 时优先用此处；否则用 SkillConfig_Defence.defenceTime 作为整数下限")]
    [MinValue(0f)]
    public float minReleaseDamageOverride;

    float _recordedDamage;
    bool _returnCdFlag;

    public override void WhenEnter(Chess user)
    {
        base.WhenEnter(user);
        user.propertyController.onGetDamage.AddListener(OnGetDamage);
    }

    public override void LeaveSkill(Chess user)
    {
        base.LeaveSkill(user);
        user.propertyController.onGetDamage.RemoveListener(OnGetDamage);
    }

    void OnGetDamage(DamageMessege mes)
    {
        if (mes == null) return;
        if (mes.damageType == DamageType.Heal || mes.damageType == DamageType.Miss)
            return;
        _recordedDamage += Mathf.Max(0f, mes.damage);
    }

    float MinToRelease()
    {
        if (minReleaseDamageOverride > 0f)
            return minReleaseDamageOverride;
        if (config != null)
            return Mathf.Max(1f, config.defenceTime);
        return 1f;
    }

    public float GetCooldownProgress01()
    {
        float need = MinToRelease();
        if (need <= 0f)
            return 1f;
        return Mathf.Clamp01(_recordedDamage / need);
    }

    public override bool IfSkillReady(Chess user)
    {
        if (_recordedDamage < MinToRelease())
            return false;
        if (readyChecker == null)
            return true;
        targets.Clear();
        return readyChecker.IfSkillReady(user, config, targets);
    }

    public override void UseSkill(Chess user)
    {
        float snap = _recordedDamage;
        _recordedDamage = 0f;
        user.skillController.context.Set(RitsuSkillContextKeys.RecordedDamageSnap, snap);
        base.UseSkill(user);
    }

    public override void SkillOver(Chess user)
    {
        if (_returnCdFlag)
        {
            _returnCdFlag = false;
            return;
        }
    }

    public override void ReturnCD()
    {
        _returnCdFlag = true;
    }

    public override void WriteToSaveData(SkillStateSaveData data)
    {
        if (data == null) return;
        data.skillType = nameof(DefenceSkill_TainakaRitsu);
        data.Set("recordedDamage", _recordedDamage);
    }

    public override void RestoreFromSaveData(SkillStateSaveData data, Chess user)
    {
        if (data == null) return;
        _recordedDamage = data.GetFloat("recordedDamage", 0f);
    }
}
