using UnityEngine;

/// <summary>
/// 维什戴尔主动：技能动画结束后可普攻，但爆裂弹药打完前不可再次释放且 CD 不转；
/// 6 发打完由 <see cref="WisadelBurstMode.EndBurst"/> 调 <see cref="SkillController.SkillOver"/> 开始 CD。
/// </summary>
public class ColdSkill_WisadelBurst : ColdSkill, ISkillFireUseSkillOnEnter
{
    Chess _owner;

    static bool IsBurstActive(Chess user)
    {
        return user?.skillController?.context != null
            && user.skillController.context.TryGet<bool>(WisadelKeys.BurstActive, out bool active)
            && active;
    }

    public override void InitSkill(Chess user)
    {
        base.InitSkill(user);
        _owner = user;
    }

    public void FireUseSkillOnEnter(Chess chess)
    {
        chess.UseSkill();
    }

    public override bool IfSkillReady(Chess user)
    {
        if (IsBurstActive(user))
            return false;

        return base.IfSkillReady(user);
    }

    public new float GetCooldownProgress01()
    {
        if (IsBurstActive(_owner))
            return 0f;

        return base.GetCooldownProgress01();
    }

    public override void SkillOver(Chess user)
    {
        if (IsBurstActive(user))
            return;

        base.SkillOver(user);
    }
}
