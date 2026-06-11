using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽整局流程入口：开 Run、进 Act、选路、解析战斗 <see cref="LevelData"/>。
/// Run 地图进度由 <see cref="RoguelikeRunSaveSystem"/> 持久化；Meta 写入 <see cref="PlayerSaveData"/>。
/// </summary>
public static class RoguelikeRunService
{
    public static RunMapConfig ActiveRunConfig { get; private set; }
    public static RoguelikeRunState State { get; private set; }

    /// <summary>当前待进入或正在进行的地图节点（战斗用）。</summary>
    public static int PendingNodeId { get; private set; } = -1;

    public static event Action<RoguelikeRunState> OnRunStarted;
    public static event Action<RoguelikeRunState> OnActMapGenerated;
    public static event Action<RoguelikeMapNode> OnNodeEntered;
    public static event Action<RoguelikeMapNode, bool> OnNodeResolved;
    public static event Action OnActCompleted;
    public static event Action OnRunCompleted;

    public static bool HasActiveRun => State != null && State.runActive;

    public static bool HasContinuableRunSave => RoguelikeRunSaveSystem.HasActiveRunSave();

    /// <summary>本 Run 获得新植物（奖励/商店等），写入 State 并 SaveRun。</summary>
    public static bool AddOwnedPlant(string creatorChessName) =>
        RoguelikeRunPlantPool.AddPlant(creatorChessName);

    public static void StartNewRun(RunMapConfig config, int? seed = null)
    {
        StartNewRun(config, null, seed);
    }

    public static void StartNewRun(RunMapConfig config, BandMes band, int? seed = null)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        if (!config.Validate(out var err))
            throw new InvalidOperationException(err);

        RoguelikeRunSaveSystem.DeleteActiveSave();

        ActiveRunConfig = config;
        State = new RoguelikeRunState
        {
            runSeed = seed ?? Environment.TickCount,
            currentActIndex = 0,
            runActive = true,
        };

        if (band != null)
        {
            State.selectedBandId = band.bandId;
            State.selectedBandName = band.bandName;
            RoguelikeRunPlantPool.InitializeFromBand(band);
        }
        else
        {
            RoguelikeRunPlantPool.InitializeForNewRun(config);
        }

        GenerateActMap(0);
        var start = State.currentMap.GetStartNode();
        if (start == null)
            throw new InvalidOperationException("生成的地图没有起点");

        State.currentNodeId = start.id;
        State.MarkVisited(start.id);
        PendingNodeId = -1;
        SaveRun();
        OnRunStarted?.Invoke(State);
    }

    /// <summary>从 active Run 存档继续；config 用于解析 RunMapConfig（需与存档名一致或放在 Resources）。</summary>
    public static bool TryContinueRun(RunMapConfig config)
    {
        if (!RoguelikeRunSaveSystem.TryLoad(out var save) || save.state == null)
            return false;

        var resolved = RoguelikeRunSaveSystem.ResolveRunConfig(save.runConfigName, config);
        if (resolved == null)
        {
            Debug.LogWarning($"[Roguelike] 无法解析 RunMapConfig: {save.runConfigName}");
            return false;
        }

        if (!resolved.Validate(out var err))
        {
            Debug.LogWarning($"[Roguelike] RunMapConfig 无效: {err}");
            return false;
        }

        ActiveRunConfig = resolved;
        State = save.state;
        State.runActive = true;
        State.currentMap?.RebuildIndex();
        PendingNodeId = save.pendingNodeId;

        if (State.ownedPlantCreatorIds == null || State.ownedPlantCreatorIds.Count == 0)
            RoguelikeRunPlantPool.InitializeForNewRun(resolved);

        SaveRun();
        OnRunStarted?.Invoke(State);
        OnActMapGenerated?.Invoke(State);
        return true;
    }

    public static void SaveRun()
    {
        if (!HasActiveRun || ActiveRunConfig == null || State == null)
            return;

        var save = new RoguelikeRunSaveData
        {
            runConfigName = ActiveRunConfig.name,
            pendingNodeId = PendingNodeId,
            state = CloneState(State),
        };
        RoguelikeRunSaveSystem.Save(save);
        RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: false, actClearedThisSession: false);
    }

    static RoguelikeRunState CloneState(RoguelikeRunState src)
    {
        var dst = new RoguelikeRunState
        {
            runSeed = src.runSeed,
            currentActIndex = src.currentActIndex,
            currentNodeId = src.currentNodeId,
            runActive = src.runActive,
            currentMap = new GeneratedRoguelikeMap
            {
                actId = src.currentMap.actId,
                seed = src.currentMap.seed,
                nodes = new List<RoguelikeMapNode>(),
            },
            visitedNodeIds = new List<int>(src.visitedNodeIds),
            clearedNodeIds = new List<int>(src.clearedNodeIds),
            ownedPlantCreatorIds = src.ownedPlantCreatorIds != null
                ? new List<string>(src.ownedPlantCreatorIds)
                : new List<string>(),
            selectedBandId = src.selectedBandId,
            selectedBandName = src.selectedBandName,
        };

        if (src.currentMap?.nodes != null)
        {
            for (int i = 0; i < src.currentMap.nodes.Count; i++)
            {
                var n = src.currentMap.nodes[i];
                dst.currentMap.nodes.Add(new RoguelikeMapNode
                {
                    id = n.id,
                    layer = n.layer,
                    slot = n.slot,
                    roomType = n.roomType,
                    nextNodeIds = new List<int>(n.nextNodeIds),
                    prevNodeIds = new List<int>(n.prevNodeIds),
                });
            }
        }

        dst.currentMap.RebuildIndex();
        return dst;
    }

    public static void GenerateActMap(int actIndex)
    {
        var act = ActiveRunConfig.GetAct(actIndex);
        if (act == null)
            throw new InvalidOperationException($"Act {actIndex} 未配置");

        int actSeed = State.runSeed + actIndex * 10007 + act.actId * 97;
        State.currentMap = MapGenerator.Generate(act, actSeed);
        State.currentActIndex = actIndex;
        State.currentMap.RebuildIndex();
        OnActMapGenerated?.Invoke(State);
    }

    public static bool TrySelectNextNode(int nodeId)
    {
        if (!HasActiveRun || !State.CanSelectNode(nodeId))
            return false;

        var node = State.currentMap.GetNode(nodeId);
        if (node == null) return false;

        State.currentNodeId = nodeId;
        State.MarkVisited(nodeId);
        PendingNodeId = nodeId;
        OnNodeEntered?.Invoke(node);

        switch (node.roomType)
        {
            case MapRoomType.Start:
                State.MarkCleared(nodeId);
                PendingNodeId = -1;
                OnNodeResolved?.Invoke(node, true);
                SaveRun();
                return true;
            case MapRoomType.Normal:
            case MapRoomType.Elite:
            case MapRoomType.Boss:
            case MapRoomType.Rest:
            case MapRoomType.Shop:
            case MapRoomType.Event:
                return true;
            default:
                return false;
        }
    }

    public static LevelData ResolveLevelForPendingCombat()
    {
        if (!HasActiveRun || PendingNodeId < 0)
            return null;

        var node = State.currentMap.GetNode(PendingNodeId);
        if (node == null) return null;

        var act = ActiveRunConfig.GetAct(State.currentActIndex);
        if (act == null) return null;

        if (node.roomType == MapRoomType.Boss && act.bossLevel != null)
            return act.bossLevel;

        if (node.roomType == MapRoomType.Elite && act.eliteLevelPool != null && act.eliteLevelPool.Count > 0)
        {
            return RoguelikeLevelPicker.PickFromPool(
                act.eliteLevelPool, node, State.currentMap, act, State.runSeed, act.roguelikeEliteDifficultyBonus,
                RoguelikeLevelKind.Elite);
        }

        if (node.roomType == MapRoomType.Event && act.eventLevelPool != null && act.eventLevelPool.Count > 0)
        {
            return RoguelikeLevelPicker.PickFromPool(
                act.eventLevelPool, node, State.currentMap, act, State.runSeed, 0f, RoguelikeLevelKind.Event);
        }

        if (act.normalLevelPool != null && act.normalLevelPool.Count > 0)
        {
            return RoguelikeLevelPicker.PickFromPool(
                act.normalLevelPool, node, State.currentMap, act, State.runSeed, 0f, RoguelikeLevelKind.Normal);
        }

        if (act.eliteLevelPool != null && act.eliteLevelPool.Count > 0)
        {
            return RoguelikeLevelPicker.PickFromPool(
                act.eliteLevelPool, node, State.currentMap, act, State.runSeed, act.roguelikeEliteDifficultyBonus,
                RoguelikeLevelKind.Elite);
        }

        return act.bossLevel;
    }

    public static void ReturnToMapUI()
    {
        if (State == null) return;
        if (HasActiveRun)
            SaveRun();

        var sm = GameManage.instance != null ? GameManage.instance.sceneManage : null;
        if (sm == null)
        {
            UIManage.GetView<RoguelikeMapPanel>()?.ShowAndRefresh();
            return;
        }

        sm.LoadScene("开始",
            () => UIManage.GetView<RoguelikeMapPanel>()?.ShowAndRefresh(),
            () =>
            {
                if (LevelManage.instance != null)
                    LevelManage.instance.LeaveState();
            });
    }

    public static void EnterCombatNode()
    {
        var level = ResolveLevelForPendingCombat();
        if (level == null)
        {
            Debug.LogWarning("[Roguelike] 无法解析战斗关卡，请检查 ActMapConfig 关卡池与 bossLevel");
            return;
        }
        SaveRun();
        RoguelikeRunPlantPool.ApplyRunPlantsToGame();
        LevelManage.instance.ChangeLevel(level);
    }

    public static void OnCombatFinished(bool win)
    {
        if (PendingNodeId < 0 || State == null)
            return;

        var node = State.currentMap.GetNode(PendingNodeId);
        if (node == null) return;

        if (!win)
        {
            FinishRunAfterFailure(node);
            return;
        }

        State.MarkCleared(PendingNodeId);
        PendingNodeId = -1;
        OnNodeResolved?.Invoke(node, true);

        if (node.roomType == MapRoomType.Boss)
            OnActBossCleared();
        else
            SaveRun();
    }

    static void FinishRunAfterFailure(RoguelikeMapNode node)
    {
        if (State != null)
            RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: false, actClearedThisSession: false);
        RoguelikeRunSaveSystem.DeleteActiveSave();
        RoguelikeRunPlantPool.RestoreMainlinePlantsFromPlayerSave();
        State.runActive = false;
        PendingNodeId = -1;
        OnNodeResolved?.Invoke(node, false);
    }

    static void OnActBossCleared()
    {
        RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: false, actClearedThisSession: true);

        if (State.currentActIndex >= ActiveRunConfig.ActCount - 1)
        {
            RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: true, actClearedThisSession: false);
            RoguelikeRunSaveSystem.DeleteActiveSave();
            RoguelikeRunPlantPool.RestoreMainlinePlantsFromPlayerSave();
            State.runActive = false;
            PendingNodeId = -1;
            OnRunCompleted?.Invoke();
            return;
        }

        int nextAct = State.currentActIndex + 1;
        GenerateActMap(nextAct);
        var start = State.currentMap.GetStartNode();
        State.currentNodeId = start.id;
        State.MarkVisited(start.id);
        State.MarkCleared(start.id);
        PendingNodeId = -1;
        SaveRun();
        OnActCompleted?.Invoke();
    }

    public static void AbandonRun()
    {
        if (State != null)
            RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: false, actClearedThisSession: false);
        RoguelikeRunSaveSystem.DeleteActiveSave();
        RoguelikeRunPlantPool.RestoreMainlinePlantsFromPlayerSave();
        PendingNodeId = -1;
        ActiveRunConfig = null;
        State = null;
    }
}
