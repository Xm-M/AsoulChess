using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>整局肉鸽：引用 3 个 <see cref="ActMapConfig"/> 与整局元数据。</summary>
[CreateAssetMenu(fileName = "RunMapConfig", menuName = "Roguelike/Run Map Config")]
public class RunMapConfig : ScriptableObject
{
    [LabelText("整局目标时长（分钟，策划参考）")]
    public int targetRunMinutes = 120;

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
