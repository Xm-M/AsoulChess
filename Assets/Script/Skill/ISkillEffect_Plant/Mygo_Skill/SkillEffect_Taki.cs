using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class SkillEffect_Taki : ISkill
{
    [LabelText("威压")]
    [SerializeReference]
    public Buff_Coercion coerction;
    [LabelText("恐惧")]
    [SerializeReference]
    public Buff_Fear fear;
     

    public float coldDown;
    public MouseDownSkill ifMouseDown;
    public GameObject fearBullet;
    float t;
    public bool IfSkillReady(Chess user)
    {
        t += Time.deltaTime;
        //这里先大于0 实际是大于1 就是四个成员在场时才能使用的技能
        if (t > coldDown && user.animatorController.animator.GetFloat("Mygo") > 1)
        {
            user.animatorController.ChangeFlash(-1);
            if (ifMouseDown.IfDown)
            {
                user.animatorController.ChangeFlash(1);
                t = 0;
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }

    }

    public void InitSkill(Chess user)
    {
         
    }

    public void LeaveSkill(Chess user)
    {
        user.propertyController.onTakeDamage.RemoveListener(AddCoerction);
    }
    //这个哈气到底要怎么检测呢...是发射波呢 还是直接检测呢
    //直接检测吧
    public void UseSkill(Chess user)
    {
        RaycastHit2D[] rays = CheckObjectPoolManage.GetHitArray(100);
        Debug.Log("椎名立希使用了哈气！");
        //for (int i = -1; i <= 1; i++)
        //{
        //    int n = Physics2D.RaycastNonAlloc(user.transform.position+Vector3.up*i*2f, user.transform.right, rays,100,
        //        ChessTeamManage.Instance.GetEnemyLayer(user.gameObject));
        //    Debug.Log(string.Format( "有{0}个敌人",n));
        //    AddFear(rays, n);
        //}
        GameObject b = ObjectPool.instance.Create(fearBullet);
        Bullet zidan = b.GetComponent<Bullet>();
        zidan.InitBullet(user, user.equipWeapon.weaponPos.position, user, user.transform.right);
        zidan.Dm.takeBuff = fear.Clone();
        CheckObjectPoolManage.ReleaseArray(100, rays);
    }
    public void AddFear(RaycastHit2D[] cols,int n)
    {
        //Debug.Log("施加恐惧");
        for (int i = 0; i < n; i++)
        {
            Chess chess = cols[i].collider.GetComponent<Chess>();
            if (chess.buffController.buffDic.ContainsKey(coerction.buffName))
            {
                chess.buffController.buffDic[coerction.buffName].BuffOver();
                chess.buffController.AddBuff(fear);
            }
        }
    }
    public void WhenEnter(Chess user)
    {
        t = 0;
        user.animatorController.ChangeFlash(1);
        user.propertyController.onTakeDamage.AddListener(AddCoerction);
    }
    public void AddCoerction(DamageMessege dm)
    {
        dm.damageTo.buffController.AddBuff(coerction);
        Debug.Log("施加威压");
    }
}
