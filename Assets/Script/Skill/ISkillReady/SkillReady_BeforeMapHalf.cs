using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 技能释放条件：尚未走过地图一半（从进场侧算起）。
/// 默认按 PVZ 僵尸从右向左：列索引越小表示越靠近玩家，走过一半则返回 false。
/// </summary>
public class SkillReady_BeforeMapHalf : ISkillReady
{
    [LabelText("行进过半比例")]
    [Range(0f, 1f)]
    [Tooltip("从进场列向玩家侧行进的距离占比达到此值时视为过半，返回 false。0.5 = 一半")]
    public float halfProgress = 0.5f;

    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets) { }

    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.moveController?.standTile == null || MapManage.instance == null)
            return false;

        int mapWidth = MapManage.instance.mapSize.x;
        if (mapWidth <= 1)
            return true;

        int currentX = user.moveController.standTile.mapPos.x;
        float traversed = GetTraversedFraction(currentX, mapWidth, user.transform.right.x);
        return traversed < halfProgress;
    }

    /// <summary>
    /// 从进场边到当前格已走过的比例，0=刚进场，1=到达对侧。
    /// </summary>
    static float GetTraversedFraction(int currentColumn, int mapWidth, float facingRightX)
    {
        int maxIndex = mapWidth - 1;
        if (maxIndex <= 0)
            return 0f;

        // 面朝左（常见僵尸）：从 maxIndex 进场，列减小为前进
        if (facingRightX < 0f)
            return (maxIndex - currentColumn) / (float)maxIndex;

        // 面朝右：从 0 进场，列增大为前进
        return currentColumn / (float)maxIndex;
    }
}
