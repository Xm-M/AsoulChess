using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SkillEffect_CreateSunLight : ISkill
{
    public float startTime;
    public float coldDown;//CD
    public int sunLightNum = 25;
    public Transform sunLightPos;
    float t;
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        t+=Time.deltaTime;
        //Debug.Log(t);
        if (t > user.propertyController.GetColdDown(coldDown))
        {
            t=0;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void InitSkill(Chess user)
    {
        
        t = startTime; 

    }

    public void LeaveSkill(Chess user)
    {
        t = 0;  
    }

 

    public void UseSkill(Chess user)
    {
        SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
        //Debug.Log(lignt.gameObject.name);
        lignt.InitSunLight( user.moveController.standTile, sunLightNum,sunLightPos.transform.position);
    }

    public void WhenEnter(Chess user)
    {
        //throw new System.NotImplementedException();
        //t = startTime;
        t = Random.Range(12.5f, 22);
    }
}

public class SkillEffect_Anon: ISkill
{
    public float startTime;
    public float coldDown;//CD
    public int sunLightNum = 25;
    public Transform sunLightPos;
    public float x0 = 0.5f;
    public float movespeed = -1f;
    public float height = 10;
    public float totalTime = 1;
    public float timeSpeed=1;
    float t;
    public bool IfSkillReady(Chess user)
    {
        if(user.animatorController.animator.GetFloat("Mygo")>0) 
            t += Time.deltaTime;
        //Debug.Log(t);
        if (t > user.propertyController.GetColdDown(coldDown))
        {
            t = 0;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void InitSkill(Chess user)
    {
        t = startTime;

    }

    public void LeaveSkill(Chess user)
    {
        t = 0;
    }



    public void UseSkill(Chess user)
    {
        SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
        //Debug.Log(lignt.gameObject.name);
        //lignt.InitSunLight(user.moveController.standTile, sunLightNum, sunLightPos.transform.position);
        float n = Random.Range(0, 1);
        int sun = sunLightNum;
        if (n < user.propertyController.GetCrit())
        {
            //Debug.Log("会心阳光");
            sun = (int)(sunLightNum * user.propertyController.GetCritDamage());
        }

        lignt.InitSunLight(sun, sunLightPos.position, totalTime, x0, height, movespeed,timeSpeed);
    }

    public void WhenEnter(Chess user)
    {
        //throw new System.NotImplementedException();
        //t = startTime;
        t = Random.Range(12.5f, 22);
    }
}
/// <summary>
/// 千早爱音的隐藏技能 为一个大范围内的所有友军恢复25%的最大生命值；如果目标是Mygo成员（非爱音），则会在目标位置生成阳光
/// </summary>
public class SkillEffect_AnonHide : ISkill
{
    public float coldDown;//CD
    public MouseDownSkill ifMouseDown;
    public float radius;
    public float healRate=0.25f;
    public GameObject healEffect;
    List<Chess> targets;
    float t;
     
    Chess user;
    public bool IfSkillReady(Chess user)
    {
        t += Time.deltaTime;

        if (t > user.propertyController.GetColdDown(coldDown) && user.animatorController.animator.GetFloat("Mygo")>1)
        {
            user.animatorController.ChangeFlash(-1);
            //Passive_Anon
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
        this.user = user;
        targets= new List<Chess>();
    }

    public void LeaveSkill(Chess user)
    {
         
    }

    public void UseSkill(Chess user)
    {
        //LayerMask layer=ChessTeamManage.Instance.get
        Collider2D[] cols= CheckObjectPoolManage.GetColArray(100);
        
        int n = Physics2D.OverlapCircleNonAlloc(user.transform.position, radius, cols, LayerMask.GetMask(user.tag));
        //Debug.Log(LayerMask.NameToLayer(user.tag));
        //Debug.Log(n);
        for(int i = 0; i < n; i++)
        {
            Chess friend= cols[i].gameObject.GetComponent<Chess>();
            friend.propertyController.Heal(friend.propertyController.GetMaxHp()*healRate);
            GameObject heal= ObjectPool.instance.Create(healEffect);
            heal.transform.position=friend.transform.position;
            //这里还要生成一个治疗的特效
            if(friend.propertyController.creator.plantTags.Contains("Mygo")&&
                friend.propertyController.creator != user.propertyController.creator)
            {
                SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
                //Debug.Log(lignt.gameObject.name);
                lignt.InitSunLight(friend.moveController.standTile, 15, friend.moveController.standTile.transform.position+Vector3.up);
            }
        }
    }
    public void ChangeState()
    {
         
        user.stateController.ChangeState(StateName.IdleState);
        user.animatorController.ChangeFlash(0);
    }

    public void WhenEnter(Chess user)
    {
        t = 0;
        user.animatorController.ChangeFlash(1);
        targets = new List<Chess>();
         
    }
}