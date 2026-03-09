using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 
/// </summary>

public class BloodLineArmor : ArmorBase
{
    [LabelText("红线起始点")]
    public Transform startPos;
    [LabelText("红线牵引目标")]
    public Chess targetChess;
    //public BloodBuff buff;//当然不可能是bloodBuff啦 
    public DamageMessege dm;//直接用dm就好
    [LabelText("红线伤害")]
    public float damage=10;//伤害
    [LabelText("伤害间隔")]
    public float interval = 1;//伤害间隔
    [LabelText("红线")]
    public LineRenderer bloodLine;
    public EdgeCollider2D edgeCollider;
    float t;
    public override void BrokenArmor()
    {
        //throw new System.NotImplementedException();
        targetChess = null;
        bloodLine.enabled = false;
    }
    private void Update()
    {
        if(targetChess != null&&!targetChess.IfDeath)
        {
            t+=Time.deltaTime;
            if(t > interval)
            {
                dm.damageTo = targetChess;
                dm.damage = this.damage;
                dm.damageFrom = user;
                user.propertyController.TakeDamage(dm);
                user.propertyController.ChangeHPMax(damage);
                t = 0;
            }
            bloodLine.SetPosition(0, startPos.position);
            bloodLine.SetPosition(1, targetChess.transform.position);
            int pointCount = bloodLine.positionCount;
            Vector2[] colliderPoints = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                Vector3 worldPos = bloodLine.GetPosition(i);
                colliderPoints[i] = new Vector2(worldPos.x, worldPos.y); // 转换为2D
            }
            edgeCollider.points = colliderPoints;
        }
        else if(targetChess != null&&targetChess.IfDeath)
        {
            BrokenArmor();
        }
    }
    public override void GetDamage(DamageMessege dm)
    {
        //throw new System.NotImplementedException();
        if ((dm.damageElementType & ElementType.Cutting) != 0)
        {
            BrokenArmor();
        }
    }

    public void UseLine()
    {
        List<Chess> enemys = ChessTeamManage.Instance.GetEnemyTeam(user.gameObject.tag);
        if (enemys.Count != 0)
        {
            int n = Random.Range(0, enemys.Count);
            targetChess = enemys[n];
            bloodLine.SetPosition(0, startPos.position);
            bloodLine.SetPosition(1, targetChess.transform.position);
        }
        else
        {
            //后续应该会添加一个 断线的效果
            BrokenArmor();
        }
    }

    public override void InitArmor()
    {
        user.WhenEnterGame.AddListener(ResetArmor);
        
    }

    public override void ResetArmor(Chess chess)
    {
        bloodLine.enabled = true;
        targetChess = null;
        t = 0;
        user.propertyController.onGetDamage.AddListener(GetDamage);
        UseLine();
    }
}
