using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生存/无尽模式关卡控制器：多轮循环；轮末 ClearTime + 双次种植（展示→插件→重种挂 Buff）。
/// </summary>
public class LevelController_Endless : LevelController
{
    public EndlessRunState RunState { get; } = new EndlessRunState();
    EnterMapPlugin_EndlessSpawnConfig spawnConfig;
    bool roundTransitioning;
    /// <summary>轮末/读档植物快照（无 buff）；供展示种与开战重种。</summary>
    List<ChessSaveData> plantSnapshot;

    public void ApplySpawnConfig(EnterMapPlugin_EndlessSpawnConfig config)
    {
        spawnConfig = config;
        RunState.BindConfig(config);
    }

    /// <summary>
    /// 生存读档：Timeline 播放前先灌入轮次与手牌缓存；植物在 EnterMap 信号时再种。
    /// </summary>
    public void PrepareSurvivalLoadFromSave(GameSaveData save)
    {
        if (save == null) return;
        if (save.levelData != null)
        {
            if (save.levelData.selectionIndex > 0)
                RunState.selectionIndex = save.levelData.selectionIndex;
            RunState.totalWavesCleared = save.levelData.totalWavesCleared;
        }
        SyncShopHandFromSave(save);
        plantSnapshot = ClonePlantList(save.playerPlants);
        // 轮界存档：本轮从进关开始，不续波
        currentWave = -1;
        t = 0;
    }

    public override void EnterMap()
    {
        if (levelData == null)
        {
            Debug.LogError("没有关卡数据");
            return;
        }

        if (SaveLoadContext.IsLoadFromSave && SaveLoadContext.CurrentSaveData != null)
        {
            RestoreLevelProgress(SaveLoadContext.CurrentSaveData.levelData);
            BuffDatabase.RestoreRegistry(SaveLoadContext.CurrentSaveData.buffRegistry);
            if (plantSnapshot == null || plantSnapshot.Count == 0)
                plantSnapshot = ClonePlantList(SaveLoadContext.CurrentSaveData.playerPlants);
            RestoreSunLightFromSave(SaveLoadContext.CurrentSaveData);
            SyncShopHandFromSave(SaveLoadContext.CurrentSaveData);
            currentWave = -1;
            t = 0;
        }

        // 选卡前：静默清场 + 无 Buff 展示种（同会话轮间与读档统一）
        if (plantSnapshot != null && plantSnapshot.Count > 0)
        {
            SilentClearPlayerPlants();
            RestorePlayerPlants(plantSnapshot, restoreRuntimeState: false, forDisplayOnly: true);
        }

        if (levelData.EnterMapPlugin != null)
        {
            for (int i = 0; i < levelData.EnterMapPlugin.Count; i++)
                levelData.EnterMapPlugin[i].StadgeEffect(this);
        }
        EventController.Instance.TriggerEvent(EventName.EnterMap.ToString());
        RunRoundEnter();
    }

    static List<ChessSaveData> ClonePlantList(List<ChessSaveData> src)
    {
        if (src == null || src.Count == 0)
            return new List<ChessSaveData>();
        return new List<ChessSaveData>(src);
    }

    static void SilentClearPlayerPlants()
    {
        var team = ChessTeamManage.Instance?.GetTeam("Player");
        if (team == null) return;
        var list = new List<Chess>(team);
        for (int i = 0; i < list.Count; i++)
        {
            var c = list[i];
            if (c == null) continue;
            c.RemoveFromFieldSilent();
        }
    }

    void CapturePlantSnapshotFromField()
    {
        plantSnapshot = new List<ChessSaveData>();
        var team = ChessTeamManage.Instance?.GetTeam("Player");
        if (team == null) return;
        foreach (var chess in team)
        {
            if (chess == null || chess.IfDeath) continue;
            var tile = chess.moveController?.standTile;
            if (tile == null) continue;
            plantSnapshot.Add(new ChessSaveData
            {
                creatorId = chess.propertyController?.creator?.chessName ?? "",
                tileX = tile.mapPos.x,
                tileY = tile.mapPos.y,
                hp = chess.propertyController.GetHp(),
                hpMax = chess.propertyController.GetMaxHp(),
                buffs = new List<BuffSaveData>(),
                stateName = (int)StateName.IdleState
            });
        }
    }

    /// <summary>GameStart 插件后：灭展示种 → 完整进战重种（挂 Buff/Timer）。</summary>
    void ReplantForCombatAfterPlugins()
    {
        if (plantSnapshot == null || plantSnapshot.Count == 0)
            return;
        SilentClearPlayerPlants();
        RestorePlayerPlants(plantSnapshot, restoreRuntimeState: false, forDisplayOnly: false);
    }

    void RunRoundEnter()
    {
        // 生存读档/轮界：始终从本轮进关开始，不续局内波次
        bool preserveWave = SaveLoadContext.IsLoadFromSave
            && currentWave >= 0
            && levelData != null
            && levelData.levelMode != LevelMode.SurvivalMode;
        int waveToKeep = currentWave;
        float tToKeep = t;
        float minToKeep = mintime;
        float maxToKeep = maxtime;

        RestoreRunStateForLoadIfNeeded();
        RunState.segmentPool.Clear();
        RunState.RebuildSegmentPool(levelData, RunState.selectionIndex);

        RunState.ResetRarityUseCounts();
        ClearZombiePreviews();
        CreateRoundWaves();
        RefreshZombiePreviewTiles(RunState.segmentPool);
        roundTransitioning = false;

        if (preserveWave)
        {
            currentWave = Mathf.Clamp(waveToKeep, -1, waveDatas != null ? waveDatas.Count - 1 : -1);
            t = tToKeep;
            mintime = minToKeep;
            maxtime = maxToKeep;
        }
        else
        {
            t = 0;
            currentWave = -1;
        }
    }

    void RestoreRunStateForLoadIfNeeded()
    {
        if (!SaveLoadContext.IsLoadFromSave || SaveLoadContext.CurrentSaveData?.levelData == null)
            return;
        var ld = SaveLoadContext.CurrentSaveData.levelData;
        if (ld.selectionIndex > 0)
            RunState.selectionIndex = ld.selectionIndex;
        RunState.totalWavesCleared = ld.totalWavesCleared;
    }

    void CreateRoundWaves()
    {
        if (waveDatas == null)
            waveDatas = new List<WaveData>();
        else
            waveDatas.Clear();

        int waves = levelData.MaxWave;
        if (spawnConfig != null && spawnConfig.wavesPerRound > 0)
            waves = Mathf.Min(spawnConfig.wavesPerRound, levelData.MaxWave);
        // 全局波号：已清波次 + 本轮序号 → 预算 / waveLimit 随轮次递增
        int globalBase = Mathf.Max(0, RunState.totalWavesCleared);
        for (int i = 0; i < waves; i++)
        {
            var waveData = new WaveData_Endless(RunState);
            int globalWave = globalBase + i + 1;
            waveData.InitWave(globalWave, levelData);
            waveDatas.Add(waveData);
        }
    }

    public override void RestoreLevelProgress(LevelSaveData data)
    {
        base.RestoreLevelProgress(data);
        if (data == null) return;
        if (data.selectionIndex > 0)
            RunState.selectionIndex = data.selectionIndex;
        RunState.totalWavesCleared = data.totalWavesCleared;
    }

    public override void GameStart()
    {
        if (levelData == null)
        {
            Debug.LogError("没有关卡数据");
            return;
        }

        bool isLoadFromSave = SaveLoadContext.IsLoadFromSave && SaveLoadContext.CurrentSaveData != null;
        ClearZombiePreviews();

        if (levelData.GameStartPlugin != null)
        {
            for (int i = 0; i < levelData.GameStartPlugin.Count; i++)
                levelData.GameStartPlugin[i].StadgeEffect(this);
        }

        // 插件基于展示种跑完后，全灭再重种以挂 Buff / 攻击 Timer
        ReplantForCombatAfterPlugins();

        TryLockShopHand();
        LevelManage.instance.GameStart();
        roundTransitioning = false;

        if (!isLoadFromSave && currentWave >= levelData.MaxWave)
            currentWave = -1;

        bool isFirstRound = RunState.selectionIndex <= 1;
        if (!isLoadFromSave && isFirstRound)
        {
            UIManage.Show<TextPanel>();
            UIManage.GetView<TextPanel>().GameStart();
        }

        if (isLoadFromSave)
        {
            SceneManage.instance.LoadOver();
        }
    }

    void TryLockShopHand()
    {
        if (levelData?.PreParePlugin == null) return;
        for (int i = 0; i < levelData.PreParePlugin.Count; i++)
        {
            if (levelData.PreParePlugin[i] is PreParePlugun_ShowPlantShop shop)
                shop.EnsureLockedHandFromShop();
        }
    }

    static void RestoreSunLightFromSave(GameSaveData save)
    {
        if (save?.plantsShopData == null || SunLightPanel.instance == null)
            return;
        SunLightPanel.instance.SetSunLight(save.plantsShopData.sunLight);
    }

    void SyncShopHandFromSave(GameSaveData save)
    {
        if (levelData?.PreParePlugin == null || save == null) return;
        for (int i = 0; i < levelData.PreParePlugin.Count; i++)
        {
            if (levelData.PreParePlugin[i] is PreParePlugun_ShowPlantShop shop)
                shop.SyncLockedHandFromSave(save);
        }
    }

    public int GetWavesPerRoundDisplay()
    {
        if (waveDatas != null && waveDatas.Count > 0)
            return waveDatas.Count;
        if (spawnConfig != null && spawnConfig.wavesPerRound > 0)
            return spawnConfig.wavesPerRound;
        return levelData != null ? levelData.MaxWave : 1;
    }

    /// <summary>当前轮次（从 1 起）。</summary>
    public int GetRoundDisplay() => Mathf.Max(1, RunState.selectionIndex);

    /// <summary>最大轮次；无尽（survivalMaxWave&lt;0）返回 -1。</summary>
    public int GetMaxRoundsDisplay()
    {
        if (spawnConfig == null || spawnConfig.survivalMaxWave < 0)
            return -1;
        int perRound = spawnConfig.wavesPerRound > 0 ? spawnConfig.wavesPerRound : levelData.MaxWave;
        if (perRound <= 0) perRound = 1;
        return Mathf.Max(1, Mathf.CeilToInt(spawnConfig.survivalMaxWave / (float)perRound));
    }

    protected override void DoEnterNextWave()
    {
        if (waveDatas == null || waveDatas.Count == 0 || levelData == null)
            return;

        int nextIndex = currentWave + 1;
        if (nextIndex >= waveDatas.Count)
            return;

        t = -2;
        int roundMax = GetWavesPerRoundDisplay();
        UIManage.GetView<ProgressBar>()?.MoveBar(nextIndex + 1, roundMax);
        waveDatas[nextIndex].EnterWave();
        currentWave++;
        mintime = 4;
        maxtime = UnityEngine.Random.Range(0, 6) + 23;
    }

    int LastWaveIndex => waveDatas != null && waveDatas.Count > 0 ? waveDatas.Count - 1 : 0;

    protected override bool WaveCanAdvance()
    {
        var wd = waveDatas[currentWave];
        bool hpOk = wd.CheckZombieHp();
        bool allDead = wd.IsCreateOver && wd.GetCurrentZombieHpSum() <= 0;
        bool isLastWaveOfRound = currentWave >= LastWaveIndex;
        if (isLastWaveOfRound)
        {
            float hardLimit = spawnConfig != null ? spawnConfig.lastWaveHardLimit : 70f;
            if (allDead)
                return true;
            return (hpOk && t > mintime) || t >= hardLimit;
        }

        bool isFinalBigWave = wd.Wave % 10 == 0;
        if (isFinalBigWave)
            return (hpOk && t > mintime) || allDead;
        return (hpOk && t > mintime) || (t > maxtime);
    }

    protected override void Update()
    {
        if (roundTransitioning)
            return;
        if (!LevelManage.instance.IfGameStart || waveDatas == null || currentWave >= waveDatas.Count)
            return;

        t += Time.deltaTime;
        if (waveDatas == null || waveDatas.Count == 0)
            return;

        if (currentWave == -1 && t > mintime)
        {
            waveDatas[0].EnterWave();
            if (RunState.selectionIndex <= 1)
                UIManage.GetView<TextPanel>().FirstZombieCom();
            EventController.Instance.TriggerEvent(EventName.FirstZombieComming.ToString());
            mintime = UnityEngine.Random.Range(0, 6);
            maxtime = mintime + 23;
            currentWave++;
            t = 0;
            UIManage.Show<ProgressBar>();
            var bar = UIManage.GetView<ProgressBar>();
            int roundMax = GetWavesPerRoundDisplay();
            bar.SetFlag(Mathf.Max(1, roundMax / 10));
            bar.MoveBar(currentWave + 1, roundMax);
        }
        else if (currentWave >= 0 && currentWave < waveDatas.Count && WaveCanAdvance())
        {
            if (currentWave >= LastWaveIndex)
            {
                roundTransitioning = true;
                StartCoroutine(OnRoundCompleteCoroutine());
                return;
            }

            t = 0;
            DoEnterNextWave();
        }
    }

    IEnumerator OnRoundCompleteCoroutine()
    {
        var lastWave = waveDatas[LastWaveIndex];
        float spawnWait = 0f;
        while (lastWave != null && !lastWave.IsCreateOver && spawnWait < 30f)
        {
            spawnWait += Time.deltaTime;
            yield return null;
        }

        // 通关奖杯落点：清场前取最后一只僵尸位（已全灭时仍可从 waveZombies 取位）
        Vector3 victoryPos = CaptureLastZombieWorldPos();

        // 70s 强切时立刻清残怪；自然清场时此处通常已无敌人
        ForceClearRoundEnemies();
        ClearFieldSunLights();
        // 整表清 Timer；下一轮靠展示种→插件→重种重建
        if (GameManage.instance?.timerManage != null)
            GameManage.instance.timerManage.ClearTime();

        bool willWin = WillSurviveWinAfterThisRound();
        // 非通关轮：ClearTime 后立刻播「更多僵尸要来了」，再等 transitionDelay
        if (!willWin)
            ShowMoreZombiesComingBanner();

        // 等待期间保持 IfGameStart=true；Timer 已清，仅阻止 Update 推进波次
        float transitionDelay = spawnConfig != null ? spawnConfig.roundTransitionDelay : 4f;
        if (transitionDelay > 0f)
            yield return new WaitForSeconds(transitionDelay);

        RoundOverPlugins();

        RunState.totalWavesCleared += waveDatas.Count;
        CapturePlantSnapshotFromField();

        if (willWin || IsSurvivalWinReached())
        {
            RunState.selectionIndex++;
            currentWave = -1;
            t = 0;
            // 掉落奖杯；玩家点击后再 GameOver(true)。删档后不再存档。
            SpawnSurvivalVictoryReward(victoryPos);
            LevelManage.instance.GamePause();
            UIManage.Close<ProgressBar>();
            roundTransitioning = false;
            yield break;
        }

        RunState.selectionIndex++;
        currentWave = -1;
        t = 0;
        ClearZombiePreviews();
        SaveSystem.SaveCurrentLevel();
        LevelManage.instance.GamePause();
        UIManage.Close<ProgressBar>();

        yield return PlayRoundTimeline();
    }

    /// <summary>本轮结束后（累加本轮波数后）是否达到 survivalMaxWave。</summary>
    bool WillSurviveWinAfterThisRound()
    {
        if (spawnConfig == null || spawnConfig.survivalMaxWave < 0)
            return false;
        int wavesThisRound = waveDatas != null ? waveDatas.Count : 0;
        return RunState.totalWavesCleared + wavesThisRound >= spawnConfig.survivalMaxWave;
    }

    bool IsSurvivalWinReached()
    {
        if (spawnConfig == null || spawnConfig.survivalMaxWave < 0)
            return false;
        return RunState.totalWavesCleared >= spawnConfig.survivalMaxWave;
    }

    static void ShowMoreZombiesComingBanner()
    {
        UIManage.Show<TextPanel>();
        var panel = UIManage.GetView<TextPanel>();
        panel?.MoreZombiesComing();
    }

    Vector3 CaptureLastZombieWorldPos()
    {
        if (waveDatas != null)
        {
            for (int w = waveDatas.Count - 1; w >= 0; w--)
            {
                if (waveDatas[w] != null && waveDatas[w].TryGetLastZombieWorldPos(out var pos))
                    return pos;
            }
        }
        var map = MapManage_PVZ.instance;
        if (map != null && map.tiles != null && MapManage.instance != null)
        {
            var size = MapManage.instance.mapSize;
            int cx = Mathf.Clamp(size.x / 2, 0, Mathf.Max(0, size.x - 1));
            int cy = Mathf.Clamp(size.y / 2, 0, Mathf.Max(0, size.y - 1));
            var tile = map.tiles[cx, cy];
            if (tile != null)
                return tile.transform.position;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// 生存通关奖励。WaveData.SpawnVictoryReward 只在波次对象上，控制器需走 levelData.outcome。
    /// </summary>
    void SpawnSurvivalVictoryReward(Vector3 lastZombiePos)
    {
        SaveSystem.DeleteSave(LevelManage.instance.currentLevel);
        (levelData?.outcome ?? new LevelOutCome_Trophy()).HandleOutcome(true, lastZombiePos);
    }

    /// <summary>
    /// 轮末重播 Timeline，后续由信号驱动：EnterMap → GamePrepare（Pause + 选卡）→ 玩家点开战 → WhenGameStart/Resume → GameStart。
    /// 不在此处等待或超时强切，选卡时间由玩家自己控制。
    /// </summary>
    IEnumerator PlayRoundTimeline()
    {
        var mapPvz = MapManage.instance as MapManage_PVZ;
        var dir = mapPvz != null ? mapPvz.dir : null;
        if (dir == null || dir.playableAsset == null)
        {
            yield return BeginNextRoundWithoutTimeline();
            yield break;
        }

        for (int i = 0; i < 3 && dir.duration <= 0; i++)
            yield return null;

        float configuredStart = spawnConfig != null ? spawnConfig.roundTimelineStartTime : 0f;
        double duration = dir.duration;
        float startTime = configuredStart;
        if (duration > 0)
            startTime = Mathf.Clamp(configuredStart, 0f, Mathf.Max(0f, (float)duration - 0.05f));

        dir.Stop();
        dir.time = startTime;
        dir.Evaluate();
        dir.Play();

        // 战斗已由 GamePause 停住；后续交给 Timeline + 选卡 UI，玩家确认后 WhenGameStart() 再 Resume
        roundTransitioning = false;
    }

    /// <summary>无 Timeline 时：只 EnterMap + GamePrepare 打开选卡，不调用 GameStart，等玩家点商店开战按钮。</summary>
    IEnumerator BeginNextRoundWithoutTimeline()
    {
        roundTransitioning = false;
        EnterMap();
        GamePrepare();
        yield return null;
    }

    void ForceClearRoundEnemies()
    {
        if (waveDatas == null)
            return;
        foreach (var wave in waveDatas)
            wave.ForceClearRemaining();
    }

    /// <summary>轮末回收场上阳光；SunLight.OnDisable 会 Stop 自动拾取 Timer。</summary>
    static void ClearFieldSunLights()
    {
        var panel = UIManage.GetView<ItemPanel>();
        if (panel == null) return;
        var lights = panel.GetComponentsInChildren<SunLight>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            var light = lights[i];
            if (light == null) continue;
            light.Recycle();
        }
    }

    public override void GameOver(bool win)
    {
        // 无尽（survivalMaxWave < 0）不允许胜利结算直接离关
        if (win && spawnConfig != null && spawnConfig.survivalMaxWave < 0)
            return;
        base.GameOver(win);
    }
}
