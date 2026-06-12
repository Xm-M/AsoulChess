using System.Collections;
using UnityEngine;

/// <summary>
/// 莴苣保护伞打断蹦极：下跳阶段强制播放 <see cref="RetreatAnimStateName"/> 返回动画后离场。
/// </summary>
public static class BungeeRetreatHelper
{
    public const string BungeePuppetChessName = "蹦极木偶";
    public const string SkillAnimStateName = "skill";
    public const string RetreatAnimStateName = "skill_raise";
    public const float DropPhaseNormalizedTime = 0.41666666f;
    const float RetreatFinishNormalizedTime = 0.98f;
    const float RetreatTimeoutSeconds = 2f;

    public static bool IsBungeeChess(Chess chess, string[] bungeeNames)
    {
        if (chess == null || chess.IfDeath) return false;
        string name = chess.propertyController?.creator?.chessName;
        if (string.IsNullOrEmpty(name)) return false;
        if (bungeeNames == null || bungeeNames.Length == 0)
            return name == BungeePuppetChessName;
        for (int i = 0; i < bungeeNames.Length; i++)
        {
            if (!string.IsNullOrEmpty(bungeeNames[i]) && bungeeNames[i] == name)
                return true;
        }
        return false;
    }

    public static bool IsInDropPhase(Chess bungee)
    {
        if (!CanInterruptDropPhase(bungee)) return false;
        return IsPlayingSkillDropAnim(bungee);
    }

    static bool CanInterruptDropPhase(Chess bungee)
    {
        if (bungee == null || bungee.IfDeath) return false;
        if (bungee.stateController?.currentState?.state?.stateName != StateName.SkillState)
            return false;
        if (bungee.skillController != null && bungee.skillController.skillEffectFiredThisCast)
            return false;
        if (bungee.skillController?.context != null
            && bungee.skillController.context.TryGet<bool>(BungeeSkillContextKeys.InterruptedByUmbrella, out bool interrupted)
            && interrupted)
            return false;
        if (bungee.skillController?.context != null
            && bungee.skillController.context.TryGet<bool>(BungeeSkillContextKeys.RetreatInProgress, out bool retreating)
            && retreating)
            return false;
        return true;
    }

    static bool IsPlayingSkillDropAnim(Chess bungee)
    {
        var anim = bungee?.animatorController?.animator;
        if (anim == null) return true;

        var info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(SkillAnimStateName))
            return info.normalizedTime < DropPhaseNormalizedTime;

        return !bungee.skillController.skillEffectFiredThisCast;
    }

    public static bool TryForceRetreat(Chess bungee)
    {
        if (!CanInterruptDropPhase(bungee)) return false;

        var sc = bungee.skillController;
        if (sc?.context == null) return false;

        sc.context.Set(BungeeSkillContextKeys.InterruptedByUmbrella, true);
        sc.context.Set(BungeeSkillContextKeys.RetreatInProgress, true);
        sc.context.Remove("霸凌目标");
        sc.context.Remove(BungeeSkillContextKeys.Victims);
        sc.skillEffectFiredThisCast = true;

        PlayRetreatAnimation(bungee);
        bungee.StartCoroutine(RetreatAndLeaveRoutine(bungee));
        return true;
    }

    static void PlayRetreatAnimation(Chess bungee)
    {
        var anim = bungee?.animatorController?.animator;
        if (anim == null) return;

        float speed = bungee.propertyController != null
            ? Mathf.Max(0.01f, bungee.propertyController.GetAccelerate())
            : 1f;
        anim.speed = speed;
        anim.Play(RetreatAnimStateName, 0, 0f);
        anim.Update(0f);

        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(RetreatAnimStateName))
        {
            anim.Play(SkillAnimStateName, 0, DropPhaseNormalizedTime);
            anim.Update(0f);
        }
    }

    static IEnumerator RetreatAndLeaveRoutine(Chess bungee)
    {
        float elapsed = 0f;
        var anim = bungee?.animatorController?.animator;
        bool useRaiseState = anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName(RetreatAnimStateName);

        while (elapsed < RetreatTimeoutSeconds)
        {
            if (bungee == null || bungee.IfDeath)
                yield break;

            if (anim != null)
            {
                var info = anim.GetCurrentAnimatorStateInfo(0);
                if (useRaiseState && info.IsName(RetreatAnimStateName) && info.normalizedTime >= RetreatFinishNormalizedTime)
                    break;
                if (!useRaiseState && info.IsName(SkillAnimStateName) && info.normalizedTime >= RetreatFinishNormalizedTime)
                    break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (bungee != null && !bungee.IfDeath)
            bungee.Death();
    }
}
