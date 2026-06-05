using UnityEngine;

/// <summary>
/// 生存/无尽模式出怪配置插件。挂在 LevelData.EnterMapPlugin 首位，每轮 EnterMap 写入 RunState。
/// </summary>
public class EnterMapPlugin_EndlessSpawnConfig : ILevelPlugin
{
    [Tooltip("每轮波数，应与 LevelData.MaxWave 一致")]
    public int wavesPerRound = 20;

    [Tooltip("本轮最后一波硬限时（秒）")]
    public float lastWaveHardLimit = 70f;

    [Tooltip("全局通关累计波次；-1 表示永不通关")]
    public int survivalMaxWave = -1;

    [Tooltip("首轮出场种类数")]
    public int initialPoolSize = 4;

    [Tooltip("每多一轮增加的种类数")]
    public int poolGrowthPerRound = 1;

    [Tooltip("每种被抽中后稀有度衰减量，0 表示不衰减")]
    public int rarityDecayPerUse;

    [Tooltip("第 2 轮起 Timeline 从该时间点播放（秒）")]
    public float roundTimelineStartTime = 30f;

    public void StadgeEffect(LevelController levelController)
    {
        if (levelController is not LevelController_Endless endless)
            return;
        endless.ApplySpawnConfig(this);
    }

    public void OverPlugin(LevelController levelController) { }
}
