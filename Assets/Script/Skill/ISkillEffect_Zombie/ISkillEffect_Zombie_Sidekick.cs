using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 追随者的技能 如果前方有被标记的目标 则只会攻击被标记的目标 无视其他植物
/// </summary>
public class StraightFindTarget_Zombie_SideKick : IFindTarget
{
    public float checkRange = 25;
    Chess target;
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);

        if (target == null || target.IfDeath) {
            RaycastHit2D[] hits = CheckObjectPoolManage.GetHitArray(100);

            int num = Physics2D.RaycastNonAlloc(user.transform.position, user.transform.right,
                hits, checkRange, GameManage.instance.chessTeamManage.GetEnemyLayer(user.gameObject));

            for (int i = 0; i < num; i++)
            {
                Chess chess = hits[i].collider.GetComponent<Chess>();
                if (chess.buffController.buffDic.ContainsKey("霸凌目标"))
                {
                    Debug.Log(chess.name);
                    target = chess;
                    break;
                }
            }
            CheckObjectPoolManage.ReleaseArray(100, hits);
            RaycastHit2D hit = Physics2D.Raycast(user.transform.position, user.transform.right,
                user.propertyController.GetAttackRange(), enemyLayer);

            if (hit.collider != null)
            {
                targets.Add(hit.collider.GetComponent<Chess>());
            }
        }
        else
        {
            if (Vector2.Distance(target.transform.position, user.transform.position) < user.propertyController.GetAttackRange())
            {
                targets.Add(target);
            }
        }

    }
}