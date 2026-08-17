using System;

/// <summary>
/// 开战「准备种植 / Ready to Plant」横幅跳过标志。
/// 由 <see cref="GameStartPlugin_SkipReadyToPlant"/> 置位，<see cref="TextPanel.GameStart"/> 消费。
/// </summary>
public static class ReadyToPlantBanner
{
    public static bool SkipOnce;

    public static void Clear() => SkipOnce = false;
}

/// <summary>
/// 跳过开战准备种植文字（动画 + 音效）。TextPanel 仍会 Show，波次/失败提示可用。
/// </summary>
[Serializable]
public class GameStartPlugin_SkipReadyToPlant : ILevelPlugin
{
    public void StadgeEffect(LevelController levelController)
    {
        ReadyToPlantBanner.SkipOnce = true;
    }

    public void OverPlugin(LevelController levelController)
    {
        ReadyToPlantBanner.Clear();
    }
}
