using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class ITriangleSearch : IFindTarget
{
    public float boxH;
    
    public void FindTarget(Chess user, List<Chess> targets)
    {
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(100);
        Vector2 box = new Vector2(user.propertyController.GetAttackRange(), boxH);
        int num= Physics2D.OverlapBoxNonAlloc(user.transform.position + user.transform.right * box.x / 2, box, 0,
           cols);
        Vector2 pos1 = user.transform.position;
        Vector2 pos2= (Vector2)user.transform.position+new Vector2(box.x,box.y/2);
        Vector2 pos3= (Vector2)user.transform.position + new Vector2(box.x, -box.y / 2);
        for (int i=0; i<num; i++)
        {
            if (IfInTriangle(pos1,pos2,pos3, cols[i].transform.position))
            {
                targets.Add(cols[i].GetComponent<Chess>());
            }
        }
    }
    public static bool IfInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 target)
    {
        // 辅助函数，用于计算点是否在向量的同一侧
        float CheckSide(Vector2 a, Vector2 b, Vector2 point)
        {
            return (b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x);
        }
        // 检查目标点是否在p1p2的同侧
        bool check1 = CheckSide(p1, p2, target) * CheckSide(p1, p2, p3) >= 0;
        // 检查目标点是否在p2p3的同侧
        bool check2 = CheckSide(p2, p3, target) * CheckSide(p2, p3, p1) >= 0;
        // 检查目标点是否在p3p1的同侧
        bool check3 = CheckSide(p3, p1, target) * CheckSide(p3, p1, p2) >= 0;
        // 如果目标点在所有三个边的同侧，则它位于三角形内部
        return check1 && check2 && check3;
    }

}
