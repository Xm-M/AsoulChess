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

    /// <summary>最近一次全 Run 通关结算快照（<see cref="FinalizeCombatVictory"/> 写入）。</summary>
    public static RoguelikeRunEndSummary LastRunEndSummary { get; private set; }

    /// <summary>肉鸽地图战斗节点进行中（失败 UI 显示「结算」、重开本关不结束 Run）。</summary>
    public static bool IsRoguelikeCombatLevel() => HasActiveRun && PendingNodeId >= 0;

    /// <summary>本 Run 获得新植物（奖励/商店等），写入 State 并 SaveRun。</summary>
    public static bool AddOwnedPlant(string creatorChessName) =>
        RoguelikeRunPlantPool.AddPlant(creatorChessName);

    /// <summary>本 Run 获得新道具（搜刮等），写入 State 并 SaveRun。</summary>
    public static bool AddOwnedProp(string propId) =>
        RoguelikeRunPropPool.AddProp(propId);

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

        ApplyNewRunDefaults(State, config);

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

        NormalizeRunStateFields(State, resolved);

        SaveRun();
        OnRunStarted?.Invoke(State);
        OnActMapGenerated?.Invoke(State);
        return true;
    }

    public static void SaveRun() => TrySaveRun();

    /// <summary>写入 active Run 存档；失败时打日志（如 Test 模式跳过、ActiveRunConfig 丢失）。</summary>
    public static bool TrySaveRun()
    {
        if (State == null || !State.runActive)
            return false;

        if (ActiveRunConfig == null)
        {
            Debug.LogWarning("[Roguelike] SaveRun 跳过：ActiveRunConfig 为空，无法写入 runConfigName");
            return false;
        }

        var save = new RoguelikeRunSaveData
        {
            runConfigName = ActiveRunConfig.name,
            pendingNodeId = PendingNodeId,
            state = CloneState(State),
        };
        if (!RoguelikeRunSaveSystem.TrySave(save))
            return false;

        RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: false, actClearedThisSession: false);
        return true;
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
            ownedPropIds = src.ownedPropIds != null
                ? new List<string>(src.ownedPropIds)
                : new List<string>(),
            selectedBandId = src.selectedBandId,
            selectedBandName = src.selectedBandName,
            runGold = src.runGold,
            activeShopNodeId = src.activeShopNodeId,
            shopOffers = CloneShopOffers(src.shopOffers),
            restUsedNodeIds = src.restUsedNodeIds != null
                ? new List<int>(src.restUsedNodeIds)
                : new List<int>(),
            runLawnMowerCount = src.runLawnMowerCount,
            runLoadoutSlotCount = src.runLoadoutSlotCount,
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

    public static RoguelikeEconomyConfig ResolveEconomyConfig() =>
        ActiveRunConfig?.economyConfig ?? RoguelikeEconomyConfig.CreateRuntimeDefaults();

    static void ApplyNewRunDefaults(RoguelikeRunState state, RunMapConfig config)
    {
        if (state == null)
            return;

        var economy = config?.economyConfig ?? RoguelikeEconomyConfig.CreateRuntimeDefaults();
        state.runLawnMowerCount = economy.GetInitialLawnMowerCount();
        state.runLoadoutSlotCount = economy.GetInitialLoadoutSlotCount();
    }

    /// <summary>旧档缺字段或值为 0 时补默认。</summary>
    public static void NormalizeRunStateFieldsForActiveRun(RoguelikeRunState state) =>
        NormalizeRunStateFields(state, ActiveRunConfig);

    static void NormalizeRunStateFields(RoguelikeRunState state, RunMapConfig config)
    {
        if (state == null)
            return;

        var economy = config?.economyConfig ?? RoguelikeEconomyConfig.CreateRuntimeDefaults();
        if (state.runLawnMowerCount <= 0)
            state.runLawnMowerCount = economy.GetInitialLawnMowerCount();
        if (state.runLoadoutSlotCount <= 0)
            state.runLoadoutSlotCount = economy.GetInitialLoadoutSlotCount();
    }

    /// <summary>
    /// 肉鸽战斗胜利、奖励面板弹出前：按本场 spawn 与存活数扣减 <see cref="RoguelikeRunState.runLawnMowerCount"/>。
    /// 仅过关触发；失败结束 Run 或重开本关不调用。
    /// </summary>
    public static void SettleCombatLawnMowerLosses()
    {
        if (!HasActiveRun || State == null)
            return;

        var level = LevelManage.instance?.currentLevel;
        if (level == null || level.roguelikeKind == RoguelikeLevelKind.None)
            return;

        if (!EnterWarPlugin_CarCreate.TryFindFromLevel(level, out var carPlugin))
            return;

        int spawned = carPlugin.SpawnedThisCombat;
        if (spawned <= 0)
            return;

        int survivors = carPlugin.CountSurvivors();
        int lost = spawned - survivors;
        if (lost <= 0)
            return;

        NormalizeRunStateFieldsForActiveRun(State);
        int before = State.runLawnMowerCount;
        State.runLawnMowerCount = Mathf.Max(0, before - lost);
        Debug.Log(
            $"[Roguelike] 小推车战后结算：spawn={spawned} 存活={survivors} 损失={lost}，Run {before}→{State.runLawnMowerCount}");
        SaveRun();
    }

    static List<RoguelikeShopOffer> CloneShopOffers(List<RoguelikeShopOffer> src)
    {
        if (src == null || src.Count == 0)
            return new List<RoguelikeShopOffer>();

        var list = new List<RoguelikeShopOffer>(src.Count);
        for (int i = 0; i < src.Count; i++)
        {
            var o = src[i];
            if (o == null)
                continue;
            list.Add(new RoguelikeShopOffer
            {
                creatorChessName = o.creatorChessName,
                price = o.price,
                sold = o.sold,
            });
        }
        return list;
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

        return SelectNodeInternal(nodeId, testBypassRules: false);
    }

    /// <summary>Test 模式：忽略选路规则，任意节点可点击进入。</summary>
    public static bool TrySelectNodeForTest(int nodeId)
    {
        if (!HasActiveRun)
            return false;

        var node = State.currentMap?.GetNode(nodeId);
        if (node == null)
            return false;

        return SelectNodeInternal(nodeId, testBypassRules: true);
    }

    static bool SelectNodeInternal(int nodeId, bool testBypassRules)
    {
        var node = State.currentMap.GetNode(nodeId);
        if (node == null)
            return false;

        State.currentNodeId = nodeId;
        State.MarkVisited(nodeId);
        PendingNodeId = nodeId;
        OnNodeEntered?.Invoke(node);

        switch (node.roomType)
        {
            case MapRoomType.Start:
                if (!testBypassRules)
                {
                    State.MarkCleared(nodeId);
                    PendingNodeId = -1;
                    OnNodeResolved?.Invoke(node, true);
                    SaveRun();
                }
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

    public static void ReturnToMapUI(Action onSceneReady = null)
    {
        if (State == null) return;
        if (HasActiveRun)
            SaveRun();

        void OnReady()
        {
            onSceneReady?.Invoke();
            UIManage.GetView<RoguelikeMapPanel>()?.ShowAndRefresh();
            RoguelikeRunInfoPanel.TryShowAndRefresh();
        }

        var sm = GameManage.instance != null ? GameManage.instance.sceneManage : null;
        if (sm == null)
        {
            OnReady();
            return;
        }

        sm.LoadScene("开始", OnReady, () =>
        {
            if (LevelManage.instance != null)
                LevelManage.instance.LeaveState();
        });
    }

    /// <summary>
    /// 肉鸽 Run 进行中从暂停返回主菜单：保存 Run、关闭地图/HUD，保留 active 存档供「继续冒险」。
    /// 战斗中途退出时保留 <see cref="PendingNodeId"/>，不存关卡内快照。
    /// </summary>
    public static void SaveAndExitToMainMenu()
    {
        if (!HasActiveRun)
            return;

        if (!TrySaveRun())
            Debug.LogWarning(
                "[Roguelike] 返回主菜单时未写入 active.json（常见于 Test 模式或 ActiveRunConfig 丢失）；" +
                "同一次运行内仍可从内存继续，重启游戏后进度会丢失。");

        RoguelikeRunPlantPool.RestoreMainlinePlantsFromPlayerSave();
        RoguelikeRunPropPool.RestoreMainlinePropsFromPlayerSave();

        void OnReady()
        {
            HideRunUiPanels();
            UIManage.GetView<StartUI>()?.Show();
        }

        var sm = GameManage.instance != null ? GameManage.instance.sceneManage : null;
        if (sm == null)
        {
            OnReady();
            return;
        }

        sm.LoadScene("开始", OnReady, () => LevelManage.instance?.LeaveState());
    }

    /// <summary>Run 已结束：回开始场景主菜单（LeaveLevel + 关闭肉鸽 UI）。</summary>
    public static void ReturnToStartAfterRunEnded()
    {
        void OnReady()
        {
            HideRunUiPanels();
            UIManage.GetView<StartUI>()?.Show();
        }

        var sm = GameManage.instance != null ? GameManage.instance.sceneManage : null;
        if (sm == null)
        {
            OnReady();
            return;
        }

        sm.LoadScene("开始", OnReady, () => LevelManage.instance?.LeaveState());
    }

    /// <summary>失败结算：快照后结束 Run（由 TextPanel「结算」调用，不自动离关）。</summary>
    public static RoguelikeRunEndSummary BuildDefeatSummaryAndFinishRun()
    {
        var summary = BuildRunEndSummary(RoguelikeRunEndKind.Defeat);
        var node = State?.currentMap?.GetNode(PendingNodeId);
        if (node != null)
            FinishRunAfterFailure(node);
        else
            FinishRunWithoutNodeEvent();
        return summary;
    }

    static RoguelikeRunEndSummary BuildRunEndSummary(RoguelikeRunEndKind kind)
    {
        var summary = new RoguelikeRunEndSummary { kind = kind };
        if (State == null)
            return summary;

        summary.layerReached = ComputeMaxLayerReached(State);
        summary.runGold = State.runGold;
        summary.plantCount = State.ownedPlantCreatorIds?.Count ?? 0;

        int actOneBased = State.currentActIndex + 1;
        if (kind == RoguelikeRunEndKind.Victory && ActiveRunConfig != null)
            summary.actReached = ActiveRunConfig.ActCount;
        else
            summary.actReached = actOneBased;

        return summary;
    }

    static int ComputeMaxLayerReached(RoguelikeRunState state)
    {
        int max = 0;
        if (state.currentMap?.nodes == null)
            return max;

        for (int i = 0; i < state.clearedNodeIds.Count; i++)
        {
            var n = state.currentMap.GetNode(state.clearedNodeIds[i]);
            if (n != null && n.layer > max)
                max = n.layer;
        }

        for (int i = 0; i < state.visitedNodeIds.Count; i++)
        {
            var n = state.currentMap.GetNode(state.visitedNodeIds[i]);
            if (n != null && n.layer > max)
                max = n.layer;
        }

        var cur = state.CurrentNode;
        if (cur != null && cur.layer > max)
            max = cur.layer;

        return max;
    }

    static void HideRunUiPanels()
    {
        UIManage.GetView<RoguelikeMapPanel>()?.Hide();
        UIManage.GetView<RoguelikeRunInfoPanel>()?.Hide();
        UIManage.GetView<RoguelikeShopPanel>()?.Hide();
        UIManage.GetView<RoguelikeRestPanel>()?.Hide();
        UIManage.GetView<RoguelikeRewardPanel>()?.Hide();
        UIManage.GetView<RoguelikeBandSelectPanel>()?.Hide();
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
        RoguelikeRunInfoPanel.HideForCombat();
        RoguelikeRunPlantPool.ApplyRunPlantsToGame();
        RoguelikeRunPropPool.ApplyRunPropsToGame();
        LevelManage.instance.ChangeLevel(level);
    }

    /// <summary>进入地图商店节点（非战斗）。</summary>
    public static void EnterShopNode()
    {
        if (!HasActiveRun || PendingNodeId < 0)
            return;

        var node = State.currentMap.GetNode(PendingNodeId);
        if (node == null || node.roomType != MapRoomType.Shop)
            return;

        RoguelikeShopFlow.EnsureOffersForNode(PendingNodeId);
        SaveRun();
        UIManage.GetView<RoguelikeMapPanel>()?.Hide();
        RoguelikeShopPanel.ShowShop();
    }

    /// <summary>离开商店：标记节点已通关并回到地图。</summary>
    public static void LeaveShopNode()
    {
        if (!HasActiveRun || PendingNodeId < 0)
            return;

        var node = State.currentMap.GetNode(PendingNodeId);
        if (node == null || node.roomType != MapRoomType.Shop)
            return;

        State.MarkCleared(PendingNodeId);
        PendingNodeId = -1;
        RoguelikeShopFlow.ClearShopState();
        OnNodeResolved?.Invoke(node, true);
        SaveRun();

        UIManage.GetView<RoguelikeShopPanel>()?.Hide();
        UIManage.GetView<RoguelikeMapPanel>()?.ShowAndRefresh();
    }

    /// <summary>进入地图休息节点（非战斗）。</summary>
    public static void EnterRestNode()
    {
        if (!HasActiveRun || PendingNodeId < 0)
            return;

        var node = State.currentMap.GetNode(PendingNodeId);
        if (node == null || node.roomType != MapRoomType.Rest)
            return;

        SaveRun();
        UIManage.GetView<RoguelikeMapPanel>()?.Hide();
        RoguelikeRestPanel.ShowRest(PendingNodeId);
    }

    /// <summary>离开休息房：标记节点已通关并回到地图（可不选任何选项）。</summary>
    public static void LeaveRestNode()
    {
        if (!HasActiveRun || PendingNodeId < 0)
            return;

        var node = State.currentMap.GetNode(PendingNodeId);
        if (node == null || node.roomType != MapRoomType.Rest)
            return;

        State.MarkCleared(PendingNodeId);
        PendingNodeId = -1;
        OnNodeResolved?.Invoke(node, true);
        SaveRun();

        UIManage.GetView<RoguelikeRestPanel>()?.Hide();
        UIManage.GetView<RoguelikeMapPanel>()?.ShowAndRefresh();
    }

    /// <summary>读档后若停在未完成的非战斗节点，恢复对应 UI。</summary>
    public static bool TryResumePendingNonCombatNode()
    {
        if (!HasActiveRun || PendingNodeId < 0)
            return false;

        if (State.IsNodeCleared(PendingNodeId))
            return false;

        var node = State.currentMap.GetNode(PendingNodeId);
        if (node == null)
            return false;

        switch (node.roomType)
        {
            case MapRoomType.Shop:
                EnterShopNode();
                return true;
            case MapRoomType.Rest:
                EnterRestNode();
                return true;
            default:
                return false;
        }
    }

    /// <summary>读档后若停在未完成的战斗节点，直接进入该关卡（如暂停返回主菜单后再继续冒险）。</summary>
    public static bool TryResumePendingCombatNode()
    {
        if (!HasActiveRun || PendingNodeId < 0)
            return false;

        if (State.IsNodeCleared(PendingNodeId))
            return false;

        var node = State.currentMap.GetNode(PendingNodeId);
        if (node == null)
            return false;

        switch (node.roomType)
        {
            case MapRoomType.Normal:
            case MapRoomType.Elite:
            case MapRoomType.Boss:
            case MapRoomType.Event:
                if (ResolveLevelForPendingCombat() == null)
                    return false;
                UIManage.GetView<RoguelikeMapPanel>()?.Hide();
                EnterCombatNode();
                return true;
            default:
                return false;
        }
    }

    /// <summary>战斗胜利后结算节点；若全 Run 通关返回 true 并写入 <see cref="LastRunEndSummary"/>。</summary>
    public static bool FinalizeCombatVictory()
    {
        LastRunEndSummary = null;
        if (PendingNodeId < 0 || State == null)
            return false;

        var node = State.currentMap.GetNode(PendingNodeId);
        if (node == null)
            return false;

        State.MarkCleared(PendingNodeId);
        PendingNodeId = -1;
        OnNodeResolved?.Invoke(node, true);

        if (node.roomType == MapRoomType.Boss)
        {
            if (TryCompleteRunAfterFinalBoss())
            {
                LastRunEndSummary = BuildRunEndSummary(RoguelikeRunEndKind.Victory);
                return true;
            }

            AdvanceToNextActAfterBoss();
            return false;
        }

        SaveRun();
        return false;
    }

    public static void OnCombatFinished(bool win)
    {
        if (win)
            FinalizeCombatVictory();
        else if (PendingNodeId >= 0 && State != null)
        {
            var node = State.currentMap.GetNode(PendingNodeId);
            if (node != null)
                FinishRunAfterFailure(node);
        }
    }

    static void FinishRunAfterFailure(RoguelikeMapNode node)
    {
        if (State != null)
            RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: false, actClearedThisSession: false);
        RoguelikeRunSaveSystem.DeleteActiveSave();
        RoguelikeRunPlantPool.RestoreMainlinePlantsFromPlayerSave();
        RoguelikeRunPropPool.RestoreMainlinePropsFromPlayerSave();
        State.runActive = false;
        PendingNodeId = -1;
        OnNodeResolved?.Invoke(node, false);
    }

    static void FinishRunWithoutNodeEvent()
    {
        if (State != null)
            RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: false, actClearedThisSession: false);
        RoguelikeRunSaveSystem.DeleteActiveSave();
        RoguelikeRunPlantPool.RestoreMainlinePlantsFromPlayerSave();
        RoguelikeRunPropPool.RestoreMainlinePropsFromPlayerSave();
        if (State != null)
            State.runActive = false;
        PendingNodeId = -1;
    }

    static bool TryCompleteRunAfterFinalBoss()
    {
        RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: false, actClearedThisSession: true);
        if (ActiveRunConfig == null || State.currentActIndex < ActiveRunConfig.ActCount - 1)
            return false;

        RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: true, actClearedThisSession: false);
        RoguelikeRunSaveSystem.DeleteActiveSave();
        RoguelikeRunPlantPool.RestoreMainlinePlantsFromPlayerSave();
        RoguelikeRunPropPool.RestoreMainlinePropsFromPlayerSave();
        State.runActive = false;
        PendingNodeId = -1;
        OnRunCompleted?.Invoke();
        return true;
    }

    static void AdvanceToNextActAfterBoss()
    {
        RoguelikeMetaProgress.ApplyFromRun(State, runCompleted: false, actClearedThisSession: true);

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
        RoguelikeRunPropPool.RestoreMainlinePropsFromPlayerSave();
        PendingNodeId = -1;
        ActiveRunConfig = null;
        State = null;
    }
}
