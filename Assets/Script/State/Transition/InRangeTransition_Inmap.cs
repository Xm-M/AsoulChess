using UnityEngine;

/// <summary>
/// 与 <see cref="InRangeTransition"/> 相同：可普攻且 <see cref="Weapon_Sample.FindEnemy"/> &gt; 0；
/// 额外要求<strong>自身</strong>当前占用格（或按位置估算的格）落在 <see cref="MapManage.IfInMapRange"/> 内，
/// 用于远程僵尸未进场（仍在场外格）时不转入攻击态。
/// </summary>
public class InRangeTransition_Inmap : Transition
{
    public override bool ifReach(Chess chess)
    {
        if (chess?.equipWeapon?.weapon == null)
            return false;
        if (!SelfIsInsideMap(chess))
            return false;
        return chess.equipWeapon.AttackAble && chess.equipWeapon.weapon.FindEnemy(chess) > 0;
    }

    /// <summary>
    /// 优先 <see cref="MoveController.standTile.mapPos"/>；无占格时用世界坐标按 <see cref="MapManage.tileSize"/> 换算格索引（与索敌几何一致，不做钳制到场内）。</summary>
    static bool SelfIsInsideMap(Chess chess)
    {
        MapManage map = MapManage.instance;
        if (map == null || chess == null)
            return false;

        if (chess.moveController != null && chess.moveController.standTile != null)
        {
            Vector2Int mp = chess.moveController.standTile.mapPos;
            return map.IfInMapViewRange(mp.x, mp.y);
        }

        Vector2 ts = map.tileSize;
        if (ts.x <= 0f || ts.y <= 0f)
            return false;

        Vector2 p = chess.transform.position;
        int ix = Mathf.FloorToInt(p.x / ts.x);
        int iy = Mathf.FloorToInt(p.y / ts.y);
        return map.IfInMapViewRange(ix, iy);
    }

    public override Transition Clone()
    {
        return new InRangeTransition_Inmap();
    }
}
