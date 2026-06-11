using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 主动技能条件：施法者所在格雾气未消散（<see cref="Effect_Smoke.IsFogActiveAt"/>）。
/// 正在播放 hide 动画视为无雾。
/// </summary>
public class SkillReady_IfStandTileInFog : ISkillReady
{
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets) { }

    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.moveController?.standTile == null)
            return false;
        if (Effect_Smoke.Instance == null)
            return false;
        return Effect_Smoke.Instance.IsFogActiveAt(user.moveController.standTile.mapPos);
    }
}
