using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 律鼓击：读取 <see cref="RitsuSkillContextKeys.RecordedDamageSnap"/>，
/// 对身前 2×3 与身后 2×3（相对面朝，与秋山澪前方格同一套 right.x 符号）范围内敌人造成伤害。
/// </summary>
public class SkillEffect_TainakaRitsuDrumSlam : ISkillEffect
{
    [LabelText("总伤害平摊到每个目标")]
    public bool splitDamageAmongTargets = true;

    [LabelText("伤害类型")] public DamageType damageType = DamageType.Physical;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || user.IfDeath) return;

        float total = 0f;
        if (!user.skillController.context.TryGet(RitsuSkillContextKeys.RecordedDamageSnap, out total))
            total = 0f;
        user.skillController.context.Remove(RitsuSkillContextKeys.RecordedDamageSnap);

        if (total <= 0f) return;

        var map = MapManage.instance;
        var stand = user.moveController?.standTile;
        if (map == null || stand == null) return;

        int cx = stand.mapPos.x;
        int cy = stand.mapPos.y;
        int forward = user.transform.right.x >= -0.01f ? 1 : -1;

        var hit = new List<Chess>(16);
        var seen = new HashSet<Chess>();
        CollectBand(map, cx, cy, forward, hit, seen, user);
        CollectBand(map, cx, cy, -forward, hit, seen, user);

        if (hit.Count == 0) return;

        float per = splitDamageAmongTargets ? total / hit.Count : total;
        var elem = ElementType.AOE | ElementType.CloseAttack;

        for (int i = 0; i < hit.Count; i++)
        {
            var enemy = hit[i];
            if (enemy == null || enemy.IfDeath) continue;
            var dm = new DamageMessege(user, enemy, per, damageType, elem);
            enemy.propertyController.GetDamage(dm);
        }
    }

    static void CollectBand(MapManage map, int cx, int cy, int dirX, List<Chess> hit, HashSet<Chess> seen, Chess user)
    {
        for (int step = 1; step <= 2; step++)
        {
            int col = cx + dirX * step;
            for (int dy = -1; dy <= 1; dy++)
            {
                int row = cy + dy;
                if (!map.IfInMapRange(col, row)) continue;
                var tile = map.tiles[col, row];
                var c = tile?.stander;
                if (c == null || c.IfDeath || seen.Contains(c)) continue;
                if (c.CompareTag(user.tag)) continue;
                seen.Add(c);
                hit.Add(c);
            }
        }
    }
}
