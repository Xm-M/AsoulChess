using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多首主动技：清压、灭头、终局锁、按头数召唤轻量幽灵、每秒加压直至死亡。
/// </summary>
[Serializable]
public class SkillEffect_MultiHeadGhostBurst : ISkillEffect
{
    [Tooltip("幽灵 PropertyCreator（须绑棋子 Prefab）")]
    public PropertyCreator ghostCreator;

    [Min(0.1f)]
    [Tooltip("加压间隔（秒）")]
    public float stressTickInterval = 1f;

    [Min(1)]
    [Tooltip("每次加压增量")]
    public int stressPerTick = 1;

    [SerializeReference]
    [Tooltip("若尚无压力 Buff 时的模板（一般多首已挂）")]
    public Buff_StressBuff_Death guestStressBuff;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || ghostCreator == null || ghostCreator.GetPre() == null)
            return;
        if (MultiHeadMutsumiKeys.IsFinaleLocked(user))
            return;

        int n = MultiHeadMutsumiKeys.GetCloneCount(user);
        if (n <= 0)
            return;

        // 灭头前先记下每个分身世界坐标，幽灵从该处「现身」
        List<Vector3> spawnPositions = SnapshotHeadWorldPositions(user, n);

        // 1) 清压
        if (user.skillController?.context != null)
            user.skillController.context.Set(MultiHeadMutsumiKeys.StressThreshold, 0);

        // 2) 灭头 + 终局锁（保持当前 stressLimit）+ Blend=3 终局形态
        MultiHeadMutsumiKeys.ClearAllHeadVisuals(user);
        MultiHeadMutsumiKeys.SetFinaleLocked(user, true);
        user.animatorController?.ChangeFloat(3f);

        // 终局：禁止后续普攻与主动技（主动技另由 SkillReady + FinaleLocked 拦截）
        ApplyFinaleCombatLock(user);

        // 3) 按分身位置召唤 N 幽灵
        List<Chess> ghosts = MultiHeadMutsumiKeys.GetOrCreateGhostList(user);
        ghosts?.Clear();

        for (int i = 0; i < n; i++)
        {
            Vector3 pos = i < spawnPositions.Count
                ? spawnPositions[i]
                : user.transform.position;
            Chess ghost = SpawnGhostOffTile(user, ghostCreator, pos);
            if (ghost == null)
                continue;
            ghosts?.Add(ghost);
        }

        // 4) 每秒加压
        StartStressTick(user);
    }

    /// <summary>终局后停普攻、清冷却图标；主动技就绪已由 FinaleLocked 禁止。</summary>
    static void ApplyFinaleCombatLock(Chess user)
    {
        if (user?.equipWeapon == null)
            return;
        user.equipWeapon.AttackAble = false;
        user.equipWeapon.StopAttack();

        if (user.skillController?.skillColdFx != null)
        {
            UnityEngine.Object.Destroy(user.skillController.skillColdFx.gameObject);
            user.skillController.skillColdFx = null;
        }
    }

    /// <summary>灭头前快照分身世界坐标；缺视觉时用本体位置补足到 count。</summary>
    static List<Vector3> SnapshotHeadWorldPositions(Chess user, int count)
    {
        var positions = new List<Vector3>(count);
        List<Transform> heads = null;
        if (user?.skillController?.context != null)
            user.skillController.context.TryGet(MultiHeadMutsumiKeys.CloneHeads, out heads);

        Vector3 fallback = user != null ? user.transform.position : Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            if (heads != null && i < heads.Count && heads[i] != null)
                positions.Add(heads[i].position);
            else
                positions.Add(fallback);
        }
        return positions;
    }

    static Chess SpawnGhostOffTile(Chess master, PropertyCreator creator, Vector3 worldPos)
    {
        if (GameManage.instance?.chessFactory == null || ChessTeamManage.Instance == null)
            return null;

        Chess pre = creator.GetPre();
        if (pre == null)
            return null;

        Chess ghost = GameManage.instance.chessFactory.ChessCreate(pre, creator.chessName);
        if (ghost == null)
            return null;

        // 确保 creator 指向幽灵数据（池化实例可能仍挂旧引用）
        if (ghost.propertyController != null)
            ghost.propertyController.creator = creator;

        ChessTeamManage.Instance.player.AddChess(ghost);
        ghost.tag = master.tag;
        ghost.gameObject.layer = LayerMask.NameToLayer(master.tag);

        ghost.transform.position = worldPos;
        ghost.gameObject.SetActive(true);

        // 须在 WhenChessEnterWar（被动 SkillEffect）之前写入，幽灵被动才能缓存主人
        ghost.skillController?.context?.Set(MultiHeadMutsumiKeys.GhostMaster, master);
        ghost.WhenChessEnterWar(true);

        // 进场后再锁不可选 / 关碰撞（WhenChessEnterWar 可能重置 layer）
        ghost.UnSelectable();
        ghost.SetCol(false);
        if (ghost.moveController != null)
        {
            ghost.moveController.standTile = null;
            ghost.moveController.tileMethod = null;
        }

        return ghost;
    }

    void StartStressTick(Chess user)
    {
        MultiHeadMutsumiKeys.StopStressTickTimer(user);
        if (GameManage.instance?.timerManage == null)
            return;

        float interval = Mathf.Max(0.1f, stressTickInterval);
        int delta = Mathf.Max(1, stressPerTick);
        Timer timer = GameManage.instance.timerManage.AddTimer(
            () => ApplyStressTick(user, delta),
            interval,
            true);
        MultiHeadMutsumiKeys.RegisterStressTickTimer(user, timer);
    }

    void ApplyStressTick(Chess user, int delta)
    {
        if (user == null || user.IfDeath || !LevelManage.instance.IfGameStart)
        {
            MultiHeadMutsumiKeys.StopStressTickTimer(user);
            return;
        }

        EnsureStressBuff(user);
        if (user.buffController?.buffDic == null
            || !user.buffController.buffDic.TryGetValue("压力", out Buff buff)
            || buff is not Buff_StressBuff_Death stressBuff)
            return;

        stressBuff.BuffReset(new Buff_StressBuff_Death { extraStress = delta });
    }

    void EnsureStressBuff(Chess user)
    {
        if (user?.buffController == null || user.buffController.buffDic.ContainsKey("压力"))
            return;
        Buff_StressBuff_Death template = guestStressBuff != null
            ? (Buff_StressBuff_Death)guestStressBuff.Clone()
            : new Buff_StressBuff_Death();
        user.buffController.AddBuff(template);
    }
}
