using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

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

        RunRoundEnter();
    }

    void RunRoundEnter()
    {
        RestoreSegmentPoolForLoadIfNeeded();
        if (RunState.segmentPool.Count == 0)
            RunState.RebuildSegmentPool(levelData, RunState.selectionIndex);

        RunState.ResetRarityUseCounts();
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

        int waves = spawnConfig != null ? spawnConfig.wavesPerRound : levelData.MaxWave;
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

        if (zombies != null)
        {
            for (int i = 0; i < zombies.Count; i++)
                zombies[i].Death();
        }

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

    protected override bool WaveCanAdvance()
    {
        var wd = waveDatas[currentWave];
        bool hpOk = wd.CheckZombieHp();
        bool isLastWaveOfRound = currentWave == levelData.MaxWave - 1;
        if (isLastWaveOfRound)
        {
            float hardLimit = spawnConfig != null ? spawnConfig.lastWaveHardLimit : 70f;
            return (hpOk && t > mintime) || t >= hardLimit;
        }

        bool isFinalBigWave = wd.Wave == levelData.MaxWave && wd.Wave % 10 == 0;
        if (isFinalBigWave)
            return hpOk && t > mintime;
        return (hpOk && t > mintime) || (t > maxtime);
    }

    protected override void Update()
    {
        if (roundTransitioning)
            return;
        if (!LevelManage.instance.IfGameStart || currentWave >= levelData.MaxWave)
            return;

        t += Time.deltaTime;
        if (currentWave == -1 && t > mintime)
        {
            SaveSystem.SaveCurrentLevel();
            waveDatas[currentWave + 1].EnterWave();
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
        else if (currentWave != -1 && WaveCanAdvance())
        {
            if (currentWave == levelData.MaxWave - 1)
            {
                roundTransitioning = true;
                StartCoroutine(OnRoundCompleteCoroutine());
                return;
            }

            if (waveDatas[currentWave].GetCurrentZombieHpSum() <= 0 && currentWave < levelData.MaxWave - 1)
                SaveSystem.SaveCurrentLevel();
            t = 0;
            DoEnterNextWave();
        }
    }

    IEnumerator OnRoundCompleteCoroutine()
    {
        var lastWave = waveDatas[levelData.MaxWave - 1];
        while (lastWave != null && !lastWave.IsCreateOver)
            yield return null;

        ForceClearRoundEnemies();
        RunState.totalWavesCleared += levelData.MaxWave;
        SaveSystem.SaveCurrentLevel();
        LevelManage.instance.GamePause();
        UIManage.Close<ProgressBar>();

        if (CheckSurvivalWin())
            yield break;

        RunState.selectionIndex++;
        currentWave = levelData.MaxWave;

        var mapPvz = MapManage.instance as MapManage_PVZ;
        var dir = mapPvz != null ? mapPvz.dir : null;
        if (dir != null)
        {
            float startTime = spawnConfig != null ? spawnConfig.roundTimelineStartTime : 30f;
            for (int i = 0; i < 3 && dir.duration <= 0; i++)
                yield return null;
            double duration = dir.duration;
            dir.Stop();
            if (duration > 0)
                dir.time = Mathf.Clamp(startTime, 0, (float)duration);
            else
                dir.time = startTime;
            dir.Play();
        }
        else
        {
            roundTransitioning = false;
            EnterMap();
            GamePrepare();
            GameStart();
        }
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
