using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 按顺序执行多个 <see cref="ISkillEffect"/>，用于 <see cref="SkillBase{TConfig}.effect"/> 只能填一个时的组合。
/// </summary>
[Serializable]
public class SkillEffect_Composite : ISkillEffect
{
    [SerializeReference]
    public List<ISkillEffect> effects = new List<ISkillEffect>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (effects == null) return;
        for (int i = 0; i < effects.Count; i++)
            effects[i]?.SkillEffect(user, config, targets);
    }
}
