using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 蹦极僵尸主动：按 <see cref="BungeeSkillContextKeys.Victims"/> 与 <see cref="targetSprites"/> 下标，
/// 将每个 <see cref="SpriteRenderer"/> 的 <c>transform.position</c> 设到对应目标世界坐标，sprite 设为目标当前外观
/// （<see cref="AnimatorController.sprite"/>）；再依次 <see cref="Chess.Death"/>。若上下文无列表则回退仅处理 <c>霸凌目标</c>。
/// </summary>
public class SkillEffect_Bungee : ISkillEffect
{
    [LabelText("抱走展示用 SpriteRenderer 列表")]
    [Tooltip("与被动 PassiveSkill_Bungee 上列表一一对应；按 Victims 顺序写入位置与 sprite。")]
    public List<SpriteRenderer> targetSprites = new List<SpriteRenderer>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || user.IfDeath) return;

        if (!user.skillController.context.TryGet<List<Chess>>(BungeeSkillContextKeys.Victims, out var victims)
            || victims == null
            || victims.Count == 0)
        {
            if (!user.skillController.context.TryGet<Chess>("霸凌目标", out var only) || only == null || only.IfDeath)
                return;
            victims = new List<Chess> { only };
        }

        int nShow = targetSprites != null ? targetSprites.Count : 0;
        for (int i = 0; i < victims.Count && i < nShow; i++)
        {
            var victim = victims[i];
            var sr = targetSprites[i];
            if (victim == null || victim.IfDeath || sr == null) continue;

            sr.transform.position = victim.transform.position;

            var bodySr = victim.animatorController != null ? victim.animatorController.sprite : null;
            sr.sprite = bodySr != null ? bodySr.sprite : null;
        }

        for (int i = 0; i < victims.Count; i++)
        {
            var victim = victims[i];
            if (victim != null && !victim.IfDeath)
                victim.Death();
        }

        for (int i = victims.Count; i < nShow; i++)
        {
            if (targetSprites[i] != null)
                targetSprites[i].sprite = null;
        }

        user.skillController.context.Remove("霸凌目标");
        user.skillController.context.Remove(BungeeSkillContextKeys.Victims);
    }
}
