using UnityEngine;

/// <summary>
/// 开局雪地：与 <see cref="GameStartPlugin_Smoke"/> 对称，用「列」与「持续时间」控制；
/// 仅在列索引 <b>大于</b> <see cref="hideColumns"/> 的可铺冰列铺冰，且 <see cref="Effect_Snow.CanPlaceIceOnTile"/>（非 Water）才铺；开局冰为 <b>敌方</b>，己方可通过技能覆盖为己方冰。
/// 预制体在 <see cref="WeatherManage"/> 上配置。
/// </summary>
public class GameStartPlugin_Snow : ILevelPlugin
{
    [Tooltip("列索引 ≤ 此值的列不铺开局冰。如 4 表示第 0、1、2、3、4 列不铺冰（与烟雾 hideColumns 语义一致）")]
    public int hideColumns = 4;

    [Tooltip("开局所铺冰块的持续时间，足够长可视为整局不化")]
    public float iceLifetimeSeconds = 99999f;

    public void StadgeEffect(LevelController levelController)
    {
        var wm = GameManage.instance?.weatherManage;
        if (wm == null) return;
        var effect = wm.GetOrCreateSnow();
        if (effect == null) return;
        effect.InitSnow();
        effect.PlaceInitialIceAfterExcludedColumns(hideColumns, iceLifetimeSeconds, "Enemy");
    }

    public void OverPlugin(LevelController levelController)
    {
        // Effect_Snow 由 WeatherManage 在 WhenLeaveLevel 统一回收并融冰
    }
}
