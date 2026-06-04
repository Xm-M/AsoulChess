using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 琴吹䌷被动：默认回复态；根据 context「坚毅形态」切换寻敌、射击、AttackAble 与坚毅 Buff。
/// </summary>
public class PassiveSkill_Tsumugi : ISkillEffect
{
    [LabelText("回复寻敌（整行友方）"), FoldoutGroup("回复"), SerializeReference]
    public FindTarget_SameRowAlliesRight healFindTarget;
    [LabelText("坚毅寻敌（空）"), FoldoutGroup("坚毅"), SerializeReference]
    public FindTarget_Empty sturdyFindTarget;
    [FoldoutGroup("回复")] public GameObject healBullet;

    [SerializeReference]
    [FoldoutGroup("坚毅")]
    public Buff_TsumugiSturdy sturdyBuffTemplate;

    Weapon_Sample weapon;
    ShootBullet_ShootAllTarget healAttack;
    Chess user;
    const string SturdyBuffName = "Buff_TsumugiSturdy";

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        this.user = user;
        weapon = user.equipWeapon.weapon as Weapon_Sample;
        healAttack = weapon?.attackFunction as ShootBullet_ShootAllTarget;
        if (weapon == null || healAttack == null)
        {
            Debug.LogWarning("[PassiveSkill_Tsumugi] 需要 Weapon_Sample + ShootBullet_ShootAllTarget。");
            return;
        }

        if (healFindTarget == null)
            healFindTarget = new FindTarget_SameRowAlliesRight();
        if (sturdyFindTarget == null)
            sturdyFindTarget = new FindTarget_Empty();
        if (sturdyBuffTemplate == null)
            sturdyBuffTemplate = new Buff_TsumugiSturdy();

        healAttack.bullet = healBullet;

        user.skillController.context.OnValueChange.AddListener(SyncFromContext);
        user.OnRemove.AddListener(OnChessRemove);

        if (!user.skillController.context.TryGet<bool>(SkillEffect_TsumugiToggleMode.ContextKeySturdyMode, out _))
            user.skillController.context.Set(SkillEffect_TsumugiToggleMode.ContextKeySturdyMode, false);
        else
            SyncFromContext();
    }

    void OnChessRemove(Chess c)
    {
        if (user != null && user.skillController?.context != null)
            user.skillController.context.OnValueChange.RemoveListener(SyncFromContext);
        RemoveSturdyBuff();
    }

    void SyncFromContext()
    {
        if (user == null || weapon == null || healAttack == null)
            return;

        bool sturdy = false;
        user.skillController.context.TryGet(SkillEffect_TsumugiToggleMode.ContextKeySturdyMode, out sturdy);

        if (sturdy)
        {
            weapon.findTarget = sturdyFindTarget;
            user.equipWeapon.AttackAble = false;
            user.animatorController.ChangeFloat(1);

            var buff = (Buff_TsumugiSturdy)sturdyBuffTemplate.Clone();
            buff.buffName = SturdyBuffName;
            user.buffController.AddBuff(buff);
        }
        else
        {
            RemoveSturdyBuff();
            weapon.findTarget = healFindTarget;
            healAttack.bullet = healBullet;
            weapon.attackFunction = healAttack;
            user.equipWeapon.AttackAble = true;
            user.animatorController.ChangeFloat(0);
        }
    }

    void RemoveSturdyBuff()
    {
        if (user?.buffController == null)
            return;
        if (user.buffController.buffDic.TryGetValue(SturdyBuffName, out var b))
            b.BuffOver();
    }
}
