using UnityEngine;

/// <summary>
/// 开局初始化雾气：预制体在 <see cref="WeatherManage"/> 上配置；本插件只负责 <see cref="InitSmokes"/> 与列隐藏参数。
/// </summary>
public class GameStartPlugin_Smoke : ILevelPlugin
{
    [Tooltip("隐藏 0~n 列的雾气，n 可配置。如 4 表示前 5 列（0~4）无雾")]
    public int hideColumns = 4;
    [Tooltip("隐藏持续时间，足够长则整局不显示")]
    public float hideTime = 99999f;

    public void StadgeEffect(LevelController levelController)
    {
        var wm = GameManage.instance?.weatherManage;
        if (wm == null) return;
        var effect = wm.GetOrCreateSmoke();
        if (effect == null) return;
        effect.InitSmokes();
        effect.HideSmokeInColumns(hideColumns, hideTime);
    }

    public void OverPlugin(LevelController levelController)
    {
        // 雾气实例由 WeatherManage 在 WhenLeaveLevel 统一回收
    }
}
