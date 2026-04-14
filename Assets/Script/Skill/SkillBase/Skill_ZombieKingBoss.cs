using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 僵王主动：站立阶段技能队列 / 俯身计时 / 召唤池与 <see cref="ZombieKingContextKeys"/> 写入。
/// 需求见 <c>docs/requirements/僵王Boss行为.md</c>；本文件为可编译骨架，业务分支标 TODO。
/// </summary>
public class Skill_ZombieKingBoss : SkillBase<SkillConfig_Cold>
{
    const float BungeeHpGate = 0.8f;

    [Title("配置")]
    [Tooltip("全种类僵尸配置；解锁条件 bendCount*2 > baseProperty.waveLimit")]
    public List<PropertyCreator> zombieTypes = new List<PropertyCreator>();

    [SerializeField, Tooltip("调试：为 true 时每帧若 readyChecker 通过则强制可放技能（仅开发用）")]
    bool debugForceReady;

    [Title("运行时（只读）")]
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    int bendCount;

    /// <summary>本局已解锁进召唤池的 PropertyCreator（由 bendCount 与 waveLimit 推导，TODO 在适当时机重建）。</summary>
    readonly List<PropertyCreator> zombieCanSummons = new List<PropertyCreator>();

    public override void InitSkill(Chess user)
    {
        base.InitSkill(user);
        bendCount = 0;
        zombieCanSummons.Clear();
    }

    public override void WhenEnter(Chess user)
    {
        base.WhenEnter(user);
        // TODO: 读档恢复后若需重置俯身计时，在此处理
    }

    public override bool IfSkillReady(Chess user)
    {
        if (config == null || user?.propertyController == null || user.skillController?.context == null)
            return false;

        if (debugForceReady)
        {
            targets ??= new List<Chess>();
            targets.Clear();
            if (readyChecker != null && !readyChecker.IfSkillReady(user, config, targets))
                return false;
            TryPrepareNextCast(user);
            return true;
        }

        // TODO: 按宏观阶段（站立序列 / 站立结束等 2s / 俯身 10s 吐球 / 俯身 20s 回站立）推进内部计时器
        // TODO: 仅在「下一条指令应释放」且 CD（如 4.5s / 3s 召唤间隔）满足时调用 TryPrepareNextCast(user) 并 return true
        return false;
    }

    /// <summary>
    /// 在 return true 前调用：写入 <see cref="ZombieKingContextKeys"/> 与 <c>stand</c>，供同一帧稍后的 <see cref="SkillState"/> → <see cref="AnimatorController_Zombieking.PlaySkill"/> 使用。
    /// </summary>
    void TryPrepareNextCast(Chess user)
    {
        var ctx = user.skillController.context;
        // TODO: 根据当前队列项设置 ZombieKingSkillAnimKind、Row、StompBand、BallVisual
        // 示例（占位）：
        // ctx.Set(ZombieKingContextKeys.SkillAnimKind, (int)ZombieKingSkillAnimKind.SpawnZombie);
        // ctx.Set(ZombieKingContextKeys.Row, ComputeSpawnAnimRow(user));
        float hp = user.propertyController.GetHpPerCent();
        bool allowBungee = hp < BungeeHpGate;
        _ = allowBungee; // 避免未使用告警，实现蹦极队列时使用
    }

    /// <summary>随机召唤/吐球行：0..Min(5, mapSize.y)-1 或 1..5 与 anim 对齐，按需求文档最终统一（TODO）。</summary>
    public static int RandomRowYForSpawn(Chess user)
    {
        var map = MapManage.instance;
        if (map == null) return 0;
        int maxY = Mathf.Min(5, map.mapSize.y);
        if (maxY <= 0) return 0;
        return Random.Range(0, maxY);
    }

    /// <summary>从俯身回到站立时调用：增加俯身次数并重建可召唤池（TODO 由状态切换钩子调用）。</summary>
    public void OnReturnedToStanding(Chess user)
    {
        bendCount++;
        RebuildSummonPool();
        // TODO: user.UnSelectable(); user.buffController.ResetList();
        user.skillController.context.Set("stand", true);
    }

    /// <summary>进入俯身时调用（TODO 由状态切换调用）。</summary>
    public void OnEnterCrouch(Chess user)
    {
        user.skillController.context.Set("stand", false);
        // TODO: user.ResumeSelectable();
    }

    void RebuildSummonPool()
    {
        zombieCanSummons.Clear();
        if (zombieTypes == null) return;
        for (int i = 0; i < zombieTypes.Count; i++)
        {
            var z = zombieTypes[i];
            if (z == null || z.baseProperty == null) continue;
            if (bendCount * 2 > z.baseProperty.waveLimit)
                zombieCanSummons.Add(z);
        }
    }

    public override void UseSkill(Chess user)
    {
        if (effect != null)
            base.UseSkill(user);
        // TODO: 或在此根据 context 分发到多个 effect，避免单一 SerializeReference 塞满逻辑
    }

    public override bool IsSkillFinished(Chess user) => base.IsSkillFinished(user);

    public override void SkillOver(Chess user)
    {
        // TODO: 与 ColdSkill 类似处理 returnCD、重置内部 CD 计时
    }

    public override void ReturnCD()
    {
        // TODO
    }

    public override void WriteToSaveData(SkillStateSaveData data)
    {
        if (data == null) return;
        data.skillType = nameof(Skill_ZombieKingBoss);
        data.Set("bendCount", bendCount);
        // TODO: 站立队列索引、俯身累计时间等
    }

    public override void RestoreFromSaveData(SkillStateSaveData data, Chess user)
    {
        if (data == null) return;
        bendCount = data.GetInt("bendCount", 0);
        RebuildSummonPool();
    }
}
