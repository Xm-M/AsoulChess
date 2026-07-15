using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多首索敌用：按行世界 Y 做水平射线。
/// </summary>
public static class MultiHeadRowFind
{
    public static Chess FindEnemyOnRow(Chess user, int rowY, Vector2 preferOrigin)
    {
        if (user == null || ChessTeamManage.Instance == null)
            return null;
        if (!MultiHeadMutsumiKeys.TryGetRowWorldY(user, rowY, out float laneY))
            return null;

        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        float range = user.propertyController != null
            ? user.propertyController.GetAttackRange()
            : 10f;
        Vector2 origin = new Vector2(preferOrigin.x, laneY);
        RaycastHit2D hit = Physics2D.Raycast(origin, user.transform.right, range, enemyLayer);
        if (hit.collider == null)
            return null;
        Chess c = hit.collider.GetComponentInParent<Chess>();
        if (c == null || c.IfDeath)
            return null;
        return c;
    }
}
