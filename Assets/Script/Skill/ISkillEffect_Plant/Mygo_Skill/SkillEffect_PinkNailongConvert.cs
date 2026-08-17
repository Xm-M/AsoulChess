using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 粉色奶龙主动：消灭 <paramref name="targets"/> 中最近的一只敌方，
/// 在其格子以我方 tag 生成 <see cref="nailongZombieCreator"/>（暂可用普通僵尸占位，之后替换 Creator）。
/// 索敌由 ColdSkill + <see cref="SkillReady_IfTargetInRange"/> + <see cref="IGridFindTarget"/> 配置。
/// </summary>
[Serializable]
public class SkillEffect_PinkNailongConvert : ISkillEffect
{
    [Tooltip("我方奶龙僵尸 PropertyCreator；未做完前可挂普通僵尸")]
    public PropertyCreator nailongZombieCreator;

    [Tooltip("成功转化后在目标世界坐标生成的特效；留空则不播")]
    public GameObject convertEffect;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || nailongZombieCreator == null || GameManage.instance?.chessTeamManage == null)
            return;

        Chess victim = PickNearest(user, targets);
        if (victim == null || victim.IfDeath)
            return;

        Tile tile = victim.moveController != null ? victim.moveController.standTile : null;
        Vector3 worldPos = victim.transform.position;
        if (tile == null)
            return;

        victim.Death();

        Chess spawned = GameManage.instance.chessTeamManage.CreateChess(
            nailongZombieCreator, tile, user.tag);
        if (spawned == null)
            return;

        spawned.transform.position = worldPos;
        SpawnConvertEffect(worldPos);
    }

    void SpawnConvertEffect(Vector3 worldPos)
    {
        if (convertEffect == null || ObjectPool.instance == null)
            return;

        GameObject fx = ObjectPool.instance.Create(convertEffect);
        if (fx == null)
            return;

        fx.transform.position = worldPos;
    }

    static Chess PickNearest(Chess user, List<Chess> targets)
    {
        if (targets == null || targets.Count == 0)
            return null;

        Chess best = null;
        float bestSqr = float.MaxValue;
        Vector3 origin = user.transform.position;

        for (int i = 0; i < targets.Count; i++)
        {
            Chess c = targets[i];
            if (c == null || c.IfDeath)
                continue;
            if (c.CompareTag(user.tag))
                continue;

            float sqr = (c.transform.position - origin).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = c;
            }
        }

        return best;
    }
}
