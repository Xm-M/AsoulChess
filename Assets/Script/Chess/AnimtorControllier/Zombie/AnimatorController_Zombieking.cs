using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>与 <c>僵王.md</c> 三档破损一致：&gt;80% / 50~80% / 10~50%；&lt;10% 仍用第三档精灵。</summary>
[System.Serializable]
public class ZombieKingTiered3Sprites
{
    [Tooltip("血量 >80%")]
    public Sprite intact;
    [Tooltip("血量 50%~80%")]
    public Sprite damagedLight;
    [Tooltip("血量 10%~50%；低于10%沿用本档")]
    public Sprite damagedHeavy;

    public Sprite GetForDamageIndex(int damageIndex012)
    {
        damageIndex012 = Mathf.Clamp(damageIndex012, 0, 2);
        if (damageIndex012 == 0) return intact;
        if (damageIndex012 == 1) return damagedLight != null ? damagedLight : intact;
        return damagedHeavy != null ? damagedHeavy : damagedLight != null ? damagedLight : intact;
    }

    public bool HasAnySprite => intact != null || damagedLight != null || damagedHeavy != null;
}

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
    /// <summary>召唤/吐球：<b>动画行</b> 1~5（与 anim_spawn_/head_attack_ 一致；anim 1 = 地图最上行）。</summary>
    public const string Row = "zombieKingRow";
    /// <summary>踩踏：<b>动画带</b> 1~4（与 anim_stomp_ 一致；与游戏「从下数」行带镜像，见 <see cref="ZombieKingMapAnim"/>）。</summary>
    public const string StompBand = "zombieKingStompBand";
    /// <summary>吐球视觉：例如 0=火 1=冰；Animator 不读，仅给换眼部/嘴部贴图用。</summary>
    public const string BallVisual = "zombieKingBallVisual";
    /// <summary>召唤时在僵王技能维护的召唤池列表中的下标（本帧写入，UseSkill 读取）。</summary>
    public const string SpawnPoolIndex = "zombieKingSpawnPoolIndex";
}

/// <summary>
/// 地图坐标与僵王动画下标对齐：地图以左下为原点，<c>tileY=0</c> 为最<b>下</b>一行（游戏内第 1 行）；
/// 资源动画 <c>anim_spawn_1</c> / <c>head_attack_1</c> 表示最<b>上</b>方一行（即游戏第 5 行，当共 5 行时）。
/// </summary>
public static class ZombieKingMapAnim
{
    public static int TileYToAnimRow(int tileY, int mapHeightY) =>
        Mathf.Clamp(mapHeightY - tileY, 1, 5);

    public static int AnimRowToTileY(int animRow, int mapHeightY) =>
        Mathf.Clamp(mapHeightY - animRow, 0, Mathf.Max(0, mapHeightY - 1));

    /// <summary>游戏「从下数」的踩踏带：带 b 覆盖游戏行 b 与 b+1（tileY 为 b-1 与 b），共 mapHeightY-1 带。</summary>
    public static int GameStompBandToAnimStompBand(int gameBand, int mapHeightY)
    {
        int anim = mapHeightY - gameBand;
        int maxAnim = Mathf.Min(4, Mathf.Max(1, mapHeightY - 1));
        return Mathf.Clamp(anim, 1, maxAnim);
    }

    /// <summary>Animator 的 stomp 下标 → 游戏踩踏带（用于落伤害格子）。</summary>
    public static int AnimStompBandToGameStompBand(int animBand, int mapHeightY)
    {
        int game = mapHeightY - animBand;
        return Mathf.Clamp(game, 1, Mathf.Max(1, mapHeightY - 1));
    }
}

/// <summary>
/// 原版僵王：待机形态由 context <c>stand</c> 与 <see cref="PlayIdle"/> 同步；技能动画由 <see cref="PlaySkill"/> 根据 context 分发。
/// </summary>
public class AnimatorController_Zombieking : AnimatorController
{
    bool stand;
    bool firstEnter;

    [Header("分部位 SpriteRenderer（Prefab 拖拽）")]
    [Tooltip("右脚；受损档由 footRightSprites 驱动")]
    public SpriteRenderer footRight;
    [Tooltip("左脚")]
    public SpriteRenderer footLeft;
    [Tooltip("头部")]
    public SpriteRenderer head;
    [Tooltip("下巴")]
    public SpriteRenderer jaw;
    [Tooltip("上半身躯干；受击时与主 sprite 同步材质闪烁")]
    public SpriteRenderer upperBody;
    [Tooltip("外侧手臂；受击时同步闪烁")]
    public SpriteRenderer outerArm;
    [Tooltip("眼部高光；吐球时切 fire/ice 组")]
    public SpriteRenderer eyeGlow;
    [Tooltip("嘴部高光；吐球时切 fire/ice 组")]
    public SpriteRenderer mouthGlow;

    [Header("脚部 / 头 / 下巴 — 三档贴图")]
    public ZombieKingTiered3Sprites footRightSprites;
    public ZombieKingTiered3Sprites footLeftSprites;
    public ZombieKingTiered3Sprites headSprites;
    public ZombieKingTiered3Sprites jawSprites;

    [Header("眼 / 嘴高光 — 均与血量无关（各一张 Sprite）")]
    [Tooltip("非吐球技能态时眼高光")]
    public Sprite eyeGlowIdleSprite;
    [Tooltip("吐火球时眼高光")]
    public Sprite eyeGlowFireSprite;
    [Tooltip("吐冰球时眼高光")]
    public Sprite eyeGlowIceSprite;
    [Tooltip("非吐球技能态时嘴高光")]
    public Sprite mouthGlowIdleSprite;
    [Tooltip("吐火球时嘴高光")]
    public Sprite mouthGlowFireSprite;
    [Tooltip("吐冰球时嘴高光")]
    public Sprite mouthGlowIceSprite;

    int _lastAppliedDamageIdx = int.MinValue;
    int _lastAppliedBallVisual = int.MinValue;

    public override void InitController(Chess chess)
    {
        base.InitController(chess);
        chess.transform.right = Vector2.right;
        firstEnter = false;
        stand = true;
        _lastAppliedDamageIdx = int.MinValue;
        _lastAppliedBallVisual = int.MinValue;
        float hp = chess.propertyController != null ? chess.propertyController.GetHpPerCent() : 1f;
        SyncPartSpritesFromContext(hp);
    }
    public override void PlayIdle()
    {
        if (!firstEnter)
        {
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
                ApplyGlowFromContext(ctx);
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

    public override void OnGetDamage(DamageMessege dm)
    {
        base.OnGetDamage(dm);
        FlashSpriteRendererMaterial(head);
        FlashSpriteRendererMaterial(jaw);
        FlashSpriteRendererMaterial(upperBody);
        FlashSpriteRendererMaterial(outerArm);
    }

    /// <summary>
    /// 由 <see cref="Skill_ZombieKingBoss.RefreshPresentation"/> 每帧或血量变化时调用：
    /// 按血量得到破损档（与 <see cref="AnimatorController.SetVisualTierPublic"/> 的 0~3 档一致，&lt;10% 仍用第 3 档精灵），
    /// 脚/头/下巴按破损档；眼、嘴高光均为单张精灵（与血量无关），吐球时在 <see cref="SkillState"/> 下按火/冰切换。
    /// </summary>
    public void SyncPartSpritesFromContext(float hpPercent01)
    {
        int visualTier = hpPercent01 > 0.8f ? 0 : hpPercent01 > 0.5f ? 1 : hpPercent01 > 0.1f ? 2 : 3;
        int damageIdx = Mathf.Clamp(visualTier, 0, 2);

        int ballVisual = -1;
        bool inSkillState = chess.stateController?.currentState?.state != null
            && chess.stateController.currentState.state.stateName == StateName.SkillState;
        if (inSkillState && chess.skillController?.context != null)
        {
            var ctx = chess.skillController.context;
            if (ctx.TryGet<int>(ZombieKingContextKeys.SkillAnimKind, out int kindInt)
                && kindInt == (int)ZombieKingSkillAnimKind.FireIceBall
                && ctx.TryGet<int>(ZombieKingContextKeys.BallVisual, out int bv))
                ballVisual = Mathf.Clamp(bv, 0, 1);
        }

        if (damageIdx == _lastAppliedDamageIdx && ballVisual == _lastAppliedBallVisual)
            return;
        _lastAppliedDamageIdx = damageIdx;
        _lastAppliedBallVisual = ballVisual;

        ApplyTierSprite(footRight, footRightSprites, damageIdx);
        ApplyTierSprite(footLeft, footLeftSprites, damageIdx);
        ApplyTierSprite(head, headSprites, damageIdx);
        ApplyTierSprite(jaw, jawSprites, damageIdx);

        ApplyGlowSprites(ballVisual);
    }

    static void ApplyTierSprite(SpriteRenderer sr, ZombieKingTiered3Sprites group, int damageIdx012)
    {
        if (sr == null || group == null || !group.HasAnySprite)
            return;
        var sp = group.GetForDamageIndex(damageIdx012);
        if (sp == null)
            return;
        sr.sprite = sp;
        sr.enabled = true;
    }

    void ApplyGlowFromContext(SkillContext ctx)
    {
        if (ctx == null || chess?.propertyController == null)
            return;
        float hp = chess.propertyController.GetHpPerCent();
        int damageIdx = Mathf.Clamp(hp > 0.8f ? 0 : hp > 0.5f ? 1 : hp > 0.1f ? 2 : 3, 0, 2);
        int bv = 0;
        if (ctx.TryGet<int>(ZombieKingContextKeys.BallVisual, out int v))
            bv = Mathf.Clamp(v, 0, 1);
        ApplyGlowSprites(bv);
        _lastAppliedDamageIdx = damageIdx;
        _lastAppliedBallVisual = bv;
    }

    void ApplyGlowSprites(int ballVisualMinus1OrNeg1)
    {
        if (ballVisualMinus1OrNeg1 == 0)
        {
            ApplyGlowOneSingle(eyeGlow, eyeGlowFireSprite);
            ApplyGlowOneSingle(mouthGlow, mouthGlowFireSprite);
        }
        else if (ballVisualMinus1OrNeg1 == 1)
        {
            ApplyGlowOneSingle(eyeGlow, eyeGlowIceSprite);
            ApplyGlowOneSingle(mouthGlow, mouthGlowIceSprite);
        }
        else
        {
            ApplyGlowOneSingle(eyeGlow, eyeGlowIdleSprite);
            ApplyGlowOneSingle(mouthGlow, mouthGlowIdleSprite);
        }
    }

    static void ApplyGlowOneSingle(SpriteRenderer sr, Sprite sprite)
    {
        if (sr == null)
            return;
        if (sprite == null)
        {
            sr.enabled = false;
            return;
        }
        sr.sprite = sprite;
        sr.enabled = true;
    }

}
