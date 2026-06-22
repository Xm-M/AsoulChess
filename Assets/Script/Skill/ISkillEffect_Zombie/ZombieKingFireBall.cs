using UnityEngine;

/// <summary>
/// 僵王俯身吐出的火球；由 <see cref="Skill_ZombieKingBoss"/> 生成时写入 <see cref="mapRowY"/>，
/// 供冰爆辣椒等整行技能判定是否在该行并清除。
/// </summary>
public class ZombieKingFireBall : MonoBehaviour
{
    public int mapRowY = -1;

    public bool IsOnMapRow(int rowY) => mapRowY >= 0 && mapRowY == rowY;
}
