using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 平泽唯被动：入场绑定武器；根据 context「发呆回血」切换寻敌、子弹与攻击动画变体。
/// </summary>
public class PassiveSkill_Yui : ISkillEffect
{
    [LabelText("普攻寻敌"), FoldoutGroup("普攻"), SerializeReference]
    public StraightFindTarget baseFindTarget;
    [LabelText("发呆寻敌"), FoldoutGroup("发呆回血"), SerializeReference]
    public FindTarget_Self healFindTarget;

    [FoldoutGroup("普攻")] public GameObject baseBullet;
    [FoldoutGroup("发呆回血")] public GameObject healBullet;

    Weapon_Sample weapon;
    ShootBullet shoot;
    Chess user;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        this.user = user;
        weapon = user.equipWeapon.weapon as Weapon_Sample;
        shoot = weapon?.attackFunction as ShootBullet;
        if (weapon == null || shoot == null)
        {
            Debug.LogWarning("[PassiveSkill_Yui] 需要 Weapon_Sample + ShootBullet。");
            return;
        }

        user.skillController.context.OnValueChange.AddListener(SyncFromContext);
        user.OnRemove.AddListener(OnChessRemove);
        user.skillController.context.Set(SkillEffect_YuiToggleHealMode.ContextKeyHealMode, false);
    }

    void OnChessRemove(Chess c)
    {
        if (user != null && user.skillController?.context != null)
            user.skillController.context.OnValueChange.RemoveListener(SyncFromContext);
    }

    void SyncFromContext()
    {
        if (user == null || weapon == null || shoot == null) return;
        bool heal = false;
        user.skillController.context.TryGet(SkillEffect_YuiToggleHealMode.ContextKeyHealMode, out heal);
        if (heal)
        {
            weapon.findTarget = healFindTarget;
            shoot.bullet = healBullet;
            user.animatorController.SetAttackAnimationVariant(1);
        }
        else
        {
            weapon.findTarget = baseFindTarget;
            shoot.bullet = baseBullet;
            user.animatorController.SetAttackAnimationVariant(0);
        }
    }
}
