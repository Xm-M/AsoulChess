using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class SkillEffect_Nina : ISkill
{
    [LabelText("闪避时间")]
    public float DodgeTime;
    [LabelText("眩晕范围")]
    public float radiu;
    [LabelText("冷却时间")]
    public float coldTime;
    [LabelText("眩晕Buff")]
    [SerializeReference]
    public DizznessBuff dizznessBuff;
    [LabelText("闪避Buff")]
    [SerializeReference]
    public DodgeBuff dodgeBuff;
    bool hit;
    float t;
    float realColdTime;
    public bool IfSkillReady(Chess user)
    {
        t += Time.deltaTime;
        if (t > user.propertyController.GetColdDown(realColdTime)&&hit)
        {
            user.animatorController.animator.SetInteger("skill", 0);
            return true;
        }else
            return false;
    }

    public void InitSkill(Chess user)
    {
        
    }

    public void LeaveSkill(Chess user)
    {
        
    }
    /// <summary>
    /// 获得攻击免疫 
    /// </summary>
    /// <param name="user"></param>
    public void UseSkill(Chess user)
    {
        user.StartCoroutine(Dodge(user));
    }
    IEnumerator Dodge(Chess user)
    {
         
        hit = false;
        float type = user.animatorController.animator.GetFloat("type");
        user.buffController.AddBuff(dodgeBuff);
        if (type > 0)
        {
            realColdTime = coldTime / 2;
            Collider2D[] teams = CheckObjectPoolManage.GetColArray(100);
            int num = Physics2D.OverlapCircleNonAlloc(user.transform.position, radiu, teams, user.gameObject.layer);
            for (int i = 0; i < num; i++)
            {
                Chess friend = teams[i].GetComponent<Chess>();
                //所以到底要不要造成伤害
                friend.buffController.AddBuff(dodgeBuff);
            }
            CheckObjectPoolManage.ReleaseColArray(100, teams);
        }
         
        yield return new WaitForSeconds(DodgeTime);
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(100);
        int n= Physics2D.OverlapCircleNonAlloc(user.transform.position, radiu, cols,ChessTeamManage.Instance.GetEnemyLayer(user.gameObject));
        for(int i = 0; i < n; i++)
        {
            Chess enemy = cols[i].GetComponent<Chess>();
            Debug.Log(enemy.name + "眩晕");
            //所以到底要不要造成伤害 
            enemy.buffController.AddBuff(dizznessBuff);
        }
        CheckObjectPoolManage.ReleaseColArray(100, cols);
        
    }


    public void WhenEnter(Chess user)
    {
        user.propertyController.onGetDamage.AddListener(OnGetDamage);
        realColdTime = coldTime;
    }
    public void OnGetDamage(DamageMessege dm)
    {
        if ((dm.damageElementType & ElementType.CloseAttack) != 0)
        {            
            hit=true;
        }
    }
}


//仁菜嘲讽技能
public class SkillEffect_NinaTaunt : ISkill
{
    [LabelText("嘲讽范围")]
    public float tauntRange=8.75f;
    [SerializeReference]
    [LabelText("生气Buff")]
    public AngryBuff angryBuff;
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        //float type = user.animatorController.animator.GetFloat("type");
        if (user.animatorController.animator.GetInteger("skill1")==2)
        {
            //SetInteger("skill", 2);
            return true;
        }
        else return false;
    }

    public void InitSkill(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public void LeaveSkill(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public void UseSkill(Chess user) 
    {
        RaycastHit2D[] hits = CheckObjectPoolManage.GetHitArray(100);
        int n = Physics2D.RaycastNonAlloc(user.transform.position, user.transform.right, hits, tauntRange, ChessTeamManage.Instance.GetEnemyLayer(user.gameObject));
        for(int i = 0; i < n; i++)
        {
            Chess chess= hits[i].collider.GetComponent<Chess>();
            chess.buffController.AddBuff(angryBuff);
            
        }
        CheckObjectPoolManage.ReleaseArray(100, hits);
        Debug.Log("结束嘲讽");
        user.animatorController.animator.SetInteger("skill1", 0);
    }

    public void WhenEnter(Chess user)
    {
        //throw new System.NotImplementedException();
    }
}


public class SkillEffect_NinaEat : ISkill
{
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        float type = user.animatorController.animator.GetFloat("type");
        if (user.animatorController.animator.GetInteger("skill1")==1)
        {
            //user.animatorController.animator.SetInteger("skill", 1);
            return true;
        }
        else return false;
    }

    public void InitSkill(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public void LeaveSkill(Chess user)
    {
         
    }

    public void UseSkill(Chess user)
    {

        user.animatorController.animator.SetInteger("skill1", 0);
    }

    public void WhenEnter(Chess user)
    {
        //throw new System.NotImplementedException();
    }
}


/// <summary>
/// 所有消耗品默认没有碰撞体积
/// </summary>
public class SkillEffect_BeafRice : ISkill
{
    [SerializeReference]
    //public MatchaParfaitBuff matchaParfaitBuff;
    public BeafRiceBuff beafRiceBuff;
    public bool IfSkillReady(Chess user)
    {
        //反正一进来就给我用技能就对了
        return true;
    }

    public void InitSkill(Chess user)
    {
        //throw new System.NotImplementedException();

    }

    public void LeaveSkill(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public void UseSkill(Chess user)
    {
        Chess target = user.moveController.standTile.stander;
        if(target != null)
            target.buffController.AddBuff(beafRiceBuff);
        Debug.Log("吃个吉野家牛肉饭,舒服");
        //death事件放在动画播放完毕自动转过去就好了
    }

    public void WhenEnter(Chess user)
    {
        //throw new System.NotImplementedException(); 
        user.UnSelectable();
    }
}

