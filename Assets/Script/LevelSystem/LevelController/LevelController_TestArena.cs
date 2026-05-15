using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 植物/僵尸测试用关卡控制器：不构建波次数据、不预生成右侧僵尸、不在 Update 里推进波次或刷怪；
/// <see cref="GameOver"/> 不触发通关/失败跳转与常规结算 UI（便于场景内手动摆怪、反复验证机制）。
/// <para>
/// 使用方式：在测试场景的 <see cref="LevelManage"/> 指向的关卡对象上挂本组件替代默认 <see cref="LevelController"/>；
/// 仍需配置基础 <see cref="LevelData"/>（场景名、选卡等可按测试需要最小化）。
/// </para>
/// </summary>
public class LevelController_TestArena : LevelController
{
    [FoldoutGroup("测试沙盒")]
    [Tooltip("为 true 时 GameStart 不弹开局文案、不写关卡存档、不显示进度条/续关面板（仍执行 GameStartPlugin、LevelManage.GameStart）。")]
    public bool skipStartupUiAndSave = true;

    public override void CreateZombieWaves()
    {
        if (zombies == null)
            zombies = new List<Chess>();
        else
            zombies.Clear();

        if (waveDatas == null)
            waveDatas = new List<WaveData>();
        else
            waveDatas.Clear();

        t = 0;
        mintime = 16;
        maxtime = 30;
        // 使基类 Update 中「currentWave < MaxWave」恒为假，彻底停用波次时间轴
        currentWave = levelData != null ? levelData.MaxWave : 0;
    }

    protected override void Update()
    {
        // 不放行任何基于波次的刷怪与推进逻辑
    }

    public override void GameStart()
    {
        if (!skipStartupUiAndSave)
        {
            base.GameStart();
            return;
        }

        if (levelData == null)
            Debug.LogError("[LevelController_TestArena] 没有关卡数据");

        bool isLoadFromSave = SaveLoadContext.IsLoadFromSave && SaveLoadContext.CurrentSaveData != null;
        if (isLoadFromSave)
            RestoreLevelProgress(SaveLoadContext.CurrentSaveData.levelData);

        for (int i = 0; i < zombies.Count; i++)
            zombies[i].Death();

        if (levelData != null && levelData.GameStartPlugin != null)
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
        // 刻意不调用：TextPanel 开局、SaveCurrentLevel、ProgressBar、ParsePanel 续关
    }

    public override void GameOver(bool win)
    {
        // 不调用 base：避免 ChangeLevel / ReturnMenu / TextPanel.GameOver / waveDatas.Clear 等胜负流程
        // LevelManage.GameOver 已将 IfGameStart 置为 false；此处仅清理进度条（若存在）
        UIManage.Close<ProgressBar>();
    }
}
