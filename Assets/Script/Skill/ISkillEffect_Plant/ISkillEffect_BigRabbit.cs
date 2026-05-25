using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
 
/// <summary>
/// 在身边随机一格生成与施法者同 <see cref="Chess.tag"/> 的棋子；数据来自可配置的 <see cref="PropertyCreator"/>。
/// 落点需通过 <see cref="PropertyCreator.IfCanPlant"/>（与种植规则一致）；若无合法格则本帧不生成。
/// </summary>
 
public class ISkillEffect_BigRabbit : ISkillEffect
{
    [Tooltip("要生成的单位配置")]
    public PropertyCreator spawnCreator;

    [Tooltip("true：八邻（含对角）；false：仅四邻（与 MapManage.NearTile 一致）")]
    public bool useEightNeighbors = true;

    public GameObject targetEffect;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || spawnCreator == null || GameManage.instance?.chessTeamManage == null)
            return;

        Tile stand = user.moveController != null ? user.moveController.standTile : null;
        if (stand == null || MapManage.instance == null)
            return;

        List<Tile> neighbors = useEightNeighbors
            ? MapManage.instance.GetEightNeighborTiles(stand)
            : MapManage.instance.NearTile(stand);

        if (neighbors == null || neighbors.Count == 0)
            return;

        Shuffle(neighbors);

        for (int i = 0; i < neighbors.Count; i++)
        {
            Tile t = neighbors[i];
            if (t == null)
                continue;
            if (!spawnCreator.IfCanPlant(t))
                continue;

            Chess c= GameManage.instance.chessTeamManage.CreateChess(spawnCreator, t, user.tag);
            if (targetEffect != null)
            {
                GameObject effect= ObjectPool.instance.Create(targetEffect);
                effect.transform.position = c.transform.position;
            }
            return;
        }
    }

    static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
