using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生存/无尽模式关卡控制器：多轮 20 波循环，末波 70s 硬限时，轮末清怪后 Timeline 衔接下一轮。
/// </summary>
public class LevelController_Endless : LevelController
{
    public EndlessRunState RunState { get; } = new EndlessRunState();
    EnterMapPlugin_EndlessSpawnConfig spawnConfig;
    bool roundTransitioning;

    public void ApplySpawnConfig(EnterMapPlugin_EndlessSpawnConfig config)
    {
        spawnConfig = config;
        RunState.BindConfig(config);
    }

    public override void EnterMap()
    {
        if (levelData == null)
        {
            Debug.LogError("没有关卡数据");
            return;
        }

        if (levelData.EnterMapPlugin != null)
        {
            for (int i = 0; i < levelData.EnterMapPlugin.Count; i++)
                levelData.EnterMapPlugin[i].StadgeEffect(this);
        }
        EventController.Instance.TriggerEvent(EventName.EnterMap.ToString());
        RunRoundEnter();
    }

    void RunRoundEnter()
    {
        RestoreSegmentPoolForLoadIfNeeded();
        if (RunState.segmentPool.Count == 0)
            RunState.RebuildSegmentPool(levelData, RunState.selectionIndex);

        RunState.ResetRarityUseCounts();
        ClearZombiePreviews();
        CreateRoundWaves();
        RefreshZombiePreviewTiles(RunState.segmentPool);
        t = 0;
        currentWave = -1;
        roundTransitioning = false;
    }

    void RestoreSegmentPoolForLoadIfNeeded()
    {
        if (!SaveLoadContext.IsLoadFromSave || SaveLoadContext.CurrentSaveData?.levelData == null)
            return;
        var ld = SaveLoadContext.CurrentSaveData.levelData;
        if (ld.selectionIndex > 0)
            RunState.selectionIndex = ld.selectionIndex;
        RunState.totalWavesCleared = ld.totalWavesCleared;
        if (ld.segmentPoolIds != null && ld.segmentPoolIds.Count > 0)
            RunState.RestoreSegmentPoolFromIds(levelData, ld.segmentPoolIds);
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
        for (int i = 0; i < waves; i++)
        {
            var waveData = new WaveData_Endless(RunState);
            waveData.InitWave(i + 1, levelData);
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
        if (isLoadFromSave)
            RestoreLevelProgress(SaveLoadContext.CurrentSaveData.levelData);

        ClearZombiePreviews();

        if (levelData.GameStartPlugin != null)
        {
            for (int i = 0; i < levelData.GameStartPlugin.Count; i++)
                levelData.GameStartPlugin[i].StadgeEffect(this);
        }

        if (isLoadFromSave)
        {
            BuffDatabase.RestoreRegistry(SaveLoadContext.CurrentSaveData.buffRegistry);
            RestorePlayerPlants(SaveLoadContext.CurrentSaveData.playerPlants);
        }

        LevelManage.instance.GameStart();
        roundTransitioning = false;

        if (!isLoadFromSave && currentWave >= levelData.MaxWave)
            currentWave = -1;

        bool isFirstRound = RunState.selectionIndex <= 1;
        if (!isLoadFromSave && isFirstRound)
        {
            UIManage.Show<TextPanel>();
            UIManage.GetView<TextPanel>().GameStart();
            SaveSystem.SaveCurrentLevel();
        }

        if (isLoadFromSave && currentWave >= 0)
        {
            UIManage.Show<ProgressBar>();
            UIManage.GetView<ProgressBar>().SetFlag(levelData.MaxWave / 10);
            UIManage.GetView<ProgressBar>().MoveBar(currentWave + 1, levelData.MaxWave);
            if (t >= mintime)
                DoEnterNextWave();
        }

        if (isLoadFromSave)
        {
            SceneManage.instance.LoadOver();
            UIManage.GetView<ParsePanel>().ShowContinuePanel();
        }
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
            SaveSystem.SaveCurrentLevel();
            waveDatas[0].EnterWave();
            if (RunState.selectionIndex <= 1)
                UIManage.GetView<TextPanel>().FirstZombieCom();
            EventController.Instance.TriggerEvent(EventName.FirstZombieComming.ToString());
            mintime = UnityEngine.Random.Range(0, 6);
            maxtime = mintime + 23;
            currentWave++;
            t = 0;
            UIManage.Show<ProgressBar>();
            UIManage.GetView<ProgressBar>().SetFlag(levelData.MaxWave / 10);
        }
        else if (currentWave >= 0 && currentWave < waveDatas.Count && WaveCanAdvance())
        {
            if (currentWave >= LastWaveIndex)
            {
                roundTransitioning = true;
                StartCoroutine(OnRoundCompleteCoroutine());
                return;
            }

            if (waveDatas[currentWave].GetCurrentZombieHpSum() <= 0 && currentWave < LastWaveIndex)
                SaveSystem.SaveCurrentLevel();
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

        // 70s 强切时立刻清残怪；自然清场时此处通常已无敌人
        ForceClearRoundEnemies();

        // 等待期间保持 IfGameStart=true，场上植物等照常运行；仅阻止 Update 推进波次
        float transitionDelay = spawnConfig != null ? spawnConfig.roundTransitionDelay : 4f;
        if (transitionDelay > 0f)
            yield return new WaitForSeconds(transitionDelay);

        RoundOverPlugins();

        RunState.totalWavesCleared += waveDatas.Count;
        SaveSystem.SaveCurrentLevel();
        LevelManage.instance.GamePause();
        UIManage.Close<ProgressBar>();

        if (CheckSurvivalWin())
        {
            roundTransitioning = false;
            yield break;
        }

        RunState.selectionIndex++;
        currentWave = -1;
        t = 0;
        ClearZombiePreviews();

        yield return PlayRoundTimeline();
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

    bool CheckSurvivalWin()
    {
        if (spawnConfig == null || spawnConfig.survivalMaxWave < 0)
            return false;
        if (RunState.totalWavesCleared < spawnConfig.survivalMaxWave)
            return false;
        LevelManage.instance.GameOver(true);
        return true;
    }

    public override void GameOver(bool win)
    {
        if (win && spawnConfig != null && spawnConfig.survivalMaxWave < 0)
            return;
        base.GameOver(win);
    }
}
