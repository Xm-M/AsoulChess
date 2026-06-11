using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 跳跃类僵尸被动：本局内首次主动技能真正施放（<see cref="SkillController.onUseSkill"/>）且
/// 技能状态结束（<see cref="SkillController.onSkillOver"/>）后，将
/// <see cref="PropertyController.GetMoveAcceleRate"/> 乘以 <see cref="speedRetainRatio"/>（默认 0.5，即移速减半）。
/// 仅生效一次；读档仅还 CD 而未施法时不会误触发。
/// </summary>
[Serializable]
public class PassiveSkillEffect_HalfMoveSpeedAfterSkill : ISkillEffect
{
    [Tooltip("技能结束后保留的移速倍率（0.5 = 减半）")]
    [Range(0.01f, 1f)]
    public float speedRetainRatio = 0.5f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.skillController == null)
            return;

        bool skillFiredThisCast = false;
        bool slowApplied = false;

        UnityAction<Chess> onUseSkill = null;
        UnityAction<Chess> onSkillOver = null;
        UnityAction<Chess> onRemove = null;

        void Cleanup()
        {
            if (user?.skillController != null)
            {
                if (onUseSkill != null)
                    user.skillController.onUseSkill.RemoveListener(onUseSkill);
                if (onSkillOver != null)
                    user.skillController.onSkillOver.RemoveListener(onSkillOver);
            }
            if (user != null && onRemove != null)
                user.OnRemove.RemoveListener(onRemove);
        }

        onRemove = _ => Cleanup();

        onUseSkill = c =>
        {
            if (c == user)
                skillFiredThisCast = true;
        };

        onSkillOver = c =>
        {
            if (c != user || slowApplied || !skillFiredThisCast)
                return;
            skillFiredThisCast = false;
            slowApplied = true;
            ApplyMoveSpeedPenalty(user);
        };

        user.skillController.onUseSkill.AddListener(onUseSkill);
        user.skillController.onSkillOver.AddListener(onSkillOver);
        user.OnRemove.AddListener(onRemove);
    }

    void ApplyMoveSpeedPenalty(Chess user)
    {
        if (user?.propertyController == null)
            return;
        float cur = user.propertyController.GetMoveAcceleRate();
        float target = cur * speedRetainRatio;
        user.propertyController.ChangeMoveAcceleRate(target - cur);
    }
}
