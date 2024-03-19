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
        // �������������ڼ�����Ƿ���������ͬһ��
        float CheckSide(Vector2 a, Vector2 b, Vector2 point)
        {
            return (b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x);
        }
        // ���Ŀ����Ƿ���p1p2��ͬ��
        bool check1 = CheckSide(p1, p2, target) * CheckSide(p1, p2, p3) >= 0;
        // ���Ŀ����Ƿ���p2p3��ͬ��
        bool check2 = CheckSide(p2, p3, target) * CheckSide(p2, p3, p1) >= 0;
        // ���Ŀ����Ƿ���p3p1��ͬ��
        bool check3 = CheckSide(p3, p1, target) * CheckSide(p3, p1, p2) >= 0;
        // ���Ŀ��������������ߵ�ͬ�࣬����λ���������ڲ�
        return check1 && check2 && check3;
    }

}
