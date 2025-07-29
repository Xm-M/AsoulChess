using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//这个是召唤猫猫的技能
public class SkillEffect_Rana : ISkill
{
    [LabelText("不同颜色的小猫")]
    public PropertyCreator cat; //具体要怎么实现小猫的换色呢...一种方式就是类似zombie那种，控制一下 不对啊 我不是可以直接在代码层修改各种播放吗
    //记得猫猫的tag里有召唤物
    public float coldDown;//CD
    public MouseDownSkill ifMouseDown;
    float t;
    Chess user;
    List<Chess> cats;
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
                Debug.Log("召唤猫猫");
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
        this.user = user;
        cats = new List<Chess>();
    }
    public void LeaveSkill(Chess user)
    {
         //这里要杀死小猫
        foreach(var cat in cats)
        {
            cat.Death();
        }
        cats.Clear();
    }
    public void UseSkill(Chess user)
    {
        //这里要召唤小猫
        Chess lcat= ChessTeamManage.Instance.CreateChess(cat, user.moveController.standTile,user.tag);
        cats.Add(lcat);
    }
    public void ChangeState()
    {
        //ObjectPool.instance.ReycleObject(black); 
        user.stateController.ChangeState(StateName.IdleState);
        user.animatorController.ChangeFlash(0);
    }
    public void WhenEnter(Chess user)
    {
        t = 0;
        user.animatorController.ChangeFlash(1);
    }
}

//这个是冻结全场的效果
public class SkillEffect_RanaFreezy : ISkill
{
    [SerializeReference]
    public FreezyBuff freezyBuff;
    [LabelText("冰冻特效全屏")]
    public GameObject freezyEffect;
    public bool IfSkillReady(Chess user)
    {
        //t += Time.deltaTime;
        //这里先大于0 实际是大于3 就是四个成员在场时才能使用的技能
        if (user.animatorController.animator.GetBool("Match") && user.animatorController.animator.GetFloat("Mygo") > 0)
        {
            return true;
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
         
        
    }

    public void UseSkill(Chess user)
    {
        foreach(var chess in ChessTeamManage.Instance.GetEnemyTeam(user.tag))
        {
            chess.buffController.AddBuff(freezyBuff);
        }
        user.animatorController.animator.SetBool("Match", false);
        ObjectPool.instance.Create(freezyEffect).transform.position = Vector3.zero;
    }

    public void WhenEnter(Chess user)
    {
        //throw new System.NotImplementedException();
    }
}
/// <summary>
/// 所有消耗品默认没有碰撞体积
/// </summary>
public class SkillEffect_MatchaParfait : ISkill
{
    [SerializeReference]
    public MatchaParfaitBuff matchaParfaitBuff;
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
        if (target != null)
            target.buffController.AddBuff(matchaParfaitBuff);
        Debug.Log("吃个抹茶芭菲");
        //death事件放在动画播放完毕自动转过去就好了
    }

    public void WhenEnter(Chess user)
    {
        //throw new System.NotImplementedException();
        user.UnSelectable();
    }
}
