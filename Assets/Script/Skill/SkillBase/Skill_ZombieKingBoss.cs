using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 僵王主动：站立队列、俯身 10s 吐球 / 20s 回站立、召唤池与 <see cref="ZombieKingContextKeys"/>。
/// 需求：<c>docs/requirements/僵王Boss行为.md</c> 与 <c>僵王.md</c>。
/// </summary>
public class Skill_ZombieKingBoss : SkillBase<SkillConfig_Cold>
{
    public const float BungeeHpGate = 0.8f;

    [Title("配置")]
    [Tooltip("全种类僵尸配置；解锁条件 bendCount*2 > baseProperty.waveLimit")]
    public List<PropertyCreator> zombieTypes = new List<PropertyCreator>();

    [Tooltip("蹦极僵尸；血量 <80% 且队列需要时生成 3 只")]
    public PropertyCreator bungeeZombie;

    [Tooltip("火球预制体（俯身吐球 BallVisual=0）")]
    public GameObject fireBallPrefab;
    [Tooltip("冰球预制体（BallVisual=1）")]
    public GameObject iceBallPrefab;

    [Tooltip("可选：冒烟根物体，血量 ≤50% 激活")]
    public GameObject smokeVfxRoot;

    [SerializeField, Tooltip("调试：强制满足 readyChecker 后每帧可 cast")]
    bool debugForceReady;

    [Title("运行时（只读）")]
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    int bendCount;

    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    ZombieKingBossPhase phase;

    /// <summary>当前站立阶段可随机到的召唤种类（由 bendCount 与 waveLimit 推导）。</summary>
    [NonSerialized]
    readonly List<PropertyCreator> zombieCanSummons = new List<PropertyCreator>();

    enum ZombieKingBossPhase
    {
        StandingQueue,
        StandingPostDelay,
        Crouching,
    }

    enum StandActionKind
    {
        Spawn,
        BungeeEnter,
        BungeeLeave,
        Stomp,
        ThrowCar,
    }

    [Serializable]
    class QueuedStandAction
    {
        public StandActionKind Kind;
    }

    readonly List<QueuedStandAction> standQueue = new List<QueuedStandAction>();
    int queueIndex;
    float castCooldownTimer;
    float postStandDelayTimer;
    float crouchTimer;
    bool fireballPlayed;
    bool returnCd;
    /// <summary>已写好 Context 并返回过 true，在 <see cref="SkillOver"/> 前不再重复 IfSkillReady=true。</summary>
    bool castPending;
    int lastVisualTier = -1;
    bool smokeActivated;
    Coroutine flashCo;
    bool lastCastWasFireball;
    /// <summary>登场或俯身→站立后倒计时，期间不进入站立技能（与 anim_enter / anim_head_leave 等衔接）。</summary>
    float standRiseSkillLockTimer;

    public const float StandSkillLockAfterRiseFromCrouch = 5f;

    public override void InitSkill(Chess user)
    {
        base.InitSkill(user);
        bendCount = 0;
        standQueue.Clear();
        queueIndex = 0;
        phase = ZombieKingBossPhase.StandingQueue;
        castCooldownTimer = 0f;
        postStandDelayTimer = 0f;
        crouchTimer = 0f;
        fireballPlayed = false;
        castPending = false;
        standRiseSkillLockTimer = 0f;
        zombieCanSummons.Clear();
    }

    public override void WhenEnter(Chess user)
    {
        base.WhenEnter(user);
        EnterStandingInitial(user);
    }

    public override void LeaveSkill(Chess user)
    {
        base.LeaveSkill(user);
        if (user != null && flashCo != null)
        {
            user.StopCoroutine(flashCo);
            flashCo = null;
        }
    }

    public override bool IfSkillReady(Chess user)
    {
        if (config == null || user?.propertyController == null || user.skillController?.context == null)
            return false;
        if (user.moveController?.standTile == null || MapManage.instance == null)
            return false;

        float dt = Time.deltaTime;
        float accel = Mathf.Max(0.01f, user.propertyController.GetAccelerate());

        RefreshPresentation(user);

        if (debugForceReady)
        {
            targets ??= new List<Chess>();
            targets.Clear();
            if (readyChecker != null && !readyChecker.IfSkillReady(user, config, targets))
                return false;
            if (castPending)
                return false;
            if (standRiseSkillLockTimer > 0f)
                return false;
            PrepareDebugCast(user);
            castPending = true;
            return true;
        }

        if (castPending)
            return false;

        switch (phase)
        {
            case ZombieKingBossPhase.StandingQueue:
                if (standRiseSkillLockTimer > 0f)
                {
                    standRiseSkillLockTimer -= dt * accel;
                    return false;
                }

                if (queueIndex >= standQueue.Count)
                {
                    phase = ZombieKingBossPhase.StandingPostDelay;
                    postStandDelayTimer = 0f;
                    return false;
                }

                castCooldownTimer += dt * accel;
                float needCast = GetSpawnInterval(user);
                if (castCooldownTimer < needCast)
                    return false;

                castCooldownTimer = 0f;
                if (!TryPrepareStandCast(user))
                    return false;
                castPending = true;
                return true;

            case ZombieKingBossPhase.StandingPostDelay:
                postStandDelayTimer += dt * accel;
                if (postStandDelayTimer >= 2f)
                    EnterCrouchPhase(user);
                return false;

            case ZombieKingBossPhase.Crouching:
                crouchTimer += dt * accel;
                if (!fireballPlayed && crouchTimer >= 10f)
                {
                    if (!TryPrepareFireballCast(user))
                        return false;
                    castPending = true;
                    return true;
                }

                if (fireballPlayed && crouchTimer >= 20f)
                {
                    EnterStandingFromCrouch(user);
                    return false;
                }

                return false;
        }

        return false;
    }

    void PrepareDebugCast(Chess user)
    {
        var ctx = user.skillController.context;
        ctx.Set(ZombieKingContextKeys.SkillAnimKind, (int)ZombieKingSkillAnimKind.SpawnZombie);
        int row = Mathf.Clamp(RandomRowAnim(user), 1, 5);
        ctx.Set(ZombieKingContextKeys.Row, row);
        ctx.Set(ZombieKingContextKeys.SpawnPoolIndex, 0);
    }

    static float GetSpawnInterval(Chess user) =>
        user.propertyController.GetHpPerCent() > 0.5f ? 4.5f : 3f;

    /// <summary>首次入场站立（不增加 bendCount）。</summary>
    void EnterStandingInitial(Chess user)
    {
        RebuildSummonPool();
        BuildStandQueue(user);
        queueIndex = 0;
        phase = ZombieKingBossPhase.StandingQueue;
        castCooldownTimer = 0f;
        postStandDelayTimer = 0f;
        castPending = false;
        standRiseSkillLockTimer = StandSkillLockAfterRiseFromCrouch;
        ApplyStandEnterPolicies(user, playIdleForStandVisual: false);
    }

    /// <summary>俯身结束回站立：bendCount++、重建池与队列。</summary>
    public void EnterStandingFromCrouch(Chess user)
    {
        bendCount++;
        RebuildSummonPool();
        BuildStandQueue(user);
        queueIndex = 0;
        phase = ZombieKingBossPhase.StandingQueue;
        castCooldownTimer = 0f;
        postStandDelayTimer = 0f;
        fireballPlayed = false;
        crouchTimer = 0f;
        castPending = false;
        standRiseSkillLockTimer = StandSkillLockAfterRiseFromCrouch;
        ApplyStandEnterPolicies(user, playIdleForStandVisual: true);
    }

    /// <param name="playIdleForStandVisual">
    /// false：本场首次进战由 <see cref="IdleState"/> 的 <see cref="AnimatorController_Zombieking.PlayIdle"/> 播 <c>anim_enter</c>；
    /// 若在 <see cref="Chess.WhenChessEnterWar"/> 里此处先 <c>PlayIdle</c>，随后状态机进 Idle 会再播一次 idle，把登场动画盖掉。
    /// true：俯身→站立须立刻播 <c>anim_head_leave</c>。
    /// </param>
    void ApplyStandEnterPolicies(Chess user, bool playIdleForStandVisual)
    {
        user.UnSelectable();
        user.buffController?.ResetList();
        user.skillController.context.Set("stand", true);
        if (playIdleForStandVisual)
            user.animatorController?.PlayIdle();
    }

    void EnterCrouchPhase(Chess user)
    {
        phase = ZombieKingBossPhase.Crouching;
        crouchTimer = 0f;
        fireballPlayed = false;
        castPending = false;
        user.skillController.context.Set("stand", false);
        user.ResumeSelectable();
        user.animatorController?.PlayIdle();
    }

    void RebuildSummonPool()
    {
        zombieCanSummons.Clear();
        if (zombieTypes == null) return;
        foreach (var z in zombieTypes)
        {
            if (z == null || z.baseProperty == null) continue;
            if (bendCount * 2 > z.baseProperty.waveLimit)
                zombieCanSummons.Add(z);
        }
    }

    void BuildStandQueue(Chess user)
    {
        standQueue.Clear();
        float hp = user.propertyController.GetHpPerCent();

        if (hp > 0.5f)
        {
            for (int i = 0; i < 7; i++)
                standQueue.Add(new QueuedStandAction { Kind = StandActionKind.Spawn });
            if (hp < BungeeHpGate)
            {
                standQueue.Add(new QueuedStandAction { Kind = StandActionKind.BungeeEnter });
                standQueue.Add(new QueuedStandAction { Kind = StandActionKind.BungeeLeave });
            }
        }
        else
        {
            for (int i = 0; i < 14; i++)
                standQueue.Add(new QueuedStandAction { Kind = StandActionKind.Spawn });
            if (UnityEngine.Random.value < 0.5f && hp < BungeeHpGate && bungeeZombie != null)
            {
                standQueue.Add(new QueuedStandAction { Kind = StandActionKind.BungeeEnter });
                standQueue.Add(new QueuedStandAction { Kind = StandActionKind.BungeeLeave });
            }
            else
                standQueue.Add(new QueuedStandAction { Kind = StandActionKind.ThrowCar });

            if (HasAnyPlantInStompFootprint(user))
                standQueue.Insert(UnityEngine.Random.Range(0, standQueue.Count + 1), new QueuedStandAction { Kind = StandActionKind.Stomp });
        }

        ShuffleStandQueue(user);
    }

    void ShuffleStandQueue(Chess user)
    {
        float hp = user.propertyController.GetHpPerCent();
        if (hp > 0.5f)
        {
            for (int i = standQueue.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (standQueue[i], standQueue[j]) = (standQueue[j], standQueue[i]);
            }
        }
    }

    bool TryPrepareStandCast(Chess user)
    {
        if (queueIndex >= standQueue.Count)
            return false;

        var act = standQueue[queueIndex];
        var ctx = user.skillController.context;

        switch (act.Kind)
        {
            case StandActionKind.Spawn:
            {
                int rowAnim = RandomRowAnim(user);
                ctx.Set(ZombieKingContextKeys.SkillAnimKind, (int)ZombieKingSkillAnimKind.SpawnZombie);
                ctx.Set(ZombieKingContextKeys.Row, rowAnim);
                int poolIdx = PickSummonPoolIndex();
                ctx.Set(ZombieKingContextKeys.SpawnPoolIndex, poolIdx);
                break;
            }
            case StandActionKind.BungeeEnter:
                ctx.Set(ZombieKingContextKeys.SkillAnimKind, (int)ZombieKingSkillAnimKind.BungeeEnter);
                break;
            case StandActionKind.BungeeLeave:
                ctx.Set(ZombieKingContextKeys.SkillAnimKind, (int)ZombieKingSkillAnimKind.BungeeLeave);
                break;
            case StandActionKind.Stomp:
            {
                var map = MapManage.instance;
                int gameBand = FindStompBandWithPlants(user);
                int animBand = map != null
                    ? ZombieKingMapAnim.GameStompBandToAnimStompBand(gameBand, map.mapSize.y)
                    : 1;
                ctx.Set(ZombieKingContextKeys.SkillAnimKind, (int)ZombieKingSkillAnimKind.Stomp);
                ctx.Set(ZombieKingContextKeys.StompBand, animBand);
                break;
            }
            case StandActionKind.ThrowCar:
                ctx.Set(ZombieKingContextKeys.SkillAnimKind, (int)ZombieKingSkillAnimKind.ThrowCar);
                break;
        }

        lastCastWasFireball = false;
        return true;
    }

    bool TryPrepareFireballCast(Chess user)
    {
        var ctx = user.skillController.context;
        int rowAnim = RandomRowAnim(user);
        int ball = UnityEngine.Random.Range(0, 2);
        ctx.Set(ZombieKingContextKeys.SkillAnimKind, (int)ZombieKingSkillAnimKind.FireIceBall);
        ctx.Set(ZombieKingContextKeys.Row, rowAnim);
        ctx.Set(ZombieKingContextKeys.BallVisual, ball);
        lastCastWasFireball = true;
        return true;
    }

    int PickSummonPoolIndex()
    {
        if (zombieCanSummons.Count > 0)
            return UnityEngine.Random.Range(0, zombieCanSummons.Count);
        if (zombieTypes != null && zombieTypes.Count > 0)
            return 0;
        return 0;
    }

    PropertyCreator ResolveSummonCreator(int poolIdx)
    {
        if (zombieCanSummons.Count > 0)
            return zombieCanSummons[Mathf.Clamp(poolIdx, 0, zombieCanSummons.Count - 1)];
        if (zombieTypes != null && zombieTypes.Count > 0)
            return zombieTypes[0];
        return null;
    }

    /// <summary>随机得到 <b>动画行</b> 1~5（与 anim_spawn_/head_attack_ 一致；地图 tileY 从下往上，见 <see cref="ZombieKingMapAnim"/>）。</summary>
    public static int RandomRowAnim(Chess user)
    {
        var map = MapManage.instance;
        if (map == null) return 1;
        int maxExclusive = Mathf.Min(5, map.mapSize.y);
        if (maxExclusive <= 0) return 1;
        int tileY = UnityEngine.Random.Range(0, maxExclusive);
        return ZombieKingMapAnim.TileYToAnimRow(tileY, map.mapSize.y);
    }

    /// <summary>返回<b>游戏</b>踩踏带 1..min(4,H-1)，覆盖 tile 行 y0 与 y0+1（从下数）。</summary>
    static int FindStompBandWithPlants(Chess user)
    {
        var map = MapManage.instance;
        if (map == null || user.moveController?.standTile == null) return 1;
        for (int t = 0; t < 12; t++)
        {
            int y0 = UnityEngine.Random.Range(0, Mathf.Min(4, Mathf.Max(1, map.mapSize.y - 1)));
            int x = user.moveController.standTile.mapPos.x + (int)user.transform.right.x;
            if (TryStompHasPlant(user, map, x, y0))
                return Mathf.Clamp(y0 + 1, 1, 4);
        }
        return 1;
    }

    static bool HasAnyPlantInStompFootprint(Chess user)
    {
        var map = MapManage.instance;
        if (map == null || user.moveController?.standTile == null) return false;
        for (int attempt = 0; attempt < 8; attempt++)
        {
            int x = user.moveController.standTile.mapPos.x + (int)user.transform.right.x;
            int y0 = UnityEngine.Random.Range(0, Mathf.Min(4, map.mapSize.y));
            if (TryStompHasPlant(user, map, x, y0))
                return true;
        }
        return false;
    }

    static bool TryStompHasPlant(Chess user, MapManage map, int x, int y0)
    {
        string plantTag = user.CompareTag("Enemy") ? "Player" : "Enemy";
        int[] dx = { 0, 1, 2, 0, 1, 2 };
        int[] dy = { 0, 0, 0, 1, 1, 1 };
        for (int i = 0; i < 6; i++)
        {
            int tx = x + dx[i];
            int ty = y0 + dy[i];
            if (!map.IfInMapRange(tx, ty)) continue;
            var t = map.tiles[tx, ty];
            if (t?.stander != null && t.stander.CompareTag(plantTag))
                return true;
        }
        return false;
    }

    void RefreshPresentation(Chess user)
    {
        float hp = user.propertyController.GetHpPerCent();
        int tier = hp > 0.8f ? 0 : hp > 0.5f ? 1 : hp > 0.1f ? 2 : 3;
        if (user.animatorController is AnimatorController_Zombieking zkVisual)
            zkVisual.SyncPartSpritesFromContext(hp);

        if (tier != lastVisualTier)
        {
            lastVisualTier = tier;
            user.animatorController?.SetVisualTierPublic(tier);
        }

        if (hp <= 0.5f && smokeVfxRoot != null && !smokeActivated)
        {
            smokeActivated = true;
            smokeVfxRoot.SetActive(true);
        }

        if (hp <= 0.1f && flashCo == null)
            flashCo = user.StartCoroutine(LowHpFlashRoutine(user));
        else if (hp > 0.1f && flashCo != null)
        {
            user.StopCoroutine(flashCo);
            flashCo = null;
        }
    }

    IEnumerator LowHpFlashRoutine(Chess user)
    {
        var sr = user.animatorController != null ? user.animatorController.sprite : null;
        if (sr == null) yield break;
        var baseCol = sr.color;
        while (user != null && user.propertyController.GetHpPerCent() <= 0.1f && !user.IfDeath)
        {
            sr.color = Color.Lerp(baseCol, Color.white, 0.35f);
            yield return new WaitForSeconds(0.12f);
            sr.color = baseCol;
            yield return new WaitForSeconds(0.12f);
        }
        if (sr != null) sr.color = baseCol;
        flashCo = null;
    }

    public override void UseSkill(Chess user)
    {
        var ctx = user.skillController.context;
        if (!ctx.TryGet<int>(ZombieKingContextKeys.SkillAnimKind, out int kindInt))
        {
            if (effect != null)
                base.UseSkill(user);
            return;
        }

        var kind = (ZombieKingSkillAnimKind)kindInt;
        switch (kind)
        {
            case ZombieKingSkillAnimKind.SpawnZombie:
                ExecSpawn(user, ctx);
                break;
            case ZombieKingSkillAnimKind.BungeeEnter:
                ExecBungee(user);
                break;
            case ZombieKingSkillAnimKind.BungeeLeave:
                break;
            case ZombieKingSkillAnimKind.Stomp:
                ExecStomp(user, ctx);
                break;
            case ZombieKingSkillAnimKind.ThrowCar:
                ExecRv(user);
                break;
            case ZombieKingSkillAnimKind.FireIceBall:
                ExecFireball(user, ctx);
                break;
        }
    }

    void ExecSpawn(Chess user, SkillContext ctx)
    {
        ctx.TryGet<int>(ZombieKingContextKeys.Row, out int rowAnim);
        ctx.TryGet<int>(ZombieKingContextKeys.SpawnPoolIndex, out int poolIdx);
        var creator = ResolveSummonCreator(poolIdx);
        if (creator == null) return;

        var map = MapManage.instance;
        if (map == null || user.moveController?.standTile == null) return;
        int tileY = ZombieKingMapAnim.AnimRowToTileY(rowAnim, map.mapSize.y);
        int x = user.moveController.standTile.mapPos.x + (int)user.transform.right.x * 2;
        if (!map.IfInMapRange(x, tileY)) return;
        var tile = map.tiles[x, tileY];
        GameManage.instance.chessTeamManage.CreateChess(creator, tile, user.tag);
    }

    void ExecBungee(Chess user)
    {
        if (bungeeZombie == null) return;
        var tiles = CollectRandomPlantTiles(user, 3);
        foreach (var t in tiles)
            GameManage.instance.chessTeamManage.CreateChess(bungeeZombie, t, user.tag);
    }

    static List<Tile> CollectRandomPlantTiles(Chess user, int count)
    {
        var ans = new List<Tile>();
        var map = MapManage.instance;
        if (map == null) return ans;
        string plantTag = user.CompareTag("Enemy") ? "Player" : "Enemy";
        var pool = new List<Tile>();
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                var t = map.tiles[x, y];
                if (t?.stander != null && t.stander.CompareTag(plantTag))
                    pool.Add(t);
            }
        }
        while (ans.Count < count && pool.Count > 0)
        {
            int i = UnityEngine.Random.Range(0, pool.Count);
            ans.Add(pool[i]);
            pool.RemoveAt(i);
        }
        return ans;
    }

    void ExecStomp(Chess user, SkillContext ctx)
    {
        ctx.TryGet<int>(ZombieKingContextKeys.StompBand, out int animBand);
        animBand = Mathf.Clamp(animBand, 1, 4);
        var map = MapManage.instance;
        if (map == null || user.moveController?.standTile == null) return;
        int gameBand = ZombieKingMapAnim.AnimStompBandToGameStompBand(animBand, map.mapSize.y);
        int y0 = gameBand - 1;
        int x = user.moveController.standTile.mapPos.x + (int)user.transform.right.x;
        float dmg = GetCrushDamage(user);
        int[] dx = { 0, 1, 2, 0, 1, 2 };
        int[] dy = { 0, 0, 0, 1, 1, 1 };
        for (int i = 0; i < 6; i++)
        {
            int tx = x + dx[i];
            int ty = y0 + dy[i];
            ApplyTileCrushDamage(user, map, tx, ty, dmg);
        }
    }

    void ExecRv(Chess user)
    {
        var map = MapManage.instance;
        if (map == null || user.moveController?.standTile == null) return;
        int x = user.moveController.standTile.mapPos.x + (int)user.transform.right.x * 6;
        const int y0 = 1;
        float dmg = GetCrushDamage(user);
        int[] dx = { 0, 1, 2, 0, 1, 2 };
        int[] dy = { 0, 0, 0, 1, 1, 1 };
        for (int i = 0; i < 6; i++)
        {
            int tx = x + dx[i];
            int ty = y0 + dy[i];
            ApplyTileCrushDamage(user, map, tx, ty, dmg);
        }
    }

    float GetCrushDamage(Chess user)
    {
        if (config != null && config.baseDamage != null && config.baseDamage.Count > 0)
            return user.propertyController.GetAttack() * config.baseDamage[0];
        return user.propertyController.GetAttack() * 1000f;
    }

    static void ApplyTileCrushDamage(Chess user, MapManage map, int tx, int ty, float damage)
    {
        if (!map.IfInMapRange(tx, ty)) return;
        var t = map.tiles[tx, ty];
        var target = t?.stander;
        if (target == null) return;
        string plantTag = user.CompareTag("Enemy") ? "Player" : "Enemy";
        if (!target.CompareTag(plantTag)) return;

        var dm = user.skillController.DM;
        dm.damageFrom = user;
        dm.damageTo = target;
        dm.damage = damage;
        dm.damageType = DamageType.Real;
        dm.damageElementType = ElementType.Grind;
        target.propertyController.GetDamage(dm);
    }

    void ExecFireball(Chess user, SkillContext ctx)
    {
        ctx.TryGet<int>(ZombieKingContextKeys.Row, out int rowAnim);
        ctx.TryGet<int>(ZombieKingContextKeys.BallVisual, out int ballVisual);
        var map = MapManage.instance;
        if (map == null || user.moveController?.standTile == null) return;
        int tileY = ZombieKingMapAnim.AnimRowToTileY(rowAnim, map.mapSize.y);
        int x = user.moveController.standTile.mapPos.x + (int)user.transform.right.x * 3;
        if (!map.IfInMapRange(x, tileY)) return;
        var tile = map.tiles[x, tileY];
        GameObject prefab = ballVisual == 1 && iceBallPrefab != null ? iceBallPrefab : fireBallPrefab;
        if (prefab == null) return;
        GameObject b = UnityEngine.Object.Instantiate(prefab);
        b.tag = user.tag;
        var armor = b.GetComponent<CarArmor>();
        if (armor != null) armor.user = user;
        b.transform.position = tile.transform.position;
    }

    public override bool IsSkillFinished(Chess user) => base.IsSkillFinished(user);

    public override void SkillOver(Chess user)
    {
        if (returnCd)
        {
            returnCd = false;
            castPending = false;
            return;
        }

        castPending = false;
        castCooldownTimer = 0f;
        if (lastCastWasFireball)
        {
            lastCastWasFireball = false;
            fireballPlayed = true;
        }
        else if (phase == ZombieKingBossPhase.StandingQueue)
            queueIndex++;
    }

    public override void ReturnCD()
    {
        returnCd = true;
        castCooldownTimer = config != null ? config.baseCd : 0f;
    }

    public override void WriteToSaveData(SkillStateSaveData data)
    {
        if (data == null) return;
        data.skillType = nameof(Skill_ZombieKingBoss);
        data.Set("bendCount", bendCount);
        data.Set("zkPhase", (int)phase);
        data.Set("zkQIdx", queueIndex);
        data.Set("zkCrouchT", Mathf.RoundToInt(crouchTimer * 1000f));
        data.Set("zkFire", fireballPlayed ? 1 : 0);
        data.Set("zkPost", Mathf.RoundToInt(postStandDelayTimer * 1000f));
        data.Set("zkCast", Mathf.RoundToInt(castCooldownTimer * 1000f));
        data.Set("zkVis", lastVisualTier);
        data.Set("zkSmoke", smokeActivated ? 1 : 0);
        data.Set("zkRiseLock", Mathf.RoundToInt(standRiseSkillLockTimer * 1000f));
    }

    public override void RestoreFromSaveData(SkillStateSaveData data, Chess user)
    {
        if (data == null) return;
        bendCount = data.GetInt("bendCount", 0);
        phase = (ZombieKingBossPhase)data.GetInt("zkPhase", 0);
        queueIndex = data.GetInt("zkQIdx", 0);
        crouchTimer = data.GetInt("zkCrouchT", 0) / 1000f;
        fireballPlayed = data.GetInt("zkFire", 0) != 0;
        postStandDelayTimer = data.GetInt("zkPost", 0) / 1000f;
        castCooldownTimer = data.GetInt("zkCast", 0) / 1000f;
        lastVisualTier = data.GetInt("zkVis", -1);
        smokeActivated = data.GetInt("zkSmoke", 0) != 0;
        standRiseSkillLockTimer = data.GetInt("zkRiseLock", 0) / 1000f;
        castPending = false;
        RebuildSummonPool();
        if (standQueue.Count == 0 && phase == ZombieKingBossPhase.StandingQueue)
            BuildStandQueue(user);
    }
}
