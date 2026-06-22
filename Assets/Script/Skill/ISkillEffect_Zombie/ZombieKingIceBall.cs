using UnityEngine;

/// <summary>
/// 僵王俯身吐出的冰球；由 <see cref="Skill_ZombieKingBoss"/> 生成时写入 <see cref="mapRowY"/>，
/// 供整行技能（如 <see cref="SkillEffect_Jalapeno"/>）判定是否在该行。
/// </summary>
public class ZombieKingIceBall : MonoBehaviour
{
    public int mapRowY = -1;

    public bool IsOnMapRow(int rowY) => mapRowY >= 0 && mapRowY == rowY;
}
