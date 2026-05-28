using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>整局肉鸽：引用 3 个 <see cref="ActMapConfig"/> 与整局元数据。</summary>
[CreateAssetMenu(fileName = "RunMapConfig", menuName = "Roguelike/Run Map Config")]
public class RunMapConfig : ScriptableObject
{
    [LabelText("整局目标时长（分钟，策划参考）")]
    public int targetRunMinutes = 120;

    [FoldoutGroup("Run 植物池"), Tooltip("新开 Run 的初始植物（creator.chessName）。为空则从 PlayerSaveData 复制；若已选乐队则以乐队为准")]
    public List<string> startingPlantCreatorIds = new List<string>();

    [FoldoutGroup("开局乐队"), Tooltip("可选：集中管理所有 BandMes")]
    public RoguelikeBandCatalog bandCatalog;

    [FoldoutGroup("开局乐队"), Tooltip("未使用 Catalog 时，直接在此配置乐队列表")]
    public List<BandMes> startingBands = new List<BandMes>();

    public ActMapConfig act1;
    public ActMapConfig act2;
    public ActMapConfig act3;

    public ActMapConfig GetAct(int actIndex)
    {
        return actIndex switch
        {
            0 => act1,
            1 => act2,
            2 => act3,
            _ => null
        };
    }

    public int ActCount => 3;

    public List<BandMes> GetStartingBands()
    {
        if (bandCatalog != null && bandCatalog.bands != null && bandCatalog.bands.Count > 0)
            return bandCatalog.bands;
        return startingBands ?? new List<BandMes>();
    }

    public bool Validate(out string error)
    {
        if (act1 == null || act2 == null || act3 == null)
        {
            error = "act1 / act2 / act3 均需配置";
            return false;
        }
        if (!act1.Validate(out error)) return false;
        if (!act2.Validate(out error)) return false;
        if (!act3.Validate(out error)) return false;
        error = null;
        return true;
    }
}
