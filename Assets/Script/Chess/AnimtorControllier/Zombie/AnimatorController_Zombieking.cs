using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>僵王本次技能要播的动画类别；由技能/AI 在进入 <see cref="SkillState"/> 前写入 <see cref="SkillContext"/>。</summary>
public enum ZombieKingSkillAnimKind
{
    /// <summary>站立：召唤僵尸，<see cref="ZombieKingContextKeys.Row"/> 1~5 对应 anim_spawn_1~5。</summary>
    SpawnZombie = 0,
    /// <summary>站立：蹦极进场 anim_bungee_1_enter。</summary>
    BungeeEnter = 1,
    /// <summary>站立：蹦极离场 anim_bungee_1_leave。</summary>
    BungeeLeave = 2,
    /// <summary>站立：踩踏，<see cref="ZombieKingContextKeys.StompBand"/> 1~4 对应行带 12 / 23 / 34 / 45 → anim_stomp_1~4。</summary>
    Stomp = 3,
    /// <summary>站立：投车 anim_RV_1。</summary>
    ThrowCar = 4,
    /// <summary>俯身：吐冰/火球，<see cref="ZombieKingContextKeys.Row"/> 1~5 → anim_head_attack_1~5；冰火贴图由 <see cref="ZombieKingContextKeys.BallVisual"/> 等在特效里处理。</summary>
    FireIceBall = 5,
}

public static class ZombieKingContextKeys
{
    public const string SkillAnimKind = "zombieKingSkillAnimKind";
    /// <summary>召唤/吐球目标行，1~5。</summary>
    public const string Row = "zombieKingRow";
    /// <summary>踩踏带：1=1~2行、2=2~3、3=3~4、4=4~5，对应 anim_stomp_1~4。</summary>
    public const string StompBand = "zombieKingStompBand";
    /// <summary>吐球视觉：例如 0=火 1=冰；Animator 不读，仅给换眼部/嘴部贴图用。</summary>
    public const string BallVisual = "zombieKingBallVisual";
}

/// <summary>
/// 原版僵王：待机形态由 context <c>stand</c> 与 <see cref="PlayIdle"/> 同步；技能动画由 <see cref="PlaySkill"/> 根据 context 分发。
/// </summary>
public class AnimatorController_Zombieking : AnimatorController
{
    bool stand;
    bool firstEnter;
    public override void InitController(Chess chess)
    {
        base.InitController(chess);
        chess.transform.right = Vector2.right;
        firstEnter = false;
        stand = true;
    }
    public override void PlayIdle()
    {
        if (!firstEnter)
        {
            Debug.Log("第一次播放");
            firstEnter = true;
            animator.Play("anim_enter");
            chess.skillController.context.Set<bool>("stand",true);
            stand = true;
            return;
        }
        bool s = false;
        chess.skillController.context.TryGet<bool>("stand",out s);
        if (stand==s) {
            if (stand)
            {
                animator.Play("anim_idle");
            } else
            {
                animator.Play("anim_head_idle");
            }
        } 
        else //如果不一样说明要切换形态
        {
            if (s)//s是战力 stand是俯身 那就是俯身->站立 head_leave
            {
                animator.Play("anim_head_leave");
            }
            else
            {
                animator.Play("anim_head_enter");
            }
            stand = s;
        }
    }
    public override void PlaySkill()
    {
        if (animator == null || chess?.skillController?.context == null)
            return;

        var ctx = chess.skillController.context;
        if (!ctx.TryGet<int>(ZombieKingContextKeys.SkillAnimKind, out int kindInt))
        {
            animator.Play("anim_head_attack_1");
            return;
        }

        var kind = (ZombieKingSkillAnimKind)kindInt;
        switch (kind)
        {
            case ZombieKingSkillAnimKind.SpawnZombie:
            {
                int row = 1;
                ctx.TryGet<int>(ZombieKingContextKeys.Row, out row);
                row = Mathf.Clamp(row, 1, 5);
                animator.Play($"anim_spawn_{row}");
                break;
            }
            case ZombieKingSkillAnimKind.BungeeEnter:
                animator.Play("anim_bungee_1_enter");
                break;
            case ZombieKingSkillAnimKind.BungeeLeave:
                animator.Play("anim_bungee_1_leave");
                break;
            case ZombieKingSkillAnimKind.Stomp:
            {
                int band = 1;
                ctx.TryGet<int>(ZombieKingContextKeys.StompBand, out band);
                band = Mathf.Clamp(band, 1, 4);
                animator.Play($"anim_stomp_{band}");
                break;
            }
            case ZombieKingSkillAnimKind.ThrowCar:
                animator.Play("anim_RV_1");
                break;
            case ZombieKingSkillAnimKind.FireIceBall:
            {
                int row = 1;
                ctx.TryGet<int>(ZombieKingContextKeys.Row, out row);
                row = Mathf.Clamp(row, 1, 5);
                animator.Play($"anim_head_attack_{row}");
                break;
            }
            default:
                animator.Play("anim_head_attack_1");
                break;
        }
    }
    public void UseSkill()
    {
        chess.UseSkill();
    }
    public override void PlayDeath()
    {
        animator.Play(("anim_death"));
    }
}
