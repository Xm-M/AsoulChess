using UnityEngine;

/// <summary>可选：爆裂弹药耗尽前视为技能未结束（若整段卡在 SkillState 时使用）。</summary>
public class SkillFinish_WisadelBurstAmmo : ISkillFinish
{
    public bool IsFinished(Chess user, SkillConfig config, SkillRuntimeInfo runtime)
    {
        if (user?.skillController?.context == null)
            return true;

        if (!user.skillController.context.TryGet<bool>(WisadelKeys.BurstActive, out bool active) || !active)
            return true;

        if (!user.skillController.context.TryGet<int>(WisadelKeys.BurstAmmo, out int ammo))
            return true;

        return ammo <= 0;
    }
}
